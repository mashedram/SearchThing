using SearchThing.Search;

namespace SearchThing.Extensions.Sort;

public class AlphabeticalSearchOrder : ISelectableSearchOrder
{
    public string Name => "ABC";

    public int Score(ISearchableCrate searchableCrate)
    {
        return -string.Compare(searchableCrate.PreprocessedString, string.Empty, StringComparison.Ordinal);
    }
}