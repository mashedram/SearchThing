using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Search;

namespace SearchThing.Extensions.Panel.Filter;

public abstract class FilterSearchSearchPanel : BasicSearchPanel<ISearchableCrate>
{
    public override abstract string Tag { get; }
    protected abstract bool Filter(ISearchableCrate searchableCrate);
    
    protected override void Search(string query, ISearchOrder order, Action<SearchResults<ISearchableCrate>> callback)
    {
        SearchManager.SearchAsync(query, GlobalCrateManager.GetCrates(), Filter, order, callback);
    }
}