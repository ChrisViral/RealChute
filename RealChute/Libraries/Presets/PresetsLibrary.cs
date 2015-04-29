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

namespace RealChute.Libraries.Presets
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
        private Dictionary<string, Preset> _presets = new Dictionary<string, Preset>();
        /// <summary>
        /// Dictionary of the preset names with their associated presets
        /// </summary>
        public Dictionary<string, Preset> presets
        {
            get { return this._presets; }
        }

        private Dictionary<int, string[]> _parameters = new Dictionary<int, string[]>();
        /// <summary>
        /// A dictionary of the number of used chutes as keys and the associated preset names as values
        /// </summary>
        public Dictionary<int, string[]> parameters
        {
            get { return this._parameters; }
        }
        #endregion

        #region Constuctor
        /// <summary>
        /// Generates a new PresetsLibrary
        /// </summary>
        public PresetsLibrary()
        {
            this._presets = RealChuteSettings.fetch.presets.Select(n => new Preset(n)).ToDictionary(p => p.name, p => p);
            RefreshData();
        }
        #endregion

        #region Methods
        /// <summary>
        /// If the Preset of the given name exists
        /// </summary>
        /// <param name="name">Name of the preset to look for</param>
        public bool ContainsPreset(string name)
        {
            return this._presets.ContainsKey(name);
        }

        /// <summary>
        /// Returns the preset of the given name
        /// </summary>
        /// <param name="name">Name of the preset to get</param>
        public Preset GetPreset(string name)
        {
            if (!ContainsPreset(name)) { throw new KeyNotFoundException("Could not find the \"" + name + "\" Preset in the library"); }
            return this._presets[name];
        }

        /// <summary>
        /// Returns the names of the presets that have a secondary chute or not
        /// </summary>
        /// <param name="chuteCount">Number of parachutes associated to the Preset</param>
        public string[] GetRelevantPresets(int chuteCount)
        {
            if (!this._parameters.ContainsKey(chuteCount)) { return new string[0]; }
            return this._parameters[chuteCount];
        }

        /// <summary>
        /// Returns the preset at the given index
        /// </summary>
        /// <param name="index">Index of the Preset to get</param>
        /// <param name="chuteCount">Number of parachutes associated to the Preset</param>
        public Preset GetPreset(int index, int chuteCount)
        {
            string[] relevant = GetRelevantPresets(chuteCount);
            if (!relevant.IndexInRange(index)) { throw new IndexOutOfRangeException("Preset index [" + index + "] for " + chuteCount + " chutes is out of range"); }
            return GetPreset(relevant[index]);
        }

        /// <summary>
        /// Adds the given preset to the library
        /// </summary>
        /// <param name="preset">Preset to add</param>
        public void AddPreset(Preset preset)
        {
            this._presets.Add(preset.name, preset);
            RefreshData();
            RealChuteSettings.SaveSettings();
        }

        /// <summary>
        /// Removes the preset of the given name from the library
        /// </summary>
        /// <param name="name">Name of the preset to delete</param>
        public void DeletePreset(Preset preset)
        {
            this._presets.Remove(preset.name);
            RefreshData();
            RealChuteSettings.SaveSettings();
        }

        /// <summary>
        /// Correctly refreshes the dictionaries and arrays of Preset data
        /// </summary>
        private void RefreshData()
        {
            if (this._presets.Count <= 0) { return; }
            int max = this._presets.Values.Select(p => p.parameters.Count).Max();
            for (int i = 1; i <= max; i++)
            {
                if (!this._parameters.ContainsKey(i)) { this._parameters.Add(i, new string[0]); }
                this._parameters[i] = this._presets.Values.Where(p => p.parameters.Count == i).Select(p => p.name).ToArray();
            }
        }
        #endregion
    }
}
