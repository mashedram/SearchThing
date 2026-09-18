using SearchThing.Search.CrateData;
using SearchThing.Search.Marrow;

namespace SearchThing.Extensions.Panel.Filter;

public class LevelTagSearchPanel : FilterSearchSearchPanel
{
    public override string Name => "Levels";
    public override string Description => "Search through all your levels.\nUse the plus button to add an item to a preset of your choice.";
    protected override bool Filter(MarrowCrate searchableCrate)
    {
        return searchableCrate is { CrateType: CrateType.Level, Redacted: false };
    }
}