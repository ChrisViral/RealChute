using System;
using System.Collections.Generic;
using System.Linq;
using RealChute.Extensions;
using UnityEngine;

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
    public class TextureConfig
    {
        #region Propreties
        private string _name = string.Empty;
        /// <summary>
        /// Name of the texture config
        /// </summary>
        public string name
        {
            get { return this._name; }
        }

        private Dictionary<string, CaseConfig> _cases = new Dictionary<string, CaseConfig>();
        /// <summary>
        /// List of all the case configs
        /// </summary>
        public Dictionary<string, CaseConfig> cases
        {
            get { return this._cases; }
        }

        public Dictionary<string, CaseConfig[]> _types = new Dictionary<string, CaseConfig[]>();
        /// <summary>
        /// Dictionary of all the available parachute types associated with all the CaseConfigs which it apply it
        /// </summary>
        private Dictionary<string, CaseConfig[]> types
        {
            get { return this._types; }
        }

        private string[] _caseNames = new string[0];
        /// <summary>
        /// Array of the names of all the case configs available
        /// </summary>
        public string[] caseNames
        {
            get { return this._caseNames; }
        }

        private Dictionary<string, CanopyConfig> _canopies = new Dictionary<string, CanopyConfig>();
        /// <summary>
        /// List of all the canopy configs
        /// </summary>
        public Dictionary<string, CanopyConfig> canopies
        {
            get { return this._canopies; }
        }

        private string[] _canopyNames = new string[0];
        /// <summary>
        /// Array of the name of all the canopy configs available
        /// </summary>
        public string[] canopyNames
        {
            get { return this._canopyNames; }
        }

        private Dictionary<string, ModelConfig> _models = new Dictionary<string, ModelConfig>();
        /// <summary>
        /// List of all the model configs
        /// </summary>
        public Dictionary<string, ModelConfig> models
        {
            get { return this._models; }
        }

        private Dictionary<string, ModelConfig> _transforms = new Dictionary<string, ModelConfig>();
        /// <summary>
        /// Name of all the model transforms for parachutes with their associated ModelConfigs
        /// </summary>
        public Dictionary<string, ModelConfig> transforms
        {
            get { return this._transforms; }
        }

        private string[] _modelNames = new string[0];
        /// <summary>
        /// Array of the names of all the model configs available
        /// </summary>
        public string[] modelNames
        {
            get { return this._modelNames; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an empty TextureConfig
        /// </summary>
        public TextureConfig() { }

        /// <summary>
        /// Initiates all the texture and model nodes in this model config
        /// </summary>
        public TextureConfig(ConfigNode node)
        {
            node.TryGetValue("name", ref _name);

            this._cases = node.GetNodes("CASE_TEXTURE").Select(n => new CaseConfig(n)).ToDictionary(c => c.name, c => c);
            string[] distrinctTypes = this._cases.Values.SelectMany(c => c.types).Distinct().ToArray();
            this._caseNames = this._cases.Keys.ToArray();
            foreach(string type in distrinctTypes)
            {
                this._types.Add(type, this._cases.Values.Where(c => c.types.Contains(type)).ToArray());
            }

            this._canopies = node.GetNodes("CANOPY_TEXTURE").Select(n => new CanopyConfig(n)).ToDictionary(c => c.name, c => c);
            this._canopyNames = this._canopies.Keys.ToArray();

            this._models = node.GetNodes("CANOPY_MODEL").Select(n => new ModelConfig(n)).ToDictionary(m => m.name, m => m);
            this._modelNames = this._models.Keys.ToArray();
            foreach (ModelConfig model in models.Values)
            {
                model.parameters.ForEach(p => _transforms.Add(p.transformName, model));
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Wether the given case config exists
        /// </summary>
        /// <param name="name">Name of the config searched for</param>
        public bool ContainsCase(string name)
        {
            return _cases.ContainsKey(name);
        }

        public bool ContainsType(string type)
        {
            return this._types.ContainsKey(type);
        }

        /// <summary>
        /// Returns the Case config of the given name
        /// </summary>
        /// <param name="name">Name of the Config to obtain</param>
        public CaseConfig GetCase(string name)
        {
            if (!ContainsCase(name)) { throw new KeyNotFoundException("Could not find the \"" + name + "\" CaseConfig in the library"); }
            return this._cases[name];
        }

        /// <summary>
        /// Returns all the CaseConfigs of the given parachute type
        /// </summary>
        /// <param name="type">Type of the parachute</param>
        public CaseConfig[] GetCasesOfType(string type)
        {
            if (!ContainsType(type)) { throw new KeyNotFoundException("Could not find \"" + type + " parachute type in the library"); }
            return this._types[type];
        }

        /// <summary>
        /// Returns the case config at this index and type
        /// </summary>
        /// <param name="index">Index of the case config</param>
        /// <param name="type">Type of the parachute</param>
        public CaseConfig GetCase(int index, string type)
        {
            CaseConfig[] cases = GetCasesOfType(type);
            if (!cases.IndexInRange(index)) { throw new IndexOutOfRangeException("CaseConfig index [" + index + "] of \"" + type + "\" type is out of range"); }
            return cases[index];
        }

        /// <summary>
        /// Sees if the config exists and stores it in th ref value
        /// </summary>
        /// <param name="name">Name of the ref value</param>
        /// <param name="parachuteCase">Variable to store the result in</param>
        public bool TryGetCase(string name, ref CaseConfig parachuteCase)
        {
            if (ContainsCase(name))
            {
                parachuteCase = this._cases[name];
                return true;
            }
            if (!string.IsNullOrEmpty(name)) { Debug.LogError("[RealChute]: Could not find the CaseConfig  \"" + name + "\" in the library"); }
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
                CaseConfig[] cases = this._types[type];
                if (cases.IndexInRange(index))
                {
                    parachuteCase = cases[index];
                    return true;
                }
            }
            if (!string.IsNullOrEmpty(type)) { Debug.LogError("[RealChute]: Could not find the CaseConfig of \"" + type + "\" type at the [" + index + "] index in the library"); }
            return false;
        }

        /// <summary>
        /// Returns the index of this case if it exists
        /// </summary>
        /// <param name="parachuteCase">Case config searched for</param>
        public int GetCaseIndex(CaseConfig parachuteCase)
        {
            return this._caseNames.IndexOf(parachuteCase.name);
        }

        /// <summary>
        /// Wether or not the given canopy config exists
        /// </summary>
        /// <param name="name">Name of the config searched for</param>
        public bool ContainsCanopy(string name)
        {
            return this._canopies.ContainsKey(name);
        }

        /// <summary>
        /// Returns the config of the given name
        /// </summary>
        /// <param name="name"></param>
        public CanopyConfig GetCanopy(string name)
        {
            if (!ContainsCanopy(name)) { throw new KeyNotFoundException("Could not find the \"" + name + "\" CanopyConfig in the library"); }
            return _canopies[name];
        }

        /// <summary>
        /// Returns the canopy config at the given index
        /// </summary>
        /// <param name="index">Index of the config</param>
        public CanopyConfig GetCanopy(int index)
        {
            if (!this._canopyNames.IndexInRange(index)) { throw new IndexOutOfRangeException("CanopyConfig index [" + index + "] is out of range"); }
            return GetCanopy(this._canopyNames[index]);
        }

        /// <summary>
        /// Sees if the given canopy config exists and stores it in the ref value
        /// </summary>
        /// <param name="name">Name of the config searched for</param>
        /// <param name="canopy">Value to store th result in</param>
        public bool TryGetCanopy(string name, ref CanopyConfig canopy)
        {
            if (ContainsCanopy(name))
            {
                canopy = this._canopies[name];
                return true;
            }
            if (!string.IsNullOrEmpty(name)) { Debug.LogError("[RealChute]: Could not find the CanopyConfig \"" + name + "\" in the library"); }
            return false;
        }

        /// <summary>
        /// Tries to get the canopy config associated to the index and returns false if it does not exist
        /// </summary>
        /// <param name="index">Index of the canopy config</param>
        /// <param name="canopy">Value to store the result in</param>
        public bool TryGetCanopy(int index, ref CanopyConfig canopy)
        {
            if (this._canopyNames.IndexInRange(index))
            {
                string name = this._canopyNames[index];
                if (ContainsCanopy(name))
                {
                    canopy = this._canopies[name];
                    return true;
                }
            }
            Debug.LogError("[RealChute]: Could not find the CanopyConfig at the [" + index + "] index in the library");
            return false;
        }

        /// <summary>
        /// Returns the index of the canopy config if it exists
        /// </summary>
        /// <param name="canopy">Canopy config searched for</param>
        public int GetCanopyIndex(CanopyConfig canopy)
        {
            return this._canopyNames.IndexOf(canopy.name);
        }

        /// <summary>
        /// Wether the model config of the given name exists
        /// </summary>
        /// <param name="name">Name of the config searched for</param>
        /// <param name="isTransformName">If the name is transform or config name</param>
        public bool ContainsModel(string name, bool isTransformName = false)
        {
            return isTransformName ? this._transforms.ContainsKey(name) : this._models.ContainsKey(name);
        }

        /// <summary>
        /// Gets the model config from the library
        /// </summary>
        /// <param name="name">Name of the config or the transform searched for</param>
        /// <param name="isTransformName">If the name is transform or config name</param>
        public ModelConfig GetModel(string name, bool isTransformName = false)
        {
            if (isTransformName)
            {
                if (!ContainsModel(name, true)) { throw new KeyNotFoundException("Could not find transform \"" + name + "\" in the library"); }
                return this._transforms[name];
            }
            if (!ContainsModel(name)) { throw new KeyNotFoundException("Could not find ModelConfig \"" + name + "\" in the library"); }
            return this._models[name];
        }

        /// <summary>
        /// Gets the ModelConfig at the specified index
        /// </summary>
        /// <param name="index">Index of the config to get</param>
        public ModelConfig GetModel(int index)
        {
            if (this._modelNames.IndexInRange(index)) { throw new IndexOutOfRangeException("ModelConfig index [" + index + "] is out of range"); }
            return GetModel(this._modelNames[index]);
        }

        /// <summary>
        /// Sees if the model config of the given name exists and stores it in the ref value
        /// </summary>
        /// <param name="name">Name of the config searched for</param>
        /// <param name="model">Value to store the result in</param>
        public bool TryGetModel(string name, ref ModelConfig model, bool isTransformName = false)
        {
            if (isTransformName)
            {
                if (ContainsModel(name, true))
                {
                    model = this._transforms[name];
                    return true;
                }
                if (!string.IsNullOrEmpty(name)) { Debug.LogError("[RealChute]: Could not find the transform \"" + name + "\" in the library"); }
                return false;
            }

            if (ContainsModel(name))
            {
                model = this._models[name];
                return true;
            }
            if (!string.IsNullOrEmpty(name)) { Debug.LogError("[RealChute]: Could not find the ModelConfig \"" + name + "\" in the library"); }
            return false;
        }

        /// <summary>
        /// Sees if there is a ModelConfig at the specified index and stores it in the ref value if possible.
        /// </summary>
        /// <param name="index">Index of the ModelConfig to look for</param>
        /// <param name="model">Value to store the result in</param>
        public bool TryGetModel(int index, ref ModelConfig model)
        {
            if (this._modelNames.IndexInRange(index))
            {
                string name = this._modelNames[index];
                if (ContainsModel(name))
                {
                    model = this._models[name];
                    return true;
                }
            }
            Debug.LogError("[RealChute]: Could not find the ModelConfig at the index [" + index + "] in the library");
            return false;
        }

        /// <summary>
        /// Returns the index of the model config if it exists
        /// </summary>
        /// <param name="model">Model config searched for</param>
        public int GetModelIndex(ModelConfig model)
        {
            return this._modelNames.IndexOf(model.name);
        }
        #endregion
    }
}
