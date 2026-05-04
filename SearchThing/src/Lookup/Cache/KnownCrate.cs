namespace SearchThing.Lookup.Cache;

public class KnownCrate
{
    public string Barcode { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string[] Tags { get; set; }
    public KnownPallet Pallet { get; set; } 
}

public class KnownPallet
{
    public string Barcode { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    
    public string DownloadLink { get; set; }
    
    public List<KnownCrate> Crates { get; set; }
}