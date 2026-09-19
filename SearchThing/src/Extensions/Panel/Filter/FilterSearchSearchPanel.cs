using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Extensions.Panel.Data;
using SearchThing.Extensions.Panel.Data.Extensions;
using SearchThing.Presets;
using SearchThing.Search.Data;
using SearchThing.Search.Marrow;
using SearchThing.Search.Search;
using SearchThing.Search.Sorting;
using SearchThing.Util;
using UnityEngine;

namespace SearchThing.Extensions.Panel.Filter;

public abstract class FilterSearchSearchPanel : BasicSearchPanel<MarrowCrate>
{
    private static readonly Sprite PresetAddIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.AddIcon.png");

    public override abstract string Name { get; }
    protected abstract bool Filter(MarrowCrate searchableCrate);

    public Color? GetItemFunctionHighlight(SpawnablePanelExtension extension, IItemRenderInfo itemInfo)
    {
        return PresetManager.IsAssignmentMode ? Color.green : null;
    }

    public void OnItemFunction(SpawnablePanelExtension extension, IItemRenderInfo itemInfo)
    {
        if (itemInfo.GetSource() is not ICrateBoundItemInfo { Crate: ISearchableItemInfo crate })
            return;
        
        PresetManager.StartAssignmentMode(extension, crate);

        extension.RenderAll();
    }

    public override ItemRenderInfo GetRenderDataForCrate(MarrowCrate crate)
    {
        return new ItemRenderInfo(crate, new ActionExtension(OnItemFunction)
        {
            GetActionIconFunc = (_, _) => PresetAddIcon,
            GetActionHighlightFunc = GetItemFunctionHighlight
        });
    }

    protected override void Search(string query, ISearchOrder order, Action<ISearchResults<MarrowCrate>> callback)
    {
        SearchManager.SearchAsync(query, MarrowCrateManager.GetCrates(), Filter, order, callback);
    }
}