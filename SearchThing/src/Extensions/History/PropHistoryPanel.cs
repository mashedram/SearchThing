using SearchThing.History;
using SearchThing.Search;

namespace SearchThing.Extensions.History;

public class PropHistoryPanel : HistoryPanelPage
{

    public override string Tag => "Prop History";
    protected override bool Filter(HistoryEntry entry)
    {
        return entry.CrateType == CrateType.Prop;
    }
}