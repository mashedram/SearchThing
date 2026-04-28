using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.UI;

namespace SearchThing.Extensions;

public interface IPanelPage
{
    public const int PageSize = 12;
    
    string Tag { get; }
    void OnQueryChange(SpawnablePanelExtension extension, string query);
    int Page { get; set; }
    int PageCount { get; }
    void ChangePage(SpawnablePanelExtension extension, int offset);
    /// <summary>
    /// Return an IEnumerable to render the page with
    /// Only the first 12 entries will be considered
    /// </summary>
    /// <returns></returns>
    IEnumerable<SpawnableCrate> Render(int page);
}