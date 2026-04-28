using BoneSearch.Extensions;
using Il2CppSLZ.UI;

namespace BoneSearch;

public static class SpawnablesPanelManager
{
    private static SpawnablePanelExtension? _extension;
    
    public static void Load(SpawnablesPanelView panelView)
    {
        if (_extension != null && _extension.Is(panelView))
            return;
        
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
}