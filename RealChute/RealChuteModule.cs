using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RealChute.Extensions;
using RealChute.Libraries;
using Random = System.Random;

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
    public enum DeploymentStates
    {
        NONE,
        STOWED,
        LOWDEPLOYED,
        PREDEPLOYED,
        DEPLOYED,
        CUT
    }

    public class RealChuteModule : PartModule, IPartCostModifier, IModuleInfo
    {
        #region Persistent fields
        // Values from the .cfg file
        [KSPField(isPersistant = true)]
        public float caseMass = 0.1f;
        [KSPField(isPersistant = true)]
        public float cutSpeed = 0.5f;
        [KSPField(isPersistant = true)]
        public float timer = 0;
        [KSPField(isPersistant = true)]
        public bool mustGoDown = false;
        [KSPField(isPersistant = true)]
        public bool deployOnGround = false;
        [KSPField(isPersistant = true)]
        public float spareChutes = 5;
        [KSPField(isPersistant = true)]
        public bool initiated = false;
        [KSPField(isPersistant = true)]
        public bool wait = true, armed = false, oneWasDeployed = false;
        [KSPField(isPersistant = true)]
        public bool staged = false, launched = false;
        [KSPField(isPersistant = true, guiActive = true, guiName = "Spare chutes")]
        public int chuteCount = 5;
        [KSPField]
        public bool reverseOrientation = false;
        #endregion

        #region Propreties
        // If the vessel is stopped on the ground
        public bool groundStop
        {
            get { return this.vessel.LandedOrSplashed && this.vessel.horizontalSrfSpeed < cutSpeed; }
        }

        // If both parachutes must cut
        public bool allMustStop
        {
            get { return this.secondaryChute && (this.groundStop || this.atmPressure == 0) && this.parachutes.Exists(p => p.deploymentState == DeploymentStates.CUT); }
        }

        // If the parachute can be repacked
        public bool canRepack
        {
            get
            {
                return (this.groundStop || this.atmPressure == 0) && this.parachutes.Exists(p => p.deploymentState == DeploymentStates.CUT)
                    && this.parachutes.TrueForAll(p => p.deploymentState == DeploymentStates.CUT || p.deploymentState == DeploymentStates.STOWED)
                    && (this.chuteCount > 0 || this.chuteCount == -1);
            }
        }

        //If any parachute is deployed
        public bool anyDeployed
        {
            get { return this.parachutes.Exists(p => p.isDeployed); }
        }

        // Wether multiple parachutes are deployed or not
        public bool manyDeployed
        {
            get { return this.parachutes.Count(p => p.isDeployed) > 1; }
        }

        //If there is more than one parachute on the part
        public bool secondaryChute
        {
            get { return this.parachutes.Count > 1; }
        }

        //Quick access to the part GUI events
        private BaseEvent _deploy = null, _arm = null, _disarm = null, _cut = null, _repack = null;
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
        private BaseEvent repack
        {
            get
            {
                if (this._repack == null) { this._repack = Events["GUIRepack"]; }
                return this._repack;
            }
        }
        #endregion

        #region Fields
        //Module
        internal Vector3 dragVector = new Vector3(), pos = new Vector3d();
        private PhysicsWatch deploymentTimer = new PhysicsWatch(), failedTimer = new PhysicsWatch(), launchTimer = new PhysicsWatch();
        private bool displayed = false, showDisarm = false;
        internal double ASL, trueAlt;
        internal double atmPressure, atmDensity;
        internal float sqrSpeed;
        internal MaterialsLibrary materials = MaterialsLibrary.instance;
        private RealChuteSettings settings = null;
        public List<Parachute> parachutes = new List<Parachute>();
        public ConfigNode node = new ConfigNode();

        //GUI
        protected bool visible = false, hid = false;
        private int ID = Guid.NewGuid().GetHashCode();
        private GUISkin skins = HighLogic.Skin;
        private Rect window = new Rect(), drag = new Rect();
        private Vector2 scroll = new Vector2();
        #endregion

        #region Part GUI
        //Deploys the parachutes if possible
        [KSPEvent(guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Deploy Chute", unfocusedRange = 5)]
        public void GUIDeploy()
        {
            ActivateRC();
        }

        //Cuts main chute chute
        [KSPEvent(guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Cut main chute", unfocusedRange = 5)]
        public void GUICut()
        {
            this.parachutes.Where(p => p.isDeployed).ForEach(p => p.Cut());
        }

        //Arms parachutes
        [KSPEvent(guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Arm parachute", unfocusedRange = 5)]
        public void GUIArm()
        {
            this.armed = true;
            ActivateRC();
        }

        [KSPEvent(guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Disarm parachute", unfocusedRange = 5)]
        public void GUIDisarm()
        {
            this.armed = false;
            this.showDisarm = false;
            this.part.stackIcon.SetIconColor(XKCDColors.White);
            this.deploy.active = true;
            this.arm.active = true;
            DeactivateRC();
        }

        //Repacks chute from EVA if in space or on the ground
        [KSPEvent(guiActive = false, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Repack chute", unfocusedRange = 5)]
        public void GUIRepack()
        {
            if (canRepack)
            {
                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER && (!FlightGlobals.ActiveVessel.isEVA || FlightGlobals.ActiveVessel.VesselValues.RepairSkill.value < 1))
                {
                    ScreenMessages.PostScreenMessage("Only a lvl 1 and higher engineer can repack a parachute", 5, ScreenMessageStyle.UPPER_CENTER);
                    return;
                }

                this.part.Effect("rcrepack");
                this.repack.guiActiveUnfocused = false;
                this.oneWasDeployed = false;
                this.part.stackIcon.SetIconColor(XKCDColors.White);
                if (this.chuteCount != -1) { this.chuteCount--; }
                this.parachutes.Where(p => p.deploymentState == DeploymentStates.CUT).ForEach(p => p.Repack());
            }
        }

        //Shows the info window
        [KSPEvent(guiActive = true, active = true, guiActiveEditor = true, guiName = "Toggle info")]
        public void GUIToggleWindow()
        {
            if (!this.visible)
            {
                List<RealChuteModule> parachutes = new List<RealChuteModule>();
                if (HighLogic.LoadedSceneIsEditor) { parachutes.AddRange(EditorLogic.SortedShipList.Where(p => p.Modules.Contains("RealChuteModule")).Select(p => (RealChuteModule)p.Modules["RealChuteModule"])); }
                else if (HighLogic.LoadedSceneIsFlight) { parachutes.AddRange(this.vessel.FindPartModulesImplementing<RealChuteModule>()); }
                if (parachutes.Count > 1 && parachutes.Exists(p => p.visible))
                {
                    RealChuteModule module = parachutes.Find(p => p.visible);
                    this.window.x = module.window.x;
                    this.window.y = module.window.y;
                    module.visible = false;
                }
            }
            this.visible = !this.visible;
        }
        #endregion

        #region Action groups
        //Deploys the parachutes if possible
        [KSPAction("Deploy chute")]
        public void ActionDeploy(KSPActionParam param)
        {
            ActivateRC();
        }

        //Cuts main chute
        [KSPAction("Cut main chute")]
        public void ActionCut(KSPActionParam param)
        {
            if (this.parachutes.Exists(p => p.isDeployed)) { GUICut(); }
        }

        //Arms parachutes
        [KSPAction("Arm parachute")]
        public void ActionArm(KSPActionParam param)
        {
            GUIArm();
        }

        [KSPAction("Disarm parachute")]
        public void ActionDisarm(KSPActionParam param)
        {
            if (this.armed) { GUIDisarm(); }
        }
        #endregion

        #region Methods
        //Checks if there is a timer and/or a mustGoDown clause active
        public void CheckForWait()
        {
            bool timerSpent = true, goesDown = true;
            //Timer
            if (this.timer > 0 && this.deploymentTimer.elapsed.TotalSeconds < this.timer)
            {
                timerSpent = false;
                if (!this.deploymentTimer.isRunning) { this.deploymentTimer.Start(); }
                if (this.vessel.isActiveVessel)
                {
                    float time = this.timer - (float)this.deploymentTimer.elapsed.TotalSeconds;
                    if (time < 60) { ScreenMessages.PostScreenMessage(String.Format("Deployment in {0:0.0}s", time), Time.fixedDeltaTime, ScreenMessageStyle.UPPER_CENTER); }
                    else { ScreenMessages.PostScreenMessage(String.Format("Deployment in {0}", RCUtils.ToMinutesSeconds(time)), Time.fixedDeltaTime, ScreenMessageStyle.UPPER_CENTER); }
                }
            }
            else if (this.deploymentTimer.isRunning) { this.deploymentTimer.Stop(); }

            //Goes down
            if (this.mustGoDown && this.vessel.verticalSpeed > 0)
            {
                goesDown = false;
                if (this.vessel.isActiveVessel)
                {
                    ScreenMessages.PostScreenMessage(String.Format("Deployment awaiting negative vertical velocity\nCurrent vertical velocity: {0:0.0}/s", this.vessel.verticalSpeed), Time.fixedDeltaTime, ScreenMessageStyle.UPPER_CENTER);
                }
            }

            //Can deploy or not
            if (timerSpent && goesDown)
            {
                this.wait = false;
                this.showDisarm = false;
                this.deploymentTimer.Reset();
            }
            else
            {
                this.part.stackIcon.SetIconColor(XKCDColors.LightCyan);
                this.wait = true;
                this.showDisarm = true;
            }
        }

        //Activates the parachute
        public void ActivateRC()
        {
            this.staged = true;
            if (this.settings.autoArm) { this.armed = true; }
            print("[RealChute]: " + this.part.partInfo.name + " was activated in stage " + this.part.inverseStage);
        }

        //Deactiates the parachute
        public void DeactivateRC()
        {
            this.staged = false;
            if (this.vessel.LandedOrSplashed) { this.launched = false; }
            this.wait = true;
            print("[RealChute]: " + this.part.partInfo.name + " was deactivated");
        }

        //Copies stats from the info window to the symmetry counterparts
        private void CopyToCouterparts()
        {
            foreach(Part part in this.part.symmetryCounterparts)
            {
                RealChuteModule module = part.Modules["RealChuteModule"] as RealChuteModule;
                for (int i = 0; i < parachutes.Count; i++)
                {
                    Parachute current = parachutes[i], counterpart = module.parachutes[i];
                    counterpart.minIsPressure = current.minIsPressure;
                    counterpart.minPressure = current.minPressure;
                    counterpart.minDeployment = current.minDeployment;
                    counterpart.deploymentAlt = current.deploymentAlt;
                }
            }
        }

        //Deactivates the part
        public void StagingReset()
        {
            DeactivateRC();
            this.armed = false;
            if (this.part.inverseStage != 0) { this.part.inverseStage = this.part.inverseStage - 1; }
            else { this.part.inverseStage = Staging.CurrentStage; }
        }

        //Allows the chute to be repacked if available
        public void SetRepack()
        {
            this.part.stackIcon.SetIconColor(XKCDColors.Red);
            this.wait = true;
            StagingReset();
        }

        //Drag formula calculations
        public float DragCalculation(float area, float Cd)
        {
            return (float)this.atmDensity * this.sqrSpeed * Cd * area / 2000f;
        }

        //Loads all the parachutes into the list
        private void LoadParachutes()
        {
            if (this.parachutes.Count > 0 || !this.node.HasNode("PARACHUTE")) { return; }
            this.parachutes = new List<Parachute>(this.node.GetNodes("PARACHUTE").Select(n => new Parachute(this, n)));
        }

        //Gives the cost for this parachute
        public float GetModuleCost(float defaultCost)
        {
            return RCUtils.Round(this.parachutes.Sum(p => p.deployedArea * p.mat.areaCost));
        }

        //Not needed
        public Callback<Rect> GetDrawModulePanelCallback()
        {
            return null;
        }

        //Sets module info title
        public string GetModuleTitle()
        {
            return "RealChute";
        }

        //Sets part info field
        public string GetPrimaryField()
        {
            return "<b>Parachute count:</b> " + this.parachutes.Count;
        }

        //Event when the UI is hidden (F2)
        private void HideUI()
        {
            this.hid = true;
        }

        //Event when the UI is shown (F2)
        private void ShowUI()
        {
            this.hid = false;
        }
        #endregion

        #region Functions
        private void Update()
        {
            if (!CompatibilityChecker.IsAllCompatible() || !HighLogic.LoadedSceneIsFlight) { return; }

            //Makes the chute icon blink if failed
            if (this.failedTimer.isRunning)
            {
                double time = this.failedTimer.elapsed.TotalSeconds;
                if (time <= 2.5)
                {
                    if (!this.displayed)
                    {
                        ScreenMessages.PostScreenMessage("Parachute deployment failed.", 2.5f, ScreenMessageStyle.UPPER_CENTER);
                        if (groundStop) { ScreenMessages.PostScreenMessage("Reason: stopped on the ground.", 2.5f, ScreenMessageStyle.UPPER_CENTER); }
                        else if (atmPressure == 0) { ScreenMessages.PostScreenMessage("Reason: in space.", 2.5f, ScreenMessageStyle.UPPER_CENTER); }
                        else { ScreenMessages.PostScreenMessage("Reason: too high.", 2.5f, ScreenMessageStyle.UPPER_CENTER); }
                        this.displayed = true;
                    }
                    if (time < 0.5 || (time >= 1 && time < 1.5) || time >= 2) { this.part.stackIcon.SetIconColor(XKCDColors.Red); }
                    else { this.part.stackIcon.SetIconColor(XKCDColors.White); }
                }
                else
                {
                    this.displayed = false;
                    this.part.stackIcon.SetIconColor(XKCDColors.White);
                    this.failedTimer.Reset();
                }
            }

            this.disarm.active = (this.armed || this.showDisarm);
            bool canDeploy = (!this.staged && this.parachutes.Exists(p => p.deploymentState != DeploymentStates.CUT));
            this.deploy.active = canDeploy;
            this.arm.active = (!this.settings.autoArm && canDeploy);
            this.repack.guiActiveUnfocused = this.canRepack;
            this.cut.active = this.anyDeployed;
        }

        private void FixedUpdate()
        {
            //Flight values
            if (!CompatibilityChecker.IsAllCompatible() || !HighLogic.LoadedSceneIsFlight || FlightGlobals.ActiveVessel == null || this.part.Rigidbody == null) { return; }
            this.pos = this.part.transform.position;
            this.ASL = FlightGlobals.getAltitudeAtPos(this.pos);
            this.trueAlt = this.vessel.GetTrueAlt(ASL);
            this.atmPressure = this.vessel.mainBody.GetPressureAtAlt(ASL);
            this.atmDensity = this.vessel.mainBody.GetDensityAtAlt(ASL);
            Vector3 velocity = this.part.Rigidbody.velocity + Krakensbane.GetFrameVelocityV3f();
            sqrSpeed = velocity.sqrMagnitude;
            dragVector = -velocity.normalized;
            if (!this.staged && GameSettings.LAUNCH_STAGES.GetKeyDown() && this.vessel.isActiveVessel && (this.part.inverseStage == Staging.CurrentStage - 1 || Staging.CurrentStage == 0)) { ActivateRC(); }
            if (this.deployOnGround && !this.staged)
            {
                if (!this.launched && !this.vessel.LandedOrSplashed)
                {
                    if (!this.vessel.LandedOrSplashed)
                    {
                        //Dampening timer
                        if (!this.launchTimer.isRunning) { this.launchTimer.Start(); }
                        if (this.launchTimer.elapsedMilliseconds >= 3000)
                        {
                            this.launchTimer.Reset();
                            this.launched = true;
                        }
                    }
                    else if (this.launchTimer.isRunning) { launchTimer.Reset(); }
                }
                else if (this.launched && this.vessel.horizontalSrfSpeed >= cutSpeed && this.vessel.LandedOrSplashed) { ActivateRC(); }
            }
            
            if (this.staged)
            {
                //Checks if the parachute must disarm
                if (this.armed)
                {
                    this.part.stackIcon.SetIconColor(XKCDColors.LightCyan);
                    if (this.parachutes.Exists(p => p.canDeploy)) { this.armed = false; }
                }
                //Parachute deployments
                else
                {
                    if (this.wait)
                    {
                        CheckForWait();
                        if (this.wait) { return; }
                    }

                    //Parachutes
                    this.parachutes.ForEach(p => p.UpdateParachute());

                    //If all parachutes must be cut
                    if (this.allMustStop)
                    {
                        GUICut();
                        SetRepack();
                    }

                    //If the parachute can't be deployed
                    if (!this.oneWasDeployed && !this.settings.autoArm)
                    {
                        this.failedTimer.Start();
                        StagingReset();
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (!CompatibilityChecker.IsAllCompatible() || (!HighLogic.LoadedSceneIsFlight && !HighLogic.LoadedSceneIsEditor)) { return; }
            //Hide/show UI event removal
            GameEvents.onHideUI.Remove(HideUI);
            GameEvents.onShowUI.Remove(ShowUI);
        }
        #endregion

        #region Overrides
        public override void OnStart(PartModule.StartState state)
        {
            if (!HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight) { return; }
            if (!CompatibilityChecker.IsAllCompatible())
            {
                foreach (BaseAction a in Actions)
                {
                    a.active = false;
                }
                foreach (BaseEvent e in Events)
                {
                    e.active = false;
                    e.guiActive = false;
                    e.guiActiveEditor = false;
                }
                Fields["chuteCount"].guiActive = false;
                return;
            }

            //Staging icon
            this.part.stagingIcon = "PARACHUTES";

            //Autoarming checkup
            settings = RealChuteSettings.fetch;

            //Part GUI
            if (spareChutes < 0) { Fields["chuteCount"].guiActive = false; }
            if (!secondaryChute)
            {
                Actions["ActionCut"].guiName = "Cut chute";
                cut.guiName = "Cut chute";
            }
            Actions["ActionArm"].active = !settings.autoArm;

            //Initiates the Parachutes
            LoadParachutes();
            this.parachutes.ForEach(p => p.Initialize());

            //First initiation of the part
            if (!this.initiated)
            {
                this.initiated = true;
                this.armed = false;
                if (this.spareChutes >= 0) { this.chuteCount = (int)spareChutes; }
            }

            //Flight loading
            if (HighLogic.LoadedSceneIsFlight)
            {
                Random random = new Random();
                this.parachutes.ForEach(p => p.randomTime = (float)random.NextDouble());

                //Hide/show UI event addition
                GameEvents.onHideUI.Add(HideUI);
                GameEvents.onShowUI.Add(ShowUI);

                if (this.canRepack) { SetRepack(); }
            }

            //GUI
            this.window = new Rect(200, 100, 350, 400);
            this.drag = new Rect(0, 0, 350, 30);
        }

        public override void OnLoad(ConfigNode node)
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }

            //Gets the materials
            this.node = node;
            LoadParachutes();
            float chuteMass = this.parachutes.Sum(p => p.chuteMass);
            this.part.mass = this.caseMass + chuteMass;
        }

        public override string GetInfo()
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return string.Empty; }
            //Info in the editor part window
            float chuteMass = this.parachutes.Sum(p => p.mat.areaDensity * p.deployedArea);
            this.part.mass = this.caseMass + chuteMass;

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("Case mass: {0}\n", this.caseMass);
            if (this.timer > 0) { builder.AppendFormat("Deployment timer: {0}s\n", this.timer); }
            if (this.mustGoDown) { builder.AppendLine("Must go downwards to deploy: true"); }
            if (this.deployOnGround) { builder.AppendLine("Deploys on ground contact: true"); }
            if (this.spareChutes >= 0) { builder.AppendFormat("Spare chutes: {0}\n", this.spareChutes); }
            builder.AppendFormat("Autocut speed: {0}m/s\n", this.cutSpeed);

            if (!secondaryChute)
            {
                Parachute parachute = parachutes[0];
                builder.AppendFormat("Parachute material: {0}\n", parachute.material);
                builder.AppendFormat("Drag coefficient: {0:0.00}\n", parachute.mat.dragCoefficient);
                builder.AppendFormat("Predeployed diameter: {0}m\n", parachute.preDeployedDiameter);
                builder.AppendFormat("Deployed diameter: {0}m\n", parachute.deployedDiameter);
                if (!parachute.minIsPressure) { builder.AppendFormat("Minimum deployment altitude: {0}m\n", parachute.minDeployment); }
                else { builder.AppendFormat("Minimum deployment pressure: {0}atm\n", parachute.minPressure); }
                builder.AppendFormat("Deployment altitude: {0}m\n", parachute.deploymentAlt);
                builder.AppendFormat("Predeployment speed: {0}s\n", parachute.preDeploymentSpeed);
                builder.AppendFormat("Deployment speed: {0}s\n", parachute.deploymentSpeed);
                if (parachute.cutAlt >= 0) { builder.AppendFormat("Autocut altitude: {0}m", parachute.cutAlt); }
            }

            //In case of more than one chute
            else
            {
                builder.Append("Parachute materials: ").AppendJoin(parachutes.Select(p => p.material), ", ").AppendLine();
                builder.Append("Drag coefficients: ").AppendJoin(parachutes.Select(p => p.mat.dragCoefficient.ToString("0.00")), ", ").AppendLine();
                builder.Append("Predeployed diameters: ").AppendJoin(parachutes.Select(p => p.preDeployedDiameter.ToString()), "m, ").AppendLine("m");
                builder.Append("Deployed diameters: ").AppendJoin(parachutes.Select(p => p.deployedDiameter.ToString()), "m, ").AppendLine("m");
                builder.Append("Minimum deployment clauses: ").AppendJoin(parachutes.Select(p => p.minIsPressure ? p.minPressure + "atm" : p.minDeployment + "m"), ", ").AppendLine();
                builder.Append("Deployment altitudes: ").AppendJoin(parachutes.Select(p => p.deploymentAlt.ToString()), "m, ").AppendLine("m");
                builder.Append("Predeployment speeds: ").AppendJoin(parachutes.Select(p => p.preDeploymentSpeed.ToString()), "s, ").AppendLine("s");
                builder.Append("Deployment speeds: ").AppendJoin(parachutes.Select(p => p.deploymentSpeed.ToString()), "s, ").Append("s");
                if (parachutes.Exists(p => p.cutAlt != -1))
                {
                    builder.Append("\nAutocut altitudes: ").AppendJoin(parachutes.Select(p => p.cutAlt == -1 ? "-- " : p.cutAlt + "m"), ", ");
                }
            }
            return builder.ToString();
        }

        public override void OnSave(ConfigNode node)
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            //Saves the parachutes to the persistence
            this.parachutes.ForEach(p => node.AddNode(p.Save()));
        }
        #endregion

        #region GUI
        private void OnGUI()
        {
            //Handles GUI rendering
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
            {
                //Info window visibility
                if (this.visible && !this.hid)
                {
                    this.window = GUILayout.Window(this.ID, this.window, Window, "RealChute Info Window " + RCUtils.assemblyVersion, this.skins.window);
                }
            }
        }

        //Info window
        private void Window(int id)
        {
            //Header
            GUI.DragWindow(this.drag);
            GUILayout.BeginVertical();

            //Top info labels
            StringBuilder builder = new StringBuilder("Part name: ").AppendLine(this.part.partInfo.title);
            builder.Append("Symmetry counterparts: ").AppendLine(this.part.symmetryCounterparts.Count.ToString());
            builder.Append("Part mass: ").Append(this.part.TotalMass().ToString("0.###")).Append("t");
            GUILayout.Label(builder.ToString(), this.skins.label);

            //Beggining scroll
            this.scroll = GUILayout.BeginScrollView(this.scroll, false, false, this.skins.horizontalScrollbar, this.skins.verticalScrollbar, this.skins.box);
            GUILayout.Space(5);
            GUILayout.Label("General:", RCUtils.boldLabel, GUILayout.Width(120));

            //General labels
            builder = new StringBuilder("Autocut speed: ").Append(cutSpeed).AppendLine("m/s");
            if (this.timer >= 60) { builder.Append("Deployment timer: ").AppendLine(RCUtils.ToMinutesSeconds(this.timer)); }
            else if (this.timer > 0) { builder.Append("Deployment timer: ").Append(this.timer.ToString("0.#")).AppendLine("s"); }
            if (this.mustGoDown) { builder.AppendLine("Must go downwards to deploy"); }
            if (this.deployOnGround) { builder.AppendLine("Automatically deploys on ground contact"); }
            if (this.spareChutes >= 0) { builder.Append("Spare chutes: ").Append(chuteCount); }
            else { builder.Append("Spare chutes: inf"); }
            GUILayout.Label(builder.ToString(), this.skins.label);

            //Specific labels
            for (int i = 0; i < this.parachutes.Count; i++)
            {
                GUILayout.Label("___________________________________________", RCUtils.boldLabel);
                GUILayout.Space(3);
                GUILayout.Label(RCUtils.ParachuteNumber(i) + ":", RCUtils.boldLabel, GUILayout.Width(120));
                this.parachutes[i].UpdateGUI();
            }

            //End scroll
            GUILayout.EndScrollView();

            //Copy button if in flight
            if (HighLogic.LoadedSceneIsFlight && this.part.symmetryCounterparts.Count > 0)
            {
                RCUtils.CenteredButton("Copy to counterparts", CopyToCouterparts);
            }

            //Close button
            RCUtils.CenteredButton("Close", () => this.visible = false);

            //Closer
            GUILayout.EndVertical();
        }
        #endregion
    }
}
