using System.Reflection;
using HarmonyLib;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Marrow;
using LabFusion.Marrow.Serialization;
using LabFusion.Network;
using SearchThing.Fusion;

namespace SearchThing.Patches.Compatibility;

public static class FusionNetworkSpawnPatch
{
    private static class FusionIntegrationPatches
    {
        [HarmonyPostfix]
        public static void RegisterSpawnHistory(ReceivedMessage received)
        {
            if (!NetworkInfo.HasServer)
                return;

            var data = received.ReadData<SerializedSpawnData>();
            
            if (data == null)
                return;
            
            FusionSpawnHistory.AddEntry(data.Barcode, received.Sender);
        }
    }

    // Seperated to avoid DLL missing errors
    private static void TryPatch()
    {
        // Automatic patching does not work, so we have to do it manually with reflection
        try
        {
            // Create Harmony instance
            var harmony = new HarmonyLib.Harmony("com.mash.searchthing.fusionspawnhistoryintegration");

            // Get the OnHandleMessage method using reflection
            var webShooterType = typeof(SpawnRequestMessage);
            var targetMethod = webShooterType.GetMethod(
                "OnHandleMessage", 
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            
            if (targetMethod == null)
            {
                MelonLoader.MelonLogger.Warning("MIDT integration: Could not find OnSpawnDelegateFusion method");
                return;
            }

            // Get the prefix method from our patch class
            var patchesType = typeof(FusionIntegrationPatches);
            var patchMethod = patchesType.GetMethod(nameof(FusionIntegrationPatches.RegisterSpawnHistory));

            // Apply the patch using reflection-based method
            var result = harmony.Patch(targetMethod, postfix: new HarmonyMethod(patchMethod));
            
            if (result != null)
            {
                MelonLoader.MelonLogger.Msg("Fusion integration: Successfully applied RegisterSpawnHistory patch");
            }
            else
            {
                MelonLoader.MelonLogger.Warning("Fusion integration: Failed to apply patch, harmony.Patch returned null");
            }
        }
        catch (Exception ex)
        {
            MelonLoader.MelonLogger.Error($"Failed to patch MIDT integration: {ex}");
        }
    }

    public static void TryInitialize()
    {
        if (!Mod.IsFusionLoaded)
            return;
        
        TryPatch();
    }
}