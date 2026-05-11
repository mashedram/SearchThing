using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Extensions.Components.Info;
using SearchThing.Extensions.Components.ItemButtons;
using SearchThing.Search.CrateData;
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
    public string Description { get; init; }
    public IEnumerable<string> Tags { get; init; }
    public string PalletName { get; init; }
    public string Author { get; init; }

    public Barcode? Barcode { get; init; }

    public ItemRender(IRequiredItemInfo crate)
    {
        Crate = crate;

        Id = crate.Id;
        Name = crate.Name;
        Redacted = crate.Redacted;
        DateAdded = crate.DateAdded;

        if (crate is IDescriptiveItemInfo descriptive)
        {
            Description = descriptive.Description;
            Tags = descriptive.Tags;
        }
        else
        {
            Description = string.Empty;
            Tags = Array.Empty<string>();
        }

        if (crate is ICreatorItemInfo creator)
        {
            PalletName = creator.PalletName;
            Author = creator.Author;
        }
        else
        {
            PalletName = string.Empty;
            Author = string.Empty;
        }

        if (crate is ICrateBoundItemInfo bound)
        {
            Barcode = bound.Barcode;
        }
        else
        {
            Barcode = null;
        }

        Icon = crate switch
        {
            ICrateIconProvider iconProvider => iconProvider.Icon,
            ICrateTypeItemInfo typeItemInfo => CrateIconProvider.GetIcon(typeItemInfo),
            _ => CrateIconProvider.GetDefaultIcon()
        };
    }
}