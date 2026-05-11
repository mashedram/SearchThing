using SearchThing.Extensions.Components;
using SearchThing.Extensions.Components.Info;
using SearchThing.Search.CrateData;
using SearchThing.Search.Data;
using SearchThing.Search.Search;

namespace SearchThing.Search.Containers;

public record SearchableCrateListWrapper<TCrate>(IEnumerable<TCrate> Crates) : ISearchableCrateList<TCrate> where TCrate : class, ISearchEntry
{
    public IEnumerable<TCrate> GetCrates()
    {
        return Crates;
    }
}

public static class SearchableCrateListWrapperHelper
{
    public static ISearchableCrateList<TCrate> ToSearchable<TCrate>(this IEnumerable<TCrate> crates) where TCrate : class, ISearchEntry
    {
        return new SearchableCrateListWrapper<TCrate>(crates);
    }

    public static ISearchableCrateList<ISearchEntry> CastToSearchable<TCrate>(this IEnumerable<TCrate> crates) where TCrate : class, IRequiredItemInfo
    {
        return new SearchableCrateListWrapper<ISearchEntry>(crates.OfType<ISearchEntry>());
    }
}