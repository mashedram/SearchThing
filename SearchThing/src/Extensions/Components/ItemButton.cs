using Il2CppSLZ.UI;
using Il2CppTMPro;
using SearchThing.Search;
using SearchThing.Util;
using UnityEngine;
using UnityEngine.UI;

namespace SearchThing.Extensions.Components;

public class ItemButton
{
    // Icons
    private static readonly Sprite AvatarIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.AvatarIcon.png");
    private static readonly Sprite CrateIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.CrateIcon.png");
    private static readonly Sprite LevelIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.LevelIcon.png");
    
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

    private Sprite GetIconForType(CrateType type)
    {
        return type switch
        {
            CrateType.Avatar => AvatarIcon,
            CrateType.Prop => CrateIcon,
            CrateType.Level => LevelIcon,
            _ => _defaultIcon!
        };
    }
    
    public void SetCrate(ISearchableCrate crate, bool isSelected = false)
    {
        _button.SetActive(true);
        
        _text.text = crate.Name.Original;
        _icon.enabled = true;
        _icon.sprite = GetIconForType(crate.CrateType);
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