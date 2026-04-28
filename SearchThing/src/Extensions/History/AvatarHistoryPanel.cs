using SearchThing.History;
using SearchThing.Search;

namespace SearchThing.Extensions.History;

public class AvatarHistoryPanel : HistoryPanelPage
{

    public override string Tag => "Avatar History";
    protected override bool Filter(HistoryEntry entry)
    {
        return entry.CrateType == CrateType.Avatar;
    }
}