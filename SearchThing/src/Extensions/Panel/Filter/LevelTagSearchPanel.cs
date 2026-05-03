using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Search;

namespace SearchThing.Extensions.Panel.Filter;

public class LevelTagSearchPanel : FilterSearchSearchPanel
{
    public override string Tag => "Levels";
    protected override bool Filter(SearchableCrate searchableCrate)
    {
        return searchableCrate.CrateType == CrateType.Level;
    }
}