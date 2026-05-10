using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Util;

namespace SearchThing.Search;

public interface ISearchableCrate
{
    IEnumerable<IFuzzySearchable> SearchFields { get; }
    /// <summary>
    /// A salt value. Needed for consistent ordering for same-score items.
    /// </summary>
    int Salt { get; }
}