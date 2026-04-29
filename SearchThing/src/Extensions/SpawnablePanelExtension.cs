using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.UI;
using Il2CppSystem.Runtime.InteropServices;
using Il2CppTMPro;
using LabFusion.Extensions;
using MelonLoader;
using SearchThing.Extensions.Filter;
using SearchThing.Extensions.History;
using SearchThing.Search;
using SearchThing.Util;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SearchThing.Extensions;

public class SpawnablePanelExtension
{
    public const int SearchTabIndex = 5;

    private const string SourceTabButtonPath = "group_tabs/grid_tabs/button_tab_05";
    private const string SearchTabName = "button_tab_search";
    
    private string _searchQuery = "";
    private readonly SpawnablesPanelView _panelView;
    private readonly Keyboard.Keyboard _keyboard;

    private static readonly Texture2D IconTexture = ImageHelper.LoadEmbeddedImage("SearchThing.resources.SearchIcon.png");

    // There is no easily stealable default behavior for the sorting button, so we need to get references to it
    private TextMeshPro _sortButtonText = null!;
    private GameObject _sortButtonObject = null!;
    
    // Page selection stuff
    private int _lastSelectedTag = -1;
    
    private readonly IPanelPage[] _pages = {
        new PropTagPanelPage(),
        new AvatarTagPanelPage(),
        new PropHistoryPanel(),
        new AvatarHistoryPanel()
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
            image.sprite = Sprite.Create(IconTexture, new Rect(0, 0, IconTexture.width, IconTexture.height), Vector2.zero);

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
    
    private IPanelPage GetSelectedPage()
    {
        var selectedTagIndex = _panelView._selectedTagIndex;
        if (selectedTagIndex < 0 || selectedTagIndex >= _pages.Length)
            return _pages.First();

        return _pages[selectedTagIndex];
    }

    public SpawnablePanelExtension(SpawnablesPanelView panelView)
    {
        _panelView = panelView;
        
        AddTab();
        FetchSortButton();
        
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
        var panel = _pages[_panelView._selectedTagIndex];
        panel.Query = _searchQuery;
        panel.RequestSearch(this);
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
        var tagePageCount = _panelView._activeTags.Count;
        _panelView.UpdateTagPageItems(0, tagePageCount);
        _panelView.UpdateTagPageText(_panelView._selectedTagIndex, tagePageCount);
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
        
        if (!activeTags.Contains(_panelView._selectedTag))
            _panelView._selectedTag = activeTags.Count > 0 ? activeTags[0] : "All";
        
        UpdateTagVisuals();
    }

    public void Rerender()
    {
        var selectedPage = GetSelectedPage();
        
        var selectedTagIndex = _panelView._selectedTagIndex;
        if (_lastSelectedTag != selectedTagIndex)
        {
            _lastSelectedTag = selectedTagIndex;
            // Ensure that we go to page 0 when hopping between them
            selectedPage.Page = 0;
            selectedPage.SelectedOrderIndex = 0;
        }

        var selectedTag = selectedPage.Tag;
        _panelView._selectedItemTag = selectedTag;
        var contents = GetAndClearList(selectedTag);

        // TODO : This doesn't filter, currently
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
        _panelView.UpdatePageItems(0, IPanelPage.PageSize);
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
    }

    public void ChangePage(int offset)
    {
        var selectedPage = GetSelectedPage();
        selectedPage.ChangePage(this, offset);
    }
    
    public void NextPage()
    {
        ChangePage(1);
    }
    
    public void PrevPage()
    {
        ChangePage(-1);
    }
    
    public void SelectCategory(int idx)
    {
        RequestRefresh();
    }
    
    public void SwapSortButton()
    {
        var selectedPage = GetSelectedPage();
        selectedPage.SelectedOrderIndex = (selectedPage.SelectedOrderIndex + 1) % selectedPage.SupportedOrders.Length;
        RequestRefresh();
    }

    public void Show()
    {
        EnableTags(_pages.Select(p => p.Tag).ToArray());
        ShowKeyboard();
        RequestRefresh();
    }

    public void Hide()
    {
        CloseKeyboard();
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