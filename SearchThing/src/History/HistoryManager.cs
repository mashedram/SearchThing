using FuzzySharp;
using FuzzySharp.PreProcess;
using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Search;
using SearchThing.Util;

namespace SearchThing.History;

public struct HistoryEntry
{
    public string SearchString { get; }
    public string PreprocessedString { get; }
    public DateTime Timestamp { get; }
    public CrateType CrateType { get; }
    public Barcode Barcode { get; }
    
    public HistoryEntry(Crate crate)
    {
        SearchString = crate.GetSearchString();
        PreprocessedString = StringPreprocessorFactory.GetPreprocessor(PreprocessMode.Full)(SearchString);
        CrateType = crate.GetCrateType();
        Barcode = crate.Barcode;
        Timestamp = DateTime.Now;
    }
}

public static class HistoryManager
{
    private const int MaxHistoryEntries = 100;
    
    private static readonly HashSet<string> RecentBarcodes = new();
    private static readonly SortedDictionary<DateTime, HistoryEntry> HistoryEntries = new();
    
    private static void RemoveOldEntry()
    {
        if (HistoryEntries.Count == 0)
            return;
        
        var oldestEntry = HistoryEntries.Keys.First();
        RecentBarcodes.Remove(HistoryEntries[oldestEntry].Barcode._id);
        HistoryEntries.Remove(oldestEntry);
    }
    
    /// <summary>
    /// Add an entry to the circular history buffer. If the buffer is full, the oldest entry will be overwritten.
    /// </summary>
    public static void AddEntry(Crate crate)
    {
        if (RecentBarcodes.Contains(crate.Barcode._id))
            return;
        
        var entry = new HistoryEntry(crate);
        HistoryEntries[entry.Timestamp] = entry;
        RecentBarcodes.Add(entry.Barcode._id);
        
        // If we have more than the max entries, remove the oldest one
        if (HistoryEntries.Count > MaxHistoryEntries)
        {
            RemoveOldEntry();
        }
    }
    
    private static IEnumerable<HistoryEntry> GetEntries()
    {
        // Get the most recent 100 entries
        return HistoryEntries.Values.OrderByDescending(entry => entry.Timestamp).Take(MaxHistoryEntries);
    }

    // No threading needed, searching is pretty fast just not on thousands of items, and we only have 100
    public static SearchResults Search(string query, Func<HistoryEntry, bool>? filter = null)
    {
        if (string.IsNullOrWhiteSpace(query))
            return GetEntries()
                .Where(filter ?? (_ => true))
                .Select(c => c.Barcode)
                .ToSearchResults();
        
        var lowerQuery = query.ToLowerInvariant();
        var preprocessedQuery = StringPreprocessorFactory.GetPreprocessor(PreprocessMode.Full)(lowerQuery);
        
        return GetEntries()
            .Where(filter ?? (_ => true))
            .Where(entry => SearchManager.ScoreCrate(preprocessedQuery, entry.PreprocessedString) >= 80)
            .Select(c => c.Barcode)
            .ToSearchResults();
    }
}