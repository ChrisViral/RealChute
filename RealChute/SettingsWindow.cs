using System;
using System.IO;
using UnityEngine;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more information contact me on the forum. */

namespace RealChute
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class SettingsWindow : MonoBehaviour
    {
        #region Fields
        private GUISkin skins = HighLogic.Skin;
        private int id = Guid.NewGuid().GetHashCode();
        private bool visible = false;
        private Rect window = new Rect();
        private Rect button = new Rect();
        private Texture2D buttonTexture = new Texture2D(1, 1);
        private RealChuteSettings settings = RealChuteSettings.fetch;
        #endregion

        #region Propreties
        private GUIStyle _buttonStyle = null;
        private GUIStyle buttonStyle
        {
            get
            {
                if (_buttonStyle == null)
                {
                    _buttonStyle = new GUIStyle(skins.button);
                    _buttonStyle.onNormal = _buttonStyle.hover;
                }
                return _buttonStyle;
            }
        }
        #endregion

        #region Initialization
        private void Awake()
        {
            this.window = new Rect(100, 100, 260, 130);
            this.button = new Rect(30, 100, 30, 30);
            this.buttonTexture.LoadImage(File.ReadAllBytes(Path.Combine(RCUtils.pluginDataURL, "RC_Icon.png")));
        }

        private void OnDestroy()
        {
            RealChuteSettings.SaveSettings();
        }
        #endregion

        #region GUI
        private void OnGUI()
        {
            this.visible = GUI.Toggle(this.button, this.visible, this.buttonTexture, buttonStyle);
            if (visible)
            {
                this.window = GUILayout.Window(this.id, this.window, Window, "RealChute Settings", skins.window);
            }
        }

        private void Window(int id)
        {
            GUI.DragWindow(new Rect(0, 0, window.width, 20));

            settings.autoArm = GUILayout.Toggle(settings.autoArm, "Automatically arm when deploying", skins.toggle);
            settings.jokeActivated = GUILayout.Toggle(settings.jokeActivated, "Activate April Fools' joke", skins.toggle);
            settings.useStaging = GUILayout.Toggle(settings.useStaging, "Use staging to activate chutes", skins.toggle);

            if(GUILayout.Button("Close", skins.button))
            {
                this.visible = false;
            }
        }
        #endregion
    }
}
