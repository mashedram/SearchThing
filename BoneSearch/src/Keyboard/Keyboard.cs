using Il2CppTMPro;
using Il2CppSLZ.Bonelab;
using UnityEngine;
using UnityEngine.UI;

namespace BoneSearch.Keyboard;

public class Keyboard
{
    private static readonly Vector2 KeySize = new Vector2(80, 80);
    private static readonly Vector2 KeySpacing = new Vector2(10, 10);
    
    private const int KeyboardLayer = 5;
    
    private readonly GameObject _parent;
    private GameObject? _keyboardRoot;
    private string _text = "";
    public event Action<string>? OnTextChanged;

    private readonly string[] _keyRows =
    {
        "1234567890",
        "QWERTYUIOP",
        "ASDFGHJKL",
        "ZXCVBNM"
    };

    public Keyboard(GameObject parent)
    {
        _parent = parent;
        Create();
    }

    public string Text => _text;

    public void SetText(string text)
    {
        _text = text;
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

        var button = keyGo.AddComponent<Button>();
        var image = keyGo.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        var outline = keyGo.AddComponent<Outline>();
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(2, -2);

        var colliderGo = new GameObject("Collider");
        colliderGo.transform.SetParent(keyGo.transform, false);
        var collider = colliderGo.AddComponent<BoxCollider>();
        var sizeWithMargin = size + margin.GetValueOrDefault();
        collider.size = new Vector3(sizeWithMargin.x, sizeWithMargin.y, 1);
        collider.isTrigger = true;

        keyGo.AddComponent<ButtonHoverClick>();

        button.onClick.AddListener(action);
        
        
        button.transition = Selectable.Transition.ColorTint;
        button.targetGraphic = image;
        var colors = button.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 1f);
        colors.highlightedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        button.colors = colors;

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(keyGo.transform, false);
        var textComponent = textGo.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
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

        for (var i = 0; i < _keyRows.Length; i++)
        {
            var row = _keyRows[i];
            var rowWidth = (row.Length * KeySize.x) + ((row.Length - 1) * KeySpacing.x);
            var startX = -rowWidth / 2f;

            for (var j = 0; j < row.Length; j++)
            {
                var keyChar = row[j];
                var position = new Vector2(startX + j * (KeySize.x + KeySpacing.x) + KeySize.x / 2, currentY);
                CreateKey(keyChar.ToString(), position, KeySize, () => OnKeyPress(keyChar));
            }
            currentY -= (KeySize.y + KeySpacing.y);
        }

        // Add special keys
        CreateKey("Backspace", new Vector2(-280, currentY), new Vector2(240, 60), OnBackspace);
        CreateKey("Space", new Vector2(0, currentY), new Vector2(249, 60), OnSpace);
        CreateKey("Clear", new Vector2(280, currentY), new Vector2(240, 60), () => SetText(""));
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