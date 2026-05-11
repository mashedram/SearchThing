using SearchThing.Extensions;

namespace SearchThing.Search.Interaction;

public interface ISelectableCrate
{
    // Called when the user clicks the button for the first time.
    bool OnSelected(SpawnablePanelExtension extension, int idx);
}