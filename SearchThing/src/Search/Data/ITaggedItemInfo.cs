namespace SearchThing.Search.Data;

public interface ITaggedItemInfo : IRequiredItemInfo
{
    IEnumerable<string> Tags { get; }
    string TagString => Tags.DefaultIfEmpty("Tags: None").Aggregate("Tags: ", (s, s1) => $"{s} s{1},");
}