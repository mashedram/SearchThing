using SearchThing.History;
using SearchThing.Search;

namespace SearchThing.Extensions.Panel.History;

public class PropHistorySearchPanel : HistorySearchPanel
{
    public override string Tag => "Prop History";
    protected override bool Filter(HistoryFullCrateDataEntry entry)
    {
        return entry.CrateType == CrateType.Prop;
    }
}