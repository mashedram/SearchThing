using SearchThing.Search;

namespace SearchThing.Extensions.Sort;

public class DateNewAddedSearchOrder : ISelectableSearchOrder
{
    public string Name => "DateNew";

    public int Order(ISearchableCrate searchableCrate)
    {
        return (int)(searchableCrate.DateAdded.Ticks >> 32);
    }
}