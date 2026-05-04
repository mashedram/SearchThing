using LiteDB;
using MelonLoader.Utils;

namespace SearchThing.Lookup.Cache;

public static class DatabaseManager
{
    private static readonly string DatabasePath = MelonEnvironment.UserDataDirectory + "/bonedns.db";
    private static LiteDatabase? _database;
    
    // Collections
    private static ILiteCollection<KnownCrate>? _crates;
    private static ILiteCollection<KnownPallet>? _pallets;

    public static void OnMelonInitialize()
    {
        _database = new LiteDatabase(DatabasePath);
        
        // Create collections
        _crates  = _database.GetCollection<KnownCrate>("KnownCrates");
        _pallets = _database.GetCollection<KnownPallet>("KnownPallets");
        
        // Set up references
        var mapper = BsonMapper.Global;

        mapper.Entity<KnownCrate>()
            .DbRef(x => x.Pallet, "KnownPallets");
        
        mapper.Entity<KnownPallet>()
            .DbRef(x => x.Crates, "KnownCrates");
        
        // Ensure indexes
        _crates.EnsureIndex(x => x.Barcode);
        _pallets.EnsureIndex(x => x.Barcode);
    }

    public static void OnMelonDeinitialize()
    {
        _database?.Dispose();
    }
}