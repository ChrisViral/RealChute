using System.Collections.Generic;
using UnityEngine;
using RealChute.Spares;
using RealChute.EVA;
using RealChute.UI;
using RealChute.Utils;
using KSP.UI.Screens;

namespace RealChute.Managers
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class SparesManager : MonoBehaviour
    {
        #region Properties
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
        #endregion

        #region Methods
        private void TryAddSpare(Part part)
        {
            foreach (PartModule m in part.Modules)
            {
                if (m is RealChuteModule)
                {
                    SpareChute s = (m as RealChuteModule).spare;
                    _spares.AddToggle(s, s.name);
                }
            }
        }

        private void AddSpare(GameEvents.HostTargetAction<Part, Part> parts)
        {
            print("add");
            TryAddSpare(parts.host);
        }

        private void RemoveSpare(GameEvents.HostTargetAction<Part, Part> parts)
        {
            print("remove");
            foreach (PartModule m in parts.target.Modules)
            {
                if (m is RealChuteModule)
                {
                    _spares.RemoveToggle((m as RealChuteModule).spare);
                }
            }
        }

        private void LoadShip(ShipConstruct ship, CraftBrowserDialog.LoadType type)
        {
            print("load");
            Part part = ship.parts[0];
            foreach(PartModule m in part.Modules)
            {
                if (m is RealChuteModule)
                {
                    SpareChute s = (m as RealChuteModule).spare;
                    _spares.AddFirst(s, s.name);
                }
            }
        }

        private void OnEvent(ConstructionEventType type, Part part)
        {
            if (type == ConstructionEventType.PartCreated)
            {
                List<Part> parts = EditorLogic.SortedShipList;
                if (parts != null && parts.Count > 0 && parts[0] == part)
                {
                    print("root event");
                    TryAddSpare(part);
                }
            }
        }

        private void Restart()
        {
            print("restart");
            _spares.ClearToggle();
        }
        #endregion

        #region Functions
        private void Awake()
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            GameEvents.onPartAttach.Add(AddSpare);
            GameEvents.onPartRemove.Add(RemoveSpare);
            GameEvents.onEditorLoad.Add(LoadShip);
            GameEvents.onEditorPartEvent.Add(OnEvent);
            GameEvents.onEditorRestart.Add(Restart);
        }

        private void Start()
        {
            if (_chutes == null) { _chutes = new LinkedToggles<EVAChute>(lib.chuteList, lib.names, skins.button, GUIUtils.toggleButton); }
            if (_spares == null) { _spares = new LinkedToggles<SpareChute>(skins.button, GUIUtils.toggleButton); }
        }

        private void OnDestroy()
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            GameEvents.onPartAttach.Remove(AddSpare);
            GameEvents.onPartRemove.Remove(RemoveSpare);
            GameEvents.onEditorLoad.Remove(LoadShip);
            GameEvents.onEditorPartEvent.Remove(OnEvent);
            GameEvents.onEditorRestart.Remove(Restart);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P) && Input.GetKey(KeyCode.LeftAlt))
            {
                print(spares.ToString());
            }
        }
        #endregion
    }
}
