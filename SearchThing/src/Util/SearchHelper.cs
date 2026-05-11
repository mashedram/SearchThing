using System.Diagnostics.CodeAnalysis;
using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Search.CrateData;
using SearchThing.Search.Data;

namespace SearchThing.Util;

public static class SearchHelper
{
    public static int GetSalt(this string str)
    {
        unchecked
        {
            return str.Aggregate(23, (current, c) => current * 31 + c);
        }
    }

    public static bool IsCrate<T>(this Crate crate) where T : Crate
    {
        return crate.TryCast<T>() != null;
    }

    public static CrateType GetCrateType(this Crate crate)
    {
        if (crate.IsCrate<AvatarCrate>())
            return CrateType.Avatar;

        if (crate.IsCrate<LevelCrate>())
            return CrateType.Level;

        return CrateType.Prop;
    }

    private static List<string> GetTags(this Crate crate)
    {
        return crate._tags._items
            .Where(tag => !string.IsNullOrEmpty(tag))
            .Select(tag => tag.ToLower())
            .ToList();
    }

    private static List<string> GetMetaDataList(this Crate crate)
    {
        var tags = crate.GetTags();
        // Add additional data to tag list
        if (!string.IsNullOrEmpty(crate.Title))
            tags.AddRange(crate.Title.ToLower().Split(" ").Select(p => p.Trim()));

        return tags;
    }

    private static CrateSubType GetPropSubType(this Crate crate)
    {
        var meta = crate.GetMetaDataList();

        if (meta.Contains("gun"))
            return CrateSubType.Gun;

        if (meta.Contains("grenade"))
            return CrateSubType.Throwable;

        if (meta.Contains("melee"))
            return CrateSubType.Melee;

        if (meta.Contains("vehicle"))
            return CrateSubType.Vehicle;

        return CrateSubType.None;
    }

    public static CrateSubType GetCrateSubType(this Crate crate, CrateType crateType)
    {
        switch (crateType)
        {
            case CrateType.Prop:
                return crate.GetPropSubType();
            default:
                return CrateSubType.None;
        }
    }

    public static bool TryGetCrate(this ICrateBoundItemInfo barcodeProvider, [MaybeNullWhen(false)] out Crate outCrate)
    {
        return AssetWarehouse.Instance.TryGetCrate(barcodeProvider.Barcode, out outCrate);
    }
}