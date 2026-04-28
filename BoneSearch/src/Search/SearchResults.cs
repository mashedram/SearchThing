using Il2CppSLZ.Marrow.Warehouse;

namespace BoneSearch.Search;

public class SearchResultEntry
{
    public Barcode Barcode;
    public int Score;

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
    
    public SearchResultEntry(Barcode barcode, int score)
    {
        Barcode = barcode;
        Score = score;
    }
}

public class SearchResults
{
    public static SearchResults Empty { get; } = new(new List<SearchResultEntry>());
    public IReadOnlyList<SearchResultEntry> Entries { get; }
    
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