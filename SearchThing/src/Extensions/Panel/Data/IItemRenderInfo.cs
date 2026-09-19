using System.Diagnostics.CodeAnalysis;
using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Search.Data;

namespace SearchThing.Extensions.Panel.Data;

public interface IItemRenderInfo
{
    public IRequiredItemInfo GetSource();
    
    // The same as in IRequiredInfo. No inheritance between the two to force a split logic between the datamodel and the renderer
    Guid Id { get; }
    string Name { get; }
    bool Redacted { get; }
    DateTime DateAdded { get; }
    
    public T? GetExtension<T>()
        where T : class, IItemRenderExtension;
    
    public bool TryGetExtension<T>([MaybeNullWhen(false)] out T extension)
        where T : class, IItemRenderExtension;
}