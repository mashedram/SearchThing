using Il2CppTMPro;
using SearchThing.Search.Data;

namespace SearchThing.Extensions.Components.Info;

public class ItemInfoBox
{
    private SpawnablePanelExtension _parent;

    private readonly TextMeshPro _title;
    private readonly TextMeshPro _description;
    private readonly TextMeshPro _author;
    private readonly TextMeshPro _pallet;
    private readonly TextMeshPro _tags;

    private readonly ItemQuickAction _quickAction;

    public IRequiredItemInfo? SelectedItem { get; private set; }

    public ItemInfoBox(SpawnablePanelExtension extension)
    {
        _parent = extension;
        var panelView = extension.PanelView;

        _quickAction = new ItemQuickAction(extension);

        _title = panelView.selectedTitle;
        _description = panelView.selectedDescription;
        _author = panelView.selectedAuthor;
        _pallet = panelView.selectedPallet;
        _tags = panelView.selectedTags;
    }
    
    private void Clear(string placeholder = "")
    {
        _title.text = placeholder;
        _description.text = placeholder;
        _author.text = placeholder;
        _pallet.text = placeholder;
        _tags.text = placeholder;
    }

    public void Reset()
    {
        Clear();
        _quickAction.Reset();
    }

    public void Render()
    {
        _quickAction.Render(SelectedItem);

        if (SelectedItem == null)
        {
            Clear();
            return;
        }

        // Ensure that overwrites always render in case the value is being edited
        _title.text = SelectedItem.Name;

        if (SelectedItem is ICreatorItemInfo sourceData)
        {
            _author.text = $"Author: {sourceData.Author}";
            _pallet.text = $"Pallet: {sourceData.PalletName}";
        }
        else
        {
            _author.text = "Author: Unknown";
            _pallet.text = "Pallet: Unknown";
        }

        if (SelectedItem is IDescriptiveItemInfo descriptiveData)
        {
            _description.text = descriptiveData.Description;
            _tags.text = $"Tags: {string.Join(", ", descriptiveData.Tags)}";
        }
        else
        {
            _description.text = "No description.";
            _tags.text = "Tags: None";
        }
    }

    public void SetContent(IRequiredItemInfo? data)
    {
        SelectedItem = data;

        if (data is IQuickActionItemInfo quickActionInfo)
        {
            _quickAction.SetQuickActionInfo(quickActionInfo);
        }
        else
        {
            _quickAction.SetQuickActionInfo(null);
        }
    }

    public void OnQuickAction()
    {
        if (SelectedItem == null)
            return;

        _quickAction.CallQuickAction(SelectedItem);
    }
}