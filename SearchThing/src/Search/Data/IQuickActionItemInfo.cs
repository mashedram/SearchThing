using SearchThing.Extensions;
using UnityEngine;

namespace SearchThing.Search.Data;

public interface IQuickActionItemInfo
{
    Sprite? GetActionIcon(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo);
    Color? GetActionHighlight(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo);
    void PerformQuickAction(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo);
}