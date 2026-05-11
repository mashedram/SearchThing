using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Safety;
using SearchThing.Extensions;
using SearchThing.Extensions.Components;
using SearchThing.Extensions.Components.Info;
using SearchThing.Extensions.Pages;
using SearchThing.Extensions.Panel;
using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Extensions.Panel.Data;
using SearchThing.Extensions.Sort;
using SearchThing.Search;
using SearchThing.Search.CrateData;
using SearchThing.Search.Data;
using SearchThing.Search.Search;
using SearchThing.Search.Sorting;
using SearchThing.Util;
using UnityEngine;

namespace SearchThing.Fusion;

public class FusionSpawnHistoryPage : BasicSearchPanel<FusionSpawnHistoryEntry>
{
    private static readonly Sprite BlockIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.BlockIcon.png");

    public override string Name => "Fusion Spawn History";
    public override bool Redacted => !NetworkInfo.HasServer;
    public override bool CanSelect => false;

    public override bool ResearchOnPageChange => true;
    
    public Color? GetItemFunctionHighlight(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo)
    {
        if (itemInfo is not ICrateBoundItemInfo { Barcode: var barcode })
            return null;

        return FusionBlacklistHelper.IsBlacklisted(barcode._id) ? Color.red : Color.green;
    }

    public void OnItemFunction(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo)
    {
        if (itemInfo is not ICrateBoundItemInfo { Barcode: var barcode })
            return;

        FusionBlacklistHelper.ToggleBlacklist(barcode._id);

        extension.RequestRefresh();
    }

    // Change search order
    public override ISelectableSearchOrder[] SupportedOrders { get; } =
    {
        new DateNewAddedSearchOrder(),
        new DateOldAddedSearchOrder(),
        new ScoreSearchOrder(),
        new AlphabeticalSearchOrder(),
        new RandomSearchOrder()
    };

    public override ItemRender GetRenderDataForCrate(FusionSpawnHistoryEntry crate)
    {
        var ownerName = crate.SpawnerId?.IsValid == true && crate.SpawnerId.TryGetDisplayName(out var name)
            ? StringHelper.RemoveUnityRichText(name)
            : "Unknown";

        return new ItemRenderWithAction(crate, OnItemFunction)
        {
            Name = $"{crate.Name} ({ownerName})",
            GetActionIconFunc = (_, _) => BlockIcon,
            GetActionHighlightFunc = GetItemFunctionHighlight
        };
    }

    protected override void Search(string query, ISearchOrder order, Action<ISearchResults<FusionSpawnHistoryEntry>> callback)
    {
        FusionSpawnHistory.SearchAsync(query, order, c => c.CrateType == CrateType.Prop, callback);
    }
}