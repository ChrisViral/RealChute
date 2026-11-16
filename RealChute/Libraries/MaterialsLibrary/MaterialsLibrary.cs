using System;
using System.Collections.Generic;
using System.Linq;
using RealChute.Extensions;
using UnityEngine;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get explicit permission on the matter by opening an issue on the RealChute GitHub repository.
 * If approved, you will be required to distribute through the attribution clause, and must make it clear to anyone using
 * your modification of my work that they must report any problem related to this usage to you, and not to me.
 * This excemption can be approved by anyone managing the RealChute repository at the time, be it the original author or anyone
 * that the project was handed to. Should the repository not exist anymore, or should you not receive an answer to your issue
 * within a period of thirty (30) days, you are granted permission to distribute RealChute under the license CC-BY-NC-SA 4.0 INTL. */

namespace RealChute.Libraries.MaterialsLibrary;

public class MaterialsLibrary
{
    #region Instance
    /// <summary>
    /// Creates an instance of the MaterialsLibrary
    /// </summary>
    public static MaterialsLibrary Instance => field ??= new MaterialsLibrary();
    #endregion

    #region Propreties
    /// <summary>
    /// Dictionary containing the name of the materials and their associated MaterialDefinition
    /// </summary>
    private Dictionary<string, MaterialDefinition> Materials { get; }

    /// <summary>
    /// String array of the materials' names
    /// </summary>
    public string[] MaterialNames { get; }

    /// <summary>
    /// The amount of materials currently stored
    /// </summary>
    public int Count { get; }
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new instance of the MaterialsLibrary
    /// </summary>
    private MaterialsLibrary()
    {
        this.Materials = GameDatabase.Instance.GetConfigNodes("MATERIAL").Select(n => new MaterialDefinition(n))
                                     .ToDictionary(m => m.Name, m => m);
        this.MaterialNames = [..this.Materials.Keys];
        this.Count         = this.MaterialNames.Length;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Returns true if the MaterialLibrary contains a definition for the given material
    /// </summary>
    /// <param name="name">Name of the material</param>
    private bool ContainsMaterial(string name) => this.Materials.ContainsKey(name);

    /// <summary>
    /// Returns the MaterialDefinition of the given name
    /// </summary>
    /// <param name="name">Name of the material</param>
    public MaterialDefinition GetMaterial(string name) => ContainsMaterial(name)
                                                              ? this.Materials[name]
                                                              : throw new KeyNotFoundException($"Could not find the \"{name}\" MaterialDefinition in the library");

    /// <summary>
    /// Returns the MaterialDefinition at the given index
    /// </summary>
    /// <param name="index">Index of the material</param>
    public MaterialDefinition GetMaterial(int index) => this.MaterialNames.IndexInRange(index)
                                                            ? GetMaterial(this.MaterialNames[index])
                                                            : throw new IndexOutOfRangeException($"Material index [{index}] is out of range");

    /// <summary>
    /// Tries to get the material of the given name and stores it in the out value
    /// </summary>
    /// <param name="name">Name of the material to find</param>
    /// <param name="material">Value to store the result into</param>
    public bool TryGetMaterial(string name, ref MaterialDefinition material)
    {
        if (ContainsMaterial(name))
        {
            material = this.Materials[name];
            return true;
        }
        if (!string.IsNullOrEmpty(name) && this.Materials.Count > 0) { Debug.LogError($"[RealChute]: Could not find the MaterialDefinition \"{name}\" in the library"); }
        return false;
    }

    /// <summary>
    /// Gets the index of the material looked for
    /// </summary>
    /// <param name="name">Name of the material</param>
    public int GetMaterialIndex(string name) => this.MaterialNames.IndexOf(name);
    #endregion
}
