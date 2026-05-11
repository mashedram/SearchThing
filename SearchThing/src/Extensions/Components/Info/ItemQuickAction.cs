using Il2CppSLZ.UI;
using MelonLoader;
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
    private readonly Image _fadedButtonImage = null!;
    private readonly Image _favoriteButtonImage = null!;
    
    // Data
    private IQuickActionItemInfo? _quickActionInfo;

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
    
    public (Sprite? sprite, Color? color) GetFavoriteSprite(IRequiredItemInfo selectedItem)
    {
        if (_quickActionInfo == null)
            return (null, null);
            
        return (_quickActionInfo.GetActionIcon(_parent, selectedItem), _quickActionInfo.GetActionHighlight(_parent, selectedItem));
    }

    public void Render(IRequiredItemInfo selectedItem)
    {
        if (_quickActionInfo == null)
        {
            Reset();
            return;
        }
        
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
    
    public void SetQuickActionInfo(IQuickActionItemInfo? info)
    {
        _quickActionInfo = info;
    }

    public void Reset()
    {
        _fadedButtonImage.sprite = _originalFavoriteSprite;
        _favoriteButtonImage.sprite = _originalFavoriteSprite;
        _favoriteButtonImage.color = _originalFavoriteColor;
    }
    
    public void CallQuickAction(IRequiredItemInfo itemInfo)
    {
        _quickActionInfo?.PerformQuickAction(_parent, itemInfo);
    }
}