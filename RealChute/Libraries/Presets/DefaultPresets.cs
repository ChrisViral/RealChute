using System.Collections.Generic;
using System.Linq;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get explicit permission on the matter by opening an issue on the RealChute GitHub repository.
 * If approved, you will be required to distribute through the attribution clause, and must make it clear to anyone using
 * your modification of my work that they must report any problem related to this usage to you, and not to me.
 * This exemption can be approved by anyone managing the RealChute repository at the time, be it the original author or anyone
 * that the project was handed to. Should the repository not exist anymore, or should you not receive an answer to your issue
 * within a period of thirty (30) days, you are granted permission to distribute RealChute under the license CC-BY-NC-SA 4.0 INTL. */

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
