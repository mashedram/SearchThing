using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Network;
using SearchThing.Extensions;
using SearchThing.Extensions.Pages;
using SearchThing.Extensions.Panel;
using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Extensions.Panel.Data;
using SearchThing.Extensions.Sort;
using SearchThing.Search;
using SearchThing.Util;
using UnityEngine;

namespace SearchThing.Fusion;

public class FusionSpawnHistoryPage : BasicSearchPanel<FusionSpawnHistoryEntry>
{
    public override string Tag { get; } = "Fusion Spawn History";

    public override ItemRenderData GetRenderDataForCrate(FusionSpawnHistoryEntry crate)
    {
        var ownerName = crate.SpawnerId.TryGetDisplayName(out var name) ? name : "Unknown";
        
        return new ItemRenderData
        {
            Name = $"{crate.Name.Original} ({ownerName})",
            Description = crate.Description,
            Author = crate.Author.Original,
            PalletName = crate.PalletName.Original,
            Tags = crate.Tags.Select(t => t.Original).ToArray(),
            Icon = CrateIconProvider.GetIcon(crate)
        };
    }

    protected override void Search(string query, ISearchOrder order, Action<SearchResults<FusionSpawnHistoryEntry>> callback)
    {
        FusionSpawnHistory.SearchAsync(query, order, _ => true, callback);
    }
}