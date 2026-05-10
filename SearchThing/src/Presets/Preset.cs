using FuzzySharp.PreProcess;
using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Extensions;
using SearchThing.Extensions.Panel;
using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Extensions.Sort;
using SearchThing.Presets.Data;
using SearchThing.Search;
using SearchThing.Util;
using UnityEngine;
using Random = System.Random;

namespace SearchThing.Presets;

internal class SpawnableCrateComparer : IEqualityComparer<IFullCrate>
{
    public bool Equals(IFullCrate? x, IFullCrate? y)
    {
        if (x == null || y == null)
            return false;
        
        return x.Name == y.Name;
    }

    public int GetHashCode(IFullCrate obj)
    {
        return obj.Name.GetHashCode();
    }
}

public class Preset : BasicSearchPanel<IFullCrate>
{
    private const string DefaultTag = "EMPTY";
 
    private static readonly Sprite PresetRemoveIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.RemoveIcon.png");
    private static readonly Sprite EditIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.EditIcon.png");
    
    private string _tag = DefaultTag;
    public override string Tag => IsInitialized ? _tag : (PresetManager.IsAssignmentMode ? "Add Presset" : "Empty Preset");
    public override bool TagEditable => true;
    public bool IsInitialized { get; private set; }
    public override bool ResearchOnPageChange => true;
    public HashSet<IFullCrate> AssignedCrates { get; } = new(new SpawnableCrateComparer());

    public Preset()
    {
        IsInitialized = false;
    }
    
    public Preset(string tag)
    {
        _tag = tag;
        IsInitialized = true;
    }

    public override bool HasPanelFunction => true;
    public override Sprite PanelFunctionIcon => EditIcon;
    public override bool HasItemFunction => true;
    public override Sprite ItemFunctionIcon => PresetRemoveIcon;

    public override Color? GetItemFunctionHighlight(SpawnablePanelExtension extension, IFullCrateData? crate)
    {
        return Color.red;
    }

    public override void OnItemFunction(SpawnablePanelExtension extension, IFullCrateData crate)
    {
        PresetManager.IsAssignmentMode = false;
        
        if (crate is not IFullCrate spawnableCrate)
            return;
        RemoveCrate(spawnableCrate);
                
        extension.RequestRefresh();
    }

    public override Color? GetPanelFunctionHighlight(SpawnablePanelExtension extension)
    {
        return extension.IsEditing ? Color.green : Color.white;
    }

    public override void OnPanelFunction(SpawnablePanelExtension extension)
    {
        extension.SetIsEditing(!extension.IsEditing);
            
        extension.RenderFavoriteButton();
    }

    public override Color? IsForceHighlighted(SpawnablePanelExtension extension, IFullCrateData? selectedCrate)
    {
        if (extension.IsPanelSelected(this))
            return Color.white;
        
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
        if (crate is not IFullCrate spawnableCrate)
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
            extension.RenderTags();
            extension.RenderFavoriteButton();
        }
        
        return false;
    }
    
    public void RemoveCrate(IFullCrate crate)
    {
        AssignedCrates.Remove(crate);
    }
    
    protected override void Search(string query, ISearchOrder order, Action<SearchResults<IFullCrate>> callback)
    {
        SearchManager.SearchAsync(query, AssignedCrates.ToSearchable(), _ => true, order, callback);
    }
}