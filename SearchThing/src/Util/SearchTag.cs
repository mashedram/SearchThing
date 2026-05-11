using SearchThing.dependencies.FuzzySharp;
using SearchThing.dependencies.FuzzySharp.PreProcess;
using SearchThing.Search.Search;

namespace SearchThing.Util;

public struct SearchTag : IFuzzySearchable
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

    public static string Preprocess(string value)
    {
        // To lower case
        var lower = value.ToLowerInvariant();
        // Remove non-alphanumeric characters
        var nonAlphanumericStripped = StringHelper.RemoveNonAlphanumeric(lower);
        // Remove Unity rich text tags
        var nonUnityRichTextStripped = StringHelper.RemoveUnityRichText(nonAlphanumericStripped);
        // Remove spaces
        var spaceRemoved = nonUnityRichTextStripped.Replace(" ", "");
        // Trim leading and trailing whitespace
        var trimmed = spaceRemoved.Trim();
        return trimmed;
    }
}