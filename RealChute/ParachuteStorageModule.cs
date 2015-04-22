using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RealChute.Libraries;
using RealChute.Extensions;

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
    public enum StorageTab
    {
        EVA = 1,
        Spares = 2
    }

    public class ParachuteStorageModule : PartModule, IModuleInfo
    {
        #region KSPFields
        [KSPField]
        public float storageSpace = 250;
        #endregion

        #region Properties
        public float usedSpace
        {
            get { return this.storedChutes.Sum(p => p.deployedArea); }
        }

        public float availableSpace
        {
            get { return this.storageSpace - this.usedSpace; }
        }

        public float storedMass
        {
            get { return this.storedChutes.Sum(p => p.chuteMass); }
        }

        private List<IParachute> _storedChutes = new List<IParachute>();
        public List<IParachute> storedChutes
        {
            get { return this._storedChutes; }
        }

        private List<string> _names = new List<string>();
        public List<string> names
        {
            get { return this._names; }
        }
        #endregion

        #region Fields
        //General
        private EVAChuteLibrary EVAlib = EVAChuteLibrary.instance;
        public ConfigNode node = new ConfigNode();

        //GUI
        private GUISkin skins = HighLogic.Skin;
        private Rect window = new Rect(), drag = new Rect();
        private Vector2 scrollAvailable = new Vector2(), scrollStored = new Vector2();
        private int id = Guid.NewGuid().GetHashCode();
        private bool visible = false;
        private StorageTab tab = StorageTab.EVA;
        private LinkedToggles<EVAChute> chutes = null;
        private LinkedToggles<SpareChute> spares = null;
        private LinkedToggles<IParachute> stored = null;
        #endregion

        #region Methods
        public bool TryAddParachute(IParachute parachute)
        {
            if (this.availableSpace > parachute.deployedArea)
            {
                this._storedChutes.Add(parachute);
                return true;
            }
            return false;
        }

        private void GetSpares()
        {
            List<SpareChute> spares = new List<SpareChute>();
            List<string> names = new List<string>();
            List<Part> parts = HighLogic.LoadedSceneIsEditor ? EditorLogic.SortedShipList : this.vessel.parts;
            foreach (Part p in parts)
            {
                foreach (PartModule m in p.Modules)
                {
                    if (m is RealChuteModule)
                    {
                        RealChuteModule rc = m as RealChuteModule;
                        spares.Add(rc.spare);
                        names.Add(rc.spare.name);
                    }
                }
            }
            this.spares = new LinkedToggles<SpareChute>(spares, names.ToArray(), skins.button, GUIUtils.toggleButton);
        }

        private void AddSpare(GameEvents.HostTargetAction<Part, Part> parts)
        {
            foreach (PartModule m in parts.host.Modules)
            {
                if (m is RealChuteModule)
                {
                    RealChuteModule rc = m as RealChuteModule;
                    this.spares.AddToggle(rc.spare, rc.spare.name);
                }
            }
        }

        private void RemoveSpare(GameEvents.HostTargetAction<Part, Part> parts)
        {
            foreach (PartModule m in parts.host.Modules)
            {
                if (m is RealChuteModule)
                {
                    this.spares.RemoveToggle((m as RealChuteModule).spare);
                }
            }
        }

        private void LoadParachutes()
        {
            if (this.storedChutes.Count > 0 && !this.node.HasNode()) { return; }
            this._storedChutes = new List<IParachute>();
            foreach (ConfigNode n in this.node.nodes)
            {
                switch(n.name)
                {
                    case "EVA":
                        {
                            EVAChute chute = new EVAChute(n);
                            this._storedChutes.Add(chute);
                            this._names.Add(chute.name);
                            break;
                        }

                    case "SPARE":
                        {
                            SpareChute spare = new SpareChute(n);
                            this._storedChutes.Add(spare);
                            this._names.Add(spare.name);
                            break;
                        }

                    default:
                        break;
                }
            }
        }

        public Callback<Rect> GetDrawModulePanelCallback()
        {
            return null;
        }

        public string GetModuleTitle()
        {
            return "Parachute Storage";
        }

        public string GetPrimaryField()
        {
            return String.Format("<b>Storage area:</b> {0}m²", this.storageSpace);
        }
        #endregion

        #region Overrides
        public override void OnAwake()
        {
            if (!HighLogic.LoadedSceneIsEditor) { return; }
            GameEvents.onPartAttach.Add(AddSpare);
            GameEvents.onPartRemove.Add(RemoveSpare);
        }

        public override void OnStart(PartModule.StartState state)
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            this.chutes = new LinkedToggles<EVAChute>(EVAlib.chuteList, EVAlib.names, skins.button, GUIUtils.toggleButton);
            this.stored = new LinkedToggles<IParachute>(this._storedChutes, this._storedChutes.Select(s => s.name).ToArray(), skins.button, GUIUtils.toggleButton);
            GetSpares();
        }

        public override void OnLoad(ConfigNode node)
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            this.node = node;
            LoadParachutes();
        }

        public override string GetInfo()
        {
            return String.Format("Storage space: {0}m²", this.storageSpace);
        }

        public override void OnSave(ConfigNode node)
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            this.storedChutes.ForEach(p => node.AddNode(p.Save()));
        }

        private void OnDestroy()
        {
            if (!HighLogic.LoadedSceneIsEditor) { return; }
            GameEvents.onPartAttach.Remove(AddSpare);
            GameEvents.onPartRemove.Remove(RemoveSpare);
        }
        #endregion

        #region GUI
        private void OnGUI()
        {
            if (this.visible)
            {
                GUILayout.Window(this.id, this.window, Window, "Storage Module Selection", this.skins.window);
            }
        }

        private void Window(int id)
        {
            EVAChute c = null;
            SpareChute s = null;
            IParachute p = null;
            GUI.DragWindow(drag);
            GUILayout.BeginVertical();

            StringBuilder b = new StringBuilder();
            b.AppendFormat("Total storage space: {0}m²\n", this.storageSpace);
            b.AppendFormat("Currently used space: {0}m²", this.usedSpace);
            GUILayout.Label(b.ToString(), skins.label);

            this.tab = EnumUtils.SelectionGrid(this.tab, 2, this.skins.button);

            GUILayout.BeginHorizontal();
            this.scrollAvailable = GUILayout.BeginScrollView(this.scrollAvailable, false, false, this.skins.horizontalScrollbar, this.skins.verticalScrollbar, this.skins.box);

            switch (this.tab)
            {
                case StorageTab.EVA:
                    {
                        if (EVAlib.names.Length > 0)
                        {
                            c = chutes.RenderToggles();
                        }
                        else
                        {
                            GUILayout.Label("No EVA chutes available", skins.label);
                        }
                        break;
                    }

                case StorageTab.Spares:
                    {
                        if (EVAlib.names.Length > 0)
                        {
                            s = spares.RenderToggles();
                        }
                        else
                        {
                            GUILayout.Label("No chutes to create spares of on the vessel", skins.label);
                        }
                        break;
                    }

                default:
                    break;
            }

            GUILayout.EndScrollView();

            this.scrollStored = GUILayout.BeginScrollView(this.scrollStored, false, false, this.skins.horizontalScrollbar, this.skins.verticalScrollbar, this.skins.box);

            if (this._storedChutes.Count > 0)
            {
                p = this.stored.RenderToggles();
            }
            else
            {
                GUILayout.Label("No chutes stored");
            }

            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();

            //Buttons

            GUILayout.EndVertical();
        }
        #endregion
    }
}
