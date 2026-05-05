using BoneLib;
using SearchThing;
using MelonLoader;
using SearchThing.Presets;
using SearchThing.Search;
using UnityEngine;

[assembly: MelonInfo(typeof(Mod), "SearchThing", "0.4.3", "Mash")]
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
    
    private static void OnWarehouseReady()
    {
        SearchManager.InitializeSearchThread();
        PresetManager.LoadPresets();
    }

    public override void OnDeinitializeMelon()
    {
        SearchManager.ShutdownSearchThread();
    }
}