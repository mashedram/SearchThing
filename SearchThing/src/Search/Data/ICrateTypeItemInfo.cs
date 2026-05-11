using SearchThing.Search.CrateData;

namespace SearchThing.Search.Data;

public interface ICrateTypeItemInfo : IRequiredItemInfo
{
    CrateType CrateType { get; }
    CrateSubType CrateSubType { get; }
}