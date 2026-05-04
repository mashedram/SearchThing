using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Search;

namespace SearchThing.Extensions.Panel.Filter;

public class AvatarTagSearchPanel : FilterSearchSearchPanel
{
    public override string Tag => "Avatar";
    protected override bool Filter(SearchableCrate searchableCrate)
    {
        return searchableCrate is { CrateType: CrateType.Avatar, Redacted: false };
    }
}