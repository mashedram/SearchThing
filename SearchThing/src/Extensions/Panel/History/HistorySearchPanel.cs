using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Extensions.Sort;
using SearchThing.History;
using SearchThing.Search;

namespace SearchThing.Extensions.Panel.History;

public abstract class HistorySearchPanel : BasicSearchPanel<HistoryFullCrateDataEntry>
{
    public override bool ResearchOnPageChange => true;
    
    public override abstract string Tag { get; }
    protected abstract bool Filter(HistoryFullCrateDataEntry entry);
    
    protected override void Search(string query, ISearchOrder searchOrder, Action<SearchResults<HistoryFullCrateDataEntry>> callback)
    {
        HistoryManager.SearchAsync(query, searchOrder, Filter, callback);
    }
    
    public override ISelectableSearchOrder[] SupportedOrders { get; } = {
        new DateNewAddedSearchOrder(),
        new DateOldAddedSearchOrder(),
        new ScoreSearchOrder(),
        new AlphabeticalSearchOrder(),
        new RandomSearchOrder()
    };
}