using SearchThing.Extensions.Pages;
using SearchThing.Extensions.Panel.Filter;
using SearchThing.Extensions.Panel.History;
using SearchThing.Presets;

namespace SearchThing.Extensions;

public class SpawnablePageProvider
{
    private readonly ISearchPage _basePage = new BasicSearchPage(
        new PropTagSearchPanel(), 
        new AvatarTagSearchPanel(), 
        new PropHistorySearchPanel(),
        new AvatarHistorySearchPanel(),
        new LevelTagSearchPanel(),
#if UNLOCKED
        new RedactedSearchPanel()
#endif
    );
    
    public int PageCount => 1 + PresetManager.GetPages().Count;
    
    public ISearchPage GetBasePage() => _basePage;
    
    public ISearchPage GetPage(int index)
    {
        if (index == 0)
            return _basePage;
        
        return PresetManager.GetPages()[index - 1];
    }
    
    public bool IsPresetPage(int index)
    {
        return index > 0;
    }
}