using Il2CppSLZ.Marrow.Forklift.Model;
using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.dependencies.LiteDB.Client.Database;
using SearchThing.Search.Database.Entity;
using UnityEngine.Rendering;

namespace SearchThing.Search.Database;

public static class CrateDatabaseManager
{
    private static LiteDatabase _db = null!;
    private static ILiteCollection<KnownCrate> _knownCrates = null!;

    public static void Initialize(string path)
    {
        _db = new LiteDatabase(path);
        _knownCrates = _db.GetCollection<KnownCrate>("known_crates");

        _knownCrates.EnsureIndex(x => x.Id, true);
        _knownCrates.EnsureIndex(x => x.Barcode, true);
    }

    public static void Dispose()
    {
        _db.Dispose();
    }

    // Crate accessors

    private static long? GetModId(Crate crate)
    {
        // Try to get the manifest for the crate's pallet
        if (!AssetWarehouse.Instance.palletManifests.TryGetValue(crate._pallet._barcode, out var manifest))
            return null;

        var listing = manifest.ModListing;
        if (listing == null)
            return null;

        foreach (var target in listing.Targets)
        {
            var modIOTarget = target.Value.TryCast<ModIOModTarget>();

            if (modIOTarget != null)
            {
                return modIOTarget.ModId;
            }
        }

        return null;
    }

    public static KnownCrate GetOrCreateKnownCrate(Crate crate)
    {
        var barcode = crate._barcode._id;
        var knownCrate = _knownCrates.FindOne(kc => kc.Barcode == barcode);
        if (knownCrate != null)
            return knownCrate;


        knownCrate = new KnownCrate
        {
            Id = Guid.NewGuid(),
            Name = crate.name,
            Barcode = barcode,
            ModId = GetModId(crate)
        };
        _knownCrates.Insert(knownCrate);
        return knownCrate;
    }

    public static string? GetBarcode(Guid crateId)
    {
        var knownCrate = _knownCrates.FindOne(kc => kc.Id == crateId);
        return knownCrate?.Barcode;
    }
}