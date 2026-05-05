using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Search;

namespace SearchThing.Util;

public static class SearchHelper
{
    public static int GetSalt(this string str)
    {
        unchecked
        {
            return str.Aggregate(23, (current, c) => current * 31 + c);
        }
    }

    public static bool IsCrate<T>(this Crate crate) where T : Crate
    {
        return crate.TryCast<T>() != null;
    }
    
    public static CrateType GetCrateType(this Crate crate)
    {
        if (crate.IsCrate<AvatarCrate>())
            return CrateType.Avatar;
        
        if (crate.IsCrate<LevelCrate>())
            return CrateType.Level;
        
        return CrateType.Prop;
    }
    
    public static SearchResults ToSearchResults(this IEnumerable<ISearchableCrate> scoredCrates)
    {
        var entries = scoredCrates.Select(searchableCrate => new SearchResultEntry(searchableCrate)).ToList();

        return new SearchResults(entries);
    }
    
    public static bool TryGetCrate(this ISearchableCrate crate, [MaybeNullWhen(false)] out Crate outCrate)
    {
        return AssetWarehouse.Instance.TryGetCrate(crate.Barcode, out outCrate);
    }
}