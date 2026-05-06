using HarmonyLib;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Data;
using LabFusion.Player;
using MelonLoader;
using SearchThing.History;
using SearchThing.Search;
using SearchThing.Util;

namespace SearchThing.Fusion;

public record FusionSpawnHistoryEntry(Barcode Barcode, PlayerID? SpawnerId) : IHistorySearchableCrate
{
    private static readonly SearchTag UnknownCrateTag = new SearchTag("Unknown Crate");
    public ISearchableCrate? Crate { get; } = GlobalCrateManager.GetCrate(Barcode); 
    
    public SearchTag Name => Crate?.Name ?? UnknownCrateTag;
    public SearchTag PalletName => Crate?.PalletName ?? UnknownCrateTag;
    public SearchTag Author => Crate?.Author ?? UnknownCrateTag;
    public SearchTag[] Tags => Crate?.Tags ?? Array.Empty<SearchTag>();
    public string Description => Crate?.Description ?? "No description available.";
    public bool Redacted => Crate?.Redacted ?? false;
    public CrateType CrateType => Crate?.CrateType ?? CrateType.Invalid;
    public CrateSubType CrateSubType => Crate?.CrateSubType ?? CrateSubType.None;
    public int Salt => Crate?.Salt ?? 0;
    public int Score => Crate?.Score ?? 0;
    public DateTime DateAdded { get; set; }
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
    
    public static void SearchAsync(string query, ISearchOrder order, Func<ISearchableCrate, bool> filter, Action<SearchResults<FusionSpawnHistoryEntry>> callback)
    {
        SearchManager.SearchAsync(query, BoundEntries, filter, order, callback);
    }
}