using FuzzySharp;
using FuzzySharp.PreProcess;
using Il2CppSLZ.Marrow.Warehouse;
using MelonLoader;
using SearchThing.Util;

namespace SearchThing.Search;

public record ScoredCrate(ISearchableCrate Crate, int Score) : ISearchableCrate
{
    public SearchTag Name => Crate.Name;
    public SearchTag PalletName => Crate.PalletName;
    public SearchTag Author => Crate.Author;
    public SearchTag[] Tags => Crate.Tags;
    public string Description => Crate.Description;
    public CrateType CrateType => Crate.CrateType;
    public int Salt => Crate.Salt;
    public Barcode Barcode => Crate.Barcode;
    public DateTime DateAdded => Crate.DateAdded;

    public static ScoredCrate ScoreCrate(ISearchableCrate crate, string preprocessedQuery)
    {
        return new ScoredCrate(crate, SearchManager.ScoreCrate(preprocessedQuery, crate));
    }
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
                            .OrderByDescending(searchOrder.Score)
                            .ThenByDescending(c => c.Salt) // Tie-breaker
                            .ToSearchResults();
                    
                    
                    onComplete.RunOnMainThread(emptyResult);
                    return;
                }

                var result = SearchableCrates
                    .AsParallel()
                    // Avoid starving the game thread of cores
                    .WithDegreeOfParallelism(Math.Max(1, Environment.ProcessorCount / 2))
                    .Where(filter)
                    .Select(crate =>
                        new ScoredCrate(crate, ScoreCrate(preprocessedQuery, crate))
                    )
                    .Where(c => c.Score > RequiredMatchRate)
                    .OrderByDescending(searchOrder.Score)
                    .ThenByDescending(c => c.Crate.Salt) // Tie-breaker
                    .Select(c => c.Crate)
                    .ToSearchResults();

                onComplete.RunOnMainThread(result);
            }
            finally
            {
                CrateLock.ExitReadLock();
            }
        });
    }
    
    public static int ScoreCrate(string preprocessedQuery, ISearchableCrate crate)
    {
        var nameScore = crate.Name.PartialRatio(preprocessedQuery); // Weight name higher
        var palletScore = crate.PalletName.PartialRatio(preprocessedQuery);
        var authorScore = crate.Author.PartialRatio(preprocessedQuery);
        var tagScore = crate.Tags.Any(t => t.PartialRatio(preprocessedQuery) > 80) ? 90 : 0;
    
        return new [] { nameScore, palletScore, authorScore, tagScore }.Max();
    }
}