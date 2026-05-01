using Il2CppTMPro;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.UI;
using UnityEngine;
using UnityEngine.UI;

namespace SearchThing.Keyboard;

public class Keyboard
{
    private static readonly Vector2 KeySize = new Vector2(80, 80);
    private static readonly Vector2 KeySpacing = new Vector2(10, 10);
    
    private const int KeyboardLayer = 5;
    private static readonly string[] KeyRows =
    {
        "1234567890",
        "QWERTYUIOP",
        "ASDFGHJKL",
        "ZXCVBNM"
    };
    
    private readonly GameObject _parent;
    private GameObject? _keyboardRoot;
    private string _text = "";
    private readonly TMP_FontAsset _font;
    private readonly Sprite _bg;
    private readonly Sprite _outlineBg;
    public event Action<string>? OnTextChanged;

    public Keyboard(GameObject parent, ButtonReferenceHolder resources)
    {
        _parent = parent;
        _font = resources.tmp.font;
        _bg = resources.background.sprite;
        _outlineBg = resources.highlight.sprite;
        Create();
    }

    public void SetText(string text, bool triggerEvent = true)
    {
        _text = text;
        if (triggerEvent)
            OnInternalTextChanged(_text);
    }

    private void Create()
    {
        _keyboardRoot = new GameObject("Keyboard");
        _keyboardRoot.transform.SetParent(_parent.transform, false);
        _keyboardRoot.transform.localPosition = new Vector3(-85f, -450f, 0);
        _keyboardRoot.transform.localRotation = Quaternion.Euler(0, 0, 0);
        _keyboardRoot.layer = KeyboardLayer;
        
        _keyboardRoot.AddComponent<CanvasRenderer>();

        var background = _keyboardRoot.AddComponent<Image>();
        background.color = Color.clear;

        var rectTransform = _keyboardRoot.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.offsetMin = new Vector2(10, 10);
        rectTransform.offsetMax = new Vector2(-10, -10);
 
        CreateKeys();
    }

    // TODO : Maybe make a class that prepares a key with references that we just clone instead of this abomination lol
    private void CreateKey(string text, Vector2 position, Vector2 size, Action action, Vector2? margin = null)
    {
        if (_keyboardRoot == null)
        {
            throw new InvalidOperationException("Keyboard root is not initialized.");
        }

        var keyGo = new GameObject($"Key_{text}");
        keyGo.transform.SetParent(_keyboardRoot.transform, false);
        var keyRect = keyGo.AddComponent<RectTransform>();
        keyGo.AddComponent<CanvasRenderer>();

        var bgGo = new GameObject("bg");
        bgGo.transform.SetParent(keyGo.transform, false);
        var bgRect = bgGo.AddComponent<RectTransform>();
        bgRect.sizeDelta = size;
        var bg = bgGo.AddComponent<Image>();
        bg.sprite = _bg;
        bg.type = Image.Type.Sliced;

        var outlineGo = new GameObject("image_backline");
        outlineGo.transform.SetParent(keyGo.transform, false);
        var outlineRect = outlineGo.AddComponent<RectTransform>();
        outlineRect.sizeDelta = size;
        var outline = outlineGo.AddComponent<Image>();
        outline.sprite = _outlineBg;
        outline.type = Image.Type.Sliced;
        outline.raycastTarget = false;
        outline.fillCenter = false;

        var colliderGo = new GameObject("Collider");
        colliderGo.transform.SetParent(keyGo.transform, false);
        var collider = colliderGo.AddComponent<BoxCollider>();
        var sizeWithMargin = size + margin.GetValueOrDefault();
        collider.size = new Vector3(sizeWithMargin.x, sizeWithMargin.y, 1);
        collider.isTrigger = true;
        
        var button = keyGo.AddComponent<Button>();
        button.onClick.AddListener(action);

        button.transition = Selectable.Transition.ColorTint;
        button.targetGraphic = bg;
        var colors = button.colors;
        colors.normalColor = new Color(0f, 0f, 0f, 0.6118f);
        colors.highlightedColor = new Color(0.6038f, 0.6038f, 0.6038f, 0.209f);
        colors.pressedColor = new Color(0.7176f, 0.7176f, 0.7176f, 1);
        // Prevent weird coloration because the button stays selected after press
        colors.selectedColor = colors.normalColor;
        button.colors = colors;
        
        
        keyGo.AddComponent<ButtonHoverClick>();
        
        var textGo = new GameObject("Text");
        textGo.transform.SetParent(keyGo.transform, false);
        var textComponent = textGo.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.font = _font;
        textComponent.fontSize = 40;
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.Center;

        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        keyRect.sizeDelta = size;
        keyRect.anchoredPosition = position;
        
        // Set layers to ensure the keys are interactable
        keyGo.layer = KeyboardLayer;
        colliderGo.layer = KeyboardLayer;
    }

    private void CreateKeys()
    {
        if (_keyboardRoot == null) return;

        float currentY = 0;

        foreach (var row in KeyRows)
        {
            var rowWidth = row.Length * KeySize.x + (row.Length - 1) * KeySpacing.x;
            var startX = -rowWidth / 2f;

            for (var j = 0; j < row.Length; j++)
            {
                var keyChar = row[j];
                var position = new Vector2(startX + j * (KeySize.x + KeySpacing.x) + KeySize.x / 2, currentY);
                CreateKey(keyChar.ToString(), position, KeySize, () => OnKeyPress(keyChar), KeySpacing / 2f);
            }
            currentY -= KeySize.y + KeySpacing.y;
        }

        // Add special keys
        CreateKey("Backspace", new Vector2(-280, currentY), new Vector2(240, 80), OnBackspace);
        CreateKey("Space", new Vector2(0, currentY), new Vector2(249, 80), OnSpace);
        CreateKey("Clear", new Vector2(280, currentY), new Vector2(240, 80), () => SetText(""));
    }

    private void OnInternalTextChanged(string text)
    {
        OnTextChanged?.Invoke(text);
    }

    private void OnKeyPress(char key)
    {
        _text += key;
        OnInternalTextChanged(_text);
    }

    private void OnBackspace()
    {
        if (_text.Length <= 0) 
            return;
        
        _text = _text[..^1];
        OnInternalTextChanged(_text);
    }

    private void OnSpace()
    {
        _text += " ";
        OnInternalTextChanged(_text);
    }

    public void Show()
    {
        if (_keyboardRoot == null) return;
        _keyboardRoot.SetActive(true);
        _keyboardRoot.transform.SetAsLastSibling(); // Ensure the keyboard is on top of other UI elements
        _keyboardRoot.transform.localPosition = new Vector3(-85f, -450f, 0); // Reset position to ensure it's in the correct place
    }

    public void Hide()
    {
        if (_keyboardRoot == null) return;
        _keyboardRoot.SetActive(false);
    }
}