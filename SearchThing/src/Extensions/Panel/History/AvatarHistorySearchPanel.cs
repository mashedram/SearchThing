using SearchThing.History;
using SearchThing.Search;
using SearchThing.Search.CrateData;

namespace SearchThing.Extensions.Panel.History;

public class AvatarHistorySearchPanel : HistorySearchPanel
{
    public override string Name => "Avatar History";
    protected override bool Filter(HistoryItemInfo entry)
    {
        return entry.CrateType == CrateType.Avatar;
    }
}