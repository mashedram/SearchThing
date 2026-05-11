using SearchThing.Search;
using SearchThing.Search.Sorting;

namespace SearchThing.Extensions.Sort;

public interface ISelectableSearchOrder : ISearchOrder
{
    string Name { get; }
}