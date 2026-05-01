using UnityEngine;

namespace SearchThing.Util;

public static class ImageHelper
{
    public static Texture2D LoadEmbeddedImage(string resourceName)
    {
        var assembly = typeof(ImageHelper).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) 
        {
            throw new Exception($"Resource not found: {resourceName}");
        }
            
        var buffer = new byte[stream.Length];
        _ = stream.Read(buffer, 0, buffer.Length);
            
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
        {
            name = resourceName,
            hideFlags = HideFlags.DontUnloadUnusedAsset
        };
        
        if (!texture.LoadImage(buffer))
        {
            throw new Exception($"Failed to load image from resource: {resourceName}");
        }
        
        texture.Apply();
        return texture;
    }
    
    public static Sprite LoadEmbeddedSprite(string resourceName)
    {
        var texture = LoadEmbeddedImage(resourceName);
        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        sprite.hideFlags = HideFlags.DontUnloadUnusedAsset;
        return sprite;
    }
}