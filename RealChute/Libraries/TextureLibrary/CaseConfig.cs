using RealChute.Extensions;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more informtion contact me on the forum. */

namespace RealChute
{
    public class CaseConfig
    {
        #region Propreties
        private string _name = string.Empty;
        /// <summary>
        /// Name of the texture
        /// </summary>
        public string name
        {
            get { return this._name; }
        }

        private string[] _types = new string[] { };
        /// <summary>
        /// Types of parachute this texture applies to
        /// </summary>
        public string[] types
        {
            get { return this._types; }
        }

        private string _textureURL = string.Empty;
        /// <summary>
        /// URL of the texture
        /// </summary>
        public string textureURL
        {
            get { return this._textureURL; }
        }
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
            node.TryGetValue("name", ref _name);
            node.TryGetValue("types", ref _types);
            node.TryGetValue("textureURL", ref _textureURL);
        }
        #endregion
    }
}
