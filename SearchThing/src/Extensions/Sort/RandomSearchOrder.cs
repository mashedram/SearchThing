using SearchThing.Search;

namespace SearchThing.Extensions.Sort;

public class RandomSearchOrder : ISelectableSearchOrder
{
    public string Name => "Random";

    public int Score(ISearchableCrate searchableCrate)
    {
        return Random.Shared.Next();
    }
}