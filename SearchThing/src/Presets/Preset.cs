using Il2CppSLZ.Marrow.Warehouse;
using MelonLoader;
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
using SearchThing.Search.Database;
using SearchThing.Search.Marrow;
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

public class Preset : ISearchableItemInfo
{
    private readonly SearchTag _nameTag;
    
    public Guid Id { get; } = Guid.NewGuid();
    public string Name => _nameTag.Original;
    // Used for deletion marking
    private bool _isRedacted;
    public bool Redacted
    {
        get => _isRedacted;
        set
        {
            _isRedacted = value;
            IsDirty = true;
        }
    }

    public DateTime DateAdded { get; } = DateTime.Now;
    public IEnumerable<IFuzzySearchable> SearchFields => new IFuzzySearchable[] { _nameTag };
    public int Salt { get; } = Random.Shared.Next();
    
    public HashSet<ISearchableItemInfo> AssignedCrates { get; } = new(new SpawnableCrateComparer());
    public bool IsDirty { get; private set; } = true;
    
    public void ToggleCrate(ISearchableItemInfo crate)
    {
        if (!AssignedCrates.Add(crate))
            AssignedCrates.Remove(crate);

        IsDirty = true;
    }
    
    public PresetData ToData()
    {
        return new PresetData
        {
            Version = 1,
            Id = Id,
            Name = Name,
            DateAdded = DateAdded,
            Items = AssignedCrates.Select(c => c.Id).ToList()
        };
    }
    
    public Preset(PresetData data)
    {
        if (data.Version != 1)
            throw new InvalidOperationException($"Unsupported preset data version: {data.Version}");

        Id = data.Id;
        _nameTag = new SearchTag(data.Name);
        DateAdded = data.DateAdded;
        AssignedCrates.Clear();

        foreach (var id in data.Items)
        {
            var barcode = CrateDatabaseManager.GetBarcode(id);
            MelonLogger.Msg($"Looking for crate with id {id}, found barcode {barcode}");
            if (barcode == null)
                continue;
            
            var crate = MarrowCrateManager.GetCrate(barcode);
            if (crate != null)
                AssignedCrates.Add(crate);
        }

        IsDirty = false;
    }
    
    public Preset(string name)
    {
        _nameTag = new SearchTag(name);
    }
}