using LabFusion.Network;
using SearchThing.Extensions;
using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Extensions.Panel.Data;
using SearchThing.Extensions.Sort;
using SearchThing.Search;
using SearchThing.Util;
using UnityEngine;

namespace SearchThing.Fusion;

public class FusionBlocklistPage : BasicSearchPanel<MarrowCrate>
{
    private static readonly Sprite BlockIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.BlockIcon.png");
    
    public override string Tag => "Fusion Blocklist";
    public override bool CanAssign => false;
    public override Sprite ItemFunctionIcon => BlockIcon;

    public override bool HasItemFunction => true;

    public override Color? GetItemFunctionHighlight(SpawnablePanelExtension extension, IFullCrateData? crate)
    {
        if (crate is not IBarcodeHolder holder)
            return null;
        
        return FusionBlacklistHelper.IsBlacklisted(holder.Barcode._id) ? Color.red : Color.green;
    }

    public override void OnItemFunction(SpawnablePanelExtension extension, IFullCrateData crate)
    {
        if (crate is not IBarcodeHolder holder)
            return;
        
        FusionBlacklistHelper.ToggleBlacklist(holder.Barcode._id);
        
        MakeDirty();
        extension.RequestRefresh();
    }
    
    // Change search order
    public override ISelectableSearchOrder[] SupportedOrders { get; } = {
        new IsBlockedFilter(),
        new DateNewAddedSearchOrder(),
        new DateOldAddedSearchOrder(),
        new ScoreSearchOrder(),
        new AlphabeticalSearchOrder()
    };
    
    protected override void Search(string query, ISearchOrder order, Action<SearchResults<MarrowCrate>> callback)
    {
        SearchManager.SearchAsync(query, MarrowCrateManager.GetCrates(), c => c.CrateType == CrateType.Prop, order, callback);
    }
}