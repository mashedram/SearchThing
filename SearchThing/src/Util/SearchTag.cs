using System.Text.RegularExpressions;
using FuzzySharp;
using FuzzySharp.PreProcess;

namespace SearchThing.Util;

public struct SearchTag
{
    public string Original { get; }
    public string Preprocessed { get; }
    
    public SearchTag(string original)
    {
        Original = original;
        Preprocessed = Preprocess(original);
    }
    
    public int PartialRatio(string other)
    {
        return Fuzz.PartialRatio(Preprocessed, other, PreprocessMode.None);
    }
    
    public bool Contains(string preprocessedQuery)
    {
        return Preprocessed.Contains(preprocessedQuery);
    }
    
    // Preperation
    
    private static readonly Regex UnityRichTextRegex = new(@"</?[a-zA-Z]*=[^>]*>|</?[a-zA-Z]+>", RegexOptions.Compiled);
    private static readonly Regex NonAlphanumericRegex = new(@"[^ a-zA-Z0-9]", RegexOptions.Compiled);
    
    public static string Preprocess(string value)
    {
        // To lower case
        var lower = value.ToLowerInvariant();
        // Remove non-alphanumeric characters
        var nonAlphanumericStripped = NonAlphanumericRegex.Replace(lower, "");
        // Remove Unity rich text tags
        var nonUnityRichTextStripped = UnityRichTextRegex.Replace(nonAlphanumericStripped, "");
        // Remove spaces
        var spaceRemoved = nonUnityRichTextStripped.Replace(" ", "");
        // Trim leading and trailing whitespace
        var trimmed = spaceRemoved.Trim();
        return trimmed;
    }
}