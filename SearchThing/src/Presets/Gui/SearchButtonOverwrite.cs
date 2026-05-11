using SearchThing.Search.Data;
using SearchThing.Search.Search;

namespace SearchThing.Presets.Gui;

public class SearchButtonOverwrite<TCrate> : ISearchResults<ISearchableItemInfo> where TCrate : class, ISearchableItemInfo
{
    private ISearchResults<TCrate> Parent { get; }
    private readonly Dictionary<int, ISearchableItemInfo> _overwriteEntries = new();

    public SearchButtonOverwrite(ISearchResults<TCrate> parent, params (int key, ISearchableItemInfo item)[] overwrites)
    {
        Parent = parent;
        foreach (var overwrite in overwrites)
        {
            _overwriteEntries[overwrite.key] = overwrite.item;
        }
    }

    private int GetOverwritesOnPage(int pageSize)
    {
        return _overwriteEntries.Count(e => e.Key < pageSize);
    }

    public IEnumerable<ISearchableItemInfo> GetPage(int page, int pageSize)
    {
        var actualPageSize = pageSize - GetOverwritesOnPage(pageSize);
        var parentEntries = Parent.GetPage(page, actualPageSize).ToList();

        var overwritenEntries = 0;
        for (var i = 0; i < pageSize; i++)
        {
            if (_overwriteEntries.TryGetValue(i, out var overwrite))
            {
                yield return overwrite;
                overwritenEntries++;
            }
            else if (i - overwritenEntries < parentEntries.Count)
            {
                yield return parentEntries[i - overwritenEntries];
            }
        }
    }

    public int GetPageCount(int pageSize)
    {
        var actualPageSize = pageSize - GetOverwritesOnPage(pageSize);
        return Parent.GetPageCount(actualPageSize);
    }
}