namespace SearchThing.Search.CrateData;

public interface IFuzzySearchable
{
    int PartialRatio(string preprocessedQuery);
}