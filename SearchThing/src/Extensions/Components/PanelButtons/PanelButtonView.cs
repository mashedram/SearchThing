using Il2CppTMPro;
using SearchThing.Extensions.Pages;
using SearchThing.Extensions.Panel;
using UnityEngine;

namespace SearchThing.Extensions.Components.PanelButtons;

public class PanelButtonView
{
    // Source
    private readonly SpawnablePanelExtension _parent;
    
    // Renderers
    private readonly List<PanelButton> _tagButtons = new();
    private readonly GameObject _tagPageNextButton;
    private readonly GameObject _tagPagePreviousButton;
    private readonly TextMeshPro _treePageText;
    
    // Data
    private readonly SpawnablePageProvider _provider;
    private int _pageIndex;
    private ISearchPage _currentPage;

    // Getters
    public ISearchPanel SelectedPanel { get; private set; }

    public PanelButtonView(SpawnablePanelExtension parent, SpawnablePageProvider provider)
    {
        _parent = parent;
        _provider = provider;
        
        _currentPage = _provider.GetPage(0) ?? throw new Exception("No pages found");
        SelectedPanel = _currentPage.Panels.FirstOrDefault(p => !p.Redacted) ?? throw new Exception("No panels found");
        
        var panelView = parent.PanelView;
        foreach (var panelViewTagButton in panelView.treeButtons)
        {
            if (panelViewTagButton == null)
                continue;

            _tagButtons.Add(new PanelButton(panelViewTagButton));
        }

        _tagPageNextButton = panelView.treeScrollDownButton.gameObject;
        _tagPagePreviousButton = panelView.treeScrollUpButton.gameObject;
        _treePageText = panelView.treePageText;
    }

    public void Render()
    {
        for (var i = 0; i < _tagButtons.Count; i++)
        {
            var button = _tagButtons[i];
            var panel = _currentPage.Panels[i];

            if (panel is not { Redacted: false })
            {
                button.Hide();
                continue;
            }

            var isSelected = panel.Id == SelectedPanel?.Id;
            // var tag = IsEditing && _currentFocus == SpawnInfoFocus.SelectedPage && isSelected
            //     ? _editValue
            //     : panel.Name;
            // TODO : Allow editing
            var tag = panel.Name;
            var forceHighlight = panel.IsForceHighlighted(_parent);

            button.SetTag(tag, isSelected || forceHighlight != null, forceHighlight);
        }

        var pageCount = _currentPage.Panels.Count;
        _treePageText.text = $"{_pageIndex + 1}/{pageCount}";
        _tagPageNextButton.SetActive(_pageIndex < pageCount - 1);
        _tagPagePreviousButton.SetActive(_pageIndex > 0);
    }
    
    public void Reset()
    {
        foreach (var button in _tagButtons)
        {
            button.Reset();
        }
    }

    public bool SelectPage(int idx)
    {
        if (idx < 0)
            return false;
        if (idx >= _currentPage.Panels.Count)
            return false;
        
        var panel = _currentPage.Panels[idx];
        if (panel.Redacted)
            return false;
        
        // Check if we can select the panel
        if (!panel.OnSelected(_parent))
            return false;

        SelectedPanel = panel;
        return true;
    }
    
    public void SetPage(int index)
    {
        if (index < 0)
            return;
        var page = _provider.GetPage(index);
        if (page == null)
            return;
        
        _pageIndex = index;
        _currentPage = page;
    }

    public void OffsetPage(int offset)
    {
        SetPage(_pageIndex + offset);
    }
}