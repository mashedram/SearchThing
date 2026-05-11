using SearchThing.Search.CrateData;

namespace SearchThing.Search.Search;

public class SearchResults<TCrate> : ISearchResults<TCrate>
    where TCrate : class, ISearchEntry
{
    public static SearchResults<ISearchEntry> Empty { get; } = new(new List<ISearchEntry>());
    private IReadOnlyList<TCrate> Entries { get; }
    public int Count => Entries.Count;

    public SearchResults(List<TCrate> entries)
    {
        Entries = entries;
    }

    private IEnumerable<TCrate> GetPageIterator(int start, int end)
    {
        for (var i = start; i < end; i++)
            yield return Entries[i];
    }

    public IEnumerable<TCrate> GetPage(int page, int pageSize)
    {
        var start = page * pageSize;
        var end = Math.Min(start + pageSize, Entries.Count);

        if (start >= Entries.Count)
            return Array.Empty<TCrate>();

        return GetPageIterator(start, end);
    }

    public TCrate? GetEntryAt(int index)
    {
        if (index < 0 || index >= Entries.Count)
            return null;

        return Entries[index];
    }

    public TCrate? GetEntryAt(int page, int pageSize, int index)
    {
        var globalIndex = page * pageSize + index;
        return GetEntryAt(globalIndex);
    }

    public int GetPageCount(int pageSize)
    {
        return (Entries.Count + pageSize - 1) / pageSize;
    }
}