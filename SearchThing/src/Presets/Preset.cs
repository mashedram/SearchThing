using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Extensions;
using SearchThing.Extensions.Components;
using SearchThing.Extensions.Components.Info;
using SearchThing.Extensions.Panel;
using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Extensions.Panel.Data;
using SearchThing.Extensions.Sort;
using SearchThing.Presets.Data;
using SearchThing.Search;
using SearchThing.Search.Containers;
using SearchThing.Search.CrateData;
using SearchThing.Search.Data;
using SearchThing.Search.Search;
using SearchThing.Search.Sorting;
using SearchThing.Util;
using UnityEngine;
using Random = System.Random;

namespace SearchThing.Presets;

internal class SpawnableCrateComparer : IEqualityComparer<IRequiredItemInfo>
{
    public bool Equals(IRequiredItemInfo? x, IRequiredItemInfo? y)
    {
        if (x == null || y == null)
            return false;

        return x.Id == y.Id;
    }

    public int GetHashCode(IRequiredItemInfo obj)
    {
        return obj.Id.GetHashCode();
    }
}

public class Preset : BasicSearchPanel<ISearchableItemInfo>, IQuickActionItemInfo
{
    private const string DefaultTag = "EMPTY";

    private static readonly Sprite PresetRemoveIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.RemoveIcon.png");
    private static readonly Sprite EditIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.EditIcon.png");

    private string _tag = DefaultTag;
    public override string Name => IsInitialized ? _tag : PresetManager.IsAssignmentMode ? "Add Presset" : "Empty Preset";
    public override bool TagEditable => true;
    public bool IsInitialized { get; private set; }
    public override bool ResearchOnPageChange => true;
    public HashSet<ISearchableItemInfo> AssignedCrates { get; } = new(new SpawnableCrateComparer());

    public Preset()
    {
        IsInitialized = false;
    }

    public Preset(string tag)
    {
        _tag = tag;
        IsInitialized = true;
    }
    
    // Panel functions
    public Sprite? GetActionIcon(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo)
    {
        return EditIcon;
    }
    
    public Color? GetActionHighlight(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo)
    {
        return extension.IsEditing ? Color.green : Color.white; 
    }
    
    public void PerformQuickAction(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo)
    {
        extension.SetIsEditing(!extension.IsEditing); 
        extension.RenderAll();
    }
    
    // Item functions

    public Color? GetItemFunctionHighlight(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo)
    {
        return Color.red;
    }

    public void OnItemFunction(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo)
    {
        PresetManager.IsAssignmentMode = false;

        if (itemInfo is not ISearchableItemInfo requiredItemInfo)
            return;
        RemoveCrate(requiredItemInfo);

        extension.RequestRefresh();
    }

    public override Color? IsForceHighlighted(SpawnablePanelExtension extension)
    {
        if (extension.IsPanelSelected(this))
            return Color.white;

        var selectedCrate = extension.GetSelectedSpawnable();
        var isForceHighlighted = IsInitialized && selectedCrate != null && AssignedCrates.Contains(selectedCrate);
        return isForceHighlighted ? Color.yellow : null;
    }

    private bool IsTagValid(string tag)
    {
        return !string.IsNullOrEmpty(tag);
    }

    public override void OnTagEdited(SpawnablePanelExtension extension, string newTag)
    {
        // If the tag is valid, assign it
        if (!IsTagValid(newTag))
            return;

        _tag = newTag;
        IsInitialized = true;
    }

    public override bool OnSelected(SpawnablePanelExtension extension)
    {
        if (!PresetManager.IsAssignmentMode)
            return true;

        var isSelected = extension.IsPanelSelected(this);

        var crate = extension.GetSelectedSpawnable();
        if (crate is not ISearchableItemInfo spawnableCrate)
            return false;

        // TODO: When initializing prompt for a tag
        if (!IsInitialized)
        {
            _tag = $"Preset {Random.Shared.Next(1000, 9999)}";
            IsInitialized = true;
        }

        if (!AssignedCrates.Add(spawnableCrate))
        {
            AssignedCrates.Remove(spawnableCrate);
        }

        // Exiting assignment mode immediately feels nicer
        PresetManager.IsAssignmentMode = false;

        if (isSelected)
        {
            extension.RequestRefresh();
        }
        else
        {
            extension.RenderAll();
        }

        return false;
    }

    public void RemoveCrate(ISearchableItemInfo crate)
    {
        AssignedCrates.Remove(crate);
    }

    public override ItemRender GetRenderDataForCrate(ISearchableItemInfo crate)
    {
        return new ItemRenderWithAction(crate, OnItemFunction)
        {
            GetActionIconFunc = (_, _) => PresetRemoveIcon,
            GetActionHighlightFunc = GetItemFunctionHighlight
        };
    }

    protected override void Search(string query, ISearchOrder order, Action<SearchResults<ISearchableItemInfo>> callback)
    {
        SearchManager.SearchAsync(query, AssignedCrates.ToSearchable(), _ => true, order, callback);
    }
}