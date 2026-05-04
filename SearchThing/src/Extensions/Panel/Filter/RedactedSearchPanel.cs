using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Search;

namespace SearchThing.Extensions.Panel.Filter;

public class RedactedSearchPanel : FilterSearchSearchPanel
{
    public override string Tag => "Redacted";
    protected override bool Filter(SearchableCrate searchableCrate)
    {
        return searchableCrate is { Redacted: true };
    }
}