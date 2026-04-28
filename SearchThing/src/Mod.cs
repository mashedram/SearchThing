using SearchThing;
using MelonLoader;

[assembly: MelonInfo(typeof(Mod), "SearchThing", "0.2.1", "Mash")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]
namespace SearchThing;

public class Mod : MelonMod
{
    public static bool IsFusionLoaded;
    
    public override void OnInitializeMelon()
    {
        IsFusionLoaded = FindMelon("LabFusion", "Lakatrazz") != null;
    }
}