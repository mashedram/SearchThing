using SearchThing.Extensions.Panel.Abstract;
using SearchThing.History;
using SearchThing.Search;

namespace SearchThing.Extensions.Panel.History;

public abstract class HistorySearchPanel : BasicSearchPanel
{
    public override bool ResearchOnPageChange => true;
    
    public override abstract string Tag { get; }
    protected abstract bool Filter(HistoryEntry entry);
    
    protected override void Search(string query, ISearchOrder searchOrder, Action<SearchResults> callback)
    {
        var result = HistoryManager.Search(query, searchOrder, Filter);
        callback(result);
    }
}