/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get explicit permission on the matter by opening an issue on the RealChute GitHub repository.
 * If approved, you will be required to distribute through the attribution clause, and must make it clear to anyone using
 * your modification of my work that they must report any problem related to this usage to you, and not to me.
 * This exemption can be approved by anyone managing the RealChute repository at the time, be it the original author or anyone
 * that the project was handed to. Should the repository not exist anymore, or should you not receive an answer to your issue
 * within a period of thirty (30) days, you are granted permission to distribute RealChute under the license CC-BY-NC-SA 4.0 INTL. */

namespace RealChute.Libraries.TextureLibrary;

public class CaseConfig
{
    #region Propreties
    private readonly string name = string.Empty;
    /// <summary>
    /// Name of the texture
    /// </summary>
    public string Name => this.name;

    /// <summary>
    /// Types of parachute this texture applies to
    /// </summary>
    public string[] Types { get; } = [];

    private readonly string textureURL = string.Empty;
    /// <summary>
    /// URL of the texture
    /// </summary>
    public string TextureURL => this.textureURL;
    #endregion

    #region Constructor
    /// <summary>
    /// Creates an empty CaseConfig
    /// </summary>
    public CaseConfig() { }

    /// <summary>
    /// Creates a new CaseConfig from the given ConfigNode
    /// </summary>
    /// <param name="node">ConfigNode to get the values from</param>
    public CaseConfig(ConfigNode node)
    {
        node.TryGetValue("name", ref this.name);
        string array = string.Empty;
        if (node.TryGetValue("types", ref array))
        {
            this.Types = RCUtils.ParseArray(array);
        }
        node.TryGetValue("textureURL", ref this.textureURL);
    }
    #endregion
}
