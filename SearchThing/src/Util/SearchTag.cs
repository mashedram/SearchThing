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
        Preprocessed = Preprocess(GetSearchString(original));
    }
    
    public int PartialRatio(string other)
    {
        return Fuzz.PartialRatio(Preprocessed, other, PreprocessMode.None);
    }
    
    // Preperation
    
    private static readonly Regex UnityRichTextRegex = new(@"</?[a-zA-Z]*=[^>]*>|</?[a-zA-Z]+>", RegexOptions.Compiled);
    private static readonly Func<string, string> Preprocessor = StringPreprocessorFactory.GetPreprocessor(PreprocessMode.Full);
    
    private static string StripDecoration(string value)
    {
        // Regex to remove unity rich text
        return UnityRichTextRegex.Replace(value, "");
    }
    
    public static string GetSearchString(string value)
    {
        var lower = value.ToLowerInvariant();
        return StripDecoration(lower);
    }
    
    private static string Preprocess(string value)
    {
        return Preprocessor(value);
    }
}