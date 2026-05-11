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
    private static Type? _returnPanel;

    public static bool IsAssignmentMode { get; private set; }

    private static readonly List<Preset> Presets = new();
    public static IEnumerable<Preset> PresetList => Presets.Where(p => !p.Redacted);
    public static int PresetCount => Presets.Count(p => !p.Redacted);

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
    
    public static void RemovePreset(Preset preset)
    {
        preset.Redacted = true;
    }

    public static void LoadPresets()
    {
        if (!Directory.Exists(UserData.PresetsPath)) return;

        Presets.Clear();
        try
        {
            var files = Directory.GetFiles(UserData.PresetsPath, "*.json");;
            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var data = JsonSerializer.Deserialize<PresetData>(json);
                    if (data == null)
                        continue;

                    var preset = new Preset(data);
                    Presets.Add(preset);
                }
                catch (Exception exception)
                {
                    MelonLogger.Error($"Failed to load preset from file {file}!", exception);
                }
            }
        }
        catch (Exception exception)
        {
            MelonLogger.Error("Failed to load presets from file!", exception);
        }
        
        Presets.Sort((a, b) => a.DateAdded.CompareTo(b.DateAdded));
    }

    public static void SavePresets()
    {
        var dirtyPresets = Presets.Where(p => p.IsDirty).ToList();
        if (dirtyPresets.Count == 0)
            return;
        
        foreach (var preset in dirtyPresets)
        {
            try
            {   
                var sanitizedName = string.Join("_", preset.Name.Split(Path.GetInvalidFileNameChars()));
                var path = UserData.PresetsPath + $"/{sanitizedName}_{preset.Id}.json";
                
                if (preset.Redacted)
                {
                    if (File.Exists(path))
                        File.Delete(path);
                    
                    Presets.Remove(preset);
                    continue;
                }
                
                var data = preset.ToData();
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
            catch (Exception exception)
            {
                MelonLogger.Error("Failed to save presets to file!", exception);
            }
        }

    }
}