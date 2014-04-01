using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using RealChute.Extensions;
using UnityEngine;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more informtion contact me on the forum. */

namespace RealChute
{
    public class RealChuteModule : PartModule
    {
        #region DeploymentStates
        public enum DeploymentStates
        {
            STOWED,
            LOWDEPLOYED,
            PREDEPLOYED,
            DEPLOYED,
            CUT
        }
        #endregion

        #region Config values
        // Values from the .cfg file
        [KSPField(isPersistant = true)]
        public float caseMass = 0.1f;
        [KSPField(isPersistant = true)]
        public string material = "Nylon";
        [KSPField(isPersistant = true)]
        public string secMaterial = "empty";
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Autocut speed", guiFormat = "0.#"), UI_FloatRange(minValue = 0.5f, maxValue = 10, stepIncrement = 0.5f)]
        public float cutSpeed = 0.5f;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Timer", guiFormat = "0.#"), UI_FloatRange(minValue = 0, maxValue = 30, stepIncrement = 1)]
        public float timer = 0;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Must down"), UI_Toggle(enabledText = "true", disabledText = "false")]
        public bool mustGoDown = false;
        [KSPField]
        public bool reverseOrientation = false;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Spares"), UI_FloatRange(minValue = -1, maxValue = 10, stepIncrement = 1)]
        public float spareChutes = 5;
        [KSPField]
        public bool secondaryChute = false;
        [KSPField]
        public bool isTweakable = true;

        //Main parachute
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Predep diam", guiFormat = "0.#"), UI_FloatRange(minValue = 0.5f, maxValue = 70, stepIncrement = 0.5f)]
        public float preDeployedDiameter = 1;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Dep diam", guiFormat = "0.#"), UI_FloatRange(minValue = 0.5f, maxValue = 70, stepIncrement = 0.5f)]
        public float deployedDiameter = 25;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Predep"), UI_Toggle(enabledText = "pressure", disabledText = "altitude")]
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
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Predep2 diam", guiFormat = "0.#"), UI_FloatRange(minValue = 0.5f, maxValue = 70, stepIncrement = 0.5f)]
        public float secPreDeployedDiameter = 1;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Dep2 diam", guiFormat = "0.#"), UI_FloatRange(minValue = 0.5f, maxValue = 70, stepIncrement = 0.5f)]
        public float secDeployedDiameter = 25;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Predep2"), UI_Toggle(enabledText = "pressure", disabledText = "altitude")]
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
        public bool staged = false;
        [KSPField(isPersistant = true)]
        public string depState = "STOWED", secDepState = "STOWED";
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
            get { return secondaryChute && (groundStop || atmPressure == 0) && (deploymentState == DeploymentStates.CUT || secDeploymentState == DeploymentStates.CUT); }
        }

        // If the parachute can be repacked
        public bool canRepack
        {
            get { return (groundStop || atmPressure == 0) && ((!secondaryChute && deploymentState == DeploymentStates.CUT) || (secondaryChute && deploymentState == DeploymentStates.CUT && secDeploymentState == DeploymentStates.CUT)) && (chuteCount > 0 || chuteCount == -1); }
        }

        // Wether both parachutes are deployed or not
        public bool bothDeployed
        {
            get { return IsDeployed(deploymentState) && IsDeployed(secDeploymentState); }
        }

        // Predeployed area of the main chute
        public float preDeployedArea
        {
            get { return RCUtils.GetArea(preDeployedDiameter); }
        }

        // Total deployed area of the main chute
        public float deployedArea
        {
            get { return RCUtils.GetArea(deployedDiameter); }
        }

        // Mass of the main chute
        public float chuteMass
        {
            get { return deployedArea * mat.areaDensity; }
        }

        // Predeployed area of the second chute
        public float secPreDeployedArea
        {
            get { return RCUtils.GetArea(secPreDeployedDiameter); }
        }

        // Total deployed area of the second chute
        public float secDeployedArea
        {
            get { return RCUtils.GetArea(secDeployedDiameter); }
        }

        //Mass of the secondary chute
        public float secChuteMass
        {
            get { return secondaryChute ? secDeployedArea * secMat.areaDensity : 0f; }
        }
        #endregion

        #region Fields
        //Vectors
        private Vector3 dragVector = new Vector3();
        private Vector3 forcePosition = new Vector3(), secForcePosition = new Vector3();
        private Vector3 phase = Vector3.zero, secPhase = Vector3.zero;
        private Vector3d pos = new Vector3d();

        //Animations
        private Animation anim = null;
        internal Transform parachute = null, secParachute = null;
        private Transform cap = null, secCap = null;
        private bool played = false, secPlayed = false;

        //Deployment
        private Stopwatch deploymentTimer = new Stopwatch(), randomTimer = new Stopwatch(), secRandomTimer = new Stopwatch();
        private Stopwatch dragTimer = new Stopwatch(), secDragTimer = new Stopwatch(), failedTimer = new Stopwatch();
        private float currentTime, deploymentTime;
        public DeploymentStates deploymentState = DeploymentStates.STOWED, secDeploymentState = DeploymentStates.STOWED;
        public Dictionary<DeploymentStates, string> states = new Dictionary<DeploymentStates, string>(5)
        {
            { DeploymentStates.STOWED, "STOWED" },
            { DeploymentStates.PREDEPLOYED, "PREDEPLOYED" },
            { DeploymentStates.LOWDEPLOYED, "LOWDEPLOYED" },
            { DeploymentStates.DEPLOYED, "DEPLOYED" },
            { DeploymentStates.CUT, "CUT" }
        };
        public float random_x, random_y;
        public float secRandom_x, secRandom_y;
        public float randomTime, secRandomTime;
        private bool randomized = false, secRandomized = false;
        private bool displayed = false, autoArm = false;

        //Vessel info
        private double terrainAlt, ASL, trueAlt;
        private double atmPressure, atmDensity;
        private float sqrSpeed;
        private float huehue;

        //Materials
        public MaterialDefinition mat = new MaterialDefinition(), secMat = new MaterialDefinition();
        private MaterialsLibrary materials = MaterialsLibrary.instance;

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
            Cut(true);
        }

        //Cuts secondary chute
        [KSPEvent(guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Cut secondary chute", unfocusedRange = 5)]
        public void GUISecCut()
        {
            Cut(false);
        }

        //Cuts both chutes if possible
        [KSPEvent(guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Cut both chutes", unfocusedRange = 5)]
        public void GUICutBoth()
        {
            Cut(true);
            Cut(false);
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
                deploymentState = DeploymentStates.STOWED;
                depState = GetStateString(deploymentState);
                cap.gameObject.SetActive(true);
                this.part.stackIcon.SetIconColor(XKCDColors.White);
                capOff = false;
                if (chuteCount != -1) { chuteCount--; }

                if (secondaryChute)
                {
                    secDeploymentState = DeploymentStates.STOWED;
                    secDepState = GetStateString(secDeploymentState);
                    secCap.gameObject.SetActive(true);
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
            if (IsDeployed(deploymentState)) { Cut(true); }
        }

        //Cuts secondary chute
        [KSPAction("Cut secondary chute")]
        public void ActionSecCut(KSPActionParam param)
        {
            if (IsDeployed(secDeploymentState)) { Cut(false); }
        }

        //Cuts both cutes if possible
        [KSPAction("Cut both chutes")]
        public void ActionCutBoth(KSPActionParam param)
        {
            if (IsDeployed(deploymentState) && IsDeployed(secDeploymentState))
            {
                Cut(true);
                Cut(false);
                Events["GUICutBoth"].active = false;
            }
        }

        //Arms parachutes
        [KSPAction("Arm parachute")]
        public void ActionArm(KSPActionParam param)
        {
            armed = true;
            ActivateRC();
        }

        [KSPAction("Disarm parachute")]
        public void ActionDisarm(KSPActionParam param)
        {
            if(armed)
            {
                armed = false;
                this.part.stackIcon.SetIconColor(XKCDColors.White);
                Events["GUIDeploy"].active = true;
                Events["GUIArm"].active = true;
                DeactivateRC();
            }
        }
        #endregion

        #region Methods
        #region General
        //Returns the right value to check
        public bool MinDeployment(float minDeploy, float minPress, bool isPressure)
        {
            return isPressure ? atmPressure >= minPress : trueAlt <= minDeploy;
        }

        //Checks if there is a timer and/or a mustGoDown clause active
        public void CheckForWait()
        {
            bool timerSpent = true;
            bool goesDown = true;

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

        //Short random deployment delayer
        public bool RandomDeployment(bool mainChute)
        {
            if (mainChute)
            {
                if (!this.randomTimer.IsRunning) { this.randomTimer.Start(); }

                if (this.randomTimer.Elapsed.TotalSeconds >= this.randomTime)
                {
                    this.randomTimer.Stop();
                    this.secRandomTimer.Reset();
                    return true;
                }
                return false;
            }
            else
            {
                if (!this.secRandomTimer.IsRunning) { this.secRandomTimer.Start(); }

                if (this.secRandomTimer.Elapsed.TotalSeconds >= this.secRandomTime)
                {
                    this.secRandomTimer.Stop();
                    this.secRandomTimer.Reset();
                    return true;
                }
                return false;
            }

        }

        //Checks if a parachute an deploy
        public bool CanDeployChute(bool mainChute)
        {
            //Automatically returns false if the craft is stopped on the ground or out of an atmosphere
            if (groundStop || atmPressure == 0) { return false; }
            //Checks if the main chute can deploy
            else if (mainChute)
            {
                if (deploymentState == DeploymentStates.CUT) { return false; }
                else if (MinDeployment(minDeployment, minPressure, minIsPressure) && cutAlt == -1) { return true; }
                else if (MinDeployment(minDeployment, minPressure, minIsPressure) && trueAlt > cutAlt) { return true; }
                else if (secondaryChute && !MinDeployment(minDeployment, minPressure, minIsPressure) && trueAlt <= secCutAlt) { return true; }
                else if (!MinDeployment(minDeployment, minPressure, minIsPressure) && IsDeployed(deploymentState)) { return true; }
            }
            //Checks if the secondary chute can deploy
            else
            {
                if (secDeploymentState == DeploymentStates.CUT) { return false; }
                else if (MinDeployment(secMinDeployment, secMinPressure, secMinIsPressure) && secCutAlt == -1) { return true; }
                else if (MinDeployment(secMinDeployment, secMinPressure, secMinIsPressure) && trueAlt > secCutAlt) { return true; }
                else if (!MinDeployment(secMinDeployment, secMinPressure, secMinIsPressure) && trueAlt <= cutAlt) { return true; }
                else if (!MinDeployment(secMinDeployment, secMinPressure, secMinIsPressure) && IsDeployed(secDeploymentState)) { return true; }
            }
            return false;
        }
        
        //Gets the DeploymentState from the string value
        public DeploymentStates GetState(string state)
        {
            return states.First(pair => pair.Value == state).Key;
        }

        //Gets the string state from a DeploymentState
        public string GetStateString(DeploymentStates value)
        {
            return states.First(pair => pair.Key == value).Value;
        }

        //Check if the chute is partially or completely deployed
        public bool IsDeployed(DeploymentStates deployState)
        {
            return GetStateString(deployState).Contains("DEPLOYED");
        }

        //Gives a random noise to the parachute orientation
        private void ParachuteNoise(Transform chute)
        {
            float x = 0;
            float y = 0;
            if (chute == parachute)
            {
                if (!this.randomized)
                {
                    this.random_x = UnityEngine.Random.Range(0f, 100f);
                    this.random_y = UnityEngine.Random.Range(0f, 100f);
                    this.randomized = true;
                }

                x = random_x;
                y = random_y;
            }
            else if (chute == secParachute)
            {
                if (!this.secRandomized)
                {
                    this.secRandom_x = UnityEngine.Random.Range(0f, 100f);
                    this.secRandom_y = UnityEngine.Random.Range(0f, 100f);
                    this.secRandomized = true;
                }

                x = secRandom_x;
                y = secRandom_y;
            }

            Vector3 rotationAngle = new Vector3(5 * (Mathf.PerlinNoise(currentTime, x + Mathf.Sin(currentTime)) - 0.5f), 5 * (Mathf.PerlinNoise(currentTime, y + Mathf.Sin(currentTime)) - 0.5f), 0);
            chute.Rotate(rotationAngle);
        }

        //Creates a vector that will angle the chute in of a certain amount away of the center of the part
        private Vector3 GetForcedVector(Transform chute, float angle)
        {
            if (angle >= 90 || angle <= 0) { return Vector3.zero; }
            Vector3 follow = chute.transform.position - pos;
            float length = 1 / Mathf.Tan((90 - angle) * Mathf.Deg2Rad);
            return follow.normalized * length;
        }

        //Makes the parachute go smoothly between forced orientation and normal orientation
        private Vector3 LerpDrag(Transform chute, Vector3 to)
        {
            if (chute == parachute)
            {
                if (phase.magnitude < (to.magnitude - 0.01f) || phase.magnitude > (to.magnitude + 0.01f)) { phase = Vector3.Lerp(phase, to, 0.01f); }
                else { phase = to; }
                return phase;
            }
            else
            {
                if (secPhase.magnitude < (to.magnitude - 0.01f) || secPhase.magnitude > (to.magnitude + 0.01f)) { secPhase = Vector3.Lerp(secPhase, to, 0.01f); }
                else { secPhase = to; }
                return secPhase;
            }
        }

        //Makes the parachute follow air flow
        private void FollowDragDirection(Transform chute, Vector3 forced)
        {
            //Smoothes the forced vector
            Vector3 orient = Vector3.zero;
            if (secondaryChute && bothDeployed) { orient = LerpDrag(chute, forced); }
            else if (secondaryChute && !bothDeployed) { orient = LerpDrag(chute, Vector3.zero); }

            Quaternion drag = Quaternion.identity;
            Vector3 follow = dragVector + orient;
            if (follow.magnitude > 0)
            {
                if (reverseOrientation) { drag = Quaternion.LookRotation(-follow.normalized, chute.up); }
                else { drag = Quaternion.LookRotation(follow.normalized, chute.up); }

                chute.rotation = drag;
            }
        }

        //Activates the parachute
        public void ActivateRC()
        {
            this.staged = true;
            if (autoArm) { this.armed = true; }
            Events["GUIDeploy"].active = false;
            Events["GUIArm"].active = false;
            print("[RealChute]: " + this.part.partInfo.name + " was activated in stage " + this.part.inverseStage);
        }

        //Deactiates the parachute
        public void DeactivateRC()
        {
            this.staged = false;
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
        #endregion

        #region Deployment
        //Parachute low deployment code
        public void LowDeploy(bool mainChute)
        {
            this.part.stackIcon.SetIconColor(XKCDColors.RadioactiveGreen);
            capOff = true;
            Events["GUIDeploy"].active = false;
            Events["GUIArm"].active = false;
            this.part.Effect("rcdeploy");
            if (mainChute)
            {
                deploymentState = DeploymentStates.LOWDEPLOYED;
                depState = GetStateString(deploymentState);
                parachute.gameObject.SetActive(true);
                cap.gameObject.SetActive(false);
                Events["GUICut"].active = true;
                this.part.PlayAnimation(preDeploymentAnimation, 1 / preDeploymentSpeed);
                dragTimer.Start();
            }
            else
            {
                secDeploymentState = DeploymentStates.LOWDEPLOYED;
                secDepState = GetStateString(secDeploymentState);
                secParachute.gameObject.SetActive(true);
                secCap.gameObject.SetActive(false);
                Events["GUISecCut"].active = true;
                this.part.PlayAnimation(secPreDeploymentAnimation, 1 / secPreDeploymentSpeed);
                secDragTimer.Start();
            }
        }

        //Parachute predeployment code
        public void PreDeploy(bool mainChute)
        {
            this.part.stackIcon.SetIconColor(XKCDColors.BrightYellow);
            capOff = true;
            Events["GUIDeploy"].active = false;
            Events["GUIArm"].active = false;
            this.part.Effect("rcpredeploy");
            if (mainChute)
            {
                deploymentState = DeploymentStates.PREDEPLOYED;
                depState = GetStateString(deploymentState);
                parachute.gameObject.SetActive(true);
                cap.gameObject.SetActive(false);
                Events["GUICut"].active = true;
                this.part.PlayAnimation(preDeploymentAnimation, 1 / preDeploymentSpeed);
                dragTimer.Start();
            }
            else
            {
                secDeploymentState = DeploymentStates.PREDEPLOYED;
                secDepState = GetStateString(secDeploymentState);
                secParachute.gameObject.SetActive(true);
                secCap.gameObject.SetActive(false);
                Events["GUISecCut"].active = true;
                this.part.PlayAnimation(secPreDeploymentAnimation, 1 / secPreDeploymentSpeed);
                secDragTimer.Start();
            }

        }

        //Parachute full deployment code
        public void Deploy(bool mainChute)
        {
            this.part.stackIcon.SetIconColor(XKCDColors.RadioactiveGreen);
            this.part.Effect("rcdeploy");
            if (mainChute)
            {
                deploymentState = DeploymentStates.DEPLOYED;
                depState = GetStateString(deploymentState);
                if (!this.part.CheckAnimationPlaying(preDeploymentAnimation))
                {
                    dragTimer.Reset();
                    dragTimer.Start();
                    this.part.PlayAnimation(deploymentAnimation, 1 / deploymentSpeed);
                    this.played = true;
                }
                else { this.played = false; }
            }
            else
            {
                secDeploymentState = DeploymentStates.DEPLOYED;
                secDepState = GetStateString(secDeploymentState);
                if (!this.part.CheckAnimationPlaying(secPreDeploymentAnimation))
                {
                    secDragTimer.Reset();
                    secDragTimer.Start();
                    this.part.PlayAnimation(secDeploymentAnimation, 1 / secDeploymentSpeed);
                    this.secPlayed = true;
                }
                else { this.secPlayed = false; }
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

        //Cuts the parachute
        public void Cut(bool mainChute)
        {
            this.part.Effect("rccut");
            if (mainChute)
            {
                deploymentState = DeploymentStates.CUT;
                depState = GetStateString(deploymentState);
                parachute.gameObject.SetActive(false);
                Events["GUICut"].active = false;
                this.played = false;
                if (!secondaryChute || secDeploymentState == DeploymentStates.CUT) { SetRepack(); }
                else if (secondaryChute && secDeploymentState == DeploymentStates.STOWED) { armed = true; }
                dragTimer.Reset();
            }
            else
            {
                secDeploymentState = DeploymentStates.CUT;
                secDepState = GetStateString(secDeploymentState);
                secParachute.gameObject.SetActive(false);
                Events["GUISecCut"].active = false;
                secPlayed = false;
                if (deploymentState == DeploymentStates.CUT) { SetRepack(); }
                else if (deploymentState == DeploymentStates.STOWED) { armed = true; }
                secDragTimer.Reset();
            }
        }

        //Allows the chute to be repacked if available
        public void SetRepack()
        {
            this.part.stackIcon.SetIconColor(XKCDColors.Red);
            randomTimer.Reset();
            secRandomTimer.Reset();
            wait = true;
            StagingReset();
            if (chuteCount > 0 || chuteCount == -1) { Events["GUIRepack"].guiActiveUnfocused = true; }
        }
        #endregion

        #region Drag
        //Calculates how much drag a part has since deployment
        private float DragDeployment(float time, float debutArea, float endArea, bool mainChute)
        {
            Stopwatch watch;
            if (mainChute) { watch = dragTimer; }
            else { watch = secDragTimer; }

            if (!watch.IsRunning) { watch.Start(); }

            if (watch.Elapsed.TotalSeconds <= time)
            {
                deploymentTime = (Mathf.Exp((float)watch.Elapsed.TotalSeconds) / Mathf.Exp(time)) * ((float)watch.Elapsed.TotalSeconds / time);
                return Mathf.Lerp(debutArea, endArea, deploymentTime);
            }
            else { return endArea; }
        }

        //Drag formula calculations
        private float DragCalculation(float area, float Cd)
        {
            return (float)atmDensity * sqrSpeed * Cd * area / 2000f;
        }

        //Calculates a part's drag
        private Vector3 DragForce(float startArea, float targetArea, float Cd, float time, bool mainChute)
        {
            return DragCalculation(DragDeployment(time, startArea, targetArea, mainChute), Cd) * dragVector;
        }
        #endregion
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

                if (autoArm) { Events["GUIArm"].guiActive = false; }
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
                this.part.mass = caseMass + chuteMass + secChuteMass;

                if (autoArm) { Actions["ActionArm"].active = false; }
            }
        }

        private void FixedUpdate()
        {
            //Flight values
            if (!CompatibilityChecker.IsCompatible() || !HighLogic.LoadedSceneIsFlight || FlightGlobals.ActiveVessel == null || this.part.Rigidbody == null) { return; }
            currentTime = Time.fixedTime;
            pos = this.part.transform.position;
            ASL = FlightGlobals.getAltitudeAtPos(pos);
            terrainAlt = this.vessel.mainBody.pqsController != null ? this.vessel.pqsAltitude : 0d;
            if (this.vessel.mainBody.ocean && terrainAlt < 0) { terrainAlt = 0; }
            trueAlt = ASL - terrainAlt;
            atmPressure = FlightGlobals.getStaticPressure(ASL, this.vessel.mainBody);
            if (atmPressure <= 1E-6) { atmPressure = 0; }
            atmDensity = FlightGlobals.getAtmDensity(atmPressure);
            sqrSpeed = (this.part.Rigidbody.velocity + Krakensbane.GetFrameVelocityV3f()).sqrMagnitude;
            dragVector = -(this.part.Rigidbody.velocity + Krakensbane.GetFrameVelocityV3f()).normalized;
            forcePosition = this.parachute.transform.position;
            if (secondaryChute) { secForcePosition = this.secParachute.transform.position; }
            if (GameSettings.LAUNCH_STAGES.GetKeyDown() && this.vessel.isActiveVessel && this.part.inverseStage == Staging.CurrentStage && !this.staged) { ActivateRC(); }
            
            if (this.staged)
            {
                //Checks if the parachute must disarm
                if (armed)
                {
                    this.part.stackIcon.SetIconColor(XKCDColors.LightCyan);
                    if (CanDeployChute(true) || (secondaryChute && CanDeployChute(false))) { armed = false; }
                }
                //Parachute deployments
                else
                {
                    //Main chute
                    if (CanDeployChute(true))
                    {
                        oneWasDeployed = true;
                        if (!wait)
                        {
                            switch (deploymentState)
                            {
                                case DeploymentStates.STOWED:
                                    {
                                        this.part.stackIcon.SetIconColor(XKCDColors.LightCyan);
                                        if (trueAlt > deploymentAlt && MinDeployment(minDeployment, minPressure, minIsPressure) && RandomDeployment(true)) { PreDeploy(true); }
                                        else if (trueAlt <= deploymentAlt && RandomDeployment(true)) { LowDeploy(true); }
                                        break;
                                    }

                                case DeploymentStates.PREDEPLOYED:
                                    {
                                        FollowDragDirection(parachute, GetForcedVector(parachute, forcedOrientation));
                                        ParachuteNoise(parachute);
                                        this.part.rigidbody.AddForceAtPosition(DragForce(0, preDeployedArea, mat.dragCoefficient, preDeploymentSpeed, true), forcePosition, ForceMode.Force);
                                        if (trueAlt <= deploymentAlt) { Deploy(true); }
                                        break;
                                    }
                                case DeploymentStates.LOWDEPLOYED:
                                    {
                                        FollowDragDirection(parachute, GetForcedVector(parachute, forcedOrientation));
                                        ParachuteNoise(parachute);
                                        this.part.rigidbody.AddForceAtPosition(/*huehuehue*/-DragForce(0, deployedArea, mat.dragCoefficient, preDeploymentSpeed + deploymentSpeed, true), forcePosition, ForceMode.Force);
                                        if (!this.part.CheckAnimationPlaying(preDeploymentAnimation) && !this.played)
                                        {
                                            dragTimer.Reset();
                                            dragTimer.Start();
                                            this.part.PlayAnimation(deploymentAnimation, 1 / deploymentSpeed);
                                            this.played = true;
                                        }
                                        break;
                                    }

                                case DeploymentStates.DEPLOYED:
                                    {
                                        FollowDragDirection(parachute, GetForcedVector(parachute, forcedOrientation));
                                        ParachuteNoise(parachute);
                                        this.part.rigidbody.AddForceAtPosition(/*huehuehue*/-DragForce(preDeployedArea, deployedArea, mat.dragCoefficient, deploymentSpeed, true), forcePosition, ForceMode.Force);
                                        if (!this.part.CheckAnimationPlaying(preDeploymentAnimation) && !this.played)
                                        {
                                            dragTimer.Reset();
                                            dragTimer.Start();
                                            this.part.PlayAnimation(deploymentAnimation, 1 / deploymentSpeed);
                                            this.played = true;
                                        }
                                        break;
                                    }
                                default:
                                    break;
                            }
                        }
                        else { CheckForWait(); }
                    }
                    //Deactivation
                    else if (!CanDeployChute(true) && IsDeployed(deploymentState)) { Cut(true); }

                    //Secondary chute
                    if (secondaryChute && CanDeployChute(false))
                    {
                        oneWasDeployed = true;
                        if (!wait)
                        {

                            switch (secDeploymentState)
                            {
                                case DeploymentStates.STOWED:
                                    {
                                        this.part.stackIcon.SetIconColor(XKCDColors.LightCyan);
                                        if (trueAlt > secDeploymentAlt && MinDeployment(secMinDeployment, secMinPressure, secMinIsPressure) && RandomDeployment(false)) { PreDeploy(false); }
                                        else if (trueAlt <= secDeploymentAlt && RandomDeployment(false)) { LowDeploy(false); }
                                        break;
                                    }

                                case DeploymentStates.PREDEPLOYED:
                                    {
                                        FollowDragDirection(secParachute, GetForcedVector(secParachute, secForcedOrientation));
                                        ParachuteNoise(secParachute);
                                        this.part.rigidbody.AddForceAtPosition(DragForce(0, secPreDeployedArea, secMat.dragCoefficient, secPreDeploymentSpeed, false), secForcePosition, ForceMode.Force);
                                        if (trueAlt <= secDeploymentAlt) { Deploy(false); }
                                        break;
                                    }
                                case DeploymentStates.LOWDEPLOYED:
                                    {
                                        FollowDragDirection(secParachute, GetForcedVector(secParachute, secForcedOrientation));
                                        ParachuteNoise(secParachute);
                                        this.part.rigidbody.AddForceAtPosition(DragForce(0, secDeployedArea, secMat.dragCoefficient, secPreDeploymentSpeed + secDeploymentSpeed, false), secForcePosition, ForceMode.Force);
                                        if (!this.part.CheckAnimationPlaying(secPreDeploymentAnimation) && !this.secPlayed)
                                        {
                                            secDragTimer.Reset();
                                            secDragTimer.Start();
                                            this.part.PlayAnimation(secDeploymentAnimation, 1 / secDeploymentSpeed);
                                            this.secPlayed = true;
                                        }
                                        break;
                                    }

                                case DeploymentStates.DEPLOYED:
                                    {
                                        FollowDragDirection(secParachute, GetForcedVector(secParachute, secForcedOrientation));
                                        ParachuteNoise(secParachute);
                                        this.part.rigidbody.AddForceAtPosition(DragForce(secPreDeployedArea, secDeployedArea, secMat.dragCoefficient, secDeploymentSpeed, false), secForcePosition, ForceMode.Force);
                                        {
                                            secDragTimer.Reset();
                                            secDragTimer.Start();
                                            this.part.PlayAnimation(secDeploymentAnimation, 1 / secDeploymentSpeed);
                                            this.secPlayed = true;
                                        }
                                        break;
                                    }
                                default:
                                    break;
                            }
                        }
                        else { CheckForWait(); }
                    }
                    //Deactivation
                    else if (!CanDeployChute(false) && IsDeployed(secDeploymentState)) { Cut(false); }

                    //If both parachutes must be cut
                    if (bothMustStop)
                    {
                        if (IsDeployed(deploymentState)) { Cut(true); }

                        if (IsDeployed(secDeploymentState)) { Cut(false); }

                        SetRepack();
                    }

                    //Allows both parachutes to be cut at the same time if both are deployed
                    if (secondaryChute && IsDeployed(deploymentState) && IsDeployed(secDeploymentState)) { Events["GUICutBoth"].active = true; }
                    else { Events["GUICutBoth"].active = false; }

                    //If the parachute can't be deployed
                    if (!oneWasDeployed && !autoArm)
                    {
                        failedTimer.Reset();
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

            //Gets the materials
            materials.TryGetMaterial(material, ref mat);
            if (secondaryChute && !materials.TryGetMaterial(secMaterial, ref secMat)) { secMat = mat; }

            //Autoarming checkup
            ConfigNode.Load(RCUtils.settingsURL).GetNode("REALCHUTE_SETTINGS").TryGetValue("autoArm", ref autoArm);

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

            if (autoArm)
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
            this.cap = this.part.FindModelTransform(capName);
            this.parachute = this.part.FindModelTransform(parachuteName);
            this.parachute.gameObject.SetActive(false);
            this.part.InitiateAnimation(preDeploymentAnimation);
            this.part.InitiateAnimation(deploymentAnimation);

            //Initiates the second parachute animations if any
            if (secondaryChute)
            {
                this.secCap = this.part.FindModelTransform(secCapName);
                this.secParachute = this.part.FindModelTransform(secParachuteName);
                this.secParachute.gameObject.SetActive(false);
                this.part.InitiateAnimation(secPreDeploymentAnimation);
                this.part.InitiateAnimation(secDeploymentAnimation);
            }

            //First initiation of the part
            if (!initiated)
            {
                deploymentState = DeploymentStates.STOWED;
                secDeploymentState = DeploymentStates.STOWED;
                depState = GetStateString(deploymentState);
                secDepState = GetStateString(secDeploymentState);
                initiated = true;
                capOff = false;
                played = false;
                secPlayed = false;
                armed = false;
                this.cap.gameObject.SetActive(true);
                if (secondaryChute) { this.secCap.gameObject.SetActive(true); }

                if (spareChutes >= 0) { chuteCount = (int)spareChutes; }
            }

            //Flight loading
            if (HighLogic.LoadedSceneIsFlight)
            {
                deploymentState = GetState(depState);
                secDeploymentState = GetState(secDepState);
                //If the part has been staged in the past
                if (capOff)
                {
                    this.part.stackIcon.SetIconColor(XKCDColors.Red);
                    cap.gameObject.SetActive(false);
                    if (secondaryChute)
                    {
                        secCap.gameObject.SetActive(false);
                    }
                }
                System.Random random = new System.Random();
                this.randomTime = (float)random.NextDouble();
                this.secRandomTime = (float)random.NextDouble();
            }

            //GUI
            window = new Rect(200, 100, 350, 400);
        }

        public override void OnLoad(ConfigNode node)
        {
            if (!CompatibilityChecker.IsCompatible()) { return; }
            //Gets the materials
            materials.TryGetMaterial(material, ref mat);
            if (secondaryChute && !materials.TryGetMaterial(secMaterial, ref secMat)) { secMat = mat; }
            this.part.mass = caseMass + chuteMass + secChuteMass;
        }

        public override string GetInfo()
        {
            if (!CompatibilityChecker.IsCompatible()) { return string.Empty; }
            //Info in the editor part window
            string infoList = string.Empty;
            infoList = String.Format("Parachute material: {0}\n", material);
            if (secondaryChute && secMaterial != material && secMaterial != "empty") { infoList += String.Format("Secondary parachute material: {0}\n", secMaterial); }
            if (material != secMaterial) { infoList += String.Format("Drag coefficient: {0}\n", mat.dragCoefficient); }
            else { infoList += String.Format("Drag coefficients: {0}, {1}\n", mat.dragCoefficient, secMat.dragCoefficient); }
            if (timer > 0) { infoList += String.Format("Deployment timer: {0}s\n", timer); }
            if (mustGoDown) { infoList += "Must go downwards to deploy: true\n"; }
            if (spareChutes >= 0) { infoList += String.Format("Spare chutes: {0}\n", spareChutes); }
            if (!secondaryChute)
            {
                infoList += String.Format("Total parachute mass: {0}t\n", (caseMass + chuteMass).ToString("0.####"));
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
                infoList += String.Format("Total parachute mass: {0}t\n", (caseMass + chuteMass + secChuteMass).ToString("0.####"));
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
            GUILayout.Label("Spare chutes: " + chuteCount, skins.label);
            GUILayout.Label("___________________________________________", RCUtils.boldLabel);
            GUILayout.Space(3);
            GUILayout.Label("Main chute:", RCUtils.boldLabel, GUILayout.Width(120));
            GUILayout.Label("Material: " + mat.name, skins.label);
            GUILayout.Label("Drag coefficient: " + mat.dragCoefficient.ToString("0.00"), skins.label);
            GUILayout.Label("Predeployed diameter: " + preDeployedDiameter + "m    area:" + preDeployedArea.ToString("0.###") + "m²", skins.label);
            GUILayout.Label("Deployed diameter: " + deployedDiameter + "m    area:" + deployedArea.ToString("0.###") + "m²", skins.label);
            if (HighLogic.LoadedSceneIsFlight)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Predeployment:", skins.label);
                if (GUILayout.Toggle(!minIsPressure, "altitude", skins.toggle)) { minIsPressure = false; }
                GUILayout.FlexibleSpace();
                if (GUILayout.Toggle(minIsPressure, "pressure", skins.toggle)) { minIsPressure = true; }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            if (minIsPressure)
            {
                GUILayout.Label("Predeployment pressure: " + minPressure + "atm", skins.label);
                if (HighLogic.LoadedSceneIsFlight)
                {
                    minPressure = GUILayout.HorizontalSlider(minPressure, 0.005f, 1, skins.horizontalSlider, skins.horizontalSliderThumb);
                    if (secondaryChute)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Copy to sec", skins.button, GUILayout.Height(20), GUILayout.Width(100)))
                        {
                            secMinIsPressure = minIsPressure;
                            secMinPressure = minPressure;
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }
            }
            else
            {
                GUILayout.Label("Predeployment altitude: " + minDeployment + "m", skins.label);
                if (HighLogic.LoadedSceneIsFlight)
                {
                    minDeployment = GUILayout.HorizontalSlider(minDeployment, 100, 20000, skins.horizontalSlider, skins.horizontalSliderThumb);
                    if (secondaryChute)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Copy to sec", skins.button, GUILayout.Height(20), GUILayout.Width(100)))
                        {
                            secMinIsPressure = minIsPressure;
                            secMinDeployment = minDeployment;
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }
            }
            GUILayout.Label("Deployment altitude: " + deploymentAlt + "m", skins.label);
            if (HighLogic.LoadedSceneIsFlight)
            {
                deploymentAlt = GUILayout.HorizontalSlider(deploymentAlt, 50, 10000, skins.horizontalSlider, skins.horizontalSliderThumb);
                if (secondaryChute)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Copy to sec", skins.button, GUILayout.Height(20), GUILayout.Width(100))) { secDeploymentAlt = deploymentAlt; }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }
            if (cutAlt > 0) { GUILayout.Label("Autocut altitude: " + cutAlt + "m", skins.label); }
            GUILayout.Label("Predeployment speed: " + preDeploymentSpeed + "s", skins.label);
            GUILayout.Label("Deployment speed: " + deploymentSpeed + "s", skins.label);
            if (secondaryChute)
            {
                GUILayout.Label("___________________________________________", RCUtils.boldLabel);
                GUILayout.Space(3);
                GUILayout.Label("Secondary chute:", RCUtils.boldLabel, GUILayout.Width(120));
                GUILayout.Label("Material: " + secMat.name, skins.label);
                GUILayout.Label("Drag coefficient: " + secMat.dragCoefficient.ToString("0.00"), skins.label);
                GUILayout.Label("Predeployed diameter: " + secPreDeployedDiameter + "m    area:" + secPreDeployedArea.ToString("0.###") + "m²", skins.label);
                GUILayout.Label("Deployed diameter: " + secDeployedDiameter + "m    area:" + secDeployedArea.ToString("0.###") + "m²", skins.label);
                if (HighLogic.LoadedSceneIsFlight)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Predeployment:", skins.label);
                    if (GUILayout.Toggle(!secMinIsPressure, "altitude", skins.toggle)) { secMinIsPressure = false; }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Toggle(secMinIsPressure, "pressure", skins.toggle)) { secMinIsPressure = true; }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                if (secMinIsPressure)
                {
                    GUILayout.Label("Predeployment pressure: " + secMinPressure + "atm", skins.label);
                    if (HighLogic.LoadedSceneIsFlight)
                    {
                        secMinPressure = GUILayout.HorizontalSlider(secMinPressure, 0.005f, 1, skins.horizontalSlider, skins.horizontalSliderThumb);
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Copy to main", skins.button, GUILayout.Height(20), GUILayout.Width(100)))
                        {
                            minIsPressure = secMinIsPressure;
                            minPressure = secMinPressure;
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }
                else
                {
                    GUILayout.Label("Predeployment altitude: " + secMinDeployment + "m", skins.label);
                    if (HighLogic.LoadedSceneIsFlight)
                    {
                        secMinDeployment = GUILayout.HorizontalSlider(secMinDeployment, 100, 20000, skins.horizontalSlider, skins.horizontalSliderThumb);
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Copy to main", skins.button, GUILayout.Height(20), GUILayout.Width(100)))
                        {
                            minIsPressure = secMinIsPressure;
                            minDeployment = secMinDeployment;
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.Label("Deployment altitude: " + secDeploymentAlt + "m", skins.label);
                if (HighLogic.LoadedSceneIsFlight)
                {
                    secDeploymentAlt = GUILayout.HorizontalSlider(secDeploymentAlt, 50, 10000, skins.horizontalSlider, skins.horizontalSliderThumb);
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Copy to main", skins.button, GUILayout.Height(20), GUILayout.Width(100))) { deploymentAlt = secDeploymentAlt; }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                if (secCutAlt > 0) { GUILayout.Label("Autocut altitude: " + secCutAlt + "m", skins.label); }
                GUILayout.Label("Predeployment speed: " + secPreDeploymentSpeed + "s", skins.label);
                GUILayout.Label("Deployment speed: " + secDeploymentSpeed + "s", skins.label);
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
            GUI.DragWindow();
        }
        #endregion
    }
}