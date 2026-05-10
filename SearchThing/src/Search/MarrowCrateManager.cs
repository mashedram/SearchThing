using Il2CppSLZ.Marrow.Warehouse;

namespace SearchThing.Search;

public static class MarrowCrateManager
{
    private static readonly SearchableCrateLookup<MarrowCrate> SearchableCrateCrates = new();
    
    public static ISearchableCrateList<MarrowCrate> GetCrates() => SearchableCrateCrates;
    
    public static void AddPallet(Pallet pallet)
    {
        SearchableCrateCrates.AddCrates(
            pallet._crates._items
                .Where(c => c != null)
                .Select(crate => new MarrowCrate(crate))
        );
    }
    
    public static MarrowCrate? GetCrate(Barcode barcode)
    {
        return SearchableCrateCrates.GetCrateByBarcode(barcode._id);
    }
}