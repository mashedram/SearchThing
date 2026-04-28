using HarmonyLib;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.History;

namespace SearchThing.Patches;

[HarmonyPatch((typeof(SpawnGun)))]
public class SpawnGunPatches
{
    // Run this as soon after we fire to capture what is fired before any other mods might change that
    [HarmonyPatch(nameof(SpawnGun.OnFire))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static void OnFire_Prefix(SpawnGun __instance)
    {
        if (__instance == null)
            return;
        
        var crate = __instance._selectedCrate;
        if (crate == null)
            return;
        
        HistoryManager.AddEntry(crate);
    }
}