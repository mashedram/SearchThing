using SearchThing.Search.Marrow;

namespace SearchThing.Extensions.Panel.Filter;

public class RedactedSearchPanel : FilterSearchSearchPanel
{
    public override string Name => "Redacted";
    public override string Description => "Search through all the redacted items in your crates.\nUse the plus button to add an item to a preset of your choice.";
    protected override bool Filter(MarrowCrate searchableCrate)
    {
        return searchableCrate is { Redacted: true };
    }
}