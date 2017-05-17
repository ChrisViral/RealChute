using System;
using RealChute.Utils;
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
    public class SettingsWindow : MonoBehaviour
    {
        #region Fields
        private readonly int id = Guid.NewGuid().GetHashCode();
        private bool showing = true, destroying;
        private string level = string.Empty;
        private Rect window, drag;
        #endregion

        #region Methods
        private void HideUI()
        {
            this.showing = false;
        }

        private void ShowUI()
        {
            this.showing = true;
        }

        private void CloseWindow()
        {
            int i;
            if (int.TryParse(this.level, out i)) { RealChuteSettings.Instance.EngineerLevel = i; }
            if (!this.destroying) { RCToolbarManager.SetApplauncherButtonFalse(); }
        }
        #endregion

        #region Initialization
        private void Awake()
        {
            if (!CompatibilityChecker.IsAllCompatible) { Destroy(this); return; }
            this.window = new Rect(100, 100, 350, 200);
            this.drag = new Rect(0, 0, 350, 30);
            this.level = RealChuteSettings.Instance.EngineerLevel.ToString();

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
            this.destroying = true;
            CloseWindow();
            RealChuteSettings.SaveSettings();

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
            if (!CompatibilityChecker.IsAllCompatible || !this.showing) { return; }

            GUI.skin = HighLogic.Skin;
            this.window = GUILayout.Window(this.id, this.window, Window, "RealChute Settings " + RCUtils.AssemblyVersion);
        }

        private void Window(int id)
        {
            GUI.DragWindow(this.drag);

            RealChuteSettings.Instance.AutoArm = GUILayout.Toggle(RealChuteSettings.Instance.AutoArm, "Automatically arm when staging");
            RealChuteSettings.Instance.JokeActivated = GUILayout.Toggle(RealChuteSettings.Instance.JokeActivated, "Activate April Fools' joke (DANGER!!)");
            RealChuteSettings.Instance.ActivateNyan = GUILayout.Toggle(RealChuteSettings.Instance.ActivateNyan, "Activate NyanMode™");
            RealChuteSettings.Instance.GuiResizeUpdates = GUILayout.Toggle(RealChuteSettings.Instance.GuiResizeUpdates, "Part GUI resize updates canopy size");
            RealChuteSettings.Instance.MustBeEngineer = GUILayout.Toggle(RealChuteSettings.Instance.MustBeEngineer, "Only engineers can repack in career");
            if (!RealChuteSettings.Instance.MustBeEngineer) { GUI.enabled = false; }
            GuiUtils.CreateEntryArea("Engineer minimum level to repack:", ref this.level, 0, 5, 100);
            GUI.enabled = true;

            GuiUtils.CenteredButton("Close", CloseWindow, 100);
        }
        #endregion
    }
}
