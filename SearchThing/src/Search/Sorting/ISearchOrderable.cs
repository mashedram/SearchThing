using SearchThing.Search.CrateData;
using SearchThing.Search.Search;

namespace SearchThing.Search.Sorting;

public interface ISearchOrderable
{
    ISearchEntry Source { get; }
    /// <summary>
    /// The score of this item. Higher is better.
    /// </summary>
    int Score { get; }
    /// <summary>
    /// A salt value. Needed for consistent ordering for same-score items.
    /// </summary>
    int Salt { get; }
}