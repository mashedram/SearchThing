using Il2CppSLZ.Marrow.Warehouse;

namespace SearchThing.Search;

public class SearchResultEntry
{
    public Barcode Barcode;

    private SpawnableCrate? _crate;
    public SpawnableCrate? Crate
    {
        get
        {
            if (_crate != null)
                return _crate;

            if (!AssetWarehouse.Instance.TryGetCrate(Barcode, out _crate))
                return null;
            
            return _crate;
        }
    }
    
    public SearchResultEntry(Barcode barcode)
    {
        Barcode = barcode;
    }
}

public class SearchResults
{
    public static SearchResults Empty { get; } = new(new List<SearchResultEntry>());
    private IReadOnlyList<SearchResultEntry> Entries { get; }
    
    public SearchResults(List<SearchResultEntry> entries)
    {
        Entries = entries;
    }

    public IEnumerable<SearchResultEntry> GetPage(int page, int pageSize)
    {
        return Entries.Skip(page * pageSize).Take(pageSize);
    }
    
    public int GetPageCount(int pageSize)
    {
        return (Entries.Count + pageSize - 1) / pageSize;
    }
}