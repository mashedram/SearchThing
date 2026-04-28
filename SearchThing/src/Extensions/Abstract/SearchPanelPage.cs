using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Search;

namespace SearchThing.Extensions.Abstract;

public abstract class SearchPanelPage : IPanelPage
{
    private string? _lastQuery;
    private SearchResults? _results;
    
    public virtual bool ResearchOnPageChange => false;
    public abstract string Tag { get; }
    protected abstract void Search(string query, Action<SearchResults> callback);

    public void OnQueryChange(SpawnablePanelExtension extension, string query)
    {
        // Prevent reloading the same query
        if (!ResearchOnPageChange && query == _lastQuery)
        {
            // We just have to rerender now, since the query is the same but the page might be different
            extension.Rerender();
            return;
        }
        _lastQuery = query;
        
        Search(query, results =>
        {
            _results = results;
            Page = 0;
            PageCount = results.GetPageCount(IPanelPage.PageSize);
            extension.Rerender();
        });
    }
    
    public int Page { get; set; }
    public int PageCount { get; private set; } = 1;

    public void ChangePage(SpawnablePanelExtension extension, int offset)
    {
        var newPage = Page + offset;
        if (newPage < 0 || newPage >= PageCount)
            return;

        Page = newPage;
        extension.Rerender();
    }

    public IEnumerable<SpawnableCrate> Render(int page)
    {
        if (_results == null)
            return Array.Empty<SpawnableCrate>();
        
        return _results.GetPage(page, IPanelPage.PageSize)
            .Select(entry => entry.Crate)
            .Where(crate => crate != null)
            .Select(crate => crate!);
    }
}