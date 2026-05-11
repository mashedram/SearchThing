namespace SearchThing.Search.Search;

public interface IFuzzySearchable
{
    int PartialRatio(string preprocessedQuery);
}