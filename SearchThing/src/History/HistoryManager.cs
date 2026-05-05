using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Search;

namespace SearchThing.History;


public static class HistoryManager
{
    private static readonly HistoryCrateList HistoryEntries = new();
    
    /// <summary>
    /// Add an entry to the circular history buffer. If the buffer is full, the oldest entry will be overwritten.
    /// </summary>
    public static void AddEntry(Crate crate)
    {
        HistoryEntries.AddCrate(new SearchableCrate(crate));
    }

    // No threading needed, searching is pretty fast just not on thousands of items, and we only have 100
    public static void SearchAsync(string query, ISearchOrder order, Func<ISearchableCrate, bool> filter, Action<SearchResults> callback)
    {
        SearchManager.SearchAsync(query, HistoryEntries, filter, order, callback);
    }
}