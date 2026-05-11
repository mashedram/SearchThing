using SearchThing.Extensions.Components;
using SearchThing.Extensions.Components.Info;
using SearchThing.Search.CrateData;
using SearchThing.Search.Data;
using UnityEngine;

namespace SearchThing.Extensions.Panel.Data;

public class ItemRenderWithAction : ItemRender, IQuickActionItemInfo
{
    public delegate Sprite? GetActionIconDelegate(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo);
    public delegate Color? GetActionHighlightDelegate(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo);
    public delegate void QuickActionDelegate(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo);

    public GetActionIconDelegate? GetActionIconFunc { get; init; }
    public GetActionHighlightDelegate? GetActionHighlightFunc { get; init; }
    public QuickActionDelegate QuickAction { get; init; }

    public ItemRenderWithAction(IRequiredItemInfo crate, QuickActionDelegate quickAction) : base(crate)
    {
        QuickAction = quickAction;
    }

    public Sprite? GetActionIcon(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo)
    {
        return GetActionIconFunc?.Invoke(extension, itemInfo);
    }

    public Color? GetActionHighlight(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo)
    {
        return GetActionHighlightFunc?.Invoke(extension, itemInfo);
    }

    public void PerformQuickAction(SpawnablePanelExtension extension, IRequiredItemInfo itemInfo)
    {
        QuickAction.Invoke(extension, itemInfo);
    }
}