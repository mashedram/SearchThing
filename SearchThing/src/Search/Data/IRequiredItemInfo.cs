using SearchThing.Search.Search;

namespace SearchThing.Search.Data;

public interface IRequiredItemInfo
{
    /// <summary>
    /// Runtime identifier. Used to quickly determine if two items are the same. Not stable across restarts.
    /// </summary>
    Guid Id { get; }
    /// <summary>
    /// The name of the item.
    /// </summary>
    string Name { get; }
    /// <summary>
    /// Whether the item is visible in basic search.
    /// </summary>
    bool Redacted { get; }
    /// <summary>
    /// When the item was added to the database. Used for sorting by date added.
    /// </summary>
    DateTime DateAdded { get; }
}