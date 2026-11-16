using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get explicit permission on the matter by opening an issue on the RealChute GitHub repository.
 * If approved, you will be required to distribute through the attribution clause, and must make it clear to anyone using
 * your modification of my work that they must report any problem related to this usage to you, and not to me.
 * This exemption can be approved by anyone managing the RealChute repository at the time, be it the original author or anyone
 * that the project was handed to. Should the repository not exist anymore, or should you not receive an answer to your issue
 * within a period of thirty (30) days, you are granted permission to distribute RealChute under the license CC-BY-NC-SA 4.0 INTL. */

namespace RealChute;

[KSPAddon(KSPAddon.Startup.Instantly, true)]
public class PersistentManager : MonoBehaviour
{
    #region Instance
    /// <summary>
    /// The current instance of the SizeManager
    /// </summary>
    public static PersistentManager Instance { get; private set; }
    #endregion

    #region Fields
    private static readonly Dictionary<string, List<SizeNode>> Sizes = new();
    private static readonly Dictionary<Type, Dictionary<string, ConfigNode>> Nodes = new();
    #endregion

    #region Functions
    private void Awake()
    {
        string version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        Debug.Log("[RealChute]: Running RealChute v" + version);
        if (!CompatibilityChecker.IsAllCompatible|| Instance != null) { Destroy(this); return; }

        Instance = this;
        DontDestroyOnLoad(this);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Adds the given list of SizeNodes to a persistent dictionary with the given Part name as Key
    /// </summary>
    /// <param name="partName">Part name to associate the sizes with</param>
    /// <param name="nodes">Size nodes for the given part</param>
    public void AddSizes(string partName, List<SizeNode> nodes)
    {
        if (!Sizes.ContainsKey(partName))
        {
            Sizes.Add(partName, nodes);
        }
    }

    /// <summary>
    /// Returns the given list of SizeNode for the wanted Part name
    /// </summary>
    /// <param name="partName">Part name to get the sizes for</param>
    public static List<SizeNode> GetSizes(string partName) => Sizes.TryGetValue(partName, out List<SizeNode> list) ? list : [];

    /// <summary>
    /// Stores a ConfigNode value in a persistent dictionary, sorted by PartModule type and Part name
    /// </summary>
    /// <typeparam name="T">PartModule type</typeparam>
    /// <param name="partName">Part name to use as Key</param>
    /// <param name="node">ConfigNode to store</param>
    public void AddNode<T>(string partName, ConfigNode node) where T : PartModule
    {
        Type type = typeof(T);
        if (!Nodes.TryGetValue(type, out Dictionary<string, ConfigNode> dict))
        {
            dict = new Dictionary<string, ConfigNode>();
            Nodes.Add(type, dict);
        }

        dict[partName] = node;  //Allows updating the stored node (MM reload)
    }

    /// <summary>
    /// Retrieves a ConfigNode for the given PartModule type and Part name
    /// </summary>
    /// <typeparam name="T">PartModule type</typeparam>
    /// <param name="partName">Part name to get the node for</param>
    /// <param name="node">Stored module confignode</param>
    public static bool TryGetNode<T>(string partName, ref ConfigNode node) where T : PartModule
    {
        if (Nodes.TryGetValue(typeof(T), out Dictionary<string, ConfigNode> dict))
        {
            if (dict.TryGetValue(partName, out node))
            {
                return true;
            }
        }
        return false;
    }
    #endregion
}
