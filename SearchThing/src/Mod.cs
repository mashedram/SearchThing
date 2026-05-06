using BoneLib;
using LabFusion.Utilities;
using SearchThing;
using MelonLoader;
using SearchThing.Fusion;
using SearchThing.Patches;
using SearchThing.Patches.Compatibility;
using SearchThing.Presets;
using SearchThing.Search;
using UnityEngine;

[assembly: MelonInfo(typeof(Mod), "SearchThing", "0.5.0", "Mash")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]
namespace SearchThing;

public class Mod : MelonMod
{
    public static bool IsFusionLoaded;
    
    public override void OnInitializeMelon()
    {
        IsFusionLoaded = FindMelon("LabFusion", "Lakatrazz") != null;
        
        Hooking.OnWarehouseReady += OnWarehouseReady;
    }

    /// <summary>
    /// Separate method to prevent loading errors if Fusion is not present.
    /// </summary>
    private static void HookFusion()
    {
        FusionNetworkSpawnPatch.TryInitialize();
        MultiplayerHooking.OnTargetLevelLoaded += SpawnGunPatches.ClearSelectedCrate;
    }
    
    private static void OnWarehouseReady()
    {
        SearchManager.InitializeSearchThread();
        PresetManager.LoadPresets();
        
        if (IsFusionLoaded)
        {
            HookFusion();
        }
        
    }

    public override void OnDeinitializeMelon()
    {
        SearchManager.ShutdownSearchThread();
    }
}