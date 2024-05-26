using System.Collections.Generic;
using System.Linq;
using System.Text;
using Highlighting;
using RealChute.Extensions;
using RealChute.Libraries;
using RealChute.Libraries.Presets;
using RealChute.Libraries.TextureLibrary;
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
        internal RealChuteModule rcModule;
        internal TextureConfig textures;
        internal CaseConfig parachuteCase = new CaseConfig();
        internal CelestialBody body;
        internal EditorGUI editorGUI = new EditorGUI();
        internal List<ChuteTemplate> chutes = new List<ChuteTemplate>();
        public ConfigNode node;

        //Sizes
        public List<SizeNode> sizes = new List<SizeNode>();
        public Transform parent;

        #endregion

        #region Part GUI
        private static ProceduralChute editorVisibleChute;

        [KSPEvent(active = true, guiActiveEditor = true, guiName = "Show Parachute Editor")]
        public void GUIToggleEditor()
        {
            if (!this.editorGUI.visible)
            {
                ShowEditor();
            }
            else
            {
                HideEditor();
            }
        }

        private void ShowEditor()
        {
            if (this.editorGUI?.visible ?? true) return;

            if (editorVisibleChute)
            {
                editorVisibleChute.HideEditor();
            }
            else
            {
                EditorGUI.ResetWindowLocation();
            }

            this.editorGUI.visible = true;
            this.Events[nameof(GUIToggleEditor)].guiName = "Hide Parachute Editor";
            editorVisibleChute = this;
        }

        internal void HideEditor()
        {
            if (!this.editorGUI?.visible ?? true) return;

            this.editorGUI.visible = false;
            this.editorGUI.failedVisible = false;
            this.editorGUI.successfulVisible = false;
            this.editorGUI.presetVisible = false;
            this.editorGUI.presetSaveVisible = false;
            this.editorGUI.presetWarningVisible = false;
            this.chutes.ForEach(c => c.templateGUI.materialsVisible = false);
            this.Events[nameof(GUIToggleEditor)].guiName = "Show Parachute Editor";
            editorVisibleChute = null;

            // Disable part highlighting
            HighlightPart(this.part, false);
            this.part.symmetryCounterparts.ForEach(p => HighlightPart(p, false));
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
        internal float GetCraftMass(bool dry) => EditorLogic.SortedShipList.Where(p => p.physicalSignificance != Part.PhysicalSignificance.NONE).Sum(p => dry ? p.mass : p.TotalMass());

        //Lists the errors of a given type
        internal List<string> GetErrors(bool general)
        {
            if (general)
            {
                List<string> errors = new List<string>();
                if (!GUIUtils.TryParseTime(this.timer, out float f) || !GUIUtils.CheckRange(f, 0, 3600)) { errors.Add("Deployment timer"); }
                if (!GUIUtils.TryParseWithEmpty(this.spares, out f) || !GUIUtils.CheckRange(f, -1, 10) || !RCUtils.IsWholeNumber(f)) { errors.Add("Spare chutes"); }
                if (!float.TryParse(this.cutSpeed, out f) || !GUIUtils.CheckRange(f, 0.01f, 100)) { errors.Add("Autocut speed"); }
                if (!float.TryParse(this.landingAlt, out f) || !GUIUtils.CheckRange(f, 0, (float)this.body.GetMaxAtmosphereAltitude())) { errors.Add("Landing altitude"); }
                return errors;
            }
            return new List<string>(this.chutes.SelectMany(c => c.templateGUI.Errors));
        }

        //Creates labels for errors.
        internal void CreateErrors()
        {
            GUILayout.Label("General:");
            StringBuilder builder = new StringBuilder();
            builder.AppendJoin(GetErrors(true), "\n");
            GUILayout.Label(builder.ToString(), GUIUtils.RedLabel);
            GUILayout.Space(10);

            GUILayout.Label("Chutes:");
            builder = new StringBuilder();
            builder.AppendJoin(GetErrors(false), "\n");
            GUILayout.Label(builder.ToString(), GUIUtils.RedLabel);
            GUILayout.Space(10);
        }

        //Applies the parameters to the parachute
        internal void Apply(bool toSymmetryCounterparts, bool showMessage = true)
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
                foreach (Part p in this.part.symmetryCounterparts)
                {
                    RealChuteModule module = (RealChuteModule)p.Modules["RealChuteModule"];
                    UpdateCaseTexture(module);
                    UpdateScale(p, module);

                    module.mustGoDown = this.mustGoDown;
                    module.timer = GUIUtils.ParseTime(this.timer);
                    module.cutSpeed = float.Parse(this.cutSpeed);
                    module.spareChutes = GUIUtils.ParseEmpty(this.spares);

                    ProceduralChute pChute = (ProceduralChute)p.Modules["ProceduralChute"];
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
                this.editorGUI.successfulVisible = true;
                if (!this.editorGUI.warning) { this.editorGUI.successfulWindow.height = 50; }
            }
            GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
        }

        //Checks if th given AttachNode has the parent part
        private bool CheckParentNode(AttachNode node) => node.attachedPart != null && this.part.parent != null && node.attachedPart == this.part.parent;

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
            bool hasTopNode = part.TryGetAttachNodeById("top", out AttachNode topNode);
            bool hasBottomNode = part.TryGetAttachNodeById("bottom", out AttachNode bottomNode);
            List<Part> allTopChildParts, allBottomChildParts;

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
                        allTopChildParts = topNode.attachedPart.GetAllChildren();
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
                        allBottomChildParts = bottomNode.attachedPart.GetAllChildren();
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
                        allTopChildParts = topNode.attachedPart.GetAllChildren();
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
                        allBottomChildParts = bottomNode.attachedPart.GetAllChildren();
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
                        Vector3 vX = (child.transform.localPosition + (child.transform.localRotation * child.srfAttachNode.position)) - this.part.transform.position;
                        Vector3 vY = child.transform.position - this.part.transform.position;
                        child.transform.Translate(vX.x * (scaleX - 1), vY.y * (scaleY - 1), vX.z * (scaleZ - 1), this.part.transform);
                        child.GetAllChildren().ForEach(c => c.transform.Translate(vX.x * (scaleX - 1), vY.y * (scaleY - 1), vX.z * (scaleZ - 1), this.part.transform));
                    }
                }
            }
            this.lastSize = this.size;
            this.part.SendMessage("RC_Rescale", new Vector3(scaleX, scaleY, scaleZ));
            if (HighLogic.LoadedSceneIsEditor) { GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship); }
            else if (HighLogic.LoadedSceneIsFlight) { this.rcModule.UpdateDragCubes(); }
        }

        //Modifies the case texture of a part
        private void UpdateCaseTexture(RealChuteModule module)
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
            Preset preset = PresetsLibrary.Instance.GetPreset(this.presetId, this.chutes.Count);
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
            PresetsLibrary.Instance.AddPreset(new Preset(this));
            RealChuteSettings.SaveSettings();
            RCUtils.PopupDialog("Preset saved", "The \"" + this.editorGUI.presetName + "\" preset was successfully saved!", "Close");
            print("[RealChute]: Saved the " + this.editorGUI.presetName + " preset to the settings file.");
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
                    RealChuteModule module = this.rcModule ?? (RealChuteModule)this.part.Modules["RealChuteModule"];
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

        //Sets the part scale temporarily to render the drag cube
        internal void SetDragCubeSize(bool on)
        {
            //Don't scale if there are not several sizes
            if (this.sizes.Count < 1) { return; }

            //Check for correct case
            if (on)
            {
                this.part.transform.localScale = this.part.transform.GetChild(0).localScale;
                this.part.transform.GetChild(0).localScale = Vector3.one;
            }
            else
            {
                this.part.transform.GetChild(0).localScale = this.part.transform.localScale;
                this.part.transform.localScale = Vector3.one;
            }
        }

        //Returns the cost for this size, if any
        public float GetModuleCost(float defaultCost, ModifierStagingSituation sit) => this.sizes.Count > 0 ? this.sizes[this.size].Cost : 0;

        //Highlights the part in the editor window
        private static void HighlightPart(Part partToHighlight, bool shouldHighlight, Color? highlightColour = null)
        {
            if (shouldHighlight)
            {
                partToHighlight.SetHighlightType(Part.HighlightType.AlwaysOn);
                partToHighlight.SetHighlightColor(highlightColour ?? Highlighter.colorPartEditorActionSelected);
            }
            else
            {
                partToHighlight.SetHighlightType(Part.HighlightType.Disabled);
                partToHighlight.SetHighlightColor();
            }

            partToHighlight.SetHighlight(shouldHighlight, false);
        }

        ModifierChangeWhen IPartCostModifier.GetModuleCostChangeWhen() => ModifierChangeWhen.FIXED;
        #endregion

        #region Functions
        private void Update()
        {
            //Updating of size if possible
            if (!CompatibilityChecker.IsAllCompatible|| !HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight) { return; }

            if (this.sizes.Count > 0 && this.part.transform.GetChild(0).localScale != Vector3.Scale(this.originalSize, this.sizes[this.size].Size))
            {
                UpdateScale(this.part, this.rcModule);
            }

            if (!HighLogic.LoadedSceneIsEditor)
            {
                HideEditor();
                return;
            }

            if (this.editorGUI.visible)
            {
                //We're running this every frame to avoid something else from clearing the highlight
                HighlightPart(this.part, true, Highlighter.colorPartHighlightDefault);
                this.part.symmetryCounterparts.ForEach(p => HighlightPart(p, true));
            }

            //Checks if size must update
            if (this.sizes.Count > 0 && this.lastSize != this.size) { UpdateScale(this.part, this.rcModule); }
            //Checks if case texture must update
            if (this.textures.Cases.Count > 0 && this.lastCaseId != this.caseId) { UpdateCaseTexture(this.rcModule); }
            this.chutes.ForEach(c => c.SwitchType());
        }

        private void OnGUI()
        {
            //Rendering manager
            if (!CompatibilityChecker.IsAllCompatible || !this.editorGUI.visible) { return; }

            GUI.skin = HighLogic.Skin;
            this.editorGUI.RenderGUI();
        }

        private void OnDestroy()
        {
            if (editorVisibleChute == this)
            {
                HideEditor();
            }
        }
        #endregion

        #region Overrides
        public override void OnStart(StartState state)
        {
            if (!HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight || !CompatibilityChecker.IsAllCompatible) { return; }
            //Identification of the RealChuteModule
            if (this.part.Modules.Contains("RealChuteModule")) { this.rcModule = (RealChuteModule)this.part.Modules["RealChuteModule"]; }
            else { return; }
            this.secondaryChute = this.rcModule.SecondaryChute;
            if (!string.IsNullOrEmpty(this.textureLibrary)) { TextureLibrary.Instance.TryGetConfig(this.textureLibrary, ref this.textures); }
            this.body = AtmoPlanets.Instance.GetBody(this.planets);

            //Initializes ChuteTemplates
            if (this.chutes.Count <= 0)
            {
                if (this.node == null && !PersistentManager.Instance.TryGetNode<ProceduralChute>(this.part.name, ref this.node)) { return; }
                LoadChutes();
            }
            this.chutes.ForEach(c => c.Initialize());
            if (this.sizes.Count <= 0) { this.sizes = PersistentManager.Instance.GetSizes(this.part.partInfo.name); }

            //Creates an instance of the texture library
            this.editorGUI = new EditorGUI(this);
            if (HighLogic.LoadedSceneIsEditor)
            {
                //Windows initiation
                this.editorGUI.windowDrag = new Rect(0f, 0f, 400f * GameSettings.UI_SCALE, 25f * GameSettings.UI_SCALE);
                this.editorGUI.closeButtonRect = new Rect(370f * GameSettings.UI_SCALE, 10f * GameSettings.UI_SCALE, 20f * GameSettings.UI_SCALE, 20f * GameSettings.UI_SCALE);
                this.chutes.ForEach(c =>
                {
                    c.templateGUI.materialsWindow = new Rect(this.editorGUI.matX, this.editorGUI.matY, 375f * GameSettings.UI_SCALE, 275f * GameSettings.UI_SCALE);
                    c.templateGUI.drag = new Rect(0f, 0f, 375f * GameSettings.UI_SCALE, 25f * GameSettings.UI_SCALE);
                });
                this.editorGUI.failedWindow = new Rect((Screen.width / 2f) - (150f * GameSettings.UI_SCALE), (Screen.height / 2f) - (150f * GameSettings.UI_SCALE), 300f * GameSettings.UI_SCALE, 300f * GameSettings.UI_SCALE);
                this.editorGUI.successfulWindow = new Rect((Screen.width / 2f) - (150f * GameSettings.UI_SCALE), (Screen.height / 2f) - (25f * GameSettings.UI_SCALE), 300f * GameSettings.UI_SCALE, 50f * GameSettings.UI_SCALE);
                this.editorGUI.presetsWindow = new Rect((Screen.width / 2f) - (200f * GameSettings.UI_SCALE), (Screen.height / 2f) - (250f * GameSettings.UI_SCALE), 400f * GameSettings.UI_SCALE, 500f * GameSettings.UI_SCALE);
                this.editorGUI.presetsSaveWindow = new Rect((Screen.width / 2f) - (175f * GameSettings.UI_SCALE), (Screen.height / 2f) - (110f * GameSettings.UI_SCALE), 350f * GameSettings.UI_SCALE, 220f * GameSettings.UI_SCALE);
                this.editorGUI.presetsWarningWindow = new Rect((Screen.width / 2f) - (100f * GameSettings.UI_SCALE), (Screen.height / 2f) - (50f * GameSettings.UI_SCALE), 200f * GameSettings.UI_SCALE, 100f * GameSettings.UI_SCALE);

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
                    if (!AtmoPlanets.Instance.TryGetBodyIndex("Kerbin", ref this.planets)) { this.planets = 0; }
                    this.body = AtmoPlanets.Instance.GetBody(this.planets);

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
            else if (this.textures != null)
            {
                this.textures.TryGetCase(this.caseId, this.type, ref this.parachuteCase);
                this.lastCaseId = this.caseId;
            }

            if (this.parent == null) { this.parent = this.part.FindModelTransform(this.rcModule.parachutes[0].parachuteName).parent; }

            //Updates the part
            if (this.textures != null)
            {
                UpdateCaseTexture(this.rcModule);
                this.editorGUI.cases = this.textures.CaseNames;
                this.editorGUI.canopies = this.textures.CanopyNames;
                this.editorGUI.models = this.textures.ModelNames;
            }
            UpdateScale(this.part, this.rcModule);
        }

        public override void OnLoad(ConfigNode node)
        {
            if (!CompatibilityChecker.IsAllCompatible) { return; }
            this.node = node;
            LoadChutes();
            if (this.node.HasNode("SIZE"))
            {
                this.sizes = new List<SizeNode>(this.node.GetNodes("SIZE").Select(n => new SizeNode(n)));
                PersistentManager.Instance.AddSizes(this.part.name, this.sizes);
            }

            //Top node original location
            if (this.part.TryGetAttachNodeById("top", out AttachNode a))
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
            return !CompatibilityChecker.IsAllCompatible|| !this.isTweakable || !this.part.Modules.Contains("RealChuteModule") ? string.Empty : "This RealChute part can be tweaked from the Action Groups window.";
        }

        public override void OnSave(ConfigNode node)
        {
            if (!CompatibilityChecker.IsAllCompatible) { return; }
            //Saves the templates to the persistence or craft file
            this.chutes.ForEach(c => node.AddNode(c.Save()));
        }
        #endregion

    }
}