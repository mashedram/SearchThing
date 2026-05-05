using SearchThing.Search;

namespace SearchThing.Extensions.Sort;

public class ScoreSearchOrder : ISelectableSearchOrder
{
    public string Name => "Relevance";

    public int Order(ISearchableCrate searchableCrate)
    {
        return searchableCrate.Score;
    }
}