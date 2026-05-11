namespace SearchThing.Presets.Data;

public class PresetData
{
    public int Version { get; set; } = 1;
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "New Preset";
    public List<Guid> Items { get; set; } = new();
    public DateTime DateAdded { get; set; } = DateTime.Now;
}