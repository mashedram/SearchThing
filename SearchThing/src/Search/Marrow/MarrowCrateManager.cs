using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Search.Containers;

namespace SearchThing.Search.Marrow;

public static class MarrowCrateManager
{
    private static readonly SearchableCrateLookup<MarrowCrate> SearchableCrateCrates = new();

    public static ISearchableCrateList<MarrowCrate> GetCrates()
    {
        return SearchableCrateCrates;
    }

    public static void AddPallet(Pallet pallet)
    {
        SearchableCrateCrates.AddCrates(
            pallet._crates._items
                .Where(c => c != null)
                .Select(crate => new MarrowCrate(crate))
        );
    }

    public static MarrowCrate? GetCrate(string id)
    {
        return SearchableCrateCrates.GetCrateByBarcode(id);
    }

    public static MarrowCrate? GetCrate(Barcode barcode)
    {
        return GetCrate(barcode._id);
    }
}