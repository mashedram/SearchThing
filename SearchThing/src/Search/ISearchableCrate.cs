using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Util;

namespace SearchThing.Search;

public interface ISearchableCrate
{
    SearchTag Name { get; }
    SearchTag PalletName { get; }
    SearchTag Author { get; }
    SearchTag[] Tags { get; }
    
    CrateType CrateType { get; }
    int Salt { get; }
    int Score { get; }
    DateTime DateAdded { get; }
    Barcode Barcode { get; }
}