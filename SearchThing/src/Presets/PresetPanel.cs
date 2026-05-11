using SearchThing.Extensions;
using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Extensions.Panel.Data;
using SearchThing.Search.Containers;
using SearchThing.Search.Data;
using SearchThing.Search.Search;
using SearchThing.Search.Sorting;
using SearchThing.Util;
using UnityEngine;

namespace SearchThing.Presets;

public class PresetPanel : BasicSearchPanel<ISearchableItemInfo>
{
    private static readonly Sprite PresetRemoveIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.RemoveIcon.png");
    private static readonly Sprite EditIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.EditIcon.png");
    
    public override string Name => "Presets";
    private Preset? _preset;

    public override bool OnItemSelected(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo)
    {
        // We selected an item, yay
        if (itemInfo is not ICrateBoundItemInfo { Crate: Preset preset })
            return true;

        if (PresetManager.IsAssignmentMode)
        {
            var selectedItem = extension.GetSelectedSpawnable();
            if (selectedItem is not ICrateBoundItemInfo { Crate: ISearchableItemInfo searchableItemInfo })
                return true;

            if (!preset.AssignedCrates.Add(searchableItemInfo))
                preset.AssignedCrates.Remove(searchableItemInfo);
            
            PresetManager.ToggleAssigmentMode(extension);
            return false;
        }
        

        _preset = preset;
        MakeDirty();
        extension.RequestRefresh();
        
        // We can't actually select a preset, so return false to prevent the panel from closing.
        return false;
    }

    public override bool OnPanelSelected(SpawnablePanelExtension extension)
    {
        _preset = null;
        MakeDirty();
        return true;
    }
    
    private void ItemQuickAction(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo)
    {
        if (_preset == null)
            return;
        if (itemInfo is not ISearchableItemInfo searchableItemInfo)
            return;
        
        _preset.AssignedCrates.Remove(searchableItemInfo);
    }

    public override ItemRender GetRenderDataForCrate(ISearchableItemInfo crate)
    {
        // TODO : Allow editing the preset name
        if (_preset == null)
            return new ItemRender(crate);
        
        return new ItemRenderWithAction(crate, ItemQuickAction)
        {
            GetActionIconFunc = (_, _) => PresetRemoveIcon,
            GetActionHighlightFunc = (_, _) => Color.red
        };
    }

    public override ISearchResults<ISearchableItemInfo> Parse(ISearchResults<ISearchableItemInfo> results)
    {
        if (_preset != null || Query == string.Empty)
            return results;

        // Add an add button to the end of the preset list
        return new SearchButtonOverwrite<ISearchableItemInfo>(results, (0, new ActionButton($"Add: {Query}",AddPreset)));
    }
    
    private void AddPreset()
    {
        PresetManager.AddPreset(new Preset(Query));
        Query = string.Empty;
    }

    protected override void Search(string query, ISearchOrder order, Action<ISearchResults<ISearchableItemInfo>> callback)
    {
        if (_preset == null)
        {
            SearchManager.SearchAsync(query, PresetManager.PresetList.ToSearchable(), c => true, order, callback);
            return;
        }
        
        SearchManager.SearchAsync(query, _preset.AssignedCrates.ToSearchable(), c => true, order, callback);
    }
}