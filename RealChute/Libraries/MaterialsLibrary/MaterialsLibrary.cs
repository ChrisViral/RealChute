using System.Collections.Generic;
using System.Linq;
using RealChute.Extensions;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more informtion contact me on the forum. */

namespace RealChute
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

        private double _count = 0;
        /// <summary>
        /// The amount of materials currently stored
        /// </summary>
        public double count
        {
            get { return this._count; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the MaterialsLibrary
        /// </summary>
        public MaterialsLibrary()
        {
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("MATERIAL"))
            {
                MaterialDefinition material = new MaterialDefinition(node);
                _materials.Add(material, material.name);
            }
            _count = materials.Count;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns true if the MaterialLibrary contains a definition for the given material
        /// </summary>
        /// <param name="name">Name of the material</param>
        public bool HasMaterial(string name)
        {
            return _materials.Values.Contains(name);
        }

        /// <summary>
        /// Returns a specified material
        /// </summary>
        /// <param name="name">Name of the material</param>
        public MaterialDefinition GetMaterial(string name)
        {
            return _materials.Keys.First(key => key.name == name);
        }

        /// <summary>
        /// Returns true and stores the value in the ref if the library contains a definition for the material.
        /// </summary>
        /// <param name="name">Name of the material</param>
        /// <param name="material">Value to store the result in</param>
        public bool TryGetMaterial(string name, ref MaterialDefinition material)
        {
            if (HasMaterial(name))
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
            for (int i = 0; i < materials.Count; i++)
            {
                if (materials.Values.ToArray()[i] == name) { return i; }
            }
            return 0;
        }
        #endregion
    }
}
