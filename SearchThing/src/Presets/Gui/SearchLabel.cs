using SearchThing.Extensions;
using SearchThing.Search.Data;
using SearchThing.Search.Interaction;
using SearchThing.Search.Search;

namespace SearchThing.Presets.Gui;

public class SearchLabel : ISearchableItemInfo, ISelectableCrate
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; }
    public bool Redacted => false;
    public DateTime DateAdded { get; } = DateTime.Now;

    public SearchLabel(string query)
    {
        Name = query;
    }
    
    public bool OnSelected(SpawnablePanelExtension extension, int idx)
    {
        return false;
    }
    
    // Unused
    public IEnumerable<IFuzzySearchable> SearchFields => Array.Empty<IFuzzySearchable>();
    public int Salt => 0;
}