using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RealChute.Extensions;
using RealChute.Libraries;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more information contact me on the forum. */

namespace RealChute
{
    public class Parachute
    {
        #region Propreties
        //Predeployed area of the chute
        public float preDeployedArea
        {
            get { return RCUtils.GetArea(this.preDeployedDiameter); }
        }

        //Deployed area of the chute
        public float deployedArea
        {
            get { return RCUtils.GetArea(this.deployedDiameter); }
        }

        //Mass of the chute
        public float chuteMass
        {
            get { return this.deployedArea * this.mat.areaDensity; }
        }

        //Part this chute is associated with
        private Part part
        {
            get { return this.module.part; }
        }

        //Position to apply the force to
        public Vector3 forcePosition
        {
            get { return this.parachute.position; }
        }

        //If the random deployment timer has been spent
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

        //If the parachute has passed the minimum deployment clause
        public bool deploymentClause
        {
            get { return this.minIsPressure ? this.module.atmPressure >= this.minPressure : this.module.trueAlt <= this.minDeployment; }
        }

        //If the parachute can deploy
        public bool canDeploy
        {
            get
            {
                if (this.module.groundStop || this.module.atmPressure == 0) { return false; }
                else if (deploymentState == DeploymentStates.CUT) { return false; }
                else if (deploymentClause && cutAlt == -1) { return true; }
                else if (deploymentClause && this.module.trueAlt > cutAlt) { return true; }
                else if (this.module.secondaryChute && !deploymentClause && this.parachutes.Any(p => this.module.trueAlt <= p.cutAlt)) { return true; }
                else if (!deploymentClause && isDeployed) { return true; }
                return false;
            }
        }

        //Returns the current DeploymentState
        public DeploymentStates getState
        {
            get { return RCUtils.states.First(pair => pair.Value == depState).Key; }
        }

        //Returns the current DeploymentState string
        public string stateString
        {
            get { return RCUtils.states.First(pair => pair.Key == deploymentState).Value; }
        }

        //If the parachute is deployed
        public bool isDeployed
        {
            get { return this.stateString.Contains("DEPLOYED"); }
        }

        //The added vector to drag to angle the parachute
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

        //The parachutes of the associated module
        public List<Parachute> parachutes
        {
            get { return this.module.parachutes; }
        }
        #endregion

        #region Fields
        //Parachute
        public string material = "Nylon";
        public float preDeployedDiameter = 1, deployedDiameter = 25;
        public bool minIsPressure = false;
        public float minDeployment = 25000, minPressure = 0.01f;
        public float deploymentAlt = 700, cutAlt = -1;
        public float preDeploymentSpeed = 2, deploymentSpeed = 6;
        public string preDeploymentAnimation = "semiDeploy", deploymentAnimation = "fullyDeploy";
        public string parachuteName = "parachute", capName = "cap", baseParachuteName = string.Empty;
        public float forcedOrientation = 0;
        public string depState = "STOWED";

        //Flight
        internal RealChuteModule module = null;
        internal bool secondary = false;
        private Animation anim = null;
        internal Transform parachute = null, cap = null;
        internal MaterialDefinition mat = new MaterialDefinition();
        internal Vector3 phase = Vector3.zero;
        internal bool played = false, randomized = false;
        internal Stopwatch randomTimer = new Stopwatch(), dragTimer = new Stopwatch();
        internal DeploymentStates deploymentState = DeploymentStates.STOWED;
        internal float randomX, randomY, randomTime;
        private GUISkin skins = HighLogic.Skin;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a parachute object from the given RealChuteModule
        /// </summary>
        /// <param name="module">RealChuteModule to create the Parachute from</param>
        /// <param name="node">ConfigNode to create the parachute from</param>
        public Parachute(RealChuteModule module, ConfigNode node)
        {
            this.module = module;
            Load(node);
        }
        #endregion

        #region Methods
        //Adds a random noise to the parachute movement
        private void ParachuteNoise()
        {
            if (!this.randomized)
            {
                this.randomX = UnityEngine.Random.Range(0f, 100f);
                this.randomY = UnityEngine.Random.Range(0f, 100f);
                this.randomized = true;
            }

            float time = Time.time;
            parachute.Rotate(new Vector3(5 * (Mathf.PerlinNoise(time, randomX + Mathf.Sin(time)) - 0.5f), 5 * (Mathf.PerlinNoise(time, randomY + Mathf.Sin(time)) - 0.5f), 0));
        }

        //Lerps the drag vector between upright and the forced angle
        private Vector3 LerpDrag(Vector3 to)
        {
            if (phase.magnitude < (to.magnitude - 0.01f) || phase.magnitude > (to.magnitude + 0.01f)) { phase = Vector3.Lerp(phase, to, 0.01f); }
            else { phase = to; }
            return phase;
        }

        //Makes the canopy follow drag direction
        private void FollowDragDirection()
        {
            //Smoothes the forced vector
            Vector3 orient = Vector3.zero;
            if (this.module.secondaryChute) { orient = LerpDrag(this.module.manyDeployed ? forcedVector : Vector3.zero); }

            Vector3 follow = this.module.dragVector + orient;
            if (follow.sqrMagnitude > 0)
            {
                Quaternion drag = this.module.reverseOrientation ? Quaternion.LookRotation(-follow.normalized, parachute.up) : Quaternion.LookRotation(follow.normalized, parachute.up);
                parachute.rotation = drag;
            }
            ParachuteNoise();
        }

        //Parachute low deployment
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
            this.part.PlayAnimation(preDeploymentAnimation, 1f / preDeploymentSpeed);
            dragTimer.Start();
        }

        //Parachute predeployment
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
            this.part.PlayAnimation(preDeploymentAnimation, 1f / preDeploymentSpeed);
            dragTimer.Start();
        }

        //Parachute deployment
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
                this.part.PlayAnimation(deploymentAnimation, 1f / deploymentSpeed);
                this.played = true;
            }
            else { this.played = false; }
        }

        //Parachute cutting
        public void Cut()
        {
            this.part.Effect("rccut");
            deploymentState = DeploymentStates.CUT;
            depState = stateString;
            parachute.gameObject.SetActive(false);
            this.module.Events["GUICut"].active = false;
            this.played = false;
            if (!this.module.secondaryChute || this.parachutes.All(p => p.deploymentState == DeploymentStates.CUT)) { this.module.SetRepack(); }
            else if (this.module.secondaryChute && this.parachutes.Any(p => p.deploymentState == DeploymentStates.STOWED)) { this.module.armed = true; }
            dragTimer.Reset();
        }

        //Calculates parachute deployed area
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

        //Drag force vector
        private Vector3 DragForce(float startArea, float targetArea, float time)
        {
            return this.module.DragCalculation(DragDeployment(time, startArea, targetArea), mat.dragCoefficient) * this.module.dragVector * (RealChuteSettings.fetch.jokeActivated ? -1 : 1);
        }

        //Parachute function
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

        //Info window GUI
        internal void UpdateGUI()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Material: ").AppendLine(mat.name);
            builder.Append("Drag coefficient: ").AppendLine(mat.dragCoefficient.ToString("0.00#"));
            builder.Append("Predeployed diameter: ").Append(preDeployedDiameter).Append("m\t\tarea: ").Append(preDeployedArea.ToString("0.###")).AppendLine("m²");
            builder.Append("Deployed diameter: ").Append(deployedDiameter).Append("m\t\tarea: ").Append(deployedArea.ToString("0.###")).Append("m²");
            GUILayout.Label(builder.ToString(), skins.label);
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
                        if (GUILayout.Button("Copy to others", skins.button, GUILayout.Height(20), GUILayout.Width(100)))
                        {
                            this.parachutes.ForEach(p => p.minIsPressure = this.minIsPressure);
                            this.parachutes.ForEach(p => p.minPressure = this.minPressure);
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
                        if (GUILayout.Button("Copy to others", skins.button, GUILayout.Height(20), GUILayout.Width(100)))
                        {
                            this.parachutes.ForEach(p => p.minIsPressure = this.minIsPressure);
                            this.parachutes.ForEach(p => p.minDeployment = this.minDeployment);
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
                    if (GUILayout.Button("Copy to others", skins.button, GUILayout.Height(20), GUILayout.Width(100))) { this.parachutes.ForEach(p => p.deploymentAlt = this.deploymentAlt); }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }
            builder = new StringBuilder();
            if (cutAlt > 0) { builder.Append("Autocut altitude: ").Append(cutAlt).AppendLine("m"); }
            builder.Append("Predeployment speed: ").Append(preDeploymentSpeed).AppendLine("s");
            builder.Append("Deployment speed: ").Append(deploymentSpeed).Append("s");
            GUILayout.Label(builder.ToString(), skins.label);
        }

        //Repack actions
        internal void Repack()
        {
            deploymentState = DeploymentStates.STOWED;
            depState = stateString;
            cap.gameObject.SetActive(true);
        }

        //Initializes the chute
        public void Initialize()
        {
            this.module.materials.TryGetMaterial(material, ref mat);

            anim = this.part.FindModelAnimators(capName).FirstOrDefault();
            this.cap = this.part.FindModelTransform(capName);

            this.parachute = this.part.FindModelTransform(parachuteName);
            if (this.parachute == null && !string.IsNullOrEmpty(baseParachuteName))
            {
                this.parachute = this.part.FindModelTransform(baseParachuteName);
            }
            this.parachute.gameObject.SetActive(false);
            this.part.InitiateAnimation(preDeploymentAnimation);
            this.part.InitiateAnimation(deploymentAnimation);

            if (string.IsNullOrEmpty(baseParachuteName)) { baseParachuteName = parachuteName; }

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

        #region Load/Save
        /// <summary>
        /// Loads the Parachute from a ConfigNode.
        /// </summary>
        /// <param name="node">Node to load the Parachute from</param>
        private void Load(ConfigNode node)
        {
            node.TryGetValue("material", ref material);
            node.TryGetValue("preDeployedDiameter", ref preDeployedDiameter);
            node.TryGetValue("deployedDiameter", ref deployedDiameter);
            node.TryGetValue("minIsPressure", ref minIsPressure);
            node.TryGetValue("minDeployment", ref minDeployment);
            node.TryGetValue("minPressure", ref minPressure);
            node.TryGetValue("deploymentAlt", ref deploymentAlt);
            node.TryGetValue("cutAlt", ref cutAlt);
            node.TryGetValue("preDeploymentSpeed", ref preDeploymentSpeed);
            node.TryGetValue("deploymentSpeed", ref deploymentSpeed);
            node.TryGetValue("parachuteName", ref parachuteName);
            node.TryGetValue("baseParachuteName", ref baseParachuteName);
            node.TryGetValue("capName", ref capName);
            node.TryGetValue("preDeploymentAnimation", ref preDeploymentAnimation);
            node.TryGetValue("deploymentAnimation", ref deploymentAnimation);
            node.TryGetValue("forcedOrientation", ref forcedOrientation);
            node.TryGetValue("depState", ref depState);
            if (!MaterialsLibrary.instance.TryGetMaterial(material, ref mat))
            {
                material = "Nylon"; 
                mat = MaterialsLibrary.instance.GetMaterial("Nylon");
            }
        }

        /// <summary>
        /// Saves the Parachute to a ConfigNode and returns this node.
        /// </summary>
        public ConfigNode Save()
        {
            ConfigNode node = new ConfigNode("PARACHUTE");
            node.AddValue("material", material);
            node.AddValue("preDeployedDiameter", preDeployedDiameter);
            node.AddValue("deployedDiameter", deployedDiameter);
            node.AddValue("minIsPressure", minIsPressure);
            node.AddValue("minDeployment", minDeployment);
            node.AddValue("minPressure", minPressure);
            node.AddValue("deploymentAlt", deploymentAlt);
            node.AddValue("cutAlt", cutAlt);
            node.AddValue("preDeploymentSpeed", preDeploymentSpeed);
            node.AddValue("deploymentSpeed", deploymentSpeed);
            node.AddValue("parachuteName", parachuteName);
            node.AddValue("baseParachuteName", baseParachuteName);
            node.AddValue("capName", capName);
            node.AddValue("preDeploymentAnimation", preDeploymentAnimation);
            node.AddValue("deploymentAnimation", deploymentAnimation);
            node.AddValue("forcedOrientation", forcedOrientation);
            node.AddValue("depState", depState);
            return node;
        }
        #endregion
    }
}