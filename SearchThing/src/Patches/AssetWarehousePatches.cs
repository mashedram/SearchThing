using HarmonyLib;
using Il2CppSLZ.Marrow.Warehouse;
using MelonLoader;
using SearchThing.Search;

namespace SearchThing.Patches;

[HarmonyPatch(typeof(AssetWarehouse))]
public static class AssetWarehousePatches
{
    [HarmonyPatch(nameof(AssetWarehouse.AddPallet))]
    [HarmonyPostfix]
    public static void AddPallet_Postfix(Pallet pallet)
    {
        GlobalCrateManager.AddPallet(pallet);
    }
}