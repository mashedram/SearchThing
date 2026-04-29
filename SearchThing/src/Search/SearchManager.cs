using FuzzySharp;
using FuzzySharp.PreProcess;
using Il2CppSLZ.Marrow.Warehouse;
using MelonLoader;
using SearchThing.Util;

namespace SearchThing.Search;

public struct SearchableCrate : ISearchableCrate
{
    public readonly string SearchableString;
    public string PreprocessedString { get; }
    public readonly int RandomId; // Used for tie-breaking to ensure consistent ordering
    public CrateType CrateType { get; }
    // Default to zero for global searchables
    public int Score => 0;
    public DateTime DateAdded { get; }
    public Barcode Barcode { get; }
    
    public SearchableCrate(SpawnableCrate spawnableCrate)
    {
        SearchableString = spawnableCrate.GetSearchString();
        PreprocessedString = StringPreprocessorFactory.GetPreprocessor(PreprocessMode.Full)(SearchableString);

        RandomId = spawnableCrate.name.GetSalt();
        
        if (!spawnableCrate._pallet.IsInMarrowGame() && AssetWarehouse.Instance.TryGetPalletManifest(spawnableCrate._pallet._barcode, out var palletManifest))
        {
            DateAdded = long.TryParse(palletManifest.UpdatedDate, out var unixTimestampMs) 
                ? DateTime.UnixEpoch.AddMilliseconds(unixTimestampMs) 
                : DateTime.MinValue;
        }
        else
        {
            DateAdded = DateTime.MinValue; // Fallback to now if we can't find the pallet, shouldn't really happen
        }

        CrateType = spawnableCrate.GetCrateType();
        
        Barcode = spawnableCrate.Barcode;
    }
}

public record struct ScoredCrate(SearchableCrate Crate, int Score) : ISearchableCrate
{
    public string PreprocessedString => Crate.PreprocessedString;
    public CrateType CrateType => Crate.CrateType;
    public Barcode Barcode => Crate.Barcode;
    public DateTime DateAdded => Crate.DateAdded;
}

public static class SearchManager
{
    private const int RequiredMatchRate = 75;
    private static readonly ReaderWriterLockSlim CrateLock = new();
    private static readonly List<SearchableCrate> SearchableCrates = new();
    
    public static void AddPallet(Pallet pallet)
    {
        // Due to the fact that searching exists on another thread, we need to wait for it to exit to avoid conflicts
        CrateLock.EnterWriteLock();
        try
        {
            foreach (var palletCrate in pallet._crates)
            {
                var spawnableCrate = palletCrate.TryCast<SpawnableCrate>();
                if (spawnableCrate == null)
                    continue;

                if (spawnableCrate._redacted)
                    continue;

                SearchableCrates.Add(new SearchableCrate(spawnableCrate));
            }
        }
        finally
        {
            CrateLock.ExitWriteLock();
        }
    }
    
    public static void SearchAsync(string query, Func<SearchableCrate, bool> filter, ISearchOrder searchOrder, Action<SearchResults> onComplete)
    {
        var lowerQuery = query.ToLowerInvariant();
        var preprocessedQuery = StringPreprocessorFactory.GetPreprocessor(PreprocessMode.Full)(lowerQuery);
        
        ThreadPool.QueueUserWorkItem(_ =>
        {
            CrateLock.EnterReadLock();
            try
            {
                if (string.IsNullOrWhiteSpace(query) || !AssetWarehouse.ready)
                {
                    var emptyResult =
                        SearchableCrates
                            .AsParallel()
                            .WithDegreeOfParallelism(Math.Max(1, Environment.ProcessorCount / 2)) 
                            .Where(filter)
                            .OrderByDescending(c => searchOrder.Score(c))
                            .ThenByDescending(c => c.RandomId) // Tie-breaker
                            .Select(c => c.Barcode)
                            .ToSearchResults();
                    
                    
                    MelonCoroutines.Start(InvokeOnMainThread(emptyResult, onComplete));
                    return;
                }

                var result = SearchableCrates
                    .AsParallel()
                    // Avoid starving the game thread of cores
                    .WithDegreeOfParallelism(Math.Max(1, Environment.ProcessorCount / 2))
                    .Where(filter)
                    .Select(crate =>
                        new ScoredCrate(crate, ScoreCrate(preprocessedQuery, crate.PreprocessedString))
                    )
                    .Where(c => c.Score > RequiredMatchRate)
                    .OrderByDescending(c => searchOrder.Score(c))
                    .ThenByDescending(c => c.Crate.RandomId) // Tie-breaker
                    .Select(c => c.Crate.Barcode)
                    .ToSearchResults();

                MelonCoroutines.Start(InvokeOnMainThread(result, onComplete));
            }
            finally
            {
                CrateLock.ExitReadLock();
            }
        });
    }
    
    private static System.Collections.IEnumerator InvokeOnMainThread(SearchResults result, Action<SearchResults> onComplete)
    {
        yield return null; // Wait one frame to ensure we're on main thread
        onComplete(result);
    }
    
    public static int ScoreCrate(string preprocessedQuery, string preprocessedCrate)
    {
        var score = Fuzz.PartialRatio(preprocessedQuery, preprocessedCrate, PreprocessMode.None);
    
        // Prefix boost for exact starts
        if (preprocessedCrate.StartsWith(preprocessedQuery, StringComparison.OrdinalIgnoreCase))
            score = Math.Min(100, score + 15);

        return score;
    }
}