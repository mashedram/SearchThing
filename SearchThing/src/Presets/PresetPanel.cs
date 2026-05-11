using SearchThing.Extensions;
using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Extensions.Panel.Data;
using SearchThing.Presets.Gui;
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
            var selectedItem = extension.GetSelectedItemInfo();
            if (selectedItem is not ICrateBoundItemInfo { Crate: ISearchableItemInfo searchableItemInfo })
                return true;

            preset.ToggleCrate(searchableItemInfo);
            
            PresetManager.ToggleAssigmentMode(extension);
            return false;
        }
        

        _preset = preset;
        Query = string.Empty;
        MakeDirty();
        extension.RequestRefresh();
        
        return true;
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

        if (itemInfo is ICrateBoundItemInfo { Crate: Preset preset }) {
            _preset = null;
            PresetManager.RemovePreset(preset);
            MakeDirty();
            extension.InfoBox.SetContent(null);
            extension.RequestRefresh();
            return;
        }
        
        if (itemInfo is not ICrateBoundItemInfo { Crate: ISearchableItemInfo searchableItemInfo })
            return;
        
        _preset.ToggleCrate(searchableItemInfo);
        extension.InfoBox.SetContent(null);
        MakeDirty();
    }

    public override ItemRender GetRenderDataForCrate(ISearchableItemInfo crate)
    {
        return new ItemRenderWithAction(crate, ItemQuickAction)
        {
            GetActionIconFunc = (_, _) => PresetRemoveIcon,
            GetActionHighlightFunc = (_, _) => Color.red
        };
    }

    public override ISearchResults<ISearchableItemInfo> Parse(ISearchResults<ISearchableItemInfo> results)
    {
        if (_preset != null)
        {
            if (_preset.AssignedCrates.Count == 0)
                return new SearchButtonList(new SearchLabel("Here be dragons!"));
            
            return results;
        }

        // Add an add button to the end of the preset list if we are typing a new preset name
        if (!string.IsNullOrWhiteSpace(Query))
            return new SearchButtonOverwrite<ISearchableItemInfo>(results, (0, new ActionButton($"Add: \"{Query}\"", AddPreset)));
        
        if (PresetManager.PresetCount == 0)
            return new SearchButtonList(new SearchLabel("Type to create a preset"));
            
        return results;
    }
    
    private void AddPreset(SpawnablePanelExtension extension, int idx)
    {
        var preset = new Preset(Query);
        PresetManager.AddPreset(preset);
        Query = string.Empty;
        
        var itemInfo = extension.GetSelectedItemInfo();
        if (itemInfo is not ICrateBoundItemInfo { Crate: ISearchableItemInfo searchableItemInfo })
            return;
        
        preset.ToggleCrate(searchableItemInfo);
        MakeDirty();
        
        PresetManager.ToggleAssigmentMode(extension);
    }

    protected override void Search(string query, ISearchOrder order, Action<ISearchResults<ISearchableItemInfo>> callback)
    {
        if (_preset == null)
        {
            SearchManager.SearchAsync(query, PresetManager.PresetList.ToSearchable(), c => !c.Redacted, order, callback);
            return;
        }
        
        SearchManager.SearchAsync(query, _preset.AssignedCrates.ToSearchable(), c => true, order, callback);
    }
}