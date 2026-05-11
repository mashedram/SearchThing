using Il2CppTMPro;
using SearchThing.Extensions.Panel;
using UnityEngine;

namespace SearchThing.Extensions.Components.ItemButtons;

public class SortButton
{
    private TextMeshPro _sortButtonText = null!;
    private GameObject _sortButtonObject = null!;

    private ISearchPanel? _panel;

    public SortButton(SpawnablePanelExtension extension)
    {
        var panelView = extension.PanelView;
        _sortButtonText = panelView.transform.Find("group_treePath/text_treePath")!.GetComponent<TextMeshPro>();
        // For some reason the text is centered by default and only goes to the left after a SwapSort call
        // SwapSort forces you to tab 3, so we need to do this workaround
        _sortButtonText.alignment = TextAlignmentOptions.Left;

        _sortButtonObject = panelView.transform.Find("group_treePath/button_SwapSort")!.gameObject;
    }

    public void Render()
    {
        if (_panel == null)
        {
            _sortButtonObject.SetActive(false);
            _sortButtonText.gameObject.SetActive(false);
            return;
        }

        // Update sort button
        if (_panel.SupportedOrders.Length > 1)
        {
            _sortButtonObject.SetActive(true);
            _sortButtonText.gameObject.SetActive(true);

            var order = _panel.SupportedOrders[_panel.SelectedOrderIndex];
            _sortButtonText.text = order.Name;
            // We do what FetchSortButton does every tick in case a tab switch undoes the left alignment
            _sortButtonText.alignment = TextAlignmentOptions.Left;
        }
        else
        {
            _sortButtonObject.SetActive(false);
            _sortButtonText.gameObject.SetActive(false);
        }
    }

    public void SetPanel(ISearchPanel? panel)
    {
        _panel = panel;
    }
}