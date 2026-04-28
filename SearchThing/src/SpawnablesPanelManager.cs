using System.Diagnostics.CodeAnalysis;
using Il2CppSLZ.UI;
using SearchThing.Extensions;

namespace SearchThing;

public static class SpawnablesPanelManager
{
    private static SpawnablePanelExtension? _extension;
    
    public static void Load(SpawnablesPanelView panelView)
    {
        if (_extension != null && _extension.Is(panelView))
        {
            _extension.Rerender();
            return;
        }
        
        _extension = new SpawnablePanelExtension(panelView);
    }
    
    public static void OnTabSelected(SpawnablesPanelView panelView, int index)
    {
        if (_extension == null || !_extension.Is(panelView))
            return;
        
        if (index == SpawnablePanelExtension.SearchTabIndex)
            _extension.Show();
        else
            _extension.Hide();
    }

    public static bool TryGet(SpawnablesPanelView instance, [MaybeNullWhen(false)] out SpawnablePanelExtension extension)
    {
        if (_extension != null && _extension.Is(instance))
        {
            extension = _extension;
            return true;
        }

        extension = null;
        return false;
    }
}