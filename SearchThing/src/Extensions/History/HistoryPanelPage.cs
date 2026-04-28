using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Extensions.Abstract;
using SearchThing.History;
using SearchThing.Search;

namespace SearchThing.Extensions.History;

public class HistoryPanelPage : SearchPanelPage
{
    public override string Tag => "History";
    protected override void Search(string query, Action<SearchResults> callback)
    {
        var result = HistoryManager.Search(query);
        callback(result);
    }
}