using SearchThing.Extensions.Panel;

namespace SearchThing.Extensions.Pages;

public interface ISearchPage
{
    /// <summary>
    /// Whether the page is visible.
    /// </summary>
    /// <remarks>DO NOT flick this on and off whenever you want. Use it to hide pages for specific mods.</remarks>
    public bool IsVisible { get; }
    public IReadOnlyList<ISearchPanel> Panels { get; }
}