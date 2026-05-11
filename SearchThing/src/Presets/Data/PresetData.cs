namespace SearchThing.Presets.Data;

public class PresetData
{
    public int Version { get; set; } = 1;
    public string Name { get; set; } = "New Preset";
    public List<string> Barcodes { get; set; } = new();
}