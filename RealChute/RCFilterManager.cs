using System.Collections.Generic;
using System.Linq;
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
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RCFilterManager : MonoBehaviour
    {
        #region Methods
        private void AddFilter()
        {
            /*
            Texture2D normal = new Texture2D(32, 32), selected = new Texture2D(32, 32);
            normal.LoadImage(File.ReadAllBytes(Path.Combine(RCUtils.pluginDataURL, "filterIcon.png")));
            selected.LoadImage(File.ReadAllBytes(Path.Combine(RCUtils.pluginDataURL, "filterIcon_selected.png")));
            PartCategorizer.Icon icon = new PartCategorizer.Icon("RealChute", normal, selected);
            */

            //Will replace with custom icons once sumghai does them
            PartCategorizer.Icon icon = PartCategorizer.Instance.GetIcon("R&D_node_icon_survivability");

            //Adds the buton in the Filter by Fodule category
            PartCategorizer.Category filterByFunction = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == "Filter by Function");
            PartCategorizer.AddCustomSubcategoryFilter(filterByFunction, "Parachutes", icon, p => p.moduleInfos.Any(m => m.moduleName == "RealChute" || m.moduleName == "Parachute"));

            //Sets the buttons in the Filter by Module category
            List<PartCategorizer.Category> modules = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == "Filter by Module").subcategories;
            modules.Remove(modules.Find(m => m.button.categoryName == "Procedural Chute"));
            modules.Select(m => m.button).Single(b => b.categoryName == "RealChute").SetIcon(icon);

            //Apparently needed to make sure the buttons in Filter by Function show correctly
            RUIToggleButtonTyped button = filterByFunction.button.activeButton;
            button.SetFalse(button, RUIToggleButtonTyped.ClickType.FORCED);
            button.SetTrue(button, RUIToggleButtonTyped.ClickType.FORCED);
        }
        #endregion

        #region Initialization
        private void Awake()
        {
            if (!CompatibilityChecker.IsAllCompatible())
            {
                //Removes RealChute parts from being seen if incompatible
                List<AvailablePart> parts = new List<AvailablePart>(PartLoader.LoadedPartsList.Where(p => p.moduleInfos.Any(m => m.moduleName == "Real Chute Module")));
                parts.ForEach(p => p.category = PartCategories.none);
            }
            else
            {
                GameEvents.onGUIEditorToolbarReady.Add(AddFilter);
            }
        }
        #endregion
    }
}
