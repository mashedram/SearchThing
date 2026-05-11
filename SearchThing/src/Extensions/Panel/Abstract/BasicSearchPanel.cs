using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Extensions.Components;
using SearchThing.Extensions.Panel.Data;
using SearchThing.Extensions.Sort;
using SearchThing.Search;
using SearchThing.Search.CrateData;
using SearchThing.Search.Data;
using SearchThing.Search.Search;
using SearchThing.Search.Sorting;
using SearchThing.Util;
using UnityEngine;

namespace SearchThing.Extensions.Panel.Abstract;

public abstract class BasicSearchPanel<TCrate> : ISearchPanel
    where TCrate : class, IRequiredItemInfo, ISearchEntry
{
    private bool _isDirty = true;
    private SearchResults<TCrate>? _results;

    public virtual bool ResearchOnPageChange => false;
    public abstract string Name { get; }
    public virtual bool Redacted => false;
    public DateTime DateAdded => DateTime.MinValue;
    public virtual bool CanSelect => true;
    public virtual bool TagEditable => false;
    protected abstract void Search(string query, ISearchOrder order, Action<SearchResults<TCrate>> callback);

    public Guid Id { get; } = Guid.NewGuid();

    private string _query = "";

    public string Query
    {
        get => _query;
        set
        {
            if (value == _query)
                return;

            _isDirty = true;
            _query = value;
        }
    }

    private int _selectedOrderIndex;

    public int SelectedOrderIndex
    {
        get => _selectedOrderIndex;
        set
        {
            if (value == _selectedOrderIndex)
                return;

            if (value < 0 || value >= SupportedOrders.Length)
                return;

            _isDirty = true;
            _selectedOrderIndex = value;
        }
    }

    public virtual ISelectableSearchOrder[] SupportedOrders { get; } =
    {
        new ScoreSearchOrder(),
        new AlphabeticalSearchOrder(),
        new DateNewAddedSearchOrder(),
        new DateOldAddedSearchOrder(),
        new RandomSearchOrder()
    };

    public void RequestSearch(SpawnablePanelExtension extension)
    {
        // Prevent reloading the same query
        if (!ResearchOnPageChange && !_isDirty)
        {
            // We just have to rerender now, since the query is the same but the page might be different
            extension.RenderAll();
            return;
        }
        _isDirty = false;

        var order = SupportedOrders[SelectedOrderIndex];
        Search(_query, order, results =>
        {
            _results = results;
            Page = 0;
            PageCount = results.GetPageCount(ISearchPanel.PanelSize);
            extension.RenderAll();
        });
    }

    public int Page { get; set; }
    public int PageCount { get; private set; } = 1;

    public void ChangePage(SpawnablePanelExtension extension, int offset)
    {
        var newPage = Page + offset;
        if (newPage < 0 || newPage >= PageCount)
            return;

        Page = newPage;
        extension.RenderAll();
    }

    public void MakeDirty()
    {
        _isDirty = true;
    }

    public virtual void OnTagEdited(SpawnablePanelExtension extension, string newTag)
    {
        // No special logic needed
    }

    public virtual Color? IsForceHighlighted(SpawnablePanelExtension extension)
    {
        // No special logic needed
        return null;
    }

    public virtual bool OnSelected(SpawnablePanelExtension extension)
    {
        // No special logic needed
        return true;
    }

    public virtual ItemRender GetRenderDataForCrate(TCrate crate)
    {
        return new ItemRender(crate);
    }

    public IRequiredItemInfo? GetCrateAt(int index)
    {
        return _results?.GetEntryAt(Page, ISearchPanel.PanelSize, index);
    }

    public IReadOnlyList<ItemRender> GetPage(int page)
    {
        if (_results == null)
            return Array.Empty<ItemRender>();

        return _results
            .GetPage(page, ISearchPanel.PanelSize)
            .Select(GetRenderDataForCrate)
            .ToList();
    }
}