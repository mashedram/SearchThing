using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Extensions.Sort;
using SearchThing.Search;

namespace SearchThing.Extensions.Panel.Abstract;

public abstract class BasicSearchPanel : ISearchPanel
{
    private bool _isDirty = true;
    private SearchResults? _results;
    
    public virtual bool ResearchOnPageChange => false;
    public abstract string Tag { get; }
    protected abstract void Search(string query, ISearchOrder order, Action<SearchResults> callback);

    public Guid Id { get; } = Guid.NewGuid();

    private string _query = "";
    public string Query
    {
        get => _query;  
        set
        {
            if (value == _query)
                return;
            
            _isDirty = true;
            _query = value;
        }
    }
    
    private int _selectedOrderIndex = 0;

    public int SelectedOrderIndex
    {
        get => _selectedOrderIndex;
        set
        {
            if (value == _selectedOrderIndex)
                return;
            
            if (value < 0 || value >= SupportedOrders.Length)
                return;
            
            _isDirty = true;
            _selectedOrderIndex = value;
        }
    }
    
    public ISelectableSearchOrder[] SupportedOrders { get; } = {
        new ScoreSearchOrder(),
        new AlphabeticalSearchOrder(),
        new DateNewAddedSearchOrder(),
        new DateOldAddedSearchOrder()
    };
    
    public void RequestSearch(SpawnablePanelExtension extension)
    {
        // Prevent reloading the same query
        if (!ResearchOnPageChange && !_isDirty)
        {
            // We just have to rerender now, since the query is the same but the page might be different
            extension.Rerender();
            return;
        }
        _isDirty = false;
        
        var order = SupportedOrders[SelectedOrderIndex];
        Search(_query, order, results =>
        {
            _results = results;
            Page = 0;
            PageCount = results.GetPageCount(ISearchPanel.PanelSize);
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
    
    public virtual bool OnSelected(SpawnablePanelExtension extension)
    {
        // No special logic needed
        return true;
    }

    public IEnumerable<SpawnableCrate> Render(int page)
    {
        if (_results == null)
            return Array.Empty<SpawnableCrate>();
        
        return _results.GetPage(page, ISearchPanel.PanelSize)
            .Select(entry => entry.Crate)
            .Where(crate => crate != null)
            .Select(crate => crate!);
    }
}