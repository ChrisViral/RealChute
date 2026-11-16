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
 * This exemption can be approved by anyone managing the RealChute repository at the time, be it the original author or anyone
 * that the project was handed to. Should the repository not exist anymore, or should you not receive an answer to your issue
 * within a period of thirty (30) days, you are granted permission to distribute RealChute under the license CC-BY-NC-SA 4.0 INTL. */

namespace RealChute.Libraries.TextureLibrary;

public class TextureConfig
{
    #region Propreties
    private readonly string name = string.Empty;
    /// <summary>
    /// Name of the texture config
    /// </summary>
    public string Name => this.name;

    /// <summary>
    /// List of all the case configs
    /// </summary>
    public Dictionary<string, CaseConfig> Cases { get; }

    /// <summary>
    /// Dictionary of all the available parachute types associated with all the CaseConfigs which it apply it
    /// </summary>
    private Dictionary<string, string[]> Types { get; }

    /// <summary>
    /// Array of the names of all the case configs available
    /// </summary>
    public string[] CaseNames { get; }

    /// <summary>
    /// List of all the canopy configs
    /// </summary>
    public Dictionary<string, CanopyConfig> Canopies { get; }

    /// <summary>
    /// Array of the name of all the canopy configs available
    /// </summary>
    public string[] CanopyNames { get; }

    /// <summary>
    /// List of all the model configs
    /// </summary>
    public Dictionary<string, ModelConfig> Models { get; }

    /// <summary>
    /// Name of all the model transforms for parachutes with their associated ModelConfigs
    /// </summary>
    private Dictionary<string, ModelConfig> Transforms { get; } = new();

    /// <summary>
    /// Amount of parameters, and so available chutes, with the names of the ModelConfigs which have the given amount
    /// </summary>
    private Dictionary<int, string[]> Parameters { get; } = new();

    /// <summary>
    /// Array of the names of all the model configs available
    /// </summary>
    public string[] ModelNames { get; }
    #endregion

    #region Constructor
    /// <summary>
    /// Initiates all the texture and model nodes in this model config
    /// </summary>
    public TextureConfig(ConfigNode node)
    {
        node.TryGetValue("name", ref this.name);

        this.Cases     = node.GetNodes("CASE_TEXTURE").Select(n => new CaseConfig(n)).ToDictionary(c => c.Name, c => c);
        this.CaseNames = [..this.Cases.Keys];
        this.Types = this.Cases.Values.SelectMany(c => c.Types).Distinct()
                         .ToDictionary(t => t, t => this.Cases.Values.Where(c => c.Types.Contains(t)).Select(c => c.Name).ToArray());

        this.Canopies    = node.GetNodes("CANOPY_TEXTURE").Select(n => new CanopyConfig(n)).ToDictionary(c => c.Name, c => c);
        this.CanopyNames = [..this.Canopies.Keys];

        this.Models     = node.GetNodes("CANOPY_MODEL").Select(n => new ModelConfig(n)).ToDictionary(m => m.Name, m => m);
        this.ModelNames = [..this.Models.Keys];
        int max = this.Models.Values.Select(m => m.Parameters.Count).Max();
        for (int i = 1; i <= max; i++)
        {
            int index = i;
            this.Parameters.Add(i, [..this.Models.Values.Where(m => m.Parameters.Count >= index).Select(m => m.Name)]);
        }
        foreach (ModelConfig model in this.Models.Values)
        {
            model.Parameters.ForEach(p => this.Transforms.Add(p.TransformName, model));
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Whether the given case config exists
    /// </summary>
    /// <param name="caseName">Name of the config searched for</param>
    public bool ContainsCase(string caseName) => this.Cases.ContainsKey(caseName);

    /// <summary>
    /// If the types dictionary contains the given type
    /// </summary>
    /// <param name="type">Type of the chute</param>
    private bool ContainsType(string type) => this.Types.ContainsKey(type);

    /// <summary>
    /// Sees if the config exists and stores it in th ref value
    /// </summary>
    /// <param name="caseName">Name of the ref value</param>
    /// <param name="parachuteCase">Variable to store the result in</param>
    public bool TryGetCase(string caseName, ref CaseConfig parachuteCase)
    {
        if (ContainsCase(caseName))
        {
            parachuteCase = this.Cases[caseName];
            return true;
        }
        if (!string.IsNullOrEmpty(caseName) && this.Cases.Count > 0) { Debug.LogError($"[RealChute]: Could not find the CaseConfig  \"{caseName}\" in the library"); }
        return false;
    }

    /// <summary>
    /// Gets the case config of the given index if possible
    /// </summary>
    /// <param name="index">Index of the case config searched for</param>
    /// <param name="type">Type of the parachute</param>
    /// <param name="parachuteCase">Value to store the result in</param>
    public bool TryGetCase(int index, string type, ref CaseConfig parachuteCase)
    {
        if (ContainsType(type))
        {
            string[] names = this.Types[type];
            if (names.IndexInRange(index))
            {
                parachuteCase = this.Cases[names[index]];
                return true;
            }
        }
        if (!string.IsNullOrEmpty(type) && this.Types.Count > 0 && this.Cases.Count > 0) { Debug.LogError($"[RealChute]: Could not find the CaseConfig of \"{type}\" type at the [{index}] index in the library"); }
        return false;
    }

    /// <summary>
    /// Returns all the CaseConfigs of the given parachute type
    /// </summary>
    /// <param name="type">Type of the parachute</param>
    public string[] GetCasesOfType(string type)
    {
        if (!ContainsType(type)) { throw new KeyNotFoundException($"Could not find \"{type} parachute type in the library"); }
        return this.Types[type];
    }

    /// <summary>
    /// Tries to get all the case names of the given type
    /// </summary>
    /// <param name="type">Type of the cases to find</param>
    /// <param name="cases">Value to store the results into</param>
    /// ReSharper disable once UnusedMethodReturnValue.Global
    public bool TryGetCasesOfType(string type, ref string[] cases)
    {
        if (ContainsType(type))
        {
            cases = this.Types[type];
            return true;
        }
        if (!string.IsNullOrEmpty(type) && this.Types.Count > 0) { Debug.LogError($"[RealChute]: Could not find the CaseConfigs of  \"{type}\" type in the library"); }
        return false;
    }

    /// <summary>
    /// Returns the index of this case if it exists
    /// </summary>
    /// <param name="caseName">Name of the CaseConfig to look for</param>
    public int GetCaseIndex(string caseName) => this.CaseNames.IndexOf(caseName);

    /// <summary>
    /// Whether or not the given canopy config exists
    /// </summary>
    /// <param name="canopyName">Name of the config searched for</param>
    public bool ContainsCanopy(string canopyName) => this.Canopies.ContainsKey(canopyName);

    /// <summary>
    /// Returns the config of the given name
    /// </summary>
    /// <param name="canopyName"></param>
    private CanopyConfig GetCanopy(string canopyName)
    {
        return ContainsCanopy(canopyName)
                   ? this.Canopies[canopyName]
                   : throw new KeyNotFoundException($"Could not find the \"{canopyName}\" CanopyConfig in the library");
    }

    /// <summary>
    /// Returns the canopy config at the given index
    /// </summary>
    /// <param name="index">Index of the config</param>
    public CanopyConfig GetCanopy(int index)
    {
        if (!this.CanopyNames.IndexInRange(index)) { throw new IndexOutOfRangeException($"CanopyConfig index [{index}] is out of range"); }
        return GetCanopy(this.CanopyNames[index]);
    }

    /// <summary>
    /// Sees if the given canopy config exists and stores it in the ref value
    /// </summary>
    /// <param name="canopyName">Name of the config searched for</param>
    /// <param name="canopy">Value to store th result in</param>
    public bool TryGetCanopy(string canopyName, ref CanopyConfig canopy)
    {
        if (ContainsCanopy(canopyName))
        {
            canopy = this.Canopies[canopyName];
            return true;
        }
        if (!string.IsNullOrEmpty(canopyName) && this.Canopies.Count > 0) { Debug.LogError($"[RealChute]: Could not find the CanopyConfig \"{canopyName}\" in the library"); }
        return false;
    }

    /// <summary>
    /// Tries to get the canopy config associated to the index and returns false if it does not exist
    /// </summary>
    /// <param name="index">Index of the canopy config</param>
    /// <param name="canopy">Value to store the result in</param>
    public bool TryGetCanopy(int index, ref CanopyConfig canopy)
    {
        if (this.CanopyNames.IndexInRange(index))
        {
            string canopyName = this.CanopyNames[index];
            if (ContainsCanopy(canopyName))
            {
                canopy = this.Canopies[canopyName];
                return true;
            }
        }
        if (this.Canopies.Count > 0) { Debug.LogError($"[RealChute]: Could not find the CanopyConfig at the [{index}] index in the library"); }
        return false;
    }

    /// <summary>
    /// Returns the index of the canopy config if it exists
    /// </summary>
    /// <param name="canopyName">CName of the CanopyConfig to find</param>
    public int GetCanopyIndex(string canopyName) => this.CanopyNames.IndexOf(canopyName);

    /// <summary>
    /// Whether the model config of the given name exists
    /// </summary>
    /// <param name="modelName">Name of the config searched for</param>
    /// <param name="isTransformName">If the name is transform or config name</param>
    public bool ContainsModel(string modelName, bool isTransformName = false) => isTransformName ? this.Transforms.ContainsKey(modelName) : this.Models.ContainsKey(modelName);

    /// <summary>
    /// If there are ModelConfigs with the given amount of parameters
    /// </summary>
    /// <param name="count">Amount of parameters</param>
    private bool ContainsParameters(int count) => this.Parameters.ContainsKey(count);

    /// <summary>
    /// Gets the model config from the library
    /// </summary>
    /// <param name="modelName">Name of the config or the transform searched for</param>
    /// <param name="isTransformName">If the name is transform or config name</param>
    private ModelConfig GetModel(string modelName, bool isTransformName = false)
    {
        if (isTransformName)
        {
            return ContainsModel(modelName, true)
                       ? this.Transforms[modelName]
                       : throw new KeyNotFoundException($"Could not find transform \"{modelName}\" in the library");
        }
        return ContainsModel(modelName)
                   ? this.Models[modelName]
                   : throw new KeyNotFoundException($"Could not find ModelConfig \"{modelName}\" in the library");
    }

    /// <summary>
    /// Gets the ModelConfig at the specified index
    /// </summary>
    /// <param name="index">Index of the config to get</param>
    public ModelConfig GetModel(int index)
    {
        return this.ModelNames.IndexInRange(index)
                   ? GetModel(this.ModelNames[index])
                   : throw new IndexOutOfRangeException($"ModelConfig index [{index}] is out of range");
    }

    /// <summary>
    /// Sees if the model config of the given name exists and stores it in the ref value
    /// </summary>
    /// <param name="modelName">Name of the config searched for</param>
    /// <param name="model">Value to store the result in</param>
    /// <param name="isTransformName">If the name of the transform is used for search</param>
    public bool TryGetModel(string modelName, ref ModelConfig model, bool isTransformName = false)
    {
        if (isTransformName)
        {
            if (ContainsModel(modelName, true))
            {
                model = this.Transforms[modelName];
                return true;
            }
            if (!string.IsNullOrEmpty(modelName) && this.Transforms.Count > 0) { Debug.LogError($"[RealChute]: Could not find the transform \"{modelName}\" in the library"); }
            return false;
        }

        if (ContainsModel(modelName))
        {
            model = this.Models[modelName];
            return true;
        }
        if (!string.IsNullOrEmpty(modelName) && this.Models.Count > 0) { Debug.LogError($"[RealChute]: Could not find the ModelConfig \"{modelName}\" in the library"); }
        return false;
    }

    /// <summary>
    /// Sees if there is a ModelConfig at the specified index and stores it in the ref value if possible.
    /// </summary>
    /// <param name="index">Index of the ModelConfig to look for</param>
    /// <param name="model">Value to store the result in</param>
    public bool TryGetModel(int index, ref ModelConfig model)
    {
        if (this.ModelNames.IndexInRange(index))
        {
            string modelName = this.ModelNames[index];
            if (ContainsModel(modelName))
            {
                model = this.Models[modelName];
                return true;
            }
        }
        if (this.Models.Count > 0) { Debug.LogError($"[RealChute]: Could not find the ModelConfig at the index [{index}] in the library"); }
        return false;
    }

    /// <summary>
    /// Tries to get the ModelConfig names with the given amount of parameters
    /// </summary>
    /// <param name="count">Amount of parameters</param>
    /// <param name="models">Value to store the results into</param>
    public bool TryGetParameterModels(int count, ref string[] models)
    {
        if (ContainsParameters(count))
        {
            models = this.Parameters[count];
            return true;
        }
        if (this.Parameters.Count > 0) { Debug.LogError($"[RealChute]: Could not find the ModelConfig with {count} parameters in the library"); }
        return false;
    }

    /// <summary>
    /// Returns the index of the model config if it exists
    /// </summary>
    /// <param name="modelName">Name of the ModelConfig to find</param>
    public int GetModelIndex(string modelName) => this.ModelNames.IndexOf(modelName);
    #endregion
}
