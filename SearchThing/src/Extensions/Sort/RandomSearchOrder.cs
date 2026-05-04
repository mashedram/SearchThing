using SearchThing.Search;

namespace SearchThing.Extensions.Sort;

public class RandomSearchOrder : ISelectableSearchOrder
{
    public string Name => "Random";

    public int Order(ISearchableCrate searchableCrate)
    {
        return Random.Shared.Next();
    }
}