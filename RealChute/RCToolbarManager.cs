using System.Collections.Generic;
using System.Linq;
using KSP.UI;
using KSP.UI.Screens;
using RealChute.Extensions;
using RUI.Icons.Selectable;
using ToolbarControl_NS;
using UnityEngine;
using Enumerable = UniLinq.Enumerable;

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
        #region Instance
        public static RCToolbarManager Instance { get; private set; }
        #endregion

        #region Fields
        private ToolbarControl controller;
        private bool visible;
        private GameObject settings;
        #endregion

        #region Methods
        // ReSharper disable once MemberCanBeMadeStatic.Local - breaks event call
        private void AddFilter()
        {
            //Loads the RealChute parachutes icon
            GameDatabase.TextureInfo iconInfo         = GameDatabase.Instance.GetTextureInfo(RCUtils.CategorizerIconURL);
            GameDatabase.TextureInfo iconSelectedInfo = GameDatabase.Instance.GetTextureInfo(RCUtils.CategorizerIconURL + "_selected");
            Icon icon = new(iconInfo.name, iconInfo.texture, iconSelectedInfo.texture);

            //Adds the Parachutes filter to the Filter by Function category
            PartCategorizer.Category filterByFunction = PartCategorizer.Instance.filters.Find(f => f.button.categoryName is "Filter by Function");
            PartCategorizer.AddCustomSubcategoryFilter(filterByFunction, "Parachutes", "Parachutes", icon, p => p.moduleInfos.Any(m => m.moduleName is "RealChute" or "Parachute"));

            //Sets the buttons in the Filter by Module category
            PartCategorizer.Category filterByModule = PartCategorizer.Instance.filters.Find(f => f.button.categoryName is "Filter by Module");
            List<PartCategorizer.Category> modules = filterByModule.subcategories;
            filterByModule.subcategories.RemoveAt(modules.FindIndex(m => m.button.categoryName is "Procedural Chute"));
            filterByModule.subcategories.Select(m => m.button).First(b => b.categoryName is "RealChute").SetIcon(icon);

            //Apparently needed to make sure the buttons in Filter by Function show when the editor is loaded
            UIRadioButton button = filterByFunction.button.activeButton;
            button.SetState(UIRadioButton.State.False, UIRadioButton.CallType.APPLICATION, null, false);
            button.SetState(UIRadioButton.State.True, UIRadioButton.CallType.APPLICATION, null, false);
        }

        private void Show()
        {
            if (this.visible) return;

            this.settings = new GameObject(typeof(SettingsWindow).FullName, typeof(SettingsWindow));
            this.visible = true;
        }

        private void Hide()
        {
            if (!this.visible) return;

            Destroy(this.settings);
            this.visible = false;
        }

        internal void RequestHide() => this.controller.SetFalse(true);
        #endregion

        #region Initialization
        private void Awake()
        {
            if (CompatibilityChecker.IsAllCompatible && !Instance)
            {
                Instance = this;
                DontDestroyOnLoad(this);
                return;
            }

            //Removes RealChute parts from being seen if incompatible
            PartLoader.LoadedPartsList.Where(p => p.moduleInfos.Exists(m => m.moduleName is "RealChute" or "ProceduralChute"))
                                      .ForEach(p => p.category = PartCategories.none);
        }

        private void Start()
        {
            if (Instance != this) return;

            Debug.Log("[RealChute]: Adding toolbar events");
            ToolbarControl.RegisterMod(nameof(RealChute), DisplayName: "RealChute Settings", useBlizzy: false, useStock: true, NoneAllowed: false);

            this.controller = this.gameObject.AddComponent<ToolbarControl>();
            this.controller.AddToAllToolbars(Show, Hide, ApplicationLauncher.AppScenes.SPACECENTER, nameof(RealChute), nameof(RCToolbarManager),
                                             RCUtils.ToolbarIconURL, RCUtils.ToolbarIconURL, RCUtils.ToolbarIconURL, RCUtils.ToolbarIconURL, "RealChute Settings");
            GameEvents.onGUIEditorToolbarReady.Add(AddFilter);
        }

        private void OnDestroy()
        {
            if (Instance != this) return;

            Debug.Log("[RealChute]: Removing toolbar events");
            GameEvents.onGUIEditorToolbarReady.Remove(AddFilter);
            this.controller.OnDestroy();
            Destroy(this.controller);
            Instance = null;
        }
        #endregion
    }
}