using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Util;

namespace SearchThing.Search;

public interface IFullCrateData : IFormalCrateData
{
    // Formal data
    string Description { get; }
    
    CrateType CrateType { get; }
    CrateSubType CrateSubType { get; }
}