using System.Text.Json;
using MelonLoader;
using MelonLoader.Utils;
using SearchThing.Extensions;
using SearchThing.Extensions.Pages;
using SearchThing.Extensions.Panel.Filter;
using SearchThing.Presets.Data;

namespace SearchThing.Presets;

public static class PresetManager
{
    // TODO : Make this configurable
    private static readonly string PresetDirectoryPath = MelonEnvironment.UserDataDirectory + "/SearchThingPresets.json";

    private static Type? _returnPanel;

    public static bool IsAssignmentMode { get; private set; }

    private static readonly List<Preset> Presets = new();
    public static IEnumerable<Preset> PresetList => Presets;

    static PresetManager()
    {
    
    }
    
    public static void ToggleAssigmentMode(SpawnablePanelExtension extension)
    {
        if (IsAssignmentMode)
        {
            extension.OpenPanel(_returnPanel ?? typeof(PropTagSearchPanel));
            _returnPanel = null;
            IsAssignmentMode = false;
            
            return;
        }
        
        _returnPanel = extension.GetSelectedPanel().GetType();
        IsAssignmentMode = true;
        extension.OpenPanel(typeof(PresetPanel));
    }
    
    public static void AddPreset(Preset preset)
    {
        Presets.Add(preset);
    }

    public static void LoadPresets()
    {
        if (!Directory.Exists(PresetDirectoryPath)) return;

        try
        {
            // TODO : Reimplement saving and loading
            // var json = File.ReadAllText(PresetDirectoryPath);
            // var data = JsonSerializer.Deserialize<List<PresetPageData>>(json);
            //
            // if (data == null) return;
            //
            // for (var i = 0; i < data.Count; i++)
            // {
            //     var presetData = data[i];
            //     if (i >= PresetPages.Count)
            //         break;
            //     
            //     PresetPages[i].FromData(presetData);
            // }
        }
        catch (Exception exception)
        {
            MelonLogger.Error("Failed to load presets from file!", exception);
        }
    }

    public static void SavePresets()
    {
        try
        {
            // Ensure the directory exists
            Directory.CreateDirectory(PresetDirectoryPath);

        }
        catch (Exception exception)
        {
            MelonLogger.Error("Failed to save presets to file!", exception);
        }
    }
}