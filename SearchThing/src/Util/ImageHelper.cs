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
}