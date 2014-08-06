using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using RealChute.Extensions;
using RealChute.Libraries;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more information contact me on the forum. */

namespace RealChute
{
    public enum DeploymentStates
    {
        STOWED,
        LOWDEPLOYED,
        PREDEPLOYED,
        DEPLOYED,
        CUT
    }

    public class RealChuteModule : PartModule, IPartCostModifier
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
        public bool initiated = false, capOff = false;
        [KSPField(isPersistant = true)]
        public bool wait = true, armed = false, oneWasDeployed = false;
        [KSPField(isPersistant = true)]
        public bool staged = false, launched = false;
        [KSPField(isPersistant = true)]
        public float baseDrag = 0.2f;
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
            get { return secondaryChute && (groundStop || atmPressure == 0) && parachutes.Any(p => p.deploymentState == DeploymentStates.CUT); }
        }

        // If the parachute can be repacked
        public bool canRepack
        {
            get { return (groundStop || atmPressure == 0) && parachutes.All(p => p.deploymentState == DeploymentStates.CUT || p.deploymentState == DeploymentStates.STOWED) && (chuteCount > 0 || chuteCount == -1); }
        }

        //If any parachute is deployed
        public bool anyDeployed
        {
            get { return parachutes.Any(p => p.isDeployed); }
        }

        // Wether multiple parachutes are deployed or not
        public bool manyDeployed
        {
            get { return parachutes.Count(p => p.isDeployed) > 1; }
        }

        //If there is more than one parachute on the part
        public bool secondaryChute
        {
            get { return parachutes.Count > 1; }
        }
        #endregion

        #region Fields
        //Module
        internal Vector3 dragVector = new Vector3(), pos = new Vector3d();
        private Stopwatch deploymentTimer = new Stopwatch(), failedTimer = new Stopwatch(), launchTimer = new Stopwatch();
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
        private Rect window = new Rect();
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
            parachutes.Where(p => p.isDeployed).ToList().ForEach(p => p.Cut());
        }

        //Arms parachutes
        [KSPEvent(guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Arm parachute", unfocusedRange = 5)]
        public void GUIArm()
        {
            armed = true;
            ActivateRC();
        }

        [KSPEvent(guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Disarm parachute", unfocusedRange = 5)]
        public void GUIDisarm()
        {
            armed = false;
            showDisarm = false;
            this.part.stackIcon.SetIconColor(XKCDColors.White);
            Events["GUIDeploy"].active = true;
            Events["GUIArm"].active = true;
            DeactivateRC();
        }

        //Repacks chute from EVA if in space or on the ground
        [KSPEvent(guiActive = false, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Repack chute", unfocusedRange = 5)]
        public void GUIRepack()
        {
            if (canRepack)
            {
                this.part.Effect("rcrepack");
                Events["GUIRepack"].guiActiveUnfocused = false;
                Events["GUIDeploy"].active = true;
                Events["GUIArm"].active = true;
                oneWasDeployed = false;
                this.part.stackIcon.SetIconColor(XKCDColors.White);
                capOff = false;
                if (chuteCount != -1) { chuteCount--; }
                parachutes.Where(p => p.deploymentState == DeploymentStates.CUT).ToList().ForEach(p => p.Repack());
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
                if (parachutes.Count > 1 && parachutes.Any(p => p.visible))
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
            if (parachutes.Any(p => p.isDeployed)) { parachutes.Where(p => p.isDeployed).ToList().ForEach(p => p.Cut()); }
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
            if (armed) { GUIDisarm(); }
        }
        #endregion

        #region Methods
        //Checks if there is a timer and/or a mustGoDown clause active
        public void CheckForWait()
        {
            bool timerSpent = true, goesDown = true;
            //Timer
            if (timer > 0 && deploymentTimer.Elapsed.TotalSeconds < timer)
            {
                timerSpent = false;
                if (!deploymentTimer.IsRunning) { deploymentTimer.Start(); }
                if (this.vessel.isActiveVessel)
                {
                    float time = timer - (float)deploymentTimer.Elapsed.TotalSeconds;
                    if (time < 60) { ScreenMessages.PostScreenMessage(String.Format("Deployment in {0:0.0}s", time), Time.fixedDeltaTime, ScreenMessageStyle.UPPER_CENTER); }
                    else { ScreenMessages.PostScreenMessage(String.Format("Deployment in {0}", RCUtils.ToMinutesSeconds(time)), Time.fixedDeltaTime, ScreenMessageStyle.UPPER_CENTER); }
                }
            }
            else if (deploymentTimer.IsRunning) { deploymentTimer.Stop(); }

            //Goes down
            if (mustGoDown && this.vessel.verticalSpeed >= 0)
            {
                goesDown = false;
                if (this.vessel.isActiveVessel)
                {
                    ScreenMessages.PostScreenMessage("Deployment awaiting negative vertical velocity", Time.fixedDeltaTime, ScreenMessageStyle.UPPER_CENTER);
                    ScreenMessages.PostScreenMessage(String.Format("Current vertical velocity: {0:0.0}/s", this.vessel.verticalSpeed), Time.fixedDeltaTime, ScreenMessageStyle.UPPER_CENTER);
                }
            }

            //Can deploy or not
            if (timerSpent && goesDown)
            {
                wait = false;
                showDisarm = false;
                deploymentTimer.Reset();
            }
            else
            {
                this.part.stackIcon.SetIconColor(XKCDColors.LightCyan);
                wait = true;
                showDisarm = true;
            }
        }

        //Activates the parachute
        public void ActivateRC()
        {
            // Stop the timewarp instantly
            TimeWarp.SetRate(0, true);
            
            this.staged = true;
            if (settings.autoArm) { this.armed = true; }
            Events["GUIDeploy"].active = false;
            Events["GUIArm"].active = false;
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
            armed = false;
            if (this.part.inverseStage != 0) { this.part.inverseStage = this.part.inverseStage - 1; }
            else { this.part.inverseStage = Staging.CurrentStage; }
        }

        //Allows the chute to be repacked if available
        public void SetRepack()
        {
            this.part.stackIcon.SetIconColor(XKCDColors.Red);
            parachutes.ForEach(p => p.randomTimer.Reset());
            wait = true;
            StagingReset();
            if (chuteCount > 0 || chuteCount == -1) { Events["GUIRepack"].guiActiveUnfocused = true; }
        }

        //Drag formula calculations
        internal float DragCalculation(float area, float Cd)
        {
            return (float)atmDensity * sqrSpeed * Cd * area / 2000f;
        }

        //Loads all the parachutes into the list
        private void LoadParachutes()
        {
            if (this.parachutes.Count > 0 || !this.node.HasNode("PARACHUTE")) { return; }
            this.parachutes = new List<Parachute>(this.node.GetNodes("PARACHUTE").Select(n => new Parachute(this, n)));
        }

        //Gives the cost for this parachute
        public float GetModuleCost()
        {
            return RCUtils.Round(this.parachutes.Sum(p => p.deployedArea * p.mat.areaCost));
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
            if (!CompatibilityChecker.IsCompatible()) { return; }
            if (HighLogic.LoadedSceneIsFlight)
            {
                //Makes the chute icon blink if failed
                if (failedTimer.IsRunning)
                {
                    if (failedTimer.Elapsed.TotalSeconds <= 2.5)
                    {
                        if (!displayed)
                        {
                            ScreenMessages.PostScreenMessage("Parachute deployment failed.", 2, ScreenMessageStyle.UPPER_CENTER);
                            if (groundStop) { ScreenMessages.PostScreenMessage("Reason: stopped on the ground.", 2, ScreenMessageStyle.UPPER_CENTER); }
                            else if (atmPressure == 0) { ScreenMessages.PostScreenMessage("Reason: in space.", 2, ScreenMessageStyle.UPPER_CENTER); }
                            else { ScreenMessages.PostScreenMessage("Reason: too high.", 2, ScreenMessageStyle.UPPER_CENTER); }
                        }
                        displayed = true;
                        double time = failedTimer.Elapsed.TotalSeconds;
                        if (time < 0.5 || (time >= 1 && time < 1.5) || (time >= 2)) { this.part.stackIcon.SetIconColor(XKCDColors.Red); }
                        else { this.part.stackIcon.SetIconColor(XKCDColors.White); }
                    }
                    else
                    {
                        displayed = false;
                        this.part.stackIcon.SetIconColor(XKCDColors.White);
                        failedTimer.Reset();
                    }
                }

                if (settings.autoArm) { Events["GUIArm"].guiActive = false; }
                Events["GUIDisarm"].guiActive = (armed || showDisarm);
                Events["GUIDisarm"].guiActiveUnfocused = (armed || showDisarm);
            }

            if (HighLogic.LoadedSceneIsEditor)
            {
                //Updates the spare chute count correctly
                chuteCount = (int)spareChutes;
                if (spareChutes < 0) { Fields["chuteCount"].guiActive = false; }

                //Calculates parachute mass
                this.part.mass = caseMass + parachutes.Sum(p => p.chuteMass);

                if (settings.autoArm) { Actions["ActionArm"].active = false; }
            }
        }

        private void FixedUpdate()
        {
            //Flight values
            if (!CompatibilityChecker.IsCompatible() || !HighLogic.LoadedSceneIsFlight || FlightGlobals.ActiveVessel == null || this.part.Rigidbody == null) { return; }
            pos = this.part.transform.position;
            ASL = FlightGlobals.getAltitudeAtPos(pos);
            trueAlt = this.vessel.GetTrueAlt(ASL);
            atmPressure = this.vessel.mainBody.GetPressureAtAlt(ASL);
            atmDensity = this.vessel.mainBody.GetDensityAtAlt(ASL);
            Vector3 velocity = this.part.Rigidbody.velocity + Krakensbane.GetFrameVelocityV3f();
            sqrSpeed = velocity.sqrMagnitude;
            dragVector = -velocity.normalized;
            if (!this.staged && GameSettings.LAUNCH_STAGES.GetKeyDown() && this.vessel.isActiveVessel && this.part.inverseStage == Staging.CurrentStage) { ActivateRC(); }
            if (this.deployOnGround && !this.staged)
            {
                if (!this.launched && !this.vessel.LandedOrSplashed)
                {
                    if (!this.vessel.LandedOrSplashed)
                    {
                        //Dampening timer
                        if (!this.launchTimer.IsRunning) { this.launchTimer.Start(); }
                        if (this.launchTimer.ElapsedMilliseconds >= 3000)
                        {
                            this.launchTimer.Reset();
                            this.launched = true;
                        }
                    }
                    else if (this.launchTimer.IsRunning) { launchTimer.Reset(); }
                }
                if (this.launched && !groundStop && this.vessel.LandedOrSplashed) { ActivateRC(); }
            }
            
            if (this.staged)
            {
                //Checks if the parachute must disarm
                if (armed)
                {
                    this.part.stackIcon.SetIconColor(XKCDColors.LightCyan);
                    if (parachutes.Any(p => p.canDeploy)) { armed = false; }
                }
                //Parachute deployments
                if (!armed)
                {
                    if (wait)
                    {
                        CheckForWait();
                        if (wait) { return; }
                    }

                    //Parachutes
                    parachutes.ForEach(p => p.UpdateParachute());

                    //If all parachutes must be cut
                    if (allMustStop)
                    {
                        GUICut();
                        SetRepack();
                    }

                    //If the parachute can't be deployed
                    if (!oneWasDeployed && !settings.autoArm)
                    {
                        failedTimer.Start();
                        StagingReset();
                        Events["GUIDeploy"].active = true;
                        Events["GUIArm"].active = true;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (!HighLogic.LoadedSceneIsFlight && !HighLogic.LoadedSceneIsEditor) { return; }
            //Hide/show UI event removal
            GameEvents.onHideUI.Remove(HideUI);
            GameEvents.onShowUI.Remove(ShowUI);
        }
        #endregion

        #region Overrides
        public override void OnStart(PartModule.StartState state)
        {
            if (!HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight) { return; }
            if (!CompatibilityChecker.IsCompatible())
            {
                foreach (BaseAction action in Actions)
                {
                    action.active = false;
                }
                foreach (BaseEvent evnt in Events)
                {
                    evnt.active = false;
                    evnt.guiActive = false;
                    evnt.guiActiveEditor = false;
                }
                Fields["chuteCount"].guiActive = false;
                return;
            }
            //Staging icon
            this.part.stagingIcon = "PARACHUTES";

            //Autoarming checkup
            settings = RealChuteSettings.fetch;

            //Part GUI
            Events["GUIDeploy"].active = true;
            Events["GUICut"].active = false;
            Events["GUIArm"].active = true;
            Events["GUIRepack"].guiActiveUnfocused = false;
            if (spareChutes < 0) { Fields["chuteCount"].guiActive = false; }
            if (!secondaryChute)
            {
                Actions["ActionCut"].guiName = "Cut chute";
                Events["GUICut"].guiName = "Cut chute";
            }

            if (settings.autoArm)
            {
                Events["GUIArm"].active = false;
                Actions["ActionArm"].active = false;
            }
            else
            {
                Events["GUIArm"].active = true;
                Actions["ActionArm"].active = true;
            }

            //Initiates the Parachutes
            LoadParachutes();
            parachutes.ForEach(p => p.Initialize());

            //First initiation of the part
            if (!initiated)
            {
                initiated = true;
                capOff = false;
                armed = false;
                this.baseDrag = this.part.maximum_drag;
                if (spareChutes >= 0) { chuteCount = (int)spareChutes; }
            }

            //Flight loading
            if (HighLogic.LoadedSceneIsFlight)
            {
                //If the part has been staged in the past
                if (capOff)
                {
                    this.part.stackIcon.SetIconColor(XKCDColors.Red);
                }
                System.Random random = new System.Random();
                parachutes.ForEach(p => p.randomTime = (float)random.NextDouble());

                //Hide/show UI event addition
                GameEvents.onHideUI.Add(HideUI);
                GameEvents.onShowUI.Add(ShowUI);
            }

            //GUI
            window = new Rect(200, 100, 350, 400);
        }

        public override void OnLoad(ConfigNode node)
        {
            if (!CompatibilityChecker.IsCompatible()) { return; }
            //Gets the materials
            this.node = node;
            LoadParachutes();
            float chuteMass = parachutes.Sum(p => p.mat.areaDensity * p.deployedArea);
            this.part.mass = caseMass + chuteMass;
        }

        public override string GetInfo()
        {
            if (!CompatibilityChecker.IsCompatible()) { return string.Empty; }
            //Info in the editor part window
            float chuteMass = parachutes.Sum(p => p.mat.areaDensity * p.deployedArea);
            this.part.mass = caseMass + chuteMass;

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("Case mass: {0}\n", caseMass);
            if (timer > 0) { builder.AppendFormat("Deployment timer: {0}s\n", timer); }
            if (mustGoDown) { builder.AppendLine("Must go downwards to deploy: true"); }
            if (deployOnGround) { builder.AppendLine("Deploys on ground contact: true"); }
            if (spareChutes >= 0) { builder.AppendFormat("Spare chutes: {0}\n", spareChutes); }
            builder.AppendFormat("Autocut speed: {0}m/s\n", cutSpeed);

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
                if (parachutes.Any(p => p.cutAlt != -1))
                {
                    builder.Append("\nAutocut altitudes: ").AppendJoin(parachutes.Select(p => p.cutAlt == -1 ? "-- " : p.cutAlt + "m"), ", ");
                }
            }
            return builder.ToString();
        }

        public override void OnActive()
        {
            if (!CompatibilityChecker.IsCompatible()) { return; }
            //Activates the part
            ActivateRC();
        }

        public override void OnSave(ConfigNode node)
        {
            //Saves the parachutes to the persistence
            parachutes.ForEach(p => node.AddNode(p.Save()));
        }
        #endregion

        #region GUI
        private void OnGUI()
        {
            //Handles GUI rendering
            if (!CompatibilityChecker.IsCompatible()) { return; }
            if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
            {
                //Info window visibility
                if (this.visible && !this.hid)
                {
                    this.window = GUILayout.Window(this.ID, this.window, Window, "RealChute Info Window " + RCUtils.assemblyVersion, skins.window);
                }
            }
        }

        //Info window
        private void Window(int id)
        {
            GUI.DragWindow(new Rect(0, 0, window.width, 30));
            GUILayout.BeginVertical();
            StringBuilder builder = new StringBuilder("Part name: ").AppendLine(this.part.partInfo.title);
            builder.Append("Symmetry counterparts: ").AppendLine(this.part.symmetryCounterparts.Count.ToString());
            builder.Append("Part mass: ").Append(this.part.TotalMass().ToString("0.###")).Append("t");
            GUILayout.Label(builder.ToString(), skins.label);
            scroll = GUILayout.BeginScrollView(scroll, false, false, skins.horizontalScrollbar, skins.verticalScrollbar, skins.box);
            GUILayout.Space(5);
            GUILayout.Label("General:", RCUtils.boldLabel, GUILayout.Width(120));
            builder = new StringBuilder("Autocut speed: ").Append(cutSpeed).AppendLine("m/s");
            if (timer >= 60) { builder.Append("Deployment timer: ").AppendLine(RCUtils.ToMinutesSeconds(timer)); }
            else if (timer > 0) { builder.Append("Deployment timer: ").Append(timer.ToString("0.#")).AppendLine("s"); }
            if (this.mustGoDown) { builder.AppendLine("Must go downwards to deploy"); }
            if (this.deployOnGround) { builder.AppendLine("Automatically deploys on ground contact"); }
            if (this.spareChutes >= 0) { builder.Append("Spare chutes: ").Append(chuteCount); }
            else { builder.Append("Spare chutes: inf"); }
            GUILayout.Label(builder.ToString(), skins.label);
            for (int i = 0; i < parachutes.Count; i++)
            {
                GUILayout.Label("___________________________________________", RCUtils.boldLabel);
                GUILayout.Space(3);
                GUILayout.Label(RCUtils.ParachuteNumber(i), RCUtils.boldLabel, GUILayout.Width(120));
                parachutes[i].UpdateGUI();
            }

                GUILayout.EndScrollView();
            if (HighLogic.LoadedSceneIsFlight && this.part.symmetryCounterparts.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Copy to counterparts", skins.button, GUILayout.Width(150)))
                {
                    CopyToCouterparts();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close", skins.button, GUILayout.Width(150)))
            {
                this.visible = false;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        #endregion
    }
}
