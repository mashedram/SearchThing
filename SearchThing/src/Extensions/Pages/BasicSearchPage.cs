using SearchThing.Extensions.Panel;
using UnityEngine.UIElements;

namespace SearchThing.Extensions.Pages;

public class BasicSearchPage : ISearchPage
{
    public IReadOnlyList<ISearchPanel> Panels { get; }
    
    public BasicSearchPage(params ISearchPanel[] panels)
    {
        Panels = panels;
    }
}