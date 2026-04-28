using System.Diagnostics;
using FuzzySharp;
using FuzzySharp.PreProcess;
using Il2CppInterop.Runtime;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Extensions;
using MelonLoader;
using SearchThing.Util;
using UnityEngine;
using Avatar = Il2CppSLZ.VRMK.Avatar;
using Random = System.Random;
using Type = Il2CppSystem.Type;

namespace SearchThing.Search;

public struct SearchableCrate
{
    public readonly string SearchableString;
    public readonly string PreprocessedString;
    public readonly int RandomId; // Used for tie-breaking to ensure consistent ordering
    public readonly CrateType Type;
    public readonly Barcode Barcode;
    
    public SearchableCrate(SpawnableCrate spawnableCrate)
    {
        SearchableString = spawnableCrate.GetSearchString();
        PreprocessedString = StringPreprocessorFactory.GetPreprocessor(PreprocessMode.Full)(SearchableString);

        RandomId = Random.Shared.Next();

        Type = spawnableCrate.GetCrateType();
        
        Barcode = spawnableCrate.Barcode;
    }
}

public record struct ScoredCrate(SearchableCrate Crate, int Score);

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
    
    public static void SearchAsync(string query, Func<SearchableCrate, bool> filter, Action<SearchResults> onComplete)
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
                            .OrderByDescending(c => c.RandomId) // Tie-breaker
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
                    .Where(crate => QuickScoreCrate(preprocessedQuery, crate.PreprocessedString) >
                                    RequiredMatchRate - 20) // Pre-filtering to improve performance
                    .Select(crate =>
                        new ScoredCrate(crate, ScoreCrate(preprocessedQuery, crate.PreprocessedString))
                    )
                    .Where(c => c.Score > RequiredMatchRate)
                    .OrderByDescending(c => c.Score)
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
    
    private static int QuickScoreCrate(string preprocessedQuery, string preprocessedCrate)
    {
        // Just one cheap algorithm for pre-filtering
        return Fuzz.PartialRatio(preprocessedQuery, preprocessedCrate, PreprocessMode.None);
    }
    
    public static int ScoreCrate(string preprocessedQuery, string preprocessedCrate)
    {
        // Use only PartialRatio - it's the fastest and handles most cases well
        var score = Fuzz.PartialRatio(preprocessedQuery, preprocessedCrate, PreprocessMode.None);
    
        // Prefix boost for exact starts
        if (preprocessedCrate.StartsWith(preprocessedQuery, StringComparison.OrdinalIgnoreCase))
            score = Math.Min(100, score + 15);

        return score;
    }
}