using SearchThing.History;
using SearchThing.Search.CrateData;

namespace SearchThing.Extensions.Panel.History;

public class PropHistorySearchPanel : HistorySearchPanel
{
    public override string Name => "Prop History";
    public override string Description => "The props you've spawned over time.";
    protected override bool Filter(HistoryItemInfo entry)
    {
        return entry.CrateType == CrateType.Prop;
    }
}