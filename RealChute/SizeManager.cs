using System.Collections.Generic;
using UnityEngine;

namespace RealChute
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class SizeManager : MonoBehaviour
    {
        #region Instance
        /// <summary>
        /// The current instance of the SizeManager
        /// </summary>
        public static SizeManager instance { get; private set; }
        #endregion

        #region Fields
        private Dictionary<string, List<SizeNode>> sizes = new Dictionary<string, List<SizeNode>>();
        #endregion

        #region Functions
        private void Awake()
        {
            if (instance != null) { Destroy(this); return; }
            instance = this;
            DontDestroyOnLoad(this);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given key/value pair to the dictionary
        /// </summary>
        /// <param name="name">Part name to associate the sizes with</param>
        /// <param name="nodes">Size nodes for the given part</param>
        public void AddSize(string name, List<SizeNode> nodes)
        {
            if (!sizes.ContainsKey(name))
            {
                sizes.Add(name, nodes);
            }
        }

        /// <summary>
        /// Returns the given list of SizeNode for the wanted part
        /// </summary>
        /// <param name="name">PartName to get the sizes for</param>
        public List<SizeNode> GetSizes(string name)
        {
            if (sizes.ContainsKey(name)) { return sizes[name]; }
            return new List<SizeNode>();
        }
        #endregion
    }
}
