using SearchThing.Search;
using SearchThing.Search.CrateData;
using SearchThing.Search.Data;
using SearchThing.Search.Sorting;

namespace SearchThing.Extensions.Sort;

public class DateNewAddedSearchOrder : ISelectableSearchOrder
{
    public string Name => "DateNew";

    public int Order(ISearchOrderable searchableCrate)
    {
        if (searchableCrate.Source is not IRequiredItemInfo data)
            return 0;

        return (int)(data.DateAdded.Ticks >> 32);
    }
}