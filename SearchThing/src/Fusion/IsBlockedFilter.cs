using LabFusion.Data;
using SearchThing.Extensions.Sort;
using SearchThing.Search;
using SearchThing.Search.CrateData;
using SearchThing.Search.Data;
using SearchThing.Search.Sorting;

namespace SearchThing.Fusion;

public class IsBlockedFilter : ISelectableSearchOrder
{
    public string Name => "Blocked First";

    public int Order(ISearchOrderable orderable)
    {
        if (orderable.Source is not ICrateBoundItemInfo barcodeHolder)
            return 0;

        var isBlocked = FusionBlacklistHelper.IsBlacklisted(barcodeHolder.Barcode._id);

        if (isBlocked)
        {
            return 10000 + orderable.Score;
        }

        return orderable.Score;
    }
}