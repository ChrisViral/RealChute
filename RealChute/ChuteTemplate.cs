using System;
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
        public CelestialBody body
        {
            get { return this.pChute.body; }
        }

        //PArameters for this chute
        private ModelConfig.ModelParameters parameters
        {
            get { return this.model.parameters[this.id]; }
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
                string[] canopies = RCUtils.ParseArray(this.pChute.currentCanopies);
                if (canopies.IndexInRange(this.id)) { return canopies[this.id]; }
                return string.Empty;
            }
        }

        //Current type for this chute
        public string currentType
        {
            get
            {
                string[] chutes = RCUtils.ParseArray(this.pChute.currentTypes);
                if (chutes.IndexInRange(this.id)) { return chutes[this.id]; }
                return string.Empty;
            }
        }

        //If this part has more than one chute
        public bool secondary
        {
            get { return this.id != 0; }
        }

        //GUI
        public EditorGUI editorGUI
        {
            get { return this.pChute.editorGUI; }
        }
        #endregion

        #region Fields
        internal ProceduralChute pChute = null;
        internal Parachute parachute = null;
        internal TemplateGUI templateGUI = new TemplateGUI();
        internal MaterialDefinition material = new MaterialDefinition();
        internal CanopyConfig canopy = new CanopyConfig();
        internal ModelConfig model = new ModelConfig();
        public Vector3 position = Vector3.zero;
        public int id = 0;
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
            this.templateGUI = new TemplateGUI(this);
            if (node != null) { Load(node); }
        }
        #endregion

        #region Methods
        //Applies changes to the parachute
        internal void ApplyChanges(bool toSymmetryCounterparts)
        {
            this.parachute.material = this.material.name;
            this.parachute.mat = this.material;

            if (this.templateGUI.calcSelect)
            {
                double m = this.templateGUI.getMass ? this.pChute.GetCraftMass(this.templateGUI.useDry) : double.Parse(this.templateGUI.mass);
                double alt = 0, acc = 0;
                switch (this.templateGUI.type)
                {
                    case ParachuteType.MAIN:
                        {
                            alt = double.Parse(this.pChute.landingAlt);
                            acc = (this.body.GeeASL * RCUtils.geeToAcc);
                            break;
                        }
                    case ParachuteType.DROGUE:
                        {
                            alt = double.Parse(this.templateGUI.refDepAlt);
                            acc = (this.body.GeeASL * RCUtils.geeToAcc);
                            break;
                        }
                    case ParachuteType.DRAG:
                        {
                            alt = double.Parse(this.pChute.landingAlt);
                            acc = double.Parse(this.templateGUI.deceleration);
                            break;
                        }
                    default:
                        break;

                }
                double density = this.body.GetDensityAtAlt(alt);
                double speed = double.Parse(this.templateGUI.landingSpeed);
                speed = speed * speed;

                Debug.Log(String.Format("[RealChute]: {0} {1} - m: {2}t, alt: {3}m, ρ: {4}kg/m³, v²: {5}m²/s², a: {6}m/s²", this.part.partInfo.title, RCUtils.ParachuteNumber(this.id), m, alt, density, speed, acc));

                this.parachute.deployedDiameter = RCUtils.Round(Math.Sqrt((8000 * m * acc) / (Math.PI * speed * material.dragCoefficient * density * double.Parse(this.templateGUI.chuteCount))));
                float maxDiam = (this.textures != null || this.textures.models.Count > 0) ? this.model.maxDiam : 70;
                if (this.parachute.deployedDiameter > this.model.maxDiam)
                {
                    this.parachute.deployedDiameter = maxDiam;
                    this.editorGUI.warning = true;
                }
                else { this.editorGUI.warning = false; }
                this.parachute.preDeployedDiameter = RCUtils.Round(this.templateGUI.type == ParachuteType.MAIN ? (this.parachute.deployedDiameter / 20) : (this.parachute.deployedDiameter / 2));
                Debug.Log(String.Format("[RealChute]: {0} {1} - depDiam: {2}m, preDepDiam: {3}m", this.part.partInfo.title, RCUtils.ParachuteNumber(this.id), this.parachute.deployedDiameter, this.parachute.preDeployedDiameter));
            }

            else
            {
                this.parachute.preDeployedDiameter = RCUtils.Round(float.Parse(this.templateGUI.preDepDiam));
                this.parachute.deployedDiameter = RCUtils.Round(float.Parse(this.templateGUI.depDiam));
                Debug.Log(String.Format("[RealChute]: {0} {1} - depDiam: {2}m, preDepDiam: {3}m", this.part.partInfo.title, RCUtils.ParachuteNumber(this.id), this.parachute.deployedDiameter, this.parachute.preDeployedDiameter));
            }

            this.parachute.minIsPressure = this.templateGUI.isPressure;
            if (this.templateGUI.isPressure) { this.parachute.minPressure = float.Parse(this.templateGUI.predepClause); }
            else { this.parachute.minDeployment = float.Parse(this.templateGUI.predepClause); }
            this.parachute.deploymentAlt = float.Parse(this.templateGUI.deploymentAlt);
            this.parachute.cutAlt = GUIUtils.ParseEmpty(this.templateGUI.cutAlt);
            this.parachute.preDeploymentSpeed = float.Parse(this.templateGUI.preDepSpeed);
            this.parachute.deploymentSpeed = float.Parse(this.templateGUI.depSpeed);

            if (toSymmetryCounterparts)
            {
                foreach (Part part in this.part.symmetryCounterparts)
                {
                    Parachute sym = ((RealChuteModule)part.Modules["RealChuteModule"]).parachutes[id];
                    sym.material = this.material.name;
                    sym.mat = this.material;
                    sym.deployedDiameter = this.parachute.deployedDiameter;
                    sym.preDeployedDiameter = this.parachute.preDeployedDiameter;
                    sym.minIsPressure = this.templateGUI.isPressure; 
                    sym.minPressure = this.parachute.minPressure;
                    sym.minDeployment = this.parachute.minDeployment;
                    sym.deploymentAlt = this.parachute.deploymentAlt;
                    sym.cutAlt = this.parachute.cutAlt;
                    sym.preDeploymentSpeed = this.parachute.preDeploymentSpeed;
                    sym.deploymentSpeed = this.parachute.deploymentSpeed;

                    TemplateGUI template = ((ProceduralChute)part.Modules["ProceduralChute"]).chutes[id].templateGUI;
                    template.chuteID = this.templateGUI.chuteID;
                    template.typeID = this.templateGUI.typeID;
                    template.modelID = this.templateGUI.modelID;
                    template.materialsID = this.templateGUI.materialsID;
                    template.isPressure = this.templateGUI.isPressure;
                    template.calcSelect = this.templateGUI.calcSelect;
                    template.getMass = this.templateGUI.getMass;
                    template.useDry = this.templateGUI.useDry;
                    template.preDepDiam = this.templateGUI.preDepDiam;
                    template.depDiam = this.templateGUI.depDiam;
                    template.predepClause = this.templateGUI.predepClause;
                    template.mass = this.templateGUI.mass;
                    template.landingSpeed = this.templateGUI.landingSpeed;
                    template.deceleration = this.templateGUI.deceleration;
                    template.refDepAlt = this.templateGUI.refDepAlt;
                    template.chuteCount = this.templateGUI.chuteCount;
                    template.deploymentAlt = this.templateGUI.deploymentAlt;
                    template.cutAlt = this.templateGUI.cutAlt;
                    template.preDepSpeed = this.templateGUI.preDepSpeed;
                    template.depSpeed = this.templateGUI.depSpeed;
                }
            }
        }

        //Updates the canopy texture
        internal void UpdateCanopyTexture()
        {
            if (this.textures == null) { return; }
            if (this.textures.TryGetCanopy(this.templateGUI.chuteID, ref this.canopy))
            {
                if (string.IsNullOrEmpty(this.canopy.textureURL))
                {
                    Debug.LogWarning("[RealChute]: The " + this.canopy.name + "URL is empty");
                    return;
                }
                Texture2D texture = GameDatabase.Instance.GetTexture(this.canopy.textureURL, false);
                if (texture == null)
                {
                    Debug.LogWarning("[RealChute]: The " + this.canopy.name + "texture is null");
                    return;
                }
                this.parachute.parachute.GetComponents<Renderer>().ForEach(r => r.material.mainTexture = texture);
            }
        }

        //Updates the canopy model
        internal void UpdateCanopy()
        {
            if (this.textures == null) { return; }
            if (this.textures.TryGetModel(this.templateGUI.modelID, ref this.model))
            {
                if (string.IsNullOrEmpty(this.parameters.modelURL))
                {
                    Debug.LogWarning("[RealChute]: The " + this.model.name + " #" + (this.id + 1) + " URL is empty");
                    return;
                }
                GameObject test = GameDatabase.Instance.GetModel(this.parameters.modelURL);
                if (test == null)
                {
                    Debug.LogWarning("[RealChute]: The " + this.model.name + " #" + (this.id + 1) + " GameObject is null");
                    return;
                }
                test.SetActive(true);
                float scale = RCUtils.GetDiameter(this.parachute.deployedArea / this.model.count) / this.model.diameter;
                test.transform.localScale = new Vector3(scale, scale, scale);
                Debug.Log("[RealChute]: " + this.part.partInfo.title + " " + RCUtils.ParachuteNumber(this.id) + " Scale: " + scale);

                GameObject obj = GameObject.Instantiate(test) as GameObject;
                GameObject.Destroy(test);
                Transform toDestroy = this.parachute.parachute, t = obj.transform;
                t.parent = toDestroy.parent;
                t.position = toDestroy.position;
                t.rotation = toDestroy.rotation;
                GameObject.DestroyImmediate(toDestroy.gameObject);
                this.parachute.parachute = this.part.FindModelTransform(this.parameters.transformName);
                this.parachute.parachuteName = this.parameters.transformName;
                this.parachute.deploymentAnimation = this.parameters.depAnim;
                this.parachute.preDeploymentAnimation = this.parameters.preDepAnim;
                this.part.InitiateAnimation(this.parachute.deploymentAnimation);
                this.part.InitiateAnimation(this.parachute.preDeploymentAnimation);
                this.parachute.parachute.gameObject.SetActive(false);
            }
            UpdateCanopyTexture();
        }

        //Type switchup event
        internal void SwitchType()
        {
            if (this.pChute.secondaryChute)
            {
                if (this.templateGUI.materialsVisible && this.pChute.chutes.TrueForAll(c => !c.templateGUI.materialsVisible))
                {
                    this.editorGUI.matX = (int)templateGUI.materialsWindow.x;
                    this.editorGUI.matY = (int)templateGUI.materialsWindow.y;
                    this.pChute.chutes.Find(c => c.templateGUI.materialsVisible).templateGUI.materialsWindow.x = this.editorGUI.matX;
                    this.pChute.chutes.Find(c => c.templateGUI.materialsVisible).templateGUI.materialsWindow.y = this.editorGUI.matY;
                }
            }

            this.templateGUI.SwitchType();
        }

        //Applies the preset on the chute
        internal void ApplyPreset(Preset preset)
        {
            Preset.ChuteParameters parameters = preset.parameters[id];
            this.material = this.pChute.materials.GetMaterial(parameters.material);
            this.templateGUI.materialsID = this.pChute.materials.GetMaterialIndex(parameters.material);
            this.templateGUI.preDepDiam = parameters.preDeployedDiameter;
            this.templateGUI.depDiam = parameters.deployedDiameter;
            this.templateGUI.isPressure = parameters.minIsPressure;
            this.templateGUI.predepClause = this.templateGUI.isPressure ? parameters.minPressure : parameters.minDeployment;
            if (templateGUI.isPressure) { this.parachute.minDeployment = float.Parse(parameters.minDeployment); }
            else { this.parachute.minPressure = float.Parse(parameters.minPressure); }
            this.templateGUI.deploymentAlt = parameters.deploymentAlt;
            this.templateGUI.cutAlt = parameters.cutAlt;
            this.templateGUI.preDepSpeed = parameters.preDeploymentSpeed;
            this.templateGUI.depSpeed = parameters.deploymentSpeed;
            if (this.textures != null)
            {
                if (this.textures.ContainsCanopy(parameters.chuteTexture)) { this.templateGUI.chuteID = this.textures.GetCanopyIndex(parameters.chuteTexture); }
                if (this.textures.ContainsModel(parameters.modelName)) { this.templateGUI.modelID = this.textures.GetModelIndex(parameters.modelName); }
            }
            this.templateGUI.typeID = parameters.type;
            this.templateGUI.calcSelect = parameters.calcSelect;
            this.templateGUI.getMass = parameters.getMass;
            this.templateGUI.useDry = parameters.useDry;
            this.templateGUI.mass = parameters.mass;
            this.templateGUI.landingSpeed = parameters.landingSpeed;
            this.templateGUI.deceleration = parameters.deceleration;
            this.templateGUI.refDepAlt = parameters.refDepAlt;
            this.templateGUI.chuteCount = parameters.chuteCount;
        }

        //Initializes the chute
        public void Initialize()
        {
            this.parachute = this.pChute.rcModule.parachutes[this.id];
            this.pChute.materials.TryGetMaterial(this.parachute.material, ref this.material);
            this.templateGUI.materialsID = this.pChute.materials.GetMaterialIndex(this.parachute.material);

            if (HighLogic.LoadedSceneIsEditor)
            {
                if (this.textures != null)
                {
                    if (this.templateGUI.chuteID == -1)
                    {
                        if (this.textures.TryGetCanopy(this.currentCanopy, ref this.canopy))
                        {
                            this.templateGUI.chuteID = this.textures.GetCanopyIndex(this.canopy.name);
                        }
                        else { this.templateGUI.modelID = 0; }
                    }

                    if (this.templateGUI.modelID == -1)
                    {
                        if (this.textures.TryGetModel(this.parachute.parachuteName, ref this.model, true))
                        {
                            this.templateGUI.modelID = this.textures.GetModelIndex(this.model.name);
                        }
                        else { this.templateGUI.modelID = 0; }
                    }

                    if (this.templateGUI.type == ParachuteType.NONE)
                    {
                        if (EnumUtils.ContainsType(this.currentType))
                        {
                            this.templateGUI.typeID = EnumUtils.IndexOfType(this.currentType);
                        }
                        else { this.templateGUI.typeID = 0; }
                    }
                }
                else if (this.textures != null)
                {
                    this.textures.TryGetCanopy(this.templateGUI.chuteID, ref this.canopy);
                    this.textures.TryGetModel(this.templateGUI.modelID, ref this.model);
                }

                this.templateGUI.preDepDiam = this.parachute.preDeployedDiameter.ToString();
                this.templateGUI.depDiam = this.parachute.deployedDiameter.ToString();
                this.templateGUI.isPressure = this.parachute.minIsPressure;
                if (this.templateGUI.isPressure) { this.templateGUI.predepClause = this.parachute.minPressure.ToString(); }
                else { this.templateGUI.predepClause = this.parachute.minDeployment.ToString(); }
                this.templateGUI.deploymentAlt = this.parachute.deploymentAlt.ToString();
                this.templateGUI.cutAlt = this.parachute.cutAlt == -1 ? string.Empty : this.parachute.cutAlt.ToString();
                this.templateGUI.preDepSpeed = this.parachute.preDeploymentSpeed.ToString();
                this.templateGUI.depSpeed = this.parachute.deploymentSpeed.ToString();
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
            int t = 0;
            node.TryGetValue("chuteID", ref this.templateGUI.chuteID);
            node.TryGetValue("modelID", ref this.templateGUI.modelID);
            if (node.TryGetValue("typeID", ref t)) { this.templateGUI.typeID = t; }
            if (node.TryGetValue("lastTypeID", ref t)) { this.templateGUI.lastTypeID = t; }
            node.TryGetValue("position", ref this.position);
            node.TryGetValue("isPressure", ref this.templateGUI.isPressure);
            node.TryGetValue("calcSelect", ref this.templateGUI.calcSelect);
            node.TryGetValue("getMass", ref this.templateGUI.getMass);
            node.TryGetValue("useDry", ref this.templateGUI.useDry);
            node.TryGetValue("preDepDiam", ref this.templateGUI.preDepDiam);
            node.TryGetValue("depDiam", ref this.templateGUI.depDiam);
            node.TryGetValue("predepClause", ref this.templateGUI.predepClause);
            node.TryGetValue("mass", ref this.templateGUI.mass);
            node.TryGetValue("landingSpeed", ref this.templateGUI.landingSpeed);
            node.TryGetValue("deceleration", ref this.templateGUI.deceleration);
            node.TryGetValue("refDepAlt", ref this.templateGUI.refDepAlt);
            node.TryGetValue("chuteCount", ref this.templateGUI.chuteCount);
            node.TryGetValue("deploymentAlt", ref this.templateGUI.deploymentAlt);
            node.TryGetValue("cutAlt", ref this.templateGUI.cutAlt);
            node.TryGetValue("preDepSpeed", ref this.templateGUI.preDepSpeed);
            node.TryGetValue("depSpeed", ref this.templateGUI.depSpeed);
        }

        /// <summary>
        /// Saves this ChuteTemplate to a ConfigNode and returns it
        /// </summary>
        public ConfigNode Save()
        {
            ConfigNode node = new ConfigNode("CHUTE");
            node.AddValue("chuteID", this.templateGUI.chuteID);
            node.AddValue("modelID", this.templateGUI.modelID);
            node.AddValue("typeID", this.templateGUI.typeID);
            node.AddValue("lastTypeID", this.templateGUI.lastTypeID);
            node.AddValue("position", this.position);
            node.AddValue("isPressure", this.templateGUI.isPressure);
            node.AddValue("calcSelect", this.templateGUI.calcSelect);
            node.AddValue("getMass", this.templateGUI.getMass);
            node.AddValue("useDry", this.templateGUI.useDry);
            node.AddValue("preDepDiam", this.templateGUI.preDepDiam);
            node.AddValue("depDiam", this.templateGUI.depDiam);
            node.AddValue("predepClause", this.templateGUI.predepClause);
            node.AddValue("mass", this.templateGUI.mass);
            node.AddValue("landingSpeed", this.templateGUI.landingSpeed);
            node.AddValue("deceleration", this.templateGUI.deceleration);
            node.AddValue("refDepAlt", this.templateGUI.refDepAlt);
            node.AddValue("chuteCount", this.templateGUI.chuteCount);
            node.AddValue("deploymentAlt", this.templateGUI.deploymentAlt);
            node.AddValue("cutAlt", this.templateGUI.cutAlt);
            node.AddValue("preDepSpeed", this.templateGUI.preDepSpeed);
            node.AddValue("depSpeed", this.templateGUI.depSpeed);
            return node;
        }
        #endregion
    }
}