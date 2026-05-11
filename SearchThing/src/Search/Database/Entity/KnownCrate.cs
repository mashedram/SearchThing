namespace SearchThing.Search.Database.Entity;

public class KnownCrate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Barcode { get; set; }

    public long? ModId { get; set; }
}