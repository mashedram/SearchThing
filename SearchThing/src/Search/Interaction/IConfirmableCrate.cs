using SearchThing.Extensions;

namespace SearchThing.Search.Interaction;

public interface IConfirmableCrate
{
    // Called when the user clicks the button again after pressing it once.
    void OnConfirmed(SpawnablePanelExtension extension, int idx);
}