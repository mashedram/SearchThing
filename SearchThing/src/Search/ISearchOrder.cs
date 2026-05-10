namespace SearchThing.Search;

public interface ISearchOrder
{
    int Order(ISearchOrderable orderable);
}