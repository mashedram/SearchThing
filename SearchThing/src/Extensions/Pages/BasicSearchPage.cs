using SearchThing.Extensions.Panel;
using UnityEngine.UIElements;

namespace SearchThing.Extensions.Pages;

public class BasicSearchPage : ISearchPage
{
    private readonly Func<bool> _isVisibleFunc = () => true;
    public bool IsVisible => _isVisibleFunc();
    public IReadOnlyList<ISearchPanel> Panels { get; }

    public BasicSearchPage(Func<bool> isVisibleFunc, params ISearchPanel[] panels)
    {
        _isVisibleFunc = isVisibleFunc;
        Panels = panels;
    }

    public BasicSearchPage(params ISearchPanel[] panels)
    {
        Panels = panels;
    }
}