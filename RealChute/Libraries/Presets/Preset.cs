using RealChute.Extensions;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more information contact me on the forum. */

namespace RealChute.Libraries
{
    public class Preset
    {
        public class ChuteParameters
        {
            #region Propreties
            private string _material = "Nylon";
            /// <summary>
            /// Material of the parachute
            /// </summary>
            public string material
            {
                get { return this._material; }
            }

            private string _preDeployedDiameter = "1";
            /// <summary>
            /// Predeployed diameter of the parachute
            /// </summary>
            public string preDeployedDiameter
            {
                get { return this._preDeployedDiameter; }
            }

            private string _deployedDiameter = "25";
            /// <summary>
            /// Deployed diameter of the parachute
            /// </summary>
            public string deployedDiameter
            {
                get { return this._deployedDiameter; }
            }

            private bool _minIsPressure = false;
            /// <summary>
            /// Wether the minimum deployment clause is pressure or altitude
            /// </summary>
            public bool minIsPressure
            {
                get { return this._minIsPressure; }
            }

            private string _minDeployment = "25000";
            /// <summary>
            /// Minimum predeployment altitude
            /// </summary>
            public string minDeployment
            {
                get { return this._minDeployment; }
            }

            private string _minPressure = "0.01";
            /// <summary>
            /// Minimum predeployment pressure
            /// </summary>
            public string minPressure
            {
                get { return this._minPressure; }
            }

            private string _deploymentAlt = "700";
            /// <summary>
            /// Full deployment altitude
            /// </summary>
            public string deploymentAlt
            {
                get { return this._deploymentAlt; }
            }

            private string _cutAlt = string.Empty;
            /// <summary>
            /// Autocut altitude
            /// </summary>
            public string cutAlt
            {
                get { return this._cutAlt; }
            }

            private string _preDeploymentSpeed = "2";
            /// <summary>
            /// Predeployment speed
            /// </summary>
            public string preDeploymentSpeed
            {
                get { return this._preDeploymentSpeed; }
            }

            private string _deploymentSpeed = "6";
            /// <summary>
            /// Deployment speed
            /// </summary>
            public string deploymentSpeed
            {
                get { return this._deploymentSpeed; }
            }

            private string _chuteTexture = string.Empty;
            /// <summary>
            /// GUI ID for the canopy texture
            /// </summary>
            public string chuteTexture
            {
                get { return this._chuteTexture; }
            }

            private string _modelName = string.Empty;
            /// <summary>
            /// GUI ID for the canopy model
            /// </summary>
            public string modelName
            {
                get { return this._modelName; }
            }

            private string _type = string.Empty;
            /// <summary>
            /// ID of the parachute type
            /// </summary>
            public string type
            {
                get { return this._type; }
            }

            private bool _calcSelect = false;
            /// <summary>
            /// GUI calculations mode
            /// </summary>
            public bool calcSelect
            {
                get { return this._calcSelect; }
            }

            private bool _getMass = true;
            /// <summary>
            /// GUI mass obtention
            /// </summary>
            public bool getMass
            {
                get { return this._getMass; }
            }

            private bool _useDry = true;
            /// <summary>
            /// GUI wet/dry mass
            /// </summary>
            public bool useDry
            {
                get { return this._useDry; }
            }

            private string _mass = "10";
            /// <summary>
            /// GUI mass field
            /// </summary>
            public string mass
            {
                get { return this._mass; }
            }

            private string _landingSpeed = "6";
            /// <summary>
            /// GUI landing speed field
            /// </summary>
            public string landingSpeed
            {
                get { return this._landingSpeed; }
            }

            private string _deceleration = "10";
            /// <summary>
            /// GUI deceleration field
            /// </summary>
            public string deceleration
            {
                get { return this._deceleration; }
            }

            private string _refDepAlt = "700";
            /// <summary>
            /// GUI mains deployment alt field
            /// </summary>
            public string refDepAlt
            {
                get { return this._refDepAlt; }
            }

            private string _chuteCount = "1";
            /// <summary>
            /// GUI chute count field
            /// </summary>
            public string chuteCount
            {
                get { return this._chuteCount; }
            }
            #endregion

            #region Constructor
            /// <summary>
            /// Creates a new ChuteParameters from the ConfigNode
            /// </summary>
            /// <param name="node">ConfigNode to create the object from</param>
            public ChuteParameters(ConfigNode node)
            {
                node.TryGetValue("material", ref _material);
                node.TryGetValue("preDeployedDiameter", ref _preDeployedDiameter);
                node.TryGetValue("deployedDiameter", ref _deployedDiameter);
                node.TryGetValue("minIsPressure", ref _minIsPressure);
                node.TryGetValue("minDeployment", ref _minDeployment);
                node.TryGetValue("minPressure", ref _minPressure);
                node.TryGetValue("deploymentAlt", ref _deploymentAlt);
                node.TryGetValue("cutAlt", ref _cutAlt);
                node.TryGetValue("preDeploymentSpeed", ref _preDeploymentSpeed);
                node.TryGetValue("deploymentSpeed", ref _deploymentSpeed);
                node.TryGetValue("chuteTexture", ref _chuteTexture);
                node.TryGetValue("modelName", ref _modelName);
                node.TryGetValue("type", ref _type);
                node.TryGetValue("calcSelect", ref _calcSelect);
                node.TryGetValue("getMass", ref _getMass);
                node.TryGetValue("useDry", ref _useDry);
                node.TryGetValue("mass", ref _mass);
                node.TryGetValue("landingSpeed", ref _landingSpeed);
                node.TryGetValue("deceleration", ref _deceleration);
                node.TryGetValue("refDepAlt", ref _refDepAlt);
                node.TryGetValue("chuteCount", ref _chuteCount);
            }

            /// <summary>
            /// Creates a ChuteParameters from the given ProceduralChute
            /// </summary>
            /// <param name="pChute"></param>
            /// <param name="secondary"></param>
            public ChuteParameters(ProceduralChute pChute, bool secondary)
            {
                ChuteTemplate temp = secondary ? pChute.secondary : pChute.main;
                this._material = MaterialsLibrary.instance.GetMaterial(temp.materialsID).name;
                this._preDeployedDiameter = temp.preDepDiam;
                this._deployedDiameter = temp.depDiam;
                this._minIsPressure = temp.isPressure;
                this._minDeployment = this.minIsPressure ? temp.parachute.minDeployment.ToString() : temp.predepClause;
                this._minPressure = this.minIsPressure ? temp.predepClause : temp.parachute.minPressure.ToString();
                this._deploymentAlt = temp.deploymentAlt;
                this._cutAlt = temp.cutAlt;
                this._preDeploymentSpeed = temp.preDepSpeed;
                this._deploymentSpeed = temp.depSpeed;
                if (pChute.textureLibrary != "none")
                {
                    if (pChute.textures.canopies.Count > 0) { this._chuteTexture = pChute.textures.GetCanopy(temp.chuteID).name; }
                    if (pChute.textures.models.Count > 0) { this._modelName = pChute.textures.GetModel(temp.modelID).name; }
                }
                this._type = RCUtils.types[temp.typeID];
                this._calcSelect = temp.calcSelect;
                this._getMass = temp.getMass;
                this._useDry = temp.useDry;
                this._mass = temp.mass;
                this._landingSpeed = temp.landingSpeed;
                this._deceleration = temp.deceleration;
                this._refDepAlt = temp.refDepAlt;
                this._chuteCount = temp.chuteCount;
            }
            #endregion

            #region Methods
            /// <summary>
            /// Saves this object to a config node
            /// </summary>
            /// <param name="secondary">Wether this is the main or secondary chute</param>
            public ConfigNode Save(bool secondary)
            {
                ConfigNode node = new ConfigNode((secondary ? "SECONDARY" : "MAIN"));
                node.AddValue("material", material);
                node.AddValue("preDeployedDiameter", preDeployedDiameter);
                node.AddValue("deployedDiameter", deployedDiameter);
                node.AddValue("minIsPressure", minIsPressure);
                node.AddValue("minDeployment", minDeployment);
                node.AddValue("minPressure", minPressure);
                node.AddValue("deploymentAlt", deploymentAlt);
                node.AddValue("cutAlt", cutAlt);
                node.AddValue("preDeploymentSpeed", preDeploymentSpeed);
                node.AddValue("deploymentSpeed", deploymentSpeed);
                node.AddValue("chuteTexture", chuteTexture);
                node.AddValue("modelName", modelName);
                node.AddValue("type", type);
                node.AddValue("calcSelect", calcSelect);
                node.AddValue("getMass", getMass);
                node.AddValue("useDry", useDry);
                node.AddValue("mass", mass);
                node.AddValue("landingSpeed", landingSpeed);
                node.AddValue("deceleration", deceleration);
                node.AddValue("refDepAlt", refDepAlt);
                node.AddValue("chuteCount", chuteCount);
                return node;
            }
            #endregion
        }

        #region Propreties
        private string _name = string.Empty;
        /// <summary>
        /// Name of the preset
        /// </summary>
        public string name
        {
            get { return this._name; }
        }

        private string _description = string.Empty;
        /// <summary>
        /// Description of the preset
        /// </summary>
        public string description
        {
            get { return this._description; }
        }

        private string _textureLibrary = "none";
        /// <summary>
        /// TextureLibrary for this part
        /// </summary>
        public string textureLibrary
        {
            get { return this._textureLibrary; }
        }

        private string _sizeID = string.Empty;
        /// <summary>
        /// Size ID for this chute
        /// </summary>
        public string sizeID
        {
            get { return this._sizeID; }
        }

        private string _cutSpeed = "0.5f";
        /// <summary>
        /// Autocut speed for the chute
        /// </summary>
        public string cutSpeed
        {
            get { return this._cutSpeed; }
        }

        private string _timer = "0s";
        /// <summary>
        /// Deployment timer for the chute
        /// </summary>
        public string timer
        {
            get { return this._timer; }
        }

        private bool _mustGoDown = false;
        /// <summary>
        /// MustGoDown clause for this preset
        /// </summary>
        public bool mustGoDown
        {
            get { return this._mustGoDown; }
        }

        private bool _deployOnGround = false;
        /// <summary>
        /// If this chute automatically deploys on ground contact or not
        /// </summary>
        public bool deployOnGround
        {
            get { return this._deployOnGround; }
        }

        private string _spares = "5";
        /// <summary>
        /// Amount of spare chutes available
        /// </summary>
        public string spares
        {
            get { return this._spares; }
        }

        private string _caseName = string.Empty;
        /// <summary>
        /// GUI ID for case texture
        /// </summary>
        public string caseName
        {
            get { return this._caseName; }
        }

        private string _bodyName = string.Empty;
        /// <summary>
        /// ID of the target planet
        /// </summary>
        public string bodyName
        {
            get { return this._bodyName; }
        }

        private ChuteParameters _main = null;
        /// <summary>
        /// Parameters for the main chute
        /// </summary>
        public ChuteParameters main
        {
            get { return this._main; }
        }

        private ChuteParameters _secondary = null;
        /// <summary>
        /// Parameters for the secondary chute
        /// </summary>
        public ChuteParameters secondary
        {
            get { return this._secondary; }
        }

        /// <summary>
        /// If this preset has a secondary chute
        /// </summary>
        public bool hasSecondary
        {
            get { return this.secondary != null; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new Preset from the given ConfigNode
        /// </summary>
        /// <param name="node">ConfigNode to create the object from</param>
        public Preset(ConfigNode node)
        {
            node.TryGetValue("name", ref _name);
            node.TryGetValue("description", ref _description);
            node.TryGetValue("textureLibrary", ref _textureLibrary);
            node.TryGetValue("sizeID", ref _sizeID);
            node.TryGetValue("cutSpeed", ref _cutSpeed);
            node.TryGetValue("timer", ref _timer);
            node.TryGetValue("mustGoDown", ref _mustGoDown);
            node.TryGetValue("deployOnGround", ref _deployOnGround);
            node.TryGetValue("spares", ref _spares);
            node.TryGetValue("caseName", ref _caseName);
            node.TryGetValue("bodyName", ref _bodyName);
            if (node.HasNode("MAIN")) { _main = new ChuteParameters(node.GetNode("MAIN")); }
            if (node.HasNode("SECONDARY")) { _secondary = new ChuteParameters(node.GetNode("SECONDARY")); }
        }

        /// <summary>
        /// Creates a new Preset from the given ProceduralChute
        /// </summary>
        /// <param name="pChute">ProceduralChute to create the object from</param>
        public Preset(ProceduralChute pChute)
        {
            this._name = pChute.presetName;
            this._description = pChute.presetDescription;
            this._textureLibrary = pChute.textureLibrary;
            if (pChute.sizes.Count > 0) { this._sizeID = pChute.sizes[pChute.size].sizeID; }
            this._cutSpeed = pChute.cutSpeed;
            this._timer = pChute.timer;
            this._mustGoDown = pChute.mustGoDown;
            this._deployOnGround = pChute.deployOnGround;
            this._spares = pChute.spares;
            if (textureLibrary != "none")
            {
                if (pChute.textures.cases.Count > 0) { this._caseName = pChute.parachuteCase.name; }
            }
            this._bodyName = pChute.body.bodyName;
            this._main = new ChuteParameters(pChute, false);
            if (pChute.secondaryChute) { this._secondary = new ChuteParameters(pChute, true); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Saves the preset to a ConfigNode
        /// </summary>
        public ConfigNode Save()
        {
            ConfigNode node = new ConfigNode("PRESET");
            node.AddValue("name", name);
            node.AddValue("description", description);
            node.AddValue("textureLibrary", textureLibrary);
            node.AddValue("sizeID", sizeID);
            node.AddValue("cutSpeed", cutSpeed);
            node.AddValue("timer", timer);
            node.AddValue("mustGoDown", mustGoDown);
            node.AddValue("deployOnGround", deployOnGround);
            node.AddValue("spares", spares);
            node.AddValue("caseName", caseName);
            node.AddValue("bodyName", bodyName);
            node.AddNode(main.Save(false));
            if (hasSecondary) { node.AddNode(secondary.Save(true)); }
            return node;
        }
        #endregion
    }
}
