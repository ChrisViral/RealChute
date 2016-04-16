using System.Collections.Generic;
using System.Linq;
using System.Text;
using RealChute.Extensions;
using UnityEngine;
using RealChute.Libraries;
using KSP.UI.Screens;

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
        public int caseId = -1, lastCaseId = -1;
        [KSPField(isPersistant = true)]
        public int size, lastSize, planets;
        [KSPField(isPersistant = true)]
        public int presetId;

        //Size vectors
        [KSPField(isPersistant = true)]
        public Vector3 originalSize;

        //Attach nodes
        [KSPField(isPersistant = true)]
        public float top, bottom, debut;

        //Bools
        [KSPField(isPersistant = true)]
        public bool initiated;
        [KSPField(isPersistant = true)]
        public bool mustGoDown, deployOnGround;
        [KSPField(isPersistant = true)]
        public bool secondaryChute;

        //GUI strings
        [KSPField(isPersistant = true)]
        public string timer = string.Empty, cutSpeed = string.Empty, spares = string.Empty, landingAlt = "0";
        #endregion

        #region Fields
        //Libraries
        private GUISkin skins = HighLogic.Skin;
        private EditorActionGroups actionPanel = EditorActionGroups.Instance;
        internal RealChuteModule rcModule;
        internal AtmoPlanets bodies;
        internal MaterialsLibrary materials = MaterialsLibrary.Instance;
        private TextureLibrary textureLib = TextureLibrary.Instance;
        internal TextureConfig textures;
        internal CaseConfig parachuteCase = new CaseConfig();
        internal PresetsLibrary presets = PresetsLibrary.Instance;
        internal CelestialBody body;
        internal EditorGui editorGui = new EditorGui();
        internal List<ChuteTemplate> chutes = new List<ChuteTemplate>();
        [SerializeField]
        private byte[] serializedChutes = new byte[0];
        public ConfigNode node;

        //Sizes
        private PersistentManager sizeLib = PersistentManager.Instance;
        public List<SizeNode> sizes = new List<SizeNode>();
        public Transform parent;
        #endregion

        #region Part GUI
        [KSPEvent(active = true, guiActiveEditor = true, guiName = "Next size")]
        public void GuiNextSize()
        {
            this.size++;
            if (this.size > this.sizes.Count - 1) { this.size = 0; }
            if (RealChuteSettings.Fetch.GuiResizeUpdates) { Apply(false, false); }
        }

        [KSPEvent(active = true, guiActiveEditor = true, guiName = "Previous size")]
        public void GuiPreviousSize()
        {
            this.size--;
            if (this.size < 0) { this.size = this.sizes.Count - 1; }
            if (RealChuteSettings.Fetch.GuiResizeUpdates) { Apply(false, false); }
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
                    return this.textures.CanopyNames;
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
                if (!GuiUtils.TryParseTime(this.timer, out f) || !GuiUtils.CheckRange(f, 0, 3600)) { errors.Add("Deployment timer"); }
                if (!GuiUtils.TryParseWithEmpty(this.spares, out f) || !GuiUtils.CheckRange(f, -1, 10) || !RCUtils.IsWholeNumber(f)) { errors.Add("Spare chutes"); }
                if (!float.TryParse(this.cutSpeed, out f) || !GuiUtils.CheckRange(f, 0.01f, 100)) { errors.Add("Autocut speed"); }
                if (!float.TryParse(this.landingAlt, out f) || !GuiUtils.CheckRange(f, 0, (float)this.body.GetMaxAtmosphereAltitude())) { errors.Add("Landing altitude"); }
                return errors;
            }
            else { return new List<string>(this.chutes.SelectMany(c => c.templateGui.Errors)); }
        }

        //Creates labels for errors.
        internal void CreateErrors()
        {
            GUILayout.Label("General:", this.skins.label);
            StringBuilder builder = new StringBuilder();
            builder.AppendJoin(GetErrors(true), "\n");
            GUILayout.Label(builder.ToString(), GuiUtils.RedLabel);
            GUILayout.Space(10);

            GUILayout.Label("Chutes:", this.skins.label);
            builder = new StringBuilder();
            builder.AppendJoin(GetErrors(false), "\n");
            GUILayout.Label(builder.ToString(), GuiUtils.RedLabel);
            GUILayout.Space(10);
        }

        //Applies the parameters to the parachute
        internal void Apply(bool toSymmetryCounterparts, bool showMessage = true)
        {
            if (!showMessage && (GetErrors(true).Count > 0 || GetErrors(false).Count > 0)) { this.editorGui.failedVisible = true; return; }
            this.rcModule.mustGoDown = this.mustGoDown;
            this.rcModule.deployOnGround = this.deployOnGround;
            this.rcModule.timer = GuiUtils.ParseTime(this.timer);
            this.rcModule.cutSpeed = float.Parse(this.cutSpeed);
            this.rcModule.spareChutes = GuiUtils.ParseEmpty(this.spares);
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
                    module.timer = GuiUtils.ParseTime(this.timer);
                    module.cutSpeed = float.Parse(this.cutSpeed);
                    module.spareChutes = GuiUtils.ParseEmpty(this.spares);

                    ProceduralChute pChute = part.Modules["ProceduralChute"] as ProceduralChute;
                    pChute.presetId = this.presetId;
                    pChute.planets = this.planets;
                    pChute.size = this.size;
                    pChute.caseId = this.caseId;
                    pChute.mustGoDown = this.mustGoDown;
                    pChute.deployOnGround = this.deployOnGround;
                    pChute.timer = this.timer;
                    pChute.cutSpeed = this.cutSpeed;
                    pChute.spares = this.spares;
                }
            }
            this.rcModule.UpdateMass();
            if (showMessage)
            {
                this.editorGui.successfulVisible = true;
                if (!this.editorGui.warning) { this.editorGui.successfulWindow.height = 50; }
            }
            GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
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
            root.localScale = Vector3.Scale(this.originalSize, size.Size);
            module.caseMass = size.CaseMass;
            module.UpdateMass();
            AttachNode topNode = null, bottomNode = null;
            bool hasTopNode = part.TryGetAttachNodeById("top", out topNode);
            bool hasBottomNode = part.TryGetAttachNodeById("bottom", out bottomNode);
            List<Part> allTopChildParts = null, allBottomChildParts = null;

            // If this is the root part, move things for the top and the bottom.
            if (HighLogic.LoadedSceneIsEditor && this.part == EditorLogic.SortedShipList[0] || HighLogic.LoadedSceneIsFlight && this.vessel.rootPart == this.part)
            {
                if (hasTopNode)
                {
                    topNode.position = size.TopNode;
                    topNode.size = size.TopNodeSize;
                    if (topNode.attachedPart != null)
                    {
                        float topDifference = size.TopNode.y - lastSize.TopNode.y;
                        topNode.attachedPart.transform.Translate(0, topDifference, 0, this.part.transform);
                        if (allTopChildParts == null) { allTopChildParts = topNode.attachedPart.GetAllChildren(); }
                        allTopChildParts.ForEach(c => c.transform.Translate(0, topDifference, 0, this.part.transform));
                    }
                }

                if (hasBottomNode)
                {
                    bottomNode.position = size.BottomNode;
                    bottomNode.size = size.BottomNodeSize;
                    if (bottomNode.attachedPart != null)
                    {
                        float bottomDifference = size.BottomNode.y - lastSize.BottomNode.y;
                        bottomNode.attachedPart.transform.Translate(0, bottomDifference, 0, this.part.transform);
                        if (allBottomChildParts == null) { allBottomChildParts = bottomNode.attachedPart.GetAllChildren(); }
                        allBottomChildParts.ForEach(c => c.transform.Translate(0, bottomDifference, 0, this.part.transform));
                    }
                }
            }

            // If not root and parent is attached to the bottom
            else if (hasBottomNode && CheckParentNode(bottomNode))
            {
                bottomNode.position = size.BottomNode;
                bottomNode.size = size.BottomNodeSize;
                float bottomDifference = size.BottomNode.y - lastSize.BottomNode.y;
                this.part.transform.Translate(0, -bottomDifference, 0, this.part.transform);
                if (hasTopNode)
                {
                    topNode.position = size.TopNode;
                    topNode.size = size.TopNodeSize;
                    if (topNode.attachedPart != null)
                    {
                        float diff = size.TopNode.y - lastSize.TopNode.y - bottomDifference;
                        topNode.attachedPart.transform.Translate(0, diff, 0, this.part.transform);
                        if (allTopChildParts == null) { allTopChildParts = topNode.attachedPart.GetAllChildren(); }
                        allTopChildParts.ForEach(c => c.transform.Translate(0, diff, 0, this.part.transform));
                    }
                }
            }
            // If not root and parent is attached to the top
            else if (hasTopNode && CheckParentNode(topNode))
            {
                topNode.position = size.TopNode;
                topNode.size = size.TopNodeSize;
                float topDifference = size.TopNode.y - lastSize.TopNode.y;
                this.part.transform.Translate(0, -topDifference, 0, this.part.transform);
                if (hasBottomNode)
                {
                    bottomNode.position = size.BottomNode;
                    bottomNode.size = size.BottomNodeSize;
                    if (bottomNode.attachedPart != null)
                    {
                        float diff = size.BottomNode.y - lastSize.BottomNode.y - topDifference;
                        bottomNode.attachedPart.transform.Translate(0, diff, 0, part.transform);
                        if (allBottomChildParts == null) { allBottomChildParts = bottomNode.attachedPart.GetAllChildren(); }
                        allBottomChildParts.ForEach(c => c.transform.Translate(0, diff, 0, part.transform));
                    }
                }
            }

            //Parachute transforms
            Vector3 scale = Vector3.Scale(this.originalSize, lastSize.Size);
            float scaleX = root.localScale.x / scale.x;
            float scaleY = root.localScale.y / scale.y;
            float scaleZ = root.localScale.z / scale.z;
            foreach (Parachute chute in this.rcModule.parachutes)
            {
                Vector3 pos = chute.ForcePosition - this.part.transform.position;
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
            this.part.SendMessage("RC_Rescale", new Vector3(scaleX, scaleY, scaleZ));
            if (HighLogic.LoadedSceneIsEditor) { GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship); }
        }

        //Modifies the case texture of a part
        private void UpdateCaseTexture(Part part, RealChuteModule module)
        {
            if (this.textures == null) { return; }
            if (this.textures.TryGetCase(this.caseId, this.type, ref this.parachuteCase))
            {
                if (string.IsNullOrEmpty(this.parachuteCase.TextureURL))
                {
                    Debug.LogWarning("[RealChute]: The " + this.parachuteCase.Name + "URL is empty");
                    this.lastCaseId = this.caseId;
                    return;
                }
                Texture2D texture = GameDatabase.Instance.GetTexture(this.parachuteCase.TextureURL, false);
                if (texture == null)
                {
                    Debug.LogWarning("[RealChute]: The " + this.parachuteCase.Name + "texture is null");
                    this.lastCaseId = this.caseId;
                    return;
                }
                this.part.GetPartRenderers(module).ForEach(r => r.material.mainTexture = texture);
            }
            this.lastCaseId = this.caseId;
        }

        //Applies the selected preset
        internal void ApplyPreset()
        {
            Preset preset = this.presets.GetPreset(this.presetId, this.chutes.Count);
            if (this.sizes.Exists(s => s.SizeId == preset.SizeId)) { this.size = this.sizes.IndexOf(this.sizes.Find(s => s.SizeId == preset.SizeId)); }
            this.cutSpeed = preset.CutSpeed;
            this.timer = preset.Timer;
            this.mustGoDown = preset.MustGoDown;
            this.deployOnGround = preset.DeployOnGround;
            this.spares = preset.Spares;
            if (this.textureLibrary == preset.TextureLibrary || this.textures != null && this.textures.ContainsCase(preset.CaseName))
            {
                this.caseId = this.textures.GetCaseIndex(preset.CaseName);
            }
            this.chutes.ForEach(c => c.ApplyPreset(preset));
            Apply(false);
            print("[RealChute]: Applied the " + preset.Name + " preset on " + this.part.partInfo.title);
        }

        //Creates and save a preset from the current stats
        internal void CreatePreset()
        {
            this.presets.AddPreset(new Preset(this));
            RealChuteSettings.SaveSettings();
            RCUtils.PopupDialog("Preset saved", "The \"" + this.editorGui.presetName + "\" preset was succesfully saved!", "Close");
            print("[RealChute]: Saved the " + this.editorGui.presetName + " preset to the settings file.");
        }

        //Reloads the size nodes
        private void LoadChutes()
        {
            if (this.chutes.Count <= 0)
            {
                if (this.node != null && this.node.HasNode("CHUTE"))
                {
                    this.chutes = new List<ChuteTemplate>(this.node.GetNodes("CHUTE").Select((n, i) => new ChuteTemplate(this, n, i)));
                }
                else
                {
                    RealChuteModule module = this.rcModule ?? this.part.Modules["RealChuteModule"] as RealChuteModule;
                    if (module.parachutes.Count > 0)
                    {
                        for (int i = 0; i < module.parachutes.Count; i++)
                        {
                            this.chutes.Add(new ChuteTemplate(this, null, i));
                        }
                    }
                }
            }
        }

        //Returns the cost for this size, if any
        public float GetModuleCost(float defaultCost)
        {
            if (this.sizes.Count > 0) { return this.sizes[this.size].Cost; }
            return 0;
        }
        #endregion

        #region Functions
        private void Update()
        {
            //Updating of size if possible
            if (!CompatibilityChecker.IsAllCompatible() || !HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight) { return; }

            if (this.sizes.Count > 0 && this.part.transform.GetChild(0).localScale != Vector3.Scale(this.originalSize, this.sizes[this.size].Size))
            {
                UpdateScale(this.part, this.rcModule);
            }

            //If unselected
            if (!HighLogic.LoadedSceneIsEditor || !EditorLogic.fetch || EditorLogic.fetch.editorScreen != EditorScreen.Actions)
            {
                this.editorGui.visible = false;
                return;
            }

            //Checks if the part is selected
            if (this.actionPanel.GetSelectedParts().Contains(this.part))
            {
                this.editorGui.visible = true;
            }
            else
            {
                this.editorGui.visible = false;
                this.chutes.ForEach(c => c.templateGui.materialsVisible = false);
                this.editorGui.failedVisible = false;
                this.editorGui.successfulVisible = false;
            }
            //Checks if size must update
            if (this.sizes.Count > 0 && this.lastSize != this.size) { UpdateScale(this.part, this.rcModule); }
            //Checks if case texture must update
            if (this.textures.Cases.Count > 0 && this.lastCaseId != this.caseId) { UpdateCaseTexture(this.part, this.rcModule); }
            this.chutes.ForEach(c => c.SwitchType());
        }

        private void OnGUI()
        {
            //Rendering manager
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            this.editorGui.RenderGui();
        }
        #endregion

        #region Overrides
        public override void OnStart(StartState state)
        {
            if (!HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight || !CompatibilityChecker.IsAllCompatible()) { return; }
            //Identification of the RealChuteModule
            if (this.part.Modules.Contains("RealChuteModule")) { this.rcModule = this.part.Modules["RealChuteModule"] as RealChuteModule; }
            else { return; }
            this.secondaryChute = this.rcModule.SecondaryChute;
            if (!string.IsNullOrEmpty(this.textureLibrary)) { this.textureLib.TryGetConfig(this.textureLibrary, ref this.textures); }
            this.bodies = AtmoPlanets.Fetch;
            this.body = this.bodies.GetBody(this.planets);

            //Initializes ChuteTemplates
            if (this.chutes.Count <= 0)
            {
                if (this.node == null && !PersistentManager.Instance.TryGetNode<ProceduralChute>(this.part.name, ref this.node)) { return; }
                LoadChutes();
            }
            this.chutes.ForEach(c => c.Initialize());
            if (this.sizes.Count <= 0) { this.sizes = this.sizeLib.GetSizes(this.part.partInfo.name); }

            //Creates an instance of the texture library
            this.editorGui = new EditorGui(this);
            if (HighLogic.LoadedSceneIsEditor)
            {
                //Windows initiation
                this.editorGui.window = new Rect(5, 390, 420, Screen.height - 395);
                this.chutes.ForEach(c =>
                {
                    c.templateGui.materialsWindow = new Rect(this.editorGui.matX, this.editorGui.matY, 375, 275);
                    c.templateGui.drag = new Rect(0, 0, 375, 25);
                });
                this.editorGui.failedWindow = new Rect(Screen.width / 2 - 150, Screen.height / 2 - 150, 300, 300);
                this.editorGui.successfulWindow = new Rect(Screen.width / 2 - 150, Screen.height / 2 - 25, 300, 50);
                this.editorGui.presetsWindow = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 250, 400, 500);
                this.editorGui.presetsSaveWindow = new Rect(Screen.width / 2 - 175, Screen.height / 2 - 110, 350, 220);
                this.editorGui.presetsWarningWindow = new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 200, 100);

                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                {
                    float level = 0;
                    bool isVab = true;
                    switch (EditorDriver.editorFacility)
                    {
                        case EditorFacility.VAB:
                            level = ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.VehicleAssemblyBuilding); break;

                        case EditorFacility.SPH:
                            level = ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.SpaceplaneHangar); isVab = false; break;

                        default:
                            break;
                    }
                    if (GameVariables.Instance.UnlockedActionGroupsStock(level, isVab))
                    {
                        this.Events.ForEach(e => e.guiActiveEditor = false);
                    }
                }
                else
                {
                    this.Events.ForEach(e => e.guiActiveEditor = false);
                }

                //Gets the original part state
                if (this.textures != null && this.caseId == -1)
                {
                    if (this.caseId == -1)
                    {
                        if (this.textures.TryGetCase(this.currentCase, ref this.parachuteCase)) { this.caseId = this.textures.GetCaseIndex(this.parachuteCase.Name); }
                    }
                    else
                    {
                        this.textures.TryGetCase(this.caseId, this.type, ref this.parachuteCase);
                    }
                    this.lastCaseId = this.caseId;
                }

                if (!this.initiated)
                {
                    if (!this.bodies.TryGetBodyIndex("Kerbin", ref this.planets)) { this.planets = 0; }
                    this.body = this.bodies.GetBody(this.planets);

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
                this.textures.TryGetCase(this.caseId, this.type, ref this.parachuteCase);
                this.lastCaseId = this.caseId;
            }

            if (this.parent == null) { this.parent = this.part.FindModelTransform(this.rcModule.parachutes[0].parachuteName).parent; }

            //Updates the part
            if (this.textures != null)
            {
                UpdateCaseTexture(this.part, this.rcModule);
                this.editorGui.cases = this.textures.CaseNames;
                this.editorGui.canopies = this.textures.CanopyNames;
                this.editorGui.models = this.textures.ModelNames;
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

            if (HighLogic.LoadedScene == GameScenes.LOADING)
            {
                PersistentManager.Instance.AddNode<ProceduralChute>(this.part.name, node);
            }
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