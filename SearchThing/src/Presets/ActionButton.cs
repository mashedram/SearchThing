using SearchThing.Extensions;
using SearchThing.Search.CrateData;
using SearchThing.Search.Data;
using SearchThing.Search.Interaction;

namespace SearchThing.Presets;

public class ActionButton : ISearchableItemInfo, ISelectableCrate
{
    private readonly Action _onSelected;
    
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; }
    public bool Redacted => false;
    public DateTime DateAdded { get; } = DateTime.Now;
    
    public bool OnSelected(SpawnablePanelExtension extension, int idx)
    {
        _onSelected.Invoke();
        return false; // Buttons shouldn't take focus
    }

    public ActionButton(string query, Action onSelected)
    {
        Name = query;
        _onSelected = onSelected;
    }
    
    // Unused
    public IEnumerable<IFuzzySearchable> SearchFields => Array.Empty<IFuzzySearchable>();
    public int Salt => 0;
}