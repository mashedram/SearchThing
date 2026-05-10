using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Search;
using SearchThing.Util;

namespace SearchThing.History;

public class HistoryFullCrateDataEntry : IHistorySearchableCrate, IFullCrateData, IBarcodeHolder
{
    public MarrowCrate Crate { get; }
    public DateTime DateAdded { get; set; }
    
    public HistoryFullCrateDataEntry(MarrowCrate crate, DateTime dateAdded)
    {
        Crate = crate;
        DateAdded = dateAdded;
    }

    public string Name => Crate.Name;
    public string PalletName => Crate.PalletName;
    public string Author => Crate.Author;
    public IEnumerable<string> Tags => Crate.Tags;
    public string Description => Crate.Description;
    public bool Redacted => Crate.Redacted;
    public CrateType CrateType => Crate.CrateType;
    public CrateSubType CrateSubType => Crate.CrateSubType;
    public IEnumerable<IFuzzySearchable> SearchFields => Crate.SearchFields;
    public int Salt => Crate.Salt;
    public Barcode Barcode => Crate.Barcode;
}


public static class HistoryManager
{
    private static readonly BoundCrateList<HistoryFullCrateDataEntry> BoundEntries = new();
    
    /// <summary>
    /// Add an entry to the circular history buffer. If the buffer is full, the oldest entry will be overwritten.
    /// </summary>
    public static void AddEntry(Crate crate)
    {
        BoundEntries.AddCrate(new HistoryFullCrateDataEntry(new MarrowCrate(crate), DateTime.Now));
    }

    // No threading needed, searching is pretty fast just not on thousands of items, and we only have 100
    public static void SearchAsync(string query, ISearchOrder order, Func<HistoryFullCrateDataEntry, bool> filter, Action<SearchResults<HistoryFullCrateDataEntry>> callback)
    {
        SearchManager.SearchAsync(query, BoundEntries, filter, order, callback);
    }
}