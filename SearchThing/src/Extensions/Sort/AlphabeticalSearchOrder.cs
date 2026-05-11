using SearchThing.Search;
using SearchThing.Search.CrateData;
using SearchThing.Search.Data;
using SearchThing.Search.Sorting;

namespace SearchThing.Extensions.Sort;

public class AlphabeticalSearchOrder : ISelectableSearchOrder
{
    public string Name => "ABC";

    public int Order(ISearchOrderable searchableCrate)
    {
        if (searchableCrate.Source is not IRequiredItemInfo data)
            return 0;

        return -string.Compare(data.Name, string.Empty, StringComparison.Ordinal);
    }
}