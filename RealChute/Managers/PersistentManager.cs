using System;
using System.Collections.Generic;
using RealChute.Utils;
using UnityEngine;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute.Managers
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class PersistentManager : MonoBehaviour
    {
        #region Instance
        /// <summary>
        /// The current instance of the SizeManager
        /// </summary>
        public static PersistentManager instance { get; private set; }
        #endregion

        #region Fields
        private static Dictionary<string, List<SizeNode>> sizes = new Dictionary<string, List<SizeNode>>();
        private static Dictionary<Type, Dictionary<string, ConfigNode>> nodes = new Dictionary<Type, Dictionary<string, ConfigNode>>();
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
        /// Adds the given list of SizeNodes to a persistent dictionary with the given Part name as Key
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
        /// Returns the given list of SizeNode for the wanted Part name
        /// </summary>
        /// <param name="name">Part name to get the sizes for</param>
        public List<SizeNode> GetSizes(string name)
        {
            List<SizeNode> list;
            if (sizes.TryGetValue(name, out list))
            {
                return list;
            }
            return new List<SizeNode>();
        }

        /// <summary>
        /// Stores a ConfigNode value in a persistent dictionary, sorted by PartModule type and Part name
        /// </summary>
        /// <typeparam name="T">PartModule type</typeparam>
        /// <param name="name">Part name to use as Key</param>
        /// <param name="node">ConfigNode to store</param>
        public void AddNode<T>(string name, ConfigNode node) where T : PartModule
        {
            Dictionary<string, ConfigNode> dict;
            Type type = typeof(T);
            if (!nodes.TryGetValue(type, out dict))
            {
                dict = new Dictionary<string, ConfigNode>();
                dict.Add(name, node);
                nodes.Add(type, dict);
            }
            else if (!dict.ContainsKey(name))
            {
                dict.Add(name, node);
            }
        }

        /// <summary>
        /// Retreives a ConfigNode for the given PartModule type and Part name
        /// </summary>
        /// <typeparam name="T">PartModule type</typeparam>
        /// <param name="name">Part name to get the node for</param>
        public bool TryGetNode<T>(string name, ref ConfigNode node) where T : PartModule
        {
            Dictionary<string, ConfigNode> dict;
            if (nodes.TryGetValue(typeof(T), out dict))
            {
                ConfigNode n;
                if (dict.TryGetValue(name, out n))
                {
                    node = n;
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
