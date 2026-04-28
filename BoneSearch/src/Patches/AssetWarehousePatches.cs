using BoneSearch.Search;
using HarmonyLib;
using Il2CppSLZ.Marrow.Warehouse;
using MelonLoader;

namespace BoneSearch.Patches;

[HarmonyPatch(typeof(AssetWarehouse))]
public static class AssetWarehousePatches
{
    [HarmonyPatch(nameof(AssetWarehouse.AddPallet))]
    [HarmonyPostfix]
    public static void AddPallet_Postfix(Pallet pallet)
    {
        SearchManager.AddPallet(pallet);
    }
}