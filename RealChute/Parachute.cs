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
                if (!this.randomTimer.isRunning) { this.randomTimer.Start(); }

                if (this.randomTimer.elapsed.TotalSeconds >= this.randomTime)
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
            get
            {
                if (this.minIsPressure) { return this.module.atmPressure >= this.minPressure; }
                else if (this.minDeployment == 0) { return this.part.vessel.LandedOrSplashed; }
                return this.module.trueAlt <= this.minDeployment;
            }
        }

        //If the parachute can deploy
        public bool canDeploy
        {
            get
            {
                if (this.module.groundStop || this.module.atmPressure == 0) { return false; }
                else if (this.deploymentState == DeploymentStates.CUT) { return false; }
                else if (this.deploymentClause && this.cutAlt == -1) { return true; }
                else if (this.deploymentClause && this.module.trueAlt > this.cutAlt) { return true; }
                else if (this.module.secondaryChute && !this.deploymentClause && this.parachutes.Exists(p => this.module.trueAlt <= p.cutAlt)) { return true; }
                else if (!this.deploymentClause && this.isDeployed) { return true; }
                return false;
            }
        }

        //If the parachute is deployed
        public bool isDeployed
        {
            get
            {
                switch (this.deploymentState)
                {
                    case DeploymentStates.PREDEPLOYED:
                    case DeploymentStates.DEPLOYED:
                        return true;

                    default:
                        return false;
                }
            }
        }

        //The added vector to drag to angle the parachute
        private Vector3 forcedVector
        {
            get
            {
                if (forcedOrientation >= 90 || forcedOrientation <= 0) { return Vector3.zero; }
                Vector3 follow = this.forcePosition - this.module.pos;
                float length = Mathf.Tan(this.forcedOrientation * Mathf.Deg2Rad);
                return  follow.normalized * length;
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
        public double time = 0;
        public string preDeploymentAnimation = string.Empty, deploymentAnimation = string.Empty;
        public string parachuteName = string.Empty, capName = string.Empty, baseParachuteName = string.Empty;
        public float forcedOrientation = 0;

        //Flight
        internal RealChuteModule module = null;
        internal bool secondary = false;
        private Animation anim = null;
        internal Transform parachute = null, cap = null;
        internal MaterialDefinition mat = new MaterialDefinition();
        internal Vector3 phase = Vector3.zero;
        internal bool randomized = false;
        internal PhysicsWatch randomTimer = new PhysicsWatch(), dragTimer = new PhysicsWatch();
        internal float randomX, randomY, randomTime;
        private GUISkin skins = HighLogic.Skin;
        public DeploymentStates deploymentState = DeploymentStates.STOWED;
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
                Random random = new Random();
                this.randomX = (float)random.NextDouble() * 100;
                this.randomY = (float)random.NextDouble() * 100;
                this.randomized = true;
            }

            float time = Time.time;
            this.parachute.Rotate(new Vector3(5 * (Mathf.PerlinNoise(time, this.randomX + Mathf.Sin(time)) - 0.5f), 5 * (Mathf.PerlinNoise(time, this.randomY + Mathf.Sin(time)) - 0.5f), 0));
        }

        //Lerps the drag vector between upright and the forced angle
        private Vector3 LerpDrag(Vector3 to)
        {
            if (this.phase.magnitude < (to.magnitude - 0.01f) || this.phase.magnitude > (to.magnitude + 0.01f)) { this.phase = Vector3.Lerp(this.phase, to, 0.01f); }
            else { this.phase = to; }
            return phase;
        }

        //Makes the canopy follow drag direction
        private void FollowDragDirection()
        {
            //Smoothes the forced vector
            Vector3 orient = Vector3.zero;
            if (this.module.secondaryChute) { orient = LerpDrag(this.module.manyDeployed ? this.forcedVector : Vector3.zero); }

            Vector3 follow = this.module.dragVector + orient;
            if (follow.sqrMagnitude > 0)
            {
                this.parachute.rotation = this.module.reverseOrientation ? Quaternion.LookRotation(-follow.normalized, this.parachute.up) : Quaternion.LookRotation(follow.normalized, this.parachute.up);
            }
            ParachuteNoise();
        }

        //Parachute predeployment
        public void PreDeploy()
        {
            this.module.oneWasDeployed = true;
            this.part.stackIcon.SetIconColor(XKCDColors.BrightYellow);
            this.part.Effect("rcpredeploy");
            this.deploymentState = DeploymentStates.PREDEPLOYED;
            this.parachute.gameObject.SetActive(true);
            this.cap.gameObject.SetActive(false);
            if (this.dragTimer.elapsedMilliseconds != 0) { this.part.SkipToAnimationEnd(this.preDeploymentAnimation); }
            else { this.part.PlayAnimation(this.preDeploymentAnimation, 1f / this.preDeploymentSpeed); }
            this.dragTimer.Start();
        }

        //Parachute deployment
        public void Deploy()
        {
            this.part.stackIcon.SetIconColor(XKCDColors.RadioactiveGreen);
            this.part.Effect("rcdeploy");
            this.deploymentState = DeploymentStates.DEPLOYED;
            this.dragTimer.Restart();
            this.part.PlayAnimation(this.deploymentAnimation, 1f / this.deploymentSpeed);
        }

        //Parachute cutting
        public void Cut()
        {
            this.part.Effect("rccut");
            this.deploymentState = DeploymentStates.CUT;
            this.parachute.gameObject.SetActive(false);
            if (!this.module.secondaryChute || this.parachutes.TrueForAll(p => p.deploymentState == DeploymentStates.CUT)) { this.module.SetRepack(); }
            else if (this.module.secondaryChute && this.parachutes.Exists(p => p.deploymentState == DeploymentStates.STOWED)) { this.module.armed = true; }
            this.dragTimer.Reset();
        }

        //Repack actions
        public void Repack()
        {
            this.deploymentState = DeploymentStates.STOWED;
            this.randomTimer.Reset();
            this.dragTimer.Reset();
            this.time = 0;
            this.cap.gameObject.SetActive(true);
        }

        //Calculates parachute deployed area
        private float DragDeployment(float time, float debutDiameter, float endDiameter)
        {
            if (!this.dragTimer.isRunning) { this.dragTimer.Start(); }

            this.time = this.dragTimer.elapsed.TotalSeconds;
            if (this.time <= time)
            {
                //While this looks linear, area scales with the square of the diameter, and therefore
                //Deployment will be quadratic. The previous exponential function was too slow at first and rough at the end
                float currentDiam = Mathf.Lerp(debutDiameter, endDiameter, (float)(this.time / time));
                return RCUtils.GetArea(currentDiam);
            }
            this.dragTimer.Stop();
            return RCUtils.GetArea(endDiameter);
        }

        //Drag force vector with deployment
        private Vector3 DragForce(float debutDiameter, float endDiameter, float time)
        {
            return this.module.DragCalculation(DragDeployment(time, debutDiameter, endDiameter), this.mat.dragCoefficient) * this.module.dragVector
                * (RealChuteSettings.fetch.jokeActivated ? -1 : 1); //April Fool's Prank '13
        }

        //Drag force static
        private Vector3 DragForce(float area)
        {
            return this.module.DragCalculation(area, this.mat.dragCoefficient) * this.module.dragVector
                * (RealChuteSettings.fetch.jokeActivated ? -1 : 1); //April Fool's Prank '13
        }

        //Parachute function
        internal void UpdateParachute()
        {
            if (this.canDeploy)
            {
                if (this.isDeployed) { FollowDragDirection(); }

                switch (this.deploymentState)
                {
                    case DeploymentStates.STOWED:
                        {
                            this.part.stackIcon.SetIconColor(XKCDColors.LightCyan);
                            if (this.deploymentClause && this.randomDeployment) { PreDeploy(); }
                            break;
                        }

                    case DeploymentStates.PREDEPLOYED:
                        {
                            if (!this.dragTimer.isRunning)
                            {
                                this.part.Rigidbody.AddForceAtPosition(DragForce(this.preDeployedArea), this.forcePosition, ForceMode.Force);
                                if (this.module.trueAlt <= this.deploymentAlt && this.dragTimer.elapsed.TotalSeconds >= this.preDeploymentSpeed) { Deploy(); }
                            }
                            else { this.part.Rigidbody.AddForceAtPosition(DragForce(0, this.preDeployedDiameter, this.preDeploymentSpeed), this.forcePosition, ForceMode.Force); }
                            break;
                        }

                    case DeploymentStates.DEPLOYED:
                        {
                            if (!this.dragTimer.isRunning)
                            {
                                this.part.Rigidbody.AddForceAtPosition(DragForce(this.deployedArea), this.forcePosition, ForceMode.Force);
                            }
                            else { this.part.rigidbody.AddForceAtPosition(DragForce(this.preDeployedDiameter, this.deployedDiameter, this.deploymentSpeed), this.forcePosition, ForceMode.Force); }
                            break;
                        }

                    default:
                        break;
                }
            }
            //Deactivation
            else if (!this.canDeploy && this.isDeployed) { Cut(); }
        }

        //Initializes the chute
        public void Initialize()
        {
            if (!this.module.materials.TryGetMaterial(this.material, ref this.mat))
            {
                this.mat = this.module.materials.GetMaterial("Nylon");
            }

            //I know this seems random, but trust me, it's needed, else some parachutes don't animate, because fuck you, that's why.
            this.anim = this.part.FindModelAnimators(this.capName).FirstOrDefault();

            this.cap = this.part.FindModelTransform(this.capName);
            this.parachute = this.part.FindModelTransform(this.parachuteName);
            if (this.parachute == null && !string.IsNullOrEmpty(this.baseParachuteName))
            {
                this.parachute = this.part.FindModelTransform(this.baseParachuteName);
            }
            this.parachute.gameObject.SetActive(true);
            this.part.InitiateAnimation(this.preDeploymentAnimation);
            this.part.InitiateAnimation(this.deploymentAnimation);
            this.parachute.gameObject.SetActive(false);

            if (string.IsNullOrEmpty(this.baseParachuteName)) { this.baseParachuteName = this.parachuteName; }

            if (!this.module.initiated)
            {
                this.cap.gameObject.SetActive(true);
            }
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (this.time != 0) { this.dragTimer = new PhysicsWatch(this.time); }
                if (this.deploymentState != DeploymentStates.STOWED)
                {
                    this.part.stackIcon.SetIconColor(XKCDColors.Red);
                    this.cap.gameObject.SetActive(false);
                }

                if (this.module.staged && this.deploymentState != DeploymentStates.CUT)
                {
                    this.deploymentState = DeploymentStates.STOWED;
                }
            }
        }
        #endregion

        #region GUI
        //Info window GUI
        internal void UpdateGUI()
        {
            //Initial label
            StringBuilder builder = new StringBuilder();
            builder.Append("Material: ").AppendLine(this.mat.name);
            builder.Append("Drag coefficient: ").AppendLine(this.mat.dragCoefficient.ToString("0.00#"));
            builder.Append("Predeployed diameter: ").Append(this.preDeployedDiameter).Append("m\t\tarea: ").Append(this.preDeployedArea.ToString("0.###")).AppendLine("m²");
            builder.Append("Deployed diameter: ").Append(this.deployedDiameter).Append("m\t\tarea: ").Append(this.deployedArea.ToString("0.###")).Append("m²");
            GUILayout.Label(builder.ToString(), this.skins.label);

            if (HighLogic.LoadedSceneIsFlight)
            {
                //Pressure/altitude predeployment toggle
                GUILayout.BeginHorizontal();
                GUILayout.Label("Predeployment:", this.skins.label);
                if (GUILayout.Toggle(!this.minIsPressure, "altitude", this.skins.toggle)) { this.minIsPressure = false; }
                GUILayout.FlexibleSpace();
                if (GUILayout.Toggle(this.minIsPressure, "pressure", this.skins.toggle)) { this.minIsPressure = true; }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            //Predeployment pressure selection
            if (this.minIsPressure)
            {
                GUILayout.Label("Predeployment pressure: " + this.minPressure + "atm", this.skins.label);
                if (HighLogic.LoadedSceneIsFlight)
                {
                    //Predeployment pressure slider
                    this.minPressure = GUILayout.HorizontalSlider(this.minPressure, 0.005f, 1, this.skins.horizontalSlider, this.skins.horizontalSliderThumb);

                    //Copy to symmetry counterparts button
                    CopyToOthers(p =>
                    {
                        p.minIsPressure = this.minIsPressure;
                        p.minPressure = this.minPressure;
                    });
                }
            }

            //Predeployment altitude selection
            else
            {
                GUILayout.Label("Predeployment altitude: " + this.minDeployment + "m", this.skins.label);
                if (HighLogic.LoadedSceneIsFlight)
                {
                    //Predeployment altitude slider
                    this.minDeployment = GUILayout.HorizontalSlider(this.minDeployment, 100, 20000, this.skins.horizontalSlider, this.skins.horizontalSliderThumb);

                    //Copy to symmetry counterparts button
                    CopyToOthers(p =>
                    {
                        p.minIsPressure = this.minIsPressure;
                        p.minDeployment = this.minDeployment;
                    });
                }
            }

            //Deployment altitude selection
            GUILayout.Label("Deployment altitude: " + this.deploymentAlt + "m", this.skins.label);
            if (HighLogic.LoadedSceneIsFlight)
            {
                //Deployment altitude slider
                this.deploymentAlt = GUILayout.HorizontalSlider(this.deploymentAlt, 50, 10000, this.skins.horizontalSlider, this.skins.horizontalSliderThumb);

                //Copy to symmetry counterparts button
                CopyToOthers(p => p.deploymentAlt = this.deploymentAlt);
            }

            //Other labels
            builder = new StringBuilder();
            if (this.cutAlt > 0) { builder.Append("Autocut altitude: ").Append(this.cutAlt).AppendLine("m"); }
            builder.Append("Predeployment speed: ").Append(this.preDeploymentSpeed).AppendLine("s");
            builder.Append("Deployment speed: ").Append(this.deploymentSpeed).Append("s");
            GUILayout.Label(builder.ToString(), this.skins.label);
        }

        //Copies the given values to the other parachutes
        private void CopyToOthers(Action<Parachute> action)
        {
            if (this.module.secondaryChute)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Copy to symmetry counterparts", this.skins.button, GUILayout.Height(20), GUILayout.Width(100)))
                {
                    foreach (Parachute p in this.parachutes)
                    {
                        if (p == this) { continue; }
                        action(p);
                    }
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
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
            node.TryGetValue("material", ref this.material);
            node.TryGetValue("preDeployedDiameter", ref this.preDeployedDiameter);
            node.TryGetValue("deployedDiameter", ref this.deployedDiameter);
            node.TryGetValue("minIsPressure", ref this.minIsPressure);
            node.TryGetValue("minDeployment", ref this.minDeployment);
            node.TryGetValue("minPressure", ref this.minPressure);
            node.TryGetValue("deploymentAlt", ref this.deploymentAlt);
            node.TryGetValue("cutAlt", ref this.cutAlt);
            node.TryGetValue("preDeploymentSpeed", ref this.preDeploymentSpeed);
            node.TryGetValue("deploymentSpeed", ref this.deploymentSpeed);
            node.TryGetValue("time", ref this.time);
            node.TryGetValue("parachuteName", ref this.parachuteName);
            node.TryGetValue("baseParachuteName", ref this.baseParachuteName);
            node.TryGetValue("capName", ref this.capName);
            node.TryGetValue("preDeploymentAnimation", ref this.preDeploymentAnimation);
            node.TryGetValue("deploymentAnimation", ref this.deploymentAnimation);
            node.TryGetValue("forcedOrientation", ref this.forcedOrientation);
            string state = "STOWED";
            node.TryGetValue("depState", ref state);
            this.deploymentState = EnumUtils.GetValue<DeploymentStates>(state);
            MaterialsLibrary.instance.TryGetMaterial(this.material, ref this.mat);
            Transform p = this.part.FindModelTransform(this.parachuteName);
            if (p != null) { p.gameObject.SetActive(false); }
        }

        /// <summary>
        /// Saves the Parachute to a ConfigNode and returns this node.
        /// </summary>
        public ConfigNode Save()
        {
            ConfigNode node = new ConfigNode("PARACHUTE");
            node.AddValue("material", this.material);
            node.AddValue("preDeployedDiameter", this.preDeployedDiameter);
            node.AddValue("deployedDiameter", this.deployedDiameter);
            node.AddValue("minIsPressure", this.minIsPressure);
            node.AddValue("minDeployment", this.minDeployment);
            node.AddValue("minPressure", this.minPressure);
            node.AddValue("deploymentAlt", this.deploymentAlt);
            node.AddValue("cutAlt", this.cutAlt);
            node.AddValue("preDeploymentSpeed", this.preDeploymentSpeed);
            node.AddValue("deploymentSpeed", this.deploymentSpeed);
            node.AddValue("time", this.time);
            node.AddValue("parachuteName", this.parachuteName);
            node.AddValue("baseParachuteName", this.baseParachuteName);
            node.AddValue("capName", this.capName);
            node.AddValue("preDeploymentAnimation", this.preDeploymentAnimation);
            node.AddValue("deploymentAnimation", this.deploymentAnimation);
            node.AddValue("forcedOrientation", this.forcedOrientation);
            node.AddValue("depState", EnumUtils.GetName(this.deploymentState));
            return node;
        }
        #endregion
    }
}