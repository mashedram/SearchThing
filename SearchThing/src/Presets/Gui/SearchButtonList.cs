using SearchThing.Search.Data;
using SearchThing.Search.Search;

namespace SearchThing.Presets.Gui;

public class SearchButtonList : ISearchResults<ISearchableItemInfo>
{
    
    public IReadOnlyList<ISearchableItemInfo> Buttons { get; }
    
    public SearchButtonList(params ISearchableItemInfo[] buttons)
    {
        Buttons = buttons;
    }
    
    public IEnumerable<ISearchableItemInfo> GetPage(int page, int pageSize)
    {
        return Buttons;
    }
    
    public int GetPageCount(int pageSize)
    {
        return 1;
    }
}