namespace SearchThing.Search;

public interface ISearchOrder
{
    int Score(ISearchableCrate searchableCrate);
}