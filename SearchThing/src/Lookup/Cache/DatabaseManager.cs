using LiteDB;
using MelonLoader.Utils;

namespace SearchThing.Lookup.Cache;

public static class DatabaseManager
{
    private static readonly string DatabasePath = MelonEnvironment.UserDataDirectory + "/bonedns.db";
    private static LiteDatabase? _database;
    
    // Collections
    private static ILiteCollection<KnownCrate>? Crates;
    private static ILiteCollection<KnownPallet>? Pallets;

    public static void OnMelonInitialize()
    {
        _database = new LiteDatabase(DatabasePath);
        
        // Create collections
        Crates  = _database.GetCollection<KnownCrate>("KnownCrates");
        Pallets = _database.GetCollection<KnownPallet>("KnownPallets");
        
        // Set up references
        var mapper = BsonMapper.Global;

        mapper.Entity<KnownCrate>()
            .DbRef(x => x.Pallet, "KnownPallets");
        
        mapper.Entity<KnownPallet>()
            .DbRef(x => x.Crates, "KnownCrates");
        
        // Ensure indexes
        Crates.EnsureIndex(x => x.Barcode);
        Pallets.EnsureIndex(x => x.Barcode);
    }

    public static void OnMelonDeinitialize()
    {
        _database?.Dispose();
    }
}