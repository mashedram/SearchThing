using LabFusion.Data;
using SearchThing.Extensions.Sort;
using SearchThing.Search;

namespace SearchThing.Fusion;

public class IsBlockedFilter : ISelectableSearchOrder
{
    public string Name => "Blocked First";

    public int Order(ISearchableCrate searchableCrate)
    {
        var isBlocked = FusionBlacklistHelper.IsBlacklisted(searchableCrate.Barcode._id);

        if (isBlocked)
        {
            return 10000 + searchableCrate.Score;
        }
        
        return searchableCrate.Score;
    }
}