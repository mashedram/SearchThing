using SearchThing.Extensions.Panel.Data;
using SearchThing.Extensions.Sort;
using SearchThing.Search.Data;
using UnityEngine;

namespace SearchThing.Extensions.Panel;

public interface ISearchPanel : IDescriptiveItemInfo
{
    public const int PANEL_SIZE = 12;
    
    bool TagEditable { get; }
    string Query { get; set; }
    int SelectedOrderIndex { get; set; }
    ISelectableSearchOrder[] SupportedOrders { get; }
    int Page { get; set; }
    int PageCount { get; }
    bool IsDirty { get; }

    /// <summary>
    /// Gets called for every crate that is rendered, return true to force the crate to be highlighted in the menu
    /// </summary>
    /// <returns></returns>
    Color? IsForceHighlighted(SpawnablePanelExtension extension);
    /// <summary>
    /// Called when the panel should perform a search
    /// </summary>
    /// <remarks>It's the panel's responsibility to call extension.Rerender() when the search is done</remarks>
    /// <param name="extension"></param>
    void RequestSearch(SpawnablePanelExtension extension);
    /// <summary>
    /// Called when the pag should change
    /// </summary>
    /// <param name="extension"></param>
    /// <param name="offset"></param>
    void ChangePage(SpawnablePanelExtension extension, int offset);
    /// <summary>
    /// Called when an item is selected, return false to prevent the item from being selected
    /// </summary>
    bool OnItemSelected(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo);
    /// <summary>
    /// Called when the panel is selected, return false to prevent the panel from being selected
    /// </summary>
    /// <returns>Return false to prevent the panel from being selected</returns>
    bool OnPanelSelected(SpawnablePanelExtension extension);
    IReadOnlyList<ItemRender> GetPage(int page);
}