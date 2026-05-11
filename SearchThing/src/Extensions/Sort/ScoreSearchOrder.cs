using SearchThing.Search;
using SearchThing.Search.Sorting;

namespace SearchThing.Extensions.Sort;

public class ScoreSearchOrder : ISelectableSearchOrder
{
    public string Name => "Relevance";

    public int Order(ISearchOrderable searchableCrate)
    {
        return searchableCrate.Score;
    }
}