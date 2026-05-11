using System.Text.RegularExpressions;

namespace SearchThing.Util;

public static class StringHelper
{
    private static readonly Regex UnityRichTextRegex = new(@"</?[a-zA-Z]*=[^>]*>|</?[a-zA-Z]+>", RegexOptions.Compiled);
    private static readonly Regex NonAlphanumericRegex = new(@"[^ a-zA-Z0-9]", RegexOptions.Compiled);

    public static string RemoveUnityRichText(string input)
    {
        return UnityRichTextRegex.Replace(input, string.Empty);
    }

    public static string RemoveNonAlphanumeric(string input)
    {
        return NonAlphanumericRegex.Replace(input, string.Empty);
    }
}