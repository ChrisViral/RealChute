using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RealChute.Extensions;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute.Libraries.Materials
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
        private Dictionary<string, MaterialDefinition> _materials = new Dictionary<string, MaterialDefinition>();
        /// <summary>
        /// Dictionary containing the name of the materials and their associated MaterialDefinition
        /// </summary>
        public Dictionary<string, MaterialDefinition> materials
        {
            get { return this._materials; }
        }

        private string[] _materialNames = new string[0];
        /// <summary>
        /// String array of the materials' names
        /// </summary>
        public string[] materialNames
        {
            get { return this._materialNames; }
        }

        private int _count = 0;
        /// <summary>
        /// The amount of materials currently stored
        /// </summary>
        public int count
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
            this._materials = GameDatabase.Instance.GetConfigNodes("MATERIAL").Select(n => new MaterialDefinition(n))
                .ToDictionary(m => m.name, m => m);
            this._materialNames = this._materials.Keys.ToArray();
            this._count = this._materialNames.Length;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns true if the MaterialLibrary contains a definition for the given material
        /// </summary>
        /// <param name="name">Name of the material</param>
        public bool ContainsMaterial(string name)
        {
            return this._materials.ContainsKey(name);
        }

        /// <summary>
        /// Returns the MAterialDefinition of the given name
        /// </summary>
        /// <param name="name">Name of the material</param>
        public MaterialDefinition GetMaterial(string name)
        {
            if (!ContainsMaterial(name)) { throw new KeyNotFoundException("Could not find the \"" + name + "\" MaterialDefinition in the library"); }
            return this._materials[name];
        }

        /// <summary>
        /// Returns the MaterialDefinition at the given index
        /// </summary>
        /// <param name="index">Index of the material</param>
        public MaterialDefinition GetMaterial(int index)
        {
            if (!this.materialNames.IndexInRange(index)) { throw new IndexOutOfRangeException("Material index [" + index + "] is out of range"); }
            return GetMaterial(this._materialNames[index]);
        }

        /// <summary>
        /// Tries to get the material of the given name and stores it in the out value
        /// </summary>
        /// <param name="name">Name of the material to find</param>
        /// <param name="material">Value to store the result into</param>
        public bool TryGetMaterial(string name, ref MaterialDefinition material)
        {
            if (ContainsMaterial(name))
            {
                material = this._materials[name];
                return true;
            }
            if (!string.IsNullOrEmpty(name) && this._materials.Count > 0) { Debug.LogError("[RealChute]: Could not find the MaterialDefinition \"" + name + "\" in the library"); }
            return false;
        }

        /// <summary>
        /// Gets the index of the material looked for
        /// </summary>
        /// <param name="name">Name of the material</param>
        public int GetMaterialIndex(string name)
        {
            return _materialNames.IndexOf(name);
        }
        #endregion
    }
}
