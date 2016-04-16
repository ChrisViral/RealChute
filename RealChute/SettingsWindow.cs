using System;
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
        private GUISkin skins = HighLogic.Skin;
        private int id = Guid.NewGuid().GetHashCode();
        private bool showing = true, destroying;
        private string level = string.Empty;
        private Rect window, drag;
        private Texture2D buttonTexture = new Texture2D(38, 38);
        private RealChuteSettings settings = RealChuteSettings.Fetch;
        #endregion

        #region Methods
        private void HideUi()
        {
            this.showing = false;
        }

        private void ShowUi()
        {
            this.showing = true;
        }

        private void CloseWindow()
        {
            int i = 1;
            if (int.TryParse(this.level, out i)) { this.settings.EngineerLevel = i; }
            if (!this.destroying) { RCToolbarManager.SetApplauncherButtonFalse(); }
        }
        #endregion

        #region Initialization
        private void Awake()
        {
            if (!CompatibilityChecker.IsAllCompatible()) { Destroy(this); return; }
            this.window = new Rect(100, 100, 330, 200);
            this.drag = new Rect(0, 0, 330, 20);
            this.level = this.settings.EngineerLevel.ToString();

            GameEvents.onShowUI.Add(ShowUi);
            GameEvents.onHideUI.Add(HideUi);
            GameEvents.onGUIAstronautComplexSpawn.Add(HideUi);
            GameEvents.onGUIAstronautComplexDespawn.Add(ShowUi);
            GameEvents.onGUIRnDComplexSpawn.Add(HideUi);
            GameEvents.onGUIRnDComplexDespawn.Add(ShowUi);
            GameEvents.onGUIMissionControlSpawn.Add(HideUi);
            GameEvents.onGUIMissionControlDespawn.Add(ShowUi);
            GameEvents.onGUIAdministrationFacilitySpawn.Add(HideUi);
            GameEvents.onGUIAdministrationFacilityDespawn.Add(ShowUi);
        }

        private void OnDestroy()
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            this.destroying = true;
            CloseWindow();
            RealChuteSettings.SaveSettings();

            GameEvents.onShowUI.Remove(ShowUi);
            GameEvents.onHideUI.Remove(HideUi);
            GameEvents.onGUIAstronautComplexSpawn.Remove(HideUi);
            GameEvents.onGUIAstronautComplexDespawn.Remove(ShowUi);
            GameEvents.onGUIRnDComplexSpawn.Remove(HideUi);
            GameEvents.onGUIRnDComplexDespawn.Remove(ShowUi);
            GameEvents.onGUIMissionControlSpawn.Remove(HideUi);
            GameEvents.onGUIMissionControlDespawn.Remove(ShowUi);
            GameEvents.onGUIAdministrationFacilitySpawn.Remove(HideUi);
            GameEvents.onGUIAdministrationFacilityDespawn.Remove(ShowUi);
        }
        #endregion

        #region GUI
        private void OnGUI()
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            if (this.showing)
            {
                this.window = GUILayout.Window(this.id, this.window, Window, "RealChute Settings " + RCUtils.AssemblyVersion, this.skins.window);
            }
        }

        private void Window(int id)
        {
            GUI.DragWindow(this.drag);
            this.settings.AutoArm = GUILayout.Toggle(this.settings.AutoArm, "Automatically arm when staging", this.skins.toggle);
            this.settings.JokeActivated = GUILayout.Toggle(this.settings.JokeActivated, "Activate April Fools' joke (USE AT OWN RISK)", this.skins.toggle);
            this.settings.ActivateNyan = GUILayout.Toggle(this.settings.ActivateNyan, "Activate NyanMode™", this.skins.toggle);
            this.settings.GuiResizeUpdates = GUILayout.Toggle(this.settings.GuiResizeUpdates, "Part GUI resize updates canopy size", this.skins.toggle);
            this.settings.MustBeEngineer = GUILayout.Toggle(this.settings.MustBeEngineer, "Only engineers can repack in career", this.skins.toggle);
            if (!this.settings.MustBeEngineer) { GUI.enabled = false; }
            GuiUtils.CreateEntryArea("Engineer minimum level to repack:", ref this.level, 0, 5, 100);
            if (!this.settings.MustBeEngineer) { GUI.enabled = true; }

            GuiUtils.CenteredButton("Close", CloseWindow, 100);
        }
        #endregion
    }
}
