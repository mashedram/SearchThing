using HarmonyLib;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Utilities;
using SearchThing.History;

namespace SearchThing.Patches;

[HarmonyPatch(typeof(RigManager))]
public static class RigManagerPatches
{
    // Separate method to avoid loading not-present assemblier
    private static bool CheckIfLocalRig(RigManager rigManager)
    {
        return rigManager.IsLocalPlayer();
    }

    // We want to know what was spawned, but not if a mod prevents the call, like with a prefix
    [HarmonyPatch(nameof(RigManager.SwapAvatarCrate))]
    [HarmonyPostfix]
    public static void SwapAvatarCrate_Prefix(RigManager __instance, Barcode barcode)
    {
        if (__instance == null)
            return;

        // Skip remote rigs
        if (Mod.IsFusionLoaded && !CheckIfLocalRig(__instance))
            return;

        if (!AssetWarehouse.ready)
            return;

        if (!AssetWarehouse.Instance.TryGetCrate(barcode, out var avatarCrate))
            return;

        HistoryManager.AddEntry(avatarCrate);
    }
}