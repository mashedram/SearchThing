using BoneLib;
using SearchThing.Extensions;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.UI;
using Il2CppTMPro;
using LabFusion.Marrow.Proxies;
using MelonLoader;
using SearchThing.History;
using SearchThing.Presets;
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

    // TODO: Make this a prefix to render content before the switch and then fake and prevent the switch
    // Should limit flickering
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

        extension.ChangePanelPage(1);
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

        extension.ChangePanelPage(-1);
        return false;
    }
    
    [HarmonyPatch(nameof(SpawnablesPanelView.SelectItem))]
    [HarmonyPrefix]
    public static bool SelectItem_Prefix(SpawnablesPanelView __instance, int idx)
    {
        if (__instance == null)
            return true;

        if (!SpawnablesPanelManager.TryGet(__instance, out var extension))
            return true;

        if (!extension.IsSearchActive())
            return true;

        extension.OnSelectItem(idx);
        return false;
    }
    
    [HarmonyPatch(nameof(SpawnablesPanelView.SelectCategory))]
    [HarmonyPrefix]
    public static bool SelectCategory_Prefix(SpawnablesPanelView __instance, int idx)
    {
        if (__instance == null)
            return true;

        if (!SpawnablesPanelManager.TryGet(__instance, out var extension))
            return true;

        if (!extension.IsSearchActive())
            return true;

        extension.SelectCategory(idx);
        return false;
    }

    [HarmonyPatch(nameof(SpawnablesPanelView.NextTagPage))]
    [HarmonyPrefix]
    private static bool NextTagPage_Prefix(SpawnablesPanelView __instance)
    {
        if (__instance == null)
            return true;

        if (!SpawnablesPanelManager.TryGet(__instance, out var extension))
            return true;

        if (!extension.IsSearchActive())
            return true;

        extension.ChangeTagPage(1);
        return false;
    }
    
    [HarmonyPatch(nameof(SpawnablesPanelView.PrevTagPage))]
    [HarmonyPrefix]
    private static bool PrevTagPage_Prefix(SpawnablesPanelView __instance)
    {
        if (__instance == null)
            return true;

        if (!SpawnablesPanelManager.TryGet(__instance, out var extension))
            return true;

        if (!extension.IsSearchActive())
            return true;

        extension.ChangeTagPage(-1);
        return false;
    }

    [HarmonyPatch(nameof(SpawnablesPanelView.SwapSortButton))]
    [HarmonyPrefix]
    public static bool SwapSortButton_Prefix(SpawnablesPanelView __instance)
    {
        if (__instance == null)
            return true;

        if (!SpawnablesPanelManager.TryGet(__instance, out var extension))
            return true;

        if (!extension.IsSearchActive())
            return true;

        extension.SwapSortButton();
        return false;
    }

    [HarmonyPatch(nameof(SpawnablesPanelView.FavoriteItem))]
    [HarmonyPrefix]
    public static bool FavoriteItem_Prefix(SpawnablesPanelView __instance)
    {
        if (__instance == null)
            return true;

        if (!SpawnablesPanelManager.TryGet(__instance, out var extension))
            return true;

        if (!extension.IsSearchActive())
            return true;

        extension.OnFavoriteButton();
        return false;
    }
    
    [HarmonyPatch(nameof(SpawnablesPanelView.SelectItem))]
    [HarmonyPostfix]
    public static void SelectItem_Postfix(SpawnablesPanelView __instance, int idx)
    {
        if (__instance == null)
            return;

        if (!SpawnablesPanelManager.TryGet(__instance, out var extension))
            return;

        if (!extension.IsSearchActive())
            return;

        // We need a seperate postfix for this because SelectItem may run after a render
        extension.RenderFavoriteButton();
    }

    [HarmonyPatch(nameof(SpawnablesPanelView.SelectItem))]
    [HarmonyPrefix]
    public static void ToggleFavorite_Prefix(SpawnablesPanelView __instance, int idx)
    {
        if (__instance == null)
            return;

        if (!SpawnablesPanelManager.TryGet(__instance, out var extension))
            return;

        if (!extension.IsSearchActive())
            return;

        extension.RenderFavoriteButton();
    }

    // private record SpawngunOverwriteInfo(SpawnGun SpawnGun, SpawnableCrate Crate);
    // // Funny thing, not doing this will cause the spawn gun to actually load the avatar
    // // In SP, this will just turn you into the avatar
    // // In Fusion, the avatar will spawn in, as just the mesh
    // [HarmonyPatch(nameof(SpawnablesPanelView.SelectItem))]
    // [HarmonyPostfix]
    // private static void SelectItem_Prefix(SpawnablesPanelView __instance, int idx, ref SpawngunOverwriteInfo? __state)
    // {
    //     __state = null;
    //     var selectedCrate = __instance.SpawnablesQuickMap[__instance._selectedTag][idx];
    //     var selectedAvatarCrate = selectedCrate.TryCast<AvatarCrate>();
    //     
    //     // If the selected crate is not an avatar, continue as normal
    //     if (selectedAvatarCrate == null)
    //         return;
    //
    //     var reference = new AvatarCrateReference(selectedAvatarCrate._barcode);
    //     var cordDevice = BodylogAccessor.GetCordDevice();
    //     if (cordDevice != null)
    //     {
    //         cordDevice.SwapAvatar(reference).Forget();
    //     }
    // }
}