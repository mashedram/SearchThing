using LabFusion.Data;
using SearchThing.Extensions.Sort;
using SearchThing.Search;

namespace SearchThing.Fusion;

public class IsBlockedFilter : ISelectableSearchOrder
{
    public string Name => "Blocked First";

    public int Order(ISearchOrderable orderable)
    {
        if (orderable.Source is not IBarcodeHolder barcodeHolder)
            return 0;
        
        var isBlocked = FusionBlacklistHelper.IsBlacklisted(barcodeHolder.Barcode._id);

        if (isBlocked)
        {
            return 10000 + orderable.Score;
        }
        
        return orderable.Score;
    }
}