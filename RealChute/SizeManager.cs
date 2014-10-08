using System.Collections.Generic;
using UnityEngine;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

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
            if (!CompatibilityChecker.IsAllCompatible() || instance != null) { Destroy(this); return; }
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
        public void AddSizes(string name, List<SizeNode> nodes)
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
