using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Search;
using SearchThing.Search.Marrow;

namespace SearchThing.Extensions.Panel.Filter;

public class RedactedSearchPanel : FilterSearchSearchPanel
{
    public override string Name => "Redacted";
    protected override bool Filter(MarrowCrate searchableCrate)
    {
        return searchableCrate is { Redacted: true };
    }
}