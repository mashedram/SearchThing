using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Player;
using SearchThing.History;
using SearchThing.Search;
using SearchThing.Util;

namespace SearchThing.Fusion;

public record FusionSpawnHistoryEntry(ISearchableCrate Crate, PlayerID SpawnerId) : IHistorySearchableCrate
{
    public SearchTag Name => Crate.Name;
    public SearchTag PalletName => Crate.PalletName;
    public SearchTag Author => Crate.Author;
    public SearchTag[] Tags => Crate.Tags;
    public string Description => Crate.Description;
    public bool Redacted => Crate.Redacted;
    public CrateType CrateType => Crate.CrateType;
    public CrateSubType CrateSubType => Crate.CrateSubType;
    public int Salt => Crate.Salt;
    public int Score => Crate.Score;
    public DateTime DateAdded { get; set; }
    public Barcode Barcode => Crate.Barcode;
}

public static class FusionSpawnHistory
{
    private static readonly BoundCrateList<FusionSpawnHistoryEntry> BoundEntries = new();
    
    public static void AddEntry(Crate crate, PlayerID spawnerId)
    {
        BoundEntries.AddCrate(new FusionSpawnHistoryEntry(new SearchableCrate(crate), spawnerId));
    }
    
    public static void SearchAsync(string query, ISearchOrder order, Func<ISearchableCrate, bool> filter, Action<SearchResults<FusionSpawnHistoryEntry>> callback)
    {
        SearchManager.SearchAsync(query, BoundEntries, filter, order, callback);
    }
}