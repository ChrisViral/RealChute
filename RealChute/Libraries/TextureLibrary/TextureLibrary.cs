using System.Collections.Generic;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more informtion contact me on the forum. */

namespace RealChute
{
    public class TextureLibrary
    {
        #region Instance
        private static TextureLibrary _instance = null;
        /// <summary>
        /// Creates a new instance
        /// </summary>
        public static TextureLibrary instance
        {
            get
            {
                if (_instance == null) { _instance = new TextureLibrary(); }
                return _instance;
            }
        }
        #endregion

        #region Propreties
        private List<TextureConfig> _configs = new List<TextureConfig>();
        /// <summary>
        /// List of all the current texture configs
        /// </summary>
        public List<TextureConfig> configs
        {
            get { return this._configs; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the library
        /// </summary>
        public TextureLibrary()
        {
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("TEXTURE_LIBRARY"))
            {
                TextureConfig config = new TextureConfig(node);
                _configs.Add(config);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Determines if the config of the given name exists in the library.
        /// </summary>
        /// <param name="name">Name of the searched config</param>
        public bool ExistsConfig(string name)
        {
            return configs.Exists(config => config.name == name);
        }

        /// <summary>
        /// Returns the config of the given name.
        /// </summary>
        /// <param name="name">Name of the config searched for</param>
        public TextureConfig GetConfig(string name)
        {
            return configs.Find(config => config.name == name);
        }

        /// <summary>
        /// Sees if the config exists and stores it in the ref value if so.
        /// </summary>
        /// <param name="name">Name of the config searched for</param>
        /// <param name="config">Variable to store the result in</param>
        public bool TryGetConfig(string name, ref TextureConfig config)
        {
            if (ExistsConfig(name))
            {
                config = GetConfig(name);
                return true;
            }
            if (name != "none") { UnityEngine.Debug.LogWarning("[RealChute]: Could not find the " + name + " texture config in the GameData folder"); }
            return false;
        }
        #endregion
    }
}
