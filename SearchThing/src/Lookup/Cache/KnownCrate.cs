using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Search;

namespace SearchThing.Lookup.Cache;

public class KnownCrate
{
    public string Barcode { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string[] Tags { get; set; }
    
    public string PalletName { get; set; }
    public string PalletAuthor { get; set; }
    
    public int ModId { get; set; }
    public int FileId { get; set; }
    
    public KnownCrate() {}

    public KnownCrate(ISearchableCrate searchableCrate)
    {
        Barcode = searchableCrate.Barcode._id;
        Name = searchableCrate.Name.Original;
        Description = searchableCrate.Description;
        Tags = searchableCrate.Tags.Select(t => t.Original).ToArray();
        
        
    }
}