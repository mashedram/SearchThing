using SearchThing.Extensions.Pages;
using SearchThing.Extensions.Panel;

namespace SearchThing.Fusion;

public class FusionPage : ISearchPage
{

    public bool IsVisible => Mod.IsFusionLoaded;
    public IReadOnlyList<ISearchPanel> Panels { get; } = new ISearchPanel[]
    {
        new FusionBlocklistPage(),
        new FusionSpawnHistoryPage()
    };
}