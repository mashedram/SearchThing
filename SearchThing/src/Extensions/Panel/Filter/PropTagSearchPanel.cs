using SearchThing.Search.CrateData;
using SearchThing.Search.Marrow;

namespace SearchThing.Extensions.Panel.Filter;

public class PropTagSearchPanel : FilterSearchSearchPanel
{
    public override string Name => "Props";
    public override string Description => "Search through all your props.\nUse the plus button to add an item to a preset of your choice.";
    protected override bool Filter(MarrowCrate searchableCrate)
    {
        return searchableCrate is { CrateType: CrateType.Prop, Redacted: false };
    }
}