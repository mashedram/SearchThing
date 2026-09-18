namespace SearchThing.Search.Data;

public interface ITaggedItemInfo : IRequiredItemInfo
{
    IEnumerable<string> Tags { get; }
}