using System.Collections.Generic;
using UnityEngine;
using RealChute.Spares;
using RealChute.EVA;
using RealChute.UI;
using RealChute.Utils;

namespace RealChute.Managers
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class SparesManager : MonoBehaviour
    {
        private static GUISkin skins = HighLogic.Skin;
        private static EVAChuteLibrary lib = EVAChuteLibrary.instance;

        private static LinkedToggles<SpareChute> _spares = null;
        public static LinkedToggles<SpareChute> spares
        {
            get { return _spares; }
        }

        private static LinkedToggles<EVAChute> _chutes = null;
        public static LinkedToggles<EVAChute> chutes
        {
            get { return _chutes; }
        }

        private void AddSpare(GameEvents.HostTargetAction<Part, Part> parts)
        {
            print("add");
            foreach (PartModule m in parts.host.Modules)
            {
                if (m is RealChuteModule)
                {
                    SpareChute s = (m as RealChuteModule).spare;
                    _spares.AddToggle(s, s.name);
                }
            }
        }

        private void RemoveSpare(GameEvents.HostTargetAction<Part, Part> parts)
        {
            print("remove");
            foreach (PartModule m in parts.host.Modules)
            {
                if (m is RealChuteModule)
                {
                    _spares.RemoveToggle((m as RealChuteModule).spare);
                }
            }
        }

        private void LoadShip(ShipConstruct ship, CraftBrowser.LoadType type)
        {
            print("load");
            List<SpareChute> list = new List<SpareChute>();
            List<string> names = new List<string>();
            foreach(Part p in ship.parts)
            {
                foreach (PartModule m in p.Modules)
                {
                    if (m is RealChuteModule)
                    {
                        SpareChute s = (m as RealChuteModule).spare;
                        list.Add(s);
                        names.Add(s.name);
                    }
                }
            }
            _spares = new LinkedToggles<SpareChute>(list, names.ToArray(), skins.button, GUIUtils.toggleButton);
        }

        private void Reset()
        {
            _spares = null;
        }

        private void Awake()
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            if (_chutes == null) { _chutes = new LinkedToggles<EVAChute>(lib.chuteList, lib.names, skins.button, GUIUtils.toggleButton); }
            GameEvents.onPartAttach.Add(AddSpare);
            GameEvents.onPartRemove.Add(RemoveSpare);
            GameEvents.onEditorLoad.Add(LoadShip);
            GameEvents.onEditorRestart.Add(Reset);
        }

        private void OnDestroy()
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            GameEvents.onPartAttach.Remove(AddSpare);
            GameEvents.onPartRemove.Remove(RemoveSpare);
            GameEvents.onEditorLoad.Remove(LoadShip);
            GameEvents.onEditorRestart.Remove(Reset);
        }
    }
}
