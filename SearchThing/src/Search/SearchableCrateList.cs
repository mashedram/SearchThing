namespace SearchThing.Search;

/// <summary>
/// Represents a list of searchable crates. This is used to store the crates that are currently being searched through.
/// </summary>
/// <remarks>This class MUST be thread safe</remarks>
public interface ISearchableCrateList<out TCrate> where TCrate : class, ISearchableCrate
{
    IEnumerable<TCrate> GetCrates();
}

public class SearchableCrateList<TCrate> : ISearchableCrateList<TCrate> where TCrate : class, ISearchableCrate
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly List<TCrate> _crates = new();
    
    public void AddCrate(TCrate crate)
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
    
    public void AddCrates(IEnumerable<TCrate> crates)
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
    
    public IEnumerable<TCrate> GetCrates()
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