using SearchThing.Extensions.Components.ItemButtons;
using SearchThing.Search.Data;
using SearchThing.Util;
using UnityEngine;

namespace SearchThing.Extensions.Panel.Data.Extensions;

public class IconExtension : IItemRenderExtension
{
    public Sprite Icon { get; }

    public IconExtension(Sprite icon)
    {
        Icon = icon;
    }
    
    public IconExtension(IRequiredItemInfo crate)
    {
        Icon = crate switch
        {
            ICrateTypeItemInfo typeItemInfo => CrateIconProvider.GetIcon(typeItemInfo),
            _ => CrateIconProvider.GetDefaultIcon()
        };
    }
}