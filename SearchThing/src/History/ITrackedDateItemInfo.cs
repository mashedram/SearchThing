using SearchThing.Search.Data;

namespace SearchThing.History;

public interface ITrackedDateItemInfo : IRequiredItemInfo
{
    new DateTime DateAdded { get; set; }
}