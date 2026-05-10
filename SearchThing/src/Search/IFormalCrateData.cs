namespace SearchThing.Search;

public interface IFormalCrateData
{
    string Name { get; }
    string PalletName { get; }
    string Author { get; }
    IEnumerable<string> Tags { get; }
    bool Redacted { get; }
    DateTime DateAdded { get; }
}