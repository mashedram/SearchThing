using BoneLib;
using BoneSearch.Extensions;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.UI;
using Il2CppTMPro;
using LabFusion.Marrow.Proxies;
using MelonLoader;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BoneSearch.Patches;

[HarmonyPatch(typeof(SpawnablesPanelView))]
public static class ToolUiPatches
{
    
    [HarmonyPatch(nameof(SpawnablesPanelView.Activate))]
    [HarmonyPostfix]
    public static void SpawnablesPanelView_Activate_Postfix(SpawnablesPanelView __instance)
    {
        if (__instance == null) 
            return;

        SpawnablesPanelManager.Load(__instance);
    }

    [HarmonyPatch(nameof(SpawnablesPanelView.SelectTab))]
    [HarmonyPostfix]
    public static void SpawnablesPanelView_SelectTab_Postfix(SpawnablesPanelView __instance, int idx)
    {
        if (__instance == null)
            return;

        SpawnablesPanelManager.OnTabSelected(__instance, idx);
    }

    [HarmonyPatch(nameof(SpawnablesPanelView.SelectItem))]
    [HarmonyPrefix]
    public static bool SelectItem_Prefix(SpawnablesPanelView __instance, int idx)
    {
        var selectedCrate = __instance.SpawnablesQuickMap[__instance._selectedTag][idx];
        var selectedAvatarCrate = selectedCrate.TryCast<AvatarCrate>();
        
        // If the selected crate is not an avatar, continue as normal
        if (selectedAvatarCrate == null)
            return true;

        Player.RigManager.SwapAvatarCrate(selectedAvatarCrate._barcode);
        
        return false;
    }
}