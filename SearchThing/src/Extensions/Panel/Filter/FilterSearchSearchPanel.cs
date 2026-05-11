using SearchThing.Extensions.Components;
using SearchThing.Extensions.Components.Info;
using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Extensions.Panel.Data;
using SearchThing.Presets;
using SearchThing.Search;
using SearchThing.Search.CrateData;
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

    public Color? GetItemFunctionHighlight(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo)
    {
        return PresetManager.IsAssignmentMode ? Color.green : null;
    }

    public void OnItemFunction(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo)
    {
        PresetManager.ToggleAssigmentMode(extension);

        extension.RenderAll();
    }

    public override ItemRender GetRenderDataForCrate(MarrowCrate crate)
    {
        return new ItemRenderWithAction(crate, OnItemFunction)
        {
            GetActionIconFunc = (_, _) => PresetAddIcon,
            GetActionHighlightFunc = GetItemFunctionHighlight
        };
    }

    protected override void Search(string query, ISearchOrder order, Action<ISearchResults<MarrowCrate>> callback)
    {
        SearchManager.SearchAsync(query, MarrowCrateManager.GetCrates(), Filter, order, callback);
    }
}