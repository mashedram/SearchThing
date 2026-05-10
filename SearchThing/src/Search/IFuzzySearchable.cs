namespace SearchThing.Search;

public interface IFuzzySearchable
{
    int PartialRatio(string preprocessedQuery);
}