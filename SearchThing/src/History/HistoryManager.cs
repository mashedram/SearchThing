using FuzzySharp;
using FuzzySharp.PreProcess;
using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Search;
using SearchThing.Util;

namespace SearchThing.History;

public struct HistoryEntry : ISearchableCrate
{
    public string SearchString { get; }
    public string PreprocessedString { get; }
    public CrateType CrateType { get; }
    // No score
    public int Score => 0;
    public DateTime DateAdded { get; }
    public Barcode Barcode { get; }
    
    public HistoryEntry(Crate crate)
    {
        SearchString = crate.GetSearchString();
        PreprocessedString = StringPreprocessorFactory.GetPreprocessor(PreprocessMode.Full)(SearchString);
        CrateType = crate.GetCrateType();
        Barcode = crate.Barcode;
        DateAdded = DateTime.Now;
    }
}

public record struct ScoredHistoryEntry(HistoryEntry Entry, int Score) : ISearchableCrate
{
    public string PreprocessedString => Entry.PreprocessedString;
    public CrateType CrateType => Entry.CrateType;
    public DateTime DateAdded => Entry.DateAdded;
    public Barcode Barcode => Entry.Barcode;
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
        HistoryEntries[entry.DateAdded] = entry;
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
        return HistoryEntries.Values;
    }

    // No threading needed, searching is pretty fast just not on thousands of items, and we only have 100
    public static SearchResults Search(string query, ISearchOrder order, Func<HistoryEntry, bool>? filter = null)
    {
        if (string.IsNullOrWhiteSpace(query))
            return GetEntries()
                .Where(filter ?? (_ => true))
                .OrderByDescending(entry => order.Score(entry))
                .ThenByDescending(entry => entry.DateAdded) // Tie-breaker: more recent entries first
                .Select(c => c.Barcode)
                .ToSearchResults();
        
        var lowerQuery = query.ToLowerInvariant();
        var preprocessedQuery = StringPreprocessorFactory.GetPreprocessor(PreprocessMode.Full)(lowerQuery);
        
        return GetEntries()
            .Where(filter ?? (_ => true))
            .Select(entry => new ScoredHistoryEntry(entry, SearchManager.ScoreCrate(preprocessedQuery, entry.PreprocessedString)))
            .Where(entry => entry.Score >= 80)
            .OrderByDescending(entry => order.Score(entry))
            .ThenByDescending(entry => entry.DateAdded) // Tie-breaker: more recent entries first
            .Select(c => c.Barcode)
            .ToSearchResults();
    }
}