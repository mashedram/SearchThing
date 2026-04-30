using SearchThing.Extensions.Panel;

namespace SearchThing.Extensions.Pages;

public interface ISearchPage
{
    public const int PanelsPerPage = 6;
    public IReadOnlyList<ISearchPanel> Panels { get; }
}