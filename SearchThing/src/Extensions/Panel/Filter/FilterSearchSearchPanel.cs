using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Presets;
using SearchThing.Search;
using SearchThing.Util;
using UnityEngine;

namespace SearchThing.Extensions.Panel.Filter;

public abstract class FilterSearchSearchPanel : BasicSearchPanel<ISearchableCrate>
{
    private static readonly Sprite PresetAddIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.AddIcon.png");
    
    public override abstract string Tag { get; }
    protected abstract bool Filter(ISearchableCrate searchableCrate);

    public override bool HasItemFunction => true;
    public override Sprite ItemFunctionIcon => PresetAddIcon;

    public override Color? GetItemFunctionHighlight(SpawnablePanelExtension extension, ISearchableCrate? crate)
    {
        return PresetManager.IsAssignmentMode ? Color.green : null;
    }

    public override void OnItemFunction(SpawnablePanelExtension extension, ISearchableCrate crate)
    {
        PresetManager.IsAssignmentMode = !PresetManager.IsAssignmentMode;
        
        extension.RenderAll();
    }

    protected override void Search(string query, ISearchOrder order, Action<SearchResults<ISearchableCrate>> callback)
    {
        SearchManager.SearchAsync(query, GlobalCrateManager.GetCrates(), Filter, order, callback);
    }
}