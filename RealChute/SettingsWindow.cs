using System;
using System.IO;
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
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class SettingsWindow : MonoBehaviour
    {
        #region Fields
        private GUISkin skins = HighLogic.Skin;
        private int id = Guid.NewGuid().GetHashCode();
        private bool visible = false, showing = true, destroying = false;
        private string level = string.Empty;
        private Rect window = new Rect(), drag = new Rect();
        private Texture2D buttonTexture = new Texture2D(38, 38);
        private ApplicationLauncherButton button = new ApplicationLauncherButton();
        private RealChuteSettings settings = RealChuteSettings.fetch;
        #endregion

        #region Methods
        private void AddButton()
        {
            if (ApplicationLauncher.Ready)
            {
                this.button = ApplicationLauncher.Instance.AddModApplication(Show, Hide,
                    Empty, Empty, Empty, Empty, ApplicationLauncher.AppScenes.SPACECENTER,
                    (Texture)this.buttonTexture);
            }
        }

        private void Show()
        {
            this.visible = true;
        }

        private void Hide()
        {
            this.visible = false;
        }

        private void Empty() { }

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
            if (!this.destroying) { this.button.SetFalse(); }
        }
        #endregion

        #region Initialization
        private void Awake()
        {
            if (!CompatibilityChecker.IsAllCompatible()) { Destroy(this); return; }
            this.window = new Rect(100, 100, 330, 200);
            this.drag = new Rect(0, 0, 330, 20);
            this.level = this.settings.engineerLevel.ToString();
            this.buttonTexture.LoadImage(File.ReadAllBytes(Path.Combine(RCUtils.pluginDataURL, "RC_Icon.png")));

            GameEvents.onGUIApplicationLauncherReady.Add(AddButton);
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

            GameEvents.onGUIApplicationLauncherReady.Remove(AddButton);
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

            ApplicationLauncher.Instance.RemoveModApplication(button);
        }
        #endregion

        #region GUI
        private void OnGUI()
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            if (this.showing && this.visible)
            {
                this.window = GUILayout.Window(this.id, this.window, Window, "RealChute Settings " + RCUtils.assemblyVersion, this.skins.window);
            }
        }

        private void Window(int id)
        {
            GUI.DragWindow(drag);
            this.settings.autoArm = GUILayout.Toggle(this.settings.autoArm, "Automatically arm when staging", this.skins.toggle);
            this.settings.jokeActivated = GUILayout.Toggle(this.settings.jokeActivated, "Activate April Fools' joke (USE AT OWN RISK)", this.skins.toggle);
            this.settings.guiResizeUpdates = GUILayout.Toggle(this.settings.guiResizeUpdates, "Part GUI resize updates canopy size", this.skins.toggle);
            this.settings.mustBeEngineer = GUILayout.Toggle(this.settings.mustBeEngineer, "Only engineers can repack in career", this.skins.toggle);
            if (!this.settings.mustBeEngineer) { GUI.enabled = false; }
            GUIUtils.CreateEntryArea("Engineer minimum level to repack:", ref this.level, 0, 5, 100);
            if (!this.settings.mustBeEngineer) { GUI.enabled = true; }

            GUIUtils.CenteredButton("Close", CloseWindow, 100);
        }
        #endregion
    }
}
