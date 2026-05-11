using SearchThing.Search.CrateData;
using SearchThing.Search.Data;
using SearchThing.Search.Search;

namespace SearchThing.Search.Containers;

public class SearchableCrateLookup<T> : ISearchableCrateList<T>
    where T : class, ISearchEntry, ICrateBoundItemInfo
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<string, T> _lookup = new();
    private readonly List<T> _crates = new();

    public void AddCrate(T crate)
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

    public void AddCrates(IEnumerable<T> crates)
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

    public T? GetCrateByBarcode(string barcode)
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


    public IEnumerable<T> GetCrates()
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