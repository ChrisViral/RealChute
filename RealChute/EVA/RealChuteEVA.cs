using System;
using System.Linq;
using UnityEngine;
using RealChute.Extensions;
using RealChute.Utils;
using RealChute.Libraries.Materials;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute.EVA
{
    public class RealChuteEVA : PartModule
    {
        public class Canopy
        {
            #region Fields
            RealChuteEVA module = null;
            Part part = null;
            private string name = string.Empty;
            public string parachuteURL = string.Empty, textureURL = string.Empty;
            public string parachuteName = string.Empty, anchorName = string.Empty, capName = string.Empty;
            public string animationName = string.Empty, material = "Nylon";
            public DeploymentStates state = DeploymentStates.STOWED;
            public ParachuteMaterial mat = null;
            public float time = 0;
            public float deploymentSpeed = 4, baseSize = 10;
            public float deployedDiameter = 10, deploymentAlt = 1000;
            public Transform parachute = null, cap = null;
            public PhysicsWatch dragTimer = new PhysicsWatch(), randomTimer = new PhysicsWatch();
            public Quaternion lastRot = Quaternion.identity;
            internal float randomX, randomY, randomTime, buffer = 0;
            internal bool randomized = false;
            private Animation anim = null;
            #endregion

            #region Properties
            //Parachute deployed area
            public float deployedArea
            {
                get { return RCUtils.GetArea(this.deployedDiameter); }
            }

            //Mass of the chute
            public float chuteMass
            {
                get { return this.deployedArea * this.mat.areaDensity; }
            }

            //If the random deployment timer has been spent
            public bool randomDeployment
            {
                get
                {
                    if (!this.randomTimer.isRunning) { this.randomTimer.Start(); }

                    if (this.randomTimer.elapsed.TotalSeconds >= this.randomTime)
                    {
                        this.randomTimer.Reset();
                        return true;
                    }
                    return false;
                }
            }

            //If the parachute can deploy
            public bool canDeploy
            {
                get
                {
                    if (this.module.vessel.LandedOrSplashed || this.module.atmDensity == 0) { return false; }
                    else if (this.state == DeploymentStates.CUT) { return false; }
                    else if (this.module.trueAlt <= this.deploymentAlt && this.state == DeploymentStates.DEPLOYED) { return true; }
                    return false;
                }
            }

            //Position to apply the force to
            public Vector3 forcePosition
            {
                get { return this.parachute.position; }
            }
            #endregion

            #region Constructor
            public Canopy(RealChuteEVA module, ConfigNode node)
            {
                this.module = module;
                this.part = module.part;
                Load(node);
            }
            #endregion

            #region Methods
            //Initializes the parachute
            public void Initialize()
            {
                this.part.InitiateAnimation(this.animationName);
                this.anim = this.part.FindModelAnimators(this.capName).FirstOrDefault();
                
                if (HighLogic.LoadedSceneIsFlight)
                {
                    if (this.time != 0) { this.dragTimer = new PhysicsWatch(this.time); }

                    if (this.module.staged && this.state == DeploymentStates.DEPLOYED)
                    {
                        Deploy();
                    }
                    else if (this.state == DeploymentStates.CUT)
                    {
                        this.part.stackIcon.SetIconColor(XKCDColors.Red);
                        this.cap.gameObject.SetActive(false);
                    }

                }
            }

            //Initiates the canopy model
            public bool InitiateCanopy()
            {
                if (string.IsNullOrEmpty(this.parachuteURL) || string.IsNullOrEmpty(this.textureURL))
                {
                    RCUtils.LogError("Canopy model URL or texture URL null");
                    return false;
                }
                if (string.IsNullOrEmpty(this.parachuteName) || string.IsNullOrEmpty(this.anchorName) || string.IsNullOrEmpty(this.capName))
                {
                    RCUtils.LogError("Canopy component name null");
                    return false;
                }
                GameObject test = GameDatabase.Instance.GetModel(this.parachuteURL);
                Texture texture = GameDatabase.Instance.GetTexture(this.textureURL, false);
                if (test == null || texture == null)
                {
                    RCUtils.LogError("Canopy or texture null");
                    return false;
                }
                test.SetActive(true);
                GameObject chute = (GameObject)GameObject.Instantiate(test);
                GameObject.Destroy(test);
                chute.SetActive(true);
                chute.GetComponentsInChildren<Renderer>().ForEach(r => r.material.mainTexture = texture);
                Transform c = chute.transform.GetChild(0), anchor = this.part.FindModelTransform(this.anchorName);
                if (anchor == null)
                {
                    RCUtils.LogError("Anchor could not be found");
                    return false;
                }
                float scale = this.deployedDiameter / this.baseSize;
                c.localScale = new Vector3(scale, scale, scale);
                c.parent = anchor;
                c.position = anchor.position;
                c.localRotation = Quaternion.identity;
                this.parachute = this.part.FindModelTransform(this.parachuteName);
                this.cap = this.part.FindModelTransform(this.capName);
                if (this.parachute == null || this.cap == null)
                {
                    RCUtils.LogError("Parachute or cap could not be found");
                    return false;
                }

                return true;
            }

            //Gives a random movement to the parachute
            private void ParachuteNoise()
            {
                if (!this.randomized)
                {
                    this.randomX = (float)RCUtils.NextDouble(100);
                    this.randomY = (float)RCUtils.NextDouble(100);
                    this.randomized = true;
                }
                
                this.buffer += TimeWarp.fixedDeltaTime;
                this.parachute.Rotate(new Vector3(5 * (Mathf.PerlinNoise(this.buffer, this.randomX) - 0.5f), 5 * (Mathf.PerlinNoise(this.randomY, this.buffer) - 0.5f), 0));
            }

            //Makes the parachute follow drag direction
            private void FollowDragDirection()
            {
                Quaternion temp = this.lastRot;
                if (this.module.dragVector.sqrMagnitude > 0)
                {
                    temp = Quaternion.LookRotation(this.module.dragVector, this.parachute.up);
                }
                this.parachute.rotation = Quaternion.Lerp(temp, this.lastRot, 0.8f * TimeWarp.fixedDeltaTime);
                this.lastRot = temp;
                ParachuteNoise();
            }

            //Deploys the parachute
            public void Deploy()
            {
                this.part.Effect("evadeploy");
                this.module.status = "Deployed";
                this.parachute.gameObject.SetActive(true);
                this.state = DeploymentStates.DEPLOYED;
                this.lastRot = Quaternion.LookRotation(this.parachute.up, this.parachute.up);
                if (this.dragTimer.elapsedMilliseconds > 0)
                {
                    if (this.dragTimer.elapsed.TotalSeconds < this.deploymentSpeed)
                    {
                        this.part.SkipToAnimationTime(this.animationName, 1f / this.deploymentSpeed, (float)this.dragTimer.elapsed.TotalSeconds / this.deploymentSpeed);
                    }
                    else
                    {
                        this.part.SkipToAnimationEnd(this.animationName);
                    }
                }
                else
                {
                    this.part.PlayAnimation(this.animationName, 1f / this.deploymentSpeed);
                }
                this.dragTimer.Start();
                this.cap.gameObject.SetActive(false);
                this.module.deploy.active = false;
                this.module.cut.active = true;
            }

            //Cuts the parachute
            public void Cut(bool destroyed)
            {
                this.part.Effect("evacut");
                this.module.status = destroyed ? "Destroyed" : "Cut";
                this.parachute.gameObject.SetActive(false);
                this.state = DeploymentStates.CUT;
                this.module.cut.active = false;
                this.dragTimer.Reset();
                this.randomTimer.Reset();
                this.module.SwitchToReserve();
                this.module.DeactivateChute();
            }

            //Smoothly opens the parachute over time
            public float DragDeployment(float time, float debutArea, float endArea)
            {
                if (!this.dragTimer.isRunning) { this.dragTimer.Start(); }

                this.time = (float)this.dragTimer.elapsed.TotalSeconds;
                if (this.time <= time)
                {
                    float deploymentTime = Mathf.Exp(this.time - time) * (this.time / time);
                    return Mathf.Lerp(debutArea, endArea, deploymentTime);
                }
                return endArea;
            }

            //Calculates if the chute must be destroyed by aero or thermal forces
            public bool CalculateDestruction()
            {
                return false;
            }

            //Canopy function
            public void UpdateCanopy()
            {
                if (this.module.armed)
                {
                    if (this.canDeploy && this.randomDeployment) { this.module.armed = false; }
                }
                else
                {
                    if (this.canDeploy)
                    {
                        switch (this.state)
                        {
                            case DeploymentStates.STOWED:
                                {
                                    if (this.module.trueAlt < this.deploymentAlt) { Deploy(); }
                                    break;
                                }

                            case DeploymentStates.DEPLOYED:
                                {
                                    if (!CalculateDestruction()) { return; }
                                    this.part.Rigidbody.AddForceAtPosition(this.module.DragForce(0, this.deployedArea, this.deploymentSpeed), this.forcePosition, ForceMode.Force);
                                    FollowDragDirection();
                                    break;
                                }

                            default:
                                break;
                        }
                    }
                    else if (this.state == DeploymentStates.DEPLOYED) { this.module.GUICut(); }
                    else { this.module.DeactivateChute(); }
                }
            }
            #endregion

            #region Load/Save
            public void Load(ConfigNode node)
            {
                this.name = node.name;
                string depState = string.Empty;
                node.TryGetValue("parachuteURL", ref this.parachuteURL);
                node.TryGetValue("textureURL", ref this.textureURL);
                node.TryGetValue("parachuteName", ref this.parachuteName);
                node.TryGetValue("anchorName", ref this.anchorName);
                node.TryGetValue("capName", ref this.capName);
                node.TryGetValue("animationName", ref this.animationName);
                node.TryGetValue("material", ref this.material);
                node.TryGetValue("depState", ref depState);
                node.TryGetValue("deploymentSpeed", ref this.deploymentSpeed);
                node.TryGetValue("time", ref this.time);
                node.TryGetValue("baseSize", ref this.baseSize);
                node.TryGetValue("deployedDiameter", ref this.deployedDiameter);
                node.TryGetValue("deploymentAlt", ref this.deploymentAlt);

                MaterialsLibrary.instance.TryGetMaterial(this.material, out this.mat);
                this.state = EnumUtils.GetValue<DeploymentStates>(depState);
            }

            public ConfigNode Save()
            {
                ConfigNode node = new ConfigNode(this.name);
                node.AddValue("parachuteURL", this.parachuteURL);
                node.AddValue("textureURL", this.textureURL);
                node.AddValue("parachuteName", this.parachuteName);
                node.AddValue("anchorName", this.anchorName);
                node.AddValue("capName", this.capName);
                node.AddValue("animationName", this.animationName);
                node.AddValue("material", this.mat.name);
                node.AddValue("depState", EnumUtils.GetName(this.state));
                node.AddValue("deploymentSpeed", this.deploymentSpeed);
                node.AddValue("time", this.time);
                node.AddValue("baseSize", this.baseSize);
                node.AddValue("deployedDiameter", this.deployedDiameter);
                node.AddValue("deploymentAlt", this.deploymentAlt);
                return new ConfigNode();
            }
            #endregion
        }

        #region KSPFields
        [KSPField(isPersistant = true)]
        public string moduleID = string.Empty, moduleDescription = string.Empty;
        [KSPField(isPersistant = true)]
        public string backpackURL = string.Empty;
        [KSPField(isPersistant = true)]
        public bool initiated = false, staged = false, armed = false;
        [KSPField(isPersistant = true, guiActive = true, guiName = "Chute status")]
        public string status = "Stowed";
        #endregion

        #region Properties
        //Quick access to the part GUI events
        private BaseEvent _deploy = null, _arm = null, _disarm = null, _cut = null;
        private BaseEvent deploy
        {
            get
            {
                if (this._deploy == null) { this._deploy = Events["GUIDeploy"]; }
                return this._deploy;
            }
        }
        private BaseEvent arm
        {
            get
            {
                if (this._arm == null) { this._arm = Events["GUIArm"]; }
                return this._arm;
            }
        }
        private BaseEvent disarm
        {
            get
            {
                if (this._disarm == null) { this._disarm = Events["GUIDisarm"]; }
                return this._disarm;
            }
        }
        private BaseEvent cut
        {
            get
            {
                if (this._cut == null) { this._cut = Events["GUICut"]; }
                return this._cut;
            }
        }
        #endregion

        #region Fields
        public Canopy main = null, reserve = null;
        private Canopy current = null;
        private GameObject backpack = null;
        private Transform j = null;
        private ProtoCrewMember kerbal = null;
        public Vector3 dragVector = new Vector3();
        internal double ASL, trueAlt, atmDensity;
        internal float speed2;
        private GameObject[] jetpack = new GameObject[0];
        private RealChuteSettings settings = null;
        #endregion

        #region Part GUI
        [KSPEvent(guiActive = true, active = true, guiActiveUnfocused = false, guiName = "Deploy chute")]
        public void GUIDeploy()
        {
            ActivateChute();
        }

        [KSPEvent(guiActive = true, active = true, guiActiveUnfocused = false, guiName = "Arm chute")]
        public void GUIArm()
        {
            this.armed = true;
            this.status = "Armed";
            ActivateChute();
            this.arm.active = false;
            this.deploy.active = false;
            this.disarm.active = true;
        }

        [KSPEvent(guiActive = true, active = true, guiActiveUnfocused = false, guiName = "Disarm chute")]
        public void GUIDisarm()
        {
            this.armed = false;
            DeactivateChute();
            this.arm.active = true;
            this.deploy.active = false;
            this.disarm.active = false;
        }

        [KSPEvent(guiActive = true, active = true, guiActiveUnfocused = false, guiName = "Cut chute")]
        public void GUICut()
        {
            this.current.Cut(false);
        }

        [KSPEvent(guiActive = true, active = true, guiActiveUnfocused = false, guiName = "Dispose pack")]
        public void GUIDisposeChute()
        {
            PopupDialog.SpawnPopupDialog(new MultiOptionDialog("Are you sure you want to discard this EVA parachute backpack?",
                "Dispose backpack?", GUIUtils.skins,
                new DialogOption("Yes", DisposeBackpack, true),
                new DialogOption("No", () => { }, true)),
                false, GUIUtils.skins);
        }
        #endregion

        #region Methods
        //Activates the parachute
        public void ActivateChute()
        {
            this.staged = true;
            if (this.settings.autoArm) { this.armed = true; }
            RCUtils.Log(this.kerbal.name + "'s EVA chute was activated");
        }

        //Deactiates the parachute
        public void DeactivateChute()
        {
            this.staged = false;
            RCUtils.Log(this.kerbal.name + "'s EVA chute was deactivated");
        }

        //Tries to add the parachute to the Kerbal model
        private bool InitiateParachute()
        {
            if (string.IsNullOrEmpty(this.backpackURL))
            {
                RCUtils.LogError("The backpack's URL is null");
                return false;
            }
            
            GameObject test = GameDatabase.Instance.GetModel(this.backpackURL);
            if (test == null)
            {
                RCUtils.LogError("Could not find one of the components in the database");
                return false;
            }
            test.SetActive(true);
            SetJetpack(false);
            this.backpack = (GameObject)GameObject.Instantiate(test);
            this.backpack.SetActive(true);
            Transform b = this.backpack.transform.GetChild(0);
            b.localScale = Vector3.one;
            //Attach to the jetpack's collider to ensure movement with walking animation
            b.parent = j.GetChild(7);
            b.position = j.position;
            b.rotation = j.rotation;

            if (this.main.InitiateCanopy())
            {
                if (this.reserve != null)
                {
                    this.reserve.InitiateCanopy();
                }
                return true;
            }
            return false;
        }

        //Shows or hides the Jetpack
        private void SetJetpack(bool active)
        {
            this.jetpack.ForEach(t => t.SetActive(active));
        }

        //Gets the jetpack transforms to hide
        private void GetJetpackTransforms()
        {
            this.jetpack = new GameObject[6];
            //General jetpack transform
            this.j = this.part.transform.GetChild(2).GetChild(1);
            //Flag decals
            this.jetpack[0] = this.part.transform.GetChild(5).GetChild(1).gameObject;
            //Jetpack base
            this.jetpack[1] = j.GetChild(2).gameObject;
            //Fuel tank 1
            this.jetpack[2] = j.GetChild(3).gameObject;
            //Fuel tank 2
            this.jetpack[3] = j.GetChild(4).gameObject;
            //Thrusters left
            this.jetpack[4] = j.GetChild(5).gameObject;
            //Thrusters right
            this.jetpack[5] = j.GetChild(6).gameObject;
        }

        //Drag formula calculations
        public float DragCalculation(float area, float Cd)
        {
            return (float)this.atmDensity * this.speed2 * Cd * area / 2000f;
        }

        //Drag force vector
        private Vector3 DragForce(float startArea, float targetArea, float time)
        {
            return DragCalculation(this.current.DragDeployment(time, startArea, targetArea), this.current.mat.dragCoefficient) * this.dragVector * (this.settings.jokeActivated ? -1 : 1);
        }

        //Tries to switch the active chute to the reserve parachute
        public void SwitchToReserve()
        {
            if (this.reserve != null)
            {
                this.current = this.reserve;
                this.deploy.active = true;
                this.deploy.guiName = "Deploy reserve";
                this.arm.active = true;
                this.arm.guiName = "Arm reserve";
                this.disarm.guiName = "Disarm reserve";
                this.cut.guiName = "Cut reserve";
            }
        }

        //Removes the backpack and module from the Kerbal
        public void DisposeBackpack()
        {
            GameObject.Destroy(this.backpack);
            SetJetpack(true);
            this.part.RemoveModule(this);
        }
        #endregion

        #region Functions
        private void FixedUpdate()
        {

            if (!CompatibilityChecker.IsAllCompatible() || FlightGlobals.ActiveVessel == null || this.part.Rigidbody == null) { return; }
            this.ASL = FlightGlobals.getAltitudeAtPos(this.part.transform.position);
            this.trueAlt = this.vessel.GetTrueAlt(ASL);
            this.atmDensity = this.vessel.mainBody.GetDensityAtAlt(ASL, this.vessel.atmosphericTemperature);
            Vector3 velocity = this.part.Rigidbody.velocity + Krakensbane.GetFrameVelocityV3f();
            this.speed2 = velocity.sqrMagnitude;
            this.dragVector = -velocity.normalized;
            
            if (this.staged)
            {
                current.UpdateCanopy();
            }
        }
        #endregion

        #region Overrides
        public override void OnStart(PartModule.StartState state)
        {
            if (!CompatibilityChecker.IsAllCompatible())
            {
                Events.ForEach(e =>
                {
                    e.active = false;
                    e.guiActive = false;
                });
                Fields["status"].guiActive = false;
                return;
            }

            GetJetpackTransforms();
            if (this.main == null || !InitiateParachute())
            {
                this.part.RemoveModule(this);
                return;
            }

            this.kerbal = this.part.protoModuleCrew[0];
            this.settings = RealChuteSettings.fetch;
            this.main.Initialize();
            float m = 0.09f + this.main.chuteMass;
            if (this.reserve != null)
            {
                this.reserve.Initialize();
                m += this.reserve.chuteMass;
            }
            this.part.mass = m;
        }

        public override void OnLoad(ConfigNode node)
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }

            ConfigNode n = new ConfigNode();
            if (node.TryGetNode("EFFECTS", ref n))
            {
                this.part.LoadEffects(n);
            }
            if (node.TryGetNode("MAIN", ref n))
            {
                this.main = new Canopy(this, n);
                this.current = main;
            }
            if (node.TryGetNode("RESERVE", ref n))
            {
                this.reserve = new Canopy(this, n);
            }
        }

        public override void OnSave(ConfigNode node)
        {
            node.AddNode(this.main.Save());
            if (this.reserve != null)
            {
                node.AddNode(this.reserve.Save());
            }
        }
        #endregion
    }
}
