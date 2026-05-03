using HarmonyLib;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Network;
using LabFusion.Scene;
using LabFusion.Utilities;
using SearchThing.History;

namespace SearchThing.Patches;

[HarmonyPatch(typeof(SpawnGun))]
public class SpawnGunPatches
{
    public static SpawnGun? HeldSpawnGun { get; private set; }
    
    private static bool IsHeldByLocalPlayer(Gun spawnGun)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
            return true;
        
        var host = spawnGun.host;
        if (host == null)
            return false;
        
        if (!host.IsAttached)
            return false;

        var rm = host.GetHand()?.manager;
        return rm != null && rm.IsLocalPlayer();
    }

    [HarmonyPatch(nameof(SpawnGun.OnTriggerGripAttached))]
    [HarmonyPostfix]
    public static void OnTriggerGripAttached_Prefix(SpawnGun __instance, Hand hand)
    {
        if (__instance == null)
            return;

        if (Mod.IsFusionLoaded && !IsHeldByLocalPlayer(__instance))
            return;
        
        HeldSpawnGun = __instance;
    }

    [HarmonyPatch(nameof(SpawnGun.OnTriggerGripDetached))]
    [HarmonyPostfix]
    public static void OnTriggerGripDetached_Prefix(SpawnGun __instance, Hand hand)
    {
        if (__instance == null)
            return;

        if (Mod.IsFusionLoaded && !IsHeldByLocalPlayer(__instance))
            return;
        
        HeldSpawnGun = null;
    }
    
    // Run this as soon after we fire to capture what is fired before any other mods might change that
    [HarmonyPatch(nameof(SpawnGun.OnFire))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static void OnFire_Prefix(SpawnGun __instance)
    {
        if (__instance == null)
            return;

        if (Mod.IsFusionLoaded && !IsHeldByLocalPlayer(__instance))
            return;
        
        var crate = __instance._selectedCrate;
        if (crate == null)
            return;
        
        HistoryManager.AddEntry(crate);
    }

    [HarmonyPatch(nameof(SpawnGun.OnSpawnableSelected))]
    [HarmonyPrefix]
    public static bool OnSpawnableSelected_Prefix(SpawnGun __instance, SpawnableCrate crate)
    {
        if (__instance == null)
            return true;
        
        if (Mod.IsFusionLoaded && !IsHeldByLocalPlayer(__instance))
            return true;

        // Ignore non-avatar crates
        if (crate.TryCast<AvatarCrate>() == null)
            return true;

        // Do not allow setting the spawnable to an avatar
        return false;
    }
}