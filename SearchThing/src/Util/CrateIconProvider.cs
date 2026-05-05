using SearchThing.Search;
using UnityEngine;

namespace SearchThing.Util;

public static class CrateIconProvider
{
    // Fallback Icons
    private static readonly Sprite AvatarIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.AvatarIcon.png");
    private static readonly Sprite CrateIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.CrateIcon.png");
    private static readonly Sprite LevelIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.LevelIcon.png");
    
    // Spawnable subtype icons
    
    private static readonly Sprite GunIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.GunIcon.png");
    private static readonly Sprite MeleeIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.MeleeIcon.png");
    private static readonly Sprite ThrowableIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.ThrowableIcon.png");
    private static readonly Sprite VehicleIcon = ImageHelper.LoadEmbeddedSprite("SearchThing.resources.VehicleIcon.png");

    
    // Fetcher

    private static Sprite GetAvatarIcon()
    {
        return AvatarIcon;
    }
    
    private static Sprite GetPropIcon(ISearchableCrate searchableCrate)
    {
        return searchableCrate.CrateSubType switch
        {
            CrateSubType.Gun => GunIcon,
            CrateSubType.Melee => MeleeIcon,
            CrateSubType.Throwable => ThrowableIcon,
            CrateSubType.Vehicle => VehicleIcon,
            _ => CrateIcon
        };
    }
    
    private static Sprite GetLevelIcon()
    {
        return LevelIcon;
    }
    
    public static Sprite GetIcon(ISearchableCrate searchableCrate)
    {
        return searchableCrate.CrateType switch
        {
            CrateType.Avatar => GetAvatarIcon(),
            CrateType.Prop => GetPropIcon(searchableCrate),
            CrateType.Level => GetLevelIcon(),
            _ => CrateIcon
        };
    }
}