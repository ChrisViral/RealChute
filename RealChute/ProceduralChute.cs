using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RealChute.Extensions;
using UnityEngine;
using RealChute.Libraries;

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
    public class ProceduralChute : PartModule, IPartCostModifier
    {
        /// <summary>
        /// Type for the GUI selector entries
        /// </summary>
        internal enum SelectorType
        {
            CASE,
            CHUTE,
            MODEL
        }

        #region Config values
        [KSPField]
        public string textureLibrary = string.Empty;
        [KSPField]
        public string type = "Cone";
        [KSPField]
        public string currentCase = string.Empty;
        [KSPField]
        public string currentCanopies = string.Empty;
        [KSPField]
        public string currentTypes = "Main";
        [KSPField]
        public bool isTweakable = true;
        #endregion

        #region Persistent values
        //Selection grid IDs
        [KSPField(isPersistant = true)]
        public int caseID = -1, lastCaseID = -1;
        [KSPField(isPersistant = true)]
        public int size = 0, lastSize = 0, planets = 0;
        [KSPField(isPersistant = true)]
        public int presetID = 0;

        //Size vectors
        [KSPField(isPersistant = true)]
        public Vector3 originalSize = new Vector3();

        //Attach nodes
        [KSPField(isPersistant = true)]
        public float top = 0, bottom = 0, debut = 0;

        //Bools
        [KSPField(isPersistant = true)]
        public bool initiated = false;
        [KSPField(isPersistant = true)]
        public bool mustGoDown = false, deployOnGround = false;
        [KSPField(isPersistant = true)]
        public bool secondaryChute = false;

        //GUI strings
        [KSPField(isPersistant = true)]
        public string timer = string.Empty, cutSpeed = string.Empty, spares = string.Empty, landingAlt = "0";
        #endregion

        #region Fields
        //Libraries
        private GUISkin skins = HighLogic.Skin;
        private EditorActionGroups actionPanel = EditorActionGroups.Instance;
        internal RealChuteModule rcModule = null;
        internal AtmoPlanets bodies = null;
        internal MaterialsLibrary materials = MaterialsLibrary.instance;
        private TextureLibrary textureLib = TextureLibrary.instance;
        internal TextureConfig textures = null;
        internal CaseConfig parachuteCase = new CaseConfig();
        internal List<ChuteTemplate> chutes = new List<ChuteTemplate>();
        internal PresetsLibrary presets = PresetsLibrary.instance;
        internal CelestialBody body = null;
        internal EditorGUI editorGUI = new EditorGUI();

        //Sizes
        private PersistentManager sizeLib = PersistentManager.instance;
        public List<SizeNode> sizes = new List<SizeNode>();
        public Transform parent = null;
        public ConfigNode node = null;
        #endregion

        #region Part GUI
        [KSPEvent(active = true, guiActiveEditor = true, guiName = "Next size")]
        public void GUINextSize()
        {
            this.size++;
            if (this.size > this.sizes.Count - 1) { this.size = 0; }
            if (RealChuteSettings.fetch.guiResizeUpdates) { this.Apply(false, this.part.partInfo.title, false); }
        }

        [KSPEvent(active = true, guiActiveEditor = true, guiName = "Previous size")]
        public void GUIPreviousSize()
        {
            this.size--;
            if (this.size < 0) { this.size = this.sizes.Count - 1; }
            if (RealChuteSettings.fetch.guiResizeUpdates) { this.Apply(false, this.part.partInfo.title, false); }
        }
        #endregion

        #region Methods
        //Gets the strings for the selection grids
        internal string[] TextureEntries(SelectorType type)
        {
            if (this.textures == null) { return new string[0]; }
            string[] texts = new string[0];
            switch (type)
            {
                case SelectorType.CASE:
                    this.textures.TryGetCasesOfType(this.type, ref texts); break;
                case SelectorType.CHUTE:
                    return this.textures.canopyNames;
                case SelectorType.MODEL:
                    this.textures.TryGetParameterModels(this.chutes.Count, ref texts); break;
                default:
                    return new string[0];
            }
            return texts;
        }

        //Gets the total mass of the craft
        internal float GetCraftMass(bool dry)
        {
            return EditorLogic.SortedShipList.Where(p => p.physicalSignificance != Part.PhysicalSignificance.NONE).Sum(p => dry ? p.mass : p.TotalMass());
        }

        //Lists the errors of a given type
        internal List<string> GetErrors(bool general)
        {
            if (general)
            {
                List<string> errors = new List<string>();
                float f = 0;
                if (!GUIUtils.TryParseTime(this.timer, out f) || !GUIUtils.CheckRange(f, 0, 3600)) { errors.Add("Deployment timer"); }
                if (!GUIUtils.TryParseWithEmpty(this.spares, out f) || !GUIUtils.CheckRange(f, -1, 10) || !RCUtils.IsWholeNumber(f)) { errors.Add("Spare chutes"); }
                if (!float.TryParse(this.cutSpeed, out f) || !GUIUtils.CheckRange(f, 0.01f, 100)) { errors.Add("Autocut speed"); }
                if (!float.TryParse(this.landingAlt, out f) || !GUIUtils.CheckRange(f, 0, (float)this.body.GetMaxAtmosphereAltitude())) { errors.Add("Landing altitude"); }
                return errors;
            }
            else { return new List<string>(this.chutes.SelectMany(c => c.templateGUI.errors)); }
        }

        //Creates labels for errors.
        internal void CreateErrors()
        {
            GUILayout.Label("General:", this.skins.label);
            StringBuilder builder = new StringBuilder();
            builder.AppendJoin(GetErrors(true), "\n");
            GUILayout.Label(builder.ToString(), GUIUtils.redLabel);
            GUILayout.Space(10);

            GUILayout.Label("Chutes:", this.skins.label);
            builder = new StringBuilder();
            builder.AppendJoin(GetErrors(false), "\n");
            GUILayout.Label(builder.ToString(), GUIUtils.redLabel);
            GUILayout.Space(10);
        }

        //Applies the parameters to the parachute
        internal void Apply(bool toSymmetryCounterparts, string spareName, bool showMessage = true)
        {
            if (!showMessage && (GetErrors(true).Count > 0 || GetErrors(false).Count > 0)) { this.editorGUI.failedVisible = true; return; }
            this.rcModule.mustGoDown = this.mustGoDown;
            this.rcModule.deployOnGround = this.deployOnGround;
            this.rcModule.timer = GUIUtils.ParseTime(this.timer);
            this.rcModule.cutSpeed = float.Parse(this.cutSpeed);
            this.rcModule.spareChutes = GUIUtils.ParseEmpty(this.spares);
            this.rcModule.chuteCount = (int)this.rcModule.spareChutes;

            this.chutes.ForEach(c => c.ApplyChanges(toSymmetryCounterparts));

            if (toSymmetryCounterparts)
            {
                foreach (Part part in this.part.symmetryCounterparts)
                {
                    RealChuteModule module = part.Modules["RealChuteModule"] as RealChuteModule;
                    UpdateCaseTexture(part, module);
                    UpdateScale(part, module);

                    module.mustGoDown = this.mustGoDown;
                    module.timer = GUIUtils.ParseTime(this.timer);
                    module.cutSpeed = float.Parse(this.cutSpeed);
                    module.spareChutes = GUIUtils.ParseEmpty(this.spares);

                    ProceduralChute pChute = part.Modules["ProceduralChute"] as ProceduralChute;
                    pChute.presetID = this.presetID;
                    pChute.planets = this.planets;
                    pChute.size = this.size;
                    pChute.caseID = this.caseID;
                    pChute.mustGoDown = this.mustGoDown;
                    pChute.deployOnGround = this.deployOnGround;
                    pChute.timer = this.timer;
                    pChute.cutSpeed = this.cutSpeed;
                    pChute.spares = this.spares;
                }
            }
            this.part.mass = this.rcModule.caseMass + this.rcModule.parachutes.Sum(p => p.chuteMass);
            if (showMessage)
            {
                this.editorGUI.successfulVisible = true;
                if (!this.editorGUI.warning) { this.editorGUI.successfulWindow.height = 50; }
            }
            GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
            this.rcModule.UpdateSpare(spareName);
        }

        //Checks if th given AttachNode has the parent part
        private bool CheckParentNode(AttachNode node)
        {
            return node.attachedPart != null && this.part.parent != null && node.attachedPart == this.part.parent;
        }

        //Modifies the size of a part
        private void UpdateScale(Part part, RealChuteModule module)
        {
            //Thanks to Brodicus for the optimization here
            if (this.sizes.Count <= 1) { return; }
            SizeNode size = this.sizes[this.size], lastSize = this.sizes[this.lastSize];
            Transform root = this.part.transform.GetChild(0);
            root.localScale = Vector3.Scale(this.originalSize, size.size);
            module.caseMass = size.caseMass;
            AttachNode topNode = null, bottomNode = null;
            bool hasTopNode = part.TryGetAttachNodeById("top", out topNode);
            bool hasBottomNode = part.TryGetAttachNodeById("bottom", out bottomNode);
            List<Part> allTopChildParts = null, allBottomChildParts = null;

            // If this is the root part, move things for the top and the bottom.
            if ((HighLogic.LoadedSceneIsEditor && this.part == EditorLogic.SortedShipList[0]) || (HighLogic.LoadedSceneIsFlight && this.vessel.rootPart == this.part))
            {
                if (hasTopNode)
                {
                    topNode.position = size.topNode;
                    topNode.size = size.topNodeSize;
                    if (topNode.attachedPart != null)
                    {
                        float topDifference = size.topNode.y - lastSize.topNode.y;
                        topNode.attachedPart.transform.Translate(0, topDifference, 0, this.part.transform);
                        if (allTopChildParts == null) { allTopChildParts = topNode.attachedPart.GetAllChildren(); }
                        allTopChildParts.ForEach(c => c.transform.Translate(0, topDifference, 0, this.part.transform));
                    }
                }

                if (hasBottomNode)
                {
                    bottomNode.position = size.bottomNode;
                    bottomNode.size = size.bottomNodeSize;
                    if (bottomNode.attachedPart != null)
                    {
                        float bottomDifference = size.bottomNode.y - lastSize.bottomNode.y;
                        bottomNode.attachedPart.transform.Translate(0, bottomDifference, 0, this.part.transform);
                        if (allBottomChildParts == null) { allBottomChildParts = bottomNode.attachedPart.GetAllChildren(); }
                        allBottomChildParts.ForEach(c => c.transform.Translate(0, bottomDifference, 0, this.part.transform));
                    }
                }
            }

            // If not root and parent is attached to the bottom
            else if (hasBottomNode && CheckParentNode(bottomNode))
            {
                bottomNode.position = size.bottomNode;
                bottomNode.size = size.bottomNodeSize;
                float bottomDifference = size.bottomNode.y - lastSize.bottomNode.y;
                this.part.transform.Translate(0, -bottomDifference, 0, this.part.transform);
                if (hasTopNode)
                {
                    topNode.position = size.topNode;
                    topNode.size = size.topNodeSize;
                    if (topNode.attachedPart != null)
                    {
                        float diff = size.topNode.y - lastSize.topNode.y - bottomDifference;
                        topNode.attachedPart.transform.Translate(0, diff, 0, this.part.transform);
                        if (allTopChildParts == null) { allTopChildParts = topNode.attachedPart.GetAllChildren(); }
                        allTopChildParts.ForEach(c => c.transform.Translate(0, diff, 0, this.part.transform));
                    }
                }
            }
            // If not root and parent is attached to the top
            else if (hasTopNode && CheckParentNode(topNode))
            {
                topNode.position = size.topNode;
                topNode.size = size.topNodeSize;
                float topDifference = size.topNode.y - lastSize.topNode.y;
                this.part.transform.Translate(0, -topDifference, 0, this.part.transform);
                if (hasBottomNode)
                {
                    bottomNode.position = size.bottomNode;
                    bottomNode.size = size.bottomNodeSize;
                    if (bottomNode.attachedPart != null)
                    {
                        float diff = size.bottomNode.y - lastSize.bottomNode.y - topDifference;
                        bottomNode.attachedPart.transform.Translate(0, diff, 0, part.transform);
                        if (allBottomChildParts == null) { allBottomChildParts = bottomNode.attachedPart.GetAllChildren(); }
                        allBottomChildParts.ForEach(c => c.transform.Translate(0, diff, 0, part.transform));
                    }
                }
            }

            //Parachute transforms
            float scaleX = root.localScale.x / Vector3.Scale(originalSize, lastSize.size).x;
            float scaleY = root.localScale.y / Vector3.Scale(originalSize, lastSize.size).y;
            float scaleZ = root.localScale.z / Vector3.Scale(originalSize, lastSize.size).z;
            foreach (Parachute chute in this.rcModule.parachutes)
            {
                Vector3 pos = chute.forcePosition - this.part.transform.position;
                chute.parachute.transform.Translate(pos.x * (scaleX - 1), pos.y * (scaleY - 1), pos.z * (scaleZ - 1), this.part.transform);
            }

            //Surface attached parts
            if (this.part.children.Exists(c => c.attachMode == AttachModes.SRF_ATTACH))
            {
                foreach (Part child in this.part.children)
                {
                    if (child.attachMode == AttachModes.SRF_ATTACH)
                    {
                        Vector3 vX = new Vector3(), vY = new Vector3();
                        vX = (child.transform.localPosition + child.transform.localRotation * child.srfAttachNode.position) - this.part.transform.position;
                        vY = child.transform.position - this.part.transform.position;
                        child.transform.Translate(vX.x * (scaleX - 1), vY.y * (scaleY - 1), vX.z * (scaleZ - 1), this.part.transform);
                        child.GetAllChildren().ForEach(c => c.transform.Translate(vX.x * (scaleX - 1), vY.y * (scaleY - 1), vX.z * (scaleZ - 1), this.part.transform));
                    }
                }
            }
            this.lastSize = this.size;
        }

        //Modifies the case texture of a part
        private void UpdateCaseTexture(Part part, RealChuteModule module)
        {
            if (this.textures == null) { return; }
            if (this.textures.TryGetCase(this.caseID, this.type, ref this.parachuteCase))
            {
                if (string.IsNullOrEmpty(this.parachuteCase.textureURL))
                {
                    Debug.LogWarning("[RealChute]: The " + this.parachuteCase.name + "URL is empty");
                    this.lastCaseID = this.caseID;
                    return;
                }
                Texture2D texture = GameDatabase.Instance.GetTexture(this.parachuteCase.textureURL, false);
                if (texture == null)
                {
                    Debug.LogWarning("[RealChute]: The " + this.parachuteCase.name + "texture is null");
                    this.lastCaseID = this.caseID;
                    return;
                }
                this.part.GetPartRenderers(module).ForEach(r => r.material.mainTexture = texture);
            }
            this.lastCaseID = this.caseID;
        }

        //Applies the selected preset
        internal void ApplyPreset()
        {
            Preset preset = this.presets.GetPreset(this.presetID, this.chutes.Count);
            if (this.sizes.Exists(s => s.sizeID == preset.sizeID)) { this.size = this.sizes.IndexOf(this.sizes.Find(s => s.sizeID == preset.sizeID)); }
            this.cutSpeed = preset.cutSpeed;
            this.timer = preset.timer;
            this.mustGoDown = preset.mustGoDown;
            this.deployOnGround = preset.deployOnGround;
            this.spares = preset.spares;
            if (this.textureLibrary == preset.textureLibrary || (this.textures != null && this.textures.ContainsCase(preset.caseName)))
            {
                this.caseID = this.textures.GetCaseIndex(preset.caseName);
            }
            this.chutes.ForEach(c => c.ApplyPreset(preset));
            Apply(false, preset.name);
            print("[RealChute]: Applied the " + preset.name + " preset on " + this.part.partInfo.title);
        }

        //Creates and save a preset from the current stats
        internal void CreatePreset()
        {
            this.presets.AddPreset(new Preset(this));
            RealChuteSettings.SaveSettings();
            PopupDialog.SpawnPopupDialog("Preset saved", "The \"" + this.editorGUI.presetName + "\" preset was succesfully saved!", "Close", false, this.skins);
            print("[RealChute]: Saved the " + this.editorGUI.presetName + " preset to the settings file.");
        }

        //Reloads the size nodes
        private void LoadChutes()
        {
            if (this.chutes.Count <= 0)
            {
                if (this.node.HasNode("CHUTE"))
                {
                    this.chutes = new List<ChuteTemplate>(this.node.GetNodes("CHUTE").Select((n, i) => new ChuteTemplate(this, n, i)));
                }
                else
                {
                    this.chutes.Clear();
                    RealChuteModule module = this.rcModule ?? this.part.Modules["RealChuteModule"] as RealChuteModule;
                    if (module.parachutes.Count <= 0) { return; }
                    for (int i = 0; i < module.parachutes.Count; i++)
                    {
                        this.chutes.Add(new ChuteTemplate(this, null, i));
                    }
                }
            }
        }

        //Returns the cost for this size, if any
        public float GetModuleCost(float defaultCost)
        {
            if (this.sizes.Count > 0) { return this.sizes[this.size].cost; }
            return 0;
        }
        #endregion

        #region Functions
        private void Update()
        {
            //Updating of size if possible
            if (!CompatibilityChecker.IsAllCompatible() || (!HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight)) { return; }

            if (this.sizes.Count > 0 && this.part.transform.GetChild(0).localScale != Vector3.Scale(this.originalSize, this.sizes[this.size].size))
            {
                UpdateScale(this.part, this.rcModule);
            }

            //If unselected
            if (!HighLogic.LoadedSceneIsEditor || !EditorLogic.fetch || EditorLogic.fetch.editorScreen != EditorScreen.Actions)
            {
                this.editorGUI.visible = false;
                return;
            }

            //Checks if the part is selected
            if (this.actionPanel.GetSelectedParts().Contains(this.part))
            {
                this.editorGUI.visible = true;
            }
            else
            {
                this.editorGUI.visible = false;
                this.chutes.ForEach(c => c.templateGUI.materialsVisible = false);
                this.editorGUI.failedVisible = false;
                this.editorGUI.successfulVisible = false;
            }
            //Checks if size must update
            if (this.sizes.Count > 0 && this.lastSize != this.size) { UpdateScale(this.part, this.rcModule); }
            //Checks if case texture must update
            if (this.textures.cases.Count > 0 && this.lastCaseID != this.caseID) { UpdateCaseTexture(this.part, this.rcModule); }
            this.chutes.ForEach(c => c.SwitchType());
        }

        private void OnGUI()
        {
            //Rendering manager
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            this.editorGUI.RenderGUI();
        }
        #endregion

        #region Overrides
        public override void OnStart(PartModule.StartState state)
        {
            if ((!HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight) || !CompatibilityChecker.IsAllCompatible()) { return; }

            //Identification of the RealChuteModule
            if (this.part.Modules.Contains("RealChuteModule")) { this.rcModule = this.part.Modules["RealChuteModule"] as RealChuteModule; }
            else { return; }
            this.secondaryChute = this.rcModule.secondaryChute;
            if (!string.IsNullOrEmpty(this.textureLibrary)) { this.textureLib.TryGetConfig(this.textureLibrary, ref this.textures); }
            this.bodies = AtmoPlanets.fetch;
            this.body = this.bodies.GetBody(this.planets);

            //Initializes ChuteTemplates
            LoadChutes();
            this.chutes.ForEach(c => c.Initialize());
            if (this.sizes.Count <= 0) { this.sizes = this.sizeLib.GetSizes(this.part.partInfo.name); }

            //Creates an instance of the texture library
            this.editorGUI = new EditorGUI(this);
            if (HighLogic.LoadedSceneIsEditor)
            {
                //Windows initiation
                this.editorGUI.window = new Rect(5, 370, 420, Screen.height - 375);
                this.chutes.ForEach(c =>
                {
                    c.templateGUI.materialsWindow = new Rect(this.editorGUI.matX, this.editorGUI.matY, 375, 275);
                    c.templateGUI.drag = new Rect(0, 0, 375, 25);
                });
                this.editorGUI.failedWindow = new Rect(Screen.width / 2 - 150, Screen.height / 2 - 150, 300, 300);
                this.editorGUI.successfulWindow = new Rect(Screen.width / 2 - 150, Screen.height / 2 - 25, 300, 50);
                this.editorGUI.presetsWindow = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 250, 400, 500);
                this.editorGUI.presetsSaveWindow = new Rect(Screen.width / 2 - 175, Screen.height / 2 - 110, 350, 220);
                this.editorGUI.presetsWarningWindow = new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 200, 100);

                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                {
                    float level = 0;
                    switch (EditorDriver.editorFacility)
                    {
                        case EditorFacility.VAB:
                            level = ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.VehicleAssemblyBuilding); break;

                        case EditorFacility.SPH:
                            level = ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.SpaceplaneHangar); break;

                        default:
                            break;
                    }
                    if (GameVariables.Instance.UnlockedActionGroupsStock(level))
                    {
                        Events.ForEach(e => e.guiActiveEditor = false);
                    }
                }
                else
                {
                    Events.ForEach(e => e.guiActiveEditor = false);
                }

                //Gets the original part state
                if (this.textures != null && this.caseID == -1)
                {
                    if (this.caseID == -1)
                    {
                        if (this.textures.TryGetCase(this.currentCase, ref this.parachuteCase)) { this.caseID = this.textures.GetCaseIndex(this.parachuteCase.name); }
                    }
                    else
                    {
                        this.textures.TryGetCase(this.caseID, this.type, ref this.parachuteCase);
                    }
                    this.lastCaseID = this.caseID;
                }

                if (!this.initiated)
                {
                    if (!this.bodies.TryGetBodyIndex("Kerbin", ref this.planets)) { this.planets = 0; }
                    this.body = this.bodies.GetBody(planets);

                    //Identification of the values from the RealChuteModule
                    this.mustGoDown = this.rcModule.mustGoDown;
                    this.deployOnGround = this.rcModule.deployOnGround;
                    this.timer = this.rcModule.timer + "s";
                    this.cutSpeed = this.rcModule.cutSpeed.ToString();
                    if (this.rcModule.spareChutes != -1) { this.spares = this.rcModule.spareChutes.ToString(); }
                    this.originalSize = this.part.transform.GetChild(0).localScale;
                    this.initiated = true;
                }
            }
            else  if (this.textures != null)
            {
                this.textures.TryGetCase(this.caseID, this.type, ref this.parachuteCase);
                this.lastCaseID = this.caseID;
            }

            if (this.parent == null) { this.parent = this.part.FindModelTransform(this.rcModule.parachutes[0].parachuteName).parent; }

            //Updates the part
            if (this.textures != null)
            {
                UpdateCaseTexture(this.part, this.rcModule);
                this.editorGUI.cases = this.textures.caseNames;
                this.editorGUI.canopies = this.textures.canopyNames;
                this.editorGUI.models = this.textures.modelNames;
            }
            UpdateScale(this.part, this.rcModule);
        }

        public override void OnLoad(ConfigNode node)
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            this.node = node;
            LoadChutes();
            if (this.node.HasNode("SIZE"))
            {
                this.sizes = new List<SizeNode>(this.node.GetNodes("SIZE").Select(n => new SizeNode(n)));
                this.sizeLib.AddSizes(this.part.name, this.sizes);
            }

            //Top node original location
            AttachNode a;
            if (this.part.TryGetAttachNodeById("top", out a))
            {
                this.top = a.originalPosition.y;
            }

            //Bottom node original location
            if (this.part.TryGetAttachNodeById("bottom", out a))
            {
                this.bottom = a.originalPosition.y;
            }

            //Original part size
            if (this.debut == 0) { this.debut = this.part.transform.GetChild(0).localScale.y; }
        }

        public override string GetInfo()
        {
            if (!CompatibilityChecker.IsAllCompatible() || !this.isTweakable || !this.part.Modules.Contains("RealChuteModule")) { return string.Empty; }
            return "This RealChute part can be tweaked from the Action Groups window.";
        }

        public override void OnSave(ConfigNode node)
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            //Saves the templates to the persistence or craft file
            this.chutes.ForEach(c => node.AddNode(c.Save()));
        }
        #endregion
    }
}