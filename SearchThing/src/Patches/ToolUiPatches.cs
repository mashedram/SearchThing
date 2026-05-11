using HarmonyLib;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.UI;
using UnityEngine;

namespace SearchThing.Patches;

[HarmonyPatch(typeof(SpawnablesPanelView))]
public static class ToolUiPatches
{
    // For some reason, the panel refuses to render default behaviour without a set spawn gun
    private static SpawnGun? _DummySpawnGun;

    private static SpawnGun GetDummySpawnGun()
    {
        if (_DummySpawnGun != null)
            return _DummySpawnGun;

        var go = new GameObject("DummySpawnGun");
        go.SetActive(false);
        _DummySpawnGun = go.AddComponent<SpawnGun>();
        return _DummySpawnGun;
    }

    [HarmonyPatch(nameof(SpawnablesPanelView.Activate))]
    [HarmonyPrefix]
    public static void SpawnablesPanelView_Activate_Prefix(SpawnablesPanelView __instance)
    {
        if (__instance == null)
            return;

        if (__instance.spawnGun != null)
            return;

        // It doesn't need to be real or persistent or attached or anything. The panel just NEEDS it to exist.
        __instance.spawnGun = GetDummySpawnGun();
        __instance._spawnGun_k__BackingField = GetDummySpawnGun();
    }

    [HarmonyPatch(nameof(SpawnablesPanelView.Activate))]
    [HarmonyPostfix]
    public static void SpawnablesPanelView_Activate_Postfix(SpawnablesPanelView __instance)
    {
        if (__instance == null)
            return;

        SpawnablesPanelManager.Load(__instance);
    }

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
        extension.RenderAll();
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

        extension.RenderAll();
    }
}