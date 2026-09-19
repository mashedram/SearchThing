using Il2CppSLZ.UI;
using Il2CppTMPro;
using SearchThing.Extensions.Panel.Data;
using SearchThing.Extensions.Panel.Data.Extensions;
using SearchThing.Search.Data;
using SearchThing.Search.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace SearchThing.Extensions.Components.ItemButtons;

public class ItemButton
{
    // Default value cache
    private static Sprite? _defaultIcon;
    private static Color? _defaultIconColor;
    private static Color? _defaultHighlightColor;
    private static Color? _defaultTextColor;

    // Parent references
    private readonly SpawnablePanelExtension _parentPanel;
    private readonly int _index;

    // Value references
    private readonly GameObject _button;
    private readonly TextMeshPro _text;
    private readonly Image _highlight;
    private readonly Image _icon;

    // Helper values
    private bool _isSelected;

    // Getters
    public IItemRenderInfo? RenderInfo { get; private set; }
    public Guid Id => RenderInfo?.GetSource().Id ?? Guid.Empty;
    public bool IsVisible { get; private set; }

    public ItemButton(SpawnablePanelExtension parentPanel, ButtonReferenceHolder button, int idx)
    {
        _parentPanel = parentPanel;
        _index = idx;

        // Cache default values
        
        // Unity check, ??= can't be used
        if (_defaultIcon == null) 
            _defaultIcon = button.special.sprite;
        
        _defaultIconColor ??= button.special.color;
        _defaultHighlightColor ??= button.highlight.color;
        _defaultTextColor ??= button.tmp.color;
        
        // Store references
        _button = button.gameObject;
        _text = button.tmp;
        _highlight = button.highlight;
        _icon = button.special;
    }

    public void SetCrate(IItemRenderInfo itemInfo, ItemButtonView view)
    {
        RenderInfo = itemInfo;
        IsVisible = true;
        _button.SetActive(true);

        var source = itemInfo.GetSource();
        
        _text.text = source.Name;
        if (itemInfo.TryGetExtension<IconExtension>(out var iconExtension))
        {
            _icon.enabled = true;
            _icon.sprite = iconExtension.Icon;
            _icon.color = Color.white;
        }
        else
        {
            _icon.enabled = false;
            _icon.sprite = _defaultIcon;
            _icon.color = _defaultIconColor!.Value;
        }

        _text.color = itemInfo.TryGetExtension<LabelColorExtension>(out var colorProvider)
            ? colorProvider.Color 
            : _defaultTextColor!.Value;

        _isSelected = view.SelectedItem != null && view.SelectedItem.Id == Id;
        _highlight.enabled = _isSelected;
    }

    public bool OnSelected()
    {
        if (RenderInfo?.GetSource() is not ICrateBoundItemInfo crateBoundItemInfo)
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
        _text.color = _defaultTextColor!.Value;
    }
}