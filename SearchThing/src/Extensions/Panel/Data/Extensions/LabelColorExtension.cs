using UnityEngine;

namespace SearchThing.Extensions.Panel.Data.Extensions;

public class LabelColorExtension : IItemRenderExtension
{
    public Color Color { get; }
    
    public LabelColorExtension(Color color)
    {
        Color = color;
    }
}