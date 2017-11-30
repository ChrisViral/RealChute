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
    public class CanopyConfig
    {
        #region Propreties
        private readonly string name = string.Empty;
        /// <summary>
        /// The name of the texture
        /// </summary>
        public string Name => this.name;

        private readonly string textureURL = string.Empty;
        /// <summary>
        /// The URL of the texture
        /// </summary>
        public string TextureURL => this.textureURL;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an empty CanopyConfig
        /// </summary>
        public CanopyConfig() { }

        /// <summary>
        /// Creates a new CanopyConfig from the given ConfigNode
        /// </summary>
        /// <param name="node">ConfigNode to make the config from</param>
        public CanopyConfig(ConfigNode node)
        {
            node.TryGetValue("name", ref this.name);
            node.TryGetValue("textureURL", ref this.textureURL);
        }
        #endregion
    }
}
