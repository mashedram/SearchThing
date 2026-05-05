using Il2CppSLZ.UI;
using Il2CppTMPro;
using SearchThing.Search;
using SearchThing.Util;
using UnityEngine;
using UnityEngine.UI;

namespace SearchThing.Extensions.Components;

public class ItemButton
{
    // Default value cache
    private static Sprite? _defaultIcon = null!;
    private static Color? _defaultIconColor = null!;
    private static Color? _defaultHighlightColor = null!;
    
    // Value references
    private GameObject _button;
    private TextMeshPro _text;
    private Image _highlight;
    private Image _icon;
    
    public ItemButton(ButtonReferenceHolder button)
    {
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
    
    public void SetCrate(ISearchableCrate crate, bool isSelected = false)
    {
        _button.SetActive(true);
        
        _text.text = crate.Name.Original;
        _icon.enabled = true;
        _icon.sprite = CrateIconProvider.GetIcon(crate);
        _icon.color = Color.white;
        _highlight.enabled = isSelected;
    }
    
    public void Hide()
    {
        _button.SetActive(false);
    }

    public void Reset()
    {
        _highlight.color = _defaultHighlightColor!.Value;
        _icon.color = _defaultIconColor!.Value;
        _icon.sprite = _defaultIcon!;
    }
}