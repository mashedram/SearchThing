using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Extensions.Sort;

namespace SearchThing.Extensions.Panel;

public interface ISearchPanel
{
    public const int PanelSize = 12;
    
    Guid Id { get; }
    string Tag { get; }
    string Query { get; set; }
    int SelectedOrderIndex { get; set; }
    ISelectableSearchOrder[] SupportedOrders { get; }
    void RequestSearch(SpawnablePanelExtension extension);
    int Page { get; set; }
    int PageCount { get; }
    void ChangePage(SpawnablePanelExtension extension, int offset);
    /// <summary>
    /// Called when the panel is selected, return false to prevent the panel from being selected
    /// </summary>
    /// <returns>Return false to prevent the panel from being selected</returns>
    bool OnSelected(SpawnablePanelExtension extension);
    /// <summary>
    /// Return an IEnumerable to render the page with
    /// Only the first 12 entries will be considered
    /// </summary>
    /// <returns></returns>
    IEnumerable<SpawnableCrate> Render(int page);
}