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
            new LevelTagSearchPanel(),
            new PresetPanel()
        ),
        new FusionPage(),
#if UNLOCKED
        new BasicSearchPage(
            new RedactedSearchPanel()
        )
#endif
    };

    public ISearchPage[] GetVisiblePages()
    {
        return _basePages.Where(p => p.IsVisible).ToArray();
    }

    public int PageCount => _basePages.Count(c => c.IsVisible);

    public ISearchPage GetBasePage()
    {
        return _basePages[0];
    }

    public ISearchPage? GetPage(int index)
    {
        var visiblePages = GetVisiblePages();
        if (visiblePages.Length == 0)
            return null;
        if (index < 0 || index >= visiblePages.Length)
            return null;

        return visiblePages[index];
    }
}