namespace SearchThing.Search;

/// <summary>
/// Represents a list of searchable crates. This is used to store the crates that are currently being searched through.
/// </summary>
/// <remarks>This class MUST be thread safe</remarks>
public interface ISearchableCrateList
{
    IEnumerable<ISearchableCrate> GetCrates();
}

public class SearchableCrateList : ISearchableCrateList
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly List<ISearchableCrate> _crates = new();
    
    public void AddCrate(ISearchableCrate crate)
    {
        _lock.EnterWriteLock();
        try
        {
            _crates.Add(crate);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    
    public void AddCrates(IEnumerable<ISearchableCrate> crates)
    {
        _lock.EnterWriteLock();
        try
        {
            _crates.AddRange(crates);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    
    public IEnumerable<ISearchableCrate> GetCrates()
    {
        _lock.EnterReadLock();
        try
        {
            return _crates.ToArray();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}