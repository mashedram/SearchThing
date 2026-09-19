using SearchThing.Search.Data;
using UnityEngine;

namespace SearchThing.Extensions.Panel.Data.Extensions;

public class ActionExtension : IItemRenderExtension
{
    public delegate Sprite? GetActionIconDelegate(SpawnablePanelExtension extension, IItemRenderInfo itemInfo);
    public delegate Color? GetActionHighlightDelegate(SpawnablePanelExtension extension, IItemRenderInfo itemInfo);
    public delegate void QuickActionDelegate(SpawnablePanelExtension extension, IItemRenderInfo itemInfo);

    public GetActionIconDelegate? GetActionIconFunc { get; init; }
    public GetActionHighlightDelegate? GetActionHighlightFunc { get; init; }
    public QuickActionDelegate QuickAction { get; init; }
    
    public ActionExtension(QuickActionDelegate quickAction)
    {
        QuickAction = quickAction;
    }

    
    public Sprite? GetActionIcon(SpawnablePanelExtension extension, IItemRenderInfo itemInfo)
    {
        return GetActionIconFunc?.Invoke(extension, itemInfo);
    }

    public Color? GetActionHighlight(SpawnablePanelExtension extension, IItemRenderInfo itemInfo)
    {
        return GetActionHighlightFunc?.Invoke(extension, itemInfo);
    }

    public void PerformQuickAction(SpawnablePanelExtension extension, IItemRenderInfo itemInfo)
    {
        QuickAction.Invoke(extension, itemInfo);
    }
}