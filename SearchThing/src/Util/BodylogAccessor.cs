using BoneLib;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;
using LabFusion.Data;

namespace SearchThing.Util;

public class BodylogAccessor
{
    private static RigManager GetFusionRig()
    {
        return RigData.Refs.RigManager;
    }

    private static RigManager GetRig()
    {
        if (Mod.IsFusionLoaded)
            return GetFusionRig();

        return Player.RigManager;
    }

    private static PullCordDevice? _cordDevice;

    public static PullCordDevice? GetCordDevice()
    {
        if (_cordDevice != null)
            return _cordDevice;

        var rig = GetRig();
        if (rig == null)
            return null;

        _cordDevice = rig.inventory.specialItems
            .Select(item => item.transform)
            .Where(transform => transform.childCount > 0)
            .Select(transform => transform.GetChild(0).GetComponent<PullCordDevice>())
            .FirstOrDefault(item => item != null);
        return _cordDevice;
    }
}