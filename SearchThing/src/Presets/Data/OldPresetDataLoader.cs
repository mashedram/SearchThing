using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using MelonLoader.Utils;
using SearchThing.Search.Marrow;
// ReSharper disable CollectionNeverUpdated.Global

namespace SearchThing.Presets.Data;

internal class OldPreset
{
    [JsonPropertyName("Name")] public string Name { get; set; } = "";
    [JsonPropertyName("Barcodes")] public List<string> Barcodes { get; set; } = new List<string>();
}

internal class OldPresetPage
{
    [JsonPropertyName("Presets")] 
    public List<OldPreset> Presets { get; set; } = new List<OldPreset>();
}

public static class OldPresetDataLoader
{
    private static readonly string OldPresetPath = MelonEnvironment.UserDataDirectory + "/SearchThingPresets.json";
    private const string EmptyPresetName = "EMPTY";

    private static bool TryReadFile([MaybeNullWhen(false)] out List<OldPresetPage> oldPresetPages)
    {
        oldPresetPages = null;
        if (!File.Exists(OldPresetPath))
            return false;
        
        var json = File.ReadAllText(OldPresetPath);
        oldPresetPages = JsonSerializer.Deserialize<List<OldPresetPage>>(json);
        return oldPresetPages != null;
    }
    
    public static void Migrate()
    {
        if (!TryReadFile(out var presetPages))
            return;

        var presets = presetPages
            .SelectMany(s => s.Presets)
            .Where(p => p.Name != EmptyPresetName)
            .ToList();
        
        foreach (var oldPreset in presets)
        {
            var name = PresetManager.PresetList.Select(p => p.Name).Contains(oldPreset.Name)
                ? $"{oldPreset.Name}-old" 
                : oldPreset.Name;

            var newPreset = new Preset(name);
            foreach (var oldPresetBarcode in oldPreset.Barcodes)
            {
                var crate = MarrowCrateManager.GetCrate(oldPresetBarcode);
                
                if (crate == null)
                    continue;

                newPreset.AssignedCrates.Add(crate);
            }
            
            PresetManager.AddPreset(newPreset);
        }
        
        // Remove the file
        try
        {
            File.Delete(OldPresetPath);
        }
        catch
        {
            // ignored
        }
    }
}