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
using SearchThing.Extensions;
using SearchThing.Extensions.Components.ItemButtons;
using SearchThing.Patches;
using SearchThing.Search.CrateData;
using SearchThing.Search.Data;
using SearchThing.Search.Interaction;
using SearchThing.Search.Search;
using SearchThing.Util;
using UnityEngine;

namespace SearchThing.Search.Marrow;

public class MarrowCrate : 
    ISearchableItemInfo,
    ICrateTypeItemInfo,
    ISelectableCrate,
    IConfirmableCrate,
    ICrateIconProvider,
    ICrateBoundItemInfo,
    IEquatable<MarrowCrate>
{
    private readonly SearchTag _name;
    private readonly SearchTag _palletName;
    private readonly SearchTag _author;
    private readonly SearchTag[] _tags;

    public Guid Id { get; } = Guid.NewGuid();
    public string Name => _name.Original;
    public string PalletName => _palletName.Original;
    public string Author => _author.Original;
    public IEnumerable<string> Tags => _tags.Select(t => t.Original);

    public string Description { get; }
    public bool Redacted { get; }
    public int Salt { get; } // Used for tie-breaking to ensure consistent ordering
    public CrateType CrateType { get; }
    public CrateSubType CrateSubType { get; }
    public Sprite Icon => CrateIconProvider.GetIcon(this);
    public DateTime DateAdded { get; }
    public Barcode Barcode { get; }
    // Self reference for crate-bound info
    public IRequiredItemInfo Crate => this;

    public MarrowCrate(Crate spawnableCrate)
    {
        if (spawnableCrate == null)
            throw new ArgumentNullException(nameof(spawnableCrate));
        
        _name = new SearchTag(spawnableCrate.name);
        Description = spawnableCrate._description;
        _palletName = new SearchTag(spawnableCrate._pallet.name);
        _author = new SearchTag(spawnableCrate._pallet._author);
        _tags = spawnableCrate._tags.ToArray().Select(t => new SearchTag(t)).ToArray();

        Redacted = spawnableCrate._redacted;

        Salt = spawnableCrate.name.GetSalt();

        if (!spawnableCrate._pallet.IsInMarrowGame() &&
            AssetWarehouse.Instance.TryGetPalletManifest(spawnableCrate._pallet._barcode, out var palletManifest))
        {
            DateAdded = long.TryParse(palletManifest.UpdatedDate, out var unixTimestampMs)
                ? DateTime.UnixEpoch.AddMilliseconds(unixTimestampMs)
                : DateTime.MinValue;
        }
        else
        {
            DateAdded = DateTime.MinValue; // Fallback to now if we can't find the pallet, shouldn't really happen
        }

        CrateType = spawnableCrate.GetCrateType();
        CrateSubType = spawnableCrate.GetCrateSubType(CrateType);

        Barcode = spawnableCrate.Barcode;
    }

    public MarrowCrate(Barcode barcode) : this(AssetWarehouse.Instance.TryGetCrate(barcode, out var crate)
        ? crate
        : throw new ArgumentException($"Crate with barcode {barcode} not found in warehouse", nameof(barcode)))
    {

    }
    public bool Equals(MarrowCrate? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Barcode._id.Equals(other.Barcode._id);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((MarrowCrate)obj);
    }

    public override int GetHashCode()
    {
        return Barcode._id.GetHashCode();
    }

    public IEnumerable<IFuzzySearchable> SearchFields => new IFuzzySearchable[]
    {
        _name,
        _palletName,
        _author,
        new SearchTagGroup(_tags)
    };
    
    // Menu interaction
    
    private bool SpawnNetworkedCrate(SpawnableCrate spawnableCrate, Vector3 position)
    {
        if (!NetworkInfo.HasServer)
            return false; // Not in a multiplayer session
        
        FusionPermissions.FetchPermissionLevel(PlayerIDManager.LocalPlatformID, out var level, out _);
        
        if (!FusionPermissions.HasSufficientPermissions(level, LobbyInfoManager.LobbyInfo.DevTools))
            return true; // Don't attempt to spawn locally if we don't have permissions
        
        var spawnable = LocalAssetSpawner.CreateSpawnable(spawnableCrate.Barcode._id);
        NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo
        {
            Spawnable =  spawnable,
            Position = position,
            Rotation = Quaternion.identity
        });
        
        return true;
    }
    
    private void AssignSpawnableCrate(SpawnableCrate spawnableCrate, SpawnablePanelExtension extension, int idx)
    {
        if (SpawnGunPatches.SelectCrate(spawnableCrate))
            return;
        
        // Figure out the pressed buttons world position
        var buttons = extension.PanelView.itemButtons;
        if (buttons == null || idx < 0 || idx >= buttons.Length)
            return;
        var position = buttons[idx]!.transform.position;

        if (Mod.IsFusionLoaded && SpawnNetworkedCrate(spawnableCrate, position))
            return;
        
        var spawnable = new Spawnable { crateRef = new SpawnableCrateReference(spawnableCrate.Barcode._id), policyData = null };
        AssetSpawner.Register(spawnable);
        
        var scale = new Il2CppSystem.Nullable<Vector3>(Vector3.zero)
        {
            hasValue = false,
        };

        var groupId = new Il2CppSystem.Nullable<int>(0)
        {
            hasValue = false,
        };

        AssetSpawner
            .SpawnAsync(spawnable, position, Quaternion.identity, scale, null, false, groupId, null, null)
            .Forget();
    }

    private void AssignAvatarCrate(Scannable avatarCrate)
    {
        var reference = new AvatarCrateReference(avatarCrate._barcode);
        var cordDevice = BodylogAccessor.GetCordDevice();
        if (cordDevice != null)
        {
            cordDevice.SwapAvatar(reference).Forget();
        }
    }

    private void LoadLevelCrate(Scannable levelCrate)
    {
        var reference = new LevelCrateReference(levelCrate._barcode);
        SceneStreamer.LoadAsync(reference).Forget();
    }

    public void OnSelected(SpawnablePanelExtension extension, int idx)
    {
        if (!this.TryGetCrate(out var crate))
            return;
        
        // On first select, assign it to the spawngun but don't do anything special yet.
        var spawnableCrate = crate.TryCast<SpawnableCrate>();
        if (spawnableCrate != null)
        {
            SpawnGunPatches.SelectCrate(spawnableCrate);
        }
    }

    public void OnConfirmed(SpawnablePanelExtension extension, int idx)
    {
        if (!this.TryGetCrate(out var crate))
            return;

        var selectedAvatarCrate = crate.TryCast<AvatarCrate>();
        if (selectedAvatarCrate != null)
        {
            AssignAvatarCrate(selectedAvatarCrate);
            return;
        }

        var spawnableCrate = crate.TryCast<SpawnableCrate>();
        if (spawnableCrate != null)
        {
            AssignSpawnableCrate(spawnableCrate, extension, idx);
        }

        var levelCrate = crate.TryCast<LevelCrate>();
        if (levelCrate != null)
        {
            LoadLevelCrate(levelCrate);
        }
    }
}