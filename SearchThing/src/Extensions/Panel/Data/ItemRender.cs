using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Extensions.Components.ItemButtons;
using SearchThing.Search.Data;
using SearchThing.Util;
using UnityEngine;

namespace SearchThing.Extensions.Panel.Data;

public class ItemRender : IDescriptiveItemInfo, ICreatorItemInfo, ICrateIconProvider, ICrateBoundItemInfo
{
    public IRequiredItemInfo Crate { get; }

    public Sprite Icon { get; init; }
    public Guid Id { get; }
    public string Name { get; init; }
    public bool Redacted { get; }
    public DateTime DateAdded { get; }
    public string Description => Crate is IDescriptiveItemInfo descriptive ? descriptive.Description : "No Description";
    public IEnumerable<string> Tags => Crate is ITaggedItemInfo taggedItemInfo ? taggedItemInfo.Tags : Array.Empty<string>();
    public string PalletName => Crate is ICreatorItemInfo creator ? creator.PalletName : "None";
    public string Author => Crate is ICreatorItemInfo creator ? creator.Author : "None";

    public Barcode? Barcode => Crate is ICrateBoundItemInfo bound ? bound.Barcode : null;

    public ItemRender(IRequiredItemInfo crate)
    {
        Crate = crate;

        Id = crate.Id;
        Name = crate.Name;
        Redacted = crate.Redacted;
        DateAdded = crate.DateAdded;


        Icon = crate switch
        {
            ICrateIconProvider iconProvider => iconProvider.Icon,
            ICrateTypeItemInfo typeItemInfo => CrateIconProvider.GetIcon(typeItemInfo),
            _ => CrateIconProvider.GetDefaultIcon()
        };
    }
}