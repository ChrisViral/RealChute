using System;
using ClickThroughFix;
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

namespace RealChute;

public class SettingsWindow : MonoBehaviour
{
    #region Fields
    private readonly int id = Guid.NewGuid().GetHashCode();
    private bool showing = true;
    private string level = string.Empty;
    private Rect window, drag;
    #endregion

    #region Methods
    private void HideUI() => this.showing = false;

    private void ShowUI() => this.showing = true;

    private void CloseWindow()
    {
        if (int.TryParse(this.level, out int i)) { RealChuteSettings.Instance.EngineerLevel = i; }

        RCToolbarManager.Instance.RequestHide();
    }
    #endregion

    #region Initialization
    private void Awake()
    {
        if (!CompatibilityChecker.IsAllCompatible) { Destroy(this); return; }
        this.window = new Rect(100f * GameSettings.UI_SCALE, 100f * GameSettings.UI_SCALE, 300f * GameSettings.UI_SCALE, 150f * GameSettings.UI_SCALE);
        this.drag   = new Rect(0f, 0f, 300f * GameSettings.UI_SCALE, 25f * GameSettings.UI_SCALE);
        this.level  = RealChuteSettings.Instance.EngineerLevel.ToString();

        GameEvents.onShowUI.Add(ShowUI);
        GameEvents.onHideUI.Add(HideUI);
        GameEvents.onGUIAstronautComplexSpawn.Add(HideUI);
        GameEvents.onGUIAstronautComplexDespawn.Add(ShowUI);
        GameEvents.onGUIRnDComplexSpawn.Add(HideUI);
        GameEvents.onGUIRnDComplexDespawn.Add(ShowUI);
        GameEvents.onGUIMissionControlSpawn.Add(HideUI);
        GameEvents.onGUIMissionControlDespawn.Add(ShowUI);
        GameEvents.onGUIAdministrationFacilitySpawn.Add(HideUI);
        GameEvents.onGUIAdministrationFacilityDespawn.Add(ShowUI);
    }

    private void OnDestroy()
    {
        if (!CompatibilityChecker.IsAllCompatible) { return; }
        CloseWindow();
        RealChuteSettings.Instance.SaveSettings();

        GameEvents.onShowUI.Remove(ShowUI);
        GameEvents.onHideUI.Remove(HideUI);
        GameEvents.onGUIAstronautComplexSpawn.Remove(HideUI);
        GameEvents.onGUIAstronautComplexDespawn.Remove(ShowUI);
        GameEvents.onGUIRnDComplexSpawn.Remove(HideUI);
        GameEvents.onGUIRnDComplexDespawn.Remove(ShowUI);
        GameEvents.onGUIMissionControlSpawn.Remove(HideUI);
        GameEvents.onGUIMissionControlDespawn.Remove(ShowUI);
        GameEvents.onGUIAdministrationFacilitySpawn.Remove(HideUI);
        GameEvents.onGUIAdministrationFacilityDespawn.Remove(ShowUI);
    }
    #endregion

    #region GUI
    private void OnGUI()
    {
        if (!CompatibilityChecker.IsAllCompatible|| !this.showing) { return; }

        GUI.skin    = HighLogic.Skin;
        this.window = ClickThruBlocker.GUILayoutWindow(this.id, this.window, Window, "RealChute Settings " + RCUtils.AssemblyVersion, GUIUtils.ScaledWindow);
    }

    private void Window(int windowId)
    {
        GUI.DragWindow(this.drag);

        RealChuteSettings.Instance.AutoArm        = GUILayout.Toggle(RealChuteSettings.Instance.AutoArm, "Automatically arm when staging", GUIUtils.ScaledToggle);

        RealChuteSettings.Instance.MustBeEngineer = GUILayout.Toggle(RealChuteSettings.Instance.MustBeEngineer, "Only engineers can repack in career", GUIUtils.ScaledToggle);
        if (!RealChuteSettings.Instance.MustBeEngineer) { GUI.enabled = false; }
        GUIUtils.CreateEntryArea("Engineer minimum level to repack:", ref this.level, 0f, 5f, 100f);
        GUI.enabled = true;

        RealChuteSettings.Instance.RequireChargeToDeploy = GUILayout.Toggle(RealChuteSettings.Instance.RequireChargeToDeploy, "Require Electric Charge to Deploy Parachutes", GUIUtils.ScaledToggle);

        RealChuteSettings.Instance.JokeActivated = GUILayout.Toggle(RealChuteSettings.Instance.JokeActivated, "Activate April Fools' joke (DANGER!!)", GUIUtils.ScaledToggle);

        RealChuteSettings.Instance.NyanMode      = GUILayout.Toggle(RealChuteSettings.Instance.NyanMode, "Activate NyanMode™", GUIUtils.ScaledToggle);

        GUIUtils.CenteredButton("Close", CloseWindow, 100f);
    }
    #endregion
}
