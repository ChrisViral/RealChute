using System.Collections.Generic;
using System.Linq;

namespace RealChute.Libraries.Presets;

public class DefaultPresets
{
    private readonly string name;
    /// <summary>
    /// Default presets name
    /// </summary>
    public string Name => this.name;

    /// <summary>
    /// List of presets within these defaults
    /// </summary>
    public Dictionary<string, Preset> Presets { get; }

    /// <summary>
    /// Loads a DefaultPresets node into memory
    /// </summary>
    /// <param name="node">ConfigNode to load from</param>
    public DefaultPresets(ConfigNode node)
    {
        // Required
        if (!node.TryGetValue("name", ref this.name)) return;

        this.Presets = node.GetNodes("PRESET").Select(n => new Preset(n)).ToDictionary(p => p.Name, p => p);
    }
}
