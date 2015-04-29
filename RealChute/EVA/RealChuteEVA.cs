using System;
using UnityEngine;
using RealChute.Extensions;
using RealChute.Utils;
using RealChute.Libraries.Materials;
using Random = System.Random;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute.EVA
{
    public class RealChuteEVA : PartModule
    {
        #region KSPFields
        [KSPField(isPersistant = true)]
        public string moduleID = string.Empty, moduleDescription = string.Empty;
        [KSPField(isPersistant = true)]
        public string parachuteURL = string.Empty, backpackURL = string.Empty;
        [KSPField(isPersistant = true)]
        public string parachuteName = string.Empty, pilotName = string.Empty;
        [KSPField(isPersistant = true)]
        public string backpackAnchorName = string.Empty, backpackCapName;
        [KSPField(isPersistant = true)]
        public string animationName = string.Empty;
        [KSPField(isPersistant = true)]
        public float deploymentSpeed = 4, time = 0;
        [KSPField(isPersistant = true)]
        public float deployedDiameter = 10, minDeploymentAlt = 5000;
        [KSPField(isPersistant = true)]
        public bool initiated = false, staged = false, armed = false;
        [KSPField(isPersistant = true)]
        public string material = "Nylon";
        [KSPField(isPersistant = true)]
        public string depState = "STOWED";
        [KSPField(isPersistant = true, guiActive = true, guiName = "Status")]
        public string status = "Stowed";
        #endregion

        #region Properties
        //Parachute deployed area
        public float deployedArea
        {
            get { return RCUtils.GetArea(this.deployedDiameter); }
        }

        //Mass of the chute
        public float chuteMass
        {
            get { return this.deployedArea * this.mat.areaDensity; }
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

        //If the parachute can deploy
        public bool canDeploy
        {
            get
            {
                if (this.vessel.LandedOrSplashed || this.atmDensity == 0) { return false; }
                else if (this.deploymentState == DeploymentStates.CUT) { return false; }
                else if (this.trueAlt <= this.minDeploymentAlt && this.deploymentState == DeploymentStates.DEPLOYED) { return true; }
                return false;
            }
        }

        //Position to apply the force to
        public Vector3 forcePosition
        {
            get { return this.parachute.position; }
        }

        //Quick access to the part GUI events
        private BaseEvent _deploy = null, _arm = null, _disarm = null, _cut = null, _dispose = null;
        private BaseEvent deploy
        {
            get
            {
                if (this._deploy == null) { this._deploy = Events["GUIDeploy"]; }
                return this._deploy;
            }
        }
        private BaseEvent arm
        {
            get
            {
                if (this._arm == null) { this._arm = Events["GUIArm"]; }
                return this._arm;
            }
        }
        private BaseEvent disarm
        {
            get
            {
                if (this._disarm == null) { this._disarm = Events["GUIDisarm"]; }
                return this._disarm;
            }
        }
        private BaseEvent cut
        {
            get
            {
                if (this._cut == null) { this._cut = Events["GUICut"]; }
                return this._cut;
            }
        }
        private BaseEvent dispose
        {
            get
            {
                if (this._dispose == null) { this._dispose = Events["GUIRepack"]; }
                return this._dispose;
            }
        }
        #endregion

        #region Fields
        public Transform parachute = null, pilot = null;
        private Transform backpackAnchor = null, backpackCap = null;
        private MaterialsLibrary materials = MaterialsLibrary.instance;
        private MaterialDefinition mat = new MaterialDefinition();
        public DeploymentStates deploymentState = DeploymentStates.STOWED;
        private PhysicsWatch dragTimer = new PhysicsWatch(), randomTimer = new PhysicsWatch();
        public Vector3 dragVector = new Vector3();
        internal double ASL, trueAlt, atmDensity;
        internal float sqrSpeed;
        internal float randomX, randomY, randomTime;
        internal bool randomized = false;
        private Transform[] jetpack = new Transform[0];
        private RealChuteSettings settings = null;
        #endregion

        #region Part GUI
        [KSPEvent(guiActive = true, active = true, guiActiveUnfocused = false, guiName = "Deploy chute")]
        public void GUIDeploy()
        {
            ActivateChute();
        }

        [KSPEvent(guiActive = true, active = true, guiActiveUnfocused = false, guiName = "Arm chute")]
        public void GUIArm()
        {
            this.armed = true;
            ActivateChute();
            this.arm.active = false;
            this.deploy.active = false;
            this.disarm.active = true;
        }

        [KSPEvent(guiActive = true, active = true, guiActiveUnfocused = false, guiName = "Disarm chute")]
        public void GUIDisarm()
        {
            this.armed = false;
            DeactivateChute();
            this.arm.active = true;
            this.deploy.active = false;
            this.disarm.active = false;
        }

        [KSPEvent(guiActive = true, active = true, guiActiveUnfocused = false, guiName = "Cut chute")]
        public void GUICut()
        {
            this.part.Effect("evacut");
            this.parachute.gameObject.SetActive(false);
            this.deploymentState = DeploymentStates.CUT;
            this.cut.active = false;
            this.dispose.active = true;
            this.dragTimer.Reset();
            this.randomTimer.Reset();
            DeactivateChute();
        }

        [KSPEvent(guiActive = true, active = false, guiActiveUnfocused = false, guiName = "Dispose chute")]
        public void GUIDisposeChute()
        {
            if (this.backpackAnchor != null)
            {
                GameObject.Destroy(this.backpackAnchor.parent.gameObject);
            }
            else { GameObject.Destroy(this.parachute.gameObject); }

            ShowJetpack();
            this.part.RemoveModule(this);
        }
        #endregion

        #region Methods
        //Activates the parachute
        public void ActivateChute()
        {
            this.staged = true;
            if (this.settings.autoArm) { this.armed = true; }
            print("[RealChute]: " + this.part.protoModuleCrew[0].name + "'s EVA chute was activated");
        }

        //Deactiates the parachute
        public void DeactivateChute()
        {
            this.staged = false;
            print("[RealChute]: " + this.part.protoModuleCrew[0].name + "'s EVA chute was deactivated");
        }

        //Tries to add the parachute to the Kerbal model
        private bool InitiateParachute()
        {
            if (string.IsNullOrEmpty(this.parachuteURL))
            {
                Debug.LogError("[RealChute] EVA parachute URL is null");
                return false;
            }
            if (string.IsNullOrEmpty(this.parachuteName))
            {
                Debug.LogError("[RealChute] EVA parachute transform name is null");
                return false;
            }

            GameDatabase gamedata = GameDatabase.Instance;
            GameObject test = gamedata.GetModel(this.parachuteURL);
            if (test == null)
            {
                Debug.LogError("[RealChute]: Could not find EVA canopy model at " + this.parachuteURL);
                return false;
            }

            test.SetActive(true);
            GameObject chute = GameObject.Instantiate(test) as GameObject, backpack = null;
            GameObject.Destroy(test);
            chute.SetActive(true);
            if (!chute.GetComponentsInChildren<Transform>().Exists(t => t.name == this.parachuteName))
            {
                Debug.LogError("[RealChute]: Could not find parachute canopy transform \"" + this.parachuteName + "in parachute model");
                return false;
            }
            Transform c = chute.transform, parent = this.part.transform;

            if (!string.IsNullOrEmpty(this.backpackURL) || !string.IsNullOrEmpty(this.backpackAnchorName) || !string.IsNullOrEmpty(this.backpackCapName))
            {
                GameObject b = gamedata.GetModel(this.backpackURL);
                if (b == null)
                {
                    Debug.LogError("[RealChute]: Could not find EVA backpack model at " + this.backpackURL);
                    return false;
                }

                b.SetActive(true);
                backpack = GameObject.Instantiate(b) as GameObject;
                GameObject.Destroy(b);
                backpack.SetActive(true);
                Transform[] transforms = backpack.GetComponentsInChildren<Transform>();
                if (!transforms.Exists(t => t.name == this.backpackAnchorName))
                {
                    Debug.LogError("[RealChute]: Could not find backpack anchor transform \"" + this.backpackAnchorName + "in backpack model");
                    return false;
                }
                if (!transforms.Exists(t => t.name == this.backpackCapName))
                {
                    Debug.LogError("[RealChute]: Could not find backpack cap transform \"" + this.backpackCapName + "in backpack model");
                    return false;
                }
                HideJetpack();
                Transform back = backpack.transform;
                back.parent = parent.GetChild(5);
                back.position = parent.position;
                back.rotation = parent.rotation;
                this.backpackAnchor = this.part.FindModelTransform(this.backpackAnchorName);
                this.backpackCap = this.part.FindModelTransform(this.backpackCapName);
                c.parent = this.backpackAnchor;
                c.position = this.backpackAnchor.position;
                c.rotation = Quaternion.identity;
                this.backpackCap.gameObject.SetActive(!this.staged);
            }
            else
            {
                c.parent = parent;
                c.position = parent.position;
                c.rotation = Quaternion.identity;       
            }
            this.parachute = this.part.FindModelTransform(this.parachuteName);
            this.pilot = this.part.FindModelTransform(this.pilotName);
            this.parachute.gameObject.SetActive(this.staged);
            return true;
        }

        //Hides the jetpack model
        private void HideJetpack()
        {
            if (this.jetpack.Length <= 0) { GetJetpackTransforms(); }
            this.jetpack.ForEach(t => t.gameObject.SetActive(false));
        }

        //Shows the jetpack model
        private void ShowJetpack()
        {
            if (this.jetpack.Length <= 0) { GetJetpackTransforms(); }
            this.jetpack.ForEach(t => t.gameObject.SetActive(true));
        }

        //Gets the jetpack transforms to hide
        private void GetJetpackTransforms()
        {
            this.jetpack = new Transform[5];
            //General jetpack transform
            Transform j = this.part.transform.GetChild(5).GetChild(3);
            //Flag decals
            this.jetpack[0] = this.part.transform.GetChild(1).GetChild(0);
            //Jetpack base
            this.jetpack[1] = j.GetChild(2);
            //Fuel tank 1
            this.jetpack[2] = j.GetChild(3);
            //Fuel tank 2
            this.jetpack[3] = j.GetChild(4);
            //Thrusters
            this.jetpack[4] = j.GetChild(5);
        }

        //Gives a random movement to the parachute
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
            if (this.pilot != null) { this.pilot.Rotate(new Vector3(5 * (Mathf.PerlinNoise(time, this.randomY + Mathf.Sin(time)) - 0.5f), 5 * (Mathf.PerlinNoise(time, this.randomX + Mathf.Sin(time)) - 0.5f), 0)); }
        }

        //Makes the parachute follow drag direction
        private void FollowDragDirection()
        {
            if (this.dragVector.sqrMagnitude > 0)
            {
                this.parachute.rotation = Quaternion.LookRotation(-this.dragVector, this.parachute.up);
            }
            ParachuteNoise();
        }

        //Deploys the parachute
        public void Deploy()
        {
            this.part.Effect("evadeploy");
            this.parachute.gameObject.SetActive(true);
            this.deploymentState = DeploymentStates.DEPLOYED;
            if (this.dragTimer.elapsedMilliseconds > 0)
            {
                if (this.dragTimer.elapsed.TotalSeconds < this.deploymentSpeed)
                {
                    this.part.SkipToAnimationTime(this.animationName, 1f / this.deploymentSpeed, (float)this.dragTimer.elapsed.TotalSeconds / this.deploymentSpeed);
                }
                else
                {
                    this.part.SkipToAnimationEnd(this.animationName);
                }
            }
            else
            {
                this.part.PlayAnimation(this.animationName, 1f / this.deploymentSpeed);
            }
            this.dragTimer.Start();
            if (this.backpackCap != null)
            {
                this.backpackCap.gameObject.SetActive(false);
            }
            this.deploy.active = false;
            this.cut.active = true;
        }

        //Smoothly opens the parachute over time
        private float DragDeployment(float time, float debutArea, float endArea)
        {
            if (!this.dragTimer.isRunning) { this.dragTimer.Start(); }

            this.time = (float)this.dragTimer.elapsed.TotalSeconds;
            if (this.time <= time)
            {
                float deploymentTime = Mathf.Exp(this.time - time) * (this.time / time);
                return Mathf.Lerp(debutArea, endArea, deploymentTime);
            }
            return endArea;
        }

        //Drag formula calculations
        public float DragCalculation(float area, float Cd)
        {
            return (float)this.atmDensity * this.sqrSpeed * Cd * area / 2000f;
        }

        //Drag force vector
        private Vector3 DragForce(float startArea, float targetArea, float time)
        {
            return DragCalculation(DragDeployment(time, startArea, targetArea), this.mat.dragCoefficient) * this.dragVector * (this.settings.jokeActivated ? -1 : 1);
        }

        public ConfigNode Save()
        {
            ConfigNode node = new ConfigNode("EVA");
            node.AddValue("moduleID", this.moduleID);
            node.AddValue("moduleDescription", this.moduleDescription);
            return node;
        }
        #endregion

        #region Functions
        private void FixedUpdate()
        {

            if (!CompatibilityChecker.IsAllCompatible() || FlightGlobals.ActiveVessel == null || this.part.Rigidbody == null) { return; }
            this.ASL = FlightGlobals.getAltitudeAtPos(this.part.transform.position);
            this.trueAlt = this.vessel.GetTrueAlt(ASL);
            this.atmDensity = this.vessel.mainBody.GetDensityAtAlt(ASL, this.vessel.atmosphericTemperature);
            Vector3 velocity = this.part.Rigidbody.velocity + Krakensbane.GetFrameVelocityV3f();
            this.sqrSpeed = velocity.sqrMagnitude;
            this.dragVector = -velocity.normalized;
            
            if (this.staged)
            {
                if (this.armed)
                {
                    if (this.canDeploy && this.randomDeployment) { this.armed = false; }
                }
                else
                {
                    if (this.canDeploy)
                    {
                        switch (this.deploymentState)
                        {
                            case DeploymentStates.STOWED:
                                {
                                    if (this.trueAlt < this.minDeploymentAlt) { Deploy(); }
                                    break;
                                }

                            case DeploymentStates.DEPLOYED:
                                {
                                    this.part.Rigidbody.AddForceAtPosition(DragForce(0, this.deployedArea, this.deploymentSpeed), this.forcePosition, ForceMode.Force);
                                    break;
                                }

                            default:
                                break;
                        }
                    }
                    else if (this.deploymentState == DeploymentStates.DEPLOYED) { GUICut(); }
                    else { DeactivateChute(); }
                }
            }
        }
        #endregion

        #region Overrides
        public override void OnStart(PartModule.StartState state)
        {
            if (!CompatibilityChecker.IsAllCompatible() || !InitiateParachute())
            {
                Fields["status"].guiActive = false;
                Events.ForEach(e => e.guiActive = false);
                return;
            }
            materials.TryGetMaterial(this.material, ref this.mat);

            //0.09t is the mass of a Kerbal, prevents accidental mass increase
            this.part.mass = 0.09f + this.chuteMass;
            this.settings = RealChuteSettings.fetch;
            this.randomTime = (float)(new Random().NextDouble());
        }

        public override void OnLoad(ConfigNode node)
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }

            ConfigNode effects = new ConfigNode();
            if (node.TryGetNode("EFFECTS", ref effects))
            {
                this.part.LoadEffects(effects);
            }
            this.deploymentState = EnumUtils.GetValue<DeploymentStates>(this.depState);
        }

        public override void OnSave(ConfigNode node)
        {
            this.depState = EnumUtils.GetName(this.deploymentState);
        }
        #endregion
    }
}
