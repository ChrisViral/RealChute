using RealChute.Extensions;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more information contact me on the forum. */

namespace RealChute.Libraries
{
    public class CanopyConfig
    {
        #region Propreties
        private string _name = string.Empty;
        /// <summary>
        /// The name of the texture
        /// </summary>
        public string name
        {
            get { return this._name; }
        }

        private string _textureURL = string.Empty;
        /// <summary>
        /// The URL of the texture
        /// </summary>
        public string textureURL
        {
            get { return this._textureURL; }
        }
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
            node.TryGetValue("name", ref _name);
            node.TryGetValue("textureURL", ref _textureURL);
        }
        #endregion
    }
}
