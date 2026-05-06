using UnityEngine;

namespace SearchThing.Extensions.Panel.Data;

public class ItemRenderData
{
    public string Name { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    public string PalletName { get; set; }
    public Sprite? Icon { get; set; }
    public string[] Tags { get; set; }
}