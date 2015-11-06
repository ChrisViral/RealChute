using System;
using RealChute.Managers;
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

namespace RealChute.UI
{
    public class SettingsWindow : MonoBehaviour
    {
        #region Fields
        private GUISkin skins = HighLogic.Skin;
        private int id = Guid.NewGuid().GetHashCode();
        private bool showing = true, destroying = false;
        private string level = string.Empty;
        private Rect window = new Rect(), drag = new Rect();
        private Texture2D buttonTexture = new Texture2D(38, 38);
        private RealChuteSettings settings = RealChuteSettings.fetch;
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
            int i = 1;
            if (int.TryParse(level, out i)) { this.settings.engineerLevel = i; }
            if (!this.destroying) { RCToolbarManager.SetAppLauncherButtonFalse(); }
        }
        #endregion

        #region Initialization
        private void Awake()
        {
            if (!CompatibilityChecker.IsAllCompatible()) { Destroy(this); return; }
            this.window = new Rect(100, 100, 330, 200);
            this.drag = new Rect(0, 0, 330, 20);
            this.level = this.settings.engineerLevel.ToString();

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
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
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
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            if (this.showing)
            {
                this.window = GUILayout.Window(this.id, this.window, Window, "RealChute Settings " + RCUtils.assemblyVersion, this.skins.window);
            }
        }

        private void Window(int id)
        {
            UnityEngine.GUI.DragWindow(drag);
            this.settings.autoArm = GUILayout.Toggle(this.settings.autoArm, "Automatically arm when staging", this.skins.toggle);
            this.settings.jokeActivated = GUILayout.Toggle(this.settings.jokeActivated, "Activate April Fools' joke (USE AT OWN RISK)", this.skins.toggle);
            this.settings.guiResizeUpdates = GUILayout.Toggle(this.settings.guiResizeUpdates, "Part GUI resize updates canopy size", this.skins.toggle);
            this.settings.mustBeEngineer = GUILayout.Toggle(this.settings.mustBeEngineer, "Only engineers can repack in career", this.skins.toggle);
            if (!this.settings.mustBeEngineer) { UnityEngine.GUI.enabled = false; }
            GUIUtils.CreateEntryArea("Engineer minimum level to repack:", ref this.level, 0, 5, 100);
            if (!this.settings.mustBeEngineer) { UnityEngine.GUI.enabled = true; }

            GUIUtils.CenteredButton("Close", CloseWindow, 100);
        }
        #endregion
    }
}
