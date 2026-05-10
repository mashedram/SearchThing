using SearchThing.Search;

namespace SearchThing.Extensions.Sort;

public class DateNewAddedSearchOrder : ISelectableSearchOrder
{
    public string Name => "DateNew";

    public int Order(ISearchOrderable searchableCrate)
    {
        if (searchableCrate.Source is not IFormalCrateData data)
            return 0;
        
        return (int)(data.DateAdded.Ticks >> 32);
    }
}