using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Extensions;
using SearchThing.Extensions.Panel;
using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Extensions.Sort;
using SearchThing.Presets.Data;
using SearchThing.Search;
using SearchThing.Util;

namespace SearchThing.Presets;

internal class BarcodeComparison : IEqualityComparer<Barcode>
{
    public bool Equals(Barcode? x, Barcode? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return x._shortCode == y._shortCode;
    }
    
    public int GetHashCode(Barcode obj)
    {
        return (int)obj._shortCode;
    }
}

public class Preset : BasicSearchPanel
{
    private const string DefaultTag = "EMPTY";
    
    private string _tag = DefaultTag;
    public override string Tag => IsInitialized ? _tag : (PresetManager.IsAssignmentMode ? "Add Presset" : "Empty Preset");
    public bool IsInitialized { get; private set; }
    public override bool ResearchOnPageChange => true;
    public HashSet<Barcode> AssignedBarcodes { get; } = new(new BarcodeComparison());

    public Preset()
    {
        IsInitialized = false;
    }
    
    public Preset(string tag)
    {
        _tag = tag;
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
            
            // If we aren't selected, we won't rerender later, so we do it now
            if (!isSelected)
                extension.Rerender();
        }

        var barcode = extension.GetSelectedSpawnable();
        if (barcode == null)
            return false;
        
        AssignedBarcodes.Add(barcode._barcode);
        
        if (isSelected)
            extension.RequestRefresh();
        
        return false;
    }
    
    protected override void Search(string query, ISearchOrder order, Action<SearchResults> callback)
    {
        // Actually search here
        var results = AssignedBarcodes
            .ToSearchResults();
        
        callback(results);
    }
    
    public PresetData ToData()
    {
        return new PresetData
        {
            Name = _tag,
            Barcodes = AssignedBarcodes.Select(b => b._id).ToList()
        };
    }
    
    public void FromData(PresetData data)
    {
        _tag = data.Name;
        IsInitialized = _tag != DefaultTag;
        
        if (!IsInitialized)
            return;
        
        AssignedBarcodes.Clear();
        foreach (var barcodeId in data.Barcodes)
        {
            AssignedBarcodes.Add(new Barcode(barcodeId));
        }
        
    }
}