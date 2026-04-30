using SearchThing.Extensions.Pages;
using SearchThing.Extensions.Panel;
using SearchThing.Presets.Data;

namespace SearchThing.Presets;

public class PresetPage : ISearchPage
{
    public const int MaxPresets = 6;
    private readonly List<Preset> _presets = new List<Preset>();
    public IReadOnlyList<ISearchPanel> Panels => _presets;

    public PresetPage()
    {
        for (var i = 0; i < 6; i++)
        {
            _presets.Add(new Preset());
        }
    }
    
    public PresetPageData ToData()
    {
        return new PresetPageData
        {
            Presets = _presets.Select(p => p.ToData()).ToList()
        };
    }
    
    public void FromData(PresetPageData data)
    {
        for (var i = 0; i < MaxPresets; i++)
        {
            _presets[i].FromData(data.Presets[i]);
        }
    }
}