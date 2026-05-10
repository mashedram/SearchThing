using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Util;

namespace SearchThing.Search;

public class MarrowCrate : IFullCrate, IBarcodeHolder, IEquatable<MarrowCrate>
{
    private readonly SearchTag _name;
    private readonly SearchTag _palletName;
    private readonly SearchTag _author;
    private readonly SearchTag[] _tags;
    
    public string Name => _name.Original;
    public string PalletName => _palletName.Original;
    public string Author => _author.Original;
    public IEnumerable<string> Tags => _tags.Select(t => t.Original);
    
    public string Description { get; }
    public bool Redacted { get; }
    public int Salt { get; } // Used for tie-breaking to ensure consistent ordering
    public CrateType CrateType { get; }
    public CrateSubType CrateSubType { get; }
    public DateTime DateAdded { get; }
    public Barcode Barcode { get; }
    
    public MarrowCrate(Crate spawnableCrate)
    {
        if (spawnableCrate == null)
            throw new ArgumentNullException(nameof(spawnableCrate));
        
        _name = new SearchTag(spawnableCrate.name);
        Description = spawnableCrate._description;
        _palletName = new SearchTag(spawnableCrate._pallet.name);
        _author = new SearchTag(spawnableCrate._pallet._author);
        _tags = spawnableCrate._tags.ToArray().Select(t => new SearchTag(t)).ToArray();
        
        Redacted = spawnableCrate._redacted;

        Salt = spawnableCrate.name.GetSalt();
        
        if (!spawnableCrate._pallet.IsInMarrowGame() && AssetWarehouse.Instance.TryGetPalletManifest(spawnableCrate._pallet._barcode, out var palletManifest))
        {
            DateAdded = long.TryParse(palletManifest.UpdatedDate, out var unixTimestampMs) 
                ? DateTime.UnixEpoch.AddMilliseconds(unixTimestampMs) 
                : DateTime.MinValue;
        }
        else
        {
            DateAdded = DateTime.MinValue; // Fallback to now if we can't find the pallet, shouldn't really happen
        }

        CrateType = spawnableCrate.GetCrateType();
        CrateSubType = spawnableCrate.GetCrateSubType(CrateType);
        
        Barcode = spawnableCrate.Barcode;
    }

    public MarrowCrate(Barcode barcode) : this(AssetWarehouse.Instance.TryGetCrate(barcode, out var crate) ? crate : throw new ArgumentException($"Crate with barcode {barcode} not found in warehouse", nameof(barcode)))
    {
        
    }
    public bool Equals(MarrowCrate? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Barcode._id.Equals(other.Barcode._id);
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((MarrowCrate)obj);
    }
    
    public override int GetHashCode()
    {
        return Barcode._id.GetHashCode();
    }

    public IEnumerable<IFuzzySearchable> SearchFields => new IFuzzySearchable[]
    {
        _name,
        _palletName,
        _author,
        new SearchTagGroup(_tags)
    };
}