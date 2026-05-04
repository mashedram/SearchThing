using SearchThing.Search;

namespace SearchThing.Extensions.Sort;

public class ScoreSearchOrder : ISelectableSearchOrder
{
    public string Name => "SCORE";

    public int Order(ISearchableCrate searchableCrate)
    {
        return searchableCrate.Score;
    }
}