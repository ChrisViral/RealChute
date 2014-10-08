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
        private bool visible = false, showing = true;
        private Rect window = new Rect();
        private Texture2D buttonTexture = new Texture2D(38, 38);
        private ApplicationLauncherButton button = new ApplicationLauncherButton();
        private RealChuteSettings settings = RealChuteSettings.fetch;
        #endregion

        #region Methods
        private void AddButton()
        {
            if (ApplicationLauncher.Ready)
            {
                button = ApplicationLauncher.Instance.AddModApplication(
                    () => { this.visible = true; },
                    () => { this.visible = false; },
                    () => { },
                    () => { },
                    () => { },
                    () => { },
                    ApplicationLauncher.AppScenes.SPACECENTER,
                    (Texture)buttonTexture);
            }
        }

        private void HideUI()
        {
            this.showing = false;
        }

        private void ShowUI()
        {
            this.showing = true;
        }
        #endregion

        #region Initialization
        private void Awake()
        {
            if (!CompatibilityChecker.IsAllCompatible()) { Destroy(this); return; }
            this.window = new Rect(100, 100, 330, 130);
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

        private void Start()
        {
            enabled = true;
        }

        private void OnDestroy()
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
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
                this.window = GUILayout.Window(this.id, this.window, Window, "RealChute Settings " + RCUtils.assemblyVersion, skins.window);
            }
        }

        private void Window(int id)
        {
            GUI.DragWindow(new Rect(0, 0, window.width, 20));
            settings.autoArm = GUILayout.Toggle(settings.autoArm, "Automatically arm when deploying", skins.toggle);
            settings.jokeActivated = GUILayout.Toggle(settings.jokeActivated, "Activate April Fools' joke (USE AT OWN RISK)", skins.toggle);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Close", skins.button))
            {
                button.SetFalse();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        #endregion
    }
}
