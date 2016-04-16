using System;
using System.Collections.Generic;
using UnityEngine;
using RealChute.Extensions;
using RealChute.Libraries;
using Object = UnityEngine.Object;

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
        public List<Parachute> Parachutes
        {
            get { return this.pChute.rcModule.parachutes; }
        }

        //Current part
        private Part Part
        {
            get { return this.pChute.part; }
        }

        //Selected CelestialBody
        public CelestialBody Body
        {
            get { return this.pChute.body; }
        }

        //PArameters for this chute
        private ModelConfig.ModelParameters Parameters
        {
            get { return this.model.Parameters[this.id]; }
        }

        //All current ChuteTemplate objects on this part
        private List<ChuteTemplate> Chutes
        {
            get { return this.pChute.chutes; }
        }

        //Current TextureConfig
        private TextureConfig Textures
        {
            get { return this.pChute.textures; }
        }

        //Current canopy for this chute
        public string CurrentCanopy
        {
            get
            {
                string[] canopies = RCUtils.ParseArray(this.pChute.currentCanopies);
                if (canopies.IndexInRange(this.id)) { return canopies[this.id]; }
                return string.Empty;
            }
        }

        //Current type for this chute
        public string CurrentType
        {
            get
            {
                string[] chutes = RCUtils.ParseArray(this.pChute.currentTypes);
                if (chutes.IndexInRange(this.id)) { return chutes[this.id]; }
                return string.Empty;
            }
        }

        //If this part has more than one chute
        public bool Secondary
        {
            get { return this.id != 0; }
        }

        //GUI
        public EditorGui EditorGui
        {
            get { return this.pChute.editorGui; }
        }
        #endregion

        #region Fields
        internal ProceduralChute pChute;
        internal Parachute parachute;
        internal TemplateGui templateGui = new TemplateGui();
        internal MaterialDefinition material = new MaterialDefinition();
        internal CanopyConfig canopy = new CanopyConfig();
        internal ModelConfig model = new ModelConfig();
        public Vector3 position = Vector3.zero;
        public int id;
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
            this.templateGui = new TemplateGui(this);
            if (node != null) { Load(node); }
        }
        #endregion

        #region Methods
        //Applies changes to the parachute
        internal void ApplyChanges(bool toSymmetryCounterparts)
        {
            this.parachute.material = this.material.Name;
            this.parachute.mat = this.material;

            if (this.templateGui.calcSelect)
            {
                double m = this.templateGui.getMass ? this.pChute.GetCraftMass(this.templateGui.useDry) : double.Parse(this.templateGui.mass);
                double alt = 0, acc = 0;
                switch (this.templateGui.Type)
                {
                    case ParachuteType.MAIN:
                        {
                            alt = double.Parse(this.pChute.landingAlt);
                            acc = this.Body.GeeASL * RCUtils.geeToAcc;
                            break;
                        }
                    case ParachuteType.DROGUE:
                        {
                            alt = double.Parse(this.templateGui.refDepAlt);
                            acc = this.Body.GeeASL * RCUtils.geeToAcc;
                            break;
                        }
                    case ParachuteType.DRAG:
                        {
                            alt = double.Parse(this.pChute.landingAlt);
                            acc = double.Parse(this.templateGui.deceleration);
                            break;
                        }
                    default:
                        break;

                }
                double density = this.Body.GetDensityAtAlt(alt, this.Body.GetMaxTemperatureAtAlt(alt));
                double speed = double.Parse(this.templateGui.landingSpeed);
                speed *= speed;

                Debug.Log(String.Format("[RealChute]: {0} {1} - m: {2}t, alt: {3}m, ρ: {4}kg/m³, v²: {5}m²/s², a: {6}m/s²", this.Part.partInfo.title, RCUtils.ParachuteNumber(this.id), m, alt, density, speed, acc));

                this.parachute.deployedDiameter = RCUtils.Round(Math.Sqrt((8000 * m * acc) / (Math.PI * speed * this.material.DragCoefficient * density * double.Parse(this.templateGui.chuteCount))));
                float maxDiam = this.Textures != null || this.Textures.Models.Count > 0 ? this.model.MaxDiam : 70;
                if (this.parachute.deployedDiameter > this.model.MaxDiam)
                {
                    this.parachute.deployedDiameter = maxDiam;
                    this.EditorGui.warning = true;
                }
                else { this.EditorGui.warning = false; }
                this.parachute.preDeployedDiameter = RCUtils.Round(this.templateGui.Type == ParachuteType.MAIN ? this.parachute.deployedDiameter / 20 : this.parachute.deployedDiameter / 2);
                Debug.Log(String.Format("[RealChute]: {0} {1} - depDiam: {2}m, preDepDiam: {3}m", this.Part.partInfo.title, RCUtils.ParachuteNumber(this.id), this.parachute.deployedDiameter, this.parachute.preDeployedDiameter));
            }

            else
            {
                this.parachute.preDeployedDiameter = RCUtils.Round(float.Parse(this.templateGui.preDepDiam));
                this.parachute.deployedDiameter = RCUtils.Round(float.Parse(this.templateGui.depDiam));
                Debug.Log(String.Format("[RealChute]: {0} {1} - depDiam: {2}m, preDepDiam: {3}m", this.Part.partInfo.title, RCUtils.ParachuteNumber(this.id), this.parachute.deployedDiameter, this.parachute.preDeployedDiameter));
            }

            this.parachute.minIsPressure = this.templateGui.isPressure;
            if (this.templateGui.isPressure) { this.parachute.minPressure = float.Parse(this.templateGui.predepClause); }
            else { this.parachute.minDeployment = float.Parse(this.templateGui.predepClause); }
            this.parachute.deploymentAlt = float.Parse(this.templateGui.deploymentAlt);
            this.parachute.cutAlt = GuiUtils.ParseEmpty(this.templateGui.cutAlt);
            this.parachute.preDeploymentSpeed = float.Parse(this.templateGui.preDepSpeed);
            this.parachute.deploymentSpeed = float.Parse(this.templateGui.depSpeed);

            if (toSymmetryCounterparts)
            {
                foreach (Part part in this.Part.symmetryCounterparts)
                {
                    Parachute sym = ((RealChuteModule)part.Modules["RealChuteModule"]).parachutes[this.id];
                    sym.material = this.material.Name;
                    sym.mat = this.material;
                    sym.deployedDiameter = this.parachute.deployedDiameter;
                    sym.preDeployedDiameter = this.parachute.preDeployedDiameter;
                    sym.minIsPressure = this.templateGui.isPressure; 
                    sym.minPressure = this.parachute.minPressure;
                    sym.minDeployment = this.parachute.minDeployment;
                    sym.deploymentAlt = this.parachute.deploymentAlt;
                    sym.cutAlt = this.parachute.cutAlt;
                    sym.preDeploymentSpeed = this.parachute.preDeploymentSpeed;
                    sym.deploymentSpeed = this.parachute.deploymentSpeed;

                    TemplateGui template = ((ProceduralChute)part.Modules["ProceduralChute"]).chutes[this.id].templateGui;
                    template.chuteId = this.templateGui.chuteId;
                    template.TypeId = this.templateGui.TypeId;
                    template.modelId = this.templateGui.modelId;
                    template.materialsId = this.templateGui.materialsId;
                    template.isPressure = this.templateGui.isPressure;
                    template.calcSelect = this.templateGui.calcSelect;
                    template.getMass = this.templateGui.getMass;
                    template.useDry = this.templateGui.useDry;
                    template.preDepDiam = this.templateGui.preDepDiam;
                    template.depDiam = this.templateGui.depDiam;
                    template.predepClause = this.templateGui.predepClause;
                    template.mass = this.templateGui.mass;
                    template.landingSpeed = this.templateGui.landingSpeed;
                    template.deceleration = this.templateGui.deceleration;
                    template.refDepAlt = this.templateGui.refDepAlt;
                    template.chuteCount = this.templateGui.chuteCount;
                    template.deploymentAlt = this.templateGui.deploymentAlt;
                    template.cutAlt = this.templateGui.cutAlt;
                    template.preDepSpeed = this.templateGui.preDepSpeed;
                    template.depSpeed = this.templateGui.depSpeed;
                }
            }
        }

        //Updates the canopy texture
        internal void UpdateCanopyTexture()
        {
            if (this.Textures == null) { return; }
            Texture2D texture = null;
            if (RealChuteSettings.Fetch.ActivateNyan) { texture = GameDatabase.Instance.GetTexture(RCUtils.nyanTextureURL, false); }
            else if (this.Textures.TryGetCanopy(this.templateGui.chuteId, ref this.canopy))
            {
                if (string.IsNullOrEmpty(this.canopy.TextureURL))
                {
                    Debug.LogWarning("[RealChute]: The " + this.canopy.Name + "URL is empty");
                    return;
                }
                texture = GameDatabase.Instance.GetTexture(this.canopy.TextureURL, false);
            }

            if (texture == null)
            {
                Debug.LogWarning("[RealChute]: The texture is null");
                return;
            }
            this.parachute.parachute.GetComponents<Renderer>().ForEach(r => r.material.mainTexture = texture);
        }

        //Updates the canopy model
        internal void UpdateCanopy()
        {
            if (this.Textures == null) { return; }
            if (this.Textures.TryGetModel(this.templateGui.modelId, ref this.model))
            {
                if (string.IsNullOrEmpty(this.Parameters.ModelURL))
                {
                    Debug.LogWarning("[RealChute]: The " + this.model.Name + " #" + (this.id + 1) + " URL is empty");
                    return;
                }
                GameObject test = GameDatabase.Instance.GetModel(this.Parameters.ModelURL);
                if (test == null)
                {
                    Debug.LogWarning("[RealChute]: The " + this.model.Name + " #" + (this.id + 1) + " GameObject is null");
                    return;
                }
                test.SetActive(true);
                float scale = RCUtils.GetDiameter(this.parachute.DeployedArea / this.model.Count) / this.model.Diameter;
                test.transform.localScale = new Vector3(scale, scale, scale);
                Debug.Log("[RealChute]: " + this.Part.partInfo.title + " " + RCUtils.ParachuteNumber(this.id) + " Scale: " + scale);

                GameObject obj = Object.Instantiate(test) as GameObject;
                Object.Destroy(test);
                Transform toDestroy = this.parachute.parachute;
                obj.transform.parent = toDestroy.parent;
                obj.transform.position = toDestroy.position;
                obj.transform.rotation = toDestroy.rotation;
                Object.DestroyImmediate(toDestroy.gameObject);
                this.parachute.parachute = this.Part.FindModelTransform(this.Parameters.TransformName);
                this.parachute.parachuteName = this.Parameters.TransformName;
                this.parachute.deploymentAnimation = this.Parameters.DepAnim;
                this.parachute.preDeploymentAnimation = this.Parameters.PreDepAnim;
                this.Part.InitiateAnimation(this.parachute.deploymentAnimation);
                this.Part.InitiateAnimation(this.parachute.preDeploymentAnimation);
                this.parachute.parachute.gameObject.SetActive(false);
            }
            UpdateCanopyTexture();
        }

        //Type switchup event
        internal void SwitchType()
        {
            if (this.pChute.secondaryChute)
            {
                if (this.templateGui.materialsVisible && this.pChute.chutes.TrueForAll(c => !c.templateGui.materialsVisible))
                {
                    this.EditorGui.matX = (int)this.templateGui.materialsWindow.x;
                    this.EditorGui.matY = (int)this.templateGui.materialsWindow.y;
                    this.pChute.chutes.Find(c => c.templateGui.materialsVisible).templateGui.materialsWindow.x = this.EditorGui.matX;
                    this.pChute.chutes.Find(c => c.templateGui.materialsVisible).templateGui.materialsWindow.y = this.EditorGui.matY;
                }
            }

            this.templateGui.SwitchType();
        }

        //Applies the preset on the chute
        internal void ApplyPreset(Preset preset)
        {
            Preset.ChuteParameters parameters = preset.Parameters[this.id];
            this.material = this.pChute.materials.GetMaterial(parameters.Material);
            this.templateGui.materialsId = this.pChute.materials.GetMaterialIndex(parameters.Material);
            this.templateGui.preDepDiam = parameters.PreDeployedDiameter;
            this.templateGui.depDiam = parameters.DeployedDiameter;
            this.templateGui.isPressure = parameters.MinIsPressure;
            this.templateGui.predepClause = this.templateGui.isPressure ? parameters.MinPressure : parameters.MinDeployment;
            if (this.templateGui.isPressure) { this.parachute.minDeployment = float.Parse(parameters.MinDeployment); }
            else { this.parachute.minPressure = float.Parse(parameters.MinPressure); }
            this.templateGui.deploymentAlt = parameters.DeploymentAlt;
            this.templateGui.cutAlt = parameters.CutAlt;
            this.templateGui.preDepSpeed = parameters.PreDeploymentSpeed;
            this.templateGui.depSpeed = parameters.DeploymentSpeed;
            if (this.Textures != null)
            {
                if (this.Textures.ContainsCanopy(parameters.ChuteTexture)) { this.templateGui.chuteId = this.Textures.GetCanopyIndex(parameters.ChuteTexture); }
                if (this.Textures.ContainsModel(parameters.ModelName)) { this.templateGui.modelId = this.Textures.GetModelIndex(parameters.ModelName); }
            }
            this.templateGui.TypeId = parameters.Type;
            this.templateGui.calcSelect = parameters.CalcSelect;
            this.templateGui.getMass = parameters.GetMass;
            this.templateGui.useDry = parameters.UseDry;
            this.templateGui.mass = parameters.Mass;
            this.templateGui.landingSpeed = parameters.LandingSpeed;
            this.templateGui.deceleration = parameters.Deceleration;
            this.templateGui.refDepAlt = parameters.RefDepAlt;
            this.templateGui.chuteCount = parameters.ChuteCount;
        }

        //Initializes the chute
        public void Initialize()
        {
            this.parachute = this.pChute.rcModule.parachutes[this.id];
            this.pChute.materials.TryGetMaterial(this.parachute.material, ref this.material);
            this.templateGui.materialsId = this.pChute.materials.GetMaterialIndex(this.parachute.material);

            if (HighLogic.LoadedSceneIsEditor)
            {
                if (this.Textures != null)
                {
                    if (this.templateGui.chuteId == -1)
                    {
                        if (this.Textures.TryGetCanopy(this.CurrentCanopy, ref this.canopy))
                        {
                            this.templateGui.chuteId = this.Textures.GetCanopyIndex(this.canopy.Name);
                        }
                        else { this.templateGui.modelId = 0; }
                    }

                    if (this.templateGui.modelId == -1)
                    {
                        if (this.Textures.TryGetModel(this.parachute.parachuteName, ref this.model, true))
                        {
                            this.templateGui.modelId = this.Textures.GetModelIndex(this.model.Name);
                        }
                        else { this.templateGui.modelId = 0; }
                    }

                    if (this.templateGui.TypeId == -1)
                    {
                        int index = EnumUtils.IndexOf<ParachuteType>(this.CurrentType);
                        if (index != -1) { this.templateGui.TypeId = index; }
                        else { this.templateGui.TypeId = 0; }
                    }
                }
                else if (this.Textures != null)
                {
                    this.Textures.TryGetCanopy(this.templateGui.chuteId, ref this.canopy);
                    this.Textures.TryGetModel(this.templateGui.modelId, ref this.model);
                }

                this.templateGui.preDepDiam = this.parachute.preDeployedDiameter.ToString();
                this.templateGui.depDiam = this.parachute.deployedDiameter.ToString();
                this.templateGui.isPressure = this.parachute.minIsPressure;
                if (this.templateGui.isPressure) { this.templateGui.predepClause = this.parachute.minPressure.ToString(); }
                else { this.templateGui.predepClause = this.parachute.minDeployment.ToString(); }
                this.templateGui.deploymentAlt = this.parachute.deploymentAlt.ToString();
                this.templateGui.cutAlt = this.parachute.cutAlt == -1 ? string.Empty : this.parachute.cutAlt.ToString();
                this.templateGui.preDepSpeed = this.parachute.preDeploymentSpeed.ToString();
                this.templateGui.depSpeed = this.parachute.deploymentSpeed.ToString();
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
            node.TryGetValue("chuteID", ref this.templateGui.chuteId);
            node.TryGetValue("modelID", ref this.templateGui.modelId);
            if (node.TryGetValue("typeID", ref t)) { this.templateGui.TypeId = t; }
            if (node.TryGetValue("lastTypeID", ref t)) { this.templateGui.LastTypeId = t; }
            node.TryGetValue("position", ref this.position);
            node.TryGetValue("isPressure", ref this.templateGui.isPressure);
            node.TryGetValue("calcSelect", ref this.templateGui.calcSelect);
            node.TryGetValue("getMass", ref this.templateGui.getMass);
            node.TryGetValue("useDry", ref this.templateGui.useDry);
            node.TryGetValue("preDepDiam", ref this.templateGui.preDepDiam);
            node.TryGetValue("depDiam", ref this.templateGui.depDiam);
            node.TryGetValue("predepClause", ref this.templateGui.predepClause);
            node.TryGetValue("mass", ref this.templateGui.mass);
            node.TryGetValue("landingSpeed", ref this.templateGui.landingSpeed);
            node.TryGetValue("deceleration", ref this.templateGui.deceleration);
            node.TryGetValue("refDepAlt", ref this.templateGui.refDepAlt);
            node.TryGetValue("chuteCount", ref this.templateGui.chuteCount);
            node.TryGetValue("deploymentAlt", ref this.templateGui.deploymentAlt);
            node.TryGetValue("cutAlt", ref this.templateGui.cutAlt);
            node.TryGetValue("preDepSpeed", ref this.templateGui.preDepSpeed);
            node.TryGetValue("depSpeed", ref this.templateGui.depSpeed);
        }

        /// <summary>
        /// Saves this ChuteTemplate to a ConfigNode and returns it
        /// </summary>
        public ConfigNode Save()
        {
            ConfigNode node = new ConfigNode("CHUTE");
            node.AddValue("chuteID", this.templateGui.chuteId);
            node.AddValue("modelID", this.templateGui.modelId);
            node.AddValue("typeID", this.templateGui.TypeId);
            node.AddValue("lastTypeID", this.templateGui.LastTypeId);
            node.AddValue("position", this.position);
            node.AddValue("isPressure", this.templateGui.isPressure);
            node.AddValue("calcSelect", this.templateGui.calcSelect);
            node.AddValue("getMass", this.templateGui.getMass);
            node.AddValue("useDry", this.templateGui.useDry);
            node.AddValue("preDepDiam", this.templateGui.preDepDiam);
            node.AddValue("depDiam", this.templateGui.depDiam);
            node.AddValue("predepClause", this.templateGui.predepClause);
            node.AddValue("mass", this.templateGui.mass);
            node.AddValue("landingSpeed", this.templateGui.landingSpeed);
            node.AddValue("deceleration", this.templateGui.deceleration);
            node.AddValue("refDepAlt", this.templateGui.refDepAlt);
            node.AddValue("chuteCount", this.templateGui.chuteCount);
            node.AddValue("deploymentAlt", this.templateGui.deploymentAlt);
            node.AddValue("cutAlt", this.templateGui.cutAlt);
            node.AddValue("preDepSpeed", this.templateGui.preDepSpeed);
            node.AddValue("depSpeed", this.templateGui.depSpeed);
            return node;
        }
        #endregion
    }
}