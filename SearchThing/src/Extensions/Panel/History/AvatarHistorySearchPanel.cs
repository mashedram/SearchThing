using SearchThing.History;
using SearchThing.Search.CrateData;

namespace SearchThing.Extensions.Panel.History;

public class AvatarHistorySearchPanel : HistorySearchPanel
{
    public override string Name => "Avatar History";
    public override string Description => "The avatars you have worn over time";
    protected override bool Filter(HistoryItemInfo entry)
    {
        return entry.CrateType == CrateType.Avatar;
    }
}