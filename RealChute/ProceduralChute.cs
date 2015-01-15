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
        #region Config values
        [KSPField]
        public string textureLibrary = "none";
        [KSPField]
        public string type = "Cone";
        [KSPField]
        public string currentCase = "none";
        [KSPField]
        public string currentCanopies = "none";
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
        internal TextureConfig textures = new TextureConfig();
        internal CaseConfig parachuteCase = new CaseConfig();
        internal List<ChuteTemplate> chutes = new List<ChuteTemplate>();
        internal PresetsLibrary presets = PresetsLibrary.instance;
        internal CelestialBody body = null;
        internal EditorGUI editorGUI = new EditorGUI();

        //Sizes
        private SizeManager sizeLib = SizeManager.instance;
        public List<SizeNode> sizes = new List<SizeNode>();
        [SerializeField]
        private Transform parent = null;
        public ConfigNode node = null;
        #endregion

        #region Part GUI
        [KSPEvent(active = true, guiActiveEditor = true, guiName = "Next size")]
        public void GUINextSize()
        {
            this.size++;
            if (this.size > this.sizes.Count - 1) { this.size = 0; }
            if (RealChuteSettings.fetch.guiResizeUpdates) { this.Apply(false, false); }
        }

        [KSPEvent(active = true, guiActiveEditor = true, guiName = "Previous size")]
        public void GUIPreviousSize()
        {
            this.size--;
            if (this.size < 0) { this.size = this.sizes.Count - 1; }
            if (RealChuteSettings.fetch.guiResizeUpdates) { this.Apply(false, false); }
        }
        #endregion

        #region Methods
        //Gets the strings for the selection grids
        internal string[] TextureEntries(string entries)
        {
            if (textureLibrary == "none") { return new string[] { }; }
            if (entries == "case" && textures.caseNames.Length > 1) { return textures.caseNames.Where(c => textures.GetCase(c).types.Contains(type)).ToArray(); }
            if (entries == "chute" && textures.canopyNames.Length > 1) { return textures.canopyNames; }
            if (entries == "model" && textures.modelNames.Length > 1) { return textures.modelNames.Where(m => textures.GetModel(m).parameters.Count >= this.chutes.Count).ToArray(); }
            return new string[] { };
        }

        //Gets the total mass of the craft
        internal float GetCraftMass(bool dry)
        {
            return EditorLogic.SortedShipList.Where(p => p.physicalSignificance != Part.PhysicalSignificance.NONE).Sum(p => dry ? p.mass : p.TotalMass());
        }

        //Lists the errors of a given type
        internal List<string> GetErrors(string type)
        {
            if (type == "general")
            {
                List<string> general = new List<string>();

                if (!RCUtils.CanParseTime(timer) || !RCUtils.CheckRange(RCUtils.ParseTime(timer), 0, 3600)) { general.Add("Deployment timer"); }
                if (!RCUtils.CanParseWithEmpty(spares) || !RCUtils.CheckRange(RCUtils.ParseWithEmpty(spares), -1, 10) || !RCUtils.IsWholeNumber(RCUtils.ParseWithEmpty(spares))) { general.Add("Spare chutes"); }
                if (!RCUtils.CanParse(cutSpeed) || !RCUtils.CheckRange(float.Parse(cutSpeed), 0.01f, 100)) { general.Add("Autocut speed"); }
                if (!RCUtils.CanParse(landingAlt) || !RCUtils.CheckRange(float.Parse(landingAlt), 0, (float)body.GetMaxAtmosphereAltitude())) { general.Add("Landing altitude"); }
                return general;
            }
            else if (type == "main" || type == "secondary") { return chutes.SelectMany(c => c.templateGUI.errors).ToList(); }
            return new List<string>();
        }

        //Creates labels for errors.
        internal void CreateErrors()
        {
            if (GetErrors("general").Count != 0)
            {
                GUILayout.Label("General:", skins.label);
                foreach (string error in GetErrors("general"))
                {
                    GUILayout.Label(error, RCUtils.redLabel);
                }
                GUILayout.Space(10);
            }

            if (GetErrors("main").Count != 0)
            {
                GUILayout.Label("Main chute:", skins.label);
                foreach (string error in GetErrors("main"))
                {
                    GUILayout.Label(error, RCUtils.redLabel);
                }
                GUILayout.Space(10);
            }

            if (secondaryChute && GetErrors("secondary").Count != 0)
            {
                GUILayout.Label("Secondary chute:", skins.label);
                foreach (string error in GetErrors("secondary"))
                {
                    GUILayout.Label(error, RCUtils.redLabel);
                }
                GUILayout.Space(10);
            }
        }

        //Applies the parameters to the parachute
        internal void Apply(bool toSymmetryCounterparts, bool showMessage = true)
        {
            if (!showMessage && (GetErrors("general").Count != 0 || GetErrors("main").Count != 0 || (secondaryChute && GetErrors("secondary").Count != 0))) { this.editorGUI.failedVisible = true; return; }
            rcModule.mustGoDown = mustGoDown;
            rcModule.deployOnGround = deployOnGround;
            rcModule.timer = RCUtils.ParseTime(timer);
            rcModule.cutSpeed = float.Parse(cutSpeed);
            rcModule.spareChutes = RCUtils.ParseWithEmpty(spares);
            rcModule.chuteCount = (int)rcModule.spareChutes;

            chutes.ForEach(c => c.ApplyChanges(toSymmetryCounterparts));
            if (toSymmetryCounterparts)
            {
                foreach (Part part in this.part.symmetryCounterparts)
                {
                    RealChuteModule module = part.Modules["RealChuteModule"] as RealChuteModule;
                    UpdateCaseTexture(part, module);
                    UpdateScale(part, module);

                    module.mustGoDown = mustGoDown;
                    module.timer = RCUtils.ParseTime(timer);
                    module.cutSpeed = float.Parse(cutSpeed);
                    module.spareChutes = RCUtils.ParseWithEmpty(spares);

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
            this.part.mass = rcModule.caseMass + rcModule.parachutes.Sum(p => p.chuteMass);
            if (showMessage)
            {
                this.editorGUI.successfulVisible = true;
                if (!editorGUI.warning) { editorGUI.successfulWindow.height = 50; }
            }
            GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
        }

        //Checks if th given AttachNode has the parent part
        private bool CheckParentNode(AttachNode node)
        {
            return node.attachedPart != null && part.parent != null && node.attachedPart == part.parent;
        }

        //Modifies the size of a part
        private void UpdateScale(Part part, RealChuteModule module)
        {
            //Thanks to Brodicus for the help here
            if (sizes.Count <= 1) { return; }
            SizeNode size = sizes[this.size], lastSize = sizes[this.lastSize];
            Transform root = part.transform.GetChild(0);
            root.localScale = Vector3.Scale(originalSize, size.size);
            module.caseMass = size.caseMass;
            AttachNode topNode = null, bottomNode = null;
            bool hasTopNode = part.TryGetAttachNodeById("top", out topNode);
            bool hasBottomNode = part.TryGetAttachNodeById("bottom", out bottomNode);
            List<Part> allTopChildParts = null, allBottomChildParts = null;

            // If this is the root part, move things for the top and the bottom.
            if ((HighLogic.LoadedSceneIsEditor && part == EditorLogic.SortedShipList[0]) || (HighLogic.LoadedSceneIsFlight && this.vessel.rootPart == part))
            {
                if (hasTopNode)
                {
                    topNode.position = size.topNode;
                    topNode.size = size.topNodeSize;
                    if (topNode.attachedPart != null)
                    {
                        float topDifference = size.topNode.y - lastSize.topNode.y;
                        topNode.attachedPart.transform.Translate(0, topDifference, 0, part.transform);
                        if (allTopChildParts == null) { allTopChildParts = topNode.attachedPart.GetAllChildren(); }
                        allTopChildParts.ForEach(c => c.transform.Translate(0, topDifference, 0, part.transform));
                    }
                }

                if (hasBottomNode)
                {
                    bottomNode.position = size.bottomNode;
                    bottomNode.size = size.bottomNodeSize;
                    if (bottomNode.attachedPart != null)
                    {
                        float bottomDifference = size.bottomNode.y - lastSize.bottomNode.y;
                        bottomNode.attachedPart.transform.Translate(0, bottomDifference, 0, part.transform);
                        if (allBottomChildParts == null) { allBottomChildParts = bottomNode.attachedPart.GetAllChildren(); }
                        allBottomChildParts.ForEach(c => c.transform.Translate(0, bottomDifference, 0, part.transform));
                    }
                }
            }

            // If not root and parent is attached to the bottom
            else if (hasBottomNode && CheckParentNode(bottomNode))
            {
                bottomNode.position = size.bottomNode;
                bottomNode.size = size.bottomNodeSize;
                float bottomDifference = size.bottomNode.y - lastSize.bottomNode.y;
                part.transform.Translate(0, -bottomDifference, 0, part.transform);
                if (hasTopNode)
                {
                    topNode.position = size.topNode;
                    topNode.size = size.topNodeSize;
                    if (topNode.attachedPart != null)
                    {
                        float topDifference = size.topNode.y - lastSize.topNode.y;
                        topNode.attachedPart.transform.Translate(0, topDifference - bottomDifference, 0, part.transform);
                        if (allTopChildParts == null) { allTopChildParts = topNode.attachedPart.GetAllChildren(); }
                        allTopChildParts.ForEach(c => c.transform.Translate(0, topDifference - bottomDifference, 0, part.transform));
                    }
                }
            }
            // If not root and parent is attached to the top
            else if (hasTopNode && CheckParentNode(topNode))
            {
                topNode.position = size.topNode;
                topNode.size = size.topNodeSize;
                float topDifference = size.topNode.y - lastSize.topNode.y;
                part.transform.Translate(0, -topDifference, 0, part.transform);
                if (hasBottomNode)
                {
                    bottomNode.position = size.bottomNode;
                    bottomNode.size = size.bottomNodeSize;
                    if (bottomNode.attachedPart != null)
                    {
                        float bottomDifference = size.bottomNode.y - lastSize.bottomNode.y;
                        bottomNode.attachedPart.transform.Translate(0, bottomDifference - topDifference, 0, part.transform);
                        if (allBottomChildParts == null) { allBottomChildParts = bottomNode.attachedPart.GetAllChildren(); }
                        allBottomChildParts.ForEach(c => c.transform.Translate(0, bottomDifference - topDifference, 0, part.transform));
                    }
                }
            }

            //Parachute transforms
            float scaleX = root.localScale.x / Vector3.Scale(originalSize, lastSize.size).x;
            float scaleY = root.localScale.y / Vector3.Scale(originalSize, lastSize.size).y;
            float scaleZ = root.localScale.z / Vector3.Scale(originalSize, lastSize.size).z;
            foreach (Parachute chute in rcModule.parachutes)
            {
                Vector3 pos = chute.forcePosition - part.transform.position;
                chute.parachute.transform.Translate(pos.x * (scaleX - 1), pos.y * (scaleY - 1), pos.z * (scaleZ - 1), part.transform);
            }

            //Surface attached parts
            if (part.children.Any(c => c.attachMode == AttachModes.SRF_ATTACH))
            {
                foreach (Part child in part.children)
                {
                    if (child.attachMode == AttachModes.SRF_ATTACH)
                    {
                        Vector3 vX = new Vector3(), vY = new Vector3();
                        vX = (child.transform.localPosition + child.transform.localRotation * child.srfAttachNode.position) - part.transform.position;
                        vY = child.transform.position - part.transform.position;
                        child.transform.Translate(vX.x * (scaleX - 1), vY.y * (scaleY - 1), vX.z * (scaleZ - 1), part.transform);
                        child.GetAllChildren().ForEach(c => c.transform.Translate(vX.x * (scaleX - 1), vY.y * (scaleY - 1), vX.z * (scaleZ - 1), part.transform));
                    }
                }
            }
            this.lastSize = this.size;
        }

        //Modifies the case texture of a part
        private void UpdateCaseTexture(Part part, RealChuteModule module)
        {
            if (textureLibrary == "none" || currentCase == "none") { return; }
            if (textures.TryGetCase(caseID, type, ref parachuteCase))
            {
                if (string.IsNullOrEmpty(parachuteCase.textureURL))
                {
                    Debug.LogWarning("[RealChute]: The " + textures.caseNames[caseID] + "URL is empty");
                    lastCaseID = caseID;
                    return;
                }
                Texture2D texture = GameDatabase.Instance.GetTexture(parachuteCase.textureURL, false);
                if (texture == null)
                {
                    Debug.LogWarning("[RealChute]: The " + textures.caseNames[caseID] + "texture is null");
                    lastCaseID = caseID;
                    return;
                }
                part.GetPartRenderers(module).ForEach(r => r.material.mainTexture = texture);
            }
            lastCaseID = caseID;
        }

        //Applies the selected preset
        internal void ApplyPreset()
        {
            Preset preset = presets.GetPreset(presets.GetRelevantPresets(this)[presetID]);
            if (sizes.Any(s => s.sizeID == preset.sizeID)) { this.size = sizes.IndexOf(sizes.First(s => s.sizeID == preset.sizeID)); }
            this.cutSpeed = preset.cutSpeed;
            this.timer = preset.timer;
            this.mustGoDown = preset.mustGoDown;
            this.deployOnGround = preset.deployOnGround;
            this.spares = preset.spares;
            if ((this.textureLibrary == preset.textureLibrary || (this.textureLibrary != "none" && this.textures.caseNames.Contains(preset.caseName))) && this.textures.cases.Count > 0 && !string.IsNullOrEmpty(preset.caseName)) { this.caseID = textures.GetCaseIndex(textures.GetCase(preset.caseName)); }
            if (bodies.bodies.Values.Contains(preset.bodyName)) { this.planets = bodies.GetPlanetIndex(preset.bodyName); }
            chutes.ForEach(c => c.ApplyPreset(preset));
            Apply(false);
            print("[RealChute]: Applied the " + preset.name + " preset on " + this.part.partInfo.title);
        }

        //Creates and save a preset from the current stats
        internal void CreatePreset()
        {
            presets.AddPreset(new Preset(this));
            RealChuteSettings.SaveSettings();
            PopupDialog.SpawnPopupDialog("Preset saved", "The \"" + editorGUI.presetName + "\" preset was succesfully saved!", "Close", false, skins);
            print("[RealChute]: Saved the " + editorGUI.presetName + " preset to the settings file.");
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
            if (this.sizes.Count > 0) { return this.sizes[size].cost; }
            return 0;
        }
        #endregion

        #region Functions
        private void Update()
        {
            //Updating of size if possible
            if (!CompatibilityChecker.IsAllCompatible() || (!HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight)) { return; }

            if (sizes.Count > 0 && this.part.transform.GetChild(0).localScale != Vector3.Scale(originalSize, sizes[size].size))
            {
                UpdateScale(this.part, rcModule);
            }

            //If unselected
            if (!HighLogic.LoadedSceneIsEditor || !EditorLogic.fetch || EditorLogic.fetch.editorScreen != EditorScreen.Actions || !this.part.Modules.Contains("RealChuteModule"))
            {
                this.editorGUI.visible = false;
                return;
            }

            //Checks if the part is selected
            if (actionPanel.GetSelectedParts().Contains(this.part))
            {
                this.editorGUI.visible = true;
            }
            else
            {
                this.editorGUI.visible = false;
                chutes.ForEach(c => c.templateGUI.materialsVisible = false);
                this.editorGUI.failedVisible = false;
                this.editorGUI.successfulVisible = false;
            }
            //Checks if size must update
            if (sizes.Count > 0 && lastSize != size) { UpdateScale(this.part, rcModule); }
            //Checks if case texture must update
            if (this.textures.caseNames.Length > 0 && lastCaseID != caseID) { UpdateCaseTexture(this.part, rcModule); }
            chutes.ForEach(c => c.SwitchType());
        }

        private void OnGUI()
        {
            //Rendering manager
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            editorGUI.RenderGUI();
        }
        #endregion

        #region Overrides
        public override void OnStart(PartModule.StartState state)
        {
            if ((!HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight) || !CompatibilityChecker.IsAllCompatible()) { return; }

            //Identification of the RealChuteModule
            if (this.part.Modules.Contains("RealChuteModule")) { rcModule = this.part.Modules["RealChuteModule"] as RealChuteModule; }
            else { return; }
            secondaryChute = rcModule.secondaryChute;
            if (textureLibrary != "none") { textureLib.TryGetConfig(textureLibrary, ref textures); }
            bodies = AtmoPlanets.fetch;
            this.body = this.bodies.GetBody(planets);

            //Initializes ChuteTemplates
            LoadChutes();
            chutes.ForEach(c => c.Initialize());
            if (sizes.Count <= 0) { sizes = sizeLib.GetSizes(this.part.partInfo.name); }

            //Creates an instance of the texture library
            editorGUI = new EditorGUI(this);
            if (textureLibrary != "none")
            {
                editorGUI.cases = textures.caseNames;
                editorGUI.canopies = textures.canopyNames;
                editorGUI.models = textures.modelNames;
                textures.TryGetCase(caseID, type, ref parachuteCase);
                lastCaseID = caseID;
            }

            if (HighLogic.LoadedSceneIsEditor)
            {
                //Windows initiation
                this.editorGUI.window = new Rect(5, 370, 420, Screen.height - 375);
                this.chutes.ForEach(c => c.templateGUI.materialsWindow = new Rect(editorGUI.matX, editorGUI.matY, 375, 275));
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
                if (textureLibrary != "none" && caseID == -1)
                {
                    if (textures.TryGetCase(currentCase, ref parachuteCase)) { caseID = textures.GetCaseIndex(parachuteCase); }
                    lastCaseID = caseID;
                }

                if (!initiated)
                {
                    if (bodies.bodyNames.Contains("Kerbin")) { planets = bodies.GetPlanetIndex("Kerbin"); }
                    this.body = this.bodies.GetBody(planets);

                    //Identification of the values from the RealChuteModule
                    mustGoDown = rcModule.mustGoDown;
                    deployOnGround = rcModule.deployOnGround;
                    timer = rcModule.timer + "s";
                    cutSpeed = rcModule.cutSpeed.ToString();
                    if (rcModule.spareChutes != -1) { spares = rcModule.spareChutes.ToString(); }
                    originalSize = this.part.transform.GetChild(0).localScale;
                    initiated = true;
                }
            }

            if (parent == null) { parent = this.part.FindModelTransform(rcModule.parachutes[0].parachuteName).parent; }

            //Updates the part
            if (textureLibrary != "none")
            {
                UpdateCaseTexture(this.part, rcModule);
            }
            UpdateScale(this.part, rcModule);
        }

        public override void OnLoad(ConfigNode node)
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            this.node = node;
            LoadChutes();
            if (node.HasNode("SIZE"))
            {
                sizes = new List<SizeNode>(node.GetNodes("SIZE").Select(n => new SizeNode(n)));
                sizeLib.AddSizes(this.part.name, sizes);
            }

            //Top node original location
            if (this.part.findAttachNode("top") != null)
            {
                top = this.part.findAttachNode("top").originalPosition.y;
            }

            //Bottom node original location
            if (this.part.findAttachNode("bottom") != null)
            {
                bottom = this.part.findAttachNode("bottom").originalPosition.y;
            }

            //Original part size
            if (debut == 0) { debut = this.part.transform.GetChild(0).localScale.y; }
        }

        public override string GetInfo()
        {
            if (!CompatibilityChecker.IsAllCompatible() || !this.isTweakable) { return string.Empty; }
            else if (this.part.Modules.Contains("RealChuteModule")) { return "This RealChute part can be tweaked from the Action Groups window."; }
            return string.Empty;
        }

        public override void OnSave(ConfigNode node)
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            //Saves the templates to the persistence or craft file
            chutes.ForEach(c => node.AddNode(c.Save()));
        }
        #endregion
    }
}