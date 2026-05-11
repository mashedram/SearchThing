using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Search;
using SearchThing.Search.Containers;
using SearchThing.Search.CrateData;
using SearchThing.Search.Data;
using SearchThing.Search.Search;

namespace SearchThing.History;

public class BoundCrateList<TCrate> : ISearchableCrateList<TCrate>
    where TCrate : class, ITrackedDateItemInfo, ICrateBoundItemInfo, ISearchEntry
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<string, TCrate> _crates = new();
    private readonly LinkedList<string> _accessOrder = new(); // Tracks order by ID
    private readonly Dictionary<string, LinkedListNode<string>> _nodes = new(); // Quick access to nodes
    private const int MaxEntries = 100; // Set your limit

    public void AddCrate(TCrate crate)
    {
        _lock.EnterWriteLock();
        try
        {
            var id = crate.Barcode._id;

            if (_crates.TryGetValue(id, out var existingEntry))
            {
                existingEntry.DateAdded = DateTime.UtcNow;
                // Move to end
                _accessOrder.Remove(_nodes[id]);
                _nodes[id] = _accessOrder.AddLast(id);
            }
            else
            {
                crate.DateAdded = DateTime.UtcNow;
                _crates[id] = crate;
                _nodes[id] = _accessOrder.AddLast(id);

                // Remove oldest if over limit
                if (_crates.Count > MaxEntries)
                {
                    RemoveOldest();
                }
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    private void RemoveOldest()
    {
        if (_accessOrder.First == null) return;

        var oldestId = _accessOrder.First.Value;
        _accessOrder.RemoveFirst();
        _nodes.Remove(oldestId);
        _crates.Remove(oldestId);
    }

    public IEnumerable<TCrate> GetCrates()
    {
        _lock.EnterReadLock();
        try
        {
            return _crates.Values.ToArray();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}