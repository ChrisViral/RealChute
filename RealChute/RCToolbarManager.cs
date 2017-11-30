using System.Collections.Generic;
using System.IO;
using System.Linq;
using KSP.Localization;
using KSP.UI;
using KSP.UI.Screens;
using RealChute.Extensions;
using RUI.Icons.Selectable;
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
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RCToolbarManager : MonoBehaviour
    {
        #region Fields
        private static ApplicationLauncherButton button;
        private static bool added;
        private static bool visible;
        private static GameObject settings;
        #endregion

        #region Methods
        private void AddFilter()
        {
            //Loads the RealChute parachutes icon
            Texture2D normal = new Texture2D(32, 32), selected = new Texture2D(32, 32);
            normal.LoadImage(File.ReadAllBytes(Path.Combine(RCUtils.PluginDataURL, "FilterIcon.png")));
            selected.LoadImage(File.ReadAllBytes(Path.Combine(RCUtils.PluginDataURL, "FilterIcon_selected.png")));
            Icon icon = new Icon("RC_Parachutes", normal, selected);

            //Adds the Parachutes filter to the Filter by Function category
            PartCategorizer.Category filterByFunction = PartCategorizer.Instance.filters
                .Find(f => f.button.categoryName == Localizer.Format("#autoLOC_453547")); // Filter by Function
            PartCategorizer.AddCustomSubcategoryFilter(filterByFunction, "Parachutes", "Parachutes", icon,
                p => p.moduleInfos.Any(m => m.moduleName == "RealChute" || m.moduleName == "Parachute"));

            //Sets the buttons in the Filter by Module category
            List<PartCategorizer.Category> modules = PartCategorizer.Instance.filters
                .Find(f => f.button.categoryName == Localizer.Format("#autoLOC_453705")).subcategories; // Filter by Module
            modules.Remove(modules.Find(m => m.button.categoryName == "Procedural Chute"));
            modules.Select(m => m.button).Single(b => b.categoryName == "RealChute").SetIcon(icon);

            //Apparently needed to make sure the buttons in Filter by Function show when the editor is loaded
            UIRadioButton button = filterByFunction.button.activeButton;
            button.SetState(UIRadioButton.State.False, UIRadioButton.CallType.APPLICATION, null, false);
            button.SetState(UIRadioButton.State.True, UIRadioButton.CallType.APPLICATION, null, false);
        }

        private void AddButton()
        {
            //The LoadedScene check shouldn't be necessary but but button is being added in FLIGHT too. (not editor, not map view, just flight)
            if (ApplicationLauncher.Ready && !added && HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                Texture2D buttonTexture = new Texture2D(38, 38);
                buttonTexture.LoadImage(File.ReadAllBytes(Path.Combine(RCUtils.PluginDataURL, "RC_Icon.png")));
                button = ApplicationLauncher.Instance.AddModApplication(Show, Hide, Empty, Empty,
                         Empty, Empty, ApplicationLauncher.AppScenes.SPACECENTER, buttonTexture);
                added = true;
            }
        }

        private void RemoveButton()
        {
            if (added)
            {
                ApplicationLauncher.Instance.RemoveModApplication(button);
                Destroy(button);
                button = null;
                added = false;
            }
        }

        private void Show()
        {
            if (!visible)
            {
                settings = new GameObject("SettingsWindow", typeof(SettingsWindow));
                visible = true;
            }
        }

        private void Hide()
        {
            if (visible)
            {
                Destroy(settings);
                visible = false;
            }
        }

        private void Empty() { }

        private void SetButtonVisibility(GameEvents.FromToAction<GameScenes, GameScenes> data)
        {
            if (data.from == GameScenes.SPACECENTER && data.to != GameScenes.SPACECENTER)
            {
                RemoveButton();
            }
        }

        public static void SetApplauncherButtonFalse() => button?.SetFalse();
        #endregion

        #region Initialization
        private void Awake()
        {
            if (!CompatibilityChecker.IsAllCompatible)
            {
                //Removes RealChute parts from being seen if incompatible
                PartLoader.LoadedPartsList.Where(p => p.moduleInfos.Exists(m => m.moduleName == "RealChute" || m.moduleName == "ProceduralChute"))
                    .ForEach(p => p.category = PartCategories.none);
            }
            else
            {
                GameEvents.onGUIEditorToolbarReady.Add(AddFilter);
                GameEvents.onGUIApplicationLauncherReady.Add(AddButton);
                GameEvents.onGUIApplicationLauncherDestroyed.Add(RemoveButton);
                GameEvents.onGameSceneSwitchRequested.Add(SetButtonVisibility);
            }
        }
        #endregion
    }
}