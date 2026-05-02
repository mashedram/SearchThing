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

public class Preset : BasicSearchPanel
{
    private const string DefaultTag = "EMPTY";
    
    private string _tag = DefaultTag;
    public override string Tag => IsInitialized ? _tag : (PresetManager.IsAssignmentMode ? "Add Presset" : "Empty Preset");
    public override bool TagEditable => true;
    public bool IsInitialized { get; private set; }
    public override bool ResearchOnPageChange => true;
    public HashSet<ISearchableCrate> AssignedCrates { get; } = new();

    public Preset()
    {
        IsInitialized = false;
    }
    
    public Preset(string tag)
    {
        _tag = tag;
        IsInitialized = true;
    }

    public override Color? IsForceHighlighted(SpawnablePanelExtension extension, ISearchableCrate? selectedCrate)
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
        
        // TODO: When initializing prompt for a tag
        if (!IsInitialized)
        {
            _tag = $"Preset {Random.Shared.Next(1000, 9999)}";
            IsInitialized = true;
        }

        var crate = extension.GetSelectedSpawnable();
        if (crate == null)
            return false;

        if (!AssignedCrates.Add(crate))
        {
            AssignedCrates.Remove(crate);
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
    
    public void RemoveCrate(ISearchableCrate crate)
    {
        AssignedCrates.Remove(crate);
    }
    
    protected override void Search(string query, ISearchOrder order, Action<SearchResults> callback)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            var emptyResults = AssignedCrates
                .OrderByDescending(order.Score)
                .ThenByDescending(entry => entry.Salt) // Tie-breaker: more recent entries first
                .ToSearchResults();
            
            callback(emptyResults);
            return;
        }
        
        var lowerQuery = query.ToLowerInvariant();
        var preprocessedQuery = StringPreprocessorFactory.GetPreprocessor(PreprocessMode.Full)(lowerQuery);
        
        // Presets cannot search for now
        var results = AssignedCrates
            .Select(entry => ScoredCrate.ScoreCrate(entry, preprocessedQuery))
            .Where(entry => entry.Score >= 80)
            .OrderByDescending(order.Score)
            .ThenByDescending(entry => entry.Salt) // Tie-breaker: more recent entries first
            .ToSearchResults();
        
        callback(results);
    }
    
    public PresetData ToData()
    {
        return new PresetData
        {
            Name = _tag,
            Barcodes = AssignedCrates.Select(b => b.Barcode._id).ToList()
        };
    }
    
    public void FromData(PresetData data)
    {
        _tag = data.Name;
        IsInitialized = _tag != DefaultTag;
        
        if (!IsInitialized)
            return;
        
        AssignedCrates.Clear();
        foreach (var barcodeId in data.Barcodes)
        {
            AssignedCrates.Add(new SearchableCrate(new Barcode(barcodeId)));
        }
        
    }
}