using SearchThing.Extensions.Abstract;
using SearchThing.Search;

namespace SearchThing.Extensions.Filter;

public class AvatarTagPanelPage : FilterSearchPanelPage
{
    public override string Tag => "Avatar";
    protected override bool Filter(SearchableCrate searchableCrate)
    {
        return searchableCrate.CrateType == CrateType.Avatar;
    }
}