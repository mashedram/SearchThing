using Il2CppSLZ.Marrow.Warehouse;

namespace SearchThing.Search;

public interface ISearchableCrate
{
    string PreprocessedString { get; }
    CrateType CrateType { get; }
    int Score { get; }
    DateTime DateAdded { get; }
    Barcode Barcode { get; }
}