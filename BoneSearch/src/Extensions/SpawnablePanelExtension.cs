using System;
using BoneSearch.Search;
using BoneSearch.Util;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.UI;
using Il2CppSystem.Runtime.InteropServices;
using Il2CppTMPro;
using LabFusion.Extensions;
using MelonLoader;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BoneSearch.Extensions;

public class SpawnablePanelExtension
{
    public const int SearchTabIndex = 5;
    
    private string _searchQuery = "";
    private readonly SpawnablesPanelView _panelView;
    private readonly Keyboard.Keyboard _keyboard;
    private const string SourceTabButtonPath = "group_tabs/grid_tabs/button_tab_05";

    private static readonly Texture2D IconTexture = ImageHelper.LoadEmbeddedImage("BoneSearch.resources.SearchIcon.png");
    
    private void AddTab()
    {
        var sourceButton = _panelView.transform.Find(SourceTabButtonPath);
        if (sourceButton == null)
        {
            MelonLogger.Error($"Failed to find source button at path: {SourceTabButtonPath}");
            return;
        }
        
        var searchButton = UnityEngine.Object.Instantiate(sourceButton.gameObject, sourceButton.parent);
        searchButton.name = "button_tab_search";

        var image = searchButton.transform.FindChild("image_icon")?.GetComponent<Image>();
        if (image != null)
            image.sprite = Sprite.Create(IconTexture, new Rect(0, 0, IconTexture.width, IconTexture.height), Vector2.zero);
        
        var searchTabButton = searchButton.GetComponent<Button>();
        if (searchTabButton == null)
        {
            MelonLogger.Error("Failed to find Button component in search button.");
            return;
        }

        searchTabButton.onClick.RemoveAllListeners();
        searchTabButton.onClick.AddListener((UnityAction)OnSearchButtonClicked);

        var tabButtons = _panelView.tabButtons.ToList();
        tabButtons.Add(searchTabButton.GetComponent<ButtonReferenceHolder>());
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

    public SpawnablePanelExtension(SpawnablesPanelView panelView)
    {
        _panelView = panelView;
        _keyboard = new Keyboard.Keyboard(_panelView.gameObject);
        _keyboard.OnTextChanged += OnSearchQueryChanged;
        _keyboard.Hide();

        AddTab();
    }

    private void OnSearchQueryChanged(string query)
    {
        _searchQuery = query;
        RequestRefresh();
    }

    public void RequestRefresh()
    {
        SearchManager.SearchAsync(_searchQuery, Refresh);
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
    
    public void EnableTags(string defaultTag, params string[] tags)
    {
        var activeTags = _panelView._activeTags;
        activeTags.Clear();
        activeTags.Add(defaultTag);
        foreach (var tag in tags)
        {
            activeTags.Add(tag);
        }
        
        if (activeTags.Contains(_panelView._selectedTag))
            return;

        _panelView._selectedTag = defaultTag;
    }

    public void Refresh(IEnumerable<SpawnableCrate> search)
    {
        var contents = _panelView.SpawnablesQuickMap;
        var propMap = GetAndClearList("Props");
        var avatarMap = GetAndClearList("Avatars");
        EnableTags("Props", "Avatars");
        
        foreach (var spawnableCrate in search)
        {
            if (spawnableCrate.TryCast<AvatarCrate>() == null)
            {
                propMap.Add(spawnableCrate);
            }
            else
            {
                avatarMap.Add(spawnableCrate);
            }
        }

        var selectedMap = contents[_panelView._selectedTag];
        var pageCount = selectedMap._size / 12;
        _panelView._numberOfPages = pageCount;
        
        _panelView.labelText.text = _searchQuery;
        _panelView.UpdatePageItems(0, 12);
        _panelView.UpdatePageText(0, pageCount);

        var tagePageCount = _panelView.SpawnablesQuickMap._count;
        _panelView.UpdateTagPageItems(0, tagePageCount);
        _panelView.UpdateTagPageText(0, tagePageCount);
    }

    public void Show()
    {
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
    
    public bool Is(SpawnablesPanelView panelView)
    {
        if (_panelView == null || panelView == null)
            return false;
        
        if (_panelView.gameObject == null)
            return false;
        
        return _panelView.gameObject.EqualsIL2CPP(panelView.gameObject);
    }
}