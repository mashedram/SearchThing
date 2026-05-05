using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Search;
using SearchThing.Util;

namespace SearchThing.History;

public class HistoryCrateEntry : ISearchableCrate
{
    public ISearchableCrate Crate { get; }
    public DateTime DateAdded { get; set; }
    
    public HistoryCrateEntry(ISearchableCrate crate, DateTime dateAdded)
    {
        Crate = crate;
        DateAdded = dateAdded;
    }

    public SearchTag Name => Crate.Name;
    public SearchTag PalletName => Crate.PalletName;
    public SearchTag Author => Crate.Author;
    public SearchTag[] Tags => Crate.Tags;
    public string Description => Crate.Description;
    public bool Redacted => Crate.Redacted;
    public CrateType CrateType => Crate.CrateType;
    public CrateSubType CrateSubType => Crate.CrateSubType;
    public int Salt => Crate.Salt;
    public int Score => 0;
    public Barcode Barcode => Crate.Barcode;
}

public class HistoryCrateList : ISearchableCrateList
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly Dictionary<string, HistoryCrateEntry> _crates = new();
    private readonly LinkedList<string> _accessOrder = new();  // Tracks order by ID
    private readonly Dictionary<string, LinkedListNode<string>> _nodes = new();  // Quick access to nodes
    private const int MaxEntries = 100;  // Set your limit
    
    public void AddCrate(ISearchableCrate crate)
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
                _crates[id] = new HistoryCrateEntry(crate, DateTime.UtcNow);
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
    
    public IEnumerable<ISearchableCrate> GetCrates()
    {
        _lock.EnterReadLock();
        try
        {
            return _crates.Values.Select(entry => entry.Crate).ToArray();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}