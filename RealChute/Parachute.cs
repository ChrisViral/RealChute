using System.Diagnostics;
using System.Linq;
using UnityEngine;
using RealChute.Extensions;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more information contact me on the forum. */

namespace RealChute
{
    public class Parachute
    {
        #region Propreties
        public string material
        {
            get { return this.secondary ? this.module.secMaterial : this.module.material; }
            set
            {
                if (secondary) { this.module.secMaterial = value; }
                else { this.module.material = value; }
            }
        }
        public float preDeployedDiameter
        {
            get { return this.secondary ? this.module.secPreDeployedDiameter : this.module.preDeployedDiameter; }
            set
            {
                if (secondary) { this.module.secPreDeployedDiameter = value; }
                else { this.module.preDeployedDiameter = value; }
            }
        }
        public float preDeployedArea
        {
            get { return RCUtils.GetArea(this.preDeployedDiameter); }
        }
        public float deployedDiameter
        {
            get { return this.secondary ? this.module.secDeployedDiameter : this.module.deployedDiameter; }
            set
            {
                if (secondary) { this.module.secDeployedDiameter = value; }
                else { this.module.deployedDiameter = value; }
            }
        }
        public float deployedArea
        {
            get { return RCUtils.GetArea(this.deployedDiameter); }
        }
        public float chuteMass
        {
            get { return this.deployedArea * this.mat.areaDensity; }
        }
        public bool minIsPressure
        {
            get { return this.secondary ? this.module.secMinIsPressure : this.module.minIsPressure; }
            set
            {
                if (secondary) { this.module.secMinIsPressure = value; }
                else { this.module.minIsPressure = value; }
            }
        }
        public float minDeployment
        {
            get { return this.secondary ? this.module.secMinDeployment : this.module.minDeployment; }
            set
            {
                if (secondary) { this.module.secMinDeployment = value; }
                else { this.module.minDeployment = value; }
            }
        }
        public float minPressure
        {
            get { return this.secondary ? this.module.secMinPressure : this.module.minPressure; }
            set
            {
                if (secondary) { this.module.secMinPressure = value; }
                else { this.module.minPressure = value; }
            }
        }
        public float deploymentAlt
        {
            get { return this.secondary ? this.module.secDeploymentAlt : this.module.deploymentAlt; }
            set
            {
                if (secondary) { this.module.secDeploymentAlt = value; }
                else { this.module.deploymentAlt = value; }
            }
        }
        public float cutAlt
        {
            get { return this.secondary ? this.module.secCutAlt : this.module.cutAlt; }
            set
            {
                if (secondary) { this.module.secCutAlt = value; }
                else { this.module.cutAlt = value; }
            }
        }
        public float preDeploymentSpeed
        {
            get { return this.secondary ? this.module.secPreDeploymentSpeed : this.module.preDeploymentSpeed; }
            set
            {
                if (secondary) { this.module.secPreDeploymentSpeed = value; }
                else { this.module.preDeploymentSpeed = value; }
            }
        }
        public float deploymentSpeed
        {
            get { return this.secondary ? this.module.secDeploymentSpeed : this.module.deploymentSpeed; }
            set
            {
                if (secondary) { this.module.secDeploymentSpeed = value; }
                else { this.module.deploymentSpeed = value; }
            }
        }
        public string preDeploymentAnimation
        {
            get { return this.secondary ? this.module.secPreDeploymentAnimation : this.module.preDeploymentAnimation; }
            set
            {
                if (secondary) { this.module.secPreDeploymentAnimation = value; }
                else { this.module.preDeploymentAnimation = value; }
            }
        }
        public string deploymentAnimation
        {
            get { return this.secondary ? this.module.secDeploymentAnimation : this.module.deploymentAnimation; }
            set
            {
                if (secondary) { this.module.secDeploymentAnimation = value; }
                else { this.module.deploymentAnimation = value; }
            }
        }
        public string parachuteName
        {
            get { return this.secondary ? this.module.secParachuteName : this.module.parachuteName; }
            set
            {
                if (secondary) { this.module.secParachuteName = value; }
                else { this.module.parachuteName = value; }
            }
        }
        public string capName
        {
            get { return this.secondary ? this.module.secCapName : this.module.capName; }
        }
        public float forcedOrientation
        {
            get { return this.secondary ? this.module.secForcedOrientation : this.module.forcedOrientation; }
        }
        public string depState
        {
            get { return this.secondary ? this.module.secDepState : this.module.depState; }
            set
            {
                if (secondary) { this.module.secDepState = value; }
                else { this.module.depState = value; }
            }
        }
        private Part part
        {
            get { return this.module.part; }
        }
        private Parachute sec
        {
            get { return this.secondary ? this.module.main : this.module.secondary; }
        }
        public Vector3 forcePosition
        {
            get { return this.parachute.position; }
        }
        public bool randomDeployment
        {
            get
            {
                if (!this.randomTimer.IsRunning) { this.randomTimer.Start(); }

                if (this.randomTimer.Elapsed.TotalSeconds >= this.randomTime)
                {
                    this.randomTimer.Reset();
                    return true;
                }
                return false;
            }
        }
        public bool deploymentClause
        {
            get { return this.minIsPressure ? this.module.atmPressure >= this.minPressure : this.module.trueAlt <= this.minDeployment; }
        }
        public bool canDeploy
        {
            get
            {
                if (this.module.groundStop || this.module.atmPressure == 0) { return false; }
                else if (deploymentState == DeploymentStates.CUT) { return false; }
                else if (deploymentClause && cutAlt == -1) { return true; }
                else if (deploymentClause && this.module.trueAlt > cutAlt) { return true; }
                else if (this.module.secondaryChute && !deploymentClause && this.module.trueAlt <= sec.cutAlt) { return true; }
                else if (!deploymentClause && isDeployed) { return true; }
                return false;
            }
        }
        public DeploymentStates getState
        {
            get { return RCUtils.states.First(pair => pair.Value == depState).Key; }
        }
        public string stateString
        {
            get { return RCUtils.states.First(pair => pair.Key == deploymentState).Value; }
        }
        public bool isDeployed
        {
            get { return this.stateString.Contains("DEPLOYED"); }
        }
        private Vector3 forcedVector
        {
            get
            {
                if (forcedOrientation >= 90 || forcedOrientation <= 0) { return Vector3.zero; }
                Vector3 follow = forcePosition - this.module.pos;
                float length = Mathf.Tan(forcedOrientation * Mathf.Deg2Rad);
                return follow.normalized * length;
            }
        }
        #endregion

        #region Fields
        internal RealChuteModule module = null;
        internal bool secondary = false;
        internal Transform parachute = null, cap = null;
        internal MaterialDefinition mat = new MaterialDefinition();
        internal Vector3 phase = Vector3.zero;
        internal bool played = false, randomized = false;
        internal Stopwatch randomTimer = new Stopwatch(), dragTimer = new Stopwatch();
        internal DeploymentStates deploymentState = DeploymentStates.STOWED;
        internal float randomX, randomY, randomTime;
        private GUISkin skins = HighLogic.Skin;
        #endregion

        #region Methods
        private void ParachuteNoise()
        {
            if (!this.randomized)
            {
                this.randomX = UnityEngine.Random.Range(0f, 100f);
                this.randomY = UnityEngine.Random.Range(0f, 100f);
                this.randomized = true;
            }

            float time = Time.time;
            Vector3 rotation = new Vector3(5 * (Mathf.PerlinNoise(time, randomX + Mathf.Sin(time)) - 0.5f), 5 * (Mathf.PerlinNoise(time, randomY + Mathf.Sin(time)) - 0.5f), 0);
            parachute.Rotate(rotation);
        }

        private Vector3 LerpDrag(Vector3 to)
        {
            if (phase.magnitude < (to.magnitude - 0.01f) || phase.magnitude > (to.magnitude + 0.01f)) { phase = Vector3.Lerp(phase, to, 0.01f); }
            else { phase = to; }
            return phase;
        }

        private void FollowDragDirection()
        {
            //Smoothes the forced vector
            Vector3 orient = Vector3.zero;
            if (this.module.secondaryChute) { orient = LerpDrag(this.module.bothDeployed ? forcedVector : Vector3.zero); }

            Quaternion drag = Quaternion.identity;
            Vector3 follow = this.module.dragVector + orient;
            if (follow.magnitude > 0)
            {
                drag = this.module.reverseOrientation ? Quaternion.LookRotation(-follow.normalized, parachute.up) : Quaternion.LookRotation(follow.normalized, parachute.up);
                parachute.rotation = drag;
            }
            ParachuteNoise();
        }

        public void LowDeploy()
        {
            this.part.stackIcon.SetIconColor(XKCDColors.RadioactiveGreen);
            this.module.capOff = true;
            this.module.Events["GUIDeploy"].active = false;
            this.module.Events["GUIArm"].active = false;
            this.part.Effect("rcdeploy");
            deploymentState = DeploymentStates.LOWDEPLOYED;
            depState = stateString;
            parachute.gameObject.SetActive(true);
            cap.gameObject.SetActive(false);
            this.module.Events["GUICut"].active = true;
            this.part.PlayAnimation(preDeploymentAnimation, 1 / preDeploymentSpeed);
            dragTimer.Start();
        }

        public void PreDeploy()
        {
            this.part.stackIcon.SetIconColor(XKCDColors.BrightYellow);
            this.module.capOff = true;
            this.module.Events["GUIDeploy"].active = false;
            this.module.Events["GUIArm"].active = false;
            this.part.Effect("rcpredeploy");
            deploymentState = DeploymentStates.PREDEPLOYED;
            depState = stateString;
            parachute.gameObject.SetActive(true);
            cap.gameObject.SetActive(false);
            this.module.Events["GUICut"].active = true;
            this.part.PlayAnimation(preDeploymentAnimation, 1 / preDeploymentSpeed);
            dragTimer.Start();
        }

        public void Deploy()
        {
            this.part.stackIcon.SetIconColor(XKCDColors.RadioactiveGreen);
            this.part.Effect("rcdeploy");
            deploymentState = DeploymentStates.DEPLOYED;
            depState = stateString;
            if (!this.part.CheckAnimationPlaying(preDeploymentAnimation))
            {
                dragTimer.Reset();
                dragTimer.Start();
                this.part.PlayAnimation(deploymentAnimation, 1 / deploymentSpeed);
                this.played = true;
            }
            else { this.played = false; }
        }

        public void Cut()
        {
            this.part.Effect("rccut");
            deploymentState = DeploymentStates.CUT;
            depState = stateString;
            parachute.gameObject.SetActive(false);
            this.module.Events["GUICut"].active = false;
            this.played = false;
            if (!this.module.secondaryChute || sec.deploymentState == DeploymentStates.CUT) { this.module.SetRepack(); }
            else if (this.module.secondaryChute && sec.deploymentState == DeploymentStates.STOWED) { this.module.armed = true; }
            dragTimer.Reset();
        }

        private float DragDeployment(float time, float debutArea, float endArea)
        {
            if (!dragTimer.IsRunning) { dragTimer.Start(); }

            if (dragTimer.Elapsed.TotalSeconds <= time)
            {
                float deploymentTime = (Mathf.Exp((float)dragTimer.Elapsed.TotalSeconds) / Mathf.Exp(time)) * ((float)dragTimer.Elapsed.TotalSeconds / time);
                return Mathf.Lerp(debutArea, endArea, deploymentTime);
            }
            else { return endArea; }
        }

        private Vector3 DragForce(float startArea, float targetArea, float time)
        {
            return this.module.DragCalculation(DragDeployment(time, startArea, targetArea), mat.dragCoefficient) * this.module.dragVector * this.module.joke;
        }

        internal void UpdateParachute()
        {
            if (canDeploy)
            {
                this.module.oneWasDeployed = true;
                if (!this.module.wait)
                {
                    if (isDeployed) { FollowDragDirection(); }

                    switch (deploymentState)
                    {
                        case DeploymentStates.STOWED:
                            {
                                this.part.stackIcon.SetIconColor(XKCDColors.LightCyan);
                                if (this.module.trueAlt > deploymentAlt && deploymentClause && randomDeployment) { PreDeploy(); }
                                else if (this.module.trueAlt <= deploymentAlt && randomDeployment) { LowDeploy(); }
                                break;
                            }

                        case DeploymentStates.PREDEPLOYED:
                            {
                                this.part.rigidbody.AddForceAtPosition(DragForce(0, preDeployedArea, preDeploymentSpeed), forcePosition, ForceMode.Force);
                                if (this.module.trueAlt <= deploymentAlt) { Deploy(); }
                                break;
                            }
                        case DeploymentStates.LOWDEPLOYED:
                            {
                                this.part.rigidbody.AddForceAtPosition(DragForce(0, deployedArea, preDeploymentSpeed + deploymentSpeed), forcePosition, ForceMode.Force);
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
                                this.part.rigidbody.AddForceAtPosition(DragForce(preDeployedArea, deployedArea, deploymentSpeed), forcePosition, ForceMode.Force);
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
            }
            //Deactivation
            else if (!canDeploy && isDeployed) { Cut(); }
        }

        internal void UpdateGUI()
        {
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
                    if (this.module.secondaryChute)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Copy to " + (this.secondary ? "main" : "sec"), skins.button, GUILayout.Height(20), GUILayout.Width(100)))
                        {
                            this.sec.minIsPressure = this.minIsPressure;
                            this.sec.minPressure = this.minPressure;
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
                    if (this.module.secondaryChute)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Copy to " + (this.secondary ? "main" : "sec"), skins.button, GUILayout.Height(20), GUILayout.Width(100)))
                        {
                            this.sec.minIsPressure = this.minIsPressure;
                            this.sec.minDeployment = this.minDeployment;
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
                if (this.module.secondaryChute)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Copy to " + (this.secondary ? "main" : "sec"), skins.button, GUILayout.Height(20), GUILayout.Width(100))) { this.sec.deploymentAlt = this.deploymentAlt; }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }
            if (cutAlt > 0) { GUILayout.Label("Autocut altitude: " + cutAlt + "m", skins.label); }
            GUILayout.Label("Predeployment speed: " + preDeploymentSpeed + "s", skins.label);
            GUILayout.Label("Deployment speed: " + deploymentSpeed + "s", skins.label);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a parachute object from the given RealChuteModule
        /// </summary>
        /// <param name="module">RealChuteModule to create the Parachute from</param>
        /// <param name="secondary">Wether this Parachute is the main or secondary parachute</param>
        public Parachute(RealChuteModule module, bool secondary)
        {
            this.module = module;
            this.secondary = secondary;
            if (this.secondary && this.material == "empty") { this.material = sec.material; }
            this.module.materials.TryGetMaterial(material, ref mat);
            this.parachute = this.part.FindModelTransform(parachuteName);
            this.cap = this.part.FindModelTransform(capName);
            this.parachute.gameObject.SetActive(false);
            this.part.InitiateAnimation(preDeploymentAnimation);
            this.part.InitiateAnimation(deploymentAnimation);

            if (!this.module.initiated)
            {
                deploymentState = DeploymentStates.STOWED;
                depState = "STOWED";
                played = false;
                this.cap.gameObject.SetActive(true);
            }

            if (HighLogic.LoadedSceneIsFlight)
            {
                deploymentState = getState;
                if (this.module.capOff)
                {
                    this.part.stackIcon.SetIconColor(XKCDColors.Red);
                    this.cap.gameObject.SetActive(false);
                }
            }
        }
        #endregion 
    }
}