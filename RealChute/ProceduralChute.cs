using System;
using System.Collections.Generic;
using System.Linq;
using RealChute.Extensions;
using UnityEngine;
using RealChute.Libraries;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more information contact me on the forum. */

namespace RealChute
{
    public class ProceduralChute : PartModule
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
        #endregion

        #region Persistent values
        //Selection grid IDs
        [KSPField(isPersistant = true)]
        public int caseID = 0, lastCaseID = 0;
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
        public string timer = string.Empty, cutSpeed = string.Empty, spares = string.Empty;
        #endregion

        #region Fields
        //Libraries
        private EditorActionGroups actionPanel = EditorActionGroups.Instance;
        internal RealChuteModule rcModule = null;
        private AtmoPlanets bodies = null;
        internal MaterialsLibrary materials = MaterialsLibrary.instance;
        private TextureLibrary textureLib = TextureLibrary.instance;
        internal TextureConfig textures = new TextureConfig();
        internal CaseConfig parachuteCase = new CaseConfig();
        internal List<ChuteTemplate> chutes = new List<ChuteTemplate>();
        private PresetsLibrary presets = PresetsLibrary.instance;

        //GUI
        private GUISkin skins = HighLogic.Skin;
        private Rect window = new Rect(), failedWindow = new Rect(), successfulWindow = new Rect();
        private Rect presetsWindow = new Rect(), presetsSaveWindow = new Rect(), presetsWarningWindow = new Rect();
        private int mainId = Guid.NewGuid().GetHashCode(), failedId = Guid.NewGuid().GetHashCode(), successId = Guid.NewGuid().GetHashCode();
        private int presetId = Guid.NewGuid().GetHashCode(), presetSaveId = Guid.NewGuid().GetHashCode(), presetWarningId = Guid.NewGuid().GetHashCode();
        internal int matX = 500, matY = 370;
        private Vector2 mainScroll = new Vector2(), failedScroll = new Vector2();
        private Vector2 presetScroll = new Vector2();
        internal string presetName = string.Empty, presetDescription = string.Empty;
        internal CelestialBody body = null;

        //Sizes
        private SizeManager sizeLib = SizeManager.instance;
        public List<SizeNode> sizes = new List<SizeNode>();
        [SerializeField]
        private Transform parent = null;
        public ConfigNode node = null;

        //GUI fields
        internal bool warning = false;
        private bool visible = false, failedVisible = false, successfulVisible = false;
        private bool presetVisible = false, presetSaveVisible = false, presetWarningVisible = false, saveWarning = false;
        private string[] cases = new string[] { }, canopies = new string[] { }, models = new string[] { };
        #endregion

        #region Methods
        //Gets the strings for the selection grids
        internal string[] TextureEntries(string entries)
        {
            if (textureLibrary == "none") { return new string[] { }; }
            if (entries == "case" && textures.caseNames.Length > 1) { return textures.caseNames.Where(c => textures.GetCase(c).types.Contains(type)).ToArray(); }
            if (entries == "chute" && textures.canopyNames.Length > 1) { return textures.canopyNames; }
            if (entries == "model" && textures.modelNames.Length > 1) { return textures.modelNames.Where(m =>textures.GetModel(m).parameters.Count == this.chutes.Count).ToArray(); }
            return new string[] { };
        }

        //Gets the total mass of the craft
        internal float GetCraftMass(bool dry)
        {
            return EditorLogic.SortedShipList.Where(p => p.physicalSignificance != Part.PhysicalSignificance.NONE).Sum(p => dry ? p.mass : p.TotalMass());
        }

        //Creates a label + text field
        internal void CreateEntryArea(string label, ref string value, float min, float max, float width = 150)
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (RCUtils.CanParse(value) && RCUtils.CheckRange(float.Parse(value), min, max)) { GUILayout.Label(label, skins.label); }
            else { GUILayout.Label(label, RCUtils.redLabel); }
            GUILayout.FlexibleSpace();
            value = GUILayout.TextField(value, 10, skins.textField, GUILayout.Width(width));
            GUILayout.EndHorizontal();
        }

        //Lists the errors of a given type
        private List<string> GetErrors(string type)
        {
            if (type == "general")
            {
                List<string> general = new List<string>();

                if (!RCUtils.CanParseTime(timer) || !RCUtils.CheckRange(RCUtils.ParseTime(timer), 0, 3600)) { general.Add("Deployment timer"); }
                if (!RCUtils.CanParseWithEmpty(spares) || !RCUtils.CheckRange(RCUtils.ParseWithEmpty(spares), -1, 10) || !RCUtils.IsWholeNumber(RCUtils.ParseWithEmpty(spares))) { general.Add("Spare chutes"); }
                if (!RCUtils.CanParse(cutSpeed) || !RCUtils.CheckRange(float.Parse(cutSpeed), 0.01f, 100)) { general.Add("Autocut speed"); }
                return general;
            }
            else if (type == "main" || type == "secondary") { return chutes.SelectMany(c => c.errors).ToList(); }
            return new List<string>();
        }

        //Creates labels for errors.
        private void CreateErrors()
        {
            if (GetErrors("general").Count != 0)
            {
                GUILayout.Label("General:", skins.label);
                foreach(string error in GetErrors("general"))
                {
                    GUILayout.Label(error, RCUtils.redLabel);
                }
                GUILayout.Space(10);
            }

            if (GetErrors("main").Count != 0)
            {
                GUILayout.Label("Main chute:", skins.label);
                foreach(string error in GetErrors("main"))
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
        private void Apply(bool toSymmetryCounterparts)
        {
            if ((GetErrors("general").Count != 0 || GetErrors("main").Count != 0 || (secondaryChute && GetErrors("secondary").Count != 0))) { this.failedVisible = true; return; }
            rcModule.mustGoDown = mustGoDown;
            rcModule.deployOnGround = deployOnGround;
            rcModule.timer = RCUtils.ParseTime(timer);
            rcModule.cutSpeed = float.Parse(cutSpeed);
            rcModule.spareChutes = RCUtils.ParseWithEmpty(spares);

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

            this.successfulVisible = true;
            if (!warning) { successfulWindow.height = 50; }
        }

        //Checks if th given AttachNode has the parent part
        private bool CheckParentNode(AttachNode node)
        {
            return node.attachedPart != null && part.parent != null && node.attachedPart == part.parent;
        }

        //Modifies the size of a part
		private void UpdateScale(Part part, RealChuteModule module)
        {
            if (sizes.Count <= 1) { return; }
            SizeNode size = sizes[this.size], lastSize = sizes[this.lastSize];
            part.transform.GetChild(0).localScale = Vector3.Scale(originalSize, size.size);
            module.caseMass = size.caseMass;

			AttachNode topNode = null, bottomNode = null;
			bool hasTopNode = part.TryGetAttachNodeById("top", out topNode);
			bool hasBottomNode = part.TryGetAttachNodeById("bottom", out bottomNode);

            List<Part> allChildParts = new List<Part>(), allTopChildParts = new List<Part>(), allBottomChildParts = new List<Part>();

			// If this is the root part, move things for the top and the bottom.
            if ((HighLogic.LoadedSceneIsEditor && part == EditorLogic.SortedShipList[0]) || (HighLogic.LoadedSceneIsFlight  && this.vessel.rootPart == part))
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
                        foreach (Part child in allTopChildParts) { child.transform.Translate(0, topDifference, 0, part.transform); }
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
                        foreach (Part child in allBottomChildParts) { child.transform.Translate(0, bottomDifference, 0, part.transform); }
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
                    float topDifference = size.topNode.y - lastSize.topNode.y;
                    if (allChildParts == null) { allChildParts = part.GetAllChildren(); }
                    foreach (Part child in allChildParts) { child.transform.Translate(0, -(bottomDifference - topDifference), 0, part.transform); }
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
                    float bottomDifference = size.bottomNode.y - lastSize.bottomNode.y;
                    if (allChildParts == null) { allChildParts = part.GetAllChildren(); }
                    foreach (Part child in allChildParts) { child.transform.Translate(0, -(topDifference - bottomDifference), 0, part.transform); }
                }
            }

			//Parachute transforms
            float scaleX = part.transform.GetChild(0).localScale.x / Vector3.Scale(originalSize, lastSize.size).x;
            float scaleZ = part.transform.GetChild(0).localScale.z / Vector3.Scale(originalSize, lastSize.size).z;
            foreach (Parachute chute in rcModule.parachutes)
            {
                Vector3 pos = chute.forcePosition - part.transform.position;
                chute.parachute.transform.Translate(pos.x * (scaleX - 1), 0, pos.z * (scaleZ - 1), part.transform);
            }

			//Surface attached parts
			foreach (Part child in part.children)
			{
				if (child.attachMode == AttachModes.SRF_ATTACH)
				{
					Vector3 pos = new Vector3();
					foreach (Part srfAttchdChild in child.GetAllChildren())
					{
						pos = srfAttchdChild.transform.position - part.transform.position;
						srfAttchdChild.transform.Translate(pos.x * (scaleX - 1), 0, pos.z * (scaleZ - 1), part.transform);
					}
					pos = child.transform.position - part.transform.position;
					child.transform.Translate(pos.x * (scaleX - 1), 0, pos.z * (scaleZ - 1), part.transform);
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
        private void ApplyPreset()
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
        private void CreatePreset()
        {
            presets.AddPreset(new Preset(this));
            RealChuteSettings.SaveSettings();
            PopupDialog.SpawnPopupDialog("Preset saved", "The \"" + presetName + "\" preset was succesfully saved!", "Close", false, skins);
            print("[RealChute]: Saved the " + presetName + " preset to the settings file.");
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
                        this.chutes.Add(new ChuteTemplate(this, new ConfigNode(), i));
                    }
                }
            }
        }

        //Copies values from the original symmetry part
        private void CopyFromOriginal(Part p)
        {
            RealChuteModule module = p.Modules["RealChuteModule"] as RealChuteModule;
            ProceduralChute pChute = p.Modules["ProceduralChute"] as ProceduralChute;

            this.mustGoDown = module.mustGoDown;
            this.timer = module.timer.ToString();
            this.cutSpeed = module.cutSpeed.ToString();
            this.spares = module.spareChutes.ToString();
            this.presetID = pChute.presetID;
            this.planets = pChute.planets;
            this.size = pChute.size;
            this.caseID = pChute.caseID;
            this.mustGoDown = pChute.mustGoDown;
            this.deployOnGround = pChute.deployOnGround;
            this.timer = pChute.timer;
            this.cutSpeed = pChute.cutSpeed;
            this.spares = pChute.spares;

            this.chutes.ForEach(c => c.CopyFromOriginal(module, pChute));

            this.part.name = this.part.partInfo.name + "(Clone)";
        }
        #endregion

        #region Functions
        private void Update()
        {
            //Updating of size if possible
            if (!CompatibilityChecker.IsCompatible()) { return; }
            if ((!HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight)) { return; }
            
            if (sizes.Count > 0 && this.part.transform.GetChild(0).localScale != Vector3.Scale(originalSize, sizes[size].size))
            {
                UpdateScale(this.part, rcModule);
            }

            //If unselected
            if (!HighLogic.LoadedSceneIsEditor || !EditorLogic.fetch || EditorLogic.fetch.editorScreen != EditorLogic.EditorScreen.Actions || !this.part.Modules.Contains("RealChuteModule"))
            {
                this.visible = false;
                return;
            }

            //Checks if the part is selected
            if (actionPanel.GetSelectedParts().Contains(this.part))
            {
                this.visible = true;
            }
            else
            {
                this.visible = false;
                chutes.ForEach(c => c.materialsVisible = false);
                this.failedVisible = false;
                this.successfulVisible = false;
            }
            //Checks if size must update
            if (sizes.Count > 0 && lastSize != size) { UpdateScale(this.part, rcModule); }
            //Checks if case texture must update
            if (this.textures.caseNames.Length > 0 && lastCaseID != caseID) { UpdateCaseTexture(this.part, rcModule); }
            chutes.ForEach(c => c.SwitchType());
        }
        #endregion

        #region Overrides
        public override void OnStart(PartModule.StartState state)
        {
            if ((!HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight) || !CompatibilityChecker.IsCompatible()) { return; }

            //Identification of the RealChuteModule
            if (this.part.Modules.Contains("RealChuteModule")) { rcModule = this.part.Modules["RealChuteModule"] as RealChuteModule; }
            else { return; }
            secondaryChute = rcModule.secondaryChute;
            if (textureLibrary != "none") { textureLib.TryGetConfig(textureLibrary, ref textures); }
            bodies = AtmoPlanets.fetch;

            //Initializes ChuteTemplates
            LoadChutes();
            if (this.part.symmetryCounterparts.Count > 0 && this.part.name.Contains("(Clone)(Clone)"))
            {
                print("Symmetry part identified");
                Part p = this.part.symmetryCounterparts.Find(prt => !prt.name.Contains("(Clone)(Clone)"));
                CopyFromOriginal(p);
            }
            chutes.ForEach(c => c.Initialize());
            if (sizes.Count <= 0) { sizes = sizeLib.GetSizes(this.part.partInfo.name); }

            //Creates an instance of the texture library
            if (textureLibrary != "none")
            {
                cases = textures.caseNames;
                canopies = textures.canopyNames;
                models = textures.modelNames;
                textures.TryGetCase(caseID, type, ref parachuteCase);
                lastCaseID = caseID;
            }

            if (HighLogic.LoadedSceneIsEditor)
            {
                //Windows initiation
                this.window = new Rect(5, 370, 420, Screen.height - 375);
                chutes.ForEach(c => c.materialsWindow = new Rect(matX, matY, 375, 280));
                this.failedWindow = new Rect(Screen.width / 2 - 150, Screen.height / 2 - 150, 300, 300);
                this.successfulWindow = new Rect(Screen.width / 2 - 150, Screen.height / 2 - 25, 300, 50);
                this.presetsWindow = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 250, 400, 500);
                this.presetsSaveWindow = new Rect(Screen.width / 2 - 175, Screen.height / 2 - 110, 350, 220);
                this.presetsWarningWindow = new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 200, 100);

                if (!initiated)
                {
                    planets = bodies.GetPlanetIndex("Kerbin");
                    //Gets the original part state
                    if (textureLibrary != "none")
                    {
                        if (textures.TryGetCase(currentCase, ref parachuteCase)) { caseID = textures.GetCaseIndex(parachuteCase); }
                        lastCaseID = caseID;
                    }

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
            if (!CompatibilityChecker.IsCompatible() || !this.part.Modules.Contains("RealChuteModule")) { return; }
            this.node = node;
            LoadChutes();
            if (node.HasNode("SIZE"))
            {
                sizes = new List<SizeNode>(node.GetNodes("SIZE").Select(n => new SizeNode(n)));
                sizeLib.AddSize(this.part.name, sizes);
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
            if (!CompatibilityChecker.IsCompatible()) { return string.Empty; }
            else if (this.part.Modules.Contains("RealChuteModule")) { return "This RealChute part can be tweaked from the Action Groups window."; }
            return string.Empty;
        }

        public override void OnSave(ConfigNode node)
        {
            //Saves the templates to the persistence or craft file
            chutes.ForEach(c => node.AddNode(c.Save()));
        }
        #endregion

        #region GUI
        private void OnGUI()
        {
            //Rendering manager
            if (!CompatibilityChecker.IsCompatible()) { return; }
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (!this.part.Modules.Contains("RealChuteModule")) { return; }              
                if (this.visible)
                {
                    this.window = GUILayout.Window(this.mainId, this.window, Window, "RealChute Parachute Editor " + RCUtils.assemblyVersion, skins.window, GUILayout.MaxWidth(420), GUILayout.MaxHeight(Screen.height - 375));
                }
                foreach (ChuteTemplate chute in chutes)
                {
                    if (chute.materialsVisible)
                    {
                        chute.materialsWindow = GUILayout.Window(chute.matId, chute.materialsWindow, chute.MaterialsWindow, "Parachute material", skins.window, GUILayout.MaxWidth(280), GUILayout.MaxHeight(265));
                    }
                }
                if (this.failedVisible)
                {
                    this.failedWindow = GUILayout.Window(this.failedId, this.failedWindow, ApplicationFailed, "Error", skins.window, GUILayout.MaxWidth(300), GUILayout.MaxHeight(300));
                }
                if (this.successfulVisible)
                {
                    this.successfulWindow = GUILayout.Window(this.successId, this.successfulWindow, ApplicationSucceeded, "Success", skins.window, GUILayout.MaxWidth(300), GUILayout.MaxHeight(200), GUILayout.ExpandHeight(true));
                }
                if (this.presetVisible)
                {
                    this.presetsWindow = GUILayout.Window(this.presetID, this.presetsWindow, Presets, "Presets", skins.window, GUILayout.MaxWidth(400), GUILayout.MaxHeight(500));
                }
                if (this.presetSaveVisible)
                {
                    this.presetsSaveWindow = GUILayout.Window(this.presetSaveId, this.presetsSaveWindow, SavePreset, "Save as preset", skins.window, GUILayout.MaxWidth(350), GUILayout.MaxHeight(400));
                }
                if (this.presetWarningVisible)
                {
                    this.presetsWarningWindow = GUILayout.Window(this.presetWarningId, this.presetsWarningWindow, PresetWarning, "Warning", skins.window, GUILayout.Width(200), GUILayout.Height(100));
                }
            }
        }

        //Main GUI window
        private void Window(int id)
        {
            GUILayout.BeginVertical();

            #region Info labels
            GUILayout.Label("Selected part: " + this.part.partInfo.title, skins.label);
            GUILayout.Label("Symmetry counterparts: " + (this.part.symmetryCounterparts.Count), skins.label);
            GUILayout.Label("Case mass: " + rcModule.caseMass + "t", skins.label);
            GUILayout.Label("Total part mass: " + this.part.TotalMass().ToString("0.###") + "t", skins.label);
            #endregion

            #region Presets
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select a preset", skins.button)) { this.presetVisible = !this.presetVisible; }

            if (GUILayout.Button("Save as preset...", skins.button)) { this.presetSaveVisible = !this.presetSaveVisible; }
            GUILayout.EndHorizontal();
            #endregion

            mainScroll = GUILayout.BeginScrollView(mainScroll, false, false, skins.horizontalScrollbar, skins.verticalScrollbar);

            #region Planet selector
            GUILayout.Space(10);
            GUILayout.BeginHorizontal(GUILayout.Height(30));
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Target planet:", skins.label);
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            planets = GUILayout.SelectionGrid(planets, bodies.GetNames(), 4, skins.button, GUILayout.Width(250));
            GUILayout.EndHorizontal();
            body = bodies.GetBody(planets);
            #endregion

            #region Size cyclers
            if (sizes.Count > 0)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(20));
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Cycle part size", skins.label);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Previous size", skins.button, GUILayout.Width(125)))
                {
                    size--;
                    if (size < 0) { size = sizes.Count - 1; }
                }
                if (GUILayout.Button("Next size", skins.button, GUILayout.Width(125)))
                {
                    size++;
                    if (size > sizes.Count - 1) { size = 0; }
                }
                GUILayout.EndHorizontal();
            }
            #endregion

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            #region Texture selectors
            GUILayout.Space(5);
            chutes[0].TextureSelector();
            #endregion

            #region General
            //Materials editor
            GUILayout.Space(5);
            chutes[0].MaterialsSelector();

            //MustGoDown
            GUILayout.Space(5);
            GUILayout.BeginHorizontal(GUILayout.MaxWidth(window.width));
            GUILayout.Label("Must go down to deploy:", skins.label);
            if (GUILayout.Toggle(mustGoDown, "True", skins.toggle)) { mustGoDown = true; }
            GUILayout.FlexibleSpace();
            if (GUILayout.Toggle(!mustGoDown, "False", skins.toggle)) { mustGoDown = false; }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            //DeployOnGround
            GUILayout.Space(5);
            GUILayout.BeginHorizontal(GUILayout.MaxWidth(window.width));
            GUILayout.Label("Deploy on ground contact:", skins.label);
            if (GUILayout.Toggle(deployOnGround, "True", skins.toggle)) { deployOnGround = true; }
            GUILayout.FlexibleSpace();
            if (GUILayout.Toggle(!deployOnGround, "False", skins.toggle)) { deployOnGround = false; }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            //Timer
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (RCUtils.CanParseTime(timer) && RCUtils.CheckRange(RCUtils.ParseTime(timer), 0, 3600)) { GUILayout.Label("Deployment timer:", skins.label); }
            else { GUILayout.Label("Deployment timer:", RCUtils.redLabel); }
            GUILayout.FlexibleSpace();
            timer = GUILayout.TextField(timer, 10, skins.textField, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            //Spares
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (RCUtils.CanParseWithEmpty(spares) && RCUtils.CheckRange(RCUtils.ParseWithEmpty(spares), -1, 10) && RCUtils.IsWholeNumber(RCUtils.ParseWithEmpty(spares))) { GUILayout.Label("Spare chutes:", skins.label); }
            else { GUILayout.Label("Spare chutes:", RCUtils.redLabel); }
            GUILayout.FlexibleSpace();
            spares = GUILayout.TextField(spares, 10, skins.textField, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            //CutSpeed
            CreateEntryArea("Autocut speed (m/s):", ref cutSpeed, 0.01f, 100);
            #endregion

            #region Main
            //Indicator label
            GUILayout.Space(10);
            GUILayout.Label("________________________________________________", RCUtils.boldLabel);
            GUILayout.Label("Main chute:", RCUtils.boldLabel, GUILayout.Width(150));
            GUILayout.Label("‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾", RCUtils.boldLabel);

            chutes[0].Calculations();
            #endregion

            #region Secondary
            if (secondaryChute)
            {
                for (int i = 1; i < this.chutes.Count; i++)
                {
                    ChuteTemplate chute = this.chutes[i];

                    //Indicator label
                    GUILayout.Space(10);
                    GUILayout.Label("________________________________________________", RCUtils.boldLabel);
                    GUILayout.Label("Chute #" + (i + 1) + " :", RCUtils.boldLabel, GUILayout.Width(150));
                    GUILayout.Label("‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾", RCUtils.boldLabel);

                    #region Texture selectors
                    GUILayout.Space(5);
                    chute.TextureSelector();
                    #endregion

                    //Materials editor
                    GUILayout.Space(5);
                    chute.MaterialsSelector();

                    chute.Calculations();
                }
            }
            #endregion

            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();

            #region Application
            GUILayout.Space(5);
            if (GUILayout.Button("Apply settings", skins.button))
            {
                Apply(false);
            }

            if (part.symmetryCounterparts.Count > 0)
            {
                if (GUILayout.Button("Apply to all symmetry counterparts", skins.button))
                {
                    Apply(true);
                }
            }
            #endregion

            GUILayout.EndVertical();
        }

        //Failure notice
        private void ApplicationFailed(int id)
        {
            GUILayout.Label("Some parameters could not be applied", skins.label);
            GUILayout.Label("Invalid parameters:", skins.label);
            GUILayout.Space(10);
            failedScroll = GUILayout.BeginScrollView(failedScroll, false, false, skins.horizontalScrollbar, skins.verticalScrollbar, skins.box, GUILayout.MaxHeight(200));
            CreateErrors();
            GUILayout.EndScrollView();
            if (GUILayout.Button("Close", skins.button))
            {
                this.failedVisible = false;
            }
        }

        //Success notice
        private void ApplicationSucceeded(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("The application of the parameters succeeded!", skins.label);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (warning)
            {
                GUILayout.Label("Warning: The mass of the craft was too high and the parachutes have been set at their limit. Please review the stats to make sure no problem may occur.", RCUtils.redLabel);
            }

            if (GUILayout.Button("Close", skins.button))
            {
                this.successfulVisible = false;
            }
        }

        //Presets selection window
        private void Presets(int id)
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            presetScroll = GUILayout.BeginScrollView(presetScroll, false, false, skins.horizontalScrollbar, skins.verticalScrollbar, skins.box, GUILayout.Width(200));
            if (presets.GetRelevantPresets(this).Length > 0) { presetID = GUILayout.SelectionGrid(presetID, presets.GetRelevantPresets(this), 1, skins.button); }
            else { GUILayout.Label("No saved presets", skins.label); }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(200));
            if (presets.GetRelevantPresets(this).Length > 0) { GUILayout.Label("Description: " + presets.GetPreset(presets.GetRelevantPresets(this)[presetID]).description, skins.label); }
            else { GUILayout.Label("---", skins.label); }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (presets.presets.Count > 0)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Select preset", skins.button))
                {
                    ApplyPreset();
                    this.presetVisible = false;
                }

                if (GUILayout.Button("Delete preset", skins.button))
                {
                    saveWarning = false;
                    this.presetWarningVisible = true;
                }
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Cancel", skins.button)) { this.presetVisible = false; }
            GUILayout.EndVertical();
        }

        //Presets saving window
        private void SavePreset(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Preset name:", skins.label);
            presetName = GUILayout.TextField(presetName, skins.textField);
            GUILayout.Label("Preset description", skins.label);
            presetDescription = GUILayout.TextArea(presetDescription, skins.textArea, GUILayout.Height(100));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save...", skins.button))
            {
                if (presetName == string.Empty) { PopupDialog.SpawnPopupDialog("Error!", "Preset name cannot be empty!", "Close", false, skins); }
                else if (presets.presetNames.Any(n => n == presetName)) { this.presetWarningVisible = true; saveWarning = true; }
                else if ((GetErrors("general").Count != 0 || GetErrors("main").Count != 0 || (secondaryChute && GetErrors("secondary").Count != 0))) { this.failedVisible = true; }
                else
                {
                    CreatePreset();
                    this.presetSaveVisible = false;
                }
            }
            if (GUILayout.Button("Cancel", skins.button)) { this.presetSaveVisible = false; }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        //Preset saving confirmation window
        private void PresetWarning(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.Label(saveWarning ? "Warning: there is already a preset saved under this name. Are you sure you wish to proceed?" : "Are you sure you wish to delete this preset?", RCUtils.redLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Yes", skins.button))
            {
                Preset preset = saveWarning ? presets.GetPreset(presetName) : presets.GetPreset(presets.GetRelevantPresets(this)[presetID]);
                print("[RealChute]: Deleting the \"" + preset.name + "\" preset from the database.");
                presets.DeletePreset(preset);
                if (saveWarning) { CreatePreset(); this.presetSaveVisible = false; }
                else { presetID = 0; }
                this.presetWarningVisible = false;
            }
            if (GUILayout.Button("No", skins.button)) { this.presetWarningVisible = false; }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        #endregion
    }
}
