using System.IO;
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

namespace RealChute;

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

    private bool requireChargeToDeploy = true;
    /// <summary>
    /// If ElectricCharge is required to deploy the parachutes
    /// </summary>
    public bool RequireChargeToDeploy
    {
        get => this.requireChargeToDeploy;
        set => this.requireChargeToDeploy = value;
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

    private bool nyanMode;
    /// <summary>
    /// Whether or not NyanMode™ is activated
    /// </summary>
    public bool NyanMode
    {
        get => this.nyanMode;
        set => this.nyanMode = value;
    }
    #endregion

    #region Constructor
    /// <summary>
    /// Loads the RealChute_Settings config to memory
    /// </summary>
    private RealChuteSettings()
    {
        Debug.Log("[RealChute]: Loading settings file.");
        if (!File.Exists(RCUtils.SettingsURL))
        {
            Debug.LogWarning("[RealChute]: RealChute_Settings.cfg is missing. Creating new.");
            SaveSettings();
            return;
        }

        ConfigNode file = ConfigNode.Load(RCUtils.SettingsURL), settings = null;
        if (file is null || !file.TryGetNode("REALCHUTE_SETTINGS", ref settings))
        {
            Debug.LogWarning("[RealChute]: Could not load RealChute settings, resetting to defaults");
            SaveSettings();
            return;
        }

        settings.TryGetValue("autoArm", ref this.autoArm);
        settings.TryGetValue("mustBeEngineer", ref this.mustBeEngineer);
        settings.TryGetValue("engineerLevel", ref this.engineerLevel);
        settings.TryGetValue("requireChargeToDeploy", ref this.requireChargeToDeploy);
        settings.TryGetValue("jokeActivated", ref this.jokeActivated);
        settings.TryGetValue("activateNyan", ref this.nyanMode);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Saves the RealChute_Settings config into GameData
    /// </summary>
    public void SaveSettings()
    {
        ConfigNode settings = new("REALCHUTE_SETTINGS");
        settings.AddValue("autoArm", this.AutoArm);
        settings.AddValue("mustBeEngineer", this.MustBeEngineer);
        settings.AddValue("engineerLevel", this.EngineerLevel);
        settings.AddValue("requireChargeToDeploy", this.RequireChargeToDeploy);
        settings.AddValue("jokeActivated", this.JokeActivated);
        settings.AddValue("activateNyan", this.NyanMode);

        ConfigNode file = new();
        file.AddNode(settings);
        file.Save(RCUtils.SettingsURL);
        Debug.Log("[RealChute]: Saved settings file.");
    }
    #endregion
}
