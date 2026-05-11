using Il2CppSLZ.Marrow.Warehouse;
using SearchThing.Extensions;
using SearchThing.Extensions.Components;
using SearchThing.Extensions.Components.Info;
using SearchThing.Extensions.Panel;
using SearchThing.Extensions.Panel.Abstract;
using SearchThing.Extensions.Panel.Data;
using SearchThing.Extensions.Sort;
using SearchThing.Presets.Data;
using SearchThing.Search;
using SearchThing.Search.Containers;
using SearchThing.Search.CrateData;
using SearchThing.Search.Data;
using SearchThing.Search.Search;
using SearchThing.Search.Sorting;
using SearchThing.Util;
using UnityEngine;
using Random = System.Random;

namespace SearchThing.Presets;

internal class SpawnableCrateComparer : IEqualityComparer<IRequiredItemInfo>
{
    public bool Equals(IRequiredItemInfo? x, IRequiredItemInfo? y)
    {
        if (x == null || y == null)
            return false;

        return x.Id == y.Id;
    }

    public int GetHashCode(IRequiredItemInfo obj)
    {
        return obj.Id.GetHashCode();
    }
}

public class Preset : ISearchableItemInfo
{
    private SearchTag _nameTag;
    
    public Guid Id { get; } = Guid.NewGuid();
    public string Name => _nameTag.Original;
    public bool Redacted { get; } = false;
    public DateTime DateAdded { get; } = DateTime.Now;
    public IEnumerable<IFuzzySearchable> SearchFields => new IFuzzySearchable[] { _nameTag };
    public int Salt { get; } = Random.Shared.Next();
    
    public HashSet<ISearchableItemInfo> AssignedCrates { get; } = new(new SpawnableCrateComparer());
    
    public Preset(string name)
    {
        _nameTag = new SearchTag(name);
    }
}