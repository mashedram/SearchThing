using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Search;
using SearchThing.Search.CrateData;
using SearchThing.Search.Data;
using SearchThing.Search.Marrow;
using SearchThing.Search.Search;
using SearchThing.Search.Sorting;
using SearchThing.Util;

namespace SearchThing.History;

public class HistoryItemInfo : ITrackedDateItemInfo, IDescriptiveItemInfo, ICreatorItemInfo, ICrateTypeItemInfo, ICrateBoundItemInfo, ISearchEntry
{
    public MarrowCrate MarrowCrate { get; }
    public DateTime DateAdded { get; set; }

    public HistoryItemInfo(MarrowCrate marrowCrate, DateTime dateAdded)
    {
        MarrowCrate = marrowCrate;
        DateAdded = dateAdded;
    }

    public Guid Id => MarrowCrate.Id;
    public string Name => MarrowCrate.Name;
    public string PalletName => MarrowCrate.PalletName;
    public string Author => MarrowCrate.Author;
    public IEnumerable<string> Tags => MarrowCrate.Tags;
    public string Description => MarrowCrate.Description;
    public bool Redacted => MarrowCrate.Redacted;
    public CrateType CrateType => MarrowCrate.CrateType;
    public CrateSubType CrateSubType => MarrowCrate.CrateSubType;
    public IEnumerable<IFuzzySearchable> SearchFields => MarrowCrate.SearchFields;
    public int Salt => MarrowCrate.Salt;
    public Barcode Barcode => MarrowCrate.Barcode;
    public IRequiredItemInfo Crate => MarrowCrate;
}

public static class HistoryManager
{
    private static readonly BoundCrateList<HistoryItemInfo> BoundEntries = new();

    /// <summary>
    /// Add an entry to the circular history buffer. If the buffer is full, the oldest entry will be overwritten.
    /// </summary>
    public static void AddEntry(Crate crate)
    {
        BoundEntries.AddCrate(new HistoryItemInfo(new MarrowCrate(crate), DateTime.Now));
    }

    // No threading needed, searching is pretty fast just not on thousands of items, and we only have 100
    public static void SearchAsync(string query, ISearchOrder order, Func<HistoryItemInfo, bool> filter,
        Action<SearchResults<HistoryItemInfo>> callback)
    {
        SearchManager.SearchAsync(query, BoundEntries, filter, order, callback);
    }
}