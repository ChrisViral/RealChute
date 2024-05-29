using System;
using System.Collections.Generic;
using RealChute.Extensions;
using RealChute.Libraries.MaterialsLibrary;
using RealChute.Libraries.Presets;
using RealChute.Libraries.TextureLibrary;
using UnityEngine;
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
        //Current part
        private Part Part => this.pChute.part;

        //Selected CelestialBody
        public CelestialBody Body => this.pChute.body;

        //Parameters for this chute
        private ModelConfig.ModelParameters Parameters => this.model.Parameters[this.id];

        //Current TextureConfig
        private TextureConfig Textures => this.pChute.textures;

        //Current canopy for this chute
        public string CurrentCanopy
        {
            get
            {
                string[] canopies = RCUtils.ParseArray(this.pChute.currentCanopies);
                return canopies.IndexInRange(this.id) ? canopies[this.id] : string.Empty;
            }
        }

        //Current canopy model for this chute
        public string CurrentCanopyModel
        {
            get
            {
                string[] models = RCUtils.ParseArray(this.pChute.currentCanopyModels);
                return models.IndexInRange(this.id) ? models[this.id] : string.Empty;
            }
        }

        //Current type for this chute
        public string CurrentType
        {
            get
            {
                string[] chutes = RCUtils.ParseArray(this.pChute.currentTypes);
                return chutes.IndexInRange(this.id) ? chutes[this.id] : string.Empty;
            }
        }

        //If this part has more than one chute
        public bool Secondary => this.id != 0;

        //GUI
        public EditorGUI EditorGUI => this.pChute.editorGUI;
        #endregion

        #region Fields
        internal ProceduralChute pChute;
        internal Parachute parachute;
        internal TemplateGUI templateGUI = new TemplateGUI();
        internal MaterialDefinition material = new MaterialDefinition();
        internal CanopyConfig canopy = new CanopyConfig();
        internal ModelConfig model = new ModelConfig();
        public Vector3 position = Vector3.zero;
        public int id;
        #endregion

        #region Constructor
        /// <summary>
        ///     Creates an empty ChuteTemplate
        /// </summary>
        public ChuteTemplate() { }

        /// <summary>
        ///     Creates a new ChuteTemplate from the given ProceduralChute
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
            this.parachute.material = this.material.Name;
            this.parachute.mat = this.material;

            if (this.templateGUI.calcSelect)
            {
                double m = this.templateGUI.getMass ? this.pChute.GetCraftMass(this.templateGUI.useDry) : double.Parse(this.templateGUI.mass);
                double alt = 0, acc = 0;
                switch (this.templateGUI.Type)
                {
                    case ParachuteType.MAIN:
                    {
                        alt = double.Parse(this.pChute.landingAlt);
                        acc = this.Body.GeeASL * RCUtils.GeeToAcc;
                        break;
                    }
                    case ParachuteType.DROGUE:
                    {
                        alt = double.Parse(this.templateGUI.refDepAlt);
                        acc = this.Body.GeeASL * RCUtils.GeeToAcc;
                        break;
                    }
                    case ParachuteType.DRAG:
                    {
                        alt = double.Parse(this.pChute.landingAlt);
                        acc = double.Parse(this.templateGUI.deceleration);
                        break;
                    }
                }

                double density = this.Body.GetDensityAtAlt(alt, this.Body.GetMaxTemperatureAtAlt(alt));
                double speed = double.Parse(this.templateGUI.landingSpeed);
                speed *= speed;

                Debug.Log($"[RealChute]: {this.Part.partInfo.title} {RCUtils.ParachuteNumber(this.id)} - m: {m}t, alt: {alt}m, ρ: {density}kg/m³, v²: {speed}m²/s², a: {acc}m/s²");

                this.parachute.deployedDiameter = RCUtils.Round(Math.Sqrt((8000 * m * acc) / (Math.PI * speed * this.material.DragCoefficient * density * double.Parse(this.templateGUI.chuteCount))));
                float maxDiam = (this.Textures != null) && (this.Textures.Models.Count > 0) ? this.model.MaxDiam : 70f;
                if (this.parachute.deployedDiameter > this.model.MaxDiam)
                {
                    this.parachute.deployedDiameter = maxDiam;
                    this.EditorGUI.warning = true;
                }
                else { this.EditorGUI.warning = false; }
                this.parachute.preDeployedDiameter = RCUtils.Round(this.templateGUI.Type == ParachuteType.MAIN ? this.parachute.deployedDiameter / 20 : this.parachute.deployedDiameter / 2);
                Debug.Log($"[RealChute]: {this.Part.partInfo.title} {RCUtils.ParachuteNumber(this.id)} - depDiam: {this.parachute.deployedDiameter}m, preDepDiam: {this.parachute.preDeployedDiameter}m");
            }

            else
            {
                this.parachute.preDeployedDiameter = RCUtils.Round(float.Parse(this.templateGUI.preDepDiam));
                this.parachute.deployedDiameter = RCUtils.Round(float.Parse(this.templateGUI.depDiam));
                Debug.Log($"[RealChute]: {this.Part.partInfo.title} {RCUtils.ParachuteNumber(this.id)} - depDiam: {this.parachute.deployedDiameter}m, preDepDiam: {this.parachute.preDeployedDiameter}m");
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
                foreach (Part part in this.Part.symmetryCounterparts)
                {
                    Parachute sym = ((RealChuteModule)part.Modules["RealChuteModule"]).parachutes[this.id];
                    sym.material = this.material.Name;
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

                    TemplateGUI template = ((ProceduralChute)part.Modules["ProceduralChute"]).chutes[this.id].templateGUI;
                    template.chuteId = this.templateGUI.chuteId;
                    template.TypeId = this.templateGUI.TypeId;
                    template.modelId = this.templateGUI.modelId;
                    template.materialsId = this.templateGUI.materialsId;
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
            if (this.Textures == null) { return; }

            Texture2D texture = null;
            if (RealChuteSettings.Instance.ActivateNyan) { texture = GameDatabase.Instance.GetTexture(RCUtils.NyanTextureURL, false); }
            else if (this.Textures.TryGetCanopy(this.templateGUI.chuteId, ref this.canopy))
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
            if (this.Textures == null)
            {
                TryUpdateCanopyScaleFromModule();
                return;
            }

            if (this.Textures.TryGetModel(this.templateGUI.modelId, ref this.model))
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
                UpdateCanopyScale(test.transform, this.model.Count, this.model.Diameter);

                Transform toDestroy = this.parachute.parachute;
                GameObject obj = Object.Instantiate(test, toDestroy.parent, true);
                Object.Destroy(test);
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

        private void TryUpdateCanopyScaleFromModule()
        {
            if (this.parachute.referenceDiameter <= 0f || this.parachute.canopyCount <= 0) return;

            UpdateCanopyScale(this.parachute.parachute, this.parachute.canopyCount, this.parachute.referenceDiameter);
        }

        private void UpdateCanopyScale(Transform canopyTransform, int canopyCount, float referenceDiameter)
        {
            float scale = RCUtils.GetDiameter(this.parachute.DeployedArea / canopyCount) / referenceDiameter;
            canopyTransform.localScale = new Vector3(scale, scale, scale);
            Debug.Log($"[RealChute]: {this.Part.partInfo.title} {RCUtils.ParachuteNumber(this.id)} Scale: {scale}");
        }

        //Type switchup event
        internal void SwitchType()
        {
            if (this.pChute.secondaryChute)
            {
                if (this.templateGUI.materialsVisible && this.pChute.chutes.TrueForAll(c => !c.templateGUI.materialsVisible))
                {
                    this.EditorGUI.matX = (int)this.templateGUI.materialsWindow.x;
                    this.EditorGUI.matY = (int)this.templateGUI.materialsWindow.y;
                    this.pChute.chutes.Find(c => c.templateGUI.materialsVisible).templateGUI.materialsWindow.x = this.EditorGUI.matX;
                    this.pChute.chutes.Find(c => c.templateGUI.materialsVisible).templateGUI.materialsWindow.y = this.EditorGUI.matY;
                }
            }

            this.templateGUI.SwitchType();
        }

        //Applies the preset on the chute
        internal void ApplyPreset(Preset preset)
        {
            Preset.ChuteParameters parameters = preset.Parameters[this.id];
            this.material = MaterialsLibrary.Instance.GetMaterial(parameters.Material);
            this.templateGUI.materialsId = MaterialsLibrary.Instance.GetMaterialIndex(parameters.Material);
            this.templateGUI.preDepDiam = parameters.PreDeployedDiameter;
            this.templateGUI.depDiam = parameters.DeployedDiameter;
            this.templateGUI.isPressure = parameters.MinIsPressure;
            this.templateGUI.predepClause = this.templateGUI.isPressure ? parameters.MinPressure : parameters.MinDeployment;
            if (this.templateGUI.isPressure) { this.parachute.minDeployment = float.Parse(parameters.MinDeployment); }
            else { this.parachute.minPressure = float.Parse(parameters.MinPressure); }
            this.templateGUI.deploymentAlt = parameters.DeploymentAlt;
            this.templateGUI.cutAlt = parameters.CutAlt;
            this.templateGUI.preDepSpeed = parameters.PreDeploymentSpeed;
            this.templateGUI.depSpeed = parameters.DeploymentSpeed;
            if (this.Textures != null)
            {
                if (this.Textures.ContainsCanopy(parameters.ChuteTexture)) { this.templateGUI.chuteId = this.Textures.GetCanopyIndex(parameters.ChuteTexture); }
                if (this.Textures.ContainsModel(parameters.ModelName)) { this.templateGUI.modelId = this.Textures.GetModelIndex(parameters.ModelName); }
            }
            this.templateGUI.TypeId = parameters.Type;
            this.templateGUI.calcSelect = parameters.CalcSelect;
            this.templateGUI.getMass = parameters.GetMass;
            this.templateGUI.useDry = parameters.UseDry;
            this.templateGUI.mass = parameters.Mass;
            this.templateGUI.landingSpeed = parameters.LandingSpeed;
            this.templateGUI.deceleration = parameters.Deceleration;
            this.templateGUI.refDepAlt = parameters.RefDepAlt;
            this.templateGUI.chuteCount = parameters.ChuteCount;
        }

        //Initializes the chute
        public void Initialize()
        {
            this.parachute = this.pChute.rcModule.parachutes[this.id];
            MaterialsLibrary.Instance.TryGetMaterial(this.parachute.material, ref this.material);
            this.templateGUI.materialsId = MaterialsLibrary.Instance.GetMaterialIndex(this.parachute.material);

            if (HighLogic.LoadedSceneIsEditor)
            {
                if (this.Textures != null)
                {
                    if (this.templateGUI.chuteId == -1)
                    {
                        this.templateGUI.chuteId = this.Textures.TryGetCanopy(this.CurrentCanopy, ref this.canopy) ? this.Textures.GetCanopyIndex(this.canopy.Name) : 0;
                    }

                    if (this.templateGUI.modelId == -1)
                    {
                        if (this.Textures.TryGetModel(this.CurrentCanopyModel, ref this.model))
                        {
                            this.templateGUI.modelId = this.Textures.GetModelIndex(this.model.Name);
                        }
                        else
                        {
                            this.templateGUI.modelId = this.Textures.TryGetModel(this.parachute.parachuteName, ref this.model, true) ? this.Textures.GetModelIndex(this.model.Name) : 0;
                        }
                    }

                    if (this.templateGUI.TypeId == -1)
                    {
                        int index = EnumUtils.IndexOf<ParachuteType>(this.CurrentType);
                        this.templateGUI.TypeId = index != -1 ? index : 0;
                    }
                }
                else if (this.Textures != null)
                {
                    this.Textures.TryGetCanopy(this.templateGUI.chuteId, ref this.canopy);
                    this.Textures.TryGetModel(this.templateGUI.modelId, ref this.model);
                }

                this.templateGUI.preDepDiam = this.parachute.preDeployedDiameter.ToString();
                this.templateGUI.depDiam = this.parachute.deployedDiameter.ToString();
                this.templateGUI.isPressure = this.parachute.minIsPressure;
                this.templateGUI.predepClause = this.templateGUI.isPressure ? this.parachute.minPressure.ToString() : this.parachute.minDeployment.ToString();
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
        ///     Loads the ChuteTemplate from the given ConfigNode
        /// </summary>
        /// <param name="node">ConfigNode to load the object from</param>
        private void Load(ConfigNode node)
        {
            int t = 0;
            node.TryGetValue("chuteID", ref this.templateGUI.chuteId);
            node.TryGetValue("modelID", ref this.templateGUI.modelId);
            if (node.TryGetValue("typeID", ref t)) { this.templateGUI.TypeId = t; }
            if (node.TryGetValue("lastTypeID", ref t)) { this.templateGUI.LastTypeId = t; }
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
        ///     Saves this ChuteTemplate to a ConfigNode and returns it
        /// </summary>
        public ConfigNode Save()
        {
            ConfigNode node = new ConfigNode("CHUTE");
            node.AddValue("chuteID", this.templateGUI.chuteId);
            node.AddValue("modelID", this.templateGUI.modelId);
            node.AddValue("typeID", this.templateGUI.TypeId);
            node.AddValue("lastTypeID", this.templateGUI.LastTypeId);
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