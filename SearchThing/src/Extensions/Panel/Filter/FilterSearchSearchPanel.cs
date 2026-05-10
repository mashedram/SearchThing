using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Presets;
using SearchThing.Search;
using SearchThing.Util;
using UnityEngine;

namespace SearchThing.Extensions.Panel.Filter;

public abstract class FilterSearchSearchPanel : BasicSearchPanel<MarrowCrate>
{
    private static readonly Sprite PresetAddIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.AddIcon.png");
    
    public override abstract string Tag { get; }
    protected abstract bool Filter(MarrowCrate searchableCrate);

    public override bool HasItemFunction => true;
    public override Sprite ItemFunctionIcon => PresetAddIcon;

    public override Color? GetItemFunctionHighlight(SpawnablePanelExtension extension, IFullCrateData? crate)
    {
        return PresetManager.IsAssignmentMode ? Color.green : null;
    }

    public override void OnItemFunction(SpawnablePanelExtension extension, IFullCrateData crate)
    {
        PresetManager.IsAssignmentMode = !PresetManager.IsAssignmentMode;
        
        extension.RenderAll();
    }

    protected override void Search(string query, ISearchOrder order, Action<SearchResults<MarrowCrate>> callback)
    {
        SearchManager.SearchAsync(query, MarrowCrateManager.GetCrates(), Filter, order, callback);
    }
}