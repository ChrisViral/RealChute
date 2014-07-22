using System.Collections.Generic;
using UnityEngine;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more information contact me on the forum. */

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
        private static Dictionary<string, List<SizeNode>> sizes = new Dictionary<string, List<SizeNode>>();
        #endregion

        #region Functions
        private void Awake()
        {
            if (instance != null) { Destroy(this); return; }
            instance = this;
            DontDestroyOnLoad(this);

            print("[RealChute]: Running RealChute " + RCUtils.assemblyVersion);
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
