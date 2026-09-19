using SearchThing.Extensions;
using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Extensions.Panel.Data;
using SearchThing.Extensions.Panel.Data.Extensions;
using SearchThing.Extensions.Sort;
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
    public override string Description => "Select items to mark them as unspawnable in your fusion lobbies";
    public override bool CanSelect => false;

    private Color? GetItemFunctionHighlight(SpawnablePanelExtension extension, IItemRenderInfo itemInfo)
    {     
        if (itemInfo.GetSource() is not ICrateBoundItemInfo { Barcode: var barcode })
            return null;

        return FusionBlacklistHelper.IsBlacklisted(barcode._id) ? Color.red : Color.green;
    }

    private void OnItemFunction(SpawnablePanelExtension extension, IItemRenderInfo itemInfo)
    {
        if (itemInfo.GetSource() is not ICrateBoundItemInfo { Barcode: var barcode })
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

    public override ItemRenderInfo GetRenderDataForCrate(MarrowCrate crate)
    {
        return new ItemRenderInfo(crate, new IconExtension(crate), new ActionExtension(OnItemFunction)
        {
            GetActionIconFunc = (_, _) => BlockIcon,
            GetActionHighlightFunc = GetItemFunctionHighlight
        }, new LabelColorExtension(FusionBlacklistHelper.IsBlacklisted(crate.Barcode._id) ? Color.red : Color.green));
    }

    protected override void Search(string query, ISearchOrder order, Action<ISearchResults<MarrowCrate>> callback)
    {
        SearchManager.SearchAsync(query, MarrowCrateManager.GetCrates(), c => c.CrateType == CrateType.Prop, order, callback);
    }
}