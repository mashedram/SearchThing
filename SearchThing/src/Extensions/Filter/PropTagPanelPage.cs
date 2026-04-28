using SearchThing.Extensions.Abstract;
using SearchThing.Search;

namespace SearchThing.Extensions.Filter;

public class PropTagPanelPage : FilterSearchPanelPage
{
    public override string Tag => "Props";
    protected override bool Filter(SearchableCrate searchableCrate)
    {
        return searchableCrate.Type == CrateType.Prop;
    }
}