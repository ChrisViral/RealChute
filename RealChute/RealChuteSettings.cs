using System.IO;
using RealChute.Libraries.Presets;
using UnityEngine;

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
        #region Instance
        /// <summary>
        /// Returns the current RealChute_Settings config file
        /// </summary>
        public static RealChuteSettings Instance { get; } = new();
        #endregion

        #region Propreties
        private bool autoArm = true;
        /// <summary>
        /// If parachutes must automatically arm when staged
        /// </summary>
        public bool AutoArm
        {
            get => this.autoArm;
            set => this.autoArm = value;
        }

        private bool jokeActivated;
        /// <summary>
        /// If April Fools joke is activated
        /// </summary>
        public bool JokeActivated
        {
            get => this.jokeActivated;
            set => this.jokeActivated = value;
        }

        private bool mustBeEngineer = true;
        /// <summary>
        /// If a Kerbal must be an engineer to repack a parachute in career
        /// </summary>
        public bool MustBeEngineer
        {
            get => this.mustBeEngineer;
            set => this.mustBeEngineer = value;
        }

        private int engineerLevel = 1;
        /// <summary>
        /// The level at which an engineer must be to be able to repack a parachute
        /// </summary>
        public int EngineerLevel
        {
            get => this.engineerLevel;
            set => this.engineerLevel = value;
        }

        private bool activateNyan;
        /// <summary>
        /// Whether or not NyanMode™ is activated
        /// </summary>
        public bool ActivateNyan
        {
            get => this.activateNyan;
            set => this.activateNyan = value;
        }

        /// <summary>
        /// All the current preset nodes
        /// </summary>
        public ConfigNode[] Presets { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Loads the RealChute_Settings config to memory
        /// </summary>
        public RealChuteSettings()
        {
            ConfigNode node = new(), settings = new("REALCHUTE_SETTINGS");
            Debug.Log("[RealChute]: Loading settings file.");
            if (!File.Exists(RCUtils.SettingsURL))
            {
                Debug.LogError("[RealChute]: RealChute_Settings.cfg is missing. Creating new.");
                settings.AddValue("autoArm", this.autoArm);
                settings.AddValue("jokeActivated", this.jokeActivated);
                settings.AddValue("mustBeEngineer", this.mustBeEngineer);
                settings.AddValue("engineerLevel", this.engineerLevel);
                settings.AddValue("activateNyan", this.activateNyan);
                node.AddNode(settings);
                this.Presets = [];
                node.Save(RCUtils.SettingsURL);
            }
            else
            {
                node = ConfigNode.Load(RCUtils.SettingsURL);
                if (!node.TryGetNode("REALCHUTE_SETTINGS", ref settings))
                {
                    SaveSettings();
                    return;
                }

                bool allValuesFound = settings.TryGetValue("autoArm", ref this.autoArm);
                allValuesFound     &= settings.TryGetValue("jokeActivated", ref this.jokeActivated);
                allValuesFound     &= settings.TryGetValue("mustBeEngineer", ref this.mustBeEngineer);
                allValuesFound     &= settings.TryGetValue("engineerLevel", ref this.engineerLevel);
                allValuesFound     &= settings.TryGetValue("activateNyan", ref this.activateNyan);
                this.Presets = settings.GetNodes("PRESET");

                if (!allValuesFound) SaveSettings();
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Saves the RealChute_Settings config into GameData
        /// </summary>
        public static void SaveSettings()
        {
            ConfigNode settings = new("REALCHUTE_SETTINGS"), node = new();
            settings.AddValue("autoArm", Instance.autoArm);
            settings.AddValue("jokeActivated", Instance.jokeActivated);
            settings.AddValue("mustBeEngineer", Instance.mustBeEngineer);
            settings.AddValue("engineerLevel", Instance.engineerLevel);
            settings.AddValue("activateNyan", Instance.activateNyan);

            if (PresetsLibrary.Instance.Presets.Count > 0)
            {
                foreach (Preset preset in PresetsLibrary.Instance.Presets.Values)
                {
                    settings.AddNode(preset.Save());
                }
            }

            node.AddNode(settings);
            node.Save(RCUtils.SettingsURL);
            Debug.Log("[RealChute]: Saved settings file.");
        }
        #endregion
    }
}
