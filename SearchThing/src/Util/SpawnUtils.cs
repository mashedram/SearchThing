using Il2CppCysharp.Threading.Tasks;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.SceneStreaming;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.Marrow.Pool;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Representation;
using LabFusion.RPC;
using LabFusion.Senders;
using SearchThing.Patches;
using UnityEngine;

namespace SearchThing.Util;

public static class SpawnUtils
{
    private static bool SpawnNetworkedCrate(SpawnableCrate spawnableCrate, Vector3 position)
    {
        if (!NetworkInfo.HasServer)
            return false; // Not in a multiplayer session

#if !UNLOCKED
        FusionPermissions.FetchPermissionLevel(PlayerIDManager.LocalPlatformID, out var level, out _);

        if (!FusionPermissions.HasSufficientPermissions(level, LobbyInfoManager.LobbyInfo.DevTools))
            return true; // Don't attempt to spawn locally if we don't have permissions
#endif

        var spawnable = LocalAssetSpawner.CreateSpawnable(spawnableCrate.Barcode._id);
        NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo
        {
            Spawnable = spawnable,
            Position = position,
            Rotation = Quaternion.identity
        });

        return true;
    }

    private static void AssignSpawnableCrate(SpawnableCrate spawnableCrate, Vector3 position)
    {
        if (SpawnGunPatches.SelectCrate(spawnableCrate))
            return;

        if (Mod.IsFusionLoaded && SpawnNetworkedCrate(spawnableCrate, position))
            return;

        var spawnable = new Spawnable
        {
            crateRef = new SpawnableCrateReference(spawnableCrate.Barcode._id),
            policyData = null
        };
        AssetSpawner.Register(spawnable);

        var scale = new Il2CppSystem.Nullable<Vector3>(Vector3.zero)
        {
            hasValue = false
        };

        var groupId = new Il2CppSystem.Nullable<int>(0)
        {
            hasValue = false
        };

        AssetSpawner
            .SpawnAsync(spawnable, position, Quaternion.identity, scale, null, false, groupId, null, null)
            .Forget();
    }

    private static void AssignAvatarCrate(Scannable avatarCrate)
    {
        var reference = new AvatarCrateReference(avatarCrate._barcode);
        var cordDevice = BodylogAccessor.GetCordDevice();
        if (cordDevice != null)
        {
            cordDevice.SwapAvatar(reference).Forget();
        }
    }

    private static bool LoadNetworkLevel(LevelCrate levelCrate)
    {
        if (!NetworkInfo.HasServer)
            return false; // Not in a multiplayer session

        if (!NetworkInfo.IsHost)
        {
            LoadSender.SendLevelRequest(levelCrate);
            return true;
        }

        SceneStreamer.Load(levelCrate._barcode);
        return true;
    }

    private static void LoadLevelCrate(LevelCrate levelCrate)
    {
        if (Mod.IsFusionLoaded && LoadNetworkLevel(levelCrate))
            return;
        
        SceneStreamer.Load(levelCrate._barcode);
    }
    
    public static void Spawn(Crate crate, Vector3 position)
    {
        var selectedAvatarCrate = crate.TryCast<AvatarCrate>();
        if (selectedAvatarCrate != null)
        {
            AssignAvatarCrate(selectedAvatarCrate);
            return;
        }

        var spawnableCrate = crate.TryCast<SpawnableCrate>();
        if (spawnableCrate != null)
        {
            AssignSpawnableCrate(spawnableCrate, position);
        }

        var levelCrate = crate.TryCast<LevelCrate>();
        if (levelCrate != null)
        {
            LoadLevelCrate(levelCrate);
        }
    }
}