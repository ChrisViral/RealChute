using System.Collections.Generic;
using System.Linq;

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
    public class PresetsLibrary
    {
        #region Instance
        private static PresetsLibrary _instance = null;
        /// <summary>
        /// Returns the current PresetsLibrary
        /// </summary>
        public static PresetsLibrary instance
        {
            get
            {
                if (_instance == null) { _instance = new PresetsLibrary(); }
                return _instance;
            }
        }
        #endregion

        #region Propreties
        private List<Preset> _presets = new List<Preset>();
        /// <summary>
        /// List of all the presets currently stored
        /// </summary>
        public List<Preset> presets
        {
            get { return this._presets; }
        }

        /// <summary>
        /// Names of the presets
        /// </summary>
        public string[] presetNames
        {
            get { return this.presets.Select(p => p.name).ToArray(); }
        }
        #endregion

        #region Constuctor
        /// <summary>
        /// Generates a new PresetsLibrary
        /// </summary>
        public PresetsLibrary()
        {
            if (System.IO.File.Exists(RCUtils.settingsURL))
            {
                ConfigNode settings = ConfigNode.Load(RCUtils.settingsURL);
                if (settings.HasNode("REALCHUTE_SETTINGS"))
                {
                    _presets.AddRange(settings.GetNode("REALCHUTE_SETTINGS").GetNodes("PRESET").Select(n => new Preset(n)));
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// If the Preset of the given name exists
        /// </summary>
        /// <param name="name">Name of the preset to look for</param>
        public bool PresetExists(string name)
        {
            return presets.Any(p => p.name == name);
        }

        /// <summary>
        /// Returns the preset of the given name
        /// </summary>
        /// <param name="name">Name of the preset to get</param>
        public Preset GetPreset(string name)
        {
            return presets.Single(p => p.name == name);
        }

        /// <summary>
        /// Returns the preset at the given index
        /// </summary>
        /// <param name="index">Index of the Preset to get</param>
        public Preset GetPreset(int index)
        {
            return presets[index];
        }

        /// <summary>
        /// Tries to get the preset of the given name and stores it in the ref value
        /// </summary>
        /// <param name="name">Name of the preset to find</param>
        /// <param name="preset">Value to store the result in</param>
        public bool TryGetPreset(string name, ref Preset preset)
        {
            if (PresetExists(name))
            {
                preset = GetPreset(name);
                return true;
            }
            UnityEngine.Debug.Log("[RealChute]: Could not find the preset " + name + " within the config");
            return false;
        }

        /// <summary>
        /// Tries to get the preset at the given index and stores it in the ref value
        /// </summary>
        /// <param name="index">Index of the preset to find</param>
        /// <param name="preset">Value to store the result in</param>
        public bool TryGetPreset(int index, ref Preset preset)
        {
            if (presets.Count > index + 1)
            {
                preset = GetPreset(index);
                return true;
            }
            UnityEngine.Debug.Log("[RealChute]: Could not find the preset at the index [" + index + "] within the config");
            return false;
        }

        /// <summary>
        /// Returns the names of the presets that have a secondary chute or not
        /// </summary>
        /// <param name="pChute">Chute to determine the result from</param>
        public string[] GetRelevantPresets(ProceduralChute pChute)
        {
            return presets.Where(p => p.parameters.Count == pChute.chutes.Count).Select(p => p.name).ToArray();
        }

        /// <summary>
        /// Adds the given preset to the library
        /// </summary>
        /// <param name="preset">Preset to add</param>
        public void AddPreset(Preset preset)
        {
            this._presets.Add(preset);
            RealChuteSettings.SaveSettings();
        }

        /// <summary>
        /// Removes the preset of the given name from the library
        /// </summary>
        /// <param name="name">Name of the preset to delete</param>
        public void DeletePreset(Preset preset)
        {
            this._presets.Remove(preset);
            RealChuteSettings.SaveSettings();
        }
        #endregion
    }
}
