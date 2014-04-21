using System;
using System.Collections.Generic;
using System.Linq;
using RealChute.Extensions;
using UnityEngine;

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
        public string currentCanopy = "none";
        [KSPField]
        public string secCurrentCanopy = "none";
        [KSPField]
        public string currentType = "Main";
        [KSPField]
        public string secCurrentType = "Main";
        [KSPField]
        public float modelDiameter = 0;
        [KSPField]
        public float secModelDiameter = 0;
        [KSPField]
        public int modelCount = 1;
        [KSPField]
        public int secModelCount = 1;
        #endregion

        #region Persistent values
        //Selection grid IDs
        [KSPField(isPersistant = true)]
        public int caseID = 0, chuteID = 0, modelID = 0;
        [KSPField(isPersistant = true)]
        public int secChuteID = 0, secModelID = 0;
        [KSPField(isPersistant = true)]
        public int size = 0, planets = 0;
        [KSPField(isPersistant = true)]
        public int lastCaseID = 0;
        [KSPField(isPersistant = true)]
        public int materialsID = 0, secMaterialsID = 0;
        [KSPField(isPersistant = true)]
        public int typeID = 0, secTypeID = 0;
        [KSPField(isPersistant = true)]
        public int lastTypeID = 0, lastSecTypeID = 0;

        //Size vectors
        [KSPField(isPersistant = true)]
        public Vector3 currentSize = new Vector3(), lastSize = new Vector3(), originalSize = new Vector3();
        [KSPField(isPersistant = true)]
        public Vector3 position = Vector3.zero, secPosition = Vector3.zero;

        //Attach nodes
        [KSPField(isPersistant = true)]
        public float top = 0, bottom = 0, debut = 0;

        //Bools
        [KSPField(isPersistant = true)]
        public bool initiated = false;
        [KSPField(isPersistant = true)]
        public bool mustGoDown = false, deployOnGround = false;
        [KSPField(isPersistant = true)]
        public bool isPressure = false, secIsPressure = false;
        [KSPField(isPersistant = true)]
        public bool calcSelect = true, secCalcSelect = true;
        [KSPField(isPersistant = true)]
        public bool getMass = true, secGetMass = true;
        [KSPField(isPersistant = true)]
        public bool useDry = true, secUseDry = true;
        [KSPField(isPersistant = true)]
        public bool secondaryChute = false;

        //GUI strings
        [KSPField(isPersistant = true)]
        public string timer = string.Empty, cutSpeed = string.Empty, spares = string.Empty;
        [KSPField(isPersistant = true)]
        public string preDepDiam = string.Empty, depDiam = string.Empty, predepClause = string.Empty;
        [KSPField(isPersistant = true)]
        public string secPreDepDiam = string.Empty, secDepDiam = string.Empty, secPredepClause = string.Empty;
        [KSPField(isPersistant = true)]
        public string mass = "10", landingSpeed = "6", deceleration = "10", refDepAlt = "700", chuteCount = "1";
        [KSPField(isPersistant = true)]
        public string secMass = "10", secLandingSpeed = "6", secDeceleration = "10", secRefDepAlt = "700", secChuteCount = "1";
        [KSPField(isPersistant = true)]
        public string deploymentAlt = string.Empty, cutAlt = string.Empty, preDepSpeed = string.Empty, depSpeed = string.Empty;
        [KSPField(isPersistant = true)]
        public string secDeploymentAlt = string.Empty, secCutAlt = string.Empty, secPreDepSpeed = string.Empty, secDepSpeed = string.Empty;
        #endregion

        #region Fields
        //Libraries
        private EditorActionGroups actionPanel = EditorActionGroups.Instance;
        internal RealChuteModule rcModule = null;
        private AtmoPlanets bodies = null;
        internal MaterialsLibrary materials = MaterialsLibrary.instance;
        private TextureLibrary textureLib = TextureLibrary.instance;
        internal TextureConfig textures = new TextureConfig();
        private CaseConfig parachuteCase = new CaseConfig();
        internal ChuteTemplate main = new ChuteTemplate(), secondary = new ChuteTemplate();

        //GUI
        private GUISkin skins = HighLogic.Skin;
        private Rect window = new Rect(), failedWindow = new Rect(), successfulWindow = new Rect();
        internal Rect materialsWindow = new Rect(), secMaterialsWindow = new Rect();
        private int mainId = Guid.NewGuid().GetHashCode(), failedId = Guid.NewGuid().GetHashCode(), successId = Guid.NewGuid().GetHashCode();
        private int matId = Guid.NewGuid().GetHashCode(), secMatId = Guid.NewGuid().GetHashCode();
        internal int matX = 500, matY = 370;
        private Vector2 mainScroll = new Vector2(), failedScroll = new Vector2();
        internal CelestialBody body = null;

        //Vectors from the module node
        [SerializeField]
        private List<Vector4> vectors = new List<Vector4>();
        private Dictionary<Vector3, float> sizes = new Dictionary<Vector3, float>();
        [SerializeField]
        private Transform parent = null;

        //GUI fields
        internal bool warning = false;
        private bool visible = false, failedVisible = false, successfulVisible = false;
        internal bool materialsVisible = false, secMaterialsVisible = false;
        private string[] cases = new string[] { }, canopies = new string[] { }, models = new string[] { };
        #endregion

        #region Methods
        //Gets the strings for the selection grids
        internal string[] TextureEntries(string entries)
        {
            if (textureLibrary == "none") { return new string[] { }; }
            if (entries == "case" && textures.caseNames.Length > 1) { return textures.caseNames.Where(c => textures.GetCase(c).types.Contains(type)).ToArray(); }
            if (entries == "chute" && textures.canopyNames.Length > 1) { return textures.canopyNames; }
            if (entries == "modelMain" && textures.modelNames.Length > 1) { return textures.modelNames.Where(m =>textures.GetModel(m).hasMain).ToArray(); }
            if (entries == "modelSec" && textures.modelNames.Length > 1) { return textures.modelNames.Where(m => textures.GetModel(m).hasSecondary).ToArray(); }
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
            else if (type == "main") { return main.errors; }
            else if (type == "secondary") { return secondary.errors; }
            return new List<string>();
        }

        //Creates labels for errors.
        private void CreateErrors()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Invalid parameters:", skins.label);
            GUILayout.Space(10);

            GUILayout.BeginVertical(skins.box);
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

            GUILayout.EndVertical();
            GUILayout.EndVertical();
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

            main.ApplyChanges(toSymmetryCounterparts);
            if (secondaryChute) { secondary.ApplyChanges(toSymmetryCounterparts); }

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
                    pChute.planets = this.planets;
                    pChute.size = this.size;
                    pChute.currentSize = this.currentSize;
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

        //Modifies the size of a part
        private void UpdateScale(Part part, RealChuteModule module)
        {
            if (sizes.Count <= 1) { return; }
            part.transform.GetChild(0).localScale = Vector3.Scale(originalSize, currentSize);
            sizes.TryGetValue(currentSize, out module.caseMass);
            if ((HighLogic.LoadedSceneIsEditor && part == EditorLogic.SortedShipList[0]) || (HighLogic.LoadedSceneIsFlight  && this.vessel.rootPart == part))
            {
                if (part.findAttachNode("top") != null)
                {
                    AttachNode topNode = part.findAttachNode("top");
                    float scale = part.transform.GetChild(0).localScale.y / debut;
                    topNode.position.y = top * scale;
                    if (topNode.attachedPart != null)
                    {
                        float topDifference = topNode.position.y - (top * (Vector3.Scale(originalSize, lastSize).y / debut));
                        topNode.attachedPart.transform.Translate(0, topDifference, 0, part.transform);
                        if (part.findAttachNode("top").attachedPart.GetAllChildren().Count > 0)
                        {
                            topNode.attachedPart.GetAllChildren().ForEach(p => p.transform.Translate(0, topDifference, 0, part.transform));
                        }
                    }
                }
                if (part.findAttachNode("bottom") != null && part.findAttachNode("bottom").attachedPart != null)
                {
                    AttachNode bottomNode = part.findAttachNode("bottom");
                    float scale = part.transform.GetChild(0).localScale.y / debut;
                    bottomNode.position.y = bottom * scale;
                    if (bottomNode.attachedPart != null)
                    {
                        float bottomDifference = bottomNode.position.y - (bottom * (Vector3.Scale(originalSize, lastSize).y / debut));
                        bottomNode.attachedPart.transform.Translate(0, bottomDifference, 0, part.transform);
                        if (part.findAttachNode("bottom").attachedPart.GetAllChildren().Count > 0)
                        {
                            bottomNode.attachedPart.GetAllChildren().ForEach(p => p.transform.Translate(0, bottomDifference, 0, part.transform));
                        }
                    }
                }
            }
            else if (part.findAttachNode("bottom") != null && part.findAttachNode("bottom").attachedPart != null && part.parent != null &&  part.findAttachNode("bottom").attachedPart == part.parent)
            {
                AttachNode bottomNode = part.findAttachNode("bottom");
                float scale = part.transform.GetChild(0).localScale.y / debut;
                bottomNode.position.y = bottom * scale;
                float bottomDifference = bottomNode.position.y - (bottom * (Vector3.Scale(originalSize, lastSize).y / debut));
                part.transform.Translate(0, -bottomDifference, 0, part.transform);
                if (part.findAttachNode("top") != null)
                {
                    AttachNode topNode = part.findAttachNode("top");
                    topNode.position.y = top * scale;
                    float topDifference = topNode.position.y - (top * (Vector3.Scale(originalSize, lastSize).y / debut));
                    if (part.GetAllChildren().Count > 0) { part.GetAllChildren().ForEach(p => p.transform.Translate(0, -(bottomDifference - topDifference), 0, part.transform)); }
                }
            }
            else if (part.findAttachNode("top") != null && part.findAttachNode("top").attachedPart != null && part.parent != null && part.findAttachNode("top").attachedPart == part.parent)
            {
                AttachNode topNode = part.findAttachNode("top");
                float scale = part.transform.GetChild(0).localScale.y / debut;
                topNode.position.y = top * scale;
                float topDifference = topNode.position.y - (top * (Vector3.Scale(originalSize, lastSize).y / debut));
                part.transform.Translate(0, -topDifference, 0, part.transform);
                if (part.findAttachNode("bottom") != null)
                {
                    AttachNode bottomNode = part.findAttachNode("bottom");
                    bottomNode.position.y = bottom * scale;
                    float bottomDifference = bottomNode.position.y - (bottom * (Vector3.Scale(originalSize, lastSize).y / debut));
                    if (part.GetAllChildren().Count > 0) { part.GetAllChildren().ForEach(p => p.transform.Translate(0, -(topDifference - bottomDifference), 0, part.transform)); }
                }
            }

            float scaleX = (part.transform.GetChild(0).localScale.x / debut) / (Vector3.Scale(originalSize, lastSize).x / debut);
            float scaleZ = (part.transform.GetChild(0).localScale.z / debut) / (Vector3.Scale(originalSize, lastSize).z / debut);
            Vector3 chute = part.FindModelTransform(module.parachuteName).transform.position - part.transform.position;
            part.FindModelTransform(module.parachuteName).transform.Translate(chute.x * (scaleX - 1), 0, chute.z * (scaleZ - 1), part.transform);
            if (secondaryChute)
            {
                Vector3 secChute = part.FindModelTransform(module.secParachuteName).transform.position - part.transform.position;
                part.FindModelTransform(module.secParachuteName).transform.Translate(secChute.x * (scaleX - 1), 0, secChute.z * (scaleZ - 1), part.transform);
            }

            if (part.children.Count(p => p.attachMode == AttachModes.SRF_ATTACH) > 0)
            {
                List<Part> surfaceAttached = new List<Part>(part.children.Where(p => p.attachMode == AttachModes.SRF_ATTACH));
                surfaceAttached.Where(p => p.GetAllChildren().Count > 0).ToList().ForEach(p => surfaceAttached.AddRange(p.GetAllChildren()));
                foreach (Part p in surfaceAttached)
                {
                    Vector3 v = p.transform.position - part.transform.position;
                    p.transform.Translate(v.x * (scaleX - 1), 0, v.z * (scaleZ - 1), part.transform);
                }
            }
            lastSize = currentSize;
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
        #endregion

        #region Functions
        private void Update()
        {
            //Updating of size if possible
            if (!CompatibilityChecker.IsCompatible()) { return; }
            if ((!HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight) || ((this.part.Modules["RealChuteModule"] != null && !((RealChuteModule)this.part.Modules["RealChuteModule"]).isTweakable))) { return; }
            
            if (this.part.transform.GetChild(0).localScale != Vector3.Scale(originalSize, currentSize))
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
                if (sizes.Count > 0)
                {
                    currentSize = sizes.Keys.ToArray()[size];
                }
            }
            else
            {
                this.visible = false;
                this.materialsVisible = false;
                this.secMaterialsVisible = false;
                this.failedVisible = false;
                this.successfulVisible = false;
            }

            //Checks if size must update
            if (lastSize != currentSize)
            {
                UpdateScale(this.part, rcModule);
            }

            //Checks if case texture must update
            if (lastCaseID != caseID)
            {
                UpdateCaseTexture(this.part, rcModule);
            }

            main.SwitchType();
            if (secondaryChute) { secondary.SwitchType(); }
        }
        #endregion

        #region Overrides
        public override void OnStart(PartModule.StartState state)
        {
            if ((!HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight) || !CompatibilityChecker.IsCompatible()) { return; }

            //Identification of the RealChuteModule
            if (this.part.Modules.Contains("RealChuteModule")) { rcModule = this.part.Modules["RealChuteModule"] as RealChuteModule; }
            else { return; }
            if (!rcModule.isTweakable) { return; }
            secondaryChute = rcModule.secondaryChute;
            if (textureLibrary != "none") { textureLib.TryGetConfig(textureLibrary, ref textures); }
            bodies = AtmoPlanets.fetch;
            main = new ChuteTemplate(this, false);
            if (secondaryChute) { secondary = new ChuteTemplate(this, true); }

            //Creation of the sizes dictionary
            if (vectors.Count > 1)
            {
                sizes = vectors.ToDictionary(v => new Vector3(v.x, v.y, v.z), v => v.w);
                currentSize = sizes.Keys.ToArray()[size];
                originalSize = this.part.transform.localScale;
            }

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
                this.materialsWindow = new Rect(matX, matY, 375, 280);
                this.secMaterialsWindow = new Rect(matX, matY, 375, 280);
                this.failedWindow = new Rect(Screen.width / 2 - 150, Screen.height / 2 - 150, 300, 300);
                this.successfulWindow = new Rect(Screen.width / 2 - 150, Screen.height / 2 - 25, 300, 50);

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
                    originalSize = this.part.transform.localScale;
                    initiated = true;
                }
            }

            if (parent == null) { parent = this.part.FindModelTransform(rcModule.parachuteName).parent; }      

            //Updates the part
            if (textureLibrary != "none")
            {
                UpdateCaseTexture(this.part, rcModule);
            }
            UpdateScale(this.part, rcModule);
        }

        public override void OnLoad(ConfigNode node)
        {
            if (!CompatibilityChecker.IsCompatible()) { return; }
            if ((HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight) && this.part.Modules.Contains("RealChuteModule") && !((RealChuteModule)this.part.Modules["RealChuteModule"]).isTweakable) { return; }
            
            //Size vectors
            if (node.GetValues("size").Length > 0 && vectors.Count <= 0) { vectors = node.GetValues("size").Select(v => KSPUtil.ParseVector4(v)).ToList(); }

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
            if (!CompatibilityChecker.IsCompatible() || (!HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight)) { return string.Empty; }
            else if (this.part.Modules.Contains("RealChuteModule") && ((RealChuteModule)this.part.Modules["RealChuteModule"]).isTweakable) { return "This RealChute part can be tweaked from the Action Groups window."; }
            return string.Empty;
        }
        #endregion

        #region GUI
        private void OnGUI()
        {
            //Rendering manager
            if (!CompatibilityChecker.IsCompatible()) { return; }
            if (HighLogic.LoadedSceneIsEditor)
            {
                if ((this.part.Modules["RealChuteModule"] != null && !((RealChuteModule)this.part.Modules["RealChuteModule"]).isTweakable)) { return; }
                
                if (this.visible)
                {
                    this.window = GUILayout.Window(this.mainId, this.window, Window, "RealChute Parachute Editor " + RCUtils.assemblyVersion, skins.window, GUILayout.MaxWidth(420), GUILayout.MaxHeight(Screen.height - 375));
                }

                if (this.materialsVisible)
                {
                    this.materialsWindow = GUILayout.Window(this.matId, this.materialsWindow, Materials, "Main parachute material", skins.window, GUILayout.MaxWidth(280), GUILayout.MaxHeight(265));
                }

                if (this.secMaterialsVisible)
                {
                    this.secMaterialsWindow = GUILayout.Window(this.secMatId, this.secMaterialsWindow, SecMaterials, "Secondary parachute material", skins.window, GUILayout.MaxWidth(280), GUILayout.MaxHeight(265));
                }

                if (this.failedVisible)
                {
                    this.failedWindow = GUILayout.Window(this.failedId, this.failedWindow, ApplicationFailed, "Error", skins.window, GUILayout.MaxWidth(300), GUILayout.MaxHeight(300));
                }

                if (this.successfulVisible)
                {
                    this.successfulWindow = GUILayout.Window(this.successId, this.successfulWindow, ApplicationSucceeded, "Success", skins.window, GUILayout.MaxWidth(300), GUILayout.MaxHeight(200), GUILayout.ExpandHeight(true));
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
            GUILayout.Label("Part mass: " + this.part.TotalMass().ToString("0.###"), skins.label);
            #endregion

            #region Planet selector
            GUILayout.Space(5);
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
            mainScroll = GUILayout.BeginScrollView(mainScroll, false, false, skins.horizontalScrollbar, skins.verticalScrollbar);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            #region Texture selectors
            GUILayout.Space(5);
            main.TextureSelector();
            #endregion

            #region General
            //Materials editor
            GUILayout.Space(5);
            main.MaterialsSelector();

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

            main.Calculations();
            #endregion

            #region Secondary
            if (secondaryChute)
            {
                //Indicator label
                GUILayout.Space(10);
                GUILayout.Label("________________________________________________", RCUtils.boldLabel);
                GUILayout.Label("Secondary chute:", RCUtils.boldLabel, GUILayout.Width(150));
                GUILayout.Label("‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾", RCUtils.boldLabel);

                #region Texture selectors
                GUILayout.Space(5);
                secondary.TextureSelector();
                #endregion

                //Materials editor
                GUILayout.Space(5);
                secondary.MaterialsSelector();

                secondary.Calculations();
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

        //Materials window of themain parachute
        private void Materials(int id)
        {
            main.MaterialsWindow();
        }

        //Materials window of the second parachute
        private void SecMaterials(int id)
        {
            secondary.MaterialsWindow();
        }

        //Failure notice
        private void ApplicationFailed(int id)
        {
            GUILayout.Label("Some parameters could not be applied", skins.label);

            failedScroll = GUILayout.BeginScrollView(failedScroll, false, false, skins.horizontalScrollbar, skins.verticalScrollbar, GUILayout.MaxHeight(200));
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
        #endregion
    }
}
