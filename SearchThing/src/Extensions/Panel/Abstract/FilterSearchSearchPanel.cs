using SearchThing.Search;

namespace SearchThing.Extensions.Panel.Abstract;

public abstract class FilterSearchSearchPanel : BasicSearchPanel
{
    public override abstract string Tag { get; }
    protected abstract bool Filter(ISearchableCrate searchableCrate);
    
    protected override void Search(string query, ISearchOrder order, Action<SearchResults> callback)
    {
        SearchManager.SearchAsync(query, GlobalCrateManager.GetCrates(), Filter, order, callback);
    }
}