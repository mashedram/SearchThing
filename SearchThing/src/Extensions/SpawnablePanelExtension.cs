using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.UI;
using Il2CppTMPro;
using LabFusion.Extensions;
using MelonLoader;
using SearchThing.Extensions.Pages;
using SearchThing.Extensions.Panel;
using SearchThing.Extensions.Panel.Filter;
using SearchThing.Extensions.Panel.History;
using SearchThing.Util;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using PresetManager = SearchThing.Presets.PresetManager;

namespace SearchThing.Extensions;

public class SpawnablePanelExtension
{
    public const int SearchTabIndex = 5;

    private const string SourceTabButtonPath = "group_tabs/grid_tabs/button_tab_05";
    private const string SearchTabName = "button_tab_search";
    
    private string _searchQuery = "";
    private readonly SpawnablesPanelView _panelView;
    private readonly Keyboard.Keyboard _keyboard;

    private static readonly Sprite TabIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.SearchIcon.png");
    // TODO: Change to a good icon
    private static readonly Sprite PresetAddIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.SearchIcon.png");
    
    // Favorite button
    private Image _fadedButtonImage = null!;
    private Image _favoriteButtonImage = null!; 
    private Sprite _originalFavoriteSprite = null!;
    // There is no easily stealable default behavior for the sorting button, so we need to get references to it
    private TextMeshPro _sortButtonText = null!;
    private GameObject _sortButtonObject = null!;
    
    // The page currently selected in the tag page, but not used for rendering in case a rerender gets called when the page changes but selected tag doesn't
    private int _renderedPageIndex = 0;
    // We need the selected tag in a prefix context, so we need to store it
    private int _selectedTagIndex = 0;
    private int _selectedPageIndex = 0;
    private readonly ISearchPage[] _pages = {
        new BasicSearchPage(new PropTagSearchPanel(), new AvatarTagSearchPanel(), new PropHistorySearchPanel(), new AvatarHistorySearchPanel()),
        PresetManager.GetPages().First()
    };
    
    private void AddTab()
    {
        var sourceButton = _panelView.transform.Find(SourceTabButtonPath);
        if (sourceButton == null)
        {
            MelonLogger.Error($"Failed to find source button at path: {SourceTabButtonPath}");
            return;
        }
        
        // Prevent accidentally adding the button if we have already found it
        if (sourceButton.transform.FindChild(SearchTabName) != null)
            return;
        
        var searchButton = UnityEngine.Object.Instantiate(sourceButton.gameObject, sourceButton.parent);
        searchButton.name = SearchTabName;
        
        var searchTabButton = searchButton.GetComponent<Button>();
        var tabButtonReferenceHolder = searchButton.GetComponent<ButtonReferenceHolder>();
        if (searchTabButton == null || tabButtonReferenceHolder == null)
        {
            MelonLogger.Error("Failed to find Button component in search button.");
            return;
        }

        searchTabButton.onClick.RemoveAllListeners();
        searchTabButton.onClick.AddListener((UnityAction)OnSearchButtonClicked);

        var image = searchButton.transform.FindChild("image_icon")?.GetComponent<Image>();
        if (image != null)
            image.sprite = TabIcon;

        var tabButtons = _panelView.tabButtons.ToList();
        tabButtons.Add(tabButtonReferenceHolder);
        _panelView.tabButtons = new Il2CppReferenceArray<ButtonReferenceHolder>(tabButtons.ToArray());

        var text = searchButton.transform.Find("text_spawnable_val")?.GetComponent<TextMeshPro>();
        if (text != null)
        {
            text.text = "Search";
        }
        else
        {
            MelonLogger.Error("Failed to find text component in search button.");
        }
    }

    private void OnSearchButtonClicked()
    {
        // Tab 5 is the new tab we added
        _panelView.SelectTab(SearchTabIndex);
    }
    
    private void FetchSortButton()
    {
        _sortButtonText = _panelView.transform.Find("group_treePath/text_treePath")!.GetComponent<TextMeshPro>();
        // For some reason the text is centered by default and only goes to the left after a SwapSort call
        // SwapSort forces you to tab 3, so we need to do this workaround
        _sortButtonText.alignment = TextAlignmentOptions.Left;
        
        _sortButtonObject = _panelView.transform.Find("group_treePath/button_SwapSort")!.gameObject;
    }
    
    private void FetchFavoriteButton()
    {
        var favoriteButton = _panelView.transform.Find("group_selectedInfo/button_Favorite");
        if (favoriteButton == null)
        {
            MelonLogger.Error("Failed to find favorite button.");
            return;
        }

        var references = favoriteButton.GetComponent<ButtonReferenceHolder>();
        _fadedButtonImage = references.highlight;
        _favoriteButtonImage = references.special;
        if (_favoriteButtonImage == null || _fadedButtonImage == null)
        {
            MelonLogger.Error("Failed to find Image component in favorite button.");
            return;
        }

        _originalFavoriteSprite = _favoriteButtonImage.sprite;
    }
    
    public bool IsPanelSelected(ISearchPanel page)
    {
        var selectedPage = GetSelectedPanel();
        return selectedPage.Id == page.Id;
    }

    private ISearchPage GetRenderPage()
    {
        var selectedPageIndex = _renderedPageIndex;
        if (selectedPageIndex < 0 || selectedPageIndex >= _pages.Length)
            return _pages.First();
        
        return _pages[selectedPageIndex];
    }
    
    private ISearchPanel GetRenderPanel(int idx)
    {
        var page = GetRenderPage();
        if (idx < 0 || idx >= page.Panels.Count)
            return page.Panels[0];
        
        return page.Panels[idx];
    }

    private ISearchPage GetSelectedPage()
    {
        var selectedPageIndex = _selectedPageIndex;
        if (selectedPageIndex < 0 || selectedPageIndex >= _pages.Length)
            return _pages.First();
        
        return _pages[selectedPageIndex];
    }
    
    private ISearchPanel GetSelectedPanel(int idx = -1)
    {
        var page = GetSelectedPage();
        var selectedTagIndex = _selectedTagIndex;
        if (selectedTagIndex < 0 || selectedTagIndex >= page.Panels.Count)
            return page.Panels[0];
        
        return page.Panels[idx >= 0 ? idx : selectedTagIndex];
    }

    public SpawnablePanelExtension(SpawnablesPanelView panelView)
    {
        _panelView = panelView;
        
        AddTab();
        FetchSortButton();
        FetchFavoriteButton();
        
        // Find the buttonReference we want to use for our style
        var buttonStyleReference = _panelView.transform.Find("group_spawnSelect/section_SpawnablesList/grid_buttons/button_item_01").GetComponent<ButtonReferenceHolder>();
        
        _keyboard = new Keyboard.Keyboard(_panelView.gameObject, buttonStyleReference);
        _keyboard.OnTextChanged += OnSearchQueryChanged;
        _keyboard.Hide();
    }

    private void OnSearchQueryChanged(string query)
    {
        _searchQuery = query;
        RequestRefresh();
    }

    public void RequestRefresh()
    {
        var panel = GetSelectedPanel();
        panel.Query = _searchQuery;
        panel.RequestSearch(this);
    }
    
    public SpawnableCrate? GetSelectedSpawnable()
    {
        var contents = _panelView.SpawnablesQuickMap;
        var selectedTag = _panelView._selectedItemTag;
        
        if (!contents.TryGetValue(selectedTag, out var list))
            return null;

        var page = _panelView._currentPage;
        var selectedIndex = _panelView._selectedItemIndex;
        var index = page * ISearchPanel.PanelSize + selectedIndex;
        
        if (index < 0 || index >= list.Count)
            return null;

        return list[index];
    }

    public void EnsureList(string name)
    {
        var contents = _panelView.SpawnablesQuickMap;
        if (!contents.ContainsKey(name))
        {
            contents[name] = new Il2CppSystem.Collections.Generic.List<SpawnableCrate>();
        }
    }

    public Il2CppSystem.Collections.Generic.List<SpawnableCrate> GetList(string name)
    {
        var contents = _panelView.SpawnablesQuickMap;
        if (contents.TryGetValue(name, out var list))
            return list;

        list = new Il2CppSystem.Collections.Generic.List<SpawnableCrate>();
        contents[name] = list;
        return list;
    }

    public Il2CppSystem.Collections.Generic.List<SpawnableCrate> GetAndClearList(string name)
    {
        var list = GetList(name);
        list.Clear();
        return list;
    }

    public void UpdateTagVisuals()
    {
        // We select tags ourself, so no need to set them
        // _panelView._selectedTagIndex = _selectedTagIndex;
        // // In case we are in a prefix, we need to manually request the panel
        // _panelView._selectedTag = GetSelectedPanel(_selectedTagIndex).Tag;
        _panelView._numberOfTagPages = _pages.Length;
        
        // We need to manually set the outline, because we skip the method that does so internally
        var tagButtons = _panelView.treeButtons;
        for (var i = 0; i < tagButtons.Count; i++)
        {
            var reference = tagButtons[i];
            if (reference == null)
                continue;
            
            reference.highlight.enabled = i == _selectedTagIndex && _selectedPageIndex == _renderedPageIndex;
        }
        
        // We only populate the first page and only wish to render that one
        
        _panelView.UpdateTagPageItems(0, ISearchPage.PanelsPerPage);
        _panelView.UpdateTagPageText(_renderedPageIndex, _pages.Length);
    }
    
    public void EnableTags(params string[] tags)
    {
        var activeTags = _panelView._activeTags;
        activeTags.Clear();
        foreach (var tag in tags)
        {
            EnsureList(tag);
            activeTags.Add(tag);
        }
        
        UpdateTagVisuals();
    }

    public void RenderTags()
    {
        var page = GetRenderPage();
        EnableTags(page.Panels.Select(p => p.Tag).ToArray());
    }
    
    public void Rerender()
    {
        var selectedPage = GetSelectedPanel();
        
        // Render tags page

        RenderTags();
  
        // Render page
        
        var selectedTag = selectedPage.Tag;
        
        // Assign to the menu what we are writing too so it renders properly
        _panelView._selectedTagIndex = _selectedTagIndex;
        _panelView._selectedTag = selectedTag;
        
        // We realy don't care what the actual tag is, as long as we put things in it and virtualize it in our systems, it's fine.
        var contents = GetAndClearList(selectedTag);

        var entries = selectedPage
            .Render(selectedPage.Page);
        foreach (var entry in entries)
        {
            contents.Add(entry);
        }

        var pageCount = selectedPage.PageCount;
        _panelView._numberOfPages = pageCount;
            
        _panelView.labelText.text = _searchQuery;
        
        // This function crashes if called from a non-unity thread
        _panelView.UpdatePageItems(0, ISearchPanel.PanelSize);
        _panelView.UpdatePageText(selectedPage.Page, pageCount);
        
        // Update sort button
        if (selectedPage.SupportedOrders.Length > 1)
        {
            _sortButtonObject.SetActive(true);
            _sortButtonText.gameObject.SetActive(true);
            
            var order = selectedPage.SupportedOrders[selectedPage.SelectedOrderIndex];
            _sortButtonText.text = order.Name;
            // We do what FetchSortButton does every tick in case a tab switch undoes the left alignment
            _sortButtonText.alignment = TextAlignmentOptions.Left;
        }
        else
        {
            _sortButtonObject.SetActive(false);
            _sortButtonText.gameObject.SetActive(false);
        }
        
        UpdateFavoriteVisual();
    }

    public void ChangePanelPage(int offset)
    {
        var selectedPage = GetSelectedPanel();
        selectedPage.ChangePage(this, offset);
    }
    
    public void SelectCategory(int idx)
    {
        // Get the new selected panel instead of the one currently selected
        var result = GetRenderPanel(idx).OnSelected(this);

        if (!result)
            return;

        // We've selected a new tag, so we need to update what we have selected to what we rendered
        _selectedPageIndex = _renderedPageIndex;
        _selectedTagIndex = idx;
        RequestRefresh();
    }

    public void ChangeTagPage(int offset)
    {
        var newPage = _renderedPageIndex + offset;
        if (newPage < 0 || newPage >= _pages.Length)            
            return;
        _renderedPageIndex = newPage;
        
        RenderTags();
    }
    
    public void SwapSortButton()
    {
        var selectedPage = GetSelectedPanel();
        selectedPage.SelectedOrderIndex = (selectedPage.SelectedOrderIndex + 1) % selectedPage.SupportedOrders.Length;
        RequestRefresh();
    }
    
    public void TogglePresetAssignmentMode()
    {
        PresetManager.IsAssignmentMode = !PresetManager.IsAssignmentMode;
        
        // Set the rendertarget to page 1 so we are at the presets if we aren't yet
        if (_renderedPageIndex < 1)
            _renderedPageIndex = 1;
        
        Rerender();
    }

    public void UpdateFavoriteVisual()
    {
        // If we have nothing selected, do nothing
        if (_panelView._selectedItem < 0)
            return;
        
        var isAssignmentMode = PresetManager.IsAssignmentMode;
        var isFavorite = _panelView.selectedObject != null && _panelView.favoriteCrates.ContainsKey(_panelView.selectedObject._barcode._id);
        _fadedButtonImage.enabled = !isAssignmentMode && !isFavorite;
        _favoriteButtonImage.enabled = isAssignmentMode || isFavorite;
        _favoriteButtonImage.sprite = isAssignmentMode ? PresetAddIcon : _originalFavoriteSprite;
    }
    
    public void RefreshPresetAssignment()
    {
        _favoriteButtonImage.sprite = PresetManager.IsAssignmentMode ? PresetAddIcon : _originalFavoriteSprite;
    }

    public void Show()
    {
        RenderTags();
        ShowKeyboard();
        RequestRefresh();
    }

    public void Hide()
    {
        CloseKeyboard();
        _favoriteButtonImage.sprite = _originalFavoriteSprite;
    }

    public void ShowKeyboard()
    {
        _keyboard.Show();
    }

    public void CloseKeyboard()
    {
        _keyboard.Hide();
    }

    public bool IsSearchActive()
    {
        return _panelView._selectedTabIndex == SearchTabIndex;
    }
    
    public bool Is(SpawnablesPanelView panelView)
    {
        if (_panelView == null || panelView == null)
            return false;
        
        if (_panelView.gameObject == null)
            return false;
        
        return _panelView.gameObject.EqualsIL2CPP(panelView.gameObject);
    }
}