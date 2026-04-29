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
    
    private static string ToToken(string value)
    {
        // Remove all the spaces from the string
        return value.Replace(" ", "");
    }
    
    public static string GetSearchString(this Crate crate)
    {
        var name = ToToken(StripDecoration(crate.name));
        var palletName = ToToken(StripDecoration(crate._pallet.name));
        var author = crate._pallet._author;
        var tags = crate._tags.ToArray();
    
        return $"{name} {palletName} {author} {string.Join(" ", tags)}"
            .ToLowerInvariant();
    }
    
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
        
        return CrateType.Prop;
    }
    
    public static SearchResults ToSearchResults(this IEnumerable<Barcode> scoredCrates)
    {
        var entries = scoredCrates.Select(barcode => new SearchResultEntry(barcode)).ToList();

        return new SearchResults(entries);
    }
}