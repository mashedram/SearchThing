using Il2CppCysharp.Threading.Tasks;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.SceneStreaming;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.UI;
using Il2CppTMPro;
using LabFusion.Extensions;
using MelonLoader;
using SearchThing.Extensions.Components;
using SearchThing.Extensions.Pages;
using SearchThing.Extensions.Panel;
using SearchThing.Patches;
using SearchThing.Presets;
using SearchThing.Search;
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
    
    // Item List
    private readonly List<ItemButton> _itemButtons = new List<ItemButton>();
    private GameObject _itemPageNextButton;
    private GameObject _itemPagePreviousButton;
    
    // Tag List
    private readonly List<TagButton> _tagButtons = new List<TagButton>();
    private GameObject _tagPageNextButton;
    private GameObject _tagPagePreviousButton;

    private static readonly Sprite TabIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.SearchIcon.png");
    private static readonly Sprite PresetAddIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.AddIcon.png");
    private static readonly Sprite PresetRemoveIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.RemoveIcon.png");
    private static readonly Sprite EditIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.EditIcon.png");
    
    // Editing
    private SpawnInfoFocus _currentFocus = SpawnInfoFocus.SelectedItem;
    private string _editValue = "";
    private bool _isEditing;
    // Favorite button
    private Image _fadedButtonImage = null!;
    private Image _favoriteButtonImage = null!; 
    private Sprite _originalFavoriteSprite = null!;
    // There is no easily stealable default behavior for the sorting button, so we need to get references to it
    private TextMeshPro _sortButtonText = null!;
    private GameObject _sortButtonObject = null!;
    
    // The page currently selected in the tag page, but not used for rendering in case a rerender gets called when the page changes but selected tag doesn't
    private int _renderedTagPageIndex;
    // We need the selected tag in a prefix context, so we need to store it
    private int _selectedItemIndex;
    private int _selectedTagIndex;
    private int _selectedPageIndex;
    private readonly SpawnablePageProvider _pages = new SpawnablePageProvider();
    
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
        searchTabButton.onClick.AddListener((UnityAction)OnSearchTabClicked);

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

    private void OnSearchTabClicked()
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
    
    private void FetchTagButtons()
    {
        foreach (var panelViewTagButton in _panelView.treeButtons)
        {
            if (panelViewTagButton == null)
                continue;

            _tagButtons.Add(new TagButton(panelViewTagButton));
        }

        _tagPageNextButton = _panelView.treeScrollDownButton.gameObject;
        _tagPagePreviousButton = _panelView.treeScrollUpButton.gameObject;
    }
    
    private void FetchItemButtons()
    {
        foreach (var panelViewItemButton in _panelView.itemButtons)
        {
            if (panelViewItemButton == null)
                continue;

            _itemButtons.Add(new ItemButton(panelViewItemButton));
        }

        _itemPageNextButton = _panelView.itemScrollDownButton.gameObject;
        _itemPagePreviousButton = _panelView.itemScrollUpButton.gameObject;
    }
    
    public SpawnablePanelExtension(SpawnablesPanelView panelView)
    {
        _panelView = panelView;
        
        AddTab();
        FetchSortButton();
        FetchFavoriteButton();
        FetchItemButtons();
        FetchTagButtons();
        
        // Find the buttonReference we want to use for our style
        var buttonStyleReference = _panelView.transform.Find("group_spawnSelect/section_SpawnablesList/grid_buttons/button_item_01").GetComponent<ButtonReferenceHolder>();
        
        _keyboard = new Keyboard.Keyboard(_panelView.gameObject, buttonStyleReference);
        _keyboard.OnTextChanged += OnSearchQueryChanged;
        _keyboard.Hide();
        
        // Prefetch to avoid the delay on first load
        RequestRefresh();
    }
    
    public bool IsPanelSelected(ISearchPanel page)
    {
        var selectedPage = GetSelectedPanel();
        return selectedPage.Id == page.Id;
    }

    private ISearchPage GetRenderPage()
    {
        var selectedPageIndex = _renderedTagPageIndex;
        if (selectedPageIndex < 0 || selectedPageIndex >= _pages.PageCount)
            return _pages.GetBasePage();
        
        return _pages.GetPage(selectedPageIndex);
    }
    
    private ISearchPanel? GetRenderPanel(int idx)
    {
        var page = GetRenderPage();
        if (idx < 0 || idx >= page.Panels.Count)
            return null;
        
        return page.Panels[idx];
    }

    private ISearchPage GetSelectedPage()
    {
        var selectedPageIndex = _selectedPageIndex;
        if (selectedPageIndex < 0 || selectedPageIndex >= _pages.PageCount)
            return _pages.GetBasePage();
        
        return _pages.GetPage(selectedPageIndex);
    }
    
    private ISearchPanel GetSelectedPanel(int idx = -1)
    {
        var page = GetSelectedPage();
        var selectedTagIndex = _selectedTagIndex;
        if (selectedTagIndex < 0 || selectedTagIndex >= page.Panels.Count)
            return page.Panels[0];
        
        return page.Panels[idx >= 0 ? idx : selectedTagIndex];
    }

    private void OnSearchQueryChanged(string query)
    {
        if (_isEditing)
        {
            _editValue = query;
            RenderTags();
            RenderFocus();
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
        if (isEditing == _isEditing)
            return;
        
        _isEditing = isEditing;
        var panel = GetSelectedPanel();
        // Enter editing
        if (_isEditing)
        {
            // Ensure we aren't assigning
            PresetManager.IsAssignmentMode = false;
            // Force the tag to know its in edit mode
            _editValue = panel.Tag;
            // We don't want to trigger events to prevent circular calls
            _keyboard.SetText(_editValue, false);
        }
        // Exit editing
        else
        {
            // Reset back to the search query
            _keyboard.SetText(_searchQuery, false);
            panel.OnTagEdited(this, _editValue);
            
            // Render tags to ensure the new tag is shown if it was changed
            RenderTags();
        }
        
        RenderFavoriteButton();
    }

    public void RequestRefresh()
    {
        var panel = GetSelectedPanel();
        panel.Query = _searchQuery;
        panel.RequestSearch(this);
    }
    
    public ISearchableCrate? GetSelectedSpawnable()
    {
        var selectedPanel = GetSelectedPanel();
        
        if (_selectedItemIndex is < 0 or >= ISearchPanel.PanelSize)
            return null;
        
        return selectedPanel.GetCrateAt(_selectedItemIndex);
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
    
    public void RenderTags()
    {
        var selectedCrate = GetSelectedSpawnable();
        // We need to manually set the outline, because we skip the method that does so internally
        for (var i = 0; i < _tagButtons.Count; i++)
        {
            var button = _tagButtons[i];
            var panel = GetRenderPanel(i);
            
            if (panel == null)
            {
                button.Hide();
                continue;
            }
            
            var isSelected = i == _selectedTagIndex && _selectedPageIndex == _renderedTagPageIndex;
            var tag = (_isEditing && _currentFocus == SpawnInfoFocus.SelectedPage && isSelected) 
                ? _editValue 
                : panel.Tag;
            var forceHighlight = panel.IsForceHighlighted(this, selectedCrate);
            
            button.SetTag(tag, isSelected || forceHighlight != null, forceHighlight);
        }
        
        var pageCount = GetRenderPage().Panels.Count;
        _panelView.treePageText .text = $"{_renderedTagPageIndex + 1}/{pageCount}";
        _tagPageNextButton.SetActive(_renderedTagPageIndex < pageCount - 1);
        _tagPagePreviousButton.SetActive(_renderedTagPageIndex > 0);
    }

    public void RenderSpecialButtons(ISearchPanel? selectedPanel = null)
    {
        selectedPanel ??= GetSelectedPanel();
        
        // Update sort button
        if (selectedPanel.SupportedOrders.Length > 1)
        {
            _sortButtonObject.SetActive(true);
            _sortButtonText.gameObject.SetActive(true);
            
            var order = selectedPanel.SupportedOrders[selectedPanel.SelectedOrderIndex];
            _sortButtonText.text = order.Name;
            // We do what FetchSortButton does every tick in case a tab switch undoes the left alignment
            _sortButtonText.alignment = TextAlignmentOptions.Left;
        }
        else
        {
            _sortButtonObject.SetActive(false);
            _sortButtonText.gameObject.SetActive(false);
        }
        
        RenderFavoriteButton();
    }

    public (Sprite, bool) GetFavoriteSprite()
    {
        if (_currentFocus == SpawnInfoFocus.SelectedPage)
        {
            return (EditIcon, _isEditing);
        }

        // If we are in a preset assignment mode, we want to show the remove icon if the selected item is already assigned to the preset, and the add icon if it isn't
        // TODO : Make this not hardcoded
        if (_selectedPageIndex > 0)
            return (PresetRemoveIcon, PresetManager.IsAssignmentMode);
        
        return (PresetAddIcon, PresetManager.IsAssignmentMode);
    }

    public void RenderFavoriteButton()
    {
        var isFavorite = _panelView.selectedObject != null && _panelView.favoriteCrates.ContainsKey(_panelView.selectedObject._barcode._id);
        if (!IsSearchActive())
        {
            // Toggle buttons
            _fadedButtonImage.enabled = !isFavorite;
            _favoriteButtonImage.enabled = isFavorite;
            
            // Reset sprite in case we were assignment mode or edit mode
            _favoriteButtonImage.sprite = _originalFavoriteSprite;
            _fadedButtonImage.sprite = _originalFavoriteSprite;
            
            return;
        }
        
        var favoriteSprite = GetFavoriteSprite();

        var overrideSprite = favoriteSprite.Item1;
        var isToggledOn = favoriteSprite.Item2;
        
        // Assign values
        _fadedButtonImage.enabled = !isToggledOn;
        _favoriteButtonImage.enabled = isToggledOn;
        // This also ensures sprite isn't null
        _fadedButtonImage.sprite = overrideSprite;
        _favoriteButtonImage.sprite = overrideSprite;
    }

    public void RenderFocus()
    {
        if (_currentFocus == SpawnInfoFocus.SelectedItem)
        {
            var selectedCrate = GetSelectedSpawnable();

            if (selectedCrate == null)
            {
                _panelView.selectedTitle.text = "";
                _panelView.selectedDescription.text = "";
                _panelView.selectedAuthor.text = "";
                _panelView.selectedPallet.text = "";
                _panelView.selectedTags.text = "";
            }
            else
            {
                _panelView.selectedTitle.text = selectedCrate.Name.Original;
                _panelView.selectedDescription.text = selectedCrate.Description;
                _panelView.selectedPallet.text = selectedCrate.PalletName.Original;
                _panelView.selectedAuthor.text = $"{selectedCrate.Author.Original}";
                _panelView.selectedTags.text = string.Join(", ", selectedCrate.Tags.Select(t => t.Original));
            }
        }
        // The page is selected
        else
        {
            var selectedPanel = GetSelectedPanel();
            _panelView.selectedTitle.text = _isEditing ? _editValue : selectedPanel.Tag;
            _panelView.selectedDescription.text = "N/A";
            _panelView.selectedAuthor.text = "N/A";
            _panelView.selectedPallet.text = "N/A";
            _panelView.selectedTags.text = "N/A";
        }
        
        RenderFavoriteButton();
    }

    public void RenderItemButtons()
    {
        var selectedPanel = GetSelectedPanel();
        
        var entries = selectedPanel
            .GetPage(selectedPanel.Page);
        
        if (_selectedItemIndex >= entries.Count)
        {
            _selectedItemIndex = entries.Count - 1;
            RenderFocus();
        }
        
        for (var i = 0; i < _itemButtons.Count; i++)
        {
            var button = _itemButtons[i];
            if (i < entries.Count)
            {
                var entry = entries[i];
                button.SetCrate(entry, _selectedItemIndex == i);
            }
            else
            {
                button.Hide();
            }
        }

        var pageCount = selectedPanel.PageCount;
        _panelView._numberOfPages = pageCount;

        _panelView.labelText.text = _searchQuery;
        _panelView.itemPageText.text = $"{selectedPanel.Page + 1}/{pageCount}";
        
        _itemPageNextButton.SetActive(selectedPanel.Page < selectedPanel.PageCount - 1);
        _itemPagePreviousButton.SetActive(selectedPanel.Page > 0);
    }
    
    public void RenderAll()
    {
        // Skip rendering if we left the search page before render gets called
        if (!IsSearchActive())
            return;
        
        // Render tags page
        RenderTags();
        // Render page
        RenderItemButtons();
        RenderFocus();
        RenderSpecialButtons();
    }

    public void ChangePanelPage(int offset)
    {
        var selectedPage = GetSelectedPanel();
        selectedPage.ChangePage(this, offset);
    }

    private void AssignSpawnableCrate(SpawnableCrate spawnableCrate)
    {
        SpawnGunPatches.SelectCrate(spawnableCrate);
    }
    
    private void AssignAvatarCrate(Scannable avatarCrate)
    {
        var reference = new AvatarCrateReference(avatarCrate._barcode);
        var cordDevice = BodylogAccessor.GetCordDevice();
        if (cordDevice != null)
        {
            cordDevice.SwapAvatar(reference).Forget();
        }
    }
    
    private void LoadLevelCrate(Scannable levelCrate)
    {
        var reference = new LevelCrateReference(levelCrate._barcode);
        SceneStreamer.LoadAsync(reference).Forget();
    }

    private void AssignCrate()
    {
        var selectedCrate = GetSelectedSpawnable();
        if (selectedCrate == null)
            return;
        if (!selectedCrate.TryGetCrate(out var crate))
            return;
        
        var selectedAvatarCrate = crate.TryCast<AvatarCrate>();
        if (selectedAvatarCrate != null)
        {
            AssignAvatarCrate(selectedAvatarCrate);
            return;
        }
        
        var spawnableCrate = crate.TryCast<SpawnableCrate>();
        if (spawnableCrate != null) {
            AssignSpawnableCrate(spawnableCrate);
        }
        
        var levelCrate = crate.TryCast<LevelCrate>();
        if (levelCrate != null)
        {
            LoadLevelCrate(levelCrate);
        }
    }

    public void OnSelectItem(int idx)
    {
        // Test name overwrite
        _currentFocus = SpawnInfoFocus.SelectedItem;
        _selectedItemIndex = idx;
        
        SetIsEditing(false);
        
        // Update tags to ensure any forced highlights are updated
        RenderItemButtons();
        RenderTags();
        RenderFocus();
        
        AssignCrate();
    }
    
    public void SelectCategory(int idx)
    {
        // Get the new selected panel instead of the one currently selected
        var result = GetRenderPanel(idx)?.OnSelected(this) ?? false;

        if (!result)
            return;

        // Test name overwrite
        _currentFocus = SpawnInfoFocus.SelectedPage;
        
        // Force out of edit mode if we switch tabs
        SetIsEditing(false);
        
        // We've selected a new tag, so we need to update what we have selected to what we rendered
        _selectedPageIndex = _renderedTagPageIndex;
        _selectedTagIndex = idx;
        
        // Clear the query so the user doesn't get an empty screen
        _searchQuery = "";
        _keyboard.SetText(_searchQuery, false);
        
        // Update everything to reflect the new selected panel
        RequestRefresh();
    }

    public void ChangeTagPage(int offset)
    {
        var newPage = _renderedTagPageIndex + offset;
        if (newPage < 0 || newPage >= _pages.PageCount)            
            return;
        _renderedTagPageIndex = newPage;
        
        RenderTags();
    }
    
    public void SwapSortButton()
    {
        var selectedPage = GetSelectedPanel();
        selectedPage.SelectedOrderIndex = (selectedPage.SelectedOrderIndex + 1) % selectedPage.SupportedOrders.Length;
        RequestRefresh();
    }
    
    public void OnFavoriteButton()
    {
        if (_currentFocus == SpawnInfoFocus.SelectedItem)
        {
            var selectedPanel = GetSelectedPanel();
            if (selectedPanel is Preset preset)
            {
                PresetManager.IsAssignmentMode = false;
                var selectedCrate = GetSelectedSpawnable();
                if (selectedCrate != null)
                    preset.RemoveCrate(selectedCrate);
                
                RequestRefresh();
            }
            else
            {
                PresetManager.IsAssignmentMode = !PresetManager.IsAssignmentMode;
        
                // Set the rendertarget to page 1 so we are at the presets if we aren't yet
                if (_renderedTagPageIndex < 1)
                    _renderedTagPageIndex = 1;
                
                RenderAll();
            }
        }
        else if (GetSelectedPanel().TagEditable)
        {
            SetIsEditing(!_isEditing);
            
            RenderFavoriteButton();
        }
    }

    public void Show()
    {
        // Show keyboard first
        ShowKeyboard();
        // Render these to prevent flickers
        RenderTags();
        RenderSpecialButtons();
        RequestRefresh();
    }

    public void Hide()
    {
        CloseKeyboard();
        RenderFavoriteButton();
        
        // Assign the original sprite back in case we were in a special mode when hiding
        _fadedButtonImage.sprite = _originalFavoriteSprite;
        _favoriteButtonImage.sprite = _originalFavoriteSprite;
        
        // Clear button images
        foreach (var itemButton in _itemButtons)
        {
            itemButton.Reset();
        }
        // Clear tag colors
        foreach (var tagButton in _tagButtons)
        {
            tagButton.Reset();
        }
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