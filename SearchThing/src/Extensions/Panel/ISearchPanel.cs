using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Extensions.Sort;
using SearchThing.Search;
using UnityEngine;

namespace SearchThing.Extensions.Panel;

public interface ISearchPanel
{
    public const int PanelSize = 12;
    
    Guid Id { get; }
    string Tag { get; }
    bool TagEditable { get; }
    string Query { get; set; }
    int SelectedOrderIndex { get; set; }
    ISelectableSearchOrder[] SupportedOrders { get; }
    int Page { get; set; }
    int PageCount { get; }
    /// <summary>
    /// Get's called when the tag edit is complete, this is where you should save the new tag
    /// </summary>
    /// <remarks>It's the panel's responsibility to call extension.Rerender() when the tag is changed</remarks>
    /// <param name="extension"></param>
    /// <param name="newTag"></param>
    void OnTagEdited(SpawnablePanelExtension extension, string newTag);
    /// <summary>
    /// Get's called for every crate that is rendered, return true to force the crate to be highlighted in the menu
    /// </summary>
    /// <returns></returns>
    Color? IsForceHighlighted(SpawnablePanelExtension extension, ISearchableCrate? selectedCrate);
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
    /// Called when the panel is selected, return false to prevent the panel from being selected
    /// </summary>
    /// <returns>Return false to prevent the panel from being selected</returns>
    bool OnSelected(SpawnablePanelExtension extension);
    ISearchableCrate? GetCrateAt(int index);
    IReadOnlyList<ISearchableCrate> GetPage(int page);
    /// <summary>
    /// Return an IEnumerable to render the page with
    /// Only the first 12 entries will be considered
    /// </summary>
    /// <returns></returns>
    IEnumerable<SpawnableCrate> Render(int page);
}