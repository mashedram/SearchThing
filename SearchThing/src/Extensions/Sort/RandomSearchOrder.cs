using SearchThing.Search;
using SearchThing.Search.Sorting;

namespace SearchThing.Extensions.Sort;

public class RandomSearchOrder : ISelectableSearchOrder
{
    public string Name => "Random";

    public int Order(ISearchOrderable searchableCrate)
    {
        return Random.Shared.Next();
    }
}