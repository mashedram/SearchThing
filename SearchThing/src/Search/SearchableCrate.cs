using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Util;

namespace SearchThing.Search;

public class SearchableCrate : ISearchableCrate, IEquatable<SearchableCrate>
{
    public SearchTag Name { get; }
    public SearchTag PalletName { get; }
    public SearchTag Author { get; }
    public SearchTag[] Tags { get; }
    public string Description { get; }
    public int Salt { get; } // Used for tie-breaking to ensure consistent ordering
    public CrateType CrateType { get; }
    // Default to zero for global searchables
    public virtual int Score => 0;
    public DateTime DateAdded { get; }
    public Barcode Barcode { get; }
    
    public SearchableCrate(Crate spawnableCrate)
    {
        if (spawnableCrate == null)
            throw new ArgumentNullException(nameof(spawnableCrate));
        
        Name = new SearchTag(spawnableCrate.name);
        Description = spawnableCrate._description;
        PalletName = new SearchTag(spawnableCrate._pallet.name);
        Author = new SearchTag(spawnableCrate._pallet._author);
        Tags = spawnableCrate._tags.ToArray().Select(t => new SearchTag(t)).ToArray();

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
        
        Barcode = spawnableCrate.Barcode;
    }

    public SearchableCrate(Barcode barcode) : this(AssetWarehouse.Instance.TryGetCrate(barcode, out var crate) ? crate : throw new ArgumentException($"Crate with barcode {barcode} not found in warehouse", nameof(barcode)))
    {
        
    }
    public bool Equals(SearchableCrate? other)
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
        return Equals((SearchableCrate)obj);
    }
    
    public override int GetHashCode()
    {
        return Barcode._id.GetHashCode();
    }
}