using SearchThing.Search;

namespace SearchThing.Util;

public class SearchTagGroup : IFuzzySearchable
{
    private IEnumerable<SearchTag> Tags { get; }
    
    public SearchTagGroup(IEnumerable<SearchTag> tags)
    {
        Tags = tags;
    }
    
    public int PartialRatio(string preprocessedQuery)
    {
        return Tags.Any(t => t.PartialRatio(preprocessedQuery) > 80) ? 90 : 0;
    }
}