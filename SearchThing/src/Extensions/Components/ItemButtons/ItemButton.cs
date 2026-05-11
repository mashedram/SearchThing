using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.UI;
using Il2CppTMPro;
using SearchThing.Extensions.Components.Info;
using SearchThing.Extensions.Panel.Data;
using SearchThing.Search.CrateData;
using SearchThing.Search.Data;
using SearchThing.Search.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace SearchThing.Extensions.Components.ItemButtons;

public class ItemButton
{
    // Default value cache
    private static Sprite? _defaultIcon = null!;
    private static Color? _defaultIconColor = null!;
    private static Color? _defaultHighlightColor = null!;

    // Parent references
    private SpawnablePanelExtension _parentPanel;
    private int _index;
    
    // Value references
    private readonly GameObject _button;
    private readonly TextMeshPro _text;
    private readonly Image _highlight;
    private readonly Image _icon;
    
    // Helper values
    private bool _isSelected = false;

    // Getters
    public IRequiredItemInfo? ItemInfo { get; private set; }
    public Guid Id => ItemInfo?.Id ?? Guid.Empty;
    public bool IsVisible { get; private set; }

    public ItemButton(SpawnablePanelExtension parentPanel, ButtonReferenceHolder button, int idx)
    {
        _parentPanel = parentPanel;
        _index = idx;
        
        // Cache default values
        if (_defaultIcon == null) _defaultIcon = button.special.sprite;
        _defaultIconColor ??= button.special.color;
        _defaultHighlightColor ??= button.highlight.color;

        // Store references
        _button = button.gameObject;
        _text = button.tmp;
        _highlight = button.highlight;
        _icon = button.special;
    }

    public void SetCrate(IRequiredItemInfo itemInfo, bool isSelected = false)
    {
        _isSelected = isSelected;
        ItemInfo = itemInfo;
        IsVisible = true;
        _button.SetActive(true);

        _text.text = itemInfo.Name;
        if (itemInfo is ICrateIconProvider iconProvider)
        {
            _icon.enabled = true;
            _icon.sprite = iconProvider.Icon;
            _icon.color = Color.white;
        }
        else
        {
            _icon.enabled = false;
            _icon.sprite = _defaultIcon;
            _icon.color = _defaultIconColor!.Value;
        }
        _highlight.enabled = isSelected;
    }

    public bool OnSelected()
    {
        if (ItemInfo is not ICrateBoundItemInfo crateBoundItemInfo)
            return true;
        
        switch (crateBoundItemInfo.Crate)
        {
            case null:
                break;
            // Only call select if not already selected
            case ISelectableCrate selectableCrate when !_isSelected:
                return selectableCrate.OnSelected(_parentPanel, _index);
            case IConfirmableCrate confirmableCrate when _isSelected:
                confirmableCrate.OnConfirmed(_parentPanel, _index);
                break;
        }
        
        return true;
    }

    public void Hide()
    {
        IsVisible = false;
        _button.SetActive(false);
    }

    public void Reset()
    {
        _highlight.color = _defaultHighlightColor!.Value;
        _icon.color = _defaultIconColor!.Value;
        _icon.sprite = _defaultIcon!;
    }
}