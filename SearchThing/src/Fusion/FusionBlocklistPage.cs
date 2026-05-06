using LabFusion.Network;
using SearchThing.Extensions;
using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Extensions.Panel.Data;
using SearchThing.Extensions.Sort;
using SearchThing.Search;
using SearchThing.Util;
using UnityEngine;

namespace SearchThing.Fusion;

public class FusionBlocklistPage : BasicSearchPanel<ISearchableCrate>
{
    private static readonly Sprite BlockIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.BlockIcon.png");
    
    public override string Tag => "Fusion Blocklist";
    public override bool CanAssign => false;
    public override Sprite ItemFunctionIcon => BlockIcon;

    public override bool HasItemFunction => true;

    public override Color? GetItemFunctionHighlight(SpawnablePanelExtension extension, ISearchableCrate? crate)
    {
        return crate != null && FusionBlacklistHelper.IsBlacklisted(crate.Barcode._id) ? Color.red : Color.green;
    }

    public override void OnItemFunction(SpawnablePanelExtension extension, ISearchableCrate crate)
    {
        FusionBlacklistHelper.ToggleBlacklist(crate.Barcode._id);
        
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
    
    protected override void Search(string query, ISearchOrder order, Action<SearchResults<ISearchableCrate>> callback)
    {
        SearchManager.SearchAsync(query, GlobalCrateManager.GetCrates(), c => c.CrateType == CrateType.Prop, order, callback);
    }
}