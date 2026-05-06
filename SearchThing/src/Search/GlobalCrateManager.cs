using Il2CppSLZ.Marrow.Warehouse;

namespace SearchThing.Search;

public static class GlobalCrateManager
{
    private static readonly SearchableCrateLookup SearchableCrateCrates = new();
    
    public static ISearchableCrateList<ISearchableCrate> GetCrates() => SearchableCrateCrates;
    
    public static void AddPallet(Pallet pallet)
    {
        SearchableCrateCrates.AddCrates(
            pallet._crates._items
                .Where(c => c != null)
                .Select(crate => new SearchableCrate(crate))
        );
    }
    
    public static ISearchableCrate? GetCrate(Barcode barcode)
    {
        return SearchableCrateCrates.GetCrateByBarcode(barcode._id);
    }
}