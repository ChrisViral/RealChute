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
        public float preDeployedDrag;
        [KSPField]
        public float deployedDrag;
        [KSPField]
        public float stowedDrag;
        [KSPField]
        public float minDeploymentAlt;
        [KSPField]
        public float deploymentAlt;
        [KSPField]
        public float cutAlt;
        [KSPField]
        public float timer;
        [KSPField]
        public bool mustGoDown;
        [KSPField]
        public float preDeploymentSpeed;
        [KSPField]
        public float deploymentSpeed;
        [KSPField]
        public string preDeploymentAnimation;
        [KSPField]
        public string deploymentAnimation;
        [KSPField]
        public string parachuteName;
        [KSPField]
        public string capName;
        [KSPField]
        public float spareChutes;
        [KSPField]
        public float cutSpeed;

        //Second parachute
        [KSPField]
        public bool secondaryChute = false;
        [KSPField]
        public float secPreDeployedDrag;
        [KSPField]
        public float secDeployedDrag;
        [KSPField]
        public float secMinDeploymentAlt;
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
        public float chuteCount;

        #endregion

        #region Variables
        //Variables
        public bool queued = false;
        public bool secQueued = false;
        public bool isPlaying;
        public Vector3 dragVector;
        public Vector3 up;
        public Vector3 CoM;
        public Quaternion parachuteRotation;
        public Transform parachute;
        public Transform cap;
        public Transform secParachute;
        public Transform secCap;
        public float altitude;
        public float ASL;
        public float terrainHeight;
        public float altitudeToUse;
        public RaycastHit craft;
        public string deploymentState;
        public string secDeploymentState;
        public bool setCount = false;
        public float debutTime;
        public float currentTime;
        public float deltaTime;
        public bool timeSet = false;
        public bool dragSet = false;
        public bool secDragSet = false;
        public float debutDrag;
        public float secDebutDrag;
        public bool timerSet = false;
        public float drag;
        public float deploymentTime;
        public string infoList = String.Empty;
        public FXGroup deployFX;
        public FXGroup preDeployFX;
        public FXGroup secDeployFX;
        public FXGroup secPreDeployFX;
        public bool goesDown = false;
        public bool timerSpent = false;
        public bool wait = true;
        public bool oneWasDeployed = false;
        public float chuteDrag = 0;
        public float secChuteDrag = 0;

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
            isPlaying = false;
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
            if (CanDeployChute("MainChute") || CanDeployChute("SecChute"))
            {
                this.part.force_activate();
            }
        }

        [KSPEvent(guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Cut main chute")]
        public void GUICut()
        {
            //Cuts main chute chute
            if (IsDeployed(deploymentState))
            {
                Cut("MainChute");
            }
        }

        [KSPEvent(guiActive = true, active = true, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Cut secondary chute")]
        public void GUISecCut()
        {
            //Cuts secondary chute
            if (IsDeployed(secDeploymentState))
            {
                Cut("SecChute");
            }
        }

        [KSPEvent(guiActive = false, externalToEVAOnly = true, guiActiveUnfocused = true, guiName = "Repack chute", unfocusedRange = 5)]
        public void GUIRepack()
        {
            //Repacks chute from EVA if in space or on the ground
            if (CheckGroundStop() || this.vessel.atmDensity == 0)
            {
                if (!secondaryChute)
                {
                    if (deploymentState == "CUT")
                    {
                        Repack();
                    }
                }

                else
                {
                    if (deploymentState == "CUT" && secDeploymentState == "CUT")
                    {
                        Repack();
                    }
                }
            }
        }

        #endregion

        #region Action groups
        //------------------------- Action groups -------------------------
        [KSPAction("Deploy chute")]
        public void ActionDeploy(KSPActionParam param)
        {
            //Forces the parachute to deploy
            if (CanDeployChute("MainChute") || CanDeployChute("SecChute"))
            {
                this.part.force_activate();
            }
        }

        [KSPAction("Cut main chute")]
        public void ActionCut(KSPActionParam param)
        {
            //Cuts main chute
            if (IsDeployed(deploymentState))
            {
                Cut("MainChute");
            }
        }

        [KSPAction("Cut secondary chute")]
        public void ActionSecCut(KSPActionParam param)
        {
            //Cuts secondary chute
            if (IsDeployed(secDeploymentState))
            {
                Cut("SecChute");
            }
        }

        #endregion

        #region Methods
        //------------------------- Methods -------------------------
        public bool CheckGroundStop()
        {
            //Checks if the vessel is on the ground and has stopped moving
            if (this.vessel.LandedOrSplashed && this.vessel.horizontalSrfSpeed < cutSpeed)
            {
                return true;
            }

            else
            {
                return false;
            }
        }

        public float GetTrueAlt()
        {
            //Gets the altitude from the ground or water
            ASL = (float)this.vessel.mainBody.GetAltitude(CoM);
            if (Physics.Raycast( CoM, -up, out craft, ASL + 10000, 1 << 15))
            {
                altitude = Mathf.Min(craft.distance, ASL);
            }

            else
            {
                altitude = ASL;
            }

            return altitude;
        }

        public float AltToUse(float minDeployment)
        {
            //Prevents some weirdness with minimal deployment altitudes
            terrainHeight = ASL - GetTrueAlt();
            if (terrainHeight > minDeployment)
            {
                altitudeToUse = GetTrueAlt();
            }

            else
            {
                altitudeToUse = ASL;
            }

            return altitudeToUse;
        }

        public void CheckForWait()
        {
            //Checks if there is a timer and/or a mustGoDown clause active
            this.part.stackIcon.SetIconColor(XKCDColors.LightCyan);
            if (!IsDeployed(deploymentState) && !IsDeployed(secDeploymentState))
            {
                if (timer <= 0)
                {
                    timerSpent = true;
                }

                else
                {
                    if (!timerSet)
                    {
                        debutTime = currentTime;
                        timerSet = true;
                    }

                    deltaTime = currentTime - debutTime;
                    if (deltaTime >= timer)
                    {
                        timerSpent = true;
                    }

                    else
                    {
                        timerSpent = false;
                        ScreenMessages.PostScreenMessage("Deployment in " + (timer - deltaTime).ToString("#0.0") + "s", Time.fixedDeltaTime, ScreenMessageStyle.UPPER_CENTER);
                    }
                }

                if (!mustGoDown)
                {
                    goesDown = true;
                }

                else
                {
                    if (this.vessel.verticalSpeed >= 0)
                    {
                        goesDown = false;
                        ScreenMessages.PostScreenMessage("Deployment awaiting negative vertical velocity", Time.fixedDeltaTime, ScreenMessageStyle.UPPER_CENTER);
                        ScreenMessages.PostScreenMessage("Current vertical velocity: " + this.vessel.verticalSpeed.ToString("#0.0") + "m/s", Time.fixedDeltaTime, ScreenMessageStyle.UPPER_CENTER);
                    }

                    else
                    {
                        goesDown = true;
                    }
                }

                if (timerSpent && goesDown)
                {
                    wait = false;
                }

                else
                {
                    wait = true;
                }
            }
        }

        public bool CanDeployChute(string whichChute)
        {
            //Automatically returns false if the craft is stopped on the ground
            if (CheckGroundStop())
            {
                return false;
            }

            //Checks if the parachute can deploy
            else if (!secondaryChute)
            {
                if (cutAlt == -1)
                {
                    if (this.vessel.atmDensity > 0 && !CheckGroundStop() && AltToUse(minDeploymentAlt) <= minDeploymentAlt)
                    {
                        return true;
                    }

                    else
                    {
                        return false;
                    }
                }

                else
                {
                    if (this.vessel.atmDensity > 0 && !CheckGroundStop() && AltToUse(minDeploymentAlt) <= minDeploymentAlt && GetTrueAlt() > cutAlt)
                    {
                        return true;
                    }

                    else
                    {
                        return false;
                    }
                }
            }

            //If more than one parachute
            else
            {
                if (whichChute == "MainChute")
                {
                    if (cutAlt == -1)
                    {
                        if (this.vessel.atmDensity > 0 && !CheckGroundStop() && AltToUse(minDeploymentAlt) <= minDeploymentAlt)
                        {
                            return true;
                        }

                        else if (this.vessel.atmDensity > 0 && !CheckGroundStop() && secCutAlt != -1 && secCutAlt > minDeploymentAlt)
                        {
                            return true;
                        }

                        else
                        {
                            return false;
                        }
                    }

                    else
                    {
                        if (this.vessel.atmDensity > 0 && !CheckGroundStop() && AltToUse(minDeploymentAlt) <= minDeploymentAlt && GetTrueAlt() > cutAlt)
                        {
                            return true;
                        }

                        else if (this.vessel.atmDensity > 0 && !CheckGroundStop() && secCutAlt != -1 && secCutAlt > minDeploymentAlt && GetTrueAlt() > cutAlt)
                        {
                            return true;
                        }

                        else
                        {
                            return false;
                        }
                    }
                }

                else
                {
                    if (secCutAlt == -1)
                    {
                        if (this.vessel.atmDensity > 0 && !CheckGroundStop() && AltToUse(secMinDeploymentAlt) <= secMinDeploymentAlt)
                        {
                            return true;
                        }

                        else if (this.vessel.atmDensity > 0 && !CheckGroundStop() && cutAlt != -1 && cutAlt > secMinDeploymentAlt)
                        {
                            return true;
                        }

                        else
                        {
                            return false;
                        }
                    }

                    else
                    {
                        if (this.vessel.atmDensity > 0 && !CheckGroundStop() && AltToUse(secMinDeploymentAlt) <= secMinDeploymentAlt && GetTrueAlt() > secCutAlt)
                        {
                            return true;
                        }

                        else if (this.vessel.atmDensity > 0 && !CheckGroundStop() && cutAlt != -1 && cutAlt > secMinDeploymentAlt && GetTrueAlt() > secCutAlt)
                        {
                            return true;
                        }

                        else
                        {
                            return false;
                        }
                    }
                }
            }
        }

        public bool IsDeployed(string deployState)
        {
            //Check if the chute is completely deployed
            if (deployState == "DEPLOYED" || deployState == "LOWDEPLOYED" || deployState == "PREDEPLOYED")
            {
                return true;
            }

            else
            {
                return false;
            }
        }

        public bool ChuteMustStop(string deployState, string whichChute)
        {
            //Checks if the chute comes into an even where it should stop
            if (!CanDeployChute(whichChute) && IsDeployed(deployState))
            {
                return true;
            }

            else
            {
                return false;
            }
        }

        public bool BothMustStop()
        {
            //When using dual chutes, checks if the parachute must consider both parachutes cut
            if(!CanDeployChute("MainChute") && !CanDeployChute("SecChute"))
            {
                return true;
            }

            else
            {
                return false;
            }
        }

        public void ParachuteNoise(Transform chute)
        {
            //Gives a random noise to the parachute orientation
            var rotationAngle = new Vector3(5 * (Mathf.PerlinNoise(currentTime, 0) - 0.5f), 5 * (Mathf.PerlinNoise(currentTime, 10) - 0.5f), 5 * (Mathf.PerlinNoise(currentTime, 20) - 0.5f));
            chute.Rotate(rotationAngle);
        }

        public Quaternion GetDragDirection()
        {
            //Returns the drag direction
            dragVector = -(Vector3)this.vessel.srf_velocity.normalized;
            parachuteRotation.SetLookRotation(dragVector, up);
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
            this.part.inverseStage = Staging.CurrentStage;
        }

        public void Cut(string whichChute)
        {
            //Cuts the chute
            if (whichChute == "MainChute")
            {
                deploymentState = "CUT";
                parachute.gameObject.SetActive(false);
                Events["GUICut"].active = false;
                queued = false;
                this.part.maximum_drag = this.part.maximum_drag - chuteDrag;
                if (this.part.maximum_drag == 0)
                {
                    this.part.maximum_drag = stowedDrag;
                }

                chuteDrag = 0;
            }

            else if (whichChute == "SecChute")
            {
                secDeploymentState = "CUT";
                secParachute.gameObject.SetActive(false);
                Events["GUISecCut"].active = false;
                secQueued = false;
                this.part.maximum_drag = this.part.maximum_drag - secChuteDrag;
                if (this.part.maximum_drag == 0)
                {
                    this.part.maximum_drag = stowedDrag;
                }

                secChuteDrag = 0;
            }
        }

        public void SetRepack()
        {
            //Allows the chute to be repacked if available
            if (chuteCount > 0 || chuteCount == float.NaN)
            {
                Events["GUIRepack"].guiActiveUnfocused = true;
            }

            this.part.stackIcon.SetIconColor(XKCDColors.Red);
            timerSpent = false;
            goesDown = false;
            timeSet = false;
            timerSet = false;
            oneWasDeployed = false;
            StagingReset();
        }

        public void SelectIconColour(string stateToCheck)
        {
            //When using dual chutes, checks the other parachute's state and assigns the correct icon colour
            if (stateToCheck == "STOWED")
            {
                this.part.stackIcon.SetIconColor(XKCDColors.LightCyan);
            }

            else if (stateToCheck == "PREDEPLOYED")
            {
                this.part.stackIcon.SetIconColor(XKCDColors.BrightYellow);
            }

            else if (stateToCheck == "DEPLOYED" || stateToCheck == "LOWDEPLOYED")
            {
                this.part.stackIcon.SetIconColor(XKCDColors.RadioactiveGreen);
            }
        }

        public void Repack()
        {
            //Repacks a cut chute
            if (chuteCount != float.NaN && chuteCount > 0)
            {
                if (!secondaryChute)
                {
                    deploymentState = "STOWED";
                    chuteCount--;
                    this.part.stackIcon.SetIconColor(XKCDColors.White);
                    cap.gameObject.SetActive(true);
                    Events["GUIRepack"].guiActiveUnfocused = false;
                    Events["GUIDeploy"].active = true;
                    capOff = false;
                }

                else
                {
                    deploymentState = "STOWED";
                    secDeploymentState = "STOWED";
                    chuteCount--;
                    this.part.stackIcon.SetIconColor(XKCDColors.White);
                    cap.gameObject.SetActive(true);
                    secCap.gameObject.SetActive(true);
                    Events["GUIRepack"].guiActiveUnfocused = false;
                    Events["GUIDeploy"].active = true;
                    capOff = false;
                }
            }

            else if (chuteCount == float.NaN)
            {
                if (!secondaryChute)
                {
                    deploymentState = "STOWED";
                    this.part.stackIcon.SetIconColor(XKCDColors.White);
                    cap.gameObject.SetActive(true);
                    Events["GUIRepack"].guiActiveUnfocused = false;
                    Events["GUIDeploy"].active = true;
                    capOff = false;
                }

                else
                {
                    deploymentState = "STOWED";
                    secDeploymentState = "STOWED";
                    this.part.stackIcon.SetIconColor(XKCDColors.White);
                    cap.gameObject.SetActive(true);
                    secCap.gameObject.SetActive(true);
                    Events["GUIRepack"].guiActiveUnfocused = false;
                    Events["GUIDeploy"].active = true;
                    capOff = false;
                }
            }
        }

        #endregion

        #region Drag code
        //------------------------- Drag code -------------------------
        public float DragDeployment(float time, float debutDrag, float endDrag)
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
                drag = Mathf.Lerp(debutDrag, endDrag, deploymentTime);
                return drag;
            }

            else
            {
                return endDrag;
            }
        }

        public void DragCalculation(float startDrag, float normalDrag, float targetDrag, float speed, float whichDrag)
        {
            //Calculates a part'S drag considering every present parachute
            if (startDrag == normalDrag)
            {
                if (this.part.maximum_drag != targetDrag)
                {
                     this.part.maximum_drag = DragDeployment(speed, normalDrag, targetDrag);
                }
                
            }
            else
            {
                if (this.part.maximum_drag != startDrag + targetDrag)
                {
                    this.part.maximum_drag = startDrag + DragDeployment(speed, 0, targetDrag);
                }
            }
        }

        public float GetChuteDrag(float startDrag, float normalDrag)
        {
            //Gets how much drag a specific parachute is adding to the part
            if (startDrag == normalDrag)
            {
                return this.part.maximum_drag;
            }

            else
            {
                return this.part.maximum_drag - startDrag;
            }
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
            this.part.maximum_drag = stowedDrag;
            Events["GUIDeploy"].active = true;
            Events["GUICut"].active = false;
            Events["GUISecCut"].active = false;
            Events["GUIRepack"].guiActiveUnfocused = false;
            if (spareChutes < 0)
            {
                Fields["chuteCount"].guiActive = false;
            }

            //FX groups
            this.deployFX = this.part.findFxGroup("rcdeploy");
            this.preDeployFX = this.part.findFxGroup("rcpredeploy");
            this.secDeployFX = this.part.findFxGroup("rcsecdeploy");
            this.secPreDeployFX = this.part.findFxGroup("rcsecpredeploy");

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
                cap.gameObject.SetActive(true);
                if (secondaryChute)
                {
                    secCap.gameObject.SetActive(true);
                }

                if (spareChutes >= 0)
                {
                    chuteCount = spareChutes;
                }

                else
                {
                    chuteCount = float.NaN;
                }
            }

            //If the part has been staged in the past
            if (capOff)
            {
                deploymentState = "CUT";
                cap.gameObject.SetActive(false);
                if (secondaryChute)
                {
                    secCap.gameObject.SetActive(false);
                    secDeploymentState = "CUT";
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

            //Adds the FX groups to the part's list
            this.part.fxGroups.Add(preDeployFX);
            this.part.fxGroups.Add(deployFX);
            this.part.fxGroups.Add(secDeployFX);
            this.part.fxGroups.Add(secPreDeployFX);
        }

        public override string GetInfo()
        {
            //Info in the editor part window
            infoList = "RealChute module:\n";
            if (!secondaryChute)
            {
                infoList += String.Format("Stowed drag: {0}\n", stowedDrag);
                infoList += String.Format("Predeployed drag: {0}\n", preDeployedDrag);
                infoList += String.Format("Deployed drag: {0}\n", deployedDrag);
                infoList += String.Format("Minimum deployment altitude: {0}m\n", minDeploymentAlt);
                infoList += String.Format("Deployment altitude: {0}m\n", deploymentAlt);
                if (cutAlt != -1)
                {
                    infoList += String.Format("Autocut altitude: {0}m\n", cutAlt);
                }

                if (timer > 0)
                {
                    infoList += String.Format("Deployment timer: {0}s\n", timer);
                }

                infoList += String.Format("Predeployment speed: {0}s\n", preDeploymentSpeed);
                infoList += String.Format("Deployment speed: {0}s\n", deploymentSpeed);
                infoList += String.Format("Autocut speed: {0}m/s\n", cutSpeed);
                if (spareChutes >= 0)
                {
                    infoList += String.Format("Spare chutes: {0}\n", spareChutes);
                }
            }

            //In case of dual chutes
            else
            {
                infoList += String.Format("Stowed drag: {0}\n", stowedDrag);
                infoList += String.Format("Predeployed drag: {0}, {1}\n", preDeployedDrag, secPreDeployedDrag);
                infoList += String.Format("Deployed drag: {0}, {1}\n", deployedDrag, secDeployedDrag);
                infoList += String.Format("Minimum deployment altitude: {0}m, {1}m\n", minDeploymentAlt, secMinDeploymentAlt);
                infoList += String.Format("Deployment altitude: {0}m, {1}m\n", deploymentAlt, secDeploymentAlt);
                if (cutAlt != -1 && secCutAlt != -1)
                {
                    infoList += String.Format("Autocut altitude: {0}m, {1}m\n", cutAlt, secCutAlt);
                }
                else if (cutAlt !=-1 && secCutAlt == -1)
                {
                    infoList += String.Format("Autocut altitude: {0}m\n", cutAlt);
                }
                else if (cutAlt == -1 && secCutAlt != -1)
                {
                    infoList += String.Format("Secondary autocut altitude: {0}m\n", secCutAlt);
                }

                if (timer > 0)
                {
                    infoList += String.Format("Deployment timer: {0}s\n", timer);
                }

                infoList += String.Format("Predeployment speed: {0}s, {1}s\n", preDeploymentSpeed, secPreDeploymentSpeed);
                infoList += String.Format("Deployment speed: {0}s, {1}s\n", deploymentSpeed, secDeploymentSpeed);
                infoList += String.Format("Autocut speed: {0}m/s\n", cutSpeed);
                if (spareChutes >= 0)
                {
                    infoList += String.Format("Spare chutes: {0}\n", spareChutes);
                }
            }
            return infoList;
        }

        public override void OnFixedUpdate()
        {
            //To prevent weird shit
            if (!HighLogic.LoadedSceneIsFlight) { return; }

            //Universal values
            currentTime = Time.time;
            CoM = this.vessel.findWorldCenterOfMass();
            up = (CoM - this.vessel.mainBody.position);

            //Deployment clauses and actions. Go figure how they work, I just know they do :P

            //Main Chute
            if (CanDeployChute("MainChute"))
            {
                oneWasDeployed = true;
                if (!wait)
                {
                    if (deploymentState == "STOWED")
                    {
                        if (GetTrueAlt() > minDeploymentAlt)
                        {
                            this.part.stackIcon.SetIconColor(XKCDColors.LightCyan);
                        }

                        else if (GetTrueAlt() > deploymentAlt)
                        {
                            PreDeploy("MainChute");
                        }

                        else if (GetTrueAlt() <= deploymentAlt)
                        {
                            LowDeploy("MainChute");
                        }

                    }

                    else if (deploymentState == "PREDEPLOYED")
                    {
                        parachute.rotation = GetDragDirection();
                        ParachuteNoise(parachute);
                        if (!dragSet)
                        {
                            debutDrag = this.part.maximum_drag;
                            dragSet = true;
                        }

                        DragCalculation(debutDrag, stowedDrag, preDeployedDrag, preDeploymentSpeed, chuteDrag);
                        if (this.part.maximum_drag <= preDeployedDrag)
                        {
                            chuteDrag = GetChuteDrag(debutDrag, stowedDrag);
                        }

                        if (GetTrueAlt() <= deploymentAlt)
                        {
                            Deploy("MainChute");
                            timeSet = false;
                            dragSet = false;
                        }
                    }

                    else if (deploymentState == "LOWDEPLOYED")
                    {
                        parachute.rotation = GetDragDirection();
                        ParachuteNoise(parachute);
                        if (!dragSet)
                        {
                            debutDrag = this.part.maximum_drag;
                            dragSet = true;
                        }

                        DragCalculation(debutDrag, stowedDrag, deployedDrag, preDeploymentSpeed + deploymentSpeed, chuteDrag);
                        chuteDrag = GetChuteDrag(debutDrag, stowedDrag);
                        if (!CheckAnimationPlaying(preDeploymentAnimation) && !queued)
                        {
                            PlayAnimation(deploymentAnimation, 0, 1 / deploymentSpeed);
                            queued = true;
                        }
                    }

                    else if (deploymentState == "DEPLOYED")
                    {
                        parachute.rotation = GetDragDirection();
                        ParachuteNoise(parachute);
                        if (!dragSet)
                        {
                            debutDrag = this.part.maximum_drag;
                            dragSet = true;
                        }

                        DragCalculation(debutDrag, preDeployedDrag, deployedDrag, deploymentSpeed, chuteDrag);
                        chuteDrag = GetChuteDrag(debutDrag, preDeployedDrag);
                    }
                }

                else
                {
                    CheckForWait();
                }
            }

            else if (ChuteMustStop(deploymentState, "MainChute"))
            {
                Cut("MainChute");
            }

            //Secondary chute
            if (CanDeployChute("SecChute"))
            {
                oneWasDeployed = true;
                if (!wait)
                {
                    if (secDeploymentState == "STOWED")
                    {
                        if (GetTrueAlt() > secMinDeploymentAlt)
                        {
                            this.part.stackIcon.SetIconColor(XKCDColors.LightCyan);
                        }

                        else if (GetTrueAlt() > secDeploymentAlt)
                        {
                            PreDeploy("SecChute");
                        }

                        else if (GetTrueAlt() <= secDeploymentAlt)
                        {
                            LowDeploy("SecChute");
                        }
                    }

                    else if (secDeploymentState == "PREDEPLOYED")
                    {
                        secParachute.rotation = GetDragDirection();
                        ParachuteNoise(secParachute);
                        if (!secDragSet)
                        {
                            secDebutDrag = this.part.maximum_drag;
                            secDragSet = true;
                        }

                        DragCalculation(secDebutDrag, stowedDrag, secPreDeployedDrag, secPreDeploymentSpeed, secChuteDrag);
                        secChuteDrag = GetChuteDrag(secDebutDrag, stowedDrag);
                        if (GetTrueAlt() <= secDeploymentAlt)
                        {
                            Deploy("SecChute");
                            timeSet = false;
                            secDragSet = false;
                        }
                    }

                    else if (secDeploymentState == "LOWDEPLOYED")
                    {
                        secParachute.rotation = GetDragDirection();
                        ParachuteNoise(secParachute);
                        if (!secDragSet)
                        {
                            secDebutDrag = this.part.maximum_drag;
                            secDragSet = true;
                        }

                        DragCalculation(secDebutDrag, stowedDrag, secDeployedDrag, secPreDeploymentSpeed + secDeploymentSpeed, secChuteDrag);
                        secChuteDrag = GetChuteDrag(secDebutDrag, stowedDrag);
                        if (!CheckAnimationPlaying(secPreDeploymentAnimation) && !secQueued)
                        {
                            PlayAnimation(secDeploymentAnimation, 0, 1 / secDeploymentSpeed);
                            secQueued = true;
                        }
                    }

                    else if (secDeploymentState == "DEPLOYED")
                    {
                        secParachute.rotation = GetDragDirection();
                        ParachuteNoise(secParachute);
                        if (!secDragSet)
                        {
                            secDebutDrag = this.part.maximum_drag;
                            secDragSet = true;
                        }

                        DragCalculation(secDebutDrag, secPreDeployedDrag, secDeployedDrag, secDeploymentSpeed, secChuteDrag);
                        secChuteDrag = GetChuteDrag(secDebutDrag, secPreDeployedDrag);
                    }
                }

                else
                {
                    CheckForWait();
                }
            }

            else if (ChuteMustStop(secDeploymentState, "SecChute"))
            {
                Cut("SecChute");
            }

            //Actions once all the parachutes are cut
            if (!secondaryChute && deploymentState == "CUT")
            {
                SetRepack();
            }

            else if (BothMustStop())
            {
                if (IsDeployed(deploymentState))
                {
                    Cut("MainChute");
                }

                else
                {
                    deploymentState = "CUT";
                }

                if (IsDeployed(secDeploymentState))
                {
                    Cut("SecChute");
                }

                else
                {
                    secDeploymentState = "CUT";
                }

                SetRepack();
            }

            else if (secondaryChute && deploymentState != "CUT" && secDeploymentState == "CUT")
            {
                SelectIconColour(deploymentState);
            }

            else if (secondaryChute && deploymentState == "CUT" && secDeploymentState != "CUT")
            {
                SelectIconColour(secDeploymentState);
            }
            
            //If the parachute can't be deployed
            if (!oneWasDeployed)
            {
                StagingReset();
            }
        }

        #endregion
    }
}
