using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Search;
using SearchThing.Search.CrateData;
using SearchThing.Search.Marrow;

namespace SearchThing.Extensions.Panel.Filter;

public class LevelTagSearchPanel : FilterSearchSearchPanel
{
    public override string Name => "Levels";
    protected override bool Filter(MarrowCrate searchableCrate)
    {
        return searchableCrate is { CrateType: CrateType.Level, Redacted: false };
    }
}