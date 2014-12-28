﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RealChute.Extensions;
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
    public class ChuteTemplate
    {
        #region Propreties
        //All Parachutes objects on the part
        public List<Parachute> parachutes
        {
            get { return this.pChute.rcModule.parachutes; }
        }

        //Current part
        private Part part
        {
            get { return this.pChute.part; }
        }

        //Selected CelestialBody
        private CelestialBody body
        {
            get { return this.pChute.body; }
        }

        //PArameters for this chute
        private ModelConfig.ModelParameters parameters
        {
            get { return model.parameters[id]; }
        }

        //All current ChuteTemplate objects on this part
        private List<ChuteTemplate> chutes
        {
            get { return this.pChute.chutes; }
        }

        //Current TextureConfig
        private TextureConfig textures
        {
            get { return this.pChute.textures; }
        }

        //Current canopy for this chute
        public string currentCanopy
        {
            get
            {
                string[] canopies = this.pChute.currentCanopies.Split(',');
                if (canopies.Length >= (id + 1)) { return canopies[id].Trim(); }
                return "none";
            }
        }

        //Current type for this chute
        public string currentType
        {
            get
            {
                string[] types = this.pChute.currentTypes.Split(',');
                if (types.Length >= (id + 1)) { return types[id].Trim(); }
                return "Main";
            }
        }

        //GUI errors
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
                    if (typeID == 1 && !RCUtils.CanParse(refDepAlt) || !RCUtils.CheckRange(float.Parse(refDepAlt), 10, (float)body.GetMaxAtmosphereAltitude())) { main.Add("Mains planned deployment alt"); }
                    if (!RCUtils.CanParse(chuteCount) || !RCUtils.CheckRange(float.Parse(chuteCount), 1, 100)) { main.Add("Parachute count"); }
                }
                else
                {
                    if (!RCUtils.CanParse(preDepDiam) || !RCUtils.CheckRange(float.Parse(preDepDiam), 0.5f, model.maxDiam / 2)) { main.Add("Predeployed diameter"); }
                    if (!RCUtils.CanParse(depDiam) || !RCUtils.CheckRange(float.Parse(depDiam), 1, model.maxDiam)) { main.Add("Deployed diameter"); }
                }
                if (!RCUtils.CanParse(predepClause) || (isPressure && !RCUtils.CheckRange(float.Parse(predepClause), 0.0001f, (float)body.GetPressureASL())) || (!isPressure && !RCUtils.CheckRange(float.Parse(predepClause), 10, (float)body.GetMaxAtmosphereAltitude())))
                {
                    if (isPressure) { main.Add("Predeployment pressure"); }
                    else { main.Add("Predeployment altitude"); }
                }
                if (!RCUtils.CanParse(deploymentAlt) || !RCUtils.CheckRange(float.Parse(deploymentAlt), 10, (float)body.GetMaxAtmosphereAltitude())) { main.Add("Deployment altitude"); }
                if (!RCUtils.CanParseWithEmpty(cutAlt) || !RCUtils.CheckRange(RCUtils.ParseWithEmpty(cutAlt), -1, (float)body.GetMaxAtmosphereAltitude())) { main.Add("Autocut altitude"); }
                if (!RCUtils.CanParse(preDepSpeed) || !RCUtils.CheckRange(float.Parse(preDepSpeed), 0.5f, 5)) { main.Add("Predeployment speed"); }
                if (!RCUtils.CanParse(depSpeed) || !RCUtils.CheckRange(float.Parse(depSpeed), 1, 10)) { main.Add("Deployment speed"); }
                return main;
            }
        }

        //If this part has more than one chute
        public bool secondary
        {
            get { return id != 0; }
        }

        //GUI
        private EditorGUI editorGUI
        {
            get { return pChute.editorGUI; }
        }
        #endregion

        #region Fields
        internal ProceduralChute pChute = null;
        internal Parachute parachute = null;
        internal MaterialDefinition material = new MaterialDefinition();
        internal CanopyConfig canopy = new CanopyConfig();
        internal ModelConfig model = new ModelConfig();
        internal Vector2 parachuteScroll = new Vector2(), materialsScroll = new Vector2();
        private GUISkin skins = HighLogic.Skin;

        //Specific fields
        public int id = 0;
        public int chuteID = -1, modelID = -1, materialsID = 0;
        public int typeID = -1, lastTypeID = 0;
        public Vector3 position = Vector3.zero;
        public bool isPressure = false, calcSelect = true;
        public bool getMass = true, useDry = true;
        public string preDepDiam = string.Empty, depDiam = string.Empty, predepClause = string.Empty;
        public string mass = "10", landingSpeed = "6", deceleration = "10", refDepAlt = "700", chuteCount = "1";
        public string deploymentAlt = string.Empty, landingAlt = "0", cutAlt = string.Empty;
        public string preDepSpeed = string.Empty, depSpeed = string.Empty;

        //GUI
        internal Rect materialsWindow = new Rect();
        internal int matId = Guid.NewGuid().GetHashCode();
        internal bool materialsVisible = false;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an empty ChuteTemplate
        /// </summary>
        public ChuteTemplate() { }

        /// <summary>
        /// Creates a new ChuteTemplate from the given ProceduralChute
        /// </summary>
        /// <param name="pChute">Module to create the object from</param>
        /// <param name="node">ConfigNode to load the data from</param>
        /// <param name="id">Index of the ChuteTemplate</param>
        public ChuteTemplate(ProceduralChute pChute, ConfigNode node, int id)
        {
            this.pChute = pChute;
            this.id = id;
            if (node != null) { Load(node); }
        }
        #endregion

        #region Methods
        //Applies changes to the parachute
        internal void ApplyChanges(bool toSymmetryCounterparts)
        {
            parachute.material = material.name;
            parachute.mat = material;

            if (calcSelect)
            {
                float m = 0;
                if (getMass) { m = this.pChute.GetCraftMass(useDry); }
                else { m = float.Parse(mass); }

                double alt = typeID == 1 ? double.Parse(refDepAlt) : double.Parse(pChute.landingAlt);
                float density = (float)body.GetDensityAtAlt(alt);

                float speed = float.Parse(landingSpeed);
                float speed2 = speed * speed;

                float acc = typeID == 2 ? float.Parse(deceleration) : (float)(body.GeeASL * RCUtils.geeToAcc);

                Debug.Log(String.Concat("[RealChute]: ", this.part.partInfo.title, " ", RCUtils.ParachuteNumber(this.id)," - m: ", m, "t, alt: ", alt, "m, rho: ", density, "kg/m³, v²: ", speed2, "m²/s², acceleration: ", acc, "m/s²"));

                parachute.deployedDiameter = RCUtils.Round(Mathf.Sqrt((8000f * m * acc) / (Mathf.PI * speed2 * material.dragCoefficient * density * float.Parse(chuteCount))));
                if ((this.pChute.textureLibrary != "none" || this.textures.modelNames.Length > 0) && parachute.deployedDiameter > model.maxDiam)
                {
                    parachute.deployedDiameter = model.maxDiam;
                    this.editorGUI.warning = true;
                }
                else if ((this.pChute.textureLibrary == "none" || this.textures.modelNames.Length <= 0) && parachute.deployedDiameter > 70f)
                {
                    parachute.deployedDiameter = 70f;
                    this.editorGUI.warning = true;
                }
                else { this.editorGUI.warning = false; }
                parachute.preDeployedDiameter = RCUtils.Round(typeID == 0 ? (parachute.deployedDiameter / 20) : (parachute.deployedDiameter / 2));
                Debug.Log(String.Concat("[RealChute]: ", this.part.partInfo.title, " ", RCUtils.ParachuteNumber(this.id), " - depDiam: ", parachute.deployedDiameter, "m, preDepDiam: ", parachute.preDeployedDiameter, "m"));
            }

            else
            {
                parachute.preDeployedDiameter = RCUtils.Round(float.Parse(preDepDiam));
                parachute.deployedDiameter = RCUtils.Round(float.Parse(depDiam));
            }

            parachute.minIsPressure = isPressure;
            if (isPressure) { parachute.minPressure = float.Parse(predepClause); }
            else { parachute.minDeployment = float.Parse(predepClause); }
            parachute.deploymentAlt = float.Parse(deploymentAlt);
            parachute.cutAlt = RCUtils.ParseWithEmpty(cutAlt);
            parachute.preDeploymentSpeed = float.Parse(preDepSpeed);
            parachute.deploymentSpeed = float.Parse(depSpeed);

            if (toSymmetryCounterparts)
            {
                foreach (Part part in this.part.symmetryCounterparts)
                {
                    Parachute sym = ((RealChuteModule)part.Modules["RealChuteModule"]).parachutes[id];
                    sym.material = material.name;
                    sym.mat = material;
                    sym.deployedDiameter = parachute.deployedDiameter;
                    sym.preDeployedDiameter = parachute.preDeployedDiameter;

                    sym.minIsPressure = isPressure;
                    if (isPressure) { sym.minPressure = parachute.minPressure; }
                    else { sym.minDeployment = parachute.minDeployment; }
                    sym.deploymentAlt = parachute.deploymentAlt;
                    sym.cutAlt = parachute.cutAlt;
                    sym.preDeploymentSpeed = parachute.preDeploymentSpeed;
                    sym.deploymentSpeed = parachute.deploymentSpeed;

                    ChuteTemplate template = ((ProceduralChute)part.Modules["ProceduralChute"]).chutes[id];
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

        //Updates the canopy texture
        internal void UpdateCanopyTexture()
        {
            if (this.pChute.textureLibrary == "none") { return; }
            if (this.textures.canopyNames.Length > 0 && this.textures.TryGetCanopy(chuteID, ref canopy))
            {
                if (string.IsNullOrEmpty(canopy.textureURL))
                {
                    Debug.LogWarning("[RealChute]: The " + this.textures.canopyNames[chuteID] + "URL is empty");
                    return;
                }
                Texture2D texture = GameDatabase.Instance.GetTexture(canopy.textureURL, false);
                if (texture == null)
                {
                    Debug.LogWarning("[RealChute]: The " + this.textures.canopyNames[chuteID] + "texture is null");
                    return;
                }
                parachute.parachute.GetComponents<Renderer>().ToList().ForEach(r => r.material.mainTexture = texture);
            }
        }

        //Updates the canopy model
        internal void UpdateCanopy()
        {
            if (this.pChute.textureLibrary != "none")
            {
                if (this.textures.modelNames.Length > 0 && this.textures.TryGetModel(this.textures.modelNames[modelID], ref model))
                {
                    if (string.IsNullOrEmpty(parameters.modelURL))
                    {
                        Debug.LogWarning("[RealChute]: The " + this.textures.modelNames[modelID] + " #" + (this.id + 1) + " URL is empty");
                        return;
                    }
                    GameObject test = GameDatabase.Instance.GetModel(parameters.modelURL);
                    if (test == null)
                    {
                        Debug.LogWarning("[RealChute]: The " + this.textures.modelNames[modelID] + " #" + (this.id + 1) + " GameObject is null");
                        return;
                    }
                    test.SetActive(true);
                    float scale = RCUtils.GetDiameter(parachute.deployedArea / model.count) / model.diameter;
                    test.transform.localScale = new Vector3(scale, scale, scale);
                    Debug.Log("[RealChute]: " + part.partInfo.title + " " + RCUtils.ParachuteNumber(this.id) + " Scale: " + scale);

                    GameObject obj = GameObject.Instantiate(test) as GameObject;
                    GameObject.Destroy(test);
                    Transform toDestroy = parachute.parachute;
                    obj.transform.parent = toDestroy.parent;
                    obj.transform.position = toDestroy.position;
                    obj.transform.rotation = toDestroy.rotation;
                    GameObject.DestroyImmediate(toDestroy.gameObject);
                    parachute.parachute = part.FindModelTransform(parameters.transformName);
                    parachute.parachuteName = parameters.transformName;
                    parachute.deploymentAnimation = parameters.depAnim;
                    parachute.preDeploymentAnimation = parameters.preDepAnim;
                    this.part.InitiateAnimation(parachute.deploymentAnimation);
                    this.part.InitiateAnimation(parachute.preDeploymentAnimation);
                    parachute.parachute.gameObject.SetActive(false);
                }
                UpdateCanopyTexture();
            }
        }

        //Type switchup event
        internal void SwitchType()
        {
            if (this.pChute.secondaryChute)
            {
                if (this.materialsVisible && this.pChute.chutes.TrueForAll(c => !c.materialsVisible))
                {
                    this.editorGUI.matX = (int)materialsWindow.x;
                    this.editorGUI.matY = (int)materialsWindow.y;
                    this.pChute.chutes.First(c => c.materialsVisible).materialsWindow.x = this.editorGUI.matX;
                    this.pChute.chutes.First(c => c.materialsVisible).materialsWindow.y = this.editorGUI.matY;
                }
            }

            if (lastTypeID != typeID)
            {
                switch (typeID)
                {
                    case 0:
                        {
                            landingSpeed = "6";
                            deploymentAlt = "700";
                            predepClause = isPressure ? "0.01" : "25000";
                            preDepSpeed = "2";
                            depSpeed = "6";
                            break;
                        }

                    case 1:
                        {
                            landingSpeed = "80";
                            deploymentAlt = "2500";
                            predepClause = isPressure ? "0.007" : "30000";
                            preDepSpeed = "1";
                            depSpeed = "3";
                            break;
                        }

                    case 2:
                        {
                            landingSpeed = "100";
                            deploymentAlt = "10";
                            predepClause = isPressure ? "0.5" : "50";
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

        //Texture selector GUI code
        internal void TextureSelector()
        {
            if ((!secondary && this.pChute.TextureEntries("case").Length > 1) || this.pChute.TextureEntries("chute").Length > 1 || this.pChute.TextureEntries("model").Length > 1)
            {
                int m = 0;
                if (!secondary && this.pChute.TextureEntries("case").Length > 1) { m++; }
                if (this.pChute.TextureEntries("chute").Length > 1) { m++; }
                if (this.pChute.TextureEntries("model").Length > 1) { m++; }

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
                if (this.pChute.TextureEntries("model").Length > 1)
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

                if (this.pChute.TextureEntries("model").Length > 1)
                {
                    GUILayout.FlexibleSpace();
                    modelID = GUILayout.SelectionGrid(modelID, this.pChute.TextureEntries("model"), this.pChute.TextureEntries("model").Length, skins.button);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                #endregion

                GUILayout.EndHorizontal();
            }
        }

        //Materials selector GUI code
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
                if (GUILayout.Button("Change material", skins.button, GUILayout.Width(150)))
                {
                    this.materialsVisible = !this.materialsVisible;
                }
                GUILayout.EndHorizontal();
            }
        }

        //Calculations GUI core
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
                    this.editorGUI.CreateEntryArea("Mass to use (t):", ref m, 0.1f, 10000, 100);
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
                    this.editorGUI.CreateEntryArea("Wanted deceleration (m/s²):", ref decel, 0.1f, 100, 100);
                    deceleration = decel;
                }

                if (typeID == 1)
                {
                    string depAlt = refDepAlt;
                    this.editorGUI.CreateEntryArea("Target altitude (m):", ref depAlt, 10, (float)body.GetMaxAtmosphereAltitude(), 100);
                    refDepAlt = depAlt;
                }

                string chutes = chuteCount;
                this.editorGUI.CreateEntryArea("Parachutes used (parts):", ref chutes, 1, 100, 100);
                chuteCount = chutes;
            }
            #endregion

            #region Manual
            else
            {
                //Predeployed diameter
                string preDep = preDepDiam, dep = depDiam;
                this.editorGUI.CreateEntryArea("Predeployed diameter (m):", ref preDep, 0.5f, model.maxDiam / 2, 100);
                if (RCUtils.CanParse(preDepDiam)) { GUILayout.Label("Resulting area: " + RCUtils.GetArea(float.Parse(preDepDiam)).ToString("0.00") + "m²", skins.label); }
                else { GUILayout.Label("Resulting predeployed area: --- m²", skins.label); }

                //Deployed diameter
                this.editorGUI.CreateEntryArea("Deployed diameter (m):", ref dep, 1, model.maxDiam, 100);
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
            if (GUILayout.Toggle(isPressure, "Pressure predeployment", skins.toggle))
            {
                if (!isPressure)
                {
                    isPressure = true;
                    this.predepClause = this.parachute.minPressure.ToString();
                }
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Toggle(!isPressure, "Altitude predeployment", skins.toggle))
            {
                if(isPressure)
                {
                    isPressure = false;
                    this.predepClause = this.parachute.minDeployment.ToString();
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (isPressure)
            {
                if (RCUtils.CanParse(predepClause) && RCUtils.CheckRange(float.Parse(predepClause), 0.0001f, (float)body.GetPressureASL())) { GUILayout.Label("Predeployment pressure (atm):", skins.label); }
                else { GUILayout.Label("Predeployment pressure (atm):", RCUtils.redLabel); }
            }
            else
            {
                if (RCUtils.CanParse(predepClause) && RCUtils.CheckRange(float.Parse(predepClause), 10, (float)body.GetMaxAtmosphereAltitude())) { GUILayout.Label("Predeployment altitude (m):", skins.label); }
                else { GUILayout.Label("Predeployment altitude (m):", RCUtils.redLabel); }
            }
            GUILayout.FlexibleSpace();
            predepClause = GUILayout.TextField(predepClause, 10, skins.textField, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            //Deployment altitude
            this.editorGUI.CreateEntryArea("Deployment altitude", ref deploymentAlt, 10, (float)body.GetMaxAtmosphereAltitude());

            //Cut altitude
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (RCUtils.CanParseWithEmpty(cutAlt) && RCUtils.CheckRange(RCUtils.ParseWithEmpty(cutAlt), -1, (float)body.GetMaxAtmosphereAltitude())) { GUILayout.Label("Autocut altitude (m):", skins.label); }
            else { GUILayout.Label("Autocut altitude (m):", RCUtils.redLabel); }
            GUILayout.FlexibleSpace();
            cutAlt = GUILayout.TextField(cutAlt, 10, skins.textField, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            //Predeployment speed
            this.editorGUI.CreateEntryArea("Pre deployment speed (s):", ref preDepSpeed, 0.5f, 5);

            //Deployment speed
            this.editorGUI.CreateEntryArea("Deployment speed (s):", ref depSpeed, 1, 10);
            #endregion
        }

        //Materials window GUI code
        internal void MaterialsWindow(int id)
        {
            GUI.DragWindow(new Rect(0, 0, materialsWindow.width, 25));
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            materialsScroll = GUILayout.BeginScrollView(materialsScroll, false, false, skins.horizontalScrollbar, skins.verticalScrollbar, skins.box, GUILayout.MaxHeight(200), GUILayout.Width(140));
            materialsID = GUILayout.SelectionGrid(materialsID, this.pChute.materials.materials.Values.ToArray(), 1, skins.button);
            GUILayout.EndScrollView();
            GUILayout.BeginVertical();
            MaterialDefinition material = this.pChute.materials.materials.Keys.ToArray()[materialsID];
            StringBuilder builder = new StringBuilder();
            builder.Append("Description:  ").AppendLine(material.description);
            builder.Append("\nDrag coefficient:  ").AppendLine(material.dragCoefficient.ToString("0.00#"));
            builder.Append("\nArea density:  ").Append(material.areaDensity * 1000).AppendLine("kg/m²\n");
            builder.Append("Area cost:  ").Append(material.areaCost.ToString()).Append("f/m²");
            GUILayout.Label(builder.ToString(), skins.label);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Choose material", skins.button, GUILayout.Width(150)))
            {
                this.material = material;
                this.materialsVisible = false;
            }
            if (GUILayout.Button("Cancel", skins.button, GUILayout.Width(150)))
            {
                this.materialsVisible = false;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        //Applies the preset on the chute
        internal void ApplyPreset(Preset preset)
        {
            Preset.ChuteParameters parameters = preset.parameters[id];
            this.material = pChute.materials.GetMaterial(parameters.material);
            this.materialsID = pChute.materials.GetMaterialIndex(parameters.material);
            this.preDepDiam = parameters.preDeployedDiameter;
            this.depDiam = parameters.deployedDiameter;
            this.isPressure = parameters.minIsPressure;
            this.predepClause = this.isPressure ? parameters.minPressure : parameters.minDeployment;
            if (isPressure) { this.parachute.minDeployment = float.Parse(parameters.minDeployment); }
            else { this.parachute.minPressure = float.Parse(parameters.minPressure); }
            this.deploymentAlt = parameters.deploymentAlt;
            this.cutAlt = parameters.cutAlt;
            this.preDepSpeed = parameters.preDeploymentSpeed;
            this.depSpeed = parameters.deploymentSpeed;
            if (pChute.textureLibrary == preset.textureLibrary || pChute.textureLibrary != "none")
            {
                if (textures.canopyNames.Contains(parameters.chuteTexture) && textures.canopies.Count > 0 && !string.IsNullOrEmpty(parameters.chuteTexture)) { this.chuteID = textures.GetCanopyIndex(textures.GetCanopy(parameters.chuteTexture)); }
                if (textures.modelNames.Contains(parameters.modelName) && textures.models.Count > 0 && !string.IsNullOrEmpty(parameters.modelName)) { this.modelID = textures.GetModelIndex(textures.GetModel(parameters.modelName)); }
            }
            this.typeID = RCUtils.types.ToList().IndexOf(parameters.type);
            this.calcSelect = parameters.calcSelect;
            this.getMass = parameters.getMass;
            this.useDry = parameters.useDry;
            this.mass = parameters.mass;
            this.landingSpeed = parameters.landingSpeed;
            this.deceleration = parameters.deceleration;
            this.refDepAlt = parameters.refDepAlt;
            this.chuteCount = parameters.chuteCount;
        }

        //Initializes the chute
        public void Initialize()
        {
            this.parachute = this.pChute.rcModule.parachutes[this.id];
            this.pChute.materials.TryGetMaterial(this.parachute.material, ref this.material);
            this.materialsID = this.pChute.materials.GetMaterialIndex(this.parachute.material);
            if (this.pChute.textureLibrary != "none" && !string.IsNullOrEmpty(this.pChute.textureLibrary))
            {
                this.textures.TryGetCanopy(this.chuteID, ref this.canopy);
                this.textures.TryGetModel(this.modelID, ref this.model);
            }

            if (HighLogic.LoadedSceneIsEditor)
            {
                if (this.pChute.textureLibrary != "none" && this.textures != null)
                {
                    if (this.chuteID == -1 && this.textures.TryGetCanopy(this.currentCanopy, ref this.canopy))
                    {
                        this.chuteID = this.textures.GetCanopyIndex(this.canopy);
                    }
                    else if (this.chuteID == -1) { this.modelID = 0; }
                    if (this.modelID == -1 && this.textures.TryGetModel(this.parachute.parachuteName, ref this.model, true))
                    {
                        this.modelID = this.textures.GetModelIndex(this.model);
                    }
                    else if (this.modelID == -1) { this.modelID = 0; }
                    if (this.typeID == -1 && RCUtils.types.Contains(this.currentType))
                    {
                        this.typeID = RCUtils.types.ToList().IndexOf(this.currentType);
                    }
                    else if (this.typeID == -1) { this.typeID = 0; }
                }

                this.preDepDiam = this.parachute.preDeployedDiameter.ToString();
                this.depDiam = this.parachute.deployedDiameter.ToString();
                this.isPressure = this.parachute.minIsPressure;
                if (this.isPressure) { this.predepClause = this.parachute.minPressure.ToString(); }
                else { this.predepClause = this.parachute.minDeployment.ToString(); }
                this.deploymentAlt = this.parachute.deploymentAlt.ToString();
                this.cutAlt = this.parachute.cutAlt.ToString();
                if (this.cutAlt == "-1") { this.cutAlt = string.Empty; }
                this.preDepSpeed = this.parachute.preDeploymentSpeed.ToString();
                this.depSpeed = this.parachute.deploymentSpeed.ToString();
            }
            this.position = this.parachute.parachute.position;

            if (HighLogic.LoadedSceneIsFlight) { UpdateCanopy(); }
        }
        #endregion

        #region Load/Save
        /// <summary>
        /// Loads the ChuteTemplate from the given ConfigNode
        /// </summary>
        /// <param name="node">ConfigNode to load the object from</param>
        private void Load(ConfigNode node)
        {
            node.TryGetValue("chuteID", ref chuteID);
            node.TryGetValue("modelID", ref modelID);
            node.TryGetValue("typeID", ref typeID);
            node.TryGetValue("lastTypeID", ref lastTypeID);
            node.TryGetValue("position", ref position);
            node.TryGetValue("isPressure", ref isPressure);
            node.TryGetValue("calcSelect", ref calcSelect);
            node.TryGetValue("getMass", ref getMass);
            node.TryGetValue("useDry", ref useDry);
            node.TryGetValue("preDepDiam", ref preDepDiam);
            node.TryGetValue("depDiam", ref depDiam);
            node.TryGetValue("predepClause", ref predepClause);
            node.TryGetValue("mass", ref mass);
            node.TryGetValue("landingSpeed", ref landingSpeed);
            node.TryGetValue("deceleration", ref deceleration);
            node.TryGetValue("refDepAlt", ref refDepAlt);
            node.TryGetValue("chuteCount", ref chuteCount);
            node.TryGetValue("deploymentAlt", ref deploymentAlt);
            node.TryGetValue("cutAlt", ref cutAlt);
            node.TryGetValue("preDepSpeed", ref preDepSpeed);
            node.TryGetValue("depSpeed", ref depSpeed);
        }

        /// <summary>
        /// Saves this ChuteTemplate to a ConfigNode and returns it
        /// </summary>
        public ConfigNode Save()
        {
            ConfigNode node = new ConfigNode("CHUTE");
            node.AddValue("chuteID", chuteID);
            node.AddValue("modelID", modelID);
            node.AddValue("typeID", typeID);
            node.AddValue("lastTypeID", lastTypeID);
            node.AddValue("position", position);
            node.AddValue("isPressure", isPressure);
            node.AddValue("calcSelect", calcSelect);
            node.AddValue("getMass", getMass);
            node.AddValue("useDry", useDry);
            node.AddValue("preDepDiam", preDepDiam);
            node.AddValue("depDiam", depDiam);
            node.AddValue("predepClause", predepClause);
            node.AddValue("mass", mass);
            node.AddValue("landingSpeed", landingSpeed);
            node.AddValue("deceleration", deceleration);
            node.AddValue("refDepAlt", refDepAlt);
            node.AddValue("chuteCount", chuteCount);
            node.AddValue("deploymentAlt", deploymentAlt);
            node.AddValue("cutAlt", cutAlt);
            node.AddValue("preDepSpeed", preDepSpeed);
            node.AddValue("depSpeed", depSpeed);
            return node;
        }
        #endregion
    }
}
