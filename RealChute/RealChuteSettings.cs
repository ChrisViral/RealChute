using System.IO;
using UnityEngine;
using RealChute.Extensions;
using RealChute.Utils;
using RealChute.Libraries.Presets;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute
{
    public class RealChuteSettings
    {
        #region Fetch
        private static RealChuteSettings _fetch = null;
        /// <summary>
        /// Returns the current RealChute_Settings config file
        /// </summary>
        public static RealChuteSettings fetch
        {
            get
            {
                if (_fetch == null) { _fetch = new RealChuteSettings(); }
                return _fetch;
            }
        }
        #endregion

        #region Propreties
        private bool _autoArm = false;
        /// <summary>
        /// If parachutes must automatically arm when staged
        /// </summary>
        public bool autoArm
        {
            get { return this._autoArm; }
            set { this._autoArm = value; }
        }

        private bool _jokeActivated = false;
        /// <summary>
        /// If April Fools joke is activated
        /// </summary>
        public bool jokeActivated
        {
            get { return this._jokeActivated; }
            set { this._jokeActivated = value; }
        }

        private bool _guiResizeUpdates = false;
        /// <summary>
        /// Whether or not resizing the parachutes through part GUI updates the canopy diameter
        /// </summary>
        public bool guiResizeUpdates
        {
            get { return this._guiResizeUpdates; }
            set { this._guiResizeUpdates = value; }
        }

        private bool _mustBeEngineer = true;
        /// <summary>
        /// If a Kerbal must be an engineer to repack a parachute in career
        /// </summary>
        public bool mustBeEngineer
        {
            get { return this._mustBeEngineer; }
            set { this._mustBeEngineer = value; }
        }

        private int _engineerLevel = 1;
        /// <summary>
        /// The level at which an engineer must be to be able to repack a parachute
        /// </summary>
        public int engineerLevel
        {
            get { return this._engineerLevel; }
            set { this._engineerLevel = value; }
        }

        private ConfigNode[] _presets = new ConfigNode[0];
        /// <summary>
        /// All the current preset nodes
        /// </summary>
        public ConfigNode[] presets
        {
            get { return this._presets; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Loads the RealChute_Settings config to memory
        /// </summary>
        public RealChuteSettings()
        {
            ConfigNode node = new ConfigNode(), settings = new ConfigNode("REALCHUTE_SETTINGS");
            Debug.Log("[RealChute]: Loading settings file.");
            if (!File.Exists(RCUtils.settingsURL))
            {
                Debug.LogError("[RealChute]: RealChute_Settings.cfg is missing. Creating new.");
                settings.AddValue("autoArm", this._autoArm);
                settings.AddValue("jokeActivated", this._jokeActivated);
                settings.AddValue("guiResizeUpdates", this._guiResizeUpdates);
                settings.AddValue("mustBeEngineer", this._mustBeEngineer);
                settings.AddValue("engineerLevel", this._engineerLevel);
                node.AddNode(settings);
                node.Save(RCUtils.settingsURL);
            }
            else
            {
                node = ConfigNode.Load(RCUtils.settingsURL);
                bool mustSave = false;
                if (!node.TryGetNode("REALCHUTE_SETTINGS", ref settings)) { SaveSettings(); return; }
                if (!settings.TryGetValue("autoArm", ref this._autoArm)) { mustSave = true; return; }
                if (!settings.TryGetValue("jokeActivated", ref this._jokeActivated)) { mustSave = true; return; }
                if (!settings.TryGetValue("guiResizeUpdates", ref this._guiResizeUpdates)) { mustSave = true; return; }
                if (!settings.TryGetValue("mustBeEngineer", ref this._mustBeEngineer)) { mustSave = true; return; }
                if (!settings.TryGetValue("engineerLevel", ref this._engineerLevel)) { mustSave = true; return; }
                this._presets = settings.GetNodes("PRESET");
                if (mustSave) { SaveSettings(); }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Saves the RealChute_Settings config into GameData
        /// </summary>
        public static void SaveSettings()
        {
            ConfigNode settings = new ConfigNode("REALCHUTE_SETTINGS"), node = new ConfigNode();
            settings.AddValue("autoArm", fetch._autoArm);
            settings.AddValue("jokeActivated", fetch._jokeActivated);
            settings.AddValue("guiResizeUpdates", fetch._guiResizeUpdates);
            settings.AddValue("mustBeEngineer", fetch._mustBeEngineer);
            settings.AddValue("engineerLevel", fetch._engineerLevel);
            if (PresetsLibrary.instance.presets.Count > 0)
            {
                foreach (Preset preset in PresetsLibrary.instance.presets.Values)
                {
                    settings.AddNode(preset.Save());
                }
            }
            node.AddNode(settings);
            node.Save(RCUtils.settingsURL);
            Debug.Log("[RealChute]: Saved settings file.");
        }
        #endregion
    }
}
