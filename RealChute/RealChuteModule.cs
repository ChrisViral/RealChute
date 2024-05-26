﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClickThroughFix;
using KSP.UI.Screens;
using RealChute.Extensions;
using UnityEngine;
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
    /// <summary>
    /// Parachute deployment states
    /// </summary>
    public enum DeploymentStates
    {
        NONE,
        STOWED,
        LOWDEPLOYED,
        PREDEPLOYED,
        DEPLOYED,
        CUT
    }

    /// <summary>
    /// Parachute deployment safety state
    /// </summary>
    public enum SafeState
    {
        SAFE,
        RISKY,
        DANGEROUS
    }

    public class RealChuteModule : PartModule, IPartCostModifier, IPartMassModifier, IModuleInfo
    {
        #region Persistent fields
        // Values from the .cfg file
        [KSPField(isPersistant = true)]
        public float caseMass = 0.1f;
        [KSPField(isPersistant = true)]
        public float cutSpeed = 0.5f;
        [KSPField(isPersistant = true)]
        public float timer;
        [KSPField(isPersistant = true)]
        public bool mustGoDown;
        [KSPField(isPersistant = true)]
        public bool deployOnGround;
        [KSPField(isPersistant = true)]
        public float spareChutes = 5;
        [KSPField(isPersistant = true)]
        public bool initiated;
        [KSPField(isPersistant = true)]
        public bool wait = true, armed, oneWasDeployed;
        [KSPField(isPersistant = true)]
        public bool staged, launched;
        [KSPField(isPersistant = true, guiActive = true, guiName = "Spare chutes")]
        public int chuteCount = 5;
        [KSPField]
        public bool reverseOrientation;
        [KSPField]
        public SafeState safeState = SafeState.SAFE;
        #endregion

        #region Propreties
        // If the vessel is stopped on the ground
        public bool GroundStop => this.vessel.LandedOrSplashed && this.vessel.srfSpeed < this.cutSpeed;

        // If both parachutes must cut
        public bool AllMustStop => this.SecondaryChute && (this.GroundStop || this.atmPressure == 0d) && this.parachutes.Exists(p => p.DeploymentState == DeploymentStates.CUT);

        // If the parachute can be repacked
        public bool CanRepack => (this.GroundStop || this.atmPressure == 0d) && this.parachutes.Exists(p => p.DeploymentState == DeploymentStates.CUT)
                                 && this.parachutes.TrueForAll(p => !p.IsDeployed) && (this.chuteCount > 0 || this.chuteCount == -1) && FlightGlobals.ActiveVessel.isEVA;

        //If the Kerbal can repack the chute in career mode
        public bool CanRepackCareer => HighLogic.CurrentGame.Mode != Game.Modes.CAREER || !RealChuteSettings.Instance.MustBeEngineer || FlightGlobals.ActiveVessel.IsEngineer()
                                       && FlightGlobals.ActiveVessel.VesselValues.RepairSkill.value >= RealChuteSettings.Instance.EngineerLevel;

        //If any parachute is deployed
        public bool AnyDeployed => this.parachutes.Exists(p => p.IsDeployed);

        // Whether multiple parachutes are deployed or not
        public bool ManyDeployed => this.parachutes.Count(p => p.IsDeployed) > 1;

        //If there is more than one parachute on the part
        public bool SecondaryChute => this.parachutes.Count > 1;

        //Check if the staging button has been pressed this frame
        public bool PressedStage => !InputLockManager.IsLocked(ControlTypes.STAGING) && GameSettings.LAUNCH_STAGES.GetKeyDown();

        //Quick access to the part GUI events
        private BaseEvent deploy, arm, disarm, cut, repack;
        private BaseEvent Deploy => this.deploy ?? (this.deploy = this.Events["GUIDeploy"]);
        private BaseEvent Arm => this.arm ?? (this.arm = this.Events["GUIArm"]);
        private BaseEvent Disarm => this.disarm ?? (this.disarm = this.Events["GUIDisarm"]);
        private BaseEvent Cut => this.cut ?? (this.cut = this.Events["GUICut"]);
        private BaseEvent Repack => this.repack ?? (this.repack = this.Events["GUIRepack"]);
        #endregion

        #region Fields
        //Module
        internal Vector3 dragVector, pos = new Vector3d();
        private readonly PhysicsWatch deploymentTimer = new PhysicsWatch(), failedTimer = new PhysicsWatch(), launchTimer = new PhysicsWatch();
        private bool displayed, showDisarm;
        internal double asl, trueAlt;
        internal double atmPressure, atmDensity;
        internal float sqrSpeed, speed, massDelta;
        internal double convectiveFactor;
        private ProceduralChute pChute;
        public List<Parachute> parachutes = new List<Parachute>();
        public ConfigNode node;

        //GUI
        protected bool visible, hid;
        private readonly int id = Guid.NewGuid().GetHashCode();
        private Rect window, drag;
        private Vector2 scroll;
        private string screenMessage = string.Empty;
        private bool showMessage;
        #endregion

        #region Part GUI
        //Deploys the parachutes if possible
        [KSPEvent(guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Deploy Chute", unfocusedRange = 5)]
        public void GUIDeploy() => ActivateRC();

        //Cuts main chute chute
        [KSPEvent(guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Cut main chute", unfocusedRange = 5)]
        public void GUICut() => this.parachutes.Where(p => p.IsDeployed).ForEach(p => p.Cut());

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
            this.Deploy.active = true;
            this.Arm.active = true;
            DeactivateRC();
        }

        //Repacks chute from EVA if in space or on the ground
        [KSPEvent(guiActive = false, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Repack chute", unfocusedRange = 5)]
        public void GUIRepack()
        {
            if (this.CanRepack)
            {
                if (!this.CanRepackCareer)
                {
                    int level = RealChuteSettings.Instance.EngineerLevel;
                    string message = level > 0 ? $"Only a level {level} and higher engineer can repack a parachute" : "Only an engineer can repack a parachute";
                    ScreenMessages.PostScreenMessage(message, 5, ScreenMessageStyle.UPPER_CENTER);
                    return;
                }

                this.part.Effect("rcrepack");
                this.Repack.guiActiveUnfocused = false;
                this.oneWasDeployed = false;
                this.part.stackIcon.SetIconColor(XKCDColors.White);
                if (this.chuteCount != -1) { this.chuteCount--; }
                this.parachutes.Where(p => p.DeploymentState == DeploymentStates.CUT).ForEach(p => p.Repack());
                UpdateDragCubes();
            }
        }

        //Shows the info window
        [KSPEvent(guiActive = true, active = true, guiActiveEditor = true, guiName = "Toggle info")]
        public void GUIToggleWindow()
        {
            if (!this.visible)
            {
                List<RealChuteModule> chutes = new List<RealChuteModule>();
                if (HighLogic.LoadedSceneIsEditor) { chutes.AddRange(EditorLogic.SortedShipList.Where(p => p.Modules.Contains("RealChuteModule")).Select(p => (RealChuteModule)p.Modules["RealChuteModule"])); }
                else if (HighLogic.LoadedSceneIsFlight) { chutes.AddRange(this.vessel.FindPartModulesImplementing<RealChuteModule>()); }
                if (chutes.Count > 1 && chutes.Exists(p => p.visible))
                {
                    RealChuteModule module = chutes.Find(p => p.visible);
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
        public void ActionDeploy(KSPActionParam param) => ActivateRC();

        //Cuts main chute
        [KSPAction("Cut main chute")]
        public void ActionCut(KSPActionParam param)
        {
            if (this.parachutes.Exists(p => p.IsDeployed)) { GUICut(); }
        }

        //Arms parachutes
        [KSPAction("Arm parachute")]
        public void ActionArm(KSPActionParam param) => GUIArm();

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
            this.screenMessage = string.Empty;
            //Timer
            if (this.timer > 0 && this.deploymentTimer.Elapsed.TotalSeconds < this.timer)
            {
                timerSpent = false;
                if (!this.deploymentTimer.IsRunning) { this.deploymentTimer.Start(); }
                if (this.vessel.isActiveVessel)
                {
                    float time = this.timer - (float)this.deploymentTimer.Elapsed.TotalSeconds;
                    this.screenMessage = time < 60 ? $"Deployment in {time:0.0}s" : $"Deployment in {RCUtils.ToMinutesSeconds(time)}";
                }
            }
            else if (this.deploymentTimer.IsRunning) { this.deploymentTimer.Stop(); }

            //Goes down
            if (this.mustGoDown && this.vessel.verticalSpeed > 0)
            {
                if (!timerSpent) { this.screenMessage += "\n"; }
                goesDown = false;
                if (this.vessel.isActiveVessel)
                {
                    this.screenMessage += $"Deployment awaiting negative vertical velocity\nCurrent vertical velocity: {this.vessel.verticalSpeed:0.0}/s";
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
            if (RealChuteSettings.Instance.AutoArm) { this.armed = true; }
            print("[RealChute]: " + this.part.partInfo.name + " was activated in stage " + this.part.inverseStage);
        }

        //Deactivates the parachute
        public void DeactivateRC()
        {
            this.staged = false;
            if (this.vessel.LandedOrSplashed) { this.launched = false; }
            this.wait = true;
            print("[RealChute]: " + this.part.partInfo.name + " was deactivated");
        }

        //Copies stats from the info window to the symmetry counterparts
        private void CopyToCounterparts()
        {
            foreach (Part p in this.part.symmetryCounterparts)
            {
                RealChuteModule module = (RealChuteModule)p.Modules["RealChuteModule"];
                for (int i = 0; i < this.parachutes.Count; i++)
                {
                    Parachute current = this.parachutes[i], counterpart = module.parachutes[i];
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
            this.part.inverseStage = this.part.inverseStage != 0 ? this.part.inverseStage - 1 : StageManager.CurrentStage;
        }

        //Allows the chute to be repacked if available
        public void SetRepack()
        {
            this.part.stackIcon.SetIconColor(XKCDColors.Red);
            this.wait = true;
            StagingReset();
        }

        //Drag formula calculations
        public float DragCalculation(float area, float cd) => ((float)this.atmDensity * this.sqrSpeed * cd * area) / 2000f;

        //Loads all the parachutes into the list
        private void LoadParachutes()
        {
            if (this.parachutes.Count <= 0 && this.node != null && this.node.HasNode("PARACHUTE"))
            {
                this.parachutes = new List<Parachute>(this.node.GetNodes("PARACHUTE").Select(n => new Parachute(this, n)));
            }
        }

        //Sets the part's staging icon background colour according to the safety to deploy
        private void SetSafeToDeploy()
        {
            SafeState[] states = this.parachutes.Select(p => p.GetSafeState()).ToArray();
            SafeState s = states[0];
            //What this does is that if the first is not risky, and that all the following are all like the first one (safe or dangerous),
            //Then the final state is the first one. If not, the final state is risky
            if (states.Length != 1 && s != SafeState.RISKY)
            {
                //Only stay safe or dangerous if all are safe or dangerous
                for (int i = 1; i < states.Length; i++)
                {
                    if (states[i] != s) { s = SafeState.RISKY; break; }
                }
            }
            if (s != this.safeState)
            {
                this.safeState = s;
                switch (this.safeState)
                {
                    case SafeState.SAFE:
                        this.part.stackIcon.SetBackgroundColor(XKCDColors.White); break;

                    case SafeState.RISKY:
                        this.part.stackIcon.SetBackgroundColor(XKCDColors.BrightYellow); break;

                    case SafeState.DANGEROUS:
                        this.part.stackIcon.SetBackgroundColor(XKCDColors.Red); break;
                }
            }
        }

        //Sets the mass delta to the correct amount
        public void UpdateMass()
        {
            Part prefab = this.part.partInfo.partPrefab;
            this.massDelta = prefab == null ? 0 : (this.caseMass + this.parachutes.Sum(p => p.ChuteMass)) - prefab.mass;
        }

        //Gives the cost for this parachute
        public float GetModuleCost(float defaultCost, ModifierStagingSituation sit) => RCUtils.Round(this.parachutes.Sum(p => p.DeployedArea * p.mat.AreaCost));

        ModifierChangeWhen IPartCostModifier.GetModuleCostChangeWhen() => ModifierChangeWhen.FIXED;

        //Sets the parts mass dynamically
        public float GetModuleMass(float defaultMass, ModifierStagingSituation sit) => this.massDelta;

        ModifierChangeWhen IPartMassModifier.GetModuleMassChangeWhen() => ModifierChangeWhen.FIXED;

        //Not needed
        Callback<Rect> IModuleInfo.GetDrawModulePanelCallback() => null;

        //Sets module info title
        public string GetModuleTitle() => "RealChute";

        //Sets part info field
        public string GetPrimaryField() => "<b>Parachute count:</b> " + this.parachutes.Count;

        //Sets up the part for DragCube rendering
        private void SetupForDragCubeRendering(bool on)
        {
            //Setup all deployed canopies
            foreach (Parachute chute in this.parachutes.Where(chute => chute.IsDeployed))
            {
                chute.parachute.gameObject.SetActive(!on);
            }

            //Setup case size
            if (this.pChute)
            {
                this.pChute.SetDragCubeSize(on);
            }
        }

        //Updates the DragCube for this part
        public void UpdateDragCubes()
        {
           SetupForDragCubeRendering(true);

            //Render new DragCube
            DragCube cube = DragCubeSystem.Instance.RenderProceduralDragCube(this.part);
            this.part.DragCubes.ClearCubes();
            this.part.DragCubes.Cubes.Add(cube);
            this.part.DragCubes.ResetCubeWeights();
            this.part.DragCubes.ForceUpdate(true, true, true);

            SetupForDragCubeRendering(false);
        }

        //Event when the UI is hidden (F2)
        private void HideUI() => this.hid = true;

        //Event when the UI is shown (F2)
        private void ShowUI() => this.hid = false;
        #endregion

        #region Functions
        private void Update()
        {
            if (!CompatibilityChecker.IsAllCompatible|| !HighLogic.LoadedSceneIsFlight) { return; }

            //Makes the chute icon blink if failed
            if (this.failedTimer.IsRunning)
            {
                double time = this.failedTimer.Elapsed.TotalSeconds;
                if (time <= 2.5)
                {
                    if (!this.displayed)
                    {
                        ScreenMessages.PostScreenMessage("Parachute deployment failed.", 2.5f, ScreenMessageStyle.UPPER_CENTER);
                        if (this.part.ShieldedFromAirstream) { ScreenMessages.PostScreenMessage("Reason: parachute is in fairings", 2.5f, ScreenMessageStyle.UPPER_CENTER); }
                        else if (this.GroundStop) { ScreenMessages.PostScreenMessage("Reason: stopped on the ground.", 2.5f, ScreenMessageStyle.UPPER_CENTER); }
                        else if (this.atmPressure == 0d) { ScreenMessages.PostScreenMessage("Reason: in space.", 2.5f, ScreenMessageStyle.UPPER_CENTER); }
                        else { ScreenMessages.PostScreenMessage("Reason: too high.", 2.5f, ScreenMessageStyle.UPPER_CENTER); }
                        this.displayed = true;
                    }
                    if (time < 0.5 || time >= 1d && time < 1.5 || time >= 2d) { this.part.stackIcon.SetIconColor(XKCDColors.Red); }
                    else { this.part.stackIcon.SetIconColor(XKCDColors.White); }
                }
                else
                {
                    this.displayed = false;
                    this.part.stackIcon.SetIconColor(XKCDColors.White);
                    this.failedTimer.Reset();
                }
            }

            if (this.showMessage)
            {
                ScreenMessages.PostScreenMessage(this.screenMessage, Time.deltaTime * 0.6f, ScreenMessageStyle.UPPER_CENTER);
            }

            this.Disarm.active = this.armed || this.showDisarm;
            bool canDeploy = !this.staged && this.parachutes.Exists(p => p.DeploymentState != DeploymentStates.CUT);
            this.Deploy.active = canDeploy;
            this.Arm.active = !RealChuteSettings.Instance.AutoArm && canDeploy;
            this.Repack.guiActiveUnfocused = this.CanRepack;
            this.Cut.active = this.AnyDeployed;
        }

        private void FixedUpdate()
        {
            //Flight values
            if (!CompatibilityChecker.IsAllCompatible|| !HighLogic.LoadedSceneIsFlight || FlightGlobals.ActiveVessel == null || this.part.Rigidbody == null) { return; }
            this.pos = this.part.transform.position;
            this.asl = FlightGlobals.getAltitudeAtPos(this.pos);
            this.trueAlt = this.vessel.GetTrueAlt(this.asl);
            this.atmPressure = this.vessel.staticPressurekPa * PhysicsGlobals.KpaToAtmospheres;
            this.atmDensity = this.vessel.atmDensity;
            Vector3 velocity = this.part.Rigidbody.velocity + Krakensbane.GetFrameVelocityV3f();
            this.sqrSpeed = velocity.sqrMagnitude;
            this.speed = Mathf.Sqrt(this.sqrSpeed);
            this.convectiveFactor = Math.Pow(UtilMath.Clamp01((this.vessel.mach - PhysicsGlobals.NewtonianMachTempLerpStartMach) / (PhysicsGlobals.NewtonianMachTempLerpEndMach - PhysicsGlobals.NewtonianMachTempLerpStartMach)), PhysicsGlobals.NewtonianMachTempLerpExponent);
            this.dragVector = -velocity.normalized;

            if (!this.staged && this.PressedStage && this.vessel.isActiveVessel && (this.part.inverseStage == StageManager.CurrentStage - 1 || StageManager.CurrentStage == 0)) { ActivateRC(); }
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
                    else if (this.launchTimer.IsRunning) { this.launchTimer.Reset(); }
                }
                else if (this.launched && this.vessel.horizontalSrfSpeed >= this.cutSpeed && this.vessel.LandedOrSplashed) { ActivateRC(); }
            }

            if (this.atmDensity > 0) { this.parachutes.ForEach(p => p.CalculateConvectiveFlux()); }
            SetSafeToDeploy();

            if (this.staged)
            {
                //Checks if the parachute must disarm
                if (this.armed)
                {
                    this.part.stackIcon.SetIconColor(XKCDColors.LightCyan);
                    if (this.parachutes.Exists(p => p.CanDeploy)) { this.armed = false; }
                }
                //Parachute deployments
                else
                {
                    if (this.wait)
                    {
                        CheckForWait();
                        if (this.wait)
                        {
                            this.showMessage = true;
                            return;
                        }
                        this.showMessage = false;
                    }

                    //Parachutes
                    this.parachutes.ForEach(p => p.UpdateParachute());

                    //If all parachutes must be cut
                    if (this.AllMustStop)
                    {
                        GUICut();
                        SetRepack();
                    }

                    //If the parachute can't be deployed
                    if (!this.oneWasDeployed && !RealChuteSettings.Instance.AutoArm)
                    {
                        this.failedTimer.Start();
                        StagingReset();
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (!CompatibilityChecker.IsAllCompatible|| !HighLogic.LoadedSceneIsFlight && !HighLogic.LoadedSceneIsEditor) { return; }
            //Hide/show UI event removal
            GameEvents.onHideUI.Remove(HideUI);
            GameEvents.onShowUI.Remove(ShowUI);
        }
        #endregion

        #region Overrides
        public override void OnStart(StartState state)
        {
            if (!HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight) { return; }
            if (!CompatibilityChecker.IsAllCompatible)
            {
                foreach (BaseAction a in this.Actions)
                {
                    a.active = false;
                }
                foreach (BaseEvent e in this.Events)
                {
                    e.active = false;
                    e.guiActive = false;
                    e.guiActiveEditor = false;
                }
                this.Fields["chuteCount"].guiActive = false;
                return;
            }
            //Staging icon
            this.part.stagingIcon = "PARACHUTES";
            this.safeState = SafeState.SAFE;

            //Part GUI
            if (this.spareChutes < 0) { this.Fields["chuteCount"].guiActive = false; }
            if (!this.SecondaryChute)
            {
                this.Actions["ActionCut"].guiName = "Cut chute";
                this.Cut.guiName = "Cut chute";
            }
            this.Actions["ActionArm"].active = !RealChuteSettings.Instance.AutoArm;

            //Initiates the Parachutes
            if (this.parachutes.Count <= 0)
            {
                if (this.node == null && !PersistentManager.Instance.TryGetNode<RealChuteModule>(this.part.name, ref this.node)) { return; }
                LoadParachutes();
            }
            this.parachutes.ForEach(p => p.Initialize());

            //First initiation of the part
            if (!this.initiated)
            {
                this.initiated = true;
                this.armed = false;
                if (this.spareChutes >= 0) { this.chuteCount = (int)this.spareChutes; }
            }

            //Flight loading
            if (HighLogic.LoadedSceneIsFlight)
            {
                this.pChute = this.part.Modules["ProceduralChute"] as ProceduralChute;
                UpdateDragCubes();
                Random random = new Random();
                this.parachutes.ForEach(p => p.randomTime = (float)random.NextDouble());

                //Hide/show UI event addition
                GameEvents.onHideUI.Add(HideUI);
                GameEvents.onShowUI.Add(ShowUI);

                if (this.CanRepack) { SetRepack(); }
            }

            //GUI
            this.window = new Rect(200f * GameSettings.UI_SCALE, 100f * GameSettings.UI_SCALE, 350f * GameSettings.UI_SCALE, 400f * GameSettings.UI_SCALE);
            this.drag = new Rect(0f, 0f, 350f * GameSettings.UI_SCALE, 30f * GameSettings.UI_SCALE);
        }

        public override void OnLoad(ConfigNode n)
        {
            if (!CompatibilityChecker.IsAllCompatible) { return; }
            this.node = n;
            LoadParachutes();
            if (HighLogic.LoadedScene == GameScenes.LOADING || !PartLoader.Instance.IsReady() || this.part.partInfo == null)
            {
                PersistentManager.Instance.AddNode<RealChuteModule>(this.part.name, n);
            }
            else { UpdateMass(); }
        }

        public override string GetInfo()
        {
            if (!CompatibilityChecker.IsAllCompatible) { return string.Empty; }
            //Info in the editor part window
            this.part.mass = this.caseMass + this.parachutes.Sum(p => p.ChuteMass);

            StringBuilder builder = StringBuilderCache.Acquire();
            builder.AppendFormat("Case mass: {0}\n", this.caseMass);
            if (this.timer > 0) { builder.AppendFormat("Deployment timer: {0}s\n", this.timer); }
            if (this.mustGoDown) { builder.AppendLine("Must go downwards to deploy: true"); }
            if (this.deployOnGround) { builder.AppendLine("Deploys on ground contact: true"); }
            if (this.spareChutes >= 0) { builder.AppendFormat("Spare chutes: {0}\n", this.spareChutes); }
            builder.AppendFormat("Autocut speed: {0}m/s\n", this.cutSpeed);

            if (!this.SecondaryChute)
            {
                Parachute parachute = this.parachutes[0];
                builder.AppendFormat("Parachute material: {0}\n", parachute.material);
                builder.AppendFormat("Drag coefficient: {0:0.00}\n", parachute.mat.DragCoefficient);
                builder.AppendFormat("Material max temperature: {0:0.#}°C", parachute.mat.MaxTemp + RCUtils.AbsoluteZero);
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
                builder.Append("Parachute materials: ").AppendJoin(this.parachutes.Select(p => p.material), ", ").AppendLine();
                builder.Append("Drag coefficients: ").AppendJoin(this.parachutes.Select(p => p.mat.DragCoefficient.ToString("0.00")), ", ").AppendLine();
                builder.Append("Chute max temperatures: ").AppendJoin(this.parachutes.Select(p => (p.mat.MaxTemp + RCUtils.AbsoluteZero).ToString("0.#")), "°C, ").AppendLine("°C");
                builder.Append("Predeployed diameters: ").AppendJoin(this.parachutes.Select(p => p.preDeployedDiameter.ToString()), "m, ").AppendLine("m");
                builder.Append("Deployed diameters: ").AppendJoin(this.parachutes.Select(p => p.deployedDiameter.ToString()), "m, ").AppendLine("m");
                builder.Append("Minimum deployment clauses: ").AppendJoin(this.parachutes.Select(p => p.minIsPressure ? p.minPressure + "atm" : p.minDeployment + "m"), ", ").AppendLine();
                builder.Append("Deployment altitudes: ").AppendJoin(this.parachutes.Select(p => p.deploymentAlt.ToString()), "m, ").AppendLine("m");
                builder.Append("Predeployment speeds: ").AppendJoin(this.parachutes.Select(p => p.preDeploymentSpeed.ToString()), "s, ").AppendLine("s");
                builder.Append("Deployment speeds: ").AppendJoin(this.parachutes.Select(p => p.deploymentSpeed.ToString()), "s, ").Append("s");
                if (this.parachutes.Exists(p => p.cutAlt != -1))
                {
                    builder.Append("\nAutocut altitudes: ").AppendJoin(this.parachutes.Select(p => p.cutAlt == -1 ? "-- " : p.cutAlt + "m"), ", ");
                }
            }
            return builder.ToStringAndRelease();
        }

        public override void OnSave(ConfigNode n)
        {
            if (!CompatibilityChecker.IsAllCompatible) { return; }
            //Saves the parachutes to the persistence
            this.parachutes.ForEach(p => n.AddNode(p.Save()));
        }

        public override void OnActive()
        {
            if (!this.staged) { ActivateRC(); }
        }

        public override bool IsStageable() => true;
        #endregion

        #region GUI
        private void OnGUI()
        {
            //Handles GUI rendering
            if (CompatibilityChecker.IsAllCompatible&& (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor) && this.visible && !this.hid)
            {
                GUI.skin = HighLogic.Skin;
                this.window = ClickThruBlocker.GUILayoutWindow(this.id, this.window, Window, "RealChute Info Window " + RCUtils.AssemblyVersion, GUIUtils.ScaledWindow);
            }
        }

        //Info window
        private void Window(int id)
        {
            //Header
            GUI.DragWindow(this.drag);
            GUILayout.BeginVertical();

            //Top info labels
            StringBuilder builder = StringBuilderCache.Acquire().Append("Part name: ").AppendLine(this.part.partInfo.title);
            builder.Append("Symmetry counterparts: ").AppendLine(this.part.symmetryCounterparts.Count.ToString());
            builder.Append("Part mass: ").Append(this.part.TotalMass().ToString("0.###")).Append("t");
            GUILayout.Label(builder.ToStringAndRelease(), GUIUtils.BoldLabel);

            //Beginning scroll
            this.scroll = GUILayout.BeginScrollView(this.scroll, false, false, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.box);
            GUILayout.Space(5f * GameSettings.UI_SCALE);
            GUILayout.Label("General:", GUIUtils.BoldLabel, GUILayout.Width(120f * GameSettings.UI_SCALE));

            //General labels
            builder = StringBuilderCache.Acquire().Append("Autocut speed: ").Append(this.cutSpeed).AppendLine("m/s");
            if (this.timer >= 60) { builder.Append("Deployment timer: ").AppendLine(RCUtils.ToMinutesSeconds(this.timer)); }
            else if (this.timer > 0) { builder.Append("Deployment timer: ").Append(this.timer.ToString("0.#")).AppendLine("s"); }
            if (this.mustGoDown) { builder.AppendLine("Must go downwards to deploy"); }
            if (this.deployOnGround) { builder.AppendLine("Automatically deploys on ground contact"); }
            if (this.spareChutes >= 0) { builder.Append("Spare chutes: ").Append(this.chuteCount); }
            else { builder.Append("Spare chutes: inf"); }
            GUILayout.Label(builder.ToStringAndRelease(), GUIUtils.ScaledLabel);

            //Specific labels
            for (int i = 0; i < this.parachutes.Count; i++)
            {
                GUILayout.Label("___________________________________________", GUIUtils.BoldLabel);
                GUILayout.Space(3f * GameSettings.UI_SCALE);
                GUILayout.Label(RCUtils.ParachuteNumber(i) + ":", GUIUtils.BoldLabel, GUILayout.Width(120f * GameSettings.UI_SCALE));
                this.parachutes[i].RenderGUI();
            }

            //End scroll
            GUILayout.EndScrollView();

            //Copy button if in flight
            if (HighLogic.LoadedSceneIsFlight && this.part.symmetryCounterparts.Count > 0)
            {
                GUIUtils.CenteredButton("Copy to others chutes", CopyToCounterparts);
            }

            //Close button
            GUIUtils.CenteredButton("Close", () => this.visible = false);

            //Closer
            GUILayout.EndVertical();
        }
        #endregion
    }
}
