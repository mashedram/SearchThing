using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Search;
using SearchThing.Search.CrateData;
using SearchThing.Search.Marrow;

namespace SearchThing.Extensions.Panel.Filter;

public class AvatarTagSearchPanel : FilterSearchSearchPanel
{
    public override string Name => "Avatar";
    protected override bool Filter(MarrowCrate searchableCrate)
    {
        return searchableCrate is { CrateType: CrateType.Avatar, Redacted: false };
    }
}