namespace SearchThing.Search.Data;

public interface ICreatorItemInfo : IRequiredItemInfo
{
    string PalletName { get; }
    string Author { get; }
}