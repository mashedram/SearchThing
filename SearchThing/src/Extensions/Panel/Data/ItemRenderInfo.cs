using System.Diagnostics.CodeAnalysis;
using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Extensions.Components.ItemButtons;
using SearchThing.Extensions.Panel.Data.Extensions;
using SearchThing.Search.Data;
using SearchThing.Util;
using UnityEngine;

namespace SearchThing.Extensions.Panel.Data;

// TODO : TUrn this into a component based system
public class ItemRenderInfo : IItemRenderInfo
{
    public IRequiredItemInfo Crate { get; }
    
    // Formal Data

    public Guid Id => Crate.Id;
    public string Name => TryGetExtension<NameOverwriteExtension>(out var nameOverwriteExtension) ? nameOverwriteExtension.Name : Crate.Name;
    public bool Redacted => Crate.Redacted;
    public DateTime DateAdded => Crate.DateAdded;

    public IRequiredItemInfo GetSource()
    {
        return Crate.GetSource();
    }
    
    // Render Extensions

    private IList<IItemRenderExtension> Extensions { get; }

    public T? GetExtension<T>()
        where T : class, IItemRenderExtension
    {
        return Extensions.OfType<T>().FirstOrDefault();
    }
    
    public bool TryGetExtension<T>([MaybeNullWhen(false)] out T extension)
        where T: class, IItemRenderExtension
    {
        extension = GetExtension<T>();
        return extension != null;
    }

    public ItemRenderInfo(IRequiredItemInfo crate, params IItemRenderExtension[] extensions)
    {
        Crate = crate;

        Extensions = extensions;
    }
}