using SearchThing.Extensions;
using SearchThing.Search.Data;
using SearchThing.Search.Interaction;
using SearchThing.Search.Search;

namespace SearchThing.Presets.Gui;

public class ActionButton : ISearchableItemInfo, ISelectableCrate
{
    public delegate void OnSelectHandler(SpawnablePanelExtension extension, int idx);
    
    private readonly OnSelectHandler _onSelected;
    
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; }
    public bool Redacted => false;
    public DateTime DateAdded { get; } = DateTime.Now;
    
    public bool OnSelected(SpawnablePanelExtension extension, int idx)
    {
        _onSelected.Invoke(extension, idx);
        return false; // Buttons shouldn't take focus
    }

    public ActionButton(string query, OnSelectHandler onSelected)
    {
        Name = query;
        _onSelected = onSelected;
    }
    
    // Unused
    public IEnumerable<IFuzzySearchable> SearchFields => Array.Empty<IFuzzySearchable>();
    public int Salt => 0;
}