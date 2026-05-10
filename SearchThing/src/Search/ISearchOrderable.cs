namespace SearchThing.Search;

public interface ISearchOrderable
{
    ISearchableCrate Source { get; }
    /// <summary>
    /// The score of this item. Higher is better.
    /// </summary>
    int Score { get; }
    /// <summary>
    /// A salt value. Needed for consistent ordering for same-score items.
    /// </summary>
    int Salt { get; }
}