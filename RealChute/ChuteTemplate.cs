using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RealChute.Extensions;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more information contact me on the forum. */

namespace RealChute
{
    public class ChuteTemplate
    {
        #region Propreties
        private Parachute chute
        {
            get { return this.secondary ? this.pChute.rcModule.secondary : this.pChute.rcModule.main; }
        }
        private Part part
        {
            get { return this.pChute.part; }
        }
        private CelestialBody body
        {
            get { return this.pChute.body; }
        }
        private ModelConfig.ParachuteParameters parameters
        {
            get { return this.secondary ? model.secondary : model.main; }
        }
        private ChuteTemplate sec
        {
            get { return this.secondary ? this.pChute.main : this.pChute.secondary; }
        }
        public string currentCanopy
        {
            get { return this.secondary ? this.pChute.secCurrentCanopy : this.pChute.currentCanopy; }
        }
        public string currentType
        {
            get { return this.secondary ? this.pChute.secCurrentType : this.pChute.currentType; }
        }
        public float modelDiameter
        {
            get { return this.secondary ? this.pChute.secModelDiameter : this.pChute.modelDiameter; }
        }
        public int modelCount
        {
            get { return this.secondary ? this.pChute.secModelCount : this.pChute.modelCount; }
        }
        public int chuteID
        {
            get { return this.secondary ? this.pChute.secChuteID : this.pChute.chuteID; }
            set
            {
                if (secondary) { this.pChute.secChuteID = value; }
                else { this.pChute.chuteID = value; }
            }
        }
        public int modelID
        {
            get { return this.secondary ? this.pChute.secModelID : this.pChute.modelID; }
            set
            {
                if (secondary) { this.pChute.secModelID = value; }
                else { this.pChute.modelID = value; }
            }
        }
        public int materialsID
        {
            get { return this.secondary ? this.pChute.secMaterialsID : this.pChute.materialsID; }
            set
            {
                if (secondary) { this.pChute.secMaterialsID = value; }
                else { this.pChute.materialsID = value; }
            }
        }
        public int typeID
        {
            get { return this.secondary ? this.pChute.secTypeID : this.pChute.typeID; }
            set
            {
                if (secondary) { this.pChute.secTypeID = value; }
                else { this.pChute.typeID = value; }
            }
        }
        public int lastTypeID
        {
            get { return this.secondary ? this.pChute.lastSecTypeID : this.pChute.lastTypeID; }
            set
            {
                if (secondary) { this.pChute.lastSecTypeID = value; }
                else { this.pChute.lastTypeID = value; }
            }
        }
        public Vector3 position
        {
            get { return this.secondary ? this.pChute.secPosition : this.pChute.position; }
            set
            {
                if (secondary) { this.pChute.secPosition = value; }
                else { this.pChute.position = value; }
            }
        }
        public bool isPressure
        {
            get { return this.secondary ? this.pChute.secIsPressure : this.pChute.isPressure; }
            set
            {
                if (secondary) { this.pChute.secIsPressure = value; }
                else { this.pChute.isPressure = value; }
            }
        }
        public bool calcSelect
        {
            get { return this.secondary ? this.pChute.secCalcSelect : this.pChute.calcSelect; }
            set
            {
                if (secondary) { this.pChute.secCalcSelect = value; }
                else { this.pChute.calcSelect = value; }
            }
        }
        public bool getMass
        {
            get { return this.secondary ? this.pChute.secGetMass : this.pChute.getMass; }
            set
            {
                if (secondary) { this.pChute.secGetMass = value; }
                else { this.pChute.getMass = value; }
            }
        }
        public bool useDry
        {
            get { return this.secondary ? this.pChute.secUseDry : this.pChute.useDry; }
            set
            {
                if (secondary) { this.pChute.secUseDry = value; }
                else { this.pChute.useDry = value; }
            }
        }
        public string preDepDiam
        {
            get { return this.secondary ? this.pChute.secPreDepDiam : this.pChute.preDepDiam; }
            set
            {
                if (secondary) { this.pChute.secPreDepDiam = value; }
                else { this.pChute.preDepDiam = value; }
            }
        }
        public string depDiam
        {
            get { return this.secondary ? this.pChute.secDepDiam : this.pChute.depDiam; }
            set
            {
                if (secondary) { this.pChute.secDepDiam = value; }
                else { this.pChute.depDiam = value; }
            }
        }
        public string predepClause
        {
            get { return this.secondary ? this.pChute.secPredepClause : this.pChute.predepClause; }
            set
            {
                if (secondary) { this.pChute.secPredepClause = value; }
                else { this.pChute.predepClause = value; }
            }
        }
        public string mass
        {
            get { return this.secondary ? this.pChute.secMass : this.pChute.mass; }
            set
            {
                if (secondary) { this.pChute.secMass = value; }
                else { this.pChute.mass = value; }
            }
        }
        public string landingSpeed
        {
            get { return this.secondary ? this.pChute.secLandingSpeed : this.pChute.landingSpeed; }
            set
            {
                if (secondary) { this.pChute.secLandingSpeed = value; }
                else { this.pChute.landingSpeed = value; }
            }
        }
        public string deceleration
        {
            get { return this.secondary ? this.pChute.secDeceleration : this.pChute.deceleration; }
            set
            {
                if (secondary) { this.pChute.secDeceleration = value; }
                else { this.pChute.deceleration = value; }
            }
        }
        public string refDepAlt
        {
            get { return this.secondary ? this.pChute.secRefDepAlt : this.pChute.refDepAlt; }
            set
            {
                if (secondary) { this.pChute.secRefDepAlt = value; }
                else { this.pChute.refDepAlt = value; }
            }
        }
        public string chuteCount
        {
            get { return this.secondary ? this.pChute.secChuteCount : this.pChute.chuteCount; }
            set
            {
                if (secondary) { this.pChute.secChuteCount = value; }
                else { this.pChute.chuteCount = value; }
            }
        }
        public string deploymentAlt
        {
            get { return this.secondary ? this.pChute.secDeploymentAlt : this.pChute.deploymentAlt; }
            set
            {
                if (secondary) { this.pChute.secDeploymentAlt = value; }
                else { this.pChute.deploymentAlt = value; }
            }
        }
        public string cutAlt
        {
            get { return this.secondary ? this.pChute.secCutAlt : this.pChute.cutAlt; }
            set
            {
                if (secondary) { this.pChute.secCutAlt = value; }
                else { this.pChute.cutAlt = value; }
            }
        }
        public string preDepSpeed
        {
            get { return this.secondary ? this.pChute.secPreDepSpeed : this.pChute.preDepSpeed; }
            set
            {
                if (secondary) { this.pChute.secPreDepSpeed = value; }
                else { this.pChute.preDepSpeed = value; }
            }
        }
        public string depSpeed
        {
            get { return this.secondary ? this.pChute.secDepSpeed : this.pChute.depSpeed; }
            set
            {
                if (secondary) { this.pChute.secDepSpeed = value; }
                else { this.pChute.depSpeed = value; }
            }
        }
        public bool materialsVisible
        {
            get { return this.secondary ? this.pChute.secMaterialsVisible : this.pChute.materialsVisible; }
            set
            {
                if (this.secondary) { this.pChute.secMaterialsVisible = value; }
                else { this.pChute.materialsVisible = value; }
            }
        }
        public Rect materialsWindow
        {
            get { return this.secondary ? this.pChute.secMaterialsWindow : this.pChute.materialsWindow; }
            set
            {
                if (secondary) { this.pChute.secMaterialsWindow = value; }
                else { this.pChute.materialsWindow = value; }
            }
        }
        public List<string> errors
        {
            get
            {
                List<string> main = new List<string>();
                if (calcSelect)
                {
                    if (!getMass && !RCUtils.CanParse(mass) || !RCUtils.CheckRange(float.Parse(mass), 0.1f, 10000)) { main.Add("Craft mass"); }
                    if (!RCUtils.CanParse(landingSpeed) || ((typeID == 1 && !RCUtils.CheckRange(float.Parse(landingSpeed), 0.1f, 5000)) || (typeID != 1 && !RCUtils.CheckRange(float.Parse(landingSpeed), 0.1f, 300)))) { main.Add("Landing speed"); }
                    if (typeID == 2 && !RCUtils.CanParse(deceleration) || !RCUtils.CheckRange(float.Parse(deceleration), 0.1f, 100)) { main.Add("Wanted deceleration"); }
                    if (typeID == 1 && !RCUtils.CanParse(refDepAlt) || !RCUtils.CheckRange(float.Parse(refDepAlt), 10, RCUtils.GetMaxAtmosphereAltitude(body))) { main.Add("Mains planned deployment alt"); }
                    if (!RCUtils.CanParse(chuteCount) || !RCUtils.CheckRange(float.Parse(chuteCount), 1, 100)) { main.Add("Parachute count"); }
                }
                else
                {
                    if (!RCUtils.CanParse(preDepDiam) || !RCUtils.CheckRange(float.Parse(preDepDiam), 0.5f, model.maxDiam / 2)) { main.Add("Predeployed diameter"); }
                    if (!RCUtils.CanParse(depDiam) || !RCUtils.CheckRange(float.Parse(depDiam), 1, model.maxDiam)) { main.Add("Deployed diameter"); }
                }
                if (!RCUtils.CanParse(predepClause) || (isPressure && !RCUtils.CheckRange(float.Parse(predepClause), 0.0001f, (float)FlightGlobals.getStaticPressure(0, body))) || (!isPressure && !RCUtils.CheckRange(float.Parse(predepClause), 10, RCUtils.GetMaxAtmosphereAltitude(body))))
                {
                    if (isPressure) { main.Add("Predeployment pressure"); }
                    else { main.Add("Predeployment altitude"); }
                }
                if (!RCUtils.CanParse(deploymentAlt) || !RCUtils.CheckRange(float.Parse(deploymentAlt), 10, RCUtils.GetMaxAtmosphereAltitude(body))) { main.Add("Deployment altitude"); }
                if (!RCUtils.CanParseWithEmpty(cutAlt) || !RCUtils.CheckRange(RCUtils.ParseWithEmpty(cutAlt), -1, RCUtils.GetMaxAtmosphereAltitude(body))) { main.Add("Autocut altitude"); }
                if (!RCUtils.CanParse(preDepSpeed) || !RCUtils.CheckRange(float.Parse(preDepSpeed), 0.5f, 5)) { main.Add("Predeployment speed"); }
                if (!RCUtils.CanParse(depSpeed) || !RCUtils.CheckRange(float.Parse(depSpeed), 1, 10)) { main.Add("Deployment speed"); }
                return main;
            }
        }
        #endregion

        #region Fields
        internal ProceduralChute pChute = null;
        internal bool secondary = false;
        internal MaterialDefinition material = new MaterialDefinition();
        internal CanopyConfig canopy = new CanopyConfig();
        internal ModelConfig model = new ModelConfig();
        internal Vector2 parachuteScroll = new Vector2(), materialsScroll = new Vector2();
        private GUISkin skins = HighLogic.Skin;
        #endregion

        #region Methods
        internal void ApplyChanges(bool toSymmetryCounterparts)
        {
            chute.material = material.name;
            chute.mat = material;

            if (calcSelect)
            {
                float m = 0;
                if (getMass) { m = this.pChute.GetCraftMass(useDry); }
                else { m = float.Parse(mass); }

                float density = 0;
                if (typeID == 1) { density = RCUtils.GetDensityAtAlt(body, float.Parse(refDepAlt)); }
                else { density = RCUtils.GetDensityAtAlt(body, 0f); }

                float speed2 = Mathf.Pow(float.Parse(landingSpeed), 2f);

                float acc = 0;
                if (typeID == 2) { acc = float.Parse(deceleration); }
                else { acc = (float)(body.GeeASL * RCUtils.geeToAcc); }
                Debug.Log(String.Concat("[RealChute]: ", this.part.partInfo.title, " MAIN - m: ", m, "t, rho: ", density, "kg/m³, v²: ", speed2, "m²/s², acceleration: ", acc, "m/s²"));

                chute.deployedDiameter = RCUtils.Round(Mathf.Sqrt((8000f * m * acc) / (Mathf.PI * speed2 * material.dragCoefficient * density * float.Parse(chuteCount))));
                if ((this.pChute.textureLibrary != "none" || this.pChute.textures.modelNames.Length > 0) && chute.deployedDiameter > model.maxDiam)
                {
                    chute.deployedDiameter = model.maxDiam;
                    this.pChute.warning = true;
                }
                else if ((this.pChute.textureLibrary == "none" || this.pChute.textures.modelNames.Length <= 0) && chute.deployedDiameter > 70f)
                {
                    chute.deployedDiameter = 70f;
                    this.pChute.warning = true;
                }
                else { this.pChute.warning = false; }
                if (typeID == 0) { chute.preDeployedDiameter = RCUtils.Round(chute.deployedDiameter / 20); }
                else { chute.preDeployedDiameter = RCUtils.Round(chute.deployedDiameter / 2); }
                Debug.Log(String.Concat("[RealChute]: ", this.part.partInfo.title, " MAIN - depDiam: ", chute.deployedDiameter, "m, preDepDiam: ", chute.preDeployedDiameter, "m"));
            }

            else
            {
                chute.preDeployedDiameter = RCUtils.Round(float.Parse(preDepDiam));
                chute.deployedDiameter = RCUtils.Round(float.Parse(depDiam));
            }

            chute.minIsPressure = isPressure;
            if (isPressure) { chute.minPressure = float.Parse(predepClause); }
            else { chute.minDeployment = float.Parse(predepClause); }
            chute.deploymentAlt = float.Parse(deploymentAlt);
            chute.cutAlt = RCUtils.ParseWithEmpty(cutAlt);
            chute.preDeploymentSpeed = float.Parse(preDepSpeed);
            chute.deploymentSpeed = float.Parse(depSpeed);

            if (toSymmetryCounterparts)
            {
                foreach (Part part in this.part.symmetryCounterparts)
                {
                    Parachute sym = secondary ? ((RealChuteModule)part.Modules["RealChuteModule"]).secondary : ((RealChuteModule)part.Modules["RealChuteModule"]).main;
                    sym.material = material.name;
                    sym.mat = material;
                    sym.deployedDiameter = chute.deployedDiameter;
                    sym.preDeployedDiameter = chute.preDeployedDiameter;

                    sym.minIsPressure = isPressure;
                    if (isPressure) { sym.minPressure = chute.minPressure; }
                    else { sym.minDeployment = chute.minDeployment; }
                    sym.deploymentAlt = chute.deploymentAlt;
                    sym.cutAlt = chute.cutAlt;
                    sym.preDeploymentSpeed = chute.preDeploymentSpeed;
                    sym.deploymentSpeed = chute.deploymentSpeed;

                    ChuteTemplate template = secondary ? ((ProceduralChute)part.Modules["ProceduralChute"]).secondary : ((ProceduralChute)part.Modules["ProceduralChute"]).main;
                    template.chuteID = this.chuteID;
                    template.typeID = this.typeID;
                    template.modelID = this.modelID;
                    template.materialsID = this.materialsID;
                    template.isPressure = this.isPressure;
                    template.calcSelect = this.calcSelect;
                    template.getMass = this.getMass;
                    template.useDry = this.useDry;
                    template.preDepDiam = this.preDepDiam;
                    template.depDiam = this.depDiam;
                    template.predepClause = this.predepClause;
                    template.mass = this.mass;
                    template.landingSpeed = this.landingSpeed;
                    template.deceleration = this.deceleration;
                    template.refDepAlt = this.refDepAlt;
                    template.chuteCount = this.chuteCount;
                    template.deploymentAlt = this.deploymentAlt;
                    template.cutAlt = this.cutAlt;
                    template.preDepSpeed = this.preDepSpeed;
                    template.depSpeed = this.depSpeed;
                }
            }
        }

        internal void UpdateCanopyTexture()
        {
            if (this.pChute.textureLibrary == "none") { return; }
            if (this.pChute.textures.TryGetCanopy(chuteID, ref canopy))
            {
                if (string.IsNullOrEmpty(canopy.textureURL))
                {
                    Debug.LogWarning("[RealChute]: The " + this.pChute.textures.canopyNames[chuteID] + "URL is empty");
                    return;
                }
                Texture2D texture = GameDatabase.Instance.GetTexture(canopy.textureURL, false);
                if (texture == null)
                {
                    Debug.LogWarning("[RealChute]: The " + this.pChute.textures.canopyNames[chuteID] + "texture is null");
                    return;
                }
                chute.parachute.GetComponents<Renderer>().ToList().ForEach(r => r.material.mainTexture = texture);
            }
        }

        internal void UpdateCanopy()
        {
            if (this.pChute.textureLibrary != "none")
            {
                if (this.pChute.textures.TryGetModel(this.pChute.textures.modelNames[modelID], ref model))
                {
                    if (string.IsNullOrEmpty(parameters.modelURL))
                    {
                        Debug.LogWarning("[RealChute]: The " + this.pChute.textures.modelNames[modelID] + (secondary ? "main" : "secondary") + "URL is empty");
                        return;
                    }
                    GameObject test = GameDatabase.Instance.GetModel(parameters.modelURL);
                    if (test == null)
                    {
                        Debug.LogWarning("[RealChute]: The " + this.pChute.textures.modelNames[modelID] + (secondary ? "main" : "secondary") + "GameObject is null");
                        return;
                    }
                    test.SetActive(true);
                    if (!secondary)
                    {
                        float scale = RCUtils.GetDiameter(chute.deployedArea / model.count) / model.diameter;
                        test.transform.localScale = new Vector3(scale, scale, scale);
                        Debug.Log("[RealChute]: " + part.partInfo.title + " Scale: " + scale);
                    }
                    else
                    {
                        float scale = RCUtils.GetDiameter(chute.deployedArea / model.count) / model.diameter;
                        test.transform.localScale = new Vector3(scale, scale, scale);
                        Debug.Log("[RealChute]: " + part.partInfo.title + " Secondary scale: " + scale);
                    }

                    GameObject obj = GameObject.Instantiate(test) as GameObject;
                    GameObject.Destroy(test);
                    Transform toDestroy;
                    toDestroy = part.FindModelTransform(chute.parachuteName);
                    obj.transform.parent = toDestroy.parent;
                    obj.transform.position = toDestroy.position;
                    GameObject.DestroyImmediate(toDestroy.gameObject);
                    chute.parachute = part.FindModelTransform(parameters.transformName);
                    chute.parachute.transform.localRotation = Quaternion.identity;
                    chute.parachuteName = parameters.transformName;
                    chute.deploymentAnimation = parameters.depAnim;
                    chute.preDeploymentAnimation = parameters.preDepAnim;
                    part.InitiateAnimation(chute.deploymentAnimation);
                    part.InitiateAnimation(chute.preDeploymentAnimation);
                    chute.parachute.gameObject.SetActive(false);
                }
                UpdateCanopyTexture();
            }
        }

        internal void SwitchType()
        {
            if (this.pChute.secondaryChute)
            {
                if (this.materialsVisible && !this.sec.materialsVisible)
                {
                    this.pChute.matX = (int)materialsWindow.x;
                    this.pChute.matY = (int)materialsWindow.y;
                    Rect window = this.sec.materialsWindow;
                    window.x = this.pChute.matX;
                    window.y = this.pChute.matY;
                    this.sec.materialsWindow = window;
                }
            }

            if (lastTypeID != typeID)
            {
                switch (typeID)
                {
                    case 0:
                        {
                            this.pChute.deployOnGround = false;
                            this.pChute.cutSpeed = "0.5";
                            landingSpeed = "6";
                            deploymentAlt = "700";
                            predepClause = isPressure ? "0.01" : "25000";
                            preDepSpeed = "2";
                            depSpeed = "6";
                            break;
                        }

                    case 1:
                        {
                            this.pChute.deployOnGround = false;
                            this.pChute.cutSpeed = "0.5";
                            landingSpeed = "80";
                            deploymentAlt = "2500";
                            predepClause = isPressure ? "0.007" : "30000";
                            preDepSpeed = "1";
                            depSpeed = "3";
                            break;
                        }

                    case 2:
                        {
                            this.pChute.deployOnGround = true;
                            this.pChute.cutSpeed = "5";
                            landingSpeed = "100";
                            deploymentAlt = "10";
                            predepClause = isPressure ? "0.9" : "100";
                            preDepSpeed = "1";
                            depSpeed = "2";
                            break;
                        }
                    default:
                        break;
                }
                lastTypeID = typeID;
            }
        }

        internal void TextureSelector()
        {
            string model = "model" + (secondary ? "Main" : "Sec");
            if ((!secondary && this.pChute.TextureEntries("case").Length > 1) || this.pChute.TextureEntries("chute").Length > 1 || this.pChute.TextureEntries(model).Length > 1)
            {
                int m = 0;
                if (!secondary && this.pChute.TextureEntries("case").Length > 1) { m++; }
                if (this.pChute.TextureEntries("chute").Length > 1) { m++; }
                if (this.pChute.TextureEntries(model).Length > 1) { m++; }

                GUILayout.BeginHorizontal();

                #region Labels
                GUILayout.BeginVertical(GUILayout.Height(35 * m));

                //Labels
                if (!secondary && this.pChute.TextureEntries("case").Length > 1)
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Case texture:", skins.label);
                }
                if (this.pChute.TextureEntries("chute").Length > 1)
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Chute texture:", skins.label);
                }
                if (this.pChute.TextureEntries(model).Length > 1)
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Chute model: ", skins.label);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                #endregion

                #region Selectors
                GUILayout.BeginVertical(skins.box, GUILayout.Height(35 * m));
                //Boxes
                if (!secondary && this.pChute.TextureEntries("case").Length > 1)
                {
                    GUILayout.FlexibleSpace();
                    this.pChute.caseID = GUILayout.SelectionGrid(this.pChute.caseID, this.pChute.TextureEntries("case"), this.pChute.TextureEntries("case").Length, skins.button);
                }

                if (this.pChute.TextureEntries("chute").Length > 1)
                {
                    GUILayout.FlexibleSpace();
                    chuteID = GUILayout.SelectionGrid(chuteID, this.pChute.TextureEntries("chute"), this.pChute.TextureEntries("chute").Length, skins.button);
                }

                if (this.pChute.TextureEntries(model).Length > 1)
                {
                    GUILayout.FlexibleSpace();
                    modelID = GUILayout.SelectionGrid(modelID, this.pChute.TextureEntries(model), this.pChute.TextureEntries(model).Length, skins.button);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                #endregion

                GUILayout.EndHorizontal();
            }
        }

        internal void MaterialsSelector()
        {
            if (this.pChute.materials.count > 1)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(20));
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Current material: " + material.name, skins.label);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Change material", skins.button,GUILayout.Width(150)))
                {
                    this.materialsVisible = !this.materialsVisible;
                }
                GUILayout.EndHorizontal();
            }
        }

        internal void Calculations()
        {
            #region Calculations
            //Selection mode
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Calculations mode:", skins.label);
            if (GUILayout.Toggle(calcSelect, "Automatic", skins.toggle)) { calcSelect = true; }
            GUILayout.FlexibleSpace();
            if (GUILayout.Toggle(!calcSelect, "Manual", skins.toggle)) { calcSelect = false; }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            //Calculations
            parachuteScroll = GUILayout.BeginScrollView(parachuteScroll, false, false, skins.horizontalScrollbar, skins.verticalScrollbar, skins.box, GUILayout.Height(160));

            #region Automatic
            if (calcSelect)
            {
                typeID = GUILayout.SelectionGrid(typeID, RCUtils.types, 3, skins.button);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Toggle(getMass, "Use current craft mass", skins.button, GUILayout.Width(150))) { getMass = true; }
                GUILayout.FlexibleSpace();
                if (GUILayout.Toggle(!getMass, "Input craft mass", skins.button, GUILayout.Width(150))) { getMass = false; }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                if (getMass)
                {
                    GUILayout.Label("Currently using " + (useDry ? "dry mass" : "wet mass"), skins.label);
                    if (useDry) { if (GUILayout.Button("Switch to wet mass", skins.button, GUILayout.Width(125))) { useDry = false; } }
                    else { if (GUILayout.Button("Switch to dry mass", skins.button, GUILayout.Width(125))) { useDry = true; } }
                }

                else
                {
                    string m = mass;
                    this.pChute.CreateEntryArea("Mass to use (t):", ref m, 0.1f, 10000, 100);
                    mass = m;
                }

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                if (typeID == 0)
                {
                    if (RCUtils.CanParse(landingSpeed) && RCUtils.CheckRange(float.Parse(landingSpeed), 0.1f, 300)) { GUILayout.Label("Wanted touchdown speed (m/s):", skins.label); }
                    else { GUILayout.Label("Wanted touchdown speed (m/s):", RCUtils.redLabel); }
                }
                else if (typeID == 1)
                {
                    if (RCUtils.CanParse(landingSpeed) && RCUtils.CheckRange(float.Parse(landingSpeed), 0.1f, 5000)) { GUILayout.Label("Wanted speed at target alt (m/s):", skins.label); }
                    else { GUILayout.Label("Wanted speed at target alt (m/s):", RCUtils.redLabel); }
                }
                else
                {
                    if (RCUtils.CanParse(landingSpeed) && RCUtils.CheckRange(float.Parse(landingSpeed), 0.1f, 300)) { GUILayout.Label("Planned landing speed (m/s):", skins.label); }
                    else { GUILayout.Label("Planned landing speed (m/s):", RCUtils.redLabel); }
                }
                GUILayout.FlexibleSpace();
                landingSpeed = GUILayout.TextField(landingSpeed, 10, skins.textField, GUILayout.Width(100));
                GUILayout.EndHorizontal();

                if (typeID == 2)
                {
                    string decel = deceleration;
                    this.pChute.CreateEntryArea("Wanted deceleration (m/s²):", ref decel, 0.1f, 100, 100);
                    deceleration = decel;
                }

                if (typeID == 1)
                {
                    string depAlt = refDepAlt;
                    this.pChute.CreateEntryArea("Target altitude (m):", ref depAlt, 10, RCUtils.GetMaxAtmosphereAltitude(body), 100);
                    refDepAlt = depAlt;
                }

                string chutes = chuteCount;
                this.pChute.CreateEntryArea("Parachutes used (parts):", ref chutes, 1, 100, 100);
                chuteCount = chutes;
            }
            #endregion

            #region Manual
            else
            {
                //Predeployed diameter
                string preDep = preDepDiam, dep = depDiam;
                this.pChute.CreateEntryArea("Predeployed diameter (m):", ref preDep, 0.5f, model.maxDiam / 2, 100);
                if (RCUtils.CanParse(preDepDiam)) { GUILayout.Label("Resulting area: " + RCUtils.GetArea(float.Parse(preDepDiam)).ToString("0.00") + "m²", skins.label); }
                else { GUILayout.Label("Resulting predeployed area: --- m²", skins.label); }

                //Deployed diameter
                this.pChute.CreateEntryArea("Deployed diameter (m):", ref dep, 1, model.maxDiam, 100);
                if (RCUtils.CanParse(depDiam)) { GUILayout.Label("Resulting area: " + RCUtils.GetArea(float.Parse(depDiam)).ToString("0.00") + "m²", skins.label); }
                else { GUILayout.Label("Resulting deployed area: --- m²", skins.label); }
                preDepDiam = preDep;
                depDiam = dep;
            }
            #endregion

            GUILayout.EndScrollView();
            #endregion

            #region Specific
            //Pressure/alt toggle
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(isPressure, "Pressure predeployment", skins.toggle)) { isPressure = true; }
            GUILayout.FlexibleSpace();
            if (GUILayout.Toggle(!isPressure, "Altitude predeployment", skins.toggle)) { isPressure = false; }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (isPressure)
            {
                if (RCUtils.CanParse(predepClause) && RCUtils.CheckRange(float.Parse(predepClause), 0.0001f, (float)FlightGlobals.getStaticPressure(0, body))) { GUILayout.Label("Predeployment pressure (atm):", skins.label); }
                else { GUILayout.Label("Predeployment pressure (atm):", RCUtils.redLabel); } 
            }
            else
            {
                if (RCUtils.CanParse(predepClause) && RCUtils.CheckRange(float.Parse(predepClause), 10, RCUtils.GetMaxAtmosphereAltitude(body))) { GUILayout.Label("Predeployment altitude (m):", skins.label); }
                else { GUILayout.Label("Predeployment altitude (m):", RCUtils.redLabel); } 
            }
            GUILayout.FlexibleSpace();
            predepClause = GUILayout.TextField(predepClause, 10, skins.textField, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            //Deployment altitude

            string alt = deploymentAlt;
            this.pChute.CreateEntryArea("Deployment altitude", ref alt, 10, RCUtils.GetMaxAtmosphereAltitude(body));
            deploymentAlt = alt;

            //Cut altitude
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (RCUtils.CanParseWithEmpty(cutAlt) && RCUtils.CheckRange(RCUtils.ParseWithEmpty(cutAlt), -1, RCUtils.GetMaxAtmosphereAltitude(body))) { GUILayout.Label("Autocut altitude (m):", skins.label); }
            else { GUILayout.Label("Autocut altitude (m):", RCUtils.redLabel); }
            GUILayout.FlexibleSpace();
            cutAlt = GUILayout.TextField(cutAlt, 10, skins.textField, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            //Predeployment speed
            string preSpeed = preDepSpeed;
            this.pChute.CreateEntryArea("Pre deployment speed (s):", ref preSpeed, 0.5f, 5);
            preDepSpeed = preSpeed;

            //Deployment speed
            string speed = depSpeed;
            this.pChute.CreateEntryArea("Deployment speed (s):", ref speed, 1, 10);
            depSpeed = speed;
            #endregion
        }

        internal void MaterialsWindow()
        {
            GUI.DragWindow(new Rect(0, 0, materialsWindow.width, 25));
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            materialsScroll = GUILayout.BeginScrollView(materialsScroll, false, false, skins.horizontalScrollbar, skins.verticalScrollbar, skins.box, GUILayout.MaxHeight(165), GUILayout.Width(140));
            materialsID = GUILayout.SelectionGrid(materialsID, this.pChute.materials.materials.Values.ToArray(), 1, skins.button);
            GUILayout.EndScrollView();
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Description:", skins.label);
            GUILayout.Label(this.pChute.materials.materials.Keys.ToArray()[materialsID].description, skins.label);
            GUILayout.FlexibleSpace();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Drag coefficient:", skins.label);
            GUILayout.Label(this.pChute.materials.materials.Keys.ToArray()[materialsID].dragCoefficient.ToString("0.00#"), skins.label);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Area density:", skins.label);
            GUILayout.Label(this.pChute.materials.materials.Keys.ToArray()[materialsID].areaDensity * 1000 + "kg/m²", skins.label);
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Choose material", skins.button))
            {
                material = this.pChute.materials.materials.Keys.ToArray()[materialsID];
                this.materialsVisible = false;
            }
            if (GUILayout.Button("Cancel", skins.button))
            {
                this.materialsVisible = false;
            }
            GUILayout.EndVertical(); 
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an empty ChuteTemplate
        /// </summary>
        public ChuteTemplate() { }

        /// <summary>
        /// Creates a ChuteTemplate from the given ProceduralChute
        /// </summary>
        /// <param name="pChute">Procedural chute to make the template from</param>
        /// <param name="secondary">If this is the secondary chute</param>
        public ChuteTemplate(ProceduralChute pChute, bool secondary)
        {
            this.pChute = pChute;
            this.secondary = secondary;
            this.pChute.materials.TryGetMaterial(chute.material, ref material);
            materialsID = this.pChute.materials.GetMaterialIndex(chute.material);
            if (this.pChute.textureLibrary != "none")
            {
                this.pChute.textures.TryGetCanopy(chuteID, ref canopy);
                this.pChute.textures.TryGetModel(modelID, ref model);
            }

            if (HighLogic.LoadedSceneIsEditor)
            {
                if (!this.pChute.initiated)
                {
                    if (this.pChute.textures.TryGetCanopy(currentCanopy, ref canopy)) { chuteID = this.pChute.textures.GetCanopyIndex(canopy); }
                    if (this.pChute.textures.TryGetModel(chute.parachuteName, ref model, true)) { modelID = this.pChute.textures.GetModelIndex(model); }
                }

                preDepDiam = chute.preDeployedDiameter.ToString();
                depDiam = chute.deployedDiameter.ToString();
                isPressure = chute.minIsPressure;
                if (isPressure) { predepClause = chute.minPressure.ToString(); }
                else { predepClause = chute.minDeployment.ToString(); }
                deploymentAlt = chute.deploymentAlt.ToString();
                cutAlt = chute.cutAlt.ToString();
                if (cutAlt == "-1") { cutAlt = string.Empty; }
                preDepSpeed = chute.preDeploymentSpeed.ToString();
                depSpeed = chute.deploymentSpeed.ToString();
                if (RCUtils.types.Contains(currentType)) { typeID = RCUtils.types.ToList().IndexOf(currentType); }
            }
            position = this.part.FindModelTransform(chute.parachuteName).position;

            if (HighLogic.LoadedSceneIsFlight) { UpdateCanopy(); }
        }
        #endregion
    }
}
