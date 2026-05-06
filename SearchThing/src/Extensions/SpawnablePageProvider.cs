using SearchThing.Extensions.Pages;
using SearchThing.Extensions.Panel.Filter;
using SearchThing.Extensions.Panel.History;
using SearchThing.Fusion;
using SearchThing.Presets;

namespace SearchThing.Extensions;

public class SpawnablePageProvider
{
    private readonly ISearchPage[] _basePages =
    {
        new BasicSearchPage(
            new PropTagSearchPanel(),
            new AvatarTagSearchPanel(),
            new PropHistorySearchPanel(),
            new AvatarHistorySearchPanel(),
            new LevelTagSearchPanel()
#if UNLOCKED
            ,new RedactedSearchPanel()
#endif
        ),
        new FusionPage()
    };
    
    public IEnumerable<ISearchPage> GetAllPages() => _basePages.Concat(PresetManager.GetPages());
    public ISearchPage[] GetVisiblePages() => GetAllPages().Where(p => p.IsVisible).ToArray();
    public int PageCount => GetAllPages().Count(c => c.IsVisible);
    
    public ISearchPage GetBasePage() => _basePages[0];
    
    public ISearchPage GetPage(int index)
    {
        var visiblePages = GetVisiblePages();
        var clampedIndex = Math.Clamp(index, 0, visiblePages.Length - 1);
        return visiblePages[clampedIndex];
    }
}