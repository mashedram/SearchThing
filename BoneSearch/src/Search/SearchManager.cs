using System.Diagnostics;
using FuzzySharp;
using FuzzySharp.PreProcess;
using Il2CppInterop.Runtime;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Extensions;
using MelonLoader;
using UnityEngine;
using Avatar = Il2CppSLZ.VRMK.Avatar;
using Type = Il2CppSystem.Type;

namespace BoneSearch.Search;

public enum CrateType
{
    Prop,
    Avatar
}

public struct SearchableCrate
{
    public readonly string SearchableString;
    public readonly CrateType Type;
    public readonly Barcode Barcode;

    private static string StripDecoration(string value)
    {
        // Regex to remove unity rich text
        return System.Text.RegularExpressions.Regex.Replace(value, @"</?[a-zA-Z]*=[^>]*>|</?[a-zA-Z]+>", "");
    }
    
    public SearchableCrate(SpawnableCrate spawnableCrate)
    {
        var name = StripDecoration(spawnableCrate.name);
        var palletName = StripDecoration(spawnableCrate._pallet.name);
        var author = spawnableCrate._pallet._author;
        var tags = spawnableCrate._tags.ToArray();
        SearchableString =  $"{name} {palletName} {author} {string.Join(" ", tags)}".ToLowerInvariant();
        
        Type = spawnableCrate.TryCast<AvatarCrate>() != null ? CrateType.Avatar : CrateType.Prop;
        
        Barcode = spawnableCrate.Barcode;
    }
}

public record struct ScoredCrate(SearchableCrate Crate, int Score);

public static class SearchManager
{
    private const int RequiredMatchRate = 82;
    private static readonly ReaderWriterLockSlim CrateLock = new();
    private static readonly List<SearchableCrate> SearchableCrates = new();
    
    public static void AddPallet(Pallet pallet)
    {
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
    
    public static void SearchAsync(string query, CrateType crateType, Action<SearchResults> onComplete)
    {
        ThreadPool.QueueUserWorkItem(_ =>
        {
            if (string.IsNullOrWhiteSpace(query) || !AssetWarehouse.ready)
            {
                onComplete(SearchResults.Empty);
                return;
            }
            
            CrateLock.EnterReadLock();
            try
            {
                var lowerQuery = query.ToLowerInvariant();

                var result = SearchableCrates
                    .AsParallel()
                    .Where(c => c.Type == crateType)
                    .Select(crate => 
                        new ScoredCrate(crate, Fuzz.PartialTokenSortRatio(lowerQuery, crate.SearchableString, PreprocessMode.Full))
                    )
                    .Where(c => c.Score > RequiredMatchRate)
                    .OrderByDescending(c => c.Score)
                    .Select(c => new SearchResultEntry(c.Crate.Barcode, c.Score))
                    .ToList();

                MelonCoroutines.Start(InvokeOnMainThread(new SearchResults(result), onComplete));
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
}