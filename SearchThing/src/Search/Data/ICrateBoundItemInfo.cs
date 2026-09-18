using Il2CppSLZ.Marrow.Warehouse;

namespace SearchThing.Search.Data;

public interface ICrateBoundItemInfo : IRequiredItemInfo
{
    IRequiredItemInfo? Crate { get; }
    Barcode? Barcode { get; }
}