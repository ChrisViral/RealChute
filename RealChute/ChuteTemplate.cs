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
                string[] chutes = this.pChute.currentTypes.Split(',');
                if (chutes.Length >= (id + 1)) { return chutes[id].Trim(); }
                return "none";
            }
        }

        //If this part has more than one chute
        public bool secondary
        {
            get { return id != 0; }
        }

        //GUI
        public EditorGUI editorGUI
        {
            get { return pChute.editorGUI; }
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
            parachute.material = material.name;
            parachute.mat = material;

            if (this.templateGUI.calcSelect)
            {
                float m = 0;
                if (this.templateGUI.getMass) { m = this.pChute.GetCraftMass(this.templateGUI.useDry); }
                else { m = float.Parse(this.templateGUI.mass); }

                double alt = this.templateGUI.typeID == 1 ? double.Parse(this.templateGUI.refDepAlt) : double.Parse(pChute.landingAlt);
                float density = (float)body.GetDensityAtAlt(alt);

                float speed = float.Parse(this.templateGUI.landingSpeed);
                float speed2 = speed * speed;

                float acc = this.templateGUI.typeID == 2 ? float.Parse(this.templateGUI.deceleration) : (float)(body.GeeASL * RCUtils.geeToAcc);

                Debug.Log(String.Concat("[RealChute]: ", this.part.partInfo.title, " ", RCUtils.ParachuteNumber(this.id), " - m: ", m, "t, alt: ", alt, "m, rho: ", density, "kg/m³, v²: ", speed2, "m²/s², acceleration: ", acc, "m/s²"));

                parachute.deployedDiameter = RCUtils.Round(Mathf.Sqrt((8000f * m * acc) / (Mathf.PI * speed2 * material.dragCoefficient * density * float.Parse(this.templateGUI.chuteCount))));
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
                parachute.preDeployedDiameter = RCUtils.Round(this.templateGUI.typeID == 0 ? (parachute.deployedDiameter / 20) : (parachute.deployedDiameter / 2));
                Debug.Log(String.Concat("[RealChute]: ", this.part.partInfo.title, " ", RCUtils.ParachuteNumber(this.id), " - depDiam: ", parachute.deployedDiameter, "m, preDepDiam: ", parachute.preDeployedDiameter, "m"));
            }

            else
            {
                parachute.preDeployedDiameter = RCUtils.Round(float.Parse(this.templateGUI.preDepDiam));
                parachute.deployedDiameter = RCUtils.Round(float.Parse(this.templateGUI.depDiam));
            }

            parachute.minIsPressure = this.templateGUI.isPressure;
            if (this.templateGUI.isPressure) { parachute.minPressure = float.Parse(this.templateGUI.predepClause); }
            else { parachute.minDeployment = float.Parse(this.templateGUI.predepClause); }
            parachute.deploymentAlt = float.Parse(this.templateGUI.deploymentAlt);
            parachute.cutAlt = RCUtils.ParseWithEmpty(this.templateGUI.cutAlt);
            parachute.preDeploymentSpeed = float.Parse(this.templateGUI.preDepSpeed);
            parachute.deploymentSpeed = float.Parse(this.templateGUI.depSpeed);

            if (toSymmetryCounterparts)
            {
                foreach (Part part in this.part.symmetryCounterparts)
                {
                    Parachute sym = ((RealChuteModule)part.Modules["RealChuteModule"]).parachutes[id];
                    sym.material = material.name;
                    sym.mat = material;
                    sym.deployedDiameter = parachute.deployedDiameter;
                    sym.preDeployedDiameter = parachute.preDeployedDiameter;

                    sym.minIsPressure = this.templateGUI.isPressure;
                    if (this.templateGUI.isPressure) { sym.minPressure = parachute.minPressure; }
                    else { sym.minDeployment = parachute.minDeployment; }
                    sym.deploymentAlt = parachute.deploymentAlt;
                    sym.cutAlt = parachute.cutAlt;
                    sym.preDeploymentSpeed = parachute.preDeploymentSpeed;
                    sym.deploymentSpeed = parachute.deploymentSpeed;

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
            if (this.pChute.textureLibrary == "none") { return; }
            if (this.textures.canopyNames.Length > 0 && this.textures.TryGetCanopy(this.templateGUI.chuteID, ref canopy))
            {
                if (string.IsNullOrEmpty(canopy.textureURL))
                {
                    Debug.LogWarning("[RealChute]: The " + this.textures.canopyNames[this.templateGUI.chuteID] + "URL is empty");
                    return;
                }
                Texture2D texture = GameDatabase.Instance.GetTexture(canopy.textureURL, false);
                if (texture == null)
                {
                    Debug.LogWarning("[RealChute]: The " + this.textures.canopyNames[this.templateGUI.chuteID] + "texture is null");
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
                if (this.textures.modelNames.Length > 0 && this.textures.TryGetModel(this.textures.modelNames[this.templateGUI.modelID], ref model))
                {
                    if (string.IsNullOrEmpty(parameters.modelURL))
                    {
                        Debug.LogWarning("[RealChute]: The " + this.textures.modelNames[this.templateGUI.modelID] + " #" + (this.id + 1) + " URL is empty");
                        return;
                    }
                    GameObject test = GameDatabase.Instance.GetModel(parameters.modelURL);
                    if (test == null)
                    {
                        Debug.LogWarning("[RealChute]: The " + this.textures.modelNames[this.templateGUI.modelID] + " #" + (this.id + 1) + " GameObject is null");
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
                if (this.templateGUI.materialsVisible && this.pChute.chutes.TrueForAll(c => !c.templateGUI.materialsVisible))
                {
                    this.editorGUI.matX = (int)templateGUI.materialsWindow.x;
                    this.editorGUI.matY = (int)templateGUI.materialsWindow.y;
                    this.pChute.chutes.Select(c => c.templateGUI).First(t => t.materialsVisible).materialsWindow.x = this.editorGUI.matX;
                    this.pChute.chutes.Select(c => c.templateGUI).First(t => t.materialsVisible).materialsWindow.y = this.editorGUI.matY;
                }
            }

            this.templateGUI.SwitchType();
        }

        //Applies the preset on the chute
        internal void ApplyPreset(Preset preset)
        {
            Preset.ChuteParameters parameters = preset.parameters[id];
            this.material = pChute.materials.GetMaterial(parameters.material);
            this.templateGUI.materialsID = pChute.materials.GetMaterialIndex(parameters.material);
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
            if (pChute.textureLibrary == preset.textureLibrary || pChute.textureLibrary != "none")
            {
                if (textures.canopyNames.Contains(parameters.chuteTexture) && textures.canopies.Count > 0 && !string.IsNullOrEmpty(parameters.chuteTexture)) { this.templateGUI.chuteID = textures.GetCanopyIndex(textures.GetCanopy(parameters.chuteTexture)); }
                if (textures.modelNames.Contains(parameters.modelName) && textures.models.Count > 0 && !string.IsNullOrEmpty(parameters.modelName)) { this.templateGUI.modelID = textures.GetModelIndex(textures.GetModel(parameters.modelName)); }
            }
            this.templateGUI.typeID = RCUtils.types.ToList().IndexOf(parameters.type);
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
            if (this.pChute.textureLibrary != "none" && !string.IsNullOrEmpty(this.pChute.textureLibrary))
            {
                this.textures.TryGetCanopy(this.templateGUI.chuteID, ref this.canopy);
                this.textures.TryGetModel(this.templateGUI.modelID, ref this.model);
            }

            if (HighLogic.LoadedSceneIsEditor)
            {
                if (this.pChute.textureLibrary != "none" && this.textures != null)
                {
                    if (this.templateGUI.chuteID == -1 && this.textures.TryGetCanopy(this.currentCanopy, ref this.canopy))
                    {
                        this.templateGUI.chuteID = this.textures.GetCanopyIndex(this.canopy);
                    }
                    else if (this.templateGUI.chuteID == -1) { this.templateGUI.modelID = 0; }
                    if (this.templateGUI.modelID == -1 && this.textures.TryGetModel(this.parachute.parachuteName, ref this.model, true))
                    {
                        this.templateGUI.modelID = this.textures.GetModelIndex(this.model);
                    }
                    else if (this.templateGUI.modelID == -1) { this.templateGUI.modelID = 0; }
                    if (this.templateGUI.typeID == -1 && RCUtils.types.Contains(this.currentType))
                    {
                        this.templateGUI.typeID = RCUtils.types.ToList().IndexOf(this.currentType);
                    }
                    else if (this.templateGUI.typeID == -1) { this.templateGUI.typeID = 0; }
                }

                this.templateGUI.preDepDiam = this.parachute.preDeployedDiameter.ToString();
                this.templateGUI.depDiam = this.parachute.deployedDiameter.ToString();
                this.templateGUI.isPressure = this.parachute.minIsPressure;
                if (this.templateGUI.isPressure) { this.templateGUI.predepClause = this.parachute.minPressure.ToString(); }
                else { this.templateGUI.predepClause = this.parachute.minDeployment.ToString(); }
                this.templateGUI.deploymentAlt = this.parachute.deploymentAlt.ToString();
                this.templateGUI.cutAlt = this.parachute.cutAlt.ToString();
                if (this.templateGUI.cutAlt == "-1") { this.templateGUI.cutAlt = string.Empty; }
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
            node.TryGetValue("chuteID", ref this.templateGUI.chuteID);
            node.TryGetValue("modelID", ref this.templateGUI.modelID);
            node.TryGetValue("typeID", ref this.templateGUI.typeID);
            node.TryGetValue("lastTypeID", ref this.templateGUI.lastTypeID);
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