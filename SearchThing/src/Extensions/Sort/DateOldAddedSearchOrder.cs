using SearchThing.Search;

namespace SearchThing.Extensions.Sort;

public class DateOldAddedSearchOrder : ISelectableSearchOrder
{
    public string Name => "DateOld";

    public int Score(ISearchableCrate searchableCrate)
    {
        return -(int)(searchableCrate.DateAdded.Ticks >> 32);
    }
}