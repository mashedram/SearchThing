using Il2CppSLZ.UI;
using Il2CppTMPro;
using SearchThing.Extensions.Panel;
using SearchThing.Search.Data;
using UnityEngine;

namespace SearchThing.Extensions.Components.ItemButtons;

public class ItemButtonView
{
    // Renderers
    private readonly List<ItemButton> _itemButtons = new();
    private readonly GameObject _itemPageNextButton;
    private readonly GameObject _itemPagePreviousButton;
    private readonly TextMeshPro _itemPageText;
    
    // Child
    private readonly SortButton _sortButton;
    
    // Data
    private ISearchPanel? _panel;
    private ItemButton? _selectedItemButton;
    
    public IRequiredItemInfo? SelectedItem => _selectedItemButton?.ItemInfo;
    
    public ItemButtonView(SpawnablePanelExtension panelExtension)
    {
        var panelView = panelExtension.PanelView;
        for (var index = 0; index < panelView.itemButtons.Count; index++)
        {
            var panelViewItemButton = panelView.itemButtons[index];
            if (panelViewItemButton == null)
                continue;

            _itemButtons.Add(new ItemButton(panelExtension, panelViewItemButton, index));
        }

        _itemPageNextButton = panelView.itemScrollDownButton.gameObject;
        _itemPagePreviousButton = panelView.itemScrollUpButton.gameObject;
        
        _itemPageText = panelView.itemPageText;
        
        _sortButton = new SortButton(panelExtension);
    }
    
    public IRequiredItemInfo? GetItemInfo(int index)
    {
        if (index < 0 || index >= _itemButtons.Count)
            return null;
        var itemButton = _itemButtons[index];
        if (!itemButton.IsVisible)           
            return null;
        
        return itemButton.ItemInfo;
    }
    
    public void SelectItem(int index)
    {
        if (_panel == null)
            return;
        if (index < 0)
            return;
        var item = _itemButtons[index];
        if (!item.IsVisible)
            return;
        
        if (!item.OnSelected())
            return;

        _selectedItemButton = item;
    }

    public void SetPanel(ISearchPanel panel)
    {
        _panel = panel;
        _sortButton.SetPanel(panel);
    }
    
    public void Render()
    {
        if (_panel == null)
            return;
        
        var entries = _panel
            .GetPage(_panel.Page);

        for (var i = 0; i < _itemButtons.Count; i++)
        {
            var button = _itemButtons[i];
            if (i < entries.Count)
            {
                var entry = entries[i];
                button.SetCrate(entry, button.Id == _selectedItemButton?.Id);
            }
            else
            {
                button.Hide();
            }
        }

        var pageCount = _panel.PageCount;

        _itemPageText.text = $"{_panel.Page + 1}/{pageCount}";

        _itemPageNextButton.SetActive(_panel.Page < _panel.PageCount - 1);
        _itemPagePreviousButton.SetActive(_panel.Page > 0);
    }
    
    public void Reset()
    {
        foreach (var button in _itemButtons)
        {
            button.Reset();
        }
    }
    
    public void SetPage(int page)
    {
        if (_panel == null)
            return;
        if (page < 0 || page >= _panel.PageCount)
            return;

        _panel.Page = page;
    }
    
    public void OffsetPage(int offset)
    {
        if (_panel == null)
            return;
        SetPage(_panel.Page + offset);
    }
}