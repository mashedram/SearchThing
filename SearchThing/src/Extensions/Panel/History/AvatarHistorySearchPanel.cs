using SearchThing.History;
using SearchThing.Search;

namespace SearchThing.Extensions.Panel.History;

public class AvatarHistorySearchPanel : HistorySearchPanel
{

    public override string Tag => "Avatar History";
    protected override bool Filter(ISearchableCrate entry)
    {
        return entry.CrateType == CrateType.Avatar;
    }
}