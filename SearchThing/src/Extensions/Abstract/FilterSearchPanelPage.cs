using SearchThing.Search;

namespace SearchThing.Extensions.Abstract;

public abstract class FilterSearchPanelPage : SearchPanelPage
{
    public override abstract string Tag { get; }
    protected abstract bool Filter(SearchableCrate searchableCrate);
    
    protected override void Search(string query, ISearchOrder order, Action<SearchResults> callback)
    {
        SearchManager.SearchAsync(query, Filter, order, callback);
    }
}