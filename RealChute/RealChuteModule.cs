using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
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

    public class RealChuteModule : PartModule
    {
        #region Config values
        // Values from the .cfg file
        [KSPField(isPersistant = true)]
        public float caseMass = 0.1f;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Autocut speed", guiFormat = "0.#"), UI_FloatRange(minValue = 0.5f, maxValue = 10, stepIncrement = 0.5f)]
        public float cutSpeed = 0.5f;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Timer", guiFormat = "0.#"), UI_FloatRange(minValue = 0, maxValue = 30, stepIncrement = 1)]
        public float timer = 0;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Must down"), UI_Toggle(enabledText = "True", disabledText = "False")]
        public bool mustGoDown = false;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Ground deploy"), UI_Toggle(enabledText = "True", disabledText = "False")]
        public bool deployOnGround = false;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Spares"), UI_FloatRange(minValue = -1, maxValue = 10, stepIncrement = 1)]
        public float spareChutes = 5;
        [KSPField]
        public bool secondaryChute = false;
        [KSPField]
        public bool reverseOrientation = false;
        [KSPField]
        public bool isTweakable = true;

        //Main parachute
        [KSPField(isPersistant = true)]
        public string material = "Nylon";
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Predep diam", guiFormat = "0.#"), UI_FloatRange(minValue = 0.5f, maxValue = 70, stepIncrement = 0.5f)]
        public float preDeployedDiameter = 1;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Dep diam", guiFormat = "0.#"), UI_FloatRange(minValue = 0.5f, maxValue = 70, stepIncrement = 0.5f)]
        public float deployedDiameter = 25;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Predep"), UI_Toggle(enabledText = "Pressure", disabledText = "Altitude")]
        public bool minIsPressure = false;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Predep alt"), UI_FloatRange(minValue = 50, maxValue = 50000, stepIncrement = 100)]
        public float minDeployment = 25000;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Predep press", guiFormat = "0.###"), UI_FloatRange(minValue = 0.005f, maxValue = 1, stepIncrement = 0.005f)]
        public float minPressure = 0.01f;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Dep alt"), UI_FloatRange(minValue = 0, maxValue = 10000, stepIncrement = 50)]
        public float deploymentAlt = 700;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Autocut alt"), UI_FloatRange(minValue = -50, maxValue = 10000, stepIncrement = 50)]
        public float cutAlt = -1;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Predep time"), UI_FloatRange(minValue = 1, maxValue = 10, stepIncrement = 1)]
        public float preDeploymentSpeed = 2;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Dep time"), UI_FloatRange(minValue = 1, maxValue = 10, stepIncrement = 1)]
        public float deploymentSpeed = 6;
        [KSPField]
        public string preDeploymentAnimation = "semiDeploy";
        [KSPField]
        public string deploymentAnimation = "fullyDeploy";
        [KSPField]
        public string parachuteName = "parachute";
        [KSPField]
        public string capName = "cap";
        [KSPField]
        public float forcedOrientation = 0;

        //Second parachute
        [KSPField(isPersistant = true)]
        public string secMaterial = "empty";
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Predep2 diam", guiFormat = "0.#"), UI_FloatRange(minValue = 0.5f, maxValue = 70, stepIncrement = 0.5f)]
        public float secPreDeployedDiameter = 1;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Dep2 diam", guiFormat = "0.#"), UI_FloatRange(minValue = 0.5f, maxValue = 70, stepIncrement = 0.5f)]
        public float secDeployedDiameter = 25;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Predep2"), UI_Toggle(enabledText = "Pressure", disabledText = "Altitude")]
        public bool secMinIsPressure = false;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Predep2 alt"), UI_FloatRange(minValue = 50, maxValue = 50000, stepIncrement = 100)]
        public float secMinDeployment = 25000;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Predep2 press", guiFormat = "0.###"), UI_FloatRange(minValue = 0.005f, maxValue = 1, stepIncrement = 0.005f)]
        public float secMinPressure = 0.01f;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Dep2 alt"), UI_FloatRange(minValue = 0, maxValue = 10000, stepIncrement = 50)]
        public float secDeploymentAlt = 700;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Autocut2 alt"), UI_FloatRange(minValue = -50, maxValue = 10000, stepIncrement = 50)]
        public float secCutAlt = -1;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Predep2 time"), UI_FloatRange(minValue = 1, maxValue = 10, stepIncrement = 1)]
        public float secPreDeploymentSpeed = 2;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Dep2 time"), UI_FloatRange(minValue = 1, maxValue = 10, stepIncrement = 1)]
        public float secDeploymentSpeed = 6;
        [KSPField]
        public string secPreDeploymentAnimation = "secSemiDeploy";
        [KSPField]
        public string secDeploymentAnimation = "secFullDeploy";
        [KSPField]
        public string secParachuteName = "secParachute";
        [KSPField]
        public string secCapName = "secCap";
        [KSPField]
        public float secForcedOrientation = 0;
        #endregion

        #region Persistant values
        [KSPField(isPersistant = true)]
        public bool initiated = false, capOff = false;
        [KSPField(isPersistant = true)]
        public bool wait = true, armed = false, oneWasDeployed = false;
        [KSPField(isPersistant = true)]
        public bool staged = false, launched = false;
        [KSPField(isPersistant = true)]
        public string depState = "STOWED", secDepState = "STOWED";
        [KSPField(isPersistant = true)]
        public float baseDrag = 0.2f;
        [KSPField(isPersistant = true, guiActive = true, guiName = "Spare chutes")]
        public int chuteCount = 5;
        #endregion

        #region Propreties
        // If the vessel is stopped on the ground
        public bool groundStop
        {
            get { return this.vessel.LandedOrSplashed && this.vessel.horizontalSrfSpeed < cutSpeed; }
        }

        // If both parachutes must cut
        public bool bothMustStop
        {
            get { return secondaryChute && (groundStop || atmPressure == 0) && (main.deploymentState == DeploymentStates.CUT || secondary.deploymentState == DeploymentStates.CUT); }
        }

        // If the parachute can be repacked
        public bool canRepack
        {
            get { return (groundStop || atmPressure == 0) && ((!secondaryChute && main.deploymentState == DeploymentStates.CUT) || (secondaryChute && main.deploymentState == DeploymentStates.CUT && secondary.deploymentState == DeploymentStates.CUT)) && (chuteCount > 0 || chuteCount == -1); }
        }

        // Wether both parachutes are deployed or not
        public bool bothDeployed
        {
            get { return main.isDeployed && secondary.isDeployed; }
        }

        //Converts The canopy diameter to an equivalent in stock drag
        public float areaToStock
        {
            get { return ((this.secondaryChute ? secondary.mat.dragCoefficient * secondary.deployedArea : 0) + (main.mat.dragCoefficient * main.deployedArea)) / (8 * this.part.TotalMass()); }
        }
        #endregion

        #region Fields
        //Module
        internal Vector3 dragVector = new Vector3(), pos = new Vector3d();
        private Animation anim = null;
        private Stopwatch deploymentTimer = new Stopwatch(), failedTimer = new Stopwatch();
        private bool displayed = false;
        internal double terrainAlt, ASL, trueAlt;
        internal double atmPressure, atmDensity;
        internal float sqrSpeed;
        internal MaterialsLibrary materials = MaterialsLibrary.instance;
        internal Parachute main = null, secondary = null;
        private RealChuteSettings settings = null;

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
            main.Cut();
        }

        //Cuts secondary chute
        [KSPEvent(guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Cut secondary chute", unfocusedRange = 5)]
        public void GUISecCut()
        {
            secondary.Cut();
        }

        //Cuts both chutes if possible
        [KSPEvent(guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Cut both chutes", unfocusedRange = 5)]
        public void GUICutBoth()
        {
            main.Cut();
            secondary.Cut();
            Events["GUICutBoth"].active = false;
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
                main.deploymentState = DeploymentStates.STOWED;
                depState = main.stateString;
                main.cap.gameObject.SetActive(true);
                this.part.stackIcon.SetIconColor(XKCDColors.White);
                capOff = false;
                if (chuteCount != -1) { chuteCount--; }

                if (secondaryChute)
                {
                    secondary.deploymentState = DeploymentStates.STOWED;
                    secDepState = secondary.stateString;
                    secondary.cap.gameObject.SetActive(true);
                }
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
            if (main.isDeployed) { main.Cut(); }
        }

        //Cuts secondary chute
        [KSPAction("Cut secondary chute")]
        public void ActionSecCut(KSPActionParam param)
        {
            if (secondary.isDeployed) { secondary.Cut(); }
        }

        //Cuts both cutes if possible
        [KSPAction("Cut both chutes")]
        public void ActionCutBoth(KSPActionParam param)
        {
            if (bothDeployed) { GUICutBoth(); }
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
                deploymentTimer.Reset();
            }
            else
            {
                this.part.stackIcon.SetIconColor(XKCDColors.LightCyan);
                wait = true;
            }
        }

        //Activates the parachute
        public void ActivateRC()
        {
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
            this.launched = false;
            print("[RealChute]: " + this.part.partInfo.name + " was deactivated");
        }

        //Copies stats from the info window to the symmetry counterparts
        private void CopyToCouterparts()
        {
            foreach(Part part in this.part.symmetryCounterparts)
            {
                RealChuteModule module = part.Modules["RealChuteModule"] as RealChuteModule;
                module.minIsPressure = this.minIsPressure;
                module.minPressure = this.minPressure;
                module.minDeployment = this.minDeployment;
                module.deploymentAlt = this.deploymentAlt;
                if (secondaryChute)
                {
                    module.secMinIsPressure = this.secMinIsPressure;
                    module.secMinPressure = this.secMinPressure;
                    module.secMinDeployment = this.secMinDeployment;
                    module.secDeploymentAlt = this.secDeploymentAlt;
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
            main.randomTimer.Reset();
            if (secondaryChute) { secondary.randomTimer.Reset(); }
            wait = true;
            StagingReset();
            if (chuteCount > 0 || chuteCount == -1) { Events["GUIRepack"].guiActiveUnfocused = true; }
        }

        //Drag formula calculations
        internal float DragCalculation(float area, float Cd)
        {
            return (float)atmDensity * sqrSpeed * Cd * area / 2000f;
        }
        #endregion

        #region Functions
        private void Update()
        {
            if ((HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor) && !CompatibilityChecker.IsCompatible()) { return; }
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

                //Hides the window if F2 is pressed
                if(HighLogic.LoadedSceneIsFlight && Input.GetKeyDown(KeyCode.F2))
                {
                    if (this.visible || this.hid)
                    {
                        this.visible = !this.visible;
                        this.hid = !this.hid;
                    }
                }

                if (settings.autoArm) { Events["GUIArm"].guiActive = false; }
                if (armed) { Events["GUIDisarm"].guiActive = true; }
                else { Events["GUIDisarm"].guiActive = false; }
            }

            if (HighLogic.LoadedSceneIsEditor)
            {
                //Tweakables pressure/altitude predeployment clauses
                if (!this.part.Modules.Contains("ProceduralChute") && isTweakable)
                {
                    //Main chute
                    if (this.minIsPressure)
                    {
                        Fields["minDeployment"].guiActiveEditor = false;
                        Fields["minPressure"].guiActiveEditor = true;
                    }
                    else
                    {
                        Fields["minDeployment"].guiActiveEditor = true;
                        Fields["minPressure"].guiActiveEditor = false;
                    }

                    //Secondary chute
                    if (this.secondaryChute)
                    {
                        if (this.secMinIsPressure)
                        {
                            Fields["secMinDeployment"].guiActiveEditor = false;
                            Fields["secMinPressure"].guiActiveEditor = true;
                        }
                        else
                        {
                            Fields["secMinDeployment"].guiActiveEditor = true;
                            Fields["secMinPressure"].guiActiveEditor = false;
                        }
                    }
                }

                //Updates the spare chute count correctly
                chuteCount = (int)spareChutes;
                if (spareChutes < 0) { Fields["chuteCount"].guiActive = false; }

                //Calculates parachute mass
                this.part.mass = caseMass + (secondaryChute ? main.chuteMass + secondary.chuteMass : main.chuteMass);

                if (settings.autoArm) { Actions["ActionArm"].active = false; }
            }
        }

        private void FixedUpdate()
        {
            //Flight values
            if (!CompatibilityChecker.IsCompatible() || !HighLogic.LoadedSceneIsFlight || FlightGlobals.ActiveVessel == null || this.part.Rigidbody == null) { return; }
            pos = this.part.transform.position;
            ASL = FlightGlobals.getAltitudeAtPos(pos);
            if (this.vessel.mainBody.pqsController != null)
            {
                terrainAlt = this.vessel.pqsAltitude;
                if (this.vessel.mainBody.ocean && terrainAlt < 0) { terrainAlt = 0; }
                trueAlt = ASL - terrainAlt;
            }
            else { terrainAlt = 0d; }
            atmPressure = FlightGlobals.getStaticPressure(pos);
            if (atmPressure <= 1E-6) { atmPressure = 0; atmDensity = 0; }
            else { atmDensity = RCUtils.GetDensityAtAlt(this.vessel.mainBody, ASL); }
            Vector3 velocity = this.part.Rigidbody.velocity + Krakensbane.GetFrameVelocityV3f();
            sqrSpeed = velocity.sqrMagnitude;
            dragVector = -velocity.normalized;
            if (!this.staged && GameSettings.LAUNCH_STAGES.GetKeyDown() && this.vessel.isActiveVessel && this.part.inverseStage == Staging.CurrentStage) { ActivateRC(); }
            if (!this.launched && !this.vessel.LandedOrSplashed) { this.launched = true; }
            if (this.launched && !this.staged && deployOnGround && !groundStop && this.vessel.LandedOrSplashed) { ActivateRC(); }
            
            if (this.staged)
            {
                //Checks if the parachute must disarm
                if (armed)
                {
                    this.part.stackIcon.SetIconColor(XKCDColors.LightCyan);
                    if (main.canDeploy || (secondaryChute && secondary.canDeploy)) { armed = false; }
                }
                //Parachute deployments
                if (!armed)
                {
                    CheckForWait();
                    main.UpdateParachute();
                    if (secondaryChute) { secondary.UpdateParachute(); }

                    //If both parachutes must be cut
                    if (bothMustStop)
                    {
                        if (main.isDeployed) { main.Cut(); }

                        if (secondary.isDeployed) { secondary.Cut(); }

                        SetRepack();
                    }

                    //Allows both parachutes to be cut at the same time if both are deployed
                    if (secondaryChute && bothDeployed) { Events["GUICutBoth"].active = true; }
                    else { Events["GUICutBoth"].active = false; }

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
                foreach (BaseField field in Fields)
                {
                    field.guiActive = false;
                    field.guiActiveEditor = false;
                }
                return;
            }
            //Staging icon
            this.part.stagingIcon = "PARACHUTES";

            //Autoarming checkup
            settings = RealChuteSettings.fetch;

            //Part GUI
            Events["GUIDeploy"].active = true;
            Events["GUICut"].active = false;
            Events["GUISecCut"].active = false;
            Events["GUIArm"].active = true;
            Events["GUICutBoth"].active = false;
            Events["GUIRepack"].guiActiveUnfocused = false;
            if (spareChutes < 0) { Fields["chuteCount"].guiActive = false; }
            if (!secondaryChute)
            {
                Actions["ActionSecCut"].active = false;
                Actions["ActionCutBoth"].active = false;
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

            //Tweakables tooltip
            if (this.part.Modules.Contains("ProceduralChute") || !isTweakable)
            {
                foreach (BaseField field in Fields)
                {
                    field.guiActiveEditor = false;
                }
            }
            else if (!secondaryChute)
            {
                Fields["secPreDeployedDiameter"].guiActiveEditor = false;
                Fields["secDeployedDiameter"].guiActiveEditor = false;
                Fields["secMinIsPressure"].guiActiveEditor = false;
                Fields["secMinDeployment"].guiActiveEditor = false;
                Fields["secMinPressure"].guiActiveEditor = false;
                Fields["secDeploymentAlt"].guiActiveEditor = false;
                Fields["secCutAlt"].guiActiveEditor = false;
                Fields["secPreDeploymentSpeed"].guiActiveEditor = false;
                Fields["secDeploymentSpeed"].guiActiveEditor = false;
            }

            //Initiates animations
            anim = this.part.FindModelAnimators(capName).FirstOrDefault();

            //Initiates the Parachutes
            main = new Parachute(this, false);
            if (secondaryChute) { secondary = new Parachute(this, true); }

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
                this.part.maximum_drag = this.baseDrag;
                //If the part has been staged in the past
                if (capOff)
                {
                    this.part.stackIcon.SetIconColor(XKCDColors.Red);
                }
                System.Random random = new System.Random();
                main.randomTime = (float)random.NextDouble();
                if (secondaryChute) { secondary.randomTime = (float)random.NextDouble(); }
            }

            //else if (HighLogic.LoadedSceneIsEditor && RCUtils.FARLoaded) { this.part.maximum_drag = areaToStock; }
            print(this.part.maximum_drag);

            //GUI
            window = new Rect(200, 100, 350, 400);
        }

        public override void OnLoad(ConfigNode node)
        {
            if (!CompatibilityChecker.IsCompatible()) { return; }
            //Gets the materials
            float chuteMass = materials.MaterialExists(material) ? materials.GetMaterial(material).areaDensity * deployedDiameter : 0;
            float secChuteMass = 0;
            if (secondaryChute && materials.MaterialExists(secMaterial)) { secChuteMass = materials.GetMaterial(secMaterial).areaDensity * secDeployedDiameter; }
            this.part.mass = caseMass + chuteMass + secChuteMass;
        }

        public override string GetInfo()
        {
            if (!CompatibilityChecker.IsCompatible()) { return string.Empty; }
            //Info in the editor part window
            MaterialDefinition mat = new MaterialDefinition(), secMat = new MaterialDefinition();
            float chuteMass = 0, secChuteMass = 0;
            if (materials.MaterialExists(material))
            {
                mat = materials.GetMaterial(material);
                chuteMass = mat.areaDensity * deployedDiameter;
            }
            if (secondaryChute && materials.MaterialExists(secMaterial))
            {
                secMat = materials.GetMaterial(secMaterial);
                secChuteMass = secMat.areaDensity * secDeployedDiameter;
            }
            this.part.mass = caseMass + chuteMass + secChuteMass;

            string infoList = string.Empty;
            infoList = String.Format("Parachute material: {0}\n", material);
            if (secondaryChute && secMaterial != material && secMaterial != "empty") { infoList += String.Format("Secondary parachute material: {0}\n", secMaterial); }
            infoList += String.Format("Case mass: {0}\n", caseMass);
            if (material == secMaterial) { infoList += String.Format("Drag coefficient: {0:0.00}\n", mat.dragCoefficient); }
            else { infoList += String.Format("Drag coefficients: {0:0.00}, {1:0.00}\n", mat.dragCoefficient, secMat.dragCoefficient); }
            if (timer > 0) { infoList += String.Format("Deployment timer: {0}s\n", timer); }
            if (mustGoDown) { infoList += "Must go downwards to deploy: true\n"; }
            if (deployOnGround) { infoList += "Deploys on ground contact: true\n"; }
            if (spareChutes >= 0) { infoList += String.Format("Spare chutes: {0}\n", spareChutes); }
            if (!secondaryChute)
            {
                infoList += String.Format("Predeployed diameter: {0}m\n", preDeployedDiameter);
                infoList += String.Format("Deployed diameter: {0}m\n", deployedDiameter);
                if (!minIsPressure) { infoList += String.Format("Minimum deployment altitude: {0}m\n", minDeployment); }
                else { infoList += String.Format("Minimum deployment pressure: {0}atm\n", minPressure); }
              
                infoList += String.Format("Deployment altitude: {0}m\n", deploymentAlt);
                infoList += String.Format("Predeployment speed: {0}s\n", preDeploymentSpeed);
                infoList += String.Format("Deployment speed: {0}s\n", deploymentSpeed);
                infoList += String.Format("Autocut speed: {0}m/s\n", cutSpeed);
                if (cutAlt >= 0) { infoList += String.Format("Autocut altitude: {0}m\n", cutAlt); }
            }

            //In case of dual chutes
            else
            {
                infoList += String.Format("Predeployed diameters: {0}m, {1}m\n", preDeployedDiameter, secPreDeployedDiameter);
                infoList += String.Format("Deployed diameters: {0}m, {1}m\n", deployedDiameter, secDeployedDiameter);
                if (!minIsPressure && !secMinIsPressure) { infoList += String.Format("Minimum deployment altitudes: {0}m, {1}m\n", minDeployment, secMinDeployment); }
                else if (minIsPressure && !secMinIsPressure) { infoList += String.Format("Minimum deployment clauses: {0}atm, {1}m\n", minDeployment, secMinDeployment); }
                else if (!minIsPressure && secMinIsPressure) { infoList += String.Format("Minimum deployment clauses: {0}m, {1}atm\n", minDeployment, secMinDeployment); }
                else if (minIsPressure && secMinIsPressure) { infoList += String.Format("Minimum deployment pressures: {0}atmm, {1}atm\n", minDeployment, secMinDeployment); }
                infoList += String.Format("Deployment altitudes: {0}m, {1}m\n", deploymentAlt, secDeploymentAlt);
                infoList += String.Format("Predeployment speeds: {0}s, {1}s\n", preDeploymentSpeed, secPreDeploymentSpeed);
                infoList += String.Format("Deployment speeds: {0}s, {1}s\n", deploymentSpeed, secDeploymentSpeed);
                infoList += String.Format("Autocut speed: {0}m/s\n", cutSpeed);
                if (cutAlt >= 0 && secCutAlt >= 0) { infoList += String.Format("Autocut altitudes: {0}m, {1}m\n", cutAlt, secCutAlt); }
                else if (cutAlt >= 0 && secCutAlt < 0) { infoList += String.Format("Main chute autocut altutude: {0}m\n", cutAlt); }
                else if (cutAlt < 0 && secCutAlt >= 0) { infoList += String.Format("Secondary chute autocut altitude: {0}m\n", secCutAlt); }
            }
            return infoList;
        }

        public override void OnActive()
        {
            if (!CompatibilityChecker.IsCompatible()) { return; }
            //Activates the part
            ActivateRC();
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
                if (this.visible)
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
            GUILayout.Label("Part: " + this.part.partInfo.title, skins.label);
            GUILayout.Label("Symmetry counterparts: " + this.part.symmetryCounterparts.Count, skins.label);
            GUILayout.Label("Mass: " + this.part.TotalMass().ToString("0.###"), skins.label);
            GUILayout.Space(5);
            scroll = GUILayout.BeginScrollView(scroll, false, false, skins.horizontalScrollbar, skins.verticalScrollbar, skins.box);
            GUILayout.Space(5);
            GUILayout.Label("General:", RCUtils.boldLabel, GUILayout.Width(120));
            GUILayout.Label("Autocut speed: " + cutSpeed + "m/s", skins.label);
            if (timer >= 60) { GUILayout.Label("Deployment timer: " + RCUtils.ToMinutesSeconds(timer), skins.label); }
            else { GUILayout.Label("Deployment timer: " + timer.ToString("0.#") + "s", skins.label); }
            GUILayout.Label("Must go down to deploy: " + mustGoDown.ToString(), skins.label);
            GUILayout.Label("Deploys on ground contact: " + deployOnGround.ToString(), skins.label);
            GUILayout.Label("Spare chutes: " + chuteCount, skins.label);
            GUILayout.Label("___________________________________________", RCUtils.boldLabel);
            GUILayout.Space(3);
            GUILayout.Label("Main chute:", RCUtils.boldLabel, GUILayout.Width(120));
            main.UpdateGUI();

            if (secondaryChute)
            {
                GUILayout.Label("___________________________________________", RCUtils.boldLabel);
                GUILayout.Space(3);
                GUILayout.Label("Secondary chute:", RCUtils.boldLabel, GUILayout.Width(120));
                secondary.UpdateGUI();
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