using Il2CppSLZ.Marrow.Warehouse;
using LiteDB;
using MelonLoader.Utils;
using SearchThing.Search;

namespace SearchThing.Lookup.Cache;

public static class DatabaseManager
{
    private static readonly string DatabasePath = MelonEnvironment.UserDataDirectory + "/bonedns.db";
    private static LiteDatabase? _database;
    
    // Collections
    private static ILiteCollection<KnownCrate>? _crates;

    public static void OnMelonInitialize()
    {
        _database = new LiteDatabase(DatabasePath);
        
        // Create collections
        _crates  = _database.GetCollection<KnownCrate>("KnownCrates");
        
        // Set up references
        var mapper = BsonMapper.Global;
        
        // Ensure indexes
        _crates.EnsureIndex(x => x.Barcode);
    }

    public static void OnMelonDeinitialize()
    {
        _database?.Dispose();
    }
    
    // Cache appending methods

    
    public static void AddCrate(ISearchableCrate crate)
    {
        if (_crates == null) 
            return;
        
        if (_crates.Exists(x => x.Barcode == crate.Barcode._id))
            return; // Already exists, skip
        
        // var knownCrate = new KnownCrate(crate);
    }
}