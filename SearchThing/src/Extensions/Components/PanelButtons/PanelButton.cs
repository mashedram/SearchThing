using Il2CppSLZ.UI;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SearchThing.Extensions.Components.PanelButtons;

public class PanelButton
{
    // Cached default values
    private static Color? _defaultHighlightColor = null!;

    // Value references
    private readonly GameObject _button;
    private readonly TextMeshPro _text;
    private readonly Image _highlight;

    public PanelButton(ButtonReferenceHolder button)
    {
        // Cache default values
        _defaultHighlightColor ??= button.highlight.color;

        // Store references
        _button = button.gameObject;
        _text = button.tmp;
        _highlight = button.highlight;
    }

    public void SetTag(string tag, bool isSelected = false, Color? color = null)
    {
        _button.SetActive(true);

        _text.text = tag;
        _highlight.enabled = isSelected;
        _highlight.color = color ?? _defaultHighlightColor!.Value;
    }

    public void Hide()
    {
        _button.SetActive(false);
    }

    public void Reset()
    {
        _highlight.color = _defaultHighlightColor!.Value;
    }
}