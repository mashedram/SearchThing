using SearchThing.Search.CrateData;
using SearchThing.Search.Marrow;

namespace SearchThing.Extensions.Panel.Filter;

public class AvatarTagSearchPanel : FilterSearchSearchPanel
{
    public override string Name => "Avatar";
    public override string Description => "Search through all your avatars.\nUse the plus button to add an item to a preset of your choice.";
    protected override bool Filter(MarrowCrate searchableCrate)
    {
        return searchableCrate is { CrateType: CrateType.Avatar, Redacted: false };
    }
}