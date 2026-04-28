using System.Text.RegularExpressions;
using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Search;

namespace SearchThing.Util;

public static class SearchHelper
{
    private static readonly Regex UnityRichTextRegex = new(@"</?[a-zA-Z]*=[^>]*>|</?[a-zA-Z]+>", RegexOptions.Compiled);
    private static readonly Regex CamelCaseRegex = new("([a-z])([A-Z])", RegexOptions.Compiled);

    
    private static string StripDecoration(string value)
    {
        // Regex to remove unity rich text
        return UnityRichTextRegex.Replace(value, "");
    }
    
    public static string GetSearchString(this Crate crate)
    {
        var name = StripDecoration(crate.name);
        var palletName = StripDecoration(crate._pallet.name);
        var author = crate._pallet._author;
        var tags = crate._tags.ToArray();
    
        // Convert camelCase/PascalCase to spaces: "StickyBomb" -> "sticky bomb"
        var spacedName = CamelCaseRegex.Replace(name, "$1 $2");
        var spacedPallet = CamelCaseRegex.Replace(palletName, "$1 $2");
    
        return $"{name} {spacedName} {palletName} {spacedPallet} {author} {string.Join(" ", tags)}"
            .ToLowerInvariant();
    }

    public static bool IsCrate<T>(this Crate crate) where T : Crate
    {
        return crate.TryCast<T>() != null;
    }
    
    public static CrateType GetCrateType(this Crate crate)
    {
        if (crate.IsCrate<AvatarCrate>())
            return CrateType.Avatar;
        
        return CrateType.Prop;
    }
    
    public static SearchResults ToSearchResults(this IEnumerable<Barcode> scoredCrates)
    {
        var entries = scoredCrates.Select(barcode => new SearchResultEntry(barcode)).ToList();

        return new SearchResults(entries);
    }
}