using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and *
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives. *
 * For more informtion contact me on the forum. */


namespace RealChute
{
    //------------------------- Part Module -------------------------
    public class RealChuteModule : PartModule
    {
        #region Config values
        // Values from the .cfg file
        [KSPField]
        public float caseMass = 0.1f;
        [KSPField]
        public string material = "Nylon";
        [KSPField]
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

        //Main parachute
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Predep diam", guiFormat = "0.#"), UI_FloatRange(minValue = 0.5f, maxValue = 70, stepIncrement = 0.5f)]
        public float preDeployedDiameter = 1;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Dep diam", guiFormat = "0.#"), UI_FloatRange(minValue = 0.5f, maxValue = 70, stepIncrement = 0.5f)]
        public float deployedDiameter = 25;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Predep"), UI_Toggle(enabledText = "pressure", disabledText = "altitude")]
        public bool minIsPressure = false;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Predep alt"), UI_FloatRange(minValue = 50, maxValue = 50000, stepIncrement = 100)]
        public float minDeployment = 40000;
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
        public Vector3 forcedOrientation = Vector3.zero;

        //Second parachute
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Predep2 diam", guiFormat = "0.#"), UI_FloatRange(minValue = 0.5f, maxValue = 70, stepIncrement = 0.5f)]
        public float secPreDeployedDiameter = 1;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Dep2 diam", guiFormat = "0.#"), UI_FloatRange(minValue = 0.5f, maxValue = 70, stepIncrement = 0.5f)]
        public float secDeployedDiameter = 25;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Predep2"), UI_Toggle(enabledText = "pressure", disabledText = "altitude")]
        public bool secMinIsPressure = false;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Predep2 alt"), UI_FloatRange(minValue = 50, maxValue = 50000, stepIncrement = 100)]
        public float secMinDeployment = 40000;
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
        public Vector3 secForcedOrientation = Vector3.zero;

        #endregion

        #region Persistant values
        //Persistant values
        [KSPField(isPersistant = true)]
        public bool initiated = false;
        [KSPField(isPersistant = true)]
        public bool capOff = false;
        [KSPField(isPersistant = true, guiActive = true, guiName = "Spare chutes")]
        public float chuteCount = 5;

        #endregion

        #region Variables
        //Variables
        private Vector3 dragVector, up;
        public Vector3 forcePosition, secForcePosition;
        private Vector3 phase = Vector3.zero, secPhase = Vector3.zero;
        private Vector3d pos;
        private Animation anim;
        public Transform parachute, secParachute;
        public Transform cap, secCap;
        private RaycastHit craft;
        public string deploymentState = "STOWED", secDeploymentState = "STOWED";
        private double terrainAlt, ASL, trueAlt;
        private double atmPressure, atmDensity;
        public float preDeployedArea, secPreDeployedArea;
        public float deployedArea, secDeployedArea;
        public float parachuteDensity, dragCoef, chuteArea;
        public float secParachuteDensity, secDragCoef;
        public float chuteMass, secChuteMass;
        private float sqrSpeed;
        private float currentTime, debutTime, deltaTime, deploymentTime;
        public float random_x, random_y;
        public float secRandom_x, secRandom_y;
        public float randomDebut, randomDelta, randomTime;
        public float secRandomDebut, secRandomDelta, secRandomTime;
        private bool queued = false, secQueued = false;
        private bool timerSet = false, randomized = false, secRandomized = false;
        private bool randomSet = false, secRandomSet = false;
        private bool wait = true, armed = false, oneWasDeployed = false;
        private bool timeSet = false;
        private bool fullPlayed = false, secFullPlayed = false;

        #endregion

        #region Animations
        //------------------------- Animations -------------------------
        private void InitiateAnimation(string animationName)
        {
            //Initiates the default values for animations
            foreach (Animation anim in part.FindModelAnimators(animationName))
            {
                AnimationState state = anim[animationName];
                state.normalizedTime = 0;
                state.normalizedSpeed = 0;
                state.enabled = false;
                state.wrapMode = WrapMode.Clamp;
                state.layer = 1;
            }
        }

        private void PlayAnimation(string animationName, float animationSpeed)
        {
            //Plays the animation
            foreach (Animation anim in part.FindModelAnimators(animationName))
            {
                AnimationState state = anim[animationName];
                state.normalizedTime = 0;
                state.normalizedSpeed = animationSpeed;
                state.enabled = true;
                anim.Play(animationName);
            }
        }

        public bool CheckAnimationPlaying(string animationName)
        {
            //Checks if a given animation is playing
            bool isPlaying = false;
            foreach (Animation anim in part.FindModelAnimators(animationName)) { isPlaying = anim.IsPlaying(animationName); }
            return isPlaying;
        }

        #endregion

        #region Part GUI
        //------------------------- Part GUI -------------------------
        [KSPEvent(guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Deploy Chute")]
        public void GUIDeploy()
        {
            //Forces the parachute to deploy
            this.part.force_activate();
            Events["GUIDeploy"].active = false;
            Events["GUIArm"].active = false;
        }

        [KSPEvent(guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Cut main chute")]
        public void GUICut()
        {
            //Cuts main chute chute
            Cut("MainChute");
        }

        [KSPEvent(guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Cut secondary chute")]
        public void GUISecCut()
        {
            //Cuts secondary chute
            Cut("SecChute");
        }

        [KSPEvent(guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Cut both chutes")]
        public void GUICutBoth()
        {
            Cut("MainChute");
            Cut("SecChute");
            Events["GUICutBoth"].active = false;
        }

        [KSPEvent(guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Arm parachute")]
        public void GUIArm()
        {
            armed = true;
            this.part.force_activate();
            Events["GUIDeploy"].active = false;
            Events["GUIArm"].active = false;
        }

        [KSPEvent(guiActive = false, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Repack chute", unfocusedRange = 5)]
        public void GUIRepack()
        {
            //Repacks chute from EVA if in space or on the ground
            if (CanRepack()) { Repack(); }
        }

        #endregion

        #region Action groups
        //------------------------- Action groups -------------------------
        [KSPAction("Deploy chute")]
        public void ActionDeploy(KSPActionParam param)
        {
            //Forces the parachute to deploy
            this.part.force_activate();
            Events["GUIDeploy"].active = false;
            Events["GUIArm"].active = false;
        }

        [KSPAction("Cut main chute")]
        public void ActionCut(KSPActionParam param)
        {
            //Cuts main chute
            if (IsDeployed(deploymentState)) { Cut("MainChute"); }
        }

        [KSPAction("Cut secondary chute")]
        public void ActionSecCut(KSPActionParam param)
        {
            //Cuts secondary chute
            if (IsDeployed(secDeploymentState)) { Cut("SecChute"); }
        }

        [KSPAction("Cut both chutes")]
        public void ActionCutBoth(KSPActionParam param)
        {
            if (IsDeployed(deploymentState) && IsDeployed(secDeploymentState))
            {
                Cut("MainChute");
                Cut("SecChute");
                Events["GUICutBoth"].active = false;
            }
        }

        [KSPAction("Arm parachute")]
        public void ActionArm(KSPActionParam param)
        {
            //Arms parachute
            armed = true;
            this.part.force_activate();
            Events["GUIDeploy"].active = false;
            Events["GUIArm"].active = false;
        }

        #endregion

        #region Methods
        //------------------------- Methods -------------------------
        public bool CheckGroundStop()
        {
            //Checks if the vessel is on the ground and has stopped moving
            if (this.vessel.LandedOrSplashed && this.vessel.horizontalSrfSpeed < cutSpeed) { return true; }

            else { return false; }
        }

        public bool MinDeployment(float minDeploy, float minPress, bool isPressure)
        {
            //Returns the right value to check
            if (isPressure)
            {
                if (atmPressure >= minPress) { return true; }

                else { return false; }
            }

            else
            {
                if (trueAlt <= minDeploy) { return true; }

                else { return false; }
            }
        }

        public void CheckForWait()
        {
            //Checks if there is a timer and/or a mustGoDown clause active
            bool timerSpent = true;
            bool goesDown = true;

            //Timer
            if (timer > 0 && deltaTime < timer)
            {
                timerSpent = false;
                if (!timerSet)
                {
                    debutTime = currentTime;
                    timerSet = true;
                }

                deltaTime = currentTime - debutTime;
                ScreenMessages.PostScreenMessage(String.Format("Deployment in {0}s", (timer - deltaTime).ToString("0.#")), Time.fixedDeltaTime, ScreenMessageStyle.UPPER_CENTER);
            }

            //Goes down
            if (mustGoDown && this.vessel.verticalSpeed >= 0)
            {
                goesDown = false;
                ScreenMessages.PostScreenMessage("Deployment awaiting negative vertical velocity", Time.fixedDeltaTime, ScreenMessageStyle.UPPER_CENTER);
                ScreenMessages.PostScreenMessage(String.Format("Current vertical velocity: {0}m/s", this.vessel.verticalSpeed.ToString("0.#")), Time.fixedDeltaTime, ScreenMessageStyle.UPPER_CENTER);
            }


            //Can deploy or not
            if (timerSpent && goesDown) { wait = false; }

            else
            {
                this.part.stackIcon.SetIconColor(XKCDColors.LightCyan);
                wait = true;
            }
        }

        public bool RandomDeployment(string whichChute)
        {
            //Short random deployment delayer
            if (whichChute == "MainChute")
            {
                if (!randomSet)
                {
                    this.randomDebut = currentTime;
                    this.randomSet = true;
                }

                this.randomDelta = currentTime - this.randomDebut;
                if (this.randomDelta < this.randomTime) { return false; }

                else { return true; }
            }

            else
            {
                if (!secRandomSet)
                {
                    this.secRandomDebut = currentTime;
                    this.secRandomSet = true;
                }

                this.secRandomDelta = currentTime - this.secRandomDebut;
                if (this.secRandomDelta < this.secRandomTime) { return false; }

                else { return true; }
            }
            
        }

        public bool CanDeployChute(string whichChute)
        {
            //Automatically returns false if the craft is stopped on the ground or out of an atmosphere
            if (CheckGroundStop() || atmPressure == 0) { return false; }

            //Checks if the main chute can deploy
            else if (whichChute == "MainChute")
            {
                if (deploymentState == "CUT") { return false; }

                else if (MinDeployment(minDeployment, minPressure, minIsPressure) && cutAlt == -1) { return true; }

                else if (MinDeployment(minDeployment, minPressure, minIsPressure) && trueAlt > cutAlt) { return true; }

                else if (secondaryChute && !MinDeployment(minDeployment, minPressure, minIsPressure) && trueAlt <= secCutAlt) { return true; }

                else { return false; }
            }

            //Checks if the secondary chute can deploy
            else if (whichChute == "SecChute")
            {
                if (secDeploymentState == "CUT") { return false; }

                else if (MinDeployment(secMinDeployment, secMinPressure, secMinIsPressure) && secCutAlt == -1) { return true; }

                else if (MinDeployment(secMinDeployment, secMinPressure, secMinIsPressure) && trueAlt > secCutAlt) { return true; }

                else if (!MinDeployment(secMinDeployment, secMinPressure, secMinIsPressure) && trueAlt <= cutAlt) { return true; }

                else { return false; }
            }

            else { return false; }
        }

        public bool IsDeployed(string deployState)
        {
            //Check if the chute is completely deployed
            if (deployState == "DEPLOYED" || deployState == "LOWDEPLOYED" || deployState == "PREDEPLOYED") { return true; }

            else { return false; }
        }

        public bool BothMustStop()
        {
            //Checks if both parachutes must stop
            if (secondaryChute && (CheckGroundStop() || atmPressure == 0))
            {
                if (deploymentState == "CUT" || secDeploymentState == "CUT") { return true; }

                else { return false; }
            }

            else { return false; }
        }

        public bool CanRepack()
        {
            //Checks if can repack
            if (CheckGroundStop() || atmPressure == 0)
            {
                if (!secondaryChute && deploymentState == "CUT") { return true; }

                else if (secondaryChute && deploymentState == "CUT" && secDeploymentState == "CUT") { return true; }

                else { return false; }
            }

            else { return false; }
        }

        private void ParachuteNoise(Transform chute)
        {
            //Gives a random noise to the parachute orientation
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

        private Vector3 LerpDrag(Transform chute, Vector3 to)
        {
            if (chute == parachute)
            {
                if (phase.magnitude < (to.magnitude - 0.01f) || phase.magnitude > (to.magnitude + 0.01f)) { phase = Vector3.Lerp(phase, to, 0.1f); }

                else { phase = to; }

                return phase;
            }

            else
            {
                if (secPhase.magnitude < (to.magnitude - 0.01f) || secPhase.magnitude > (to.magnitude + 0.01f)) { secPhase = Vector3.Lerp(secPhase, to, 0.1f); }

                else { secPhase = to; }

                return secPhase;
            }
        }

        private void GetDragDirection(Transform chute, Vector3 forced)
        {
            //Smoothes the forced vector
            Vector3 orient = Vector3.zero;
            if (secondaryChute && IsDeployed(deploymentState) && IsDeployed(secDeploymentState)) { orient = LerpDrag(chute, forced); }

            else if (secondaryChute && (!IsDeployed(deploymentState) || !IsDeployed(secDeploymentState))) { orient = LerpDrag(chute, Vector3.zero); }

            //Makes the parachute follow air flow
            Quaternion drag = Quaternion.identity;
            if (reverseOrientation) { drag = Quaternion.LookRotation(-(dragVector + chute.TransformDirection(orient)).normalized, chute.up); }

            else { drag = Quaternion.LookRotation((dragVector + chute.TransformDirection(orient)).normalized, chute.up); }

            chute.rotation = drag;
        }

        #endregion

        #region Deployment code
        //------------------------- Deployment code -------------------------
        public void LowDeploy(string whichChute)
        {
                this.part.stackIcon.SetIconColor(XKCDColors.RadioactiveGreen);
                capOff = true;
                Events["GUIDeploy"].active = false;
                Events["GUIArm"].active = false;
                this.part.Effect("rcdeploy");
                if (whichChute == "MainChute")
                {
                    deploymentState = "LOWDEPLOYED";
                    parachute.gameObject.SetActive(true);
                    cap.gameObject.SetActive(false);
                    Events["GUICut"].active = true;
                    PlayAnimation(preDeploymentAnimation, 1 / preDeploymentSpeed);
                }

                else if (whichChute == "SecChute")
                {
                    secDeploymentState = "LOWDEPLOYED";
                    secParachute.gameObject.SetActive(true);
                    secCap.gameObject.SetActive(false);
                    Events["GUISecCut"].active = true;
                    PlayAnimation(secPreDeploymentAnimation, 1 / secPreDeploymentSpeed);
                }
        }

        public void PreDeploy(string whichChute)
        {
            //Parachute predeployment code
            this.part.stackIcon.SetIconColor(XKCDColors.BrightYellow);
            capOff = true;
            Events["GUIDeploy"].active = false;
            Events["GUIArm"].active = false;
            this.part.Effect("rcpredeploy");
            if (whichChute == "MainChute")
            {
                deploymentState = "PREDEPLOYED";
                parachute.gameObject.SetActive(true);
                cap.gameObject.SetActive(false);
                Events["GUICut"].active = true;
                PlayAnimation(preDeploymentAnimation, 1 / preDeploymentSpeed);
            }

            else if (whichChute == "SecChute")
            {
                secDeploymentState = "PREDEPLOYED";
                secParachute.gameObject.SetActive(true);
                secCap.gameObject.SetActive(false);
                Events["GUISecCut"].active = true;
                PlayAnimation(secPreDeploymentAnimation, 1 / secPreDeploymentSpeed);
            }

        }

        public void Deploy(string whichChute)
        {
            //Parachute full deployment code
            this.part.stackIcon.SetIconColor(XKCDColors.RadioactiveGreen);
            this.part.Effect("rcdeploy");
            if (whichChute == "MainChute")
            {
                deploymentState = "DEPLOYED";
                if (!CheckAnimationPlaying(preDeploymentAnimation))
                {
                    PlayAnimation(deploymentAnimation, 1 / deploymentSpeed);
                    fullPlayed = true;
                }

                else { fullPlayed = false; }
            }

            else if (whichChute == "SecChute")
            {
                secDeploymentState = "DEPLOYED";
                if (!CheckAnimationPlaying(secPreDeploymentAnimation))
                {
                    PlayAnimation(secDeploymentAnimation, 1 / secDeploymentSpeed);
                    secFullPlayed = true;
                }

                else { secFullPlayed = false; }
            }
        }

        public void StagingReset()
        {
            //Deactivates the part
            this.part.deactivate();
            armed = false;
            if (this.part.inverseStage != 0) { this.part.inverseStage = this.part.inverseStage - 1; }
            else { this.part.inverseStage = Staging.CurrentStage; }
        }

        public void Cut(string whichChute)
        {
            //Cuts the chute
            this.part.Effect("rccut");
            if (whichChute == "MainChute")
            {
                deploymentState = "CUT";
                parachute.gameObject.SetActive(false);
                Events["GUICut"].active = false;
                queued = false;
                if (!secondaryChute || secDeploymentState == "CUT") { SetRepack(); }

                else if (secondaryChute && secDeploymentState == "STOWED") { armed = true; }
            }

            else if (whichChute == "SecChute")
            {
                secDeploymentState = "CUT";
                secParachute.gameObject.SetActive(false);
                Events["GUISecCut"].active = false;
                secQueued = false;
                if (deploymentState == "CUT") { SetRepack(); }

                else if (deploymentState == "STOWED") { armed = true; }
            }
        }

        public void SetRepack()
        {
            //Allows the chute to be repacked if available
            this.part.stackIcon.SetIconColor(XKCDColors.Red);
            timeSet = false;
            timerSet = false;
            wait = true;
            StagingReset();
            if (chuteCount > 0 || chuteCount == -1) { Events["GUIRepack"].guiActiveUnfocused = true; }
        }

        public void Repack()
        {
            //Repacks a cut chute
            if (chuteCount > 0 || chuteCount == -1)
            {
                this.part.Effect("rcrepack");
                Events["GUIRepack"].guiActiveUnfocused = false;
                Events["GUIDeploy"].active = true;
                Events["GUIArm"].active = true;
                oneWasDeployed = false;
                randomSet = false;
                fullPlayed = false;
                secFullPlayed = false;
                deploymentState = "STOWED";
                cap.gameObject.SetActive(true);

                this.part.stackIcon.SetIconColor(XKCDColors.White);
                capOff = false;
                if (chuteCount != -1) { chuteCount--; }

                if (secondaryChute)
                {
                    secDeploymentState = "STOWED";
                    secCap.gameObject.SetActive(true);
                }
            }
        }

        #endregion

        #region Drag code
        //------------------------- Drag code -------------------------
        private float GetArea(float diameter)
        {
            return Mathf.Pow(diameter, 2) * Mathf.PI / 4;
        }

        private float DragDeployment(float time, float debutArea, float endArea)
        {
            //Calculates how much drag a part has since deployment
            if (!timeSet)
            {
                debutTime = currentTime;
                timeSet = true;
            }

            deltaTime = currentTime - debutTime;
            if (deltaTime <= time)
            {
                deploymentTime = (Mathf.Exp(deltaTime) / Mathf.Exp(time)) * (deltaTime / time);
                return Mathf.Lerp(debutArea, endArea, deploymentTime);
            }

            else { return endArea; }
        }

        private float DragCalculation(float area, float Cd)
        {
            return (float)atmDensity * sqrSpeed * Cd * area / 2000;
        }

        private Vector3 DragForce(float startArea, float targetArea, float Cd, float time)
        {
            //Calculates a part's drag
            float chuteArea = DragDeployment(time, startArea, targetArea);
            return DragCalculation(chuteArea, Cd) * dragVector;
        }

        #endregion

        #region Activation code
        //------------------------- Activation code -------------------------
        public override void OnStart(PartModule.StartState state)
        {
            //Initiates the part
            this.part.stagingIcon = "PARACHUTES";
            deploymentState = "STOWED";
            secDeploymentState = "STOWED";

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

            //Tweakables tooltip
            if (!secondaryChute)
            {
                Fields["forcedOrientation"].guiActiveEditor = false;
                Fields["secPreDeployedDiameter"].guiActiveEditor = false;
                Fields["secDeployedDiameter"].guiActiveEditor = false;
                Fields["secMinIsPressure"].guiActiveEditor = false;
                Fields["secMinDeployment"].guiActiveEditor = false;
                Fields["secMinPressure"].guiActiveEditor = false;
                Fields["secDeploymentAlt"].guiActiveEditor = false;
                Fields["secCutAlt"].guiActiveEditor = false;
                Fields["secPreDeploymentSpeed"].guiActiveEditor = false;
                Fields["secDeploymentSpeed"].guiActiveEditor = false;
                Fields["secForcedOrientation"].guiActiveEditor = false;
            }

            //Calculates parachute area
            preDeployedArea = GetArea(preDeployedDiameter);
            deployedArea = GetArea(deployedDiameter);
            if (secondaryChute)
            {
                secPreDeployedArea = GetArea(secPreDeployedDiameter);
                secDeployedArea = GetArea(secDeployedDiameter);
            }

            //Initiates animations
            anim = part.FindModelAnimators(capName).FirstOrDefault();
            this.cap = this.part.FindModelTransform(capName);
            this.parachute = this.part.FindModelTransform(parachuteName);
            parachute.gameObject.SetActive(false);
            InitiateAnimation(preDeploymentAnimation);
            InitiateAnimation(deploymentAnimation);

            //Initiates the second parachute animations if any
            if (secondaryChute)
            {
                this.secCap = this.part.FindModelTransform(secCapName);
                this.secParachute = this.part.FindModelTransform(secParachuteName);
                secParachute.gameObject.SetActive(false);
                InitiateAnimation(secPreDeploymentAnimation);
                InitiateAnimation(secDeploymentAnimation);
            }

            //First initiation of the part
            if (!initiated)
            {
                initiated = true;
                capOff = false;
                queued = false;
                secQueued = false;
                armed = false;
                cap.gameObject.SetActive(true);
                if (secondaryChute) { secCap.gameObject.SetActive(true); }

                if (spareChutes >= 0) { chuteCount = spareChutes; }
            }

            //If the part has been staged in the past
            if (capOff)
            {
                deploymentState = "CUT";
                cap.gameObject.SetActive(false);
                if (secondaryChute)
                {
                    secDeploymentState = "CUT";
                    secCap.gameObject.SetActive(false);
                }
            }

            //Sets random deployment times
            this.randomTime = UnityEngine.Random.value;
            this.secRandomTime = UnityEngine.Random.value;
        }

        public override void OnLoad(ConfigNode node)
        {
            //Gets values from the material definition file
            foreach (ConfigNode cfg in GameDatabase.Instance.GetConfigNodes("MATERIAL"))
            {
                if (cfg.HasValue("name") && cfg.GetValue("name") == material)
                {          
                    //Gets the drag coefficient
                    if(cfg.HasValue("dragCoefficient"))
                    {
                        float Cd;
                        if (float.TryParse(cfg.GetValue("dragCoefficient"), out Cd)) { dragCoef = Cd; }
                    }

                    //Gets the parachute density
                    if(cfg.HasValue("areaDensity"))
                    {
                        float aD;
                        if (float.TryParse(cfg.GetValue("areaDensity"), out aD)) { parachuteDensity = aD; }
                    }
                }

                if (secondaryChute && cfg.HasValue("name") && cfg.GetValue("name") == secMaterial)
                {
                    //Get second drag coefficient
                    if (cfg.HasValue("dragCoefficient"))
                    {
                        float Cd;
                        if (float.TryParse(cfg.GetValue("dragCoefficient"), out Cd)) { secDragCoef = Cd; }
                    }

                    //Get second area density
                    if (cfg.HasValue("areaDensity"))
                    {
                        float aD;
                        if (float.TryParse(cfg.GetValue("areaDensity"), out aD)) { secParachuteDensity = aD; }
                    }                  
                }
            }

            //Sets secondary parachute material
            if (secondaryChute && secMaterial == "empty")
            {
                secDragCoef = dragCoef;
                secParachuteDensity = parachuteDensity;
            }

            //Calculates mass for GetInfo()
            chuteMass = GetArea(deployedDiameter) * parachuteDensity;
            if (secondaryChute) { secChuteMass = GetArea(secDeployedDiameter) * secParachuteDensity; }
        }

        public override string GetInfo()
        { 
            //Info in the editor part window
            string infoList;
            infoList = String.Format("Parachute material: {0}\n", material);
            if (secondaryChute && secMaterial != material && secMaterial != "empty") { infoList += String.Format("Secondary parachute material: {0}\n", secMaterial); }

            if (material != secMaterial) { infoList += String.Format("Drag coefficient: {0}\n", dragCoef); }

            else { infoList += String.Format("Drag coefficients: {0}, {1}\n", dragCoef, secDragCoef); }

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

        public void Update()
        {
            //Tweakables pressure/altitude preddeployment clauses
            if (HighLogic.LoadedSceneIsEditor)
            {
                //main chute
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

                //Updates the spare chute count correctly
                chuteCount = spareChutes;
                if (spareChutes < 0) { Fields["chuteCount"].guiActive = false; }

                //Calculates parachute area
                preDeployedArea = GetArea(preDeployedDiameter);
                deployedArea = GetArea(deployedDiameter);
                if (secondaryChute)
                {
                    secPreDeployedArea = GetArea(secPreDeployedDiameter);
                    secDeployedArea = GetArea(secDeployedDiameter);
                }

                //Calculates parachute mass
                chuteMass = deployedArea * parachuteDensity;
                if (secondaryChute) { secChuteMass = secDeployedArea * secParachuteDensity; }

                else { secChuteMass = 0; }

                this.part.mass = caseMass + chuteMass + secChuteMass;
            }

            //To prevent weird shit
            else if (!HighLogic.LoadedSceneIsFlight || this.vessel.packed) { return; }

            //Universal values
            else
            {
                currentTime = Time.time;
                pos = this.part.transform.position;
                ASL = FlightGlobals.getAltitudeAtPos(pos);
                terrainAlt = this.vessel.pqsAltitude;
                if (this.vessel.mainBody.ocean && terrainAlt < 0) { terrainAlt = 0; }
                trueAlt = ASL - terrainAlt;
                atmPressure = FlightGlobals.getStaticPressure(ASL, this.vessel.mainBody);
                if (atmPressure < 0.000001) { atmPressure = 0; }
                atmDensity = FlightGlobals.getAtmDensity(atmPressure);
                up = FlightGlobals.getUpAxis(pos);
                sqrSpeed = (this.part.Rigidbody.velocity + Krakensbane.GetFrameVelocityV3f()).sqrMagnitude;
                dragVector = -(this.part.Rigidbody.velocity + Krakensbane.GetFrameVelocityV3f()).normalized;
                forcePosition = this.parachute.transform.position;
                if (secondaryChute) { secForcePosition = this.secParachute.transform.position; }
            }
        }

        public override void OnFixedUpdate()
        {
            //To prevent weird shit
            if (!HighLogic.LoadedSceneIsFlight) { return; }
            
            //Deployment clauses and actions. Go figure how they work, I just know they do :P
            //Checks if the parachute must disarm
            if (armed)
            {
                this.part.stackIcon.SetIconColor(XKCDColors.LightCyan);
                if (CanDeployChute("MainChute") || (secondaryChute && CanDeployChute("SecChute"))) { armed = false; }
            }

            else
            {
                //Main Chute
                if (CanDeployChute("MainChute"))
                {
                    oneWasDeployed = true;
                    if (!wait)
                    {
                        //When the chute is stowed
                        if (deploymentState == "STOWED")
                        {
                            this.part.stackIcon.SetIconColor(XKCDColors.LightCyan);
                            if (trueAlt > deploymentAlt && MinDeployment(minDeployment, minPressure, minIsPressure)) { if (RandomDeployment("MainChute")) { PreDeploy("MainChute"); } }

                            else if (trueAlt <= deploymentAlt) { if (RandomDeployment("MainChute")) { LowDeploy("MainChute"); } }
                        }

                        //When the chute is predeployed
                        else if (deploymentState == "PREDEPLOYED")
                        {
                            GetDragDirection(parachute, forcedOrientation);
                            ParachuteNoise(parachute);
                            this.part.rigidbody.AddForceAtPosition(DragForce(0, preDeployedArea, dragCoef, preDeploymentSpeed), forcePosition, ForceMode.Force);
                            if (trueAlt <= deploymentAlt)
                            {
                                Deploy("MainChute");
                                timeSet = false;
                            }
                        }

                        //When the chute was deployed below full deployment altitude
                        else if (deploymentState == "LOWDEPLOYED")
                        {
                            GetDragDirection(parachute, forcedOrientation);
                            ParachuteNoise(parachute);
                            this.part.rigidbody.AddForceAtPosition(DragForce(0, deployedArea, dragCoef, preDeploymentSpeed + deploymentSpeed), forcePosition, ForceMode.Force);
                            if (!CheckAnimationPlaying(preDeploymentAnimation) && !queued)
                            {
                                PlayAnimation(deploymentAnimation, 1 / deploymentSpeed);
                                queued = true;
                            }
                        }

                        //When the parachute is fully deployed
                        else if (deploymentState == "DEPLOYED")
                        {
                            GetDragDirection(parachute, forcedOrientation);
                            ParachuteNoise(parachute);
                            this.part.rigidbody.AddForceAtPosition(DragForce(preDeployedArea, deployedArea, dragCoef, deploymentSpeed), forcePosition, ForceMode.Force);
                            if (!fullPlayed && !CheckAnimationPlaying(preDeploymentAnimation))
                            {
                                PlayAnimation(deploymentAnimation, 1 / deploymentSpeed);
                                fullPlayed = true;
                            }
                        }
                    }

                    //If deployment is on hold
                    else { CheckForWait(); }
                }

                //Deactivation
                else if (!CanDeployChute("MainChute") && IsDeployed(deploymentState)) { Cut("MainChute"); }

                //Secondary chute
                if (secondaryChute && CanDeployChute("SecChute"))
                {
                    oneWasDeployed = true;
                    if (!wait)
                    {
                        //When the chute is stowed
                        if (secDeploymentState == "STOWED")
                        {
                            this.part.stackIcon.SetIconColor(XKCDColors.LightCyan);
                            if (trueAlt > secDeploymentAlt && MinDeployment(secMinDeployment, secMinPressure, secMinIsPressure)) { if (RandomDeployment("SecChute")) { PreDeploy("SecChute"); } }

                            else if (trueAlt <= secDeploymentAlt) { if (RandomDeployment("SecChute")) { LowDeploy("SecChute"); } }
                        }

                        //When the chute is predeployed
                        else if (secDeploymentState == "PREDEPLOYED")
                        {
                            GetDragDirection(secParachute, secForcedOrientation);
                            ParachuteNoise(secParachute);
                            this.part.rigidbody.AddForceAtPosition(DragForce(0, secPreDeployedArea, secDragCoef, secPreDeploymentSpeed), secForcePosition, ForceMode.Force);
                            if (trueAlt <= secDeploymentAlt)
                            {
                                Deploy("SecChute");
                                timeSet = false;
                            }
                        }

                        //If the chute was deployed bellow full deployment altitude
                        else if (secDeploymentState == "LOWDEPLOYED")
                        {
                            GetDragDirection(secParachute, secForcedOrientation);
                            ParachuteNoise(secParachute);
                            this.part.rigidbody.AddForceAtPosition(DragForce(0, secDeployedArea, secDragCoef, secPreDeploymentSpeed + secDeploymentSpeed), secForcePosition, ForceMode.Force);
                            if (!CheckAnimationPlaying(secPreDeploymentAnimation) && !secQueued)
                            {
                                PlayAnimation(secDeploymentAnimation, 1 / secDeploymentSpeed);
                                secQueued = true;
                            }
                        }

                        //When the parachute is fully deployed
                        else if (secDeploymentState == "DEPLOYED")
                        {
                            GetDragDirection(secParachute, secForcedOrientation);
                            ParachuteNoise(secParachute);
                            this.part.rigidbody.AddForceAtPosition(DragForce(secPreDeployedArea, secDeployedArea, secDragCoef, secDeploymentSpeed), secForcePosition, ForceMode.Force);
                            if (!secFullPlayed && !CheckAnimationPlaying(secPreDeploymentAnimation))
                            {
                                PlayAnimation(secDeploymentAnimation, 1 / secDeploymentSpeed);
                                secFullPlayed = true;
                            }
                        }
                    }

                    //If deployment is on hold
                    else { CheckForWait(); }
                }

                //Deactivation
                else if (!CanDeployChute("SecChute") && IsDeployed(secDeploymentState)) { Cut("SecChute"); }

                //If both parachutes must be cut
                if (BothMustStop())
                {
                    if (IsDeployed(deploymentState)) { Cut("MainChute"); }

                    if (IsDeployed(secDeploymentState)) { Cut("SecChute"); }

                    SetRepack();
                }

                //Alows both parachutes to be cut at the same time if both are dpeloyed
                if (secondaryChute && IsDeployed(deploymentState) && IsDeployed(secDeploymentState)) { Events["GUICutBoth"].active = true; }

                else { Events["GUICutBoth"].active = false; }

                //If the parachute can't be deployed
                if (!oneWasDeployed)
                {
                    StagingReset();
                    Events["GUIDeploy"].active = true;
                    Events["GUIArm"].active = true;
                }
            }
        }

        #endregion
    }
}
