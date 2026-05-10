using SearchThing.Search;

namespace SearchThing.Extensions.Sort;

public class AlphabeticalSearchOrder : ISelectableSearchOrder
{
    public string Name => "ABC";

    public int Order(ISearchOrderable searchableCrate)
    {
        if (searchableCrate.Source is not IFormalCrateData data)
            return 0;
        
        return -string.Compare(data.Name, string.Empty, StringComparison.Ordinal);
    }
}