using System.Collections.Generic;
using System.Linq;
using RealChute.Extensions;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute.Libraries
{
    public class MaterialsLibrary
    {
        #region Instance
        private static MaterialsLibrary _instance = null;
        /// <summary>
        /// Creates an instance of the MaterialsLibrary
        /// </summary>
        public static MaterialsLibrary instance
        {
            get
            {
                if (_instance == null) { _instance = new MaterialsLibrary(); }
                return _instance;
            }
        }
        #endregion

        #region Propreties
        private Dictionary<MaterialDefinition, string> _materials = new Dictionary<MaterialDefinition, string>();
        /// <summary>
        /// Dictionary containing MaterialDefinitions as keys and material names as values
        /// </summary>
        public Dictionary<MaterialDefinition, string> materials
        {
            get { return this._materials; }
        }

        /// <summary>
        /// The amount of materials currently stored
        /// </summary>
        public double count
        {
            get { return this.materials.Count; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the MaterialsLibrary
        /// </summary>
        public MaterialsLibrary()
        {
            _materials = GameDatabase.Instance.GetConfigNodes("MATERIAL").Select(n => new MaterialDefinition(n)).ToDictionary(m => m, m => m.name);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns true if the MaterialLibrary contains a definition for the given material
        /// </summary>
        /// <param name="name">Name of the material</param>
        public bool MaterialExists(string name)
        {
            return materials.Values.Contains(name);
        }

        /// <summary>
        /// Returns a specified material
        /// </summary>
        /// <param name="name">Name of the material</param>
        public MaterialDefinition GetMaterial(string name)
        {
            return materials.Single(pair => pair.Value == name).Key;
        }

        /// <summary>
        /// Returns the MaterialDefinition at the given index
        /// </summary>
        /// <param name="index">Index of the material</param>
        public MaterialDefinition GetMaterial(int index)
        {
            return materials.Keys.ToArray()[index];
        }

        /// <summary>
        /// Returns true and stores the value in the ref if the library contains a definition for the material.
        /// </summary>
        /// <param name="name">Name of the material</param>
        /// <param name="material">Value to store the result in</param>
        public bool TryGetMaterial(string name, ref MaterialDefinition material)
        {
            if (MaterialExists(name))
            {
                material = GetMaterial(name);
                return true;
            }
            if (name != "empty" && name != string.Empty) { UnityEngine.Debug.LogWarning("[RealChute]: Could not find the " + name + " material within library"); }
            return false;
        }

        /// <summary>
        /// Gets the index of the material looked for
        /// </summary>
        /// <param name="name">Name of the material</param>
        public int GetMaterialIndex(string name)
        {
            return materials.Values.ToList().IndexOf(name);
        }
        #endregion
    }
}
