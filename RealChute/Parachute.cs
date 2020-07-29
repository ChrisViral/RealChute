using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RealChute.Extensions;
using RealChute.Libraries.MaterialsLibrary;
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
    public class Parachute
    {
        #region Propreties
        //Predeployed area of the chute
        public float PreDeployedArea => RCUtils.GetArea(this.preDeployedDiameter);

        //Deployed area of the chute
        public float DeployedArea => RCUtils.GetArea(this.deployedDiameter);

        //Mass of the chute
        public float ChuteMass => this.DeployedArea * this.mat.AreaDensity;

        //The inverse of the thermal mass of the parachute
        private double InvThermalMass
        {
            get
            {
                if (this.thermMass == 0) { this.thermMass = 1d / (this.mat.SpecificHeat * this.ChuteMass); }
                return this.thermMass;
            }
        }

        //The current useful convection area
        private double ConvectionArea
        {
            get
            {
                if (this.DeploymentState == DeploymentStates.PREDEPLOYED && this.dragTimer.Elapsed.Seconds < this.preDeploymentSpeed)
                {
                    return UtilMath.Lerp(0, this.DeployedArea, this.dragTimer.Elapsed.Seconds / this.preDeploymentSpeed);
                }
                return this.DeployedArea;
            }
        }

        //The current convective coefficient
        private double ConvectiveCoefficient => UtilMath.LerpUnclamped(this.ConvectiveCoefficientNewtonian, this.ConvectiveCoefficientMach, this.fi.convectiveMachLerp) * this.Vessel.mainBody.convectionMultiplier;

        //Newtonian convective coefficient
        private double ConvectiveCoefficientNewtonian => (this.fi.density > 1.0 ?
                                                            this.fi.density :
                                                            Math.Pow(this.fi.density, PhysicsGlobals.NewtonianDensityExponent))
                                                       * (PhysicsGlobals.NewtonianConvectionFactorBase + Math.Pow(this.fi.spd, PhysicsGlobals.NewtonianVelocityExponent)) * PhysicsGlobals.NewtonianConvectionFactorTotal;

        //Mach convective coefficient
        private double ConvectiveCoefficientMach => (this.fi.density > 1.0 ?
                                                        this.fi.density :
                                                        Math.Pow(this.fi.density, PhysicsGlobals.MachConvectionDensityExponent))
                                                  * Math.Pow(this.fi.spd, PhysicsGlobals.MachConvectionVelocityExponent) * PhysicsGlobals.MachConvectionFactor * 1E-07;

        //Part this chute is associated with
        private Part Part => this.module.part;

        //Vessel this chute is associated with
        private Vessel Vessel => this.module.vessel;

        //Position to apply the force to
        public Vector3 ForcePosition => this.parachute.position;

        //If the random deployment timer has been spent
        public bool RandomDeployment
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
        public bool DeploymentClause
        {
            get
            {
                if (this.minIsPressure) { return this.module.atmPressure >= this.minPressure; }
                return this.minDeployment == 0 ? this.Part.vessel.LandedOrSplashed : this.module.trueAlt <= this.minDeployment;
            }
        }

        //If the parachute can deploy
        public bool CanDeploy
        {
            get
            {
                if (this.module.GroundStop || this.module.atmPressure == 0 || this.Part.ShieldedFromAirstream) { return false; }
                if (this.DeploymentState == DeploymentStates.CUT) { return false; }
                if (this.DeploymentClause)
                {
                    if (this.cutAlt == -1) { return true; }
                    if (this.module.trueAlt > this.cutAlt) { return true; }
                }
                else if (this.module.SecondaryChute && this.Parachutes.Exists(p => this.module.trueAlt <= p.cutAlt)) { return true; }
                else if (this.IsDeployed) { return true; }
                return false;
            }
        }

        //If the parachute is deployed
        public bool IsDeployed
        {
            get
            {
                switch (this.DeploymentState)
                {
                    case DeploymentStates.PREDEPLOYED:
                    case DeploymentStates.LOWDEPLOYED:
                    case DeploymentStates.DEPLOYED:
                        return true;

                    default:
                        return false;
                }
            }
        }

        //The added vector to drag to angle the parachute
        private bool check = true;
        private Vector3 ForcedVector
        {
            get
            {
                if (this.check)
                {
                    if (this.forcedOrientation >= 90 || this.forcedOrientation <= 0) { this.forced = Vector3.zero; }
                    Vector3 follow = this.ForcePosition - this.module.pos;
                    float length = Mathf.Tan(this.forcedOrientation * Mathf.Deg2Rad);
                    this.forced = follow.normalized * length;
                    this.check = false;
                }
                return this.forced;
            }
        }

        //The parachutes of the associated module
        public List<Parachute> Parachutes => this.module.parachutes;

        //Gets/sets the DeploymentState correctly
        public DeploymentStates DeploymentState
        {
            get
            {
                if (this.state == DeploymentStates.NONE) { this.state = EnumUtils.GetValue<DeploymentStates>(this.depState); }
                return this.state;
            }
            set
            {
                this.state = value;
                this.depState = EnumUtils.GetName(this.state);
            }
        }
        #endregion

        #region Fields
        //Parachute
        public string material = "Nylon";
        public float preDeployedDiameter = 1, deployedDiameter = 25;
        public bool minIsPressure, capOff;
        public float minDeployment = 25000, minPressure = 0.01f;
        public float deploymentAlt = 700, cutAlt = -1;
        public float preDeploymentSpeed = 2, deploymentSpeed = 6;
        public double time;
        public string preDeploymentAnimation = "semiDeploy", deploymentAnimation = "fullyDeploy";
        public string parachuteName = "parachute", capName = "cap", baseParachuteName = string.Empty;
        public float forcedOrientation, maxRotation = 90f;
        public string depState = "STOWED";
        public double currentArea, chuteTemperature = 300, thermMass;
        private double convectiveFlux;
        private SafeState safeState = SafeState.SAFE;

        //Flight
        internal RealChuteModule module;
        internal bool secondary = false;
        internal Transform parachute, cap;
        private Rigidbody rigidbody;
        private FlightIntegrator fi;
        internal MaterialDefinition mat = new MaterialDefinition();
        internal Vector3 phase = Vector3.zero;
        private Quaternion? lastRotation;
        internal bool played;
        internal PhysicsWatch randomTimer = new PhysicsWatch(), dragTimer = new PhysicsWatch();
        private readonly Random random = new Random();
        internal DeploymentStates state = DeploymentStates.NONE;
        internal float randomX, randomY, randomTime;
        private Vector3 forced;
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
            this.parachute.Rotate(new Vector3(5 * (Mathf.PerlinNoise(Time.time, this.randomX + Mathf.Sin(Time.time)) - 0.5f), 5 * (Mathf.PerlinNoise(Time.time, this.randomY + Mathf.Sin(Time.time)) - 0.5f), 0));
        }

        //Lerps the drag vector between upright and the forced angle
        private Vector3 LerpDrag(Vector3 to)
        {
            if (this.phase.magnitude < to.magnitude - 0.01f || this.phase.magnitude > to.magnitude + 0.01f) { this.phase = Vector3.Lerp(this.phase, to, 0.01f); }
            else { this.phase = to; }
            return this.phase;
        }

        //Makes the canopy follow drag direction
        private void FollowDragDirection()
        {
            //Smooths the forced vector
            Vector3 orient = Vector3.zero;
            if (this.module.SecondaryChute) { orient = LerpDrag(this.module.ManyDeployed ? this.ForcedVector : Vector3.zero); }

            Vector3 follow = this.module.dragVector + orient;
            if (follow.sqrMagnitude > 0)
            {
                Quaternion drag = this.module.reverseOrientation ? Quaternion.LookRotation(-follow.normalized, this.parachute.up) : Quaternion.LookRotation(follow.normalized, this.parachute.up);
                this.parachute.rotation = drag;
            }
            ParachuteNoise();

            if (this.lastRotation != null && this.maxRotation > 0f)
            {
                this.parachute.rotation = Quaternion.RotateTowards(this.lastRotation.Value, this.parachute.rotation, this.maxRotation * Time.fixedDeltaTime);
                this.lastRotation = this.parachute.rotation;
            }
        }

        //Parachute low deployment
        public void LowDeploy()
        {
            this.Part.stackIcon.SetIconColor(XKCDColors.RadioactiveGreen);
            this.capOff = true;
            if (RealChuteSettings.Instance.ActivateNyan) { this.Part.Effect("nyan", 1); }
            else { this.Part.Effect("rcdeploy"); }
            this.DeploymentState = DeploymentStates.LOWDEPLOYED;
            this.parachute.gameObject.SetActive(true);
            this.cap.gameObject.SetActive(false);
            if (this.dragTimer.ElapsedMilliseconds != 0) { this.Part.SkipToAnimationEnd(this.deploymentAnimation); this.played = true; }
            else { this.Part.PlayAnimation(this.preDeploymentAnimation, 1f / this.preDeploymentSpeed); }
            this.dragTimer.Start();
        }

        //Parachute predeployment
        public void PreDeploy()
        {
            this.Part.stackIcon.SetIconColor(XKCDColors.BrightYellow);
            this.capOff = true;
            if (RealChuteSettings.Instance.ActivateNyan) { this.Part.Effect("nyan", 1); }
            else { this.Part.Effect("rcpredeploy"); }
            this.DeploymentState = DeploymentStates.PREDEPLOYED;
            this.parachute.gameObject.SetActive(true);
            this.cap.gameObject.SetActive(false);
            if (this.dragTimer.ElapsedMilliseconds != 0) { this.Part.SkipToAnimationEnd(this.preDeploymentAnimation); }
            else { this.Part.PlayAnimation(this.preDeploymentAnimation, 1f / this.preDeploymentSpeed); }
            this.dragTimer.Start();
        }

        //Parachute deployment
        public void Deploy()
        {
            this.Part.stackIcon.SetIconColor(XKCDColors.RadioactiveGreen);
            if (!RealChuteSettings.Instance.ActivateNyan) { this.Part.Effect("rcdeploy"); }
            this.DeploymentState = DeploymentStates.DEPLOYED;
            if (!this.Part.CheckAnimationPlaying(this.preDeploymentAnimation))
            {
                this.dragTimer.Restart();
                this.Part.PlayAnimation(this.deploymentAnimation, 1f / this.deploymentSpeed);
                this.played = true;
            }
            else { this.played = false; }
        }

        //Parachute cutting
        public void Cut()
        {
            if (RealChuteSettings.Instance.ActivateNyan) { this.Part.Effect("nyan", 0); }
            else { this.Part.Effect("rccut"); }
            this.DeploymentState = DeploymentStates.CUT;
            this.parachute.gameObject.SetActive(false);
            this.played = false;
            if (!this.module.SecondaryChute || this.Parachutes.TrueForAll(p => p.DeploymentState == DeploymentStates.CUT)) { this.module.SetRepack(); }
            else if (this.module.SecondaryChute && this.Parachutes.Exists(p => p.DeploymentState == DeploymentStates.STOWED)) { this.module.armed = true; }
            this.dragTimer.Reset();
            this.currentArea = 0;
            this.chuteTemperature = RCUtils.StartTemp;
        }

        //Repack actions
        public void Repack()
        {
            this.DeploymentState = DeploymentStates.STOWED;
            this.randomTimer.Reset();
            this.randomTime = (float)this.random.NextDouble();
            this.dragTimer.Reset();
            this.time = 0;
            this.capOff = false;
            this.cap.gameObject.SetActive(true);
            this.lastRotation = null;
        }

        //Calculates parachute deployed area
        private float DragDeployment(float time, float debutArea, float endArea)
        {
            if (!this.dragTimer.IsRunning) { this.dragTimer.Start(); }

            this.time = this.dragTimer.Elapsed.TotalSeconds;
            if (this.time <= time)
            {
                float deploymentTime = (float)(Math.Exp(this.time - time) * (this.time / time));
                this.currentArea =  Mathf.Lerp(debutArea, endArea, deploymentTime);
                return (float)this.currentArea;
            }
            this.currentArea = endArea;
            return (float)this.currentArea;
        }

        //Drag force vector
        private Vector3 DragForce(float startArea, float targetArea, float time)
        {
            return this.module.DragCalculation(DragDeployment(time, startArea, targetArea), this.mat.DragCoefficient) * this.module.dragVector * (RealChuteSettings.Instance.JokeActivated ? -1 : 1);
        }

        //Parachute function
        internal void UpdateParachute()
        {
            if (this.CanDeploy)
            {
                this.module.oneWasDeployed = true;
                if (this.IsDeployed)
                {
                    if (!CalculateChuteTemp()) { return; }
                    FollowDragDirection();
                }

                this.Part.GetComponentCached(ref this.rigidbody);
                switch (this.DeploymentState)
                {
                    case DeploymentStates.STOWED:
                        {
                            this.Part.stackIcon.SetIconColor(XKCDColors.LightCyan);
                            if (this.module.trueAlt > this.deploymentAlt && this.DeploymentClause && this.RandomDeployment) { PreDeploy(); }
                            else if (this.module.trueAlt <= this.deploymentAlt && this.RandomDeployment) { LowDeploy(); }
                            break;
                        }

                    case DeploymentStates.PREDEPLOYED:
                        {
                            this.Part.AddForceAtPosition(DragForce(0, this.PreDeployedArea, this.preDeploymentSpeed), this.ForcePosition);
                            if (this.module.trueAlt <= this.deploymentAlt) { Deploy(); }
                            break;
                        }
                    case DeploymentStates.LOWDEPLOYED:
                        {
                            this.Part.AddForceAtPosition(DragForce(0, this.DeployedArea, this.preDeploymentSpeed + this.deploymentSpeed), this.ForcePosition);
                            if (!this.Part.CheckAnimationPlaying(this.preDeploymentAnimation) && !this.played)
                            {
                                this.dragTimer.Restart();
                                this.Part.PlayAnimation(this.deploymentAnimation, 1f / this.deploymentSpeed);
                                this.played = true;
                            }
                            break;
                        }

                    case DeploymentStates.DEPLOYED:
                        {
                            this.Part.AddForceAtPosition(DragForce(this.PreDeployedArea, this.DeployedArea, this.deploymentSpeed), this.ForcePosition);
                            if (!this.Part.CheckAnimationPlaying(this.preDeploymentAnimation) && !this.played)
                            {
                                this.dragTimer.Restart();
                                this.Part.PlayAnimation(this.deploymentAnimation, 1f / this.deploymentSpeed);
                                this.played = true;
                            }
                            break;
                        }
                }
            }
            //Deactivation
            else if (!this.CanDeploy && this.IsDeployed) { Cut(); }
        }

        //Gets convective flux
        //Thanks to Starwaster for an overheating fix here
        public void CalculateConvectiveFlux()
        {
            this.convectiveFlux = this.ConvectiveCoefficient * UtilMath.Lerp(1d, 1d + (Math.Sqrt(this.Vessel.mach * this.Vessel.mach * this.Vessel.mach)* (this.Vessel.dynamicPressurekPa / 101.325)),
                (this.Vessel.mach - PhysicsGlobals.FullToCrossSectionLerpStart) / PhysicsGlobals.FullToCrossSectionLerpEnd)
                * (this.Vessel.externalTemperature - this.chuteTemperature);
        }

        //Calculates the temperature of the chute and cuts it if needed. Big thanks to NathanKell
        private bool CalculateChuteTemp()
        {
            if (this.chuteTemperature < PhysicsGlobals.SpaceTemperature) { this.chuteTemperature = RCUtils.StartTemp; }

            double emissiveFlux = 0;
            if (this.chuteTemperature > 0)
            {
                double temp2 = this.chuteTemperature * this.chuteTemperature;
                emissiveFlux = 2 * PhysicsGlobals.StefanBoltzmanConstant * this.mat.Emissivity * PhysicsGlobals.RadiationFactor * temp2 * temp2;
            }

            this.chuteTemperature = Math.Max(PhysicsGlobals.SpaceTemperature,
                this.chuteTemperature + ((this.convectiveFlux - emissiveFlux) * this.InvThermalMass * this.ConvectionArea * TimeWarp.fixedDeltaTime * 0.001));
            if (this.chuteTemperature > this.mat.MaxTemp)
            {
                ScreenMessages.PostScreenMessage("<color=orange>[RealChute]: " + this.Part.partInfo.title + "'s parachute has been destroyed due to aero forces and heat.</color>", 6f, ScreenMessageStyle.UPPER_LEFT);
                Cut();
                return false;
            }
            return true;
        }

        //Returns if the parachute can safely open or not
        public SafeState GetSafeState()
        {
            if (this.Vessel.externalTemperature <= this.mat.MaxTemp || this.convectiveFlux < 0) { this.safeState = SafeState.SAFE; }
            else
            {
                this.safeState = this.chuteTemperature + (0.00035 * this.InvThermalMass * this.convectiveFlux * this.DeployedArea) <= this.mat.MaxTemp ? SafeState.RISKY : SafeState.DANGEROUS;
            }
            return this.safeState;
        }

        //Initializes the chute
        public void Initialize()
        {
            MaterialsLibrary.Instance.TryGetMaterial(this.material, ref this.mat);

            //I know this seems random, but trust me, it's needed, else some parachutes don't animate, because fuck you, that's why.
            Animation anim = this.Part.FindModelAnimators(this.capName).FirstOrDefault();

            this.cap = this.Part.FindModelTransform(this.capName);
            this.parachute = this.Part.FindModelTransform(this.parachuteName);
            if (this.parachute == null && !string.IsNullOrEmpty(this.baseParachuteName))
            {
                this.parachute = this.Part.FindModelTransform(this.baseParachuteName);
            }
            this.parachute.gameObject.SetActive(true);
            this.Part.InitiateAnimation(this.preDeploymentAnimation);
            this.Part.InitiateAnimation(this.deploymentAnimation);
            this.parachute.gameObject.SetActive(false);

            if (string.IsNullOrEmpty(this.baseParachuteName)) { this.baseParachuteName = this.parachuteName; }

            if (!this.module.initiated)
            {
                this.played = false;
                this.cap.gameObject.SetActive(true);
            }
            if (HighLogic.LoadedSceneIsFlight)
            {
                this.fi = this.Vessel.FindVesselModuleImplementing<FlightIntegrator>();
                this.randomX = (float)((this.random.NextDouble() - 0.5) * 200);
                this.randomY = (float)((this.random.NextDouble() - 0.5) * 200);
                this.randomTime = (float)this.random.NextDouble();
                if (this.time != 0) { this.dragTimer = new PhysicsWatch(this.time); }
                if (this.capOff)
                {
                    this.Part.stackIcon.SetIconColor(XKCDColors.Red);
                    this.cap.gameObject.SetActive(false);
                }

                if (this.module.staged && this.DeploymentState != DeploymentStates.CUT)
                {
                    this.DeploymentState = DeploymentStates.STOWED;
                }
            }
        }
        #endregion

        #region GUI
        //Info window GUI
        internal void RenderGUI()
        {
            //Initial label
            StringBuilder builder = new StringBuilder();
            builder.Append("Material: ").AppendLine(this.mat.Name);
            builder.Append("Drag coefficient: ").AppendLine(this.mat.DragCoefficient.ToString("0.00#"));
            builder.Append("Predeployed diameter: ").Append(this.preDeployedDiameter).Append("m\n    area: ").Append(this.PreDeployedArea.ToString("0.###")).AppendLine("m²");
            builder.Append("Deployed diameter: ").Append(this.deployedDiameter).Append("m\n    area: ").Append(this.DeployedArea.ToString("0.###")).Append("m²");
            GUILayout.Label(builder.ToString());

            if (HighLogic.LoadedSceneIsFlight)
            {
                //DeploymentSafety
                switch (this.safeState)
                {
                    case SafeState.SAFE:
                        GUILayout.Label("Deployment safety: safe"); break;

                    case SafeState.RISKY:
                        GUILayout.Label("Deployment safety: risky", GuiUtils.YellowLabel); break;

                    case SafeState.DANGEROUS:
                        GUILayout.Label("Deployment safety: dangerous", GuiUtils.RedLabel); break;
                }

                //Temperature info
                builder = new StringBuilder();
                builder.Append("Chute max temperature: ").Append(this.mat.MaxTemp + RCUtils.AbsoluteZero).AppendLine("°C");
                builder.Append("Current chute temperature: ").Append(Math.Round(this.chuteTemperature + RCUtils.AbsoluteZero, 1, MidpointRounding.AwayFromZero)).Append("°C");
                GUILayout.Label(builder.ToString(), this.chuteTemperature / this.mat.MaxTemp > 0.85 ? GuiUtils.RedLabel : GUI.skin.label);


                //Pressure/altitude predeployment toggle
                GUILayout.BeginHorizontal();
                GUILayout.Label("Predeployment:");
                if (GUILayout.Toggle(!this.minIsPressure, "altitude")) { this.minIsPressure = false; }
                GUILayout.FlexibleSpace();
                if (GUILayout.Toggle(this.minIsPressure, "pressure")) { this.minIsPressure = true; }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            //Predeployment pressure selection
            if (this.minIsPressure)
            {
                GUILayout.Label("Predeployment pressure: " + this.minPressure + "atm");
                if (HighLogic.LoadedSceneIsFlight)
                {
                    //Predeployment pressure slider
                    this.minPressure = 0.005f * (float) Math.Round(GUILayout.HorizontalSlider(this.minPressure / 0.005f, 1, 200));

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
                GUILayout.Label("Predeployment altitude: " + this.minDeployment + "m");
                if (HighLogic.LoadedSceneIsFlight)
                {
                    //Predeployment altitude slider
                    this.minDeployment = 100 * (float) Math.Round(GUILayout.HorizontalSlider(this.minDeployment / 100, 1, 200));

                    //Copy to symmetry counterparts button
                    CopyToOthers(p =>
                    {
                        p.minIsPressure = this.minIsPressure;
                        p.minDeployment = this.minDeployment;
                    });
                }
            }

            //Deployment altitude selection
            GUILayout.Label("Deployment altitude: " + this.deploymentAlt + "m");
            if (HighLogic.LoadedSceneIsFlight)
            {
                //Deployment altitude slider
                this.deploymentAlt = 50 * (float) Math.Round(GUILayout.HorizontalSlider(this.deploymentAlt / 50, 1, 200));

                //Copy to symmetry counterparts button
                CopyToOthers(p => p.deploymentAlt = this.deploymentAlt);
            }

            //Other labels
            builder = new StringBuilder();
            if (this.cutAlt > 0) { builder.Append("Autocut altitude: ").Append(this.cutAlt).AppendLine("m"); }
            builder.Append("Predeployment speed: ").Append(this.preDeploymentSpeed).AppendLine("s");
            builder.Append("Deployment speed: ").Append(this.deploymentSpeed).Append("s");
            GUILayout.Label(builder.ToString());
        }

        //Copies the given values to the other parachutes
        private void CopyToOthers(Callback<Parachute> callback)
        {
            if (this.module.SecondaryChute)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Copy to symmetry counterparts", GUILayout.Height(25), GUILayout.Width(250)))
                {
                    foreach (Parachute p in this.Parachutes)
                    {
                        if (p == this) { continue; }
                        callback(p);
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
            node.TryGetValue("capOff", ref this.capOff);
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
            node.TryGetValue("maxRotation", ref this.maxRotation);
            node.TryGetValue("depState", ref this.depState);
            MaterialsLibrary.Instance.TryGetMaterial(this.material, ref this.mat);
            Transform p = this.Part.FindModelTransform(this.parachuteName);
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
            node.AddValue("capOff", this.capOff);
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
            node.AddValue("maxRotation", this.maxRotation);
            node.AddValue("depState", this.depState);
            return node;
        }
        #endregion
    }
}