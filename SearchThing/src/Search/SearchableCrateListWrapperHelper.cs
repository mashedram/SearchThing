namespace SearchThing.Search;

public record SearchableCrateListWrapper<TCrate>(IEnumerable<TCrate> Crates) : ISearchableCrateList<TCrate> where TCrate : class, ISearchableCrate
{
    public IEnumerable<TCrate> GetCrates()
    {
        return Crates;
    }
}

public static class SearchableCrateListWrapperHelper
{
    public static ISearchableCrateList<TCrate> ToSearchable<TCrate>(this IEnumerable<TCrate> crates) where TCrate : class, ISearchableCrate
    {
        return new SearchableCrateListWrapper<TCrate>(crates);
    }
}