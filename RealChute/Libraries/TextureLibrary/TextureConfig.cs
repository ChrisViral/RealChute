using System.Collections.Generic;
using System.Linq;
using RealChute.Extensions;
using UnityEngine;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more informtion contact me on the forum. */

namespace RealChute
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

        private List<CaseConfig> _cases = new List<CaseConfig>();
        /// <summary>
        /// List of all the case configs
        /// </summary>
        public List<CaseConfig> cases
        {
            get { return this._cases; }
        }

        private string[] _caseNames = new string[] { };
        /// <summary>
        /// Array of the names of all the case configs available
        /// </summary>
        public string[] caseNames
        {
            get { return this._caseNames; }
        }

        private List<CanopyConfig> _canopies = new List<CanopyConfig>();
        /// <summary>
        /// List of all the canopy configs
        /// </summary>
        public List<CanopyConfig> canopies
        {
            get { return this._canopies; }
        }

        private string[] _canopyNames = new string[] { };
        /// <summary>
        /// Array of the name of all the canopy configs available
        /// </summary>
        public string[] canopyNames
        {
            get { return this._canopyNames; }
        }

        private List<ModelConfig> _models = new List<ModelConfig>();
        /// <summary>
        /// List of all the model configs
        /// </summary>
        public List<ModelConfig> models
        {
            get { return this._models; }
        }

        private string[] _modelNames = new string[] { };
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
            foreach(ConfigNode cfg in node.nodes)
            {
                if (cfg.name == "CASE_TEXTURE")
                {
                    CaseConfig parachuteCase = new CaseConfig(cfg);
                    _cases.Add(parachuteCase);
                    continue;
                }

                if (cfg.name == "CANOPY_TEXTURE")
                {
                    CanopyConfig canopy = new CanopyConfig(cfg);
                    _canopies.Add(canopy);
                    continue;
                }

                if (cfg.name == "CANOPY_MODEL")
                {
                    ModelConfig model = new ModelConfig(cfg);
                    _models.Add(model);
                    continue;
                }
            }
            _caseNames = _cases.Select(c => c.name).ToArray();
            _canopyNames = _canopies.Select(c => c.name).ToArray();
            _modelNames = _models.Select(m => m.name).ToArray();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Wether the given case config exists
        /// </summary>
        /// <param name="name">Name of the config searched for</param>
        public bool CaseExists(string name)
        {
            return cases.Exists(_case => _case.name == name);
        }

        /// <summary>
        /// Returns the Case config of the given name
        /// </summary>
        /// <param name="name">Name of the Config to obtain</param>
        public CaseConfig GetCase(string name)
        {
            return cases.Find(c => c.name == name);
        }

        /// <summary>
        /// Returns the case config at this index
        /// </summary>
        /// <param name="index">Index of the case config</param>
        public CaseConfig GetCase(int index, string type)
        {
            return cases.Where(c => c.types.Contains(type)).ToArray()[index];
        }

        /// <summary>
        /// Sees if the config exists and stores it in th ref value
        /// </summary>
        /// <param name="name">Name of the ref value</param>
        /// <param name="parachuteCase">Variable to store the result in</param>
        public bool TryGetCase(string name, ref CaseConfig parachuteCase)
        {
            if (CaseExists(name))
            {
                parachuteCase = GetCase(name);
                return true;
            }
            if (name != string.Empty) { Debug.LogWarning("[RealChute]: Could not find the " + name + " case texture within library"); }
            return false;
        }

        /// <summary>
        /// Gets the case config of the given index if possible
        /// </summary>
        /// <param name="index">Index of the case config searched for</param>
        /// <param name="parachuteCase">Value to store the result in</param>
        public bool TryGetCase(int index, string type, ref CaseConfig parachuteCase)
        {
            if (CaseExists(caseNames[index]))
            {
                parachuteCase = GetCase(index, type);
                return true;
            }
            Debug.LogWarning("[RealChute]: Could not find the case texture at  the index [" + index + "] within library");
            return false;
        }

        /// <summary>
        /// Returns the index of this case if it exists
        /// </summary>
        /// <param name="parachuteCase">Case config searched for</param>
        public int GetCaseIndex(CaseConfig parachuteCase)
        {
            for (int i = 0; i < caseNames.Length; i++)
            {
                if (caseNames[i] == parachuteCase.name) { return i; }
            }
            return 0;
        }

        /// <summary>
        /// Wether or not the given canopy config exists
        /// </summary>
        /// <param name="name">Name of the config searched for</param>
        public bool CanopyExists(string name)
        {
            return canopies.Exists(canopy => canopy.name == name);
        }

        /// <summary>
        /// Returns the config of the given name
        /// </summary>
        /// <param name="name"></param>
        public CanopyConfig GetCanopy(string name)
        {
            return canopies.Find(canopy => canopy.name == name);
        }

        /// <summary>
        /// Returns the canopy config at the given index
        /// </summary>
        /// <param name="index">Index of the config</param>
        public CanopyConfig GetCanopy(int index)
        {
            return canopies[index];
        }

        /// <summary>
        /// Sees if the given canopy config exists and stores it in the ref value
        /// </summary>
        /// <param name="name">Name of the config searched for</param>
        /// <param name="canopy">Value to store th result in</param>
        public bool TryGetCanopy(string name, ref CanopyConfig canopy)
        {
            if (CanopyExists(name))
            {
                canopy = GetCanopy(name);
                return true;
            }
            if (name != string.Empty) { Debug.LogWarning("[RealChute]: Could not find the " + name + " canopy texture within library"); }
            return false;
        }

        /// <summary>
        /// Tries to get the canopy config associated to the index and returns false if it does not exist
        /// </summary>
        /// <param name="index">Index of the canopy config</param>
        /// <param name="canopy">Value to store the result in</param>
        public bool TryGetCanopy(int index, ref CanopyConfig canopy)
        {
            if (CanopyExists(canopyNames[index]))
            {
                canopy = GetCanopy(index);
                return true;
            }
            Debug.LogWarning("[RealChute]: Could not find the canopy texture at the index [" + index + "] within library");
            return false;
        }

        /// <summary>
        /// Returns the index of the canopy config if it exists
        /// </summary>
        /// <param name="canopy">Canopy config searched for</param>
        public int GetCanopyIndex(CanopyConfig canopy)
        {
            for (int i = 0; i < canopyNames.Length; i++)
            {
                if (canopyNames[i] == canopy.name) { return i; }
            }
            return 0;
        }

        /// <summary>
        /// Wether the model config of the given name exists
        /// </summary>
        /// <param name="name">Name of the config searched for</param>
        /// <param name="isTransformName">If the name is transform or config name</param>
        public bool ModelExists(string name, bool isTransformName = false)
        {
            if (!isTransformName) { return models.Exists(model => model.name == name); }
            else { return models.Exists(model => model.main.transformName == name || model.secondary.transformName == name); }
        }

        /// <summary>
        /// Gets the model config from the library
        /// </summary>
        /// <param name="name">Name of the config or the transform searched for</param>
        /// <param name="isTransformName">If the name is transform or config name</param>
        public ModelConfig GetModel(string name, bool isTransformName = false)
        {
            if (!isTransformName) { return models.Find(model => model.name == name); }
            else { return models.Find(model => model.main.transformName == name || model.secondary.transformName == name); }
        }

        /// <summary>
        /// Gets the ModelConfig at the specified index
        /// </summary>
        /// <param name="index">Index of the config to get</param>
        public ModelConfig GetModel(int index)
        {
            return models[index];
        }

        /// <summary>
        /// Sees if the model config of the given name exists and stores it in the ref value
        /// </summary>
        /// <param name="name">Name of the config searched for</param>
        /// <param name="model">Value to store the result in</param>
        public bool TryGetModel(string name, ref ModelConfig model, bool isTransformName = false)
        {
            if (ModelExists(name, isTransformName))
            {
                model = GetModel(name, isTransformName);
                return true;
            }
            if (name != string.Empty) { Debug.LogWarning("[RealChute]: Could not find the " + name + " parachute model within library"); }
            return false;
        }

        /// <summary>
        /// Sees if there is a ModelConfig at the specified index and stores it in the ref value if possible.
        /// </summary>
        /// <param name="index">Index of the ModelConfig to look for</param>
        /// <param name="model">Value to store the result in</param>
        public bool TryGetModel(int index, ref ModelConfig model)
        {
            if (ModelExists(modelNames[index]))
            {
                model = GetModel(index);
                return true;
            }
            Debug.LogWarning("[RealChute]: Could not find the parachute model at the index [" + index + "] within library");
            return false;
        }

        /// <summary>
        /// Returns the index of the model config if it exists
        /// </summary>
        /// <param name="model">Model config searched for</param>
        public int GetModelIndex(ModelConfig model)
        {
            for (int i = 0; i < modelNames.Length; i++)
            {
                if (modelNames[i] == model.name) { return i; }
            }
            return 0;
        }
        #endregion
    }
}
