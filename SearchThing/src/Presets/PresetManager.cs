using System.Text.Json;
using MelonLoader;
using MelonLoader.Utils;
using SearchThing.Extensions.Pages;
using SearchThing.Presets.Data;

namespace SearchThing.Presets;

public static class PresetManager
{
    // TODO : Make this configurable
    private static readonly string PresetDirectoryPath = MelonEnvironment.UserDataDirectory + "/SearchThingPresets.json";
    private const int MaxPresets = 6;

    private static bool _assignmentMode;

    public static bool IsAssignmentMode
    {
        get => _assignmentMode;
        set
        {
            if (!value)
                SavePresets();

            _assignmentMode = value;
        }
    }

    private static readonly List<PresetPage> PresetPages = new();

    static PresetManager()
    {
        for (var i = 0; i < MaxPresets; i++)
        {
            PresetPages.Add(new PresetPage());
        }
    }

    public static IReadOnlyList<ISearchPage> GetPages()
    {
        return PresetPages;
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