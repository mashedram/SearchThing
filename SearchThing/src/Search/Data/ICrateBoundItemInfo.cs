using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Search.CrateData;

namespace SearchThing.Search.Data;

public interface ICrateBoundItemInfo : IRequiredItemInfo
{
    IRequiredItemInfo? Crate { get; }
    Barcode? Barcode { get; }
}