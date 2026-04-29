using SearchThing.Search;

namespace SearchThing.Extensions.Sort;

public interface ISelectableSearchOrder : ISearchOrder
{ 
    string Name { get; }
}