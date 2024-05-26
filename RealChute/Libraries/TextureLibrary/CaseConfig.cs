/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute.Libraries.TextureLibrary
{
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
        public string[] Types { get; } = new string[0];

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
}
