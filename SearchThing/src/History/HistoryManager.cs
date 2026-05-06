using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Search;
using SearchThing.Util;

namespace SearchThing.History;

public class HistoryCrateEntry : IHistorySearchableCrate
{
    public ISearchableCrate Crate { get; }
    public DateTime DateAdded { get; set; }
    
    public HistoryCrateEntry(ISearchableCrate crate, DateTime dateAdded)
    {
        Crate = crate;
        DateAdded = dateAdded;
    }

    public SearchTag Name => Crate.Name;
    public SearchTag PalletName => Crate.PalletName;
    public SearchTag Author => Crate.Author;
    public SearchTag[] Tags => Crate.Tags;
    public string Description => Crate.Description;
    public bool Redacted => Crate.Redacted;
    public CrateType CrateType => Crate.CrateType;
    public CrateSubType CrateSubType => Crate.CrateSubType;
    public int Salt => Crate.Salt;
    public int Score => 0;
    public Barcode Barcode => Crate.Barcode;
}


public static class HistoryManager
{
    private static readonly BoundCrateList<HistoryCrateEntry> BoundEntries = new();
    
    /// <summary>
    /// Add an entry to the circular history buffer. If the buffer is full, the oldest entry will be overwritten.
    /// </summary>
    public static void AddEntry(Crate crate)
    {
        BoundEntries.AddCrate(new HistoryCrateEntry(new SearchableCrate(crate), DateTime.Now));
    }

    // No threading needed, searching is pretty fast just not on thousands of items, and we only have 100
    public static void SearchAsync(string query, ISearchOrder order, Func<ISearchableCrate, bool> filter, Action<SearchResults<HistoryCrateEntry>> callback)
    {
        SearchManager.SearchAsync(query, BoundEntries, filter, order, callback);
    }
}