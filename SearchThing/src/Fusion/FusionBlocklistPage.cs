using LabFusion.Network;
using SearchThing.Extensions;
using SearchThing.Extensions.Components;
using SearchThing.Extensions.Components.Info;
using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Extensions.Panel.Data;
using SearchThing.Extensions.Sort;
using SearchThing.Search;
using SearchThing.Search.CrateData;
using SearchThing.Search.Data;
using SearchThing.Search.Marrow;
using SearchThing.Search.Search;
using SearchThing.Search.Sorting;
using SearchThing.Util;
using UnityEngine;

namespace SearchThing.Fusion;

public class FusionBlocklistPage : BasicSearchPanel<MarrowCrate>
{
    private static readonly Sprite BlockIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.BlockIcon.png");

    public override string Name => "Fusion Blocklist";
    public override bool CanSelect => false;

    public Color? GetItemFunctionHighlight(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo)
    {
        if (itemInfo is not ICrateBoundItemInfo { Barcode: var barcode })
            return null;

        if (barcode == null)
            return null;

        return FusionBlacklistHelper.IsBlacklisted(barcode._id) ? Color.red : Color.green;
    }

    private void OnItemFunction(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo)
    {
        if (itemInfo is not ICrateBoundItemInfo { Barcode: var barcode })
            return;

        FusionBlacklistHelper.ToggleBlacklist(barcode._id);

        MakeDirty();
        extension.RequestRefresh();
    }

    // Change search order
    public override ISelectableSearchOrder[] SupportedOrders { get; } =
    {
        new IsBlockedFilter(),
        new DateNewAddedSearchOrder(),
        new DateOldAddedSearchOrder(),
        new ScoreSearchOrder(),
        new AlphabeticalSearchOrder()
    };

    public override ItemRender GetRenderDataForCrate(MarrowCrate crate)
    {
        return new ItemRenderWithAction(crate, OnItemFunction)
        {
            GetActionIconFunc = (_, _) => BlockIcon,
            GetActionHighlightFunc = GetItemFunctionHighlight
        };
    }

    protected override void Search(string query, ISearchOrder order, Action<ISearchResults<MarrowCrate>> callback)
    {
        SearchManager.SearchAsync(query, MarrowCrateManager.GetCrates(), c => c.CrateType == CrateType.Prop, order, callback);
    }
}