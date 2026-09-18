namespace SearchThing.Search.Data;

public interface IDescriptiveItemInfo : IRequiredItemInfo
{
    string Description { get; }
}