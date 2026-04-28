using BoneLib;
using SearchThing.Extensions;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.UI;
using Il2CppTMPro;
using LabFusion.Marrow.Proxies;
using MelonLoader;
using SearchThing.Util;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SearchThing.Patches;

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

    [HarmonyPatch(nameof(SpawnablesPanelView.NextPage))]
    [HarmonyPrefix]
    public static bool NextPage_Prefix(SpawnablesPanelView __instance)
    {
        if (__instance == null)
            return true;

        if (!SpawnablesPanelManager.TryGet(__instance, out var extension))
            return true;

        if (!extension.IsSearchActive())
            return true;

        extension.NextPage();
        return false;
    }
    
    [HarmonyPatch(nameof(SpawnablesPanelView.PrevPage))]
    [HarmonyPrefix]
    public static bool PrevPage_Prefix(SpawnablesPanelView __instance)
    {
        if (__instance == null)
            return true;

        if (!SpawnablesPanelManager.TryGet(__instance, out var extension))
            return true;

        if (!extension.IsSearchActive())
            return true;

        extension.PrevPage();
        return false;
    }
    
    [HarmonyPatch(nameof(SpawnablesPanelView.SelectCategory))]
    [HarmonyPostfix]
    public static void SelectCategory_Prefix(SpawnablesPanelView __instance, int idx)
    {
        if (__instance == null)
            return;

        if (!SpawnablesPanelManager.TryGet(__instance, out var extension))
            return;

        if (!extension.IsSearchActive())
            return;

        extension.SelectCategory(idx);
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

        var reference = new AvatarCrateReference(selectedAvatarCrate._barcode);
        var cordDevice = BodylogAccessor.GetCordDevice();
        if (cordDevice != null)
        {
            cordDevice.SwapAvatar(reference).Forget();
        }
        
        return false;
    }
}