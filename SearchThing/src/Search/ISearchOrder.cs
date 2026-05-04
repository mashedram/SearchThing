namespace SearchThing.Search;

public interface ISearchOrder
{
    int Order(ISearchableCrate searchableCrate);
}