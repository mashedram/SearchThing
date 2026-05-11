using HarmonyLib;
using Il2CppSLZ.Bonelab;
using SearchThing.Presets;

namespace SearchThing.Patches;

[HarmonyPatch(typeof(PopUpMenuView))]
public static class PopupMenuViewPatches
{
    [HarmonyPatch(nameof(PopUpMenuView.Activate))]
    [HarmonyPrefix]
    public static void Activate_Prefix(PopUpMenuView __instance)
    {
        if (__instance == null)
            return;

        // Always show the spawn menu
        __instance.AddSpawnMenu();
    }

    [HarmonyPatch(nameof(PopUpMenuView.RemoveSpawnMenu))]
    [HarmonyPrefix]
    public static bool RemoveSpawnMenu_Prefix(PopUpMenuView __instance)
    {
        // Prevent the spawn menu from being removed
        return false;
    }
    
    [HarmonyPatch(nameof(PopUpMenuView.Deactivate))]
    [HarmonyPostfix]
    public static void Deactivate_Postfix(PopUpMenuView __instance)
    {
        if (__instance == null)
            return;

        PresetManager.SavePresets();
    }
}