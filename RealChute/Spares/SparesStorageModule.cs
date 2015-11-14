using System;
using System.Collections.Generic;
using System.Linq;
using RealChute.Extensions;
using RealChute.EVA;
using RealChute.UI;
using RealChute.Utils;
using RealChute.Managers;
using RealChute.Libraries.Materials;
using UnityEngine;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute.Spares
{
    //Parachute storage type
    public enum StorageType
    {
        Spares,
        EVA,
        Both
    }

    //GUI tabs
    public enum StorageTab
    {
        Spares = 0,
        EVA = 1,
        Stored = 2
    }

    //Flight GUI tabs
    public enum EquippedTab
    {
        Stored = 0,
        Equipped = 1
    }

    public class SparesStorageModule : PartModule, IModuleInfo
    {
        public class CustomSpare
        {
            public string diameter = "25";
            public MaterialDefinition material = MaterialsLibrary.defaultMaterial;

            public CustomSpare() { }
        }

        #region Static fields
        private static readonly StorageTab[] sparesTabs = { StorageTab.Spares, StorageTab.Stored };

        private static readonly StorageTab[] EVATabs = { StorageTab.EVA, StorageTab.Stored };
        #endregion

        #region KSPFields
        [KSPField]
        public float storageSpace = 250;

        [KSPField(isPersistant = true)]
        public float baseMass = 0;

        [KSPField(isPersistant = true)]
        public bool initiated = false;
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
        private Part kerbal = null;
        private KerbalEVA kerbalEVA = null;
        private bool holdsSpare = false, holdsEVA = false;

        //GUI
        private GUISkin skins = HighLogic.Skin;
        private Rect window = new Rect(), drag = new Rect(), flightWindow = new Rect(), flightDrag = new Rect();
        private Vector2 scrollAvailable = new Vector2(), scrollStored = new Vector2();
        private int id = Guid.NewGuid().GetHashCode(), flightID = Guid.NewGuid().GetHashCode();
        private bool visible = false, flightVisible = false;
        private StorageType type = StorageType.Spares;
        private StorageTab tab = StorageTab.Spares;
        private EquippedTab eTab = EquippedTab.Stored;
        private LinkedToggles<IParachute> stored = null;
        private List<string> storedNames = new List<string>();

        //Custom chute GUI fields
        private int canopyNum = 1;
        private string spareName = string.Empty;
        private Vector2 customsScroll = new Vector2();
        private List<CustomSpare> customs = new List<CustomSpare>();
        private bool inputCustom = false;
        #endregion

        #region Part GUI
        [KSPEvent(guiActive = false, active = true, guiActiveEditor = true, guiName = "Edit contents")]
        public void ToggleWintow()
        {
            if (!this.visible)
            {
                List<SparesStorageModule> storages = new List<SparesStorageModule>();
                if (HighLogic.LoadedSceneIsEditor) { storages = EditorLogic.SortedShipList.FindPartModulesImplementing<SparesStorageModule>(); }
                else if (HighLogic.LoadedSceneIsFlight) { storages = this.vessel.FindPartModulesImplementing<SparesStorageModule>(); }
                SparesStorageModule m = null;
                if (storages.Count > 1 && storages.TryFind(p => p.visible, ref m))
                {
                    this.window.x = m.window.x;
                    this.window.y = m.window.y;
                    m.visible = false;
                }
                this.visible = true;
            }
            else { this.visible = false; }
        }

        [KSPEvent(guiActive = false, active = true, guiActiveEditor = false, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "See contents", unfocusedRange = 5)]
        public void ToggleFlightWindow()
        {
            if (!this.flightVisible)
            {
                Part p = FlightGlobals.ActiveVessel.parts[0];
                PartModuleList modules = p.Modules;
                if (modules.Contains("KerbalEVA"))
                {
                    List<SparesStorageModule> storages = this.vessel.FindPartModulesImplementing<SparesStorageModule>();
                    SparesStorageModule m = null;
                    if (storages.Count > 1 && storages.TryFind(s => s.flightVisible, ref m))
                    {
                        this.flightWindow.x = m.flightWindow.x;
                        this.flightWindow.y = m.flightWindow.y;
                        m.flightVisible = false;
                    }
                    this.kerbal = p;
                    this.kerbalEVA = modules["KerbalEVA"] as KerbalEVA;
                    this.flightVisible = true;
                    foreach (PartModule module in modules)
                    {
                        if (module is SpareCarrierModule) { this.holdsSpare = true; break; }
                        if (module is RealChuteEVA) { this.holdsEVA = true; break; }
                    }
                    
                }
                else { ScreenMessages.PostScreenMessage("Only a kerbal can access the container.", 5, ScreenMessageStyle.UPPER_CENTER); }
            }
            else
            {
                this.kerbal = null;
                this.kerbalEVA = null;
                this.flightVisible = false;
                this.holdsSpare = false;
                this.holdsEVA = false;
            }
        }
        #endregion

        #region Methods
        public void AddParachute(IParachute parachute)
        {
            IParachute clone = parachute.Clone();
            this._storedChutes.Add(clone);
            this.stored.AddToggle(clone, clone.name);
            UpdateMass();
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

        private string GetStoredString(IParachute parachute)
        {
            return parachute.name + "\n\t<b><color=#f05800ff>" + EnumUtils.GetName(parachute.category) + "\t" + parachute.deployedArea + "m²</color></b>"; 
        }

        private void UpdateMass()
        {
            this.part.mass = this.baseMass + this._storedChutes.Sum(p => p.chuteMass);
        }

        public Callback<Rect> GetDrawModulePanelCallback()
        {
            return null;
        }

        public string GetModuleTitle()
        {
            return "Spares Storage";
        }

        public string GetPrimaryField()
        {
            return String.Format("<b>Storage area:</b> {0}m²", this.storageSpace);
        }
        #endregion

        #region Overrides
        public override void OnStart(PartModule.StartState state)
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            this.stored = new LinkedToggles<IParachute>(this._storedChutes, this._storedChutes.Select(s => s.name).ToArray(), skins.button, GUIUtils.toggleButton);
            this.window = new Rect((Screen.width / 2) - 300, (Screen.height / 2) - 400, 600, 800);
            switch (this.type)
            {
                case StorageType.Spares:
                case StorageType.Both:
                    this.tab = StorageTab.Spares; break;

                case StorageType.EVA:
                    this.tab = StorageTab.EVA; break;
            }
            if (!this.initiated)
            {
                this.baseMass = this.part.mass;
                this.initiated = true;
            }
            UpdateMass();
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
        #endregion

        #region GUI
        private void OnGUI()
        {
            if (HighLogic.LoadedSceneIsEditor && this.visible)
            {
                this.window = GUILayout.Window(this.id, this.window, Window, "Storage Module Selection", this.skins.window);
            }

            else if (HighLogic.LoadedSceneIsFlight && this.flightVisible)
            {
                this.flightWindow = GUILayout.Window(this.flightID, this.flightWindow, FlightWindow, "Storage", this.skins.window);
            }
        }

        public IParachute StoredView()
        {
            //Stored chutes selection
            this.scrollAvailable = GUILayout.BeginScrollView(this.scrollAvailable, false, false, this.skins.horizontalScrollbar, this.skins.verticalScrollbar, this.skins.box, GUILayout.MaxWidth(400));
            IParachute p = this.stored.RenderToggles();
            GUILayout.EndScrollView();

            //Selected chutes details
            this.scrollStored = GUILayout.BeginScrollView(this.scrollStored, false, false, this.skins.horizontalScrollbar, this.skins.verticalScrollbar, this.skins.box, GUILayout.MaxWidth(200));
            string info = "No chutes selected";
            if (p != null) { info = p.GetInfo(); }
            GUILayout.Label(info, skins.label, GUILayout.Width(190));
            GUILayout.EndScrollView();
            return p;
        }

        /* WARNING: HELL AHEAD
         * I tried. I swear.*/
        private void Window(int id)
        {
            //Init window
            GUI.DragWindow(this.drag);
            GUILayout.BeginVertical();

            //Normal window
            if (!this.inputCustom)
            {
                //Tabs
                GUILayout.BeginHorizontal(skins.box);
                switch (this.type)
                {
                    case StorageType.Spares:
                        this.tab = EnumUtils.SelectionGrid(this.tab, sparesTabs, 2, this.skins.button); break;

                    case StorageType.EVA:
                        this.tab = EnumUtils.SelectionGrid(this.tab, EVATabs, 2, this.skins.button); break;

                    case StorageType.Both:
                        this.tab = EnumUtils.SelectionGrid(this.tab, 3, this.skins.button); break;
                }
                GUILayout.EndHorizontal();

                //Selection windows
                GUILayout.BeginHorizontal();
                switch (this.tab)
                {
                    //Spare chutes tab
                    case StorageTab.Spares:
                        {
                            //Available spares selection
                            this.scrollAvailable = GUILayout.BeginScrollView(this.scrollAvailable, false, false, this.skins.horizontalScrollbar, this.skins.verticalScrollbar, this.skins.box, GUILayout.MaxWidth(400));
                            SpareChute s = SparesManager.spares.RenderToggles();
                            GUILayout.EndScrollView();

                            //Stored chutes list
                            this.scrollStored = GUILayout.BeginScrollView(this.scrollStored, false, false, this.skins.horizontalScrollbar, this.skins.verticalScrollbar, this.skins.box, GUILayout.MaxWidth(200));
                            GUILayout.Label(this.storedNames.Count > 0 ? this.storedNames.Join("\n\n") : "No chutes stored", skins.label);
                            GUILayout.EndScrollView();
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            if (s == null || s.deployedArea < this.availableSpace) { GUI.enabled = false; }
                            //Adds selected spare
                            if (GUILayout.Button("Add selected", skins.button, GUILayout.Width(400)))
                            {
                                AddParachute(s);
                            }
                            GUI.enabled = true;
                            
                            if (this.availableSpace == 0) { GUI.enabled = false; }
                            //Switches to custom spares inputting
                            if (GUILayout.Button("Custom", skins.button, GUILayout.Width(200)))
                            {
                                this.inputCustom = true;
                                this.customs = new List<CustomSpare> { new CustomSpare() };
                                this.spareName = "Spare" + (this._storedChutes.Count + 1);
                            }
                            GUILayout.EndHorizontal();
                            break;
                        }

                    //EVA chutes tab
                    case StorageTab.EVA:
                        {
                            //EVA chutes selection
                            this.scrollAvailable = GUILayout.BeginScrollView(this.scrollAvailable, false, false, this.skins.horizontalScrollbar, this.skins.verticalScrollbar, this.skins.box, GUILayout.MaxWidth(400));
                            EVAChute c = SparesManager.chutes.RenderToggles();
                            GUILayout.EndScrollView();

                            //Stored chutes list
                            this.scrollStored = GUILayout.BeginScrollView(this.scrollStored, false, false, this.skins.horizontalScrollbar, this.skins.verticalScrollbar, this.skins.box, GUILayout.MaxWidth(200));
                            GUILayout.Label(this.storedNames.Count > 0 ? this.storedNames.Join("\n\n") : "No chutes stored", skins.label);
                            GUILayout.EndScrollView();
                            GUILayout.EndHorizontal();

                            GUILayout.BeginHorizontal();
                            if (c == null || c.deployedArea < this.availableSpace) { GUI.enabled = false; }
                            //Adds selected EVA chute
                            if (GUILayout.Button("Add selected", skins.button, GUILayout.Width(400)))
                            {
                                AddParachute(c);
                            }
                            GUI.enabled = true;
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                            break;
                        }

                    //Stored chutes tab
                    case StorageTab.Stored:
                        {
                            IParachute p = StoredView();
                            GUILayout.EndHorizontal();

                            //Removes selected chute
                            GUILayout.BeginHorizontal();
                            if (p == null) { GUI.enabled = false; }
                            if (GUILayout.Button("Remove", skins.button, GUILayout.Width(400)))
                            {
                                this._storedChutes.Remove(p);
                                this.stored.RemoveToggle(p);
                            }
                            GUI.enabled = true;
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                            break;
                        }
                }

                GUILayout.Label(String.Format("Maximum space: {0}m²     Used space: {1}m²     Available space: {2}m²", this.storageSpace, this.usedSpace, this.availableSpace), skins.label);

                //Close button
                if (GUILayout.Button("Close", skins.button))
                {
                    this.visible = false;
                }
            }
            //Creating custom chute
            else
            {
                GUILayout.Label("Number of canopies:");
                GUILayout.BeginHorizontal();
                //Decrements number of canopies
                if (GUILayout.Button("<<", skins.button) && this.canopyNum > 1)
                {
                    this.canopyNum--;
                    this.customs.RemoveLast();
                }
                //Cnopy number indicator
                GUILayout.Box(this.canopyNum.ToString(), skins.textField, GUILayout.Width(40));
                //Increments number of canopies
                if (GUILayout.Button(">>", skins.button) && this.canopyNum < 10)
                {
                    this.canopyNum++;
                    this.customs.Add(new CustomSpare());
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                //Name box
                if (!string.IsNullOrEmpty(this.spareName)) { GUILayout.Label("Spare name:", skins.label); }
                else { GUILayout.Label("Spare name:", GUIUtils.redLabel); }
                this.spareName = GUILayout.TextField(this.spareName, skins.textField);
                GUILayout.Space(5);

                bool correct = !string.IsNullOrEmpty(this.spareName);

                //Canopies
                this.customsScroll = GUILayout.BeginScrollView(this.customsScroll, false, false, skins.horizontalScrollbar, skins.verticalScrollbar);
                foreach(CustomSpare spare in customs)
                {
                    GUILayout.BeginVertical(skins.box);
                    //Diameter input
                    GUILayout.Label("Deployed diameter (m):", skins.label);
                    float diam = -1;
                    bool parse = float.TryParse(spare.diameter, out diam) && GUIUtils.CheckRange(diam, 1, 70);
                    if (parse) { GUILayout.Label("Deployed diameter (m):", skins.label); }
                    else { GUILayout.Label("Deployed diameter (m):", GUIUtils.redLabel); correct = false; }
                    spare.diameter = GUILayout.TextField(spare.diameter, skins.textField, GUILayout.Width(200));
                    if (parse) { GUILayout.Label(String.Format("Resulting area: {0}m²", RCUtils.GetArea(diam))); }
                    else { GUILayout.Label("Resulting area: --m²", GUIUtils.redLabel); }
                    GUILayout.Space(5);

                    //Material
                    GUILayout.BeginHorizontal();
                    MaterialsLibrary mat = MaterialsLibrary.instance;
                    //Decrements
                    if (GUILayout.Button("<<", skins.button, GUILayout.Width(100)))
                    {
                        int index = mat.GetMaterialIndex(spare.material.name);
                        index++;
                        if (index > mat.count) { index = 0; }
                        spare.material = mat.GetMaterial(index);
                    }
                    //Increments
                    if (GUILayout.Button(">>", skins.button, GUILayout.Width(100)))
                    {
                        int index = mat.GetMaterialIndex(spare.material.name);
                        index--;
                        if (index < 0) { index = mat.count - 1; }
                        spare.material = mat.GetMaterial(index);
                    }
                    GUILayout.EndHorizontal();
                    //Shows current material
                    GUILayout.Box(spare.material.name, skins.textField, GUILayout.Width(200));
                    GUILayout.EndVertical();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndScrollView();

                GUILayout.BeginHorizontal();
                //Creates new spare
                if (!correct) { GUI.enabled = false; }
                if (GUILayout.Button("Create", skins.button))
                {
                    SpareChute sc = new SpareChute(this.spareName, this.customs);
                    this._storedChutes.Add(sc);
                    this.stored.AddToggle(sc, sc.name);
                    this.inputCustom = false;
                }
                GUI.enabled = true;
                //Returns to tabulated screen
                if (GUILayout.Button("Cancel", skins.button))
                {
                    this.inputCustom = false;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void FlightWindow(int id)
        {
            //Init window
            GUI.DragWindow(this.flightDrag);
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal(skins.box);
            this.eTab = EnumUtils.SelectionGrid(this.eTab, 2, skins.button);

            switch (this.eTab)
            {
                case EquippedTab.Stored:
                    {
                        IParachute p = StoredView();
                        break;
                    }

                case EquippedTab.Equipped:
                    {

                        break;
                    }
            }
        }
        #endregion
    }
}
