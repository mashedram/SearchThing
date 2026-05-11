using HarmonyLib;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Data;
using LabFusion.Player;
using MelonLoader;
using SearchThing.History;
using SearchThing.Search;
using SearchThing.Search.CrateData;
using SearchThing.Search.Data;
using SearchThing.Search.Marrow;
using SearchThing.Search.Search;
using SearchThing.Search.Sorting;
using SearchThing.Util;

namespace SearchThing.Fusion;

public record FusionSpawnHistoryEntry(Barcode Barcode, PlayerID? SpawnerId) : ITrackedDateItemInfo, IDescriptiveItemInfo, ICreatorItemInfo, ICrateTypeItemInfo, ICrateBoundItemInfo, ISearchEntry
{
    public MarrowCrate? MarrowCrate { get; } = MarrowCrateManager.GetCrate(Barcode);


    public Guid Id => MarrowCrate?.Id ?? Guid.Empty;
    public string Name => MarrowCrate?.Name ?? "Unknown Crate";
    public string PalletName => MarrowCrate?.PalletName ?? "Unknown Pallet";
    public string Author => MarrowCrate?.Author ?? "Unknown Author";
    public IEnumerable<string> Tags => MarrowCrate?.Tags ?? Array.Empty<string>();
    public string Description => MarrowCrate?.Description ?? "No description available.";
    public bool Redacted => MarrowCrate?.Redacted ?? false;
    public CrateType CrateType => MarrowCrate?.CrateType ?? CrateType.Invalid;
    public CrateSubType CrateSubType => MarrowCrate?.CrateSubType ?? CrateSubType.None;
    public int Salt => MarrowCrate?.Salt ?? 0;
    public DateTime DateAdded { get; set; }
    public IEnumerable<IFuzzySearchable> SearchFields => MarrowCrate?.SearchFields ?? Array.Empty<IFuzzySearchable>();
    public IRequiredItemInfo? Crate => MarrowCrate ?? null;
}

public static class FusionSpawnHistory
{
    private static readonly BoundCrateList<FusionSpawnHistoryEntry> BoundEntries = new();

    public static void AddEntry(string barcode, byte? smallPlayerId)
    {
        var spawnerId = smallPlayerId.HasValue ? PlayerIDManager.GetPlayerID(smallPlayerId.Value) : null;

        var barcodeObj = new Barcode(barcode);

        BoundEntries.AddCrate(new FusionSpawnHistoryEntry(barcodeObj, spawnerId));
    }

    public static void SearchAsync(string query, ISearchOrder order, Func<FusionSpawnHistoryEntry, bool> filter,
        Action<SearchResults<FusionSpawnHistoryEntry>> callback)
    {
        SearchManager.SearchAsync(query, BoundEntries, filter, order, callback);
    }
}