using Il2CppCysharp.Threading.Tasks;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.SceneStreaming;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.UI;
using Il2CppTMPro;
using MelonLoader;
using SearchThing.Extensions.Components;
using SearchThing.Extensions.Components.ItemButtons;
using SearchThing.Extensions.Components.PanelButtons;
using SearchThing.Extensions.Pages;
using SearchThing.Extensions.Panel;
using SearchThing.Patches;
using SearchThing.Presets;
using SearchThing.Search;
using SearchThing.Search.CrateData;
using SearchThing.Search.Data;
using SearchThing.Search.Interaction;
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
    public SpawnablesPanelView PanelView { get; }
    private readonly Keyboard.Keyboard _keyboard;
    
    // Components
    private readonly PanelButtonView _panelButtonView;
    private readonly ItemButtonView _itemButtonView;
    private readonly ItemInfoBox _infoBox;

    private static readonly Sprite TabIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.SearchIcon.png");

    // Editing
    private string _editValue = "";
    public bool IsEditing { get; private set; }

    // // Favorite button
    // private Image _fadedButtonImage = null!;
    // private Image _favoriteButtonImage = null!;
    // private Sprite _originalFavoriteSprite = null!;
    //
    // private Color _originalFavoriteColor = Color.white;

    // The page currently selected in the tag page, but not used for rendering in case a rerender gets called when the page changes but selected tag doesn't
    // private int _renderedTagPageIndex;
    //
    // // We need the selected tag in a prefix context, so we need to store it
    // private int _selectedItemIndex;
    // private int _selectedTagIndex;
    // private int _selectedPageIndex;
    // // Confirmation logic
    // private int _lastSelectedItemIndex;
    // Pages
    private readonly SpawnablePageProvider _pages = new();

    private void AddTab()
    {
        var sourceButton = PanelView.transform.Find(SourceTabButtonPath);
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
        searchTabButton.onClick.AddListener((UnityAction)OnSearchTabClicked);

        var image = searchButton.transform.FindChild("image_icon")?.GetComponent<Image>();
        if (image != null)
            image.sprite = TabIcon;

        var tabButtons = PanelView.tabButtons.ToList();
        tabButtons.Add(tabButtonReferenceHolder);
        PanelView.tabButtons = new Il2CppReferenceArray<ButtonReferenceHolder>(tabButtons.ToArray());

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

    private void OnSearchTabClicked()
    {
        // Tab 5 is the new tab we added
        PanelView.SelectTab(SearchTabIndex);
    }

    public SpawnablePanelExtension(SpawnablesPanelView panelView)
    {
        PanelView = panelView;

        AddTab();
        
        _panelButtonView = new PanelButtonView(this, _pages);
        _itemButtonView = new ItemButtonView(this);
        _infoBox = new ItemInfoBox(this);
        
        // Set default content
        _itemButtonView.SetPanel(_panelButtonView.SelectedPanel);

        // Find the buttonReference we want to use for our style
        var buttonStyleReference = PanelView.transform.Find("group_spawnSelect/section_SpawnablesList/grid_buttons/button_item_01")
            .GetComponent<ButtonReferenceHolder>();

        _keyboard = new Keyboard.Keyboard(PanelView.gameObject, buttonStyleReference);
        _keyboard.OnTextChanged += OnSearchQueryChanged;
        _keyboard.Hide();

        // Prefetch to avoid the delay on first load
        RequestRefresh();
    }
    
    private void OnSearchQueryChanged(string query)
    {
        if (IsEditing)
        {
            _editValue = query;
            RenderAll();
        }
        else
        {
            _searchQuery = query;
            RequestRefresh();
        }
    }

    public void SetIsEditing(bool isEditing)
    {
        // Skip if we are already in the correct mode
        if (isEditing == IsEditing)
            return;

        IsEditing = isEditing;
        // var panel = GetSelectedPanel();
        //
        // if (!panel.TagEditable)
        // {
        //     IsEditing = false;
        //     return;
        // }
        //
        // // Enter editing
        // if (IsEditing)
        // {
        //     // Ensure we aren't assigning
        //     PresetManager.IsAssignmentMode = false;
        //     // Force the tag to know its in edit mode
        //     _editValue = panel.Name;
        //     // We don't want to trigger events to prevent circular calls
        //     _keyboard.SetText(_editValue, false);
        // }
        // // Exit editing
        // else
        // {
        //     // Reset back to the search query
        //     _keyboard.SetText(_searchQuery, false);
        //     panel.OnTagEdited(this, _editValue);
        //
        //     // Render tags to ensure the new tag is shown if it was changed
        //     // TODO
        //     // RenderTags();
        // }
        //
        // // Do an empty search to fill the void, prevents small hitch on load
        // RequestRefresh();
    }
    
    public ISearchPanel GetSelectedPanel()
    {
        return _panelButtonView.SelectedPanel;
    }
    
    public bool IsPanelSelected(ISearchPanel panel)
    {
        return GetSelectedPanel().Id == panel.Id;
    }
    
    public IRequiredItemInfo? GetSelectedSpawnable()
    {
        return _itemButtonView.SelectedItem;
    }

    public void RequestRefresh()
    {
        var panel = GetSelectedPanel();
        panel.Query = _searchQuery;
        panel.RequestSearch(this);
    }

    public void RenderAll()
    {
        // Skip rendering if we left the search page before render gets called
        if (!IsSearchActive())
            return;

        _panelButtonView.Render();
        _itemButtonView.Render();
        _infoBox.Render();
    }

    public void ChangePanelPage(int offset)
    {
        var selectedPage = GetSelectedPanel();
        selectedPage.ChangePage(this, offset);
    }

    public void OnSelectItem(int idx)
    {
        SetIsEditing(false);
        
        var panel = GetSelectedPanel();
        if (panel.CanSelect)
            _itemButtonView.SelectItem(idx);
        
        // Update the infobox
        _infoBox.SetContent(GetSelectedSpawnable());
        
        // Update tags to ensure any forced highlights are updated
        RenderAll();
    }

    public void SelectCategory(int idx)
    {
        // Get the new selected panel instead of the one currently selected
        var result = _panelButtonView.SelectPage(idx);

        if (!result)
            return;
        
        // Update the item buttons to the new panel
        _itemButtonView.SetPanel(_panelButtonView.SelectedPanel);

        // Force out of edit mode if we switch tabs
        SetIsEditing(false);

        // Clear the query so the user doesn't get an empty screen
        _searchQuery = "";
        _keyboard.SetText(_searchQuery, false);

        // Update everything to reflect the new selected panel
        RequestRefresh();
    }

    public void ChangeTagPage(int offset)
    {
        _panelButtonView.OffsetPage(offset);
        RenderAll();
    }

    public void SwapSortButton()
    {
        var selectedPage = GetSelectedPanel();
        selectedPage.SelectedOrderIndex = (selectedPage.SelectedOrderIndex + 1) % selectedPage.SupportedOrders.Length;
        RequestRefresh();
    }

    public void OnFavoriteButton()
    {
        _infoBox.OnQuickAction();
    }

    public void Show()
    {
        // Show keyboard first
        ShowKeyboard();
        // Render these to prevent flickers
        RenderAll();
        RequestRefresh();
    }

    public void Hide()
    {
        CloseKeyboard();
        
        // Clear button images
        _itemButtonView.Reset();
        _panelButtonView.Reset();
        _infoBox.Reset();
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
        return PanelView._selectedTabIndex == SearchTabIndex;
    }

    public bool Is(SpawnablesPanelView panelView)
    {
        if (PanelView == null || panelView == null)
            return false;

        if (PanelView.gameObject == null)
            return false;

        return PanelView.gameObject.GetInstanceID() == panelView.gameObject.GetInstanceID();
    }
}