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
    public override string Description => GetDescription();
    // True when the preset is first clicked, and thus selected but not open
    private Preset? _preset;

    private string GetDescription()
    {
        if (PresetManager.IsAssignmentMode)
            return "Click a preset to assign the item you selected to that preset.\n Start typing if you want to make a new preset.";

        if (_preset == null)
        {
            if (PresetManager.PresetCount <= 0)
                return "No presets have been made yet.\nStart typing to make your first preset.";

            return "Select a preset to edit or spawn its contents.";
        }

        return _preset.Description;
    }

    public override bool OnItemSelected(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo)
    {
        // We selected an item, yay
        if (itemInfo is not ICrateBoundItemInfo { Crate: Preset /*Check if it's a preset, and if not, don't do anything*/ preset })
            return true;

        if (PresetManager.IsAssignmentMode)
        {
            if (!PresetManager.TryGetAssigningCrate(out var searchableItemInfo))
                return true;

            preset.ToggleCrate(searchableItemInfo);

            PresetManager.StopAssignmentMode(extension);
            return false;
        }


        if (_preset != preset)
        {
            _preset = preset;
            _preset.IsPreview = true;
        }
        else
        {
            Query = string.Empty;
            _preset.IsPreview = false;
        }
        MakeDirty();
        // Might need a refresh here

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

        if (itemInfo is ICrateBoundItemInfo { Crate: Preset preset })
        {
            // Presets can only be deleted while they are being previewed
            if (preset.IsPreview)
                return;
            
            _preset = null;
            PresetManager.RemovePreset(preset);
            MakeDirty();
            extension.InfoBox.SetContent(this);
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

        return results;
    }

    private void AddPreset(SpawnablePanelExtension extension, int idx)
    {
        var preset = new Preset(Query);
        PresetManager.AddPreset(preset);
        Query = string.Empty;

        if (!PresetManager.TryGetAssigningCrate(out var searchableItemInfo))
            return;

        preset.ToggleCrate(searchableItemInfo);
        MakeDirty();

        PresetManager.StopAssignmentMode(extension);
    }

    protected override void Search(string query, ISearchOrder order, Action<ISearchResults<ISearchableItemInfo>> callback)
    {
        if (_preset == null || _preset.IsPreview)
        {
            SearchManager.SearchAsync(query, PresetManager.PresetList.ToSearchable(), c => !c.Redacted, order, callback);
            return;
        }

        SearchManager.SearchAsync(query, _preset.AssignedCrates.ToSearchable(), _ => true, order, callback);
    }
}