using SearchThing.Search.CrateData;

namespace SearchThing.Search.Search;

public interface ISearchEntry
{
    IEnumerable<IFuzzySearchable> SearchFields { get; }
    /// <summary>
    /// A salt value. Needed for consistent ordering for same-score items.
    /// </summary>
    int Salt { get; }
}