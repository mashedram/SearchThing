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

public class SearchableCrate
{
    public SpawnableCrate SpawnableCrate;
    public string Name { get; }
    public string PalletName { get; }
    public string Author { get; }
    public string[] Tags { get; }
    
    public string SearchableString { get; }

    private static string StripDecoration(string value)
    {
        // Regex to remove unity rich text
        return System.Text.RegularExpressions.Regex.Replace(value, @"</?[a-zA-Z]*=[^>]*>|</?[a-zA-Z]+>", "");
    }
    
    public SearchableCrate(SpawnableCrate spawnableCrate)
    {
        Name = StripDecoration(spawnableCrate.name);
        PalletName = StripDecoration(spawnableCrate._pallet.name);
        Author = spawnableCrate._pallet._author;
        Tags = spawnableCrate._tags.ToArray();
        SearchableString =  $"{Name} {PalletName} {Author} {string.Join(" ", Tags)}".ToLowerInvariant();
         
        SpawnableCrate = spawnableCrate;
    }
}

public record struct ScoredCrate(SearchableCrate Crate, int Score);

public static class SearchManager
{
    private const int RequiredMatchRate = 82;
    private static readonly List<SearchableCrate> SearchableCrates = new();
#if DEBUG
    private static readonly Stopwatch Stopwatch = new Stopwatch();
#endif
    
    public static void AddPallet(Pallet pallet)
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
    
    public static void SearchAsync(string query, Action<IEnumerable<SpawnableCrate>> onComplete)
    {
        ThreadPool.QueueUserWorkItem(_ =>
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                onComplete(Enumerable.Empty<SpawnableCrate>());
                return;
            }
        
            var lowerQuery = query.ToLowerInvariant();
        
            var result = SearchableCrates
                .AsParallel()
                .Select(crate => 
                    new ScoredCrate(crate, Fuzz.PartialTokenSortRatio(lowerQuery, crate.SearchableString, PreprocessMode.Full))
                )
                .Where(c => c.Score > RequiredMatchRate)
                .OrderByDescending(c => c.Score)
                .Select(c => c.Crate.SpawnableCrate)
                .ToList();

            MelonCoroutines.Start(InvokeOnMainThread(result, onComplete));
        });
    }
    
    private static System.Collections.IEnumerator InvokeOnMainThread(IEnumerable<SpawnableCrate> result, Action<IEnumerable<SpawnableCrate>> onComplete)
    {
        yield return null; // Wait one frame to ensure we're on main thread
        onComplete(result);
    }
}