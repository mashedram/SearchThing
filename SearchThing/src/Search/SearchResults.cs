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
    
    private IEnumerable<SearchResultEntry> GetPageIterator(int start, int end)
    {
        for (var i = start; i < end; i++)
            yield return Entries[i];
    }

    public IEnumerable<SearchResultEntry> GetPage(int page, int pageSize)
    {
        var start = page * pageSize;
        var end = Math.Min(start + pageSize, Entries.Count);

        if (start >= Entries.Count)
            return Array.Empty<SearchResultEntry>();

        return GetPageIterator(start, end);
    }
    
    public int GetPageCount(int pageSize)
    {
        return (Entries.Count + pageSize - 1) / pageSize;
    }
}