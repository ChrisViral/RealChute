using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


/* RealChute was made by stupid_chris and is licensed under CC-BY-NC-SA. You can remix, modify and redistribute
 * the work, but you must give attribution to the original author and you cannot sell your derivatives.
 * For more informtion contact me on the forum.*/


namespace stupid_chris
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
        public float timer = 0;
        [KSPField]
        public bool mustGoDown = false;
        [KSPField]
        public float spareChutes = 5;
        [KSPField]
        public bool secondaryChute = false;

        //Main parachute
        [KSPField]
        public float preDeployedDiameter = 1;
        [KSPField]
        public float deployedDiameter = 25;
        [KSPField]
        public bool minIsPressure = false;
        [KSPField]
        public float minDeployment = 40000;
        [KSPField]
        public float deploymentAlt = 700;
        [KSPField]
        public float cutSpeed = 0.5f;
        [KSPField]
        public float cutAlt = -1;
        [KSPField]
        public float preDeploymentSpeed = 2;
        [KSPField]
        public float deploymentSpeed = 6;
        [KSPField]
        public string preDeploymentAnimation = "semiDeploy";
        [KSPField]
        public string deploymentAnimation = "fullyDeploy";
        [KSPField]
        public string parachuteName = "parachute";
        [KSPField]
        public string capName = "cap";

        //Second parachute
        [KSPField]
        public float secPreDeployedDiameter;
        [KSPField]
        public float secDeployedDiameter;
        [KSPField]
        public bool secMinIsPressure;
        [KSPField]
        public float secMinDeployment;
        [KSPField]
        public float secDeploymentAlt;
        [KSPField]
        public float secCutAlt;
        [KSPField]
        public float secPreDeploymentSpeed;
        [KSPField]
        public float secDeploymentSpeed;
        [KSPField]
        public string secPreDeploymentAnimation;
        [KSPField]
        public string secDeploymentAnimation;
        [KSPField]
        public string secParachuteName;
        [KSPField]
        public string secCapName;

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
        public Vector3 dragVector, up;
        public Vector3d pos;
        public Transform parachute, secParachute;
        public Transform cap, secCap;
        public RaycastHit craft;
        public FXGroup deployFX, secDeployFX;
        public FXGroup preDeployFX, secPreDeployFX;
        public FXGroup cutFX, repackFX;
        public string deploymentState = "STOWED", secDeploymentState = "STOWED";
        public string infoList = String.Empty;
        public float preDeployedArea, secPreDeployedArea;
        public float deployedArea, secDeployedArea;
        public float parachuteDensity, dragCoef, chuteArea;
        public float chuteMass, secChuteMass;
        public float currentForce, ASL, srfSpeed;
        public float currentTime, debutTime, deltaTime, deploymentTime;
        public float atmPressure, atmDensity;
        public bool queued = false, secQueued = false;
        public bool wait = false, armed = false, oneWasDeployed = false;
        public bool timerSet = false, timeSet = false, setCount = false;

        #endregion

        #region Animations
        //------------------------- Animations -------------------------
        public void InitiateAnimation(string animationName)
        {
            //Initiates the default values for animations
            foreach (var animation in part.FindModelAnimators(animationName))
            {
                var animationState = animation[animationName];
                animationState.normalizedTime = 0;
                animationState.normalizedSpeed = 0;
                animationState.enabled = false;
                animationState.wrapMode = WrapMode.Clamp;
                animationState.layer = 1;
            }
        }

        public void PlayAnimation(string animationName, float animationTime, float animationSpeed)
        {
            //Plays the animation
            foreach (var animation in part.FindModelAnimators(animationName))
            {
                var animationState = animation[animationName];
                animationState.normalizedTime = animationTime;
                animationState.normalizedSpeed = animationSpeed;
                animationState.enabled = true;
                animation.Play(animationName);
            }
        }

        public bool CheckAnimationPlaying(string animationName)
        {
            //Checks if a given animation is playing
            var isPlaying = false;
            foreach (var animation in part.FindModelAnimators(animationName))
            {
                isPlaying = animation.IsPlaying(animationName);
            }
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

        public float GetTrueAlt()
        {
            //Gets the altitude from the ground or water
            if (Physics.Raycast(pos, -up, out craft, ASL + 10000, 1 << 15)) { return Mathf.Min(craft.distance, ASL); }

            else { return ASL; }
        }

        public bool MinDeployment(float minDeploy)
        {
            //Returns the right value to check
            if (minIsPressure)
            {
                if (atmDensity >= minDeploy) { return true; }

                else { return false; }
            }

            else
            {
                if (GetTrueAlt() <= minDeploy) { return true; }

                else { return false; }
            }
        }

        public void CheckForWait()
        {
            //Checks if there is a timer and/or a mustGoDown clause active
            var timerSpent = true;
            var goesDown = true;

            //Timer
            if (timer <= 0) { timerSpent = true; }

            else
            {
                if (!timerSet)
                {
                    debutTime = currentTime;
                    timerSet = true;
                }

                deltaTime = currentTime - debutTime;
                if (deltaTime < timer)
                {
                    timerSpent = false;
                    ScreenMessages.PostScreenMessage(String.Format("Deployment in {0}s", (timer - deltaTime).ToString("#0.0")), Time.fixedDeltaTime, ScreenMessageStyle.UPPER_CENTER);
                }
            }

            //Goes down
            if (!mustGoDown) { goesDown = true; }

            else
            {
                if (this.vessel.verticalSpeed >= 0)
                {
                    goesDown = false;
                    ScreenMessages.PostScreenMessage("Deployment awaiting negative vertical velocity", Time.fixedDeltaTime, ScreenMessageStyle.UPPER_CENTER);
                    ScreenMessages.PostScreenMessage(String.Format("Current vertical velocity: {0}m/s", this.vessel.verticalSpeed.ToString("#0.0")), Time.fixedDeltaTime, ScreenMessageStyle.UPPER_CENTER);
                }
            }

            if (timerSpent && goesDown) { wait = false; }

            else
            {
                this.part.stackIcon.SetIconColor(XKCDColors.LightCyan);
                wait = true;
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

                else if (MinDeployment(minDeployment) && cutAlt == -1) { return true; }

                else if (MinDeployment(minDeployment) && GetTrueAlt() > cutAlt) { return true; }

                else { return false; }
            }

            //Checks if the secondary chute can deploy
            else if (secondaryChute && whichChute == "SecChute")
            {
                if (secDeploymentState == "CUT") { return false; }

                else if (MinDeployment(secMinDeployment)  && secCutAlt == -1) { return true; }

                else if (MinDeployment(secMinDeployment)  && GetTrueAlt() > secCutAlt) { return true; }

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
                if (!armed) {return true;}

                else if (deploymentState == "CUT" || secDeploymentState == "CUT") { return true; }

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

        public void ParachuteNoise(Transform chute)
        {
            //Gives a random noise to the parachute orientation
            Vector3 rotationAngle = new Vector3(5 * (Mathf.PerlinNoise(currentTime, 0) - 0.5f), 5 * (Mathf.PerlinNoise(currentTime, 20) - 0.5f), 0);
            chute.Rotate(rotationAngle);
        }

        public Quaternion GetDragDirection(Vector3 parachuteUp)
        {
            //Makes the parachute follow air flow
            var parachuteRotation = Quaternion.LookRotation(dragVector, parachuteUp);
            return parachuteRotation;
        }

        #endregion

        #region Deployment code
        //------------------------- Deployment code -------------------------
        public void LowDeploy(string whichChute)
        {
            //Parachute low deployment code
            this.part.stackIcon.SetIconColor(XKCDColors.RadioactiveGreen);
            capOff = true;
            Events["GUIDeploy"].active = false;
            Events["GUIArm"].active = false;
            if (whichChute == "MainChute")
            {
                deploymentState = "LOWDEPLOYED";                
                parachute.gameObject.SetActive(true);
                cap.gameObject.SetActive(false);
                deployFX.Burst();
                Events["GUICut"].active = true;
                PlayAnimation(preDeploymentAnimation, 0, 1 / preDeploymentSpeed);
            }

            else if (whichChute == "SecChute")
            {
                secDeploymentState = "LOWDEPLOYED";
                secParachute.gameObject.SetActive(true);
                secCap.gameObject.SetActive(false);
                secDeployFX.Burst();
                Events["GUISecCut"].active = true;
                PlayAnimation(secPreDeploymentAnimation, 0, 1 / secPreDeploymentSpeed);
            }
        }

        public void PreDeploy(string whichChute)
        {
            //Parachute predeployment code
            this.part.stackIcon.SetIconColor(XKCDColors.BrightYellow);
            capOff = true;
            Events["GUIDeploy"].active = false;
            Events["GUIArm"].active = false;
            if (whichChute == "MainChute")
            {
                deploymentState = "PREDEPLOYED";
                parachute.gameObject.SetActive(true);
                cap.gameObject.SetActive(false);
                preDeployFX.Burst();
                Events["GUICut"].active = true;
                PlayAnimation(preDeploymentAnimation, 0, 1 / preDeploymentSpeed);
            }

            else if (whichChute == "SecChute")
            {
                secDeploymentState = "PREDEPLOYED";
                secParachute.gameObject.SetActive(true);
                secCap.gameObject.SetActive(false);
                secPreDeployFX.Burst();
                Events["GUISecCut"].active = true;
                PlayAnimation(secPreDeploymentAnimation, 0, 1 / secPreDeploymentSpeed);
            }
        }

        public void Deploy(string whichChute)
        {
            //Parachute full deployment code
            this.part.stackIcon.SetIconColor(XKCDColors.RadioactiveGreen);
            if (whichChute == "MainChute")
            {
                deploymentState = "DEPLOYED";
                deployFX.Burst();
                PlayAnimation(deploymentAnimation, 0, 1 / deploymentSpeed);
            }

            else if (whichChute == "SecChute")
            {
                secDeploymentState = "DEPLOYED";
                secDeployFX.Burst();
                PlayAnimation(secDeploymentAnimation, 0, 1 / secDeploymentSpeed);
            }
        }

        public void StagingReset()
        {
            //Deactivates the part
            this.part.deactivate();
            armed = false;
            this.part.inverseStage = Staging.CurrentStage;
        }

        public void Cut(string whichChute)
        {
            //Cuts the chute
            cutFX.Burst();
            if (whichChute == "MainChute")
            {
                deploymentState = "CUT";
                parachute.gameObject.SetActive(false);
                Events["GUICut"].active = false;
                queued = false;
                if (!secondaryChute || secDeploymentState == "CUT") { SetRepack(); }

                if (secondaryChute && secDeploymentState != "STOWED") { armed = true; }
            }

            else if (whichChute == "SecChute")
            {
                secDeploymentState = "CUT";
                secParachute.gameObject.SetActive(false);
                Events["GUISecCut"].active = false;
                secQueued = false;
                if (deploymentState == "CUT") { SetRepack(); }

                if (deploymentState == "STOWED") { armed = true; }
            }
        }

        public void SetRepack()
        {
            //Allows the chute to be repacked if available
            this.part.stackIcon.SetIconColor(XKCDColors.Red);
            timeSet = false;
            timerSet = false;
            wait = false;
            StagingReset();
            if (chuteCount > 0 || chuteCount == -1)
            {
                Events["GUIRepack"].guiActiveUnfocused = true;
            }
        }

        public void Repack()
        {
            //Repacks a cut chute
            if (chuteCount > 0 || chuteCount == -1)
            {
                Events["GUIRepack"].guiActiveUnfocused = false;
                Events["GUIDeploy"].active = true;
                Events["GUIArm"].active = true;
                repackFX.Burst();
                oneWasDeployed = false;
                if (chuteCount != -1) { chuteCount--; }
                deploymentState = "STOWED";
                cap.gameObject.SetActive(true);
                this.part.stackIcon.SetIconColor(XKCDColors.White);
                capOff = false;
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
        public float GetArea(float diameter)
        {
            return Mathf.Pow(diameter, 2) * Mathf.PI / 4;
        }

        public float DragDeployment(float time, float debutArea, float endArea)
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

            else
            {
                return endArea;
            }
        }

        public float DragCalculation(float area, float Cd)
        {
            var velocity2 = Mathf.Pow(srfSpeed, 2);
            return atmDensity * velocity2 * Cd * area / 2000;
        }

        public Vector3 DragForce(float startArea, float targetArea, float Cd, float speed)
        {
            //Calculates a part's drag
            var chuteArea = DragDeployment(speed, startArea, targetArea);
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
            if (spareChutes < 0) { Fields["chuteCount"].guiActive = false; }

            //Part GUI
            Events["GUIDeploy"].active = true;
            Events["GUICut"].active = false;
            Events["GUISecCut"].active = false;
            Events["GUIArm"].active = true;
            Events["GUIRepack"].guiActiveUnfocused = false;
            if (!secondaryChute)
            {
                Actions["ActionSecCut"].active = false;
                Actions["ActionCut"].guiName = "Cut chute";
                Events["GUICut"].guiName = "Cut chute";
            }

            //Calculates parachute area
            preDeployedArea = GetArea(preDeployedDiameter);
            deployedArea = GetArea(deployedDiameter);
            if (secondaryChute)
            {
                secPreDeployedArea = GetArea(secPreDeployedDiameter);
                secDeployedArea = GetArea(secDeployedDiameter);
            }

            //Initiates parachute mass
            this.part.mass = caseMass;
            this.part.mass += chuteMass;
            if (secondaryChute)
            {
                this.part.mass +=  secChuteMass;
            }

            //FX groups
            this.deployFX = this.part.findFxGroup("rcdeploy");
            this.preDeployFX = this.part.findFxGroup("rcpredeploy");
            this.secDeployFX = this.part.findFxGroup("rcsecdeploy");
            this.secPreDeployFX = this.part.findFxGroup("rcsecpredeploy");
            this.cutFX = this.part.findFxGroup("rccut");
            this.repackFX = this.part.findFxGroup("rcrepack");

            //Initiates animation
            this.cap = this.part.FindModelTransform(capName);
            this.parachute = this.part.FindModelTransform(parachuteName);
            parachute.gameObject.SetActive(false);
            InitiateAnimation(preDeploymentAnimation);
            InitiateAnimation(deploymentAnimation);

            //Initiates the second parachute if any
            if (secondaryChute)
            {
                this.secCap = this.part.FindModelTransform(secCapName);
                this.secParachute = this.part.FindModelTransform(secParachuteName);
                secParachute.gameObject.SetActive(false);;
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
        }

        public override void OnLoad(ConfigNode node)
        {
            //Creates the FXgroups
            this.preDeployFX = new FXGroup("rcpredeploy");
            this.deployFX = new FXGroup("rcdeploy");
            this.secDeployFX = new FXGroup("rcsecdeploy");
            this.secPreDeployFX = new FXGroup("rcsecpredeploy");
            this.cutFX = new FXGroup("rccut");
            this.repackFX = new FXGroup("rcrepack");

            //Adds the FX groups to the part's list
            this.part.fxGroups.Add(preDeployFX);
            this.part.fxGroups.Add(deployFX);
            this.part.fxGroups.Add(secDeployFX);
            this.part.fxGroups.Add(secPreDeployFX);
            this.part.fxGroups.Add(cutFX);
            this.part.fxGroups.Add(repackFX);

            //Gets values from the resource definition file
            foreach (ConfigNode cfg in GameDatabase.Instance.GetConfigNodes("MATERIAL"))
            {
                if (cfg.HasValue("name") && cfg.GetValue("name") == material)
                {          
                    //Gets the drag coefficient
                    if(cfg.HasValue("dragCoefficient"))
                    {
                        float Cd;
                        if (float.TryParse(cfg.GetValue("dragCoefficient"), out Cd))
                        {
                            dragCoef = Cd;
                        }
                    }

                    //Gets the parachute density
                    if(cfg.HasValue("areaDensity"))
                    {
                        float aD;
                        if (float.TryParse(cfg.GetValue("areaDensity"), out aD))
                        {
                            parachuteDensity = aD;
                        }
                    }
                }
            }

            chuteMass = GetArea(deployedDiameter) * parachuteDensity;
            if (secondaryChute)
            {
                secChuteMass = GetArea(secDeployedDiameter) * parachuteDensity;
            }
        }

        public override string GetInfo()
        { 
            //Info in the editor part window

            infoList = "RealChute module:\n";
            infoList += String.Format("Parachute material: {0}\n", material);
            infoList += String.Format("Drag coefficient: {0}\n", dragCoef);
            if (timer > 0) { infoList += String.Format("Deployment timer: {0}s\n", timer); }

            if (mustGoDown) { infoList += "Must go downwards to deploy: true\n"; }

            if (spareChutes >= 0) { infoList += String.Format("Spare chutes: {0}\n", spareChutes); }

            if (!secondaryChute)
            {
                infoList += String.Format("Total parachute mass: {0}t\n", (caseMass + chuteMass).ToString("#0.0000"));
                infoList += String.Format("Predeployed diameter: {0}m\n", preDeployedDiameter);
                infoList += String.Format("Deployed diameter: {0}m\n", deployedDiameter);
                if (!minIsPressure) { infoList += String.Format("Minimum deployment altitude: {0}m\n", minDeployment); }

                else { infoList += String.Format("Minimum deployment pressure: {0}atm\n", minDeployment); }
              
                infoList += String.Format("Deployment altitude: {0}m\n", deploymentAlt);
                infoList += String.Format("Predeployment speed: {0}s\n", preDeploymentSpeed);
                infoList += String.Format("Deployment speed: {0}s\n", deploymentSpeed);
                infoList += String.Format("Autocut speed: {0}m/s\n", cutSpeed);
                if (cutAlt != -1) { infoList += String.Format("Autocut altitude: {0}m\n", cutAlt); }

            }

            //In case of dual chutes
            else
            {
                infoList += String.Format("Predeployed diameters: {0}m, {1}m\n", preDeployedDiameter, secPreDeployedDiameter);
                infoList += String.Format("Deployed diameters: {0}m, {1}m\n", deployedDiameter, secDeployedDiameter);
                infoList += String.Format("Total parachute mass: {0}t\n", (caseMass + chuteMass + secChuteMass).ToString("#0.0000"));
                if (!minIsPressure && !secMinIsPressure) { infoList += String.Format("Minimum deployment altitudes: {0}m, {1}m\n", minDeployment, secMinDeployment); }

                else if (minIsPressure && !secMinIsPressure) { infoList += String.Format("Minimum deployment clauses: {0}atm, {1}m\n", minDeployment, secMinDeployment); }

                else if (!minIsPressure && secMinIsPressure) { infoList += String.Format("Minimum deployment clauses: {0}m, {1}atm\n", minDeployment, secMinDeployment); }

                else if (minIsPressure && secMinIsPressure) { infoList += String.Format("Minimum deployment pressures: {0}atmm, {1}atm\n", minDeployment, secMinDeployment); }

                    infoList += String.Format("Deployment altitudes: {0}m, {1}m\n", deploymentAlt, secDeploymentAlt);
                infoList += String.Format("Predeployment speeds: {0}s, {1}s\n", preDeploymentSpeed, secPreDeploymentSpeed);
                infoList += String.Format("Deployment speeds: {0}s, {1}s\n", deploymentSpeed, secDeploymentSpeed);
                infoList += String.Format("Autocut speed: {0}m/s\n", cutSpeed);
                if (cutAlt != -1 && secCutAlt != -1) { infoList += String.Format("Autocut altitudes: {0}m, {1}m\n", cutAlt, secCutAlt); }

                else if (cutAlt != -1 && secCutAlt == -1) { infoList += String.Format("Main chute autocut altutude: {0}m\n", cutAlt); }

                else if (cutAlt == -1 && secCutAlt != -1) { infoList += String.Format("Secondary chute autocut altitude: {0}m\n", secCutAlt); }
            }
            return infoList;
        }

        public void Update()
        {
            //Preventing weird shit
            if (!HighLogic.LoadedSceneIsFlight) { return; }

            //Universal values
            currentTime = Time.time;
            pos = this.part.transform.position;
            ASL = (float)FlightGlobals.getAltitudeAtPos(pos);
            atmPressure = (float)FlightGlobals.getStaticPressure(ASL, this.vessel.mainBody);
            if (atmPressure < 0.000001) { atmPressure = 0; }
            atmDensity = (float)FlightGlobals.getAtmDensity(atmPressure);
            up = FlightGlobals.getUpAxis(pos);
            srfSpeed = (float)this.vessel.srf_velocity.magnitude;
            dragVector = -this.vessel.srf_velocity.normalized;
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
                if (CanDeployChute("MainChute") || CanDeployChute("SecChute")) { armed = false; }
            }

            //Main Chute
            if (!armed && CanDeployChute("MainChute"))
            {
                oneWasDeployed = true;
                if (!wait)
                {
                    //When the chute is stowed
                    if (deploymentState == "STOWED")
                    {
                        Events["GUIDeploy"].active = false;
                        if (GetTrueAlt() > deploymentAlt) { PreDeploy("MainChute"); }

                        else if (GetTrueAlt() <= deploymentAlt) { LowDeploy("MainChute"); }
                    }

                    //When the chute is predeployed
                    else if (deploymentState == "PREDEPLOYED")
                    {
                        parachute.rotation = GetDragDirection(parachute.transform.up);
                        ParachuteNoise(parachute);
                        this.part.Rigidbody.AddForce(DragForce(0, 0.8f, 1, 3), ForceMode.Force);
                        if (GetTrueAlt() <= deploymentAlt)
                        {
                            Deploy("MainChute");
                            timeSet = false;
                        }
                    }

                    //When the chute was deployed below full deployment altitude
                    else if (deploymentState == "LOWDEPLOYED")
                    {
                        parachute.rotation = GetDragDirection(parachute.transform.up);
                        ParachuteNoise(parachute);
                        this.part.Rigidbody.AddForce(DragForce(0, deployedArea, dragCoef, preDeploymentSpeed + deploymentSpeed), ForceMode.Force);
                        if (!CheckAnimationPlaying(preDeploymentAnimation) && !queued)
                        {
                            PlayAnimation(deploymentAnimation, 0, 1 / deploymentSpeed);
                            queued = true;
                        }
                    }

                    //When the parachute is fully deployed
                    else if (deploymentState == "DEPLOYED")
                    {
                        parachute.rotation = GetDragDirection(parachute.transform.up);
                        ParachuteNoise(parachute);
                        this.part.Rigidbody.AddForce(DragForce(preDeployedArea, deployedArea, dragCoef, deploymentSpeed), ForceMode.Force);
                    }
                }

                else { CheckForWait(); }
            }

            else if (!armed && !CanDeployChute("MainChute") && IsDeployed(deploymentState))
            {
                //Deactivation
                Cut("MainChute");
            }

            //Secondary chute
            if (!armed && secondaryChute && CanDeployChute("SecChute"))
            {
                oneWasDeployed = true;
                if (!wait)
                {
                    //When the chute is stowed
                    if (secDeploymentState == "STOWED")
                    {
                        if (GetTrueAlt() > secDeploymentAlt) { PreDeploy("SecChute"); }

                        else if (GetTrueAlt() <= secDeploymentAlt) { LowDeploy("SecChute"); }
                    }

                    //When the chute is predeployed
                    else if (secDeploymentState == "PREDEPLOYED")
                    {

                        secParachute.rotation = GetDragDirection(secParachute.transform.up);
                        ParachuteNoise(secParachute);
                        this.part.Rigidbody.AddForce(DragForce(0, secPreDeployedArea, dragCoef, secPreDeploymentSpeed), ForceMode.Force);
                        if (GetTrueAlt() <= secDeploymentAlt)
                        {
                            Deploy("SecChute");
                            timeSet = false;
                        }
                    }

                    //If the chute was deployed bellow full deployment altitude
                    else if (secDeploymentState == "LOWDEPLOYED")
                    {
                        secParachute.rotation = GetDragDirection(secParachute.transform.up);
                        ParachuteNoise(secParachute);
                        this.part.Rigidbody.AddForce(DragForce(0, secDeployedArea, dragCoef, secPreDeploymentSpeed + secDeploymentSpeed), ForceMode.Force);
                        if (!CheckAnimationPlaying(secPreDeploymentAnimation) && !secQueued)
                        {
                            PlayAnimation(secDeploymentAnimation, 0, 1 / secDeploymentSpeed);
                            secQueued = true;
                        }
                    }

                    //When the parachute is fully deployed
                    else if (secDeploymentState == "DEPLOYED")
                    {
                        secParachute.rotation = GetDragDirection(secParachute.transform.up);
                        ParachuteNoise(secParachute);
                        this.part.Rigidbody.AddForce(DragForce(secPreDeployedArea, secDeployedArea, dragCoef, secDeploymentSpeed), ForceMode.Force);
                    }
                }

                else { CheckForWait(); }
            }

            else if (!armed && !CanDeployChute("SecChute") && IsDeployed(secDeploymentState))
            {
                Cut("SecChute");
            }

            //If both parachutes must be cut
            if (BothMustStop())
            {
                if (IsDeployed(deploymentState)) { Cut("MainChute"); }

                if (IsDeployed(secDeploymentState)) { Cut("SecChute"); }

                SetRepack();
            }

            //If the parachute can't be deployed
            if (!oneWasDeployed && !armed) 
            {
                StagingReset();
                Events["GUIDeploy"].active = true;
                Events["GUIArm"].active = true;
            }
        }

        #endregion
    }
}
