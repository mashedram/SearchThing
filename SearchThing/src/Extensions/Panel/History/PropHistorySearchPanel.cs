using SearchThing.History;
using SearchThing.Search;
using SearchThing.Search.CrateData;

namespace SearchThing.Extensions.Panel.History;

public class PropHistorySearchPanel : HistorySearchPanel
{
    public override string Name => "Prop History";
    protected override bool Filter(HistoryItemInfo entry)
    {
        return entry.CrateType == CrateType.Prop;
    }
}