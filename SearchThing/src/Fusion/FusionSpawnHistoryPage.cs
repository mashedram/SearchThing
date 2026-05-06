using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Safety;
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
    private static readonly Sprite BlockIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.BlockIcon.png");
    
    public override string Tag => "Fusion Spawn History";
    public override bool IsVisible => NetworkInfo.HasServer;
    public override bool CanAssign => false;

    public override bool ResearchOnPageChange => true;
    public override Sprite ItemFunctionIcon => BlockIcon;

    public override bool HasItemFunction => true;

    public override Color? GetItemFunctionHighlight(SpawnablePanelExtension extension, ISearchableCrate? crate)
    {
        return crate != null && FusionBlacklistHelper.IsBlacklisted(crate.Barcode._id) ? Color.red : Color.green;
    }

    public override void OnItemFunction(SpawnablePanelExtension extension, ISearchableCrate crate)
    {
        FusionBlacklistHelper.ToggleBlacklist(crate.Barcode._id);
        
        extension.RequestRefresh();
    }
    
    // Change search order
    public override ISelectableSearchOrder[] SupportedOrders { get; } = {
        new DateNewAddedSearchOrder(),
        new DateOldAddedSearchOrder(),
        new ScoreSearchOrder(),
        new AlphabeticalSearchOrder(),
        new RandomSearchOrder()
    };

    public override ItemRenderData GetRenderDataForCrate(FusionSpawnHistoryEntry crate)
    {
        var ownerName = crate.SpawnerId?.IsValid == true && crate.SpawnerId.TryGetDisplayName(out var name) 
            ? StringHelper.RemoveUnityRichText(name) 
            : "Unknown";
        
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
        FusionSpawnHistory.SearchAsync(query, order, c => c.CrateType == CrateType.Prop, callback);
    }
}