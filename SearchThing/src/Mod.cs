using BoneLib;
using SearchThing;
using MelonLoader;
using SearchThing.Lookup.Cache;
using SearchThing.Presets;
using SearchThing.Search;

[assembly: MelonInfo(typeof(Mod), "SearchThing", "0.4.1", "Mash")]
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
        DatabaseManager.OnMelonInitialize();
        SearchManager.InitializeSearchThread();
        PresetManager.LoadPresets();
    }

    public override void OnDeinitializeMelon()
    {
        DatabaseManager.OnMelonDeinitialize();
        SearchManager.ShutdownSearchThread();
    }
}