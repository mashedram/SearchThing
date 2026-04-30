using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Search;

namespace SearchThing.Extensions.Panel.Filter;

public class PropTagSearchPanel : FilterSearchSearchPanel
{
    public override string Tag => "Props";
    protected override bool Filter(SearchableCrate searchableCrate)
    {
        return searchableCrate.CrateType == CrateType.Prop;
    }
}