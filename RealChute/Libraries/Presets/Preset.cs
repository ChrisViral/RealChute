using System.Collections.Generic;
using System.Linq;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute.Libraries.Presets
{
    public class Preset
    {
        public class ChuteParameters
        {
            #region Propreties
            private readonly string material = "Nylon";
            /// <summary>
            /// Material of the parachute
            /// </summary>
            public string Material
            {
                get { return this.material; }
            }

            private readonly string preDeployedDiameter = "1";
            /// <summary>
            /// Predeployed diameter of the parachute
            /// </summary>
            public string PreDeployedDiameter
            {
                get { return this.preDeployedDiameter; }
            }

            private readonly string deployedDiameter = "25";
            /// <summary>
            /// Deployed diameter of the parachute
            /// </summary>
            public string DeployedDiameter
            {
                get { return this.deployedDiameter; }
            }

            private readonly bool minIsPressure;
            /// <summary>
            /// Wether the minimum deployment clause is pressure or altitude
            /// </summary>
            public bool MinIsPressure
            {
                get { return this.minIsPressure; }
            }

            private readonly string minDeployment = "25000";
            /// <summary>
            /// Minimum predeployment altitude
            /// </summary>
            public string MinDeployment
            {
                get { return this.minDeployment; }
            }

            private readonly string minPressure = "0.01";
            /// <summary>
            /// Minimum predeployment pressure
            /// </summary>
            public string MinPressure
            {
                get { return this.minPressure; }
            }

            private readonly string deploymentAlt = "700";
            /// <summary>
            /// Full deployment altitude
            /// </summary>
            public string DeploymentAlt
            {
                get { return this.deploymentAlt; }
            }

            private readonly string cutAlt = string.Empty;
            /// <summary>
            /// Autocut altitude
            /// </summary>
            public string CutAlt
            {
                get { return this.cutAlt; }
            }

            private readonly string preDeploymentSpeed = "2";
            /// <summary>
            /// Predeployment speed
            /// </summary>
            public string PreDeploymentSpeed
            {
                get { return this.preDeploymentSpeed; }
            }

            private readonly string deploymentSpeed = "6";
            /// <summary>
            /// Deployment speed
            /// </summary>
            public string DeploymentSpeed
            {
                get { return this.deploymentSpeed; }
            }

            private readonly string chuteTexture = string.Empty;
            /// <summary>
            /// GUI ID for the canopy texture
            /// </summary>
            public string ChuteTexture
            {
                get { return this.chuteTexture; }
            }

            private readonly string modelName = string.Empty;
            /// <summary>
            /// GUI ID for the canopy model
            /// </summary>
            public string ModelName
            {
                get { return this.modelName; }
            }

            private readonly int type;
            /// <summary>
            /// ID of the parachute type
            /// </summary>
            public int Type
            {
                get { return this.type; }
            }

            private readonly bool calcSelect;
            /// <summary>
            /// GUI calculations mode
            /// </summary>
            public bool CalcSelect
            {
                get { return this.calcSelect; }
            }

            private readonly bool getMass = true;
            /// <summary>
            /// GUI mass obtention
            /// </summary>
            public bool GetMass
            {
                get { return this.getMass; }
            }

            private readonly bool useDry = true;
            /// <summary>
            /// GUI wet/dry mass
            /// </summary>
            public bool UseDry
            {
                get { return this.useDry; }
            }

            private readonly string mass = "10";
            /// <summary>
            /// GUI mass field
            /// </summary>
            public string Mass
            {
                get { return this.mass; }
            }

            private readonly string landingSpeed = "6";
            /// <summary>
            /// GUI landing speed field
            /// </summary>
            public string LandingSpeed
            {
                get { return this.landingSpeed; }
            }

            private readonly string deceleration = "10";
            /// <summary>
            /// GUI deceleration field
            /// </summary>
            public string Deceleration
            {
                get { return this.deceleration; }
            }

            private readonly string refDepAlt = "700";
            /// <summary>
            /// GUI mains deployment alt field
            /// </summary>
            public string RefDepAlt
            {
                get { return this.refDepAlt; }
            }

            private readonly string chuteCount = "1";
            /// <summary>
            /// GUI chute count field
            /// </summary>
            public string ChuteCount
            {
                get { return this.chuteCount; }
            }
            #endregion

            #region Constructor
            /// <summary>
            /// Creates a new ChuteParameters from the ConfigNode
            /// </summary>
            /// <param name="node">ConfigNode to create the object from</param>
            public ChuteParameters(ConfigNode node)
            {
                node.TryGetValue("material", ref this.material);
                node.TryGetValue("preDeployedDiameter", ref this.preDeployedDiameter);
                node.TryGetValue("deployedDiameter", ref this.deployedDiameter);
                node.TryGetValue("minIsPressure", ref this.minIsPressure);
                node.TryGetValue("minDeployment", ref this.minDeployment);
                node.TryGetValue("minPressure", ref this.minPressure);
                node.TryGetValue("deploymentAlt", ref this.deploymentAlt);
                node.TryGetValue("cutAlt", ref this.cutAlt);
                node.TryGetValue("preDeploymentSpeed", ref this.preDeploymentSpeed);
                node.TryGetValue("deploymentSpeed", ref this.deploymentSpeed);
                node.TryGetValue("chuteTexture", ref this.chuteTexture);
                node.TryGetValue("modelName", ref this.modelName);
                node.TryGetValue("type", ref this.type);
                node.TryGetValue("calcSelect", ref this.calcSelect);
                node.TryGetValue("getMass", ref this.getMass);
                node.TryGetValue("useDry", ref this.useDry);
                node.TryGetValue("mass", ref this.mass);
                node.TryGetValue("landingSpeed", ref this.landingSpeed);
                node.TryGetValue("deceleration", ref this.deceleration);
                node.TryGetValue("refDepAlt", ref this.refDepAlt);
                node.TryGetValue("chuteCount", ref this.chuteCount);
            }

            /// <summary>
            /// Creates a ChuteParameters from the given ProceduralChute
            /// </summary>
            public ChuteParameters(ProceduralChute pChute, ChuteTemplate chute)
            {
                TemplateGUI templateGui = chute.templateGUI;
                this.material = MaterialsLibrary.MaterialsLibrary.Instance.GetMaterial(templateGui.materialsId).Name;
                this.preDeployedDiameter = templateGui.preDepDiam;
                this.deployedDiameter = templateGui.depDiam;
                this.minIsPressure = templateGui.isPressure;
                this.minDeployment = this.MinIsPressure ? chute.parachute.minDeployment.ToString() : templateGui.predepClause;
                this.minPressure = this.MinIsPressure ? templateGui.predepClause : chute.parachute.minPressure.ToString();
                this.deploymentAlt = templateGui.deploymentAlt;
                this.cutAlt = templateGui.cutAlt;
                this.preDeploymentSpeed = templateGui.preDepSpeed;
                this.deploymentSpeed = templateGui.depSpeed;
                if (pChute.textureLibrary != "none")
                {
                    if (pChute.textures.Canopies.Count > 0) { this.chuteTexture = pChute.textures.GetCanopy(templateGui.chuteId).Name; }
                    if (pChute.textures.Models.Count > 0) { this.modelName = pChute.textures.GetModel(templateGui.modelId).Name; }
                }
                this.type = templateGui.TypeID;
                this.calcSelect = templateGui.calcSelect;
                this.getMass = templateGui.getMass;
                this.useDry = templateGui.useDry;
                this.mass = templateGui.mass;
                this.landingSpeed = templateGui.landingSpeed;
                this.deceleration = templateGui.deceleration;
                this.refDepAlt = templateGui.refDepAlt;
                this.chuteCount = templateGui.chuteCount;
            }
            #endregion

            #region Methods
            /// <summary>
            /// Saves this object to a config node
            /// </summary>
            public ConfigNode Save()
            {
                ConfigNode node = new ConfigNode("CHUTE");
                node.AddValue("material", this.Material);
                node.AddValue("preDeployedDiameter", this.PreDeployedDiameter);
                node.AddValue("deployedDiameter", this.DeployedDiameter);
                node.AddValue("minIsPressure", this.MinIsPressure);
                node.AddValue("minDeployment", this.MinDeployment);
                node.AddValue("minPressure", this.MinPressure);
                node.AddValue("deploymentAlt", this.DeploymentAlt);
                node.AddValue("cutAlt", this.CutAlt);
                node.AddValue("preDeploymentSpeed", this.PreDeploymentSpeed);
                node.AddValue("deploymentSpeed", this.DeploymentSpeed);
                node.AddValue("chuteTexture", this.ChuteTexture);
                node.AddValue("modelName", this.ModelName);
                node.AddValue("type", this.Type);
                node.AddValue("calcSelect", this.CalcSelect);
                node.AddValue("getMass", this.GetMass);
                node.AddValue("useDry", this.UseDry);
                node.AddValue("mass", this.Mass);
                node.AddValue("landingSpeed", this.LandingSpeed);
                node.AddValue("deceleration", this.Deceleration);
                node.AddValue("refDepAlt", this.RefDepAlt);
                node.AddValue("chuteCount", this.ChuteCount);
                return node;
            }
            #endregion
        }

        #region Propreties
        private readonly string name = string.Empty;
        /// <summary>
        /// Name of the preset
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        private readonly string description = string.Empty;
        /// <summary>
        /// Description of the preset
        /// </summary>
        public string Description
        {
            get { return this.description; }
        }

        private readonly string textureLibrary = string.Empty;
        /// <summary>
        /// TextureLibrary for this part
        /// </summary>
        public string TextureLibrary
        {
            get { return this.textureLibrary; }
        }

        private readonly string sizeId = string.Empty;
        /// <summary>
        /// Size ID for this chute
        /// </summary>
        public string SizeId
        {
            get { return this.sizeId; }
        }

        private readonly string cutSpeed = "0.5f";
        /// <summary>
        /// Autocut speed for the chute
        /// </summary>
        public string CutSpeed
        {
            get { return this.cutSpeed; }
        }

        private readonly string timer = "0s";
        /// <summary>
        /// Deployment timer for the chute
        /// </summary>
        public string Timer
        {
            get { return this.timer; }
        }

        private readonly bool mustGoDown;
        /// <summary>
        /// MustGoDown clause for this preset
        /// </summary>
        public bool MustGoDown
        {
            get { return this.mustGoDown; }
        }

        private readonly bool deployOnGround;
        /// <summary>
        /// If this chute automatically deploys on ground contact or not
        /// </summary>
        public bool DeployOnGround
        {
            get { return this.deployOnGround; }
        }

        private readonly string spares = "5";
        /// <summary>
        /// Amount of spare chutes available
        /// </summary>
        public string Spares
        {
            get { return this.spares; }
        }

        private readonly string landingAlt = "0";
        /// <summary>
        /// Planned landing altitude of the craft
        /// </summary>
        public string LandingAlt
        {
            get { return this.landingAlt; }
        }

        private readonly string caseName = string.Empty;
        /// <summary>
        /// GUI ID for case texture
        /// </summary>
        public string CaseName
        {
            get { return this.caseName; }
        }

        private readonly List<ChuteParameters> parameters;
        /// <summary>
        /// All parameters for potential chutes
        /// </summary>
        public List<ChuteParameters> Parameters
        {
            get { return this.parameters; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new Preset from the given ConfigNode
        /// </summary>
        /// <param name="node">ConfigNode to create the object from</param>
        public Preset(ConfigNode node)
        {
            node.TryGetValue("name", ref this.name);
            node.TryGetValue("description", ref this.description);
            node.TryGetValue("textureLibrary", ref this.textureLibrary);
            node.TryGetValue("sizeID", ref this.sizeId);
            node.TryGetValue("cutSpeed", ref this.cutSpeed);
            node.TryGetValue("timer", ref this.timer);
            node.TryGetValue("mustGoDown", ref this.mustGoDown);
            node.TryGetValue("deployOnGround", ref this.deployOnGround);
            node.TryGetValue("spares", ref this.spares);
            node.TryGetValue("landingAlt", ref this.landingAlt);
            node.TryGetValue("caseName", ref this.caseName);
            this.parameters = new List<ChuteParameters>(node.GetNodes("CHUTE").Select(n => new ChuteParameters(n)));
        }

        /// <summary>
        /// Creates a new Preset from the given ProceduralChute
        /// </summary>
        /// <param name="pChute">ProceduralChute to create the object from</param>
        public Preset(ProceduralChute pChute)
        {
            this.name = pChute.editorGUI.presetName;
            this.description = pChute.editorGUI.presetDescription;
            this.textureLibrary = pChute.textureLibrary;
            if (pChute.sizes.Count > 0) { this.sizeId = pChute.sizes[pChute.size].SizeId; }
            this.cutSpeed = pChute.cutSpeed;
            this.timer = pChute.timer;
            this.mustGoDown = pChute.mustGoDown;
            this.deployOnGround = pChute.deployOnGround;
            this.spares = pChute.spares;
            this.landingAlt = pChute.landingAlt;
            if (this.TextureLibrary != "none")
            {
                if (pChute.textures.Cases.Count > 0) { this.caseName = pChute.parachuteCase.Name; }
            }
            this.parameters = new List<ChuteParameters>(pChute.chutes.Select(c => new ChuteParameters(pChute, c)));
        }
        #endregion

        #region Methods
        /// <summary>
        /// Saves the preset to a ConfigNode
        /// </summary>
        public ConfigNode Save()
        {
            ConfigNode node = new ConfigNode("PRESET");
            node.AddValue("name", this.Name);
            node.AddValue("description", this.Description);
            node.AddValue("textureLibrary", this.TextureLibrary);
            node.AddValue("sizeID", this.SizeId);
            node.AddValue("cutSpeed", this.CutSpeed);
            node.AddValue("timer", this.Timer);
            node.AddValue("mustGoDown", this.MustGoDown);
            node.AddValue("deployOnGround", this.DeployOnGround);
            node.AddValue("spares", this.Spares);
            node.AddValue("landingAlt", this.LandingAlt);
            node.AddValue("caseName", this.CaseName);
            this.Parameters.ForEach(p => node.AddNode(p.Save()));
            return node;
        }
        #endregion
    }
}