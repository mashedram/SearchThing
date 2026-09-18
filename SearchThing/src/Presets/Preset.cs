using MelonLoader;
using SearchThing.Presets.Data;
using SearchThing.Search.Data;
using SearchThing.Search.Database;
using SearchThing.Search.Marrow;
using SearchThing.Search.Search;
using SearchThing.Util;
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

public class Preset : ISearchableItemInfo, IDescriptiveItemInfo
{
    private readonly SearchTag _nameTag;

    public Guid Id { get; } = Guid.NewGuid();
    public string Name => _nameTag.Original;

    public string Description =>
        IsPreview
            ? "Select again to open the preset.\nUse the minus button to delete the preset."
            : "Select an item to spawn it.\nUse the minus on an item to remove it from the preset.";

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
    public IEnumerable<IFuzzySearchable> SearchFields => new IFuzzySearchable[]
    {
        _nameTag
    };
    public int Salt { get; } = Random.Shared.Next();

    public HashSet<ISearchableItemInfo> AssignedCrates { get; } = new(new SpawnableCrateComparer());
    public bool IsDirty { get; private set; } = true;
    public bool IsPreview { get; set; }

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