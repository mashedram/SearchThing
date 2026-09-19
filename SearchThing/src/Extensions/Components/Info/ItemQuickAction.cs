using Il2CppSLZ.UI;
using MelonLoader;
using SearchThing.Extensions.Panel.Data;
using SearchThing.Extensions.Panel.Data.Extensions;
using SearchThing.Search.Data;
using UnityEngine;
using UnityEngine.UI;

namespace SearchThing.Extensions.Components.Info;

public class ItemQuickAction
{
    private SpawnablePanelExtension _parent;

    // References to original state
    private readonly Sprite _originalFavoriteSprite = null!;
    private readonly Color _originalFavoriteColor = Color.white;

    // Renderers
    private readonly GameObject _buttonObject = null!;
    private readonly Image _fadedButtonImage = null!;
    private readonly Image _favoriteButtonImage = null!;

    // Data
    private ActionExtension? _actionExtension;

    public ItemQuickAction(SpawnablePanelExtension extension)
    {
        _parent = extension;

        var favoriteButton = extension.PanelView.transform.Find("group_selectedInfo/button_Favorite");
        if (favoriteButton == null)
        {
            MelonLogger.Error("Failed to find favorite button.");
            return;
        }

        var references = favoriteButton.GetComponent<ButtonReferenceHolder>();
        _buttonObject = references.gameObject;
        _fadedButtonImage = references.highlight;
        _favoriteButtonImage = references.special;
        if (_favoriteButtonImage == null || _fadedButtonImage == null)
        {
            MelonLogger.Error("Failed to find Image component in favorite button.");
            return;
        }

        _originalFavoriteSprite = _favoriteButtonImage.sprite;
        _originalFavoriteColor = _favoriteButtonImage.color;
    }

    public (Sprite? sprite, Color? color) GetFavoriteSprite(IItemRenderInfo selectedItem)
    {
        if (_actionExtension == null)
            return (null, null);

        return (_actionExtension.GetActionIcon(_parent, selectedItem), _actionExtension.GetActionHighlight(_parent, selectedItem));
    }

    public void Render(IItemRenderInfo? selectedItem)
    {
        if (_actionExtension == null || selectedItem == null)
        {
            _buttonObject.SetActive(false);
            return;
        }
        
        _buttonObject.SetActive(true);

        var favoriteSprite = GetFavoriteSprite(selectedItem);
        var overrideSprite = favoriteSprite.sprite;
        var isVisible = overrideSprite != null;

        var highlightColor = favoriteSprite.color;
        var isHighlightOn = highlightColor != null;

        // Assign values
        _fadedButtonImage.enabled = !isHighlightOn && isVisible;
        _favoriteButtonImage.enabled = isHighlightOn && isVisible;
        // This also ensures sprite isn't null
        _fadedButtonImage.sprite = overrideSprite;
        _favoriteButtonImage.sprite = overrideSprite;
        // And assign the highlight color if we have one
        if (isHighlightOn)
            _favoriteButtonImage.color = highlightColor!.Value;
    }

    public void SetQuickActionInfo(ActionExtension? info)
    {
        _actionExtension = info;
    }

    public void Reset()
    {
        _buttonObject.SetActive(true);
        
        _fadedButtonImage.sprite = _originalFavoriteSprite;
        _favoriteButtonImage.sprite = _originalFavoriteSprite;
        _favoriteButtonImage.color = _originalFavoriteColor;
    }

    public void CallQuickAction(IItemRenderInfo itemInfo)
    {
        _actionExtension?.PerformQuickAction(_parent, itemInfo);
    }
}