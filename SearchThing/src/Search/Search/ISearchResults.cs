namespace SearchThing.Search.Search;

public interface ISearchResults<out TCrate>
    where TCrate : class, ISearchEntry
{
    IEnumerable<TCrate> GetPage(int page, int pageSize);
    public int GetPageCount(int pageSize);
}