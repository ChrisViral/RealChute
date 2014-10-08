using System.IO;
using UnityEngine;
using RealChute.Extensions;
using RealChute.Libraries;

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
                Debug.LogWarning("[RealChute]: RealChute_Settings.cfg is missing. Creating new.");
                settings.AddValue("autoArm", autoArm);
                settings.AddValue("jokeActivated", jokeActivated);
                node.AddNode(settings);
                node.Save(RCUtils.settingsURL);
            }
            else
            {
                node = ConfigNode.Load(RCUtils.settingsURL);
                if (!node.TryGetNode("REALCHUTE_SETTINGS", ref settings)) { goto missing; }
                if (!settings.TryGetValue("autoArm", ref _autoArm)) { goto missing; }
                if (!settings.TryGetValue("jokeActivated", ref _jokeActivated)) { goto missing; }
                return;

                missing:
                {
                    Debug.LogWarning("[RealChute]: RealChute_Settings.cfg is missing component. Fixing settings file.");
                    settings.ClearValues();
                    settings.AddValue("autoArm", autoArm);
                    settings.AddValue("jokeActivated", jokeActivated);
                    node.ClearData();
                    node.AddNode(settings);
                    node.Save(RCUtils.settingsURL);
                    return;
                }
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
            settings.AddValue("autoArm", fetch.autoArm);
            settings.AddValue("jokeActivated", fetch.jokeActivated);
            if (PresetsLibrary.instance.presets.Count > 0)
            {
                PresetsLibrary.instance.presets.ForEach(p => settings.AddNode(p.Save()));
            }
            node.AddNode(settings);
            node.Save(RCUtils.settingsURL);
            Debug.Log("[RealChute]: Saved settings file.");
        }
        #endregion
    }
}
