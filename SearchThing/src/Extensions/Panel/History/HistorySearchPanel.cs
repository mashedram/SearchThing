using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Extensions.Sort;
using SearchThing.History;
using SearchThing.Search;
using SearchThing.Search.Search;
using SearchThing.Search.Sorting;

namespace SearchThing.Extensions.Panel.History;

public abstract class HistorySearchPanel : BasicSearchPanel<HistoryItemInfo>
{
    public override bool ResearchOnPageChange => true;

    public override abstract string Name { get; }
    protected abstract bool Filter(HistoryItemInfo entry);

    protected override void Search(string query, ISearchOrder searchOrder, Action<SearchResults<HistoryItemInfo>> callback)
    {
        HistoryManager.SearchAsync(query, searchOrder, Filter, callback);
    }

    public override ISelectableSearchOrder[] SupportedOrders { get; } =
    {
        new DateNewAddedSearchOrder(),
        new DateOldAddedSearchOrder(),
        new ScoreSearchOrder(),
        new AlphabeticalSearchOrder(),
        new RandomSearchOrder()
    };
}