using SearchThing.Extensions.Abstract;
using SearchThing.History;
using SearchThing.Search;

namespace SearchThing.Extensions.History;

public abstract class HistoryPanelPage : SearchPanelPage
{
    public override bool ResearchOnPageChange => true;
    
    public override abstract string Tag { get; }
    protected abstract bool Filter(HistoryEntry entry);
    
    protected override void Search(string query, Action<SearchResults> callback)
    {
        var result = HistoryManager.Search(query, Filter);
        callback(result);
    }
}