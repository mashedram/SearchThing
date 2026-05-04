namespace SearchThing.Search;

public class SearchableCrateLookup : ISearchableCrateList
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<string, ISearchableCrate> _lookup = new();
    private readonly List<ISearchableCrate> _crates = new();
    
    public void AddCrate(ISearchableCrate crate)
    {
        _lock.EnterWriteLock();
        try
        {
            _lookup[crate.Barcode._id] = crate;
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
            foreach (var crate in crates)
            {
                _lookup[crate.Barcode._id] = crate;
                _crates.Add(crate);
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    
    public ISearchableCrate? GetCrateByBarcode(string barcode)
    {
        _lock.EnterReadLock();
        try
        {
            return _lookup.GetValueOrDefault(barcode);
        }
        finally
        {
            _lock.ExitReadLock();
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