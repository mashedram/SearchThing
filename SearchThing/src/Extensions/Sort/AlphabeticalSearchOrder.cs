using SearchThing.Search;

namespace SearchThing.Extensions.Sort;

public class AlphabeticalSearchOrder : ISelectableSearchOrder
{
    public string Name => "ABC";

    public int Order(ISearchableCrate searchableCrate)
    {
        return -string.Compare(searchableCrate.Name.Original, string.Empty, StringComparison.Ordinal);
    }
}