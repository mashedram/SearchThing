namespace SearchThing.Search.Sorting;

public interface ISearchOrder
{
    int Order(ISearchOrderable orderable);
}