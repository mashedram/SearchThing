using FuzzySharp;
using FuzzySharp.PreProcess;
using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Search;
using SearchThing.Util;

namespace SearchThing.History;

public class HistoryEntry : ISearchableCrate
{
    public SearchTag Name { get; }
    public SearchTag PalletName { get; }
    public SearchTag Author { get; }
    public SearchTag[] Tags { get; }
    public CrateType CrateType { get; }
    // No score
    public int Score => 0;
    // Random Salt
    public int Salt => Random.Shared.Next();
    public DateTime DateAdded { get; }
    public Barcode Barcode { get; }
    
    public HistoryEntry(Crate crate)
    {
        Name = new SearchTag(crate.name);
        PalletName = new SearchTag(crate._pallet.name);
        Author = new SearchTag(crate._pallet._author);
        Tags = crate._tags.ToArray().Select(t => new SearchTag(t)).ToArray();
        
        CrateType = crate.GetCrateType();
        Barcode = crate.Barcode;
        DateAdded = DateTime.Now;
    }
}

public record ScoredHistoryEntry(HistoryEntry Entry, int Score) : ISearchableCrate
{
    public SearchTag Name => Entry.Name;
    public SearchTag PalletName => Entry.PalletName;
    public SearchTag Author => Entry.Author;
    public SearchTag[] Tags => Entry.Tags;
    public CrateType CrateType => Entry.CrateType;
    public int Salt => Entry.Salt;
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
                .OrderByDescending(order.Score)
                .ThenByDescending(entry => entry.DateAdded) // Tie-breaker: more recent entries first
                .ToSearchResults();
        
        var lowerQuery = query.ToLowerInvariant();
        var preprocessedQuery = StringPreprocessorFactory.GetPreprocessor(PreprocessMode.Full)(lowerQuery);
        
        return GetEntries()
            .Where(filter ?? (_ => true))
            .Select(entry => new ScoredHistoryEntry(entry, SearchManager.ScoreCrate(preprocessedQuery, entry)))
            .Where(entry => entry.Score >= 80)
            .OrderByDescending(order.Score)
            .ThenByDescending(entry => entry.DateAdded) // Tie-breaker: more recent entries first
            .ToSearchResults();
    }
}