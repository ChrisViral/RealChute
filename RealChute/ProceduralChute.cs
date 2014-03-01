using System;
using System.Collections.Generic;
using System.Linq;
using RealChute.Extensions;
using UnityEngine;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more informtion contact me on the forum. */

namespace RealChute
{
    public class ProceduralChute : PartModule
    {
        #region Config values
        [KSPField]
        public string textureLibrary = "none";
        [KSPField]
        public string type = string.Empty;
        [KSPField]
        public string currentCase = string.Empty;
        [KSPField]
        public string currentCanopy = string.Empty;
        [KSPField]
        public string secCurentCanopy = string.Empty;
        #endregion

        #region Persistent values
        [KSPField(isPersistant = true)]
        public int caseID = 0, chuteID = 0, modelID = 0;
        [KSPField(isPersistant = true)]
        public int secChuteID = 0, secModelID = 0;
        [KSPField(isPersistant = true)]
        public int size = 0, planets = 0;
        [KSPField(isPersistant = true)]
        public int lastCaseID = 0, lastChuteID = 0, lastModelID = 0;
        [KSPField(isPersistant = true)]
        public int lastSecChuteID = 0, lastSecModelID = 0;
        [KSPField(isPersistant = true)]
        public int materialsID = 0, secMaterialsID = 0;
        [KSPField(isPersistant = true)]
        public int typeID = 0, secTypeID = 0;
        [KSPField(isPersistant = true)]
        public int lastTypeID = 0, lastSecTypeID = 0;
        [KSPField(isPersistant = true)]
        public Vector3 currentSize = new Vector3(), lastSize = new Vector3(), originalSize = new Vector3();
        [KSPField(isPersistant = true)]
        public Vector3 position = Vector3.zero, secPosition = Vector3.zero;
        [KSPField(isPersistant = true)]
        public float top = 0, bottom = 0, debut = 0;
        [KSPField(isPersistant = true)]
        public bool initiated = false;
        [KSPField(isPersistant = true)]
        private bool mustGoDown = false, isPressure = false, secIsPressure = false;
        [KSPField(isPersistant = true)]
        private bool calcSelect = true, secCalcSelect = true;
        [KSPField(isPersistant = true)]
        private bool getMass = true, secGetMass = true;
        [KSPField(isPersistant = true)]
        private bool secondaryChute = false;
        [KSPField(isPersistant = true)]
        public string originalTransform = string.Empty, secOriginalTransform = string.Empty;
        [KSPField(isPersistant = true)]
        public string lastTransform = string.Empty, secLastTransform = string.Empty;
        [KSPField(isPersistant = true)]
        private string timer = string.Empty, cutSpeed = string.Empty, spares = string.Empty;
        [KSPField(isPersistant = true)]
        private string preDepDiam = string.Empty, depDiam = string.Empty, predepClause = string.Empty;
        [KSPField(isPersistant = true)]
        private string secPreDepDiam = string.Empty, secDepDiam = string.Empty, secPredepClause = string.Empty;
        [KSPField(isPersistant = true)]
        private string mass = "10", landingSpeed = "6", deceleration = "10", refDepAlt = "700", chuteCount = "1";
        [KSPField(isPersistant = true)]
        private string secMass = "10", secLandingSpeed = "6", secDeceleration = "10", secRefDepAlt = "700", secChuteCount = "1";
        [KSPField(isPersistant = true)]
        private string deploymentAlt = string.Empty, cutAlt = string.Empty, preDepSpeed = string.Empty, depSpeed = string.Empty;
        [KSPField(isPersistant = true)]
        private string secDeploymentAlt = string.Empty, secCutAlt = string.Empty, secPreDepSpeed = string.Empty, secDepSpeed = string.Empty;
        #endregion

        #region Fields
        private EditorActionGroups actionPanel = EditorActionGroups.Instance;
        private GUISkin skins = HighLogic.Skin;
        private GUIStyle redLabel = new GUIStyle(), boldLabel = new GUIStyle();
        private RealChuteModule rcModule = null;
        private AtmoPlanets bodies = new AtmoPlanets();
        private MaterialsLibrary materials = null;
        private MaterialDefinition material = new MaterialDefinition(), secMaterial = new MaterialDefinition();
        private TextureLibrary textureLib = null;
        private TextureConfig textures = new TextureConfig();
        private CaseConfig parachuteCase = new CaseConfig();
        private CanopyConfig canopy = new CanopyConfig(), secCanopy = new CanopyConfig();
        private ModelConfig model = new ModelConfig(), secModel = new ModelConfig();
        private Rect window = new Rect(), failedWindow = new Rect(), successfulWindow = new Rect();
        private Rect materialsWindow = new Rect(), secMaterialsWindow = new Rect();
        private int mainId = Guid.NewGuid().GetHashCode(), failedId = Guid.NewGuid().GetHashCode(), successId = Guid.NewGuid().GetHashCode();
        private int matId = Guid.NewGuid().GetHashCode(), secMatId = Guid.NewGuid().GetHashCode();
        private int matX = 500, matY = 370;
        private Vector2 mainScroll = new Vector2(), failedScroll = new Vector2();
        private Vector2 parachuteScroll = new Vector2(), secParachuteScroll = new Vector2();
        private Vector2 materialsScroll = new Vector2(), secMaterialsScroll = new Vector2();
        [SerializeField]
        private List<Vector4> vectors = new List<Vector4>();
        private Dictionary<Vector3, float> sizes = new Dictionary<Vector3, float>();
        [SerializeField]
        private Transform parent = null;
        private bool warning = false;
        private bool visible = false, failedVisible = false, successfulVisible = false;
        private bool materialsVisible = false, secMaterialsVisible = false;
        private bool locked = false;
        private string[] cases = new string[] { }, canopies = new string[] { }, models = new string[] { };
        #endregion

        #region Methods
        private string[] TextureEntries(string entries)
        {
            if (textureLibrary == "none") { return new string[] { }; }

            if (entries == "case") { return textures.caseNames.Where(c => textures.GetCase(c).types.Contains(type)).ToArray(); }

            if (entries == "chute") { return textures.canopyNames; }

            if (entries == "model") { return textures.modelNames; }

            return new string[] { };
        }

        private float GetCraftMass()
        {
            return EditorLogic.SortedShipList.Sum(part => part.TotalMass());
        }

        private void CreateEntryArea(string label, ref string value, float min, float max, float width = 150)
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (RCUtils.CanParse(value) && RCUtils.CheckRange(float.Parse(value), min, max)) { GUILayout.Label(label, skins.label); }
            else { GUILayout.Label(label, redLabel); }
            GUILayout.FlexibleSpace();
            value = GUILayout.TextField(value, 10, skins.textField, GUILayout.Width(width));
            GUILayout.EndHorizontal();
        }

        private List<string> GetErrors(string type)
        {
            if (type == "general")
            {
                List<string> general = new List<string>();

                if (!RCUtils.CanParseTime(timer) || !RCUtils.CheckRange(RCUtils.ParseTime(timer), 0, 3600)) { general.Add("Deployment timer"); }
                if (!RCUtils.CanParseWithEmpty(spares) || !RCUtils.CheckRange(RCUtils.ParseWithEmpty(spares), -1, 10) || !RCUtils.IsWholeNumber(RCUtils.ParseWithEmpty(spares))) { general.Add("Spare chutes"); }
                if (!RCUtils.CanParse(cutSpeed) || !RCUtils.CheckRange(float.Parse(cutSpeed), 0.01f, 100)) { general.Add("Autocut speed"); }
                return general;
            }
            else if (type == "main")
            {
                List<string> main = new List<string>();
                if (calcSelect)
                {
                    if (!getMass && !RCUtils.CanParse(mass) || !RCUtils.CheckRange(float.Parse(mass), 0.1f, 10000)) { main.Add("Craft mass"); }
                    if (!RCUtils.CanParse(landingSpeed) || !RCUtils.CheckRange(float.Parse(landingSpeed), 0.1f, 300)) { main.Add("Landing speed"); }
                    if (typeID == 2 && !RCUtils.CanParse(deceleration)  || !RCUtils.CheckRange(float.Parse(deceleration), 0.1f, 100)) { main.Add("Wanted deceleration"); }
                    if (typeID == 1 && !RCUtils.CanParse(refDepAlt) || !RCUtils.CheckRange(float.Parse(refDepAlt), 10, 45000)) { main.Add("Mains planned deployment alt"); }
                    if (!RCUtils.CanParse(chuteCount) || !RCUtils.CheckRange(float.Parse(chuteCount), 1, 100)) { main.Add("Parachute count"); }
                }
                else
                {
                    if (!RCUtils.CanParse(preDepDiam) || !RCUtils.CheckRange(float.Parse(preDepDiam), 0.5f, model.maxDiam / 2)) { main.Add("Predeployed diameter"); }
                    if (!RCUtils.CanParse(depDiam) || !RCUtils.CheckRange(float.Parse(depDiam), 1, model.maxDiam)) { main.Add("Deployed diameter"); }
                }
                if (!RCUtils.CanParse(predepClause) || (isPressure && !RCUtils.CheckRange(float.Parse(predepClause), 0.0001f, 5f)) || (!isPressure && !RCUtils.CheckRange(float.Parse(predepClause), 25, 50000)))
                {
                    if (isPressure) { main.Add("Predeployment pressure"); }
                    else { main.Add("Predeployment altitude"); }
                }
                if (!RCUtils.CanParse(deploymentAlt) || !RCUtils.CheckRange(float.Parse(deploymentAlt), 10, 45000)) { main.Add("Deployment altitude"); }
                if (!RCUtils.CanParseWithEmpty(cutAlt) || !RCUtils.CheckRange(RCUtils.ParseWithEmpty(cutAlt), -1, 45000)) { main.Add("Autocut altitude"); }
                if (!RCUtils.CanParse(preDepSpeed) || !RCUtils.CheckRange(float.Parse(preDepSpeed), 0.5f, 5)) { main.Add("Predeployment speed"); }
                if (!RCUtils.CanParse(depSpeed) || !RCUtils.CheckRange(float.Parse(depSpeed), 1, 10)) { main.Add("Deployment speed"); }
                return main;
            }
            else if (type == "secondary")
            {
                List<string> secondary = new List<string>();
                if (calcSelect)
                {
                    if (!getMass && !RCUtils.CanParse(secMass) || !RCUtils.CheckRange(float.Parse(secMass), 0.1f, 10000)) { secondary.Add("Craft mass"); }
                    if (!RCUtils.CanParse(secLandingSpeed) || !RCUtils.CheckRange(float.Parse(secLandingSpeed), 0.1f, 300)) { secondary.Add("Landing speed"); }
                    if (secTypeID == 2 && !RCUtils.CanParse(secDeceleration) || !RCUtils.CheckRange(float.Parse(secDeceleration), 0.1f, 100)) { secondary.Add("Wanted deceleration"); }
                    if (secTypeID == 1 && !RCUtils.CanParse(secRefDepAlt) || !RCUtils.CheckRange(float.Parse(secRefDepAlt), 10, 45000)) { secondary.Add("Mains planned deployment alt"); }
                    if (!RCUtils.CanParse(secChuteCount) || !RCUtils.CheckRange(float.Parse(secChuteCount), 1, 100)) { secondary.Add("Parachute count"); }
                }
                else
                {
                    if (!RCUtils.CanParse(secPreDepDiam) || !RCUtils.CheckRange(float.Parse(secPreDepDiam), 0.5f, secModel.maxDiam / 2)) { secondary.Add("Predeployed diameter"); }
                    if (!RCUtils.CanParse(secDepDiam) || !RCUtils.CheckRange(float.Parse(secDepDiam), 1, secModel.maxDiam)) { secondary.Add("Deployed diameter"); }
                }
                if (!RCUtils.CanParse(secPredepClause) || (secIsPressure && !RCUtils.CheckRange(float.Parse(secPredepClause), 0.0001f, 5f)) || (!secIsPressure && !RCUtils.CheckRange(float.Parse(secPredepClause), 25, 50000)))
                {
                    if (secIsPressure) { secondary.Add("Predeployment pressure"); }
                    else { secondary.Add("Predeployment altitude"); }
                }
                if (!RCUtils.CanParse(secDeploymentAlt) || !RCUtils.CheckRange(float.Parse(secDeploymentAlt), 10, 45000)) { secondary.Add("Deployment altitude"); }
                if (!RCUtils.CanParseWithEmpty(secCutAlt) || !RCUtils.CheckRange(RCUtils.ParseWithEmpty(secCutAlt), -1, 45000)) { secondary.Add("Autocut altitude"); }
                if (!RCUtils.CanParse(secPreDepSpeed) || !RCUtils.CheckRange(float.Parse(secPreDepSpeed), 0.5f, 5)) { secondary.Add("Predeployment speed"); }
                if (!RCUtils.CanParse(secDepSpeed) || !RCUtils.CheckRange(float.Parse(secDepSpeed), 1, 10)) { secondary.Add("Deployment speed"); }
                return secondary;
            }
            return new List<string>();
        }

        private void CreateErrors()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Invalid parameters:", skins.label);
            GUILayout.Space(10);

            GUILayout.BeginVertical(skins.box);
            if (GetErrors("general").Count != 0)
            {
                GUILayout.Label("General:", skins.label);
                foreach(string error in GetErrors("general"))
                {
                    GUILayout.Label(error, redLabel);
                }
                GUILayout.Space(10);
            }

            if (GetErrors("main").Count != 0)
            {
                GUILayout.Label("Main chute:", skins.label);
                foreach(string error in GetErrors("main"))
                {
                    GUILayout.Label(error, redLabel);
                }
                GUILayout.Space(10);
            }

            if (secondaryChute && GetErrors("secondary").Count != 0)
            {
                GUILayout.Label("Secondary chute:", skins.label);
                foreach (string error in GetErrors("secondary"))
                {
                    GUILayout.Label(error, redLabel);
                }
                GUILayout.Space(10);
            }

            GUILayout.EndVertical();
            GUILayout.EndVertical();
        }

        private void Apply()
        {
            if ((GetErrors("general").Count != 0 || GetErrors("main").Count != 0 || (secondaryChute && GetErrors("secondary").Count != 0)))
            {
                this.failedVisible = true;
                return;
            }
            else
            {
                rcModule.material = material.name;
                rcModule.mat = material;
                if (secondaryChute)
                {
                    rcModule.secMaterial = secMaterial.name;
                    rcModule.secMat = secMaterial;
                }
                rcModule.timer = RCUtils.ParseTime(timer);
                rcModule.cutSpeed = float.Parse(cutSpeed);
                rcModule.spareChutes = RCUtils.ParseWithEmpty(spares);

                if (calcSelect)
                {
                    CelestialBody body = bodies.GetBody(planets);
                    float m = 0;
                    if (getMass) { m = GetCraftMass(); }
                    else { m = float.Parse(mass); }

                    float density = 0;
                    if (typeID == 1) { density = RCUtils.GetDensityAtAlt(body, float.Parse(refDepAlt)); }
                    else { density = RCUtils.GetDensityAtAlt(body, 0f); }

                    float speed2 = Mathf.Pow(float.Parse(landingSpeed), 2f);

                    float acc = 0;
                    if (typeID == 2) { acc = float.Parse(deceleration); }
                    else { acc = (float)(body.GeeASL * RCUtils.geeToAcc); }
                    print(String.Concat("[RealChute]: ", this.part.partInfo.title, " MAIN - m: ", m, "t, rho: ", density, "kg/m³, v²: ", speed2, "m²/s², acceleration: ", acc, "m/s²"));

                    rcModule.deployedDiameter = RCUtils.Round(Mathf.Sqrt((8000f * m * acc) / (Mathf.PI * speed2 * material.dragCoefficient * density * float.Parse(chuteCount))));
                    if (rcModule.deployedDiameter > model.maxDiam)
                    {
                        rcModule.deployedDiameter = model.maxDiam;
                        warning = true;
                    }
                    else { warning = false; }
                    if (typeID == 0) { rcModule.preDeployedDiameter = RCUtils.Round(rcModule.deployedDiameter / 20); }
                    else { rcModule.preDeployedDiameter = RCUtils.Round(rcModule.deployedDiameter / 2); }
                    float scale = RCUtils.GetDiameter(rcModule.deployedArea / model.count) / model.diameter;
                    rcModule.parachute.localScale = new Vector3(scale, scale, scale);
                    print(String.Concat("[RealChute]: ", this.part.partInfo.title, " MAIN - depDiam: ", rcModule.deployedDiameter, "m, preDepDiam: ", rcModule.preDeployedDiameter, "m, scale: ", scale));
                }

                else
                {
                    rcModule.preDeployedDiameter = RCUtils.Round(float.Parse(preDepDiam));
                    rcModule.deployedDiameter = RCUtils.Round(float.Parse(depDiam));
                }

                rcModule.minIsPressure = isPressure;
                if (isPressure) { rcModule.minPressure = float.Parse(predepClause); }
                else { rcModule.minDeployment = float.Parse(predepClause); }
                rcModule.deploymentAlt = float.Parse(deploymentAlt);
                rcModule.cutAlt = RCUtils.ParseWithEmpty(cutAlt);
                rcModule.preDeploymentSpeed = float.Parse(preDepSpeed);
                rcModule.deploymentSpeed = float.Parse(depSpeed);

                if (secondaryChute)
                {
                    if (secCalcSelect)
                    {
                        CelestialBody body = bodies.GetBody(planets);
                        float m = 0;
                        if (getMass) { m = GetCraftMass(); }
                        else { m = float.Parse(secMass); }

                        float density = 0;
                        if (secTypeID == 1) { density = RCUtils.GetDensityAtAlt(body, float.Parse(secRefDepAlt)); }
                        else { density = RCUtils.GetDensityAtAlt(body, 0f); }

                        float speed2 = Mathf.Pow(float.Parse(secLandingSpeed), 2f);

                        float acc = 0;
                        if (secTypeID == 2) { acc = float.Parse(secDeceleration); }
                        else { acc = (float)(body.GeeASL * RCUtils.geeToAcc); }
                        print(String.Concat("[RealChute]: ", this.part.partInfo.title, " SECONDARY - m: ", m, "t, rho: ", density, "kg/m³, v²: ", speed2, "m²/s², acceleration: ", acc, "m/s²"));

                        rcModule.secDeployedDiameter = RCUtils.Round(Mathf.Sqrt((8000f * m * acc) / (Mathf.PI * speed2 * secMaterial.dragCoefficient * density * float.Parse(secChuteCount))));
                        if (rcModule.secDeployedDiameter > secModel.maxDiam)
                        {
                            rcModule.secDeployedDiameter = secModel.maxDiam;
                            warning = true;
                        }
                        else { warning = false; }
                        if (secTypeID == 0) { rcModule.secPreDeployedDiameter = RCUtils.Round(rcModule.secDeployedDiameter / 20); }
                        else { rcModule.secPreDeployedDiameter = RCUtils.Round(rcModule.secDeployedDiameter / 2); }
                        float scale = RCUtils.GetDiameter(rcModule.secDeployedArea / secModel.count) / secModel.diameter;
                        rcModule.secParachute.localScale = new Vector3(scale, scale, scale);
                        print(String.Concat("[RealChute]: ", this.part.partInfo.title, " SECONDARY - depDiam: ", rcModule.secDeployedDiameter, "m, preDepDiam: ", rcModule.secPreDeployedDiameter, "m, scale: ", scale));
                    }

                    else
                    {
                        rcModule.secPreDeployedDiameter = float.Parse(secPreDepDiam);
                        rcModule.secDeployedDiameter = float.Parse(secDepDiam);
                    }
                    rcModule.secMinIsPressure = secIsPressure;
                    if (secIsPressure) { rcModule.secMinPressure = float.Parse(secPredepClause); }
                    else { rcModule.secMinDeployment = float.Parse(secPredepClause); }
                    rcModule.secDeploymentAlt = float.Parse(secDeploymentAlt);
                    rcModule.secCutAlt = RCUtils.ParseWithEmpty(secCutAlt);
                    rcModule.secPreDeploymentSpeed = float.Parse(secPreDepSpeed);
                    rcModule.secDeploymentSpeed = float.Parse(secDepSpeed);
                }

                this.successfulVisible = true;
            }
        }

        private void Apply(bool toSymmetryCounterparts)
        {
            Apply();
            if (toSymmetryCounterparts && !this.failedVisible)
            {
                foreach (Part part in this.part.symmetryCounterparts)
                {
                    RealChuteModule module = part.Modules["RealChuteModule"] as RealChuteModule;

                    module.material = material.name;
                    module.mat = material;
                    if (module.secondaryChute)
                    {
                        module.secMaterial = secMaterial.name;
                        module.secMat = secMaterial;
                    }
                    module.timer = RCUtils.ParseTime(timer);
                    module.cutSpeed = float.Parse(cutSpeed);
                    module.spareChutes = RCUtils.ParseWithEmpty(spares);

                    if (calcSelect)
                    {
                        CelestialBody body = bodies.GetBody(planets);
                        float m = 0;
                        if (getMass) { m = GetCraftMass(); }
                        else { m = float.Parse(mass); }

                        float density = 0;
                        if (typeID == 1) { density = RCUtils.GetDensityAtAlt(body, float.Parse(refDepAlt)); }
                        else { density = RCUtils.GetDensityAtAlt(body, 0f); }

                        float speed2 = Mathf.Pow(float.Parse(landingSpeed), 2f);

                        float acc = 0;
                        if (typeID == 2) { acc = float.Parse(deceleration); }
                        else { acc = (float)(body.GeeASL * RCUtils.geeToAcc); }
                        print(String.Concat("[RealChute]: ", part.partInfo.title, " MAIN - m: ", m, "t, rho: ", density, "kg/m³, v²: ", speed2, "m²/s², acceleration: ", acc, "m/s²"));

                        module.deployedDiameter = RCUtils.Round(Mathf.Sqrt((8000f * m * acc) / (Mathf.PI * speed2 * material.dragCoefficient * density * float.Parse(chuteCount))));
                        if (module.deployedDiameter > model.maxDiam) { module.deployedDiameter = model.maxDiam; }
                        if (typeID == 0) { module.preDeployedDiameter = RCUtils.Round(module.deployedDiameter / 20); }
                        else { module.preDeployedDiameter = RCUtils.Round(module.deployedDiameter / 2); }
                        float scale = RCUtils.GetDiameter(module.deployedArea / model.count) / model.diameter;
                        module.parachute.localScale = new Vector3(scale, scale, scale);
                        print(String.Concat("[RealChute]: ", part.partInfo.title, " MAIN - depDiam: ", module.deployedDiameter, "m, preDepDiam: ", module.preDeployedDiameter, "m, scale: ", scale));
                    }

                    else
                    {
                        module.preDeployedDiameter = RCUtils.Round(float.Parse(preDepDiam));
                        module.deployedDiameter = RCUtils.Round(float.Parse(depDiam));
                    }
                    module.minIsPressure = isPressure;
                    if (isPressure) { module.minPressure = float.Parse(predepClause); }
                    else { module.minDeployment = float.Parse(predepClause); }
                    module.deploymentAlt = float.Parse(deploymentAlt);
                    module.cutAlt = RCUtils.ParseWithEmpty(cutAlt);
                    module.preDeploymentSpeed = float.Parse(preDepSpeed);
                    module.deploymentSpeed = float.Parse(depSpeed);

                    if (module.secondaryChute)
                    {
                        if (secCalcSelect)
                        {
                            CelestialBody body = bodies.GetBody(planets);
                            float m = 0;
                            if (getMass) { m = GetCraftMass(); }
                            else { m = float.Parse(secMass); }

                            float density = 0;
                            if (secTypeID == 1) { density = RCUtils.GetDensityAtAlt(body, float.Parse(secRefDepAlt)); }
                            else { density = RCUtils.GetDensityAtAlt(body, 0f); }

                            float speed2 = Mathf.Pow(float.Parse(secLandingSpeed), 2f);

                            float acc = 0;
                            if (secTypeID == 2) { acc = float.Parse(secDeceleration); }
                            else { acc = (float)(body.GeeASL * RCUtils.geeToAcc); }
                            print(String.Concat("[RealChute]: ", part.partInfo.title, " SECONDARY - m: ", m, "t, rho: ", density, "kg/m³, v²: ", speed2, "m²/s², acceleration: ", acc, "m/s²"));

                            module.secDeployedDiameter = RCUtils.Round(Mathf.Sqrt((8f * m * acc) / (Mathf.PI * speed2 * secMaterial.dragCoefficient * density * float.Parse(secChuteCount))));
                            if (module.secDeployedDiameter > secModel.maxDiam) { module.secDeployedDiameter = secModel.maxDiam; }
                            if (secTypeID == 0) { module.secPreDeployedDiameter = RCUtils.Round(module.secDeployedDiameter / 10); }
                            else { module.secPreDeployedDiameter = RCUtils.Round(module.secDeployedDiameter / 2); }
                            float scale = RCUtils.GetDiameter(module.secDeployedArea / secModel.count) / secModel.diameter;
                            module.secParachute.localScale = new Vector3(scale, scale, scale);
                            print(String.Concat("[RealChute]: ", part.partInfo.title, " SECONDARY - depDiam: ", module.secDeployedDiameter, "m, preDepDiam: ", module.secPreDeployedDiameter, "m, scale: ", scale));
                        }

                        else
                        {
                            module.secPreDeployedDiameter = float.Parse(secPreDepDiam);
                            module.secDeployedDiameter = float.Parse(secDepDiam);
                        };
                        module.secMinIsPressure = secIsPressure;
                        if (secIsPressure) { module.secMinPressure = float.Parse(secPredepClause); }
                        else { module.secMinDeployment = float.Parse(secPredepClause); }
                        module.secDeploymentAlt = float.Parse(secDeploymentAlt);
                        module.secCutAlt = RCUtils.ParseWithEmpty(secCutAlt);
                        module.secPreDeploymentSpeed = float.Parse(secPreDepSpeed);
                        module.secDeploymentSpeed = float.Parse(secDepSpeed);
                    }

                    UpdateScale(part, module);
                    UpdateCanopy(part, module, false);
                    if (secondaryChute)
                    {
                        UpdateCanopy(part, module, true);
                    }
                    UpdateCaseTexture(part, module);

                    ProceduralChute pChute = part.Modules["ProceduralChute"] as ProceduralChute;
                    pChute.caseID = this.caseID;
                    pChute.chuteID = this.chuteID;
                    pChute.secChuteID = this.secChuteID;
                    pChute.modelID = this.modelID;
                    pChute.secModelID = this.secModelID;
                    pChute.lastCaseID = this.lastCaseID;
                    pChute.lastChuteID = this.lastChuteID;
                    pChute.lastSecChuteID = this.lastSecChuteID;
                    pChute.lastModelID = this.lastModelID;
                    pChute.lastSecModelID = this.lastSecModelID;
                }
            }
        }

        private void UpdateScale(Part part, RealChuteModule module)
        {        
            part.transform.GetChild(0).localScale = Vector3.Scale(originalSize, currentSize);
            sizes.TryGetValue(currentSize, out module.caseMass);
            if ((HighLogic.LoadedSceneIsEditor && part == EditorLogic.SortedShipList[0]) || (HighLogic.LoadedSceneIsFlight  && this.vessel.rootPart == part))
            {
                if (part.findAttachNode("top") != null && part.findAttachNode("top").attachedPart != null)
                {
                    AttachNode topNode = part.findAttachNode("top");
                    float scale = part.transform.GetChild(0).localScale.y / debut;
                    topNode.position.y = top * scale;
                    float topDifference = topNode.position.y - (top * (Vector3.Scale(originalSize, lastSize).y / debut));
                    topNode.attachedPart.transform.Translate(0, topDifference, 0, part.transform);
                    if (part.findAttachNode("top").attachedPart.GetAllChildren().Count > 0)
                    {
                        topNode.attachedPart.GetAllChildren().ForEach(p => p.transform.Translate(0, topDifference, 0, part.transform));
                    }
                }
                if (part.findAttachNode("bottom") != null && part.findAttachNode("bottom").attachedPart != null)
                {
                    AttachNode bottomNode = part.findAttachNode("bottom");
                    float scale = part.transform.GetChild(0).localScale.y / debut;
                    bottomNode.position.y = bottom * scale;
                    float bottomDifference = bottomNode.position.y - (bottom * (Vector3.Scale(originalSize, lastSize).y / debut));
                    bottomNode.attachedPart.transform.Translate(0, bottomDifference, 0, part.transform);
                    if (part.findAttachNode("bottom").attachedPart.GetAllChildren().Count > 0)
                    {
                        bottomNode.attachedPart.GetAllChildren().ForEach(p => p.transform.Translate(0, bottomDifference, 0, part.transform));
                    }
                }
            }
            else if (part.findAttachNode("bottom") != null && part.findAttachNode("bottom").attachedPart != null && part.parent != null &&  part.findAttachNode("bottom").attachedPart == part.parent)
            {
                AttachNode bottomNode = part.findAttachNode("bottom");
                float scale = part.transform.GetChild(0).localScale.y / debut;
                bottomNode.position.y = bottom * scale;
                float bottomDifference = bottomNode.position.y - (bottom * (Vector3.Scale(originalSize, lastSize).y / debut));
                part.transform.Translate(0, -bottomDifference, 0, part.transform);
                if (part.findAttachNode("top") != null)
                {
                    AttachNode topNode = part.findAttachNode("top");
                    topNode.position.y = top * scale;
                    float topDifference = topNode.position.y - (top * (Vector3.Scale(originalSize, lastSize).y / debut));
                    if (part.GetAllChildren().Count > 0) { part.GetAllChildren().ForEach(p => p.transform.Translate(0, -(bottomDifference - topDifference), 0, part.transform)); }
                }
            }
            else if (part.findAttachNode("top") != null && part.findAttachNode("top").attachedPart != null && part.parent != null && part.findAttachNode("top").attachedPart == part.parent)
            {
                AttachNode topNode = part.findAttachNode("top");
                float scale = part.transform.GetChild(0).localScale.y / debut;
                topNode.position.y = top * scale;
                float topDifference = topNode.position.y - (top * (Vector3.Scale(originalSize, lastSize).y / debut));
                part.transform.Translate(0, -topDifference, 0, part.transform);
                if (part.findAttachNode("bottom") != null)
                {
                    AttachNode bottomNode = part.findAttachNode("bottom");
                    bottomNode.position.y = bottom * scale;
                    float bottomDifference = bottomNode.position.y - (bottom * (Vector3.Scale(originalSize, lastSize).y / debut));
                    if (part.GetAllChildren().Count > 0) { part.GetAllChildren().ForEach(p => p.transform.Translate(0, -(topDifference - bottomDifference), 0, part.transform)); }
                }
            }

            float scaleX = (part.transform.GetChild(0).localScale.x / debut) / (Vector3.Scale(originalSize, lastSize).x / debut);
            float scaleZ = (part.transform.GetChild(0).localScale.z / debut) / (Vector3.Scale(originalSize, lastSize).z / debut);
            Vector3 chute = part.FindModelTransform(module.parachuteName).transform.position - part.transform.position;
            part.FindModelTransform(module.parachuteName).transform.Translate(chute.x * (scaleX - 1), 0, chute.z * (scaleZ - 1), part.transform);
            if (secondaryChute)
            {
                Vector3 secChute = part.FindModelTransform(module.secParachuteName).transform.position - part.transform.position;
                part.FindModelTransform(module.secParachuteName).transform.Translate(secChute.x * (scaleX - 1), 0, secChute.z * (scaleZ - 1), part.transform);
            }

            if (part.children.Count(p => p.attachMode == AttachModes.SRF_ATTACH) > 0)
            {
                List<Part> surfaceAttached = new List<Part>(part.children.Where(p => p.attachMode == AttachModes.SRF_ATTACH));
                surfaceAttached.Where(p => p.GetAllChildren().Count > 0).ToList().ForEach(p => surfaceAttached.AddRange(p.GetAllChildren()));
                foreach (Part p in surfaceAttached)
                {
                    Vector3 v = p.transform.position - part.transform.position;
                    p.transform.Translate(v.x * (scaleX - 1), 0, v.z * (scaleZ - 1), part.transform);
                }
            }
            lastSize = currentSize;
        }

        private void UpdateCaseTexture(Part part, RealChuteModule module)
        {
            if (textures.TryGetCase(caseID, type, ref parachuteCase))
            {
                if (string.IsNullOrEmpty(parachuteCase.textureURL))
                {
                    Debug.LogWarning("[RealChute]: The " + textures.caseNames[caseID] + "URL is empty");
                    lastCaseID = caseID;
                    return;
                }
                Texture2D texture = GameDatabase.Instance.GetTexture(parachuteCase.textureURL, false);
                if (texture == null)
                {
                    Debug.LogWarning("[RealChute]: The " + textures.caseNames[caseID] + "texture is null");
                    lastCaseID = caseID;
                    return;
                }
                part.GetPartRenderers(module).ForEach(r => r.material.mainTexture = texture);
            }
            lastCaseID = caseID;
        }

        private void UpdateCanopyTexture(RealChuteModule module, bool secondary)
        {
            if (!secondary)
            {
                if (textures.TryGetCanopy(chuteID, ref canopy))
                {
                    if (string.IsNullOrEmpty(canopy.textureURL))
                    {
                        Debug.LogWarning("[RealChute]: The " + textures.canopyNames[chuteID] + "URL is empty");
                        lastChuteID = chuteID;
                        return;
                    }
                    Texture2D texture = GameDatabase.Instance.GetTexture(canopy.textureURL, false);
                    if (texture == null)
                    {
                        Debug.LogWarning("[RealChute]: The " + textures.canopyNames[chuteID] + "texture is null");
                        lastChuteID = chuteID;
                        return;
                    }
                    module.parachute.GetComponents<Renderer>().ToList().ForEach(r => r.material.mainTexture = texture);
                }
                lastChuteID = chuteID;
            }
            else
            {
                if (textures.TryGetCanopy(secChuteID, ref secCanopy))
                {
                    if (string.IsNullOrEmpty(secCanopy.textureURL))
                    {
                        Debug.LogWarning("[RealChute]: The " + textures.canopyNames[secChuteID] + "URL is empty");
                        lastSecChuteID = secChuteID;
                        return;
                    }
                    Texture2D texture = GameDatabase.Instance.GetTexture(secCanopy.textureURL, false);
                    if (texture == null)
                    {
                        Debug.LogWarning("[RealChute]: The " + textures.canopyNames[secChuteID] + "texture is null");
                        lastSecChuteID = secChuteID;
                        return;
                    }
                    module.secParachute.GetComponents<Renderer>().ToList().ForEach(r => r.material.mainTexture = texture);
                }
                lastSecChuteID = secChuteID;
            }
        }

        private void UpdateCanopy(Part part, RealChuteModule module, bool secondary)
        {
            int id = secondary ? secModelID : modelID;
            ModelConfig model = new ModelConfig();
            if (textures.TryGetModel(textures.modelNames[id], ref model))
            {
                ParachuteParameters parameters = secondary ? model.secondary : model.main;
                if (string.IsNullOrEmpty(parameters.modelURL))
                {
                    Debug.LogWarning("[RealChute]: The " + textures.modelNames[modelID] + (secondary ? "main" : "secondary") + "URL is empty");
                    if (!secondary) { lastModelID = modelID; }
                    else { lastSecModelID = secModelID; }
                    return;
                }
                GameObject test = GameDatabase.Instance.GetModel(parameters.modelURL);
                if (test == null)
                {
                    Debug.LogWarning("[RealChute]: The " + textures.modelNames[modelID] + (secondary ? "main" : "secondary") + "GameObject is null");
                    if (!secondary) { lastModelID = modelID; }
                    else { lastSecModelID = secModelID; }
                    return;
                }
                test.SetActive(true);
                if (!secondary)
                {
                    float scale = RCUtils.GetDiameter(module.deployedArea / model.count) / model.diameter;
                    test.transform.localScale = new Vector3(scale, scale, scale);
                }
                else
                {
                    float scale = RCUtils.GetDiameter(module.secDeployedArea / model.count) / model.diameter;
                    test.transform.localScale = new Vector3(scale, scale, scale);
                }
                    
                GameObject obj = Instantiate(test) as GameObject;
                Destroy(test);
                Transform toDestroy;
                if (!secondary)
                {
                    toDestroy = part.FindModelTransform(originalTransform);
                    obj.transform.parent = toDestroy.parent;
                    obj.transform.position = toDestroy.position;
                    if (HighLogic.LoadedSceneIsFlight) { DestroyImmediate(toDestroy.gameObject); }
                    this.model = model;
                    module.parachute = part.FindModelTransform(parameters.transformName);
                    module.parachute.transform.localRotation = Quaternion.identity;
                    module.parachute.gameObject.SetActive(false);
                    float scale = RCUtils.GetDiameter(module.deployedArea / model.count) / model.diameter;
                    print("[RealChute]: " + part.partInfo.title + " MAIN - scale: " + scale);
                    module.parachute.localScale = new Vector3(scale, scale, scale);
                    module.parachuteName = parameters.transformName;
                    module.deploymentAnimation = parameters.depAnim;
                    module.preDeploymentAnimation = parameters.preDepAnim;
                    part.InitiateAnimation(module.deploymentAnimation);
                    part.InitiateAnimation(module.preDeploymentAnimation);
                }
                else
                {
                    toDestroy = part.FindModelTransform(secOriginalTransform);
                    obj.transform.parent = toDestroy.parent;
                    obj.transform.position = toDestroy.position;
                    if (HighLogic.LoadedSceneIsFlight) { DestroyImmediate(toDestroy.gameObject); }
                    this.secModel = model;
                    module.secParachute = part.FindModelTransform(parameters.transformName); ;
                    module.secParachute.transform.localRotation = Quaternion.identity;
                    module.secParachute.gameObject.SetActive(false);
                    float scale = RCUtils.GetDiameter(module.secDeployedArea / model.count) / model.diameter;
                    print("[RealChute]: " + part.partInfo.title + " SECONDARY - scale: " + scale);
                    module.secParachute.localScale = new Vector3(scale, scale, scale);
                    module.secParachuteName = parameters.transformName;
                    module.secDeploymentAnimation = parameters.depAnim;
                    module.secPreDeploymentAnimation = parameters.preDepAnim;
                    part.InitiateAnimation(module.secDeploymentAnimation);
                    part.InitiateAnimation(module.secPreDeploymentAnimation);
                }
            }
            if (!secondary)
            {
                lastModelID = modelID;
                lastTransform = model.main.transformName;
            }
            else
            {
                lastSecModelID = secModelID;
                secLastTransform = model.secondary.transformName;
            }
            UpdateCanopyTexture(module, secondary);
        }
        #endregion

        #region Functions
        private void Update()
        {
            if (!CompatibilityChecker.IsCompatible()) { return; }
            if (!HighLogic.LoadedSceneIsEditor || ((this.part.Modules["RealChuteModule"] != null && !((RealChuteModule)this.part.Modules["RealChuteModule"]).isTweakable))) { return; }
            
            if (this.part.transform.GetChild(0).localScale != Vector3.Scale(originalSize, currentSize))
            {
                UpdateScale(this.part, rcModule);
            }

            if (!HighLogic.LoadedSceneIsEditor || !EditorLogic.fetch || EditorLogic.fetch.editorScreen != EditorLogic.EditorScreen.Actions || !this.part.Modules.Contains("RealChuteModule"))
            {
                this.visible = false;
                return;
            }

            if (this.materialsVisible && !this.secMaterialsVisible)
            {
                matX = (int)materialsWindow.x;
                matY = (int)materialsWindow.y;
                secMaterialsWindow.x = matX;
                secMaterialsWindow.y = matY;
            }

            if (this.secMaterialsVisible && !this.materialsVisible)
            {
                matX = (int)secMaterialsWindow.x;
                matY = (int)secMaterialsWindow.y;
                materialsWindow.x = matX;
                materialsWindow.y = matY;
            }

            if (actionPanel.GetSelectedParts().Contains(this.part))
            {
                this.visible = true;
                if (sizes.Count > 0)
                {
                    currentSize = sizes.Keys.ToArray()[size];
                }
            }

            else
            {
                this.visible = false;
                this.materialsVisible = false;
                this.secMaterialsVisible = false;
                this.failedVisible = false;
                this.successfulVisible = false;
            }

            if (lastSize != currentSize)
            {
                UpdateScale(this.part, rcModule);
            }

            if (lastCaseID != caseID)
            {
                UpdateCaseTexture(this.part, rcModule);
            }

            if (lastChuteID != chuteID)
            {
                UpdateCanopyTexture(rcModule, false);
            }

            if (lastSecChuteID != secChuteID)
            {
                UpdateCanopyTexture(rcModule, true);
            }

            if (lastModelID != modelID)
            {
                UpdateCanopy(this.part, rcModule, false);
            }

            if (lastSecModelID != secModelID)
            {
                UpdateCanopy(this.part, rcModule, true);
            }

            if (lastTypeID != typeID)
            {
                switch (typeID)
                {
                    case 0:
                        {
                            landingSpeed = "6";
                            deploymentAlt = "700";
                            preDepSpeed = "2";
                            depSpeed = "6";
                            break;
                        }

                    case 1:
                        {
                            landingSpeed = "80";
                            deploymentAlt = "2500";
                            preDepSpeed = "1";
                            depSpeed = "3";
                            break;
                        }

                    case 2:
                        {
                            landingSpeed = "100";
                            deploymentAlt = "100";
                            preDepSpeed = "1";
                            depSpeed = "2";
                            break;
                        }
                    default:
                        break;
                }
            }
            if (lastSecTypeID != secTypeID)
            {
                switch (secTypeID)
                {
                    case 0:
                        {
                            secLandingSpeed = "6";
                            secDeploymentAlt = "700";
                            secPreDepSpeed = "2";
                            secDepSpeed = "6";
                            break;
                        }

                    case 1:
                        {
                            secLandingSpeed = "80";
                            secDeploymentAlt = "2500";
                            secPreDepSpeed = "1";
                            secDepSpeed = "3";
                            break;
                        }

                    case 2:
                        {
                            secLandingSpeed = "100";
                            secDeploymentAlt = "100";
                            secPreDepSpeed = "1";
                            secDepSpeed = "2";
                            break;
                        }
                    default:
                        break;
                }
            }
        }
        #endregion

        #region Overrides
        public override void OnStart(PartModule.StartState state)
        {
            if ((!HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight) || !CompatibilityChecker.IsCompatible()) { return; }

            //Identification of the RealChuteModule
            if (this.part.Modules.Contains("RealChuteModule")) { rcModule = this.part.Modules["RealChuteModule"] as RealChuteModule; }
            else { return; }
            if (!rcModule.isTweakable) { return; }
            secondaryChute = rcModule.secondaryChute;

            //Creation of the sizes dictionary
            sizes = vectors.ToDictionary(v => new Vector3(v.x, v.y, v.z), v => v.w);
            currentSize = sizes.Keys.ToArray()[size];
            originalSize = this.part.transform.localScale;

            //Creation of the materials library
            materials = MaterialsLibrary.instance;
            materials.TryGetMaterial(rcModule.material, ref material);
            materialsID = materials.GetMaterialIndex(rcModule.material);
            if (secondaryChute)
            {
                if (rcModule.secMaterial != "empty")
                {
                    materials.TryGetMaterial(rcModule.secMaterial, ref secMaterial);
                    secMaterialsID = materials.GetMaterialIndex(rcModule.secMaterial);
                }
                else
                {
                    secMaterial = material;
                    secMaterialsID = materialsID;
                }
            }

            //Creation of the atmospheric planets library
            bodies = AtmoPlanets.fetch;
            planets = bodies.GetPlanetIndex("Kerbin");

            //Creates an instance of the texture library
            if (textureLibrary != "none")
            {
                textureLib = TextureLibrary.instance;
                textureLib.TryGetConfig(textureLibrary, ref textures);
                cases = textures.caseNames;
                canopies = textures.canopyNames;
                models = textures.modelNames;
                parachuteCase = textures.GetCase(caseID, type);
                canopy = textures.GetCanopy(chuteID);
                secCanopy = textures.GetCanopy(lastSecChuteID);
                model = textures.GetModel(modelID);
                secModel = textures.GetModel(secModelID);
                lastCaseID = caseID;
                lastChuteID = chuteID;
                lastSecChuteID = secChuteID;
                lastModelID = modelID;
                lastSecModelID = secModelID;
            }

            if (HighLogic.LoadedSceneIsEditor)
            {
                //Windows initiation
                this.window = new Rect(5, 370, 420, Screen.height - 375);
                this.materialsWindow = new Rect(matX, matY, 280, 265);
                this.secMaterialsWindow = new Rect(matX, matY, 280, 265);
                this.failedWindow = new Rect(Screen.width / 2 - 150, Screen.height / 2 - 150, 300, 300);
                this.successfulWindow = new Rect(Screen.width / 2 - 150, Screen.height / 2 - 25, 300, 50);

                if (!initiated)
                {
                    //Gets the original part state
                    if (textureLibrary != "none")
                    {
                        if (textures.TryGetCase(currentCase, ref parachuteCase)) { caseID = textures.GetCaseIndex(parachuteCase); }
                        if (textures.TryGetCanopy(currentCanopy, ref canopy)) { chuteID = textures.GetCanopyIndex(canopy); }
                        if (secondaryChute && textures.TryGetCanopy(secCurentCanopy, ref secCanopy)) { secChuteID = textures.GetCanopyIndex(secCanopy); }
                        if (textures.TryGetModel(rcModule.parachuteName, ref model, true)) { modelID = textures.GetModelIndex(model); }
                        if (secondaryChute && textures.TryGetModel(rcModule.secParachuteName, ref secModel, true)) { secModelID = textures.GetModelIndex(secModel); }
                        lastCaseID = caseID;
                        lastChuteID = chuteID;
                        lastSecChuteID = secChuteID;
                        lastModelID = modelID;
                        lastSecModelID = secModelID;
                    }

                    //Identification of the values from the RealChuteModule
                    mustGoDown = rcModule.mustGoDown;
                    timer = rcModule.timer + "s";
                    cutSpeed = rcModule.cutSpeed.ToString();
                    if (rcModule.spareChutes != -1) { spares = rcModule.spareChutes.ToString(); }
                    preDepDiam = rcModule.preDeployedDiameter.ToString();
                    depDiam = rcModule.deployedDiameter.ToString();
                    isPressure = rcModule.minIsPressure;
                    if (isPressure) { predepClause = rcModule.minPressure.ToString(); }
                    else { predepClause = rcModule.minDeployment.ToString(); }
                    deploymentAlt = rcModule.deploymentAlt.ToString();
                    cutAlt = rcModule.cutAlt.ToString();
                    if (cutAlt == "-1") { cutAlt = string.Empty; }
                    preDepSpeed = rcModule.preDeploymentSpeed.ToString();
                    depSpeed = rcModule.deploymentSpeed.ToString();
                    if (secondaryChute)
                    {
                        secPreDepDiam = rcModule.secPreDeployedDiameter.ToString();
                        secDepDiam = rcModule.secDeployedDiameter.ToString();
                        secIsPressure = rcModule.secMinIsPressure;
                        if (secIsPressure) { secPredepClause = rcModule.secMinPressure.ToString(); }
                        else { secPredepClause = rcModule.secMinDeployment.ToString(); }
                        secDeploymentAlt = rcModule.secDeploymentAlt.ToString();
                        secCutAlt = rcModule.secCutAlt.ToString();
                        if (secCutAlt == "-1") { secCutAlt = string.Empty; }
                        secPreDepSpeed = rcModule.secPreDeploymentSpeed.ToString();
                        secDepSpeed = rcModule.secDeploymentSpeed.ToString();
                    }
                    originalTransform = rcModule.parachuteName;
                    if (secondaryChute)
                    {
                        secOriginalTransform = rcModule.secParachuteName;
                    }
                    originalSize = this.part.transform.localScale;

                    initiated = true;
                }
                lastTransform = originalTransform;
                secLastTransform = secOriginalTransform;

                //Creation of the red label
                redLabel = new GUIStyle(skins.label);
                redLabel.normal.textColor = XKCDColors.Red;
                redLabel.hover.textColor = XKCDColors.Red;

                //Creation of the large label
                boldLabel = new GUIStyle(skins.label);
                boldLabel.fontStyle = FontStyle.Bold;
            }

            if (parent == null) { parent = this.part.FindModelTransform(originalTransform).parent; }
            position = this.part.FindModelTransform(originalTransform).position;
            if (secondaryChute) { secPosition = this.part.FindModelTransform(secOriginalTransform).position; }
         

            //Updates the part
            UpdateCanopy(this.part, rcModule, false);
            if (secondaryChute)
            {
                UpdateCanopy(this.part, rcModule, true);
            }
            UpdateScale(this.part, rcModule);
            UpdateCaseTexture(this.part, rcModule);
        }

        public override void OnLoad(ConfigNode node)
        {
            if (!CompatibilityChecker.IsCompatible()) { return; }
            if ((HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight) && ((this.part.Modules["RealChuteModule"] != null && !((RealChuteModule)this.part.Modules["RealChuteModule"]).isTweakable))) { return; }
            
            if (vectors.Count == 0) { vectors = node.GetValues("size").Select(v => KSPUtil.ParseVector4(v)).ToList(); }

            if (this.part.findAttachNode("top") != null)
            {
                top = this.part.findAttachNode("top").originalPosition.y;
            }

            if (this.part.findAttachNode("bottom") != null)
            {
                bottom = this.part.findAttachNode("bottom").originalPosition.y;
            }
            if (debut == 0) { debut = this.part.transform.GetChild(0).localScale.y; }   
        }

        public override string GetInfo()
        {
            if (!CompatibilityChecker.IsCompatible()) { return string.Empty; }
            if ((HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight) && ((this.part.Modules["RealChuteModule"] != null && !((RealChuteModule)this.part.Modules["RealChuteModule"]).isTweakable))) { return string.Empty; }           
            if (this.part.Modules.Contains("RealChuteModule")) { return "This RealChute part can be tweaked from the Action Groups window."; }
            return string.Empty;
        }
        #endregion

        #region GUI
        private void OnGUI()
        {
            if (!CompatibilityChecker.IsCompatible()) { return; }
            if (HighLogic.LoadedSceneIsEditor)
            {
                if ((this.part.Modules["RealChuteModule"] != null && !((RealChuteModule)this.part.Modules["RealChuteModule"]).isTweakable)) { return; }
                
                if (this.visible)
                {
                    this.window = GUILayout.Window(this.mainId, this.window, Window, "RealChute Parachute Editor", skins.window, GUILayout.MaxWidth(420), GUILayout.MaxHeight(Screen.height - 375));
                }

                if (this.materialsVisible)
                {
                    this.materialsWindow = GUILayout.Window(this.matId, this.materialsWindow, Materials, "Choose a material", skins.window, GUILayout.MaxWidth(280), GUILayout.MaxHeight(265));
                }

                if (this.secMaterialsVisible)
                {
                    this.secMaterialsWindow = GUILayout.Window(this.secMatId, this.secMaterialsWindow, SecMaterials, "Choose a material", skins.window, GUILayout.MaxWidth(280), GUILayout.MaxHeight(265));
                }

                if (this.failedVisible)
                {
                    this.failedWindow = GUILayout.Window(this.failedId, this.failedWindow, ApplicationFailed, "Error", skins.window, GUILayout.MaxWidth(300), GUILayout.MaxHeight(300));
                }

                if (this.successfulVisible)
                {
                    this.successfulWindow = GUILayout.Window(this.successId, this.successfulWindow, ApplicationSucceeded, "Success", skins.window, GUILayout.MaxWidth(300), GUILayout.MaxHeight(200));
                }
            }
        }

        private void Window(int id)
        {
            GUILayout.BeginVertical();

            #region Info labels
            GUILayout.Label("Selected part: " + this.part.partInfo.title, skins.label);
            GUILayout.Label("Symmetry counterparts: " + (this.part.symmetryCounterparts.Count), skins.label);
            GUILayout.Label("Part mass: " + this.part.TotalMass().ToString("0.###"), skins.label);
            #endregion

            #region Planet selector
            GUILayout.Space(5);
            GUILayout.BeginHorizontal(GUILayout.Height(30));
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Target planet:", skins.label);
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            planets = GUILayout.SelectionGrid(planets, bodies.GetNames(), 4, skins.button, GUILayout.Width(250));
            GUILayout.EndHorizontal();
            #endregion

            #region Size cyclers
            if (sizes.Count > 0)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(20));
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Cycle part size", skins.label);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Next size", skins.button, GUILayout.Width(125)))
                {
                    size++;
                    if (size > sizes.Count - 1) { size = 0; }
                }
                if (GUILayout.Button("Previous size", skins.button, GUILayout.Width(125)))
                {
                    size--;
                    if (size < 0) { size = sizes.Count - 1; }
                }
                GUILayout.EndHorizontal();
            }
            #endregion

            GUILayout.Space(5);
            mainScroll = GUILayout.BeginScrollView(mainScroll, false, false, skins.horizontalScrollbar, skins.verticalScrollbar);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            #region Texture selectors
            GUILayout.Space(5);
            if (TextureEntries("case").Length > 1 || TextureEntries("chute").Length > 1 || TextureEntries("model").Length > 1)
            {
                int m = 0;
                if (TextureEntries("case").Length > 1) { m++; }
                if (TextureEntries("chute").Length > 1) { m++; }
                if (TextureEntries("model").Length > 1) { m++; }

                GUILayout.BeginHorizontal();

                #region Labels
                GUILayout.BeginVertical(GUILayout.Height(35 * m));

                //Labels
                if (TextureEntries("case").Length > 1)
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Case texture:", skins.label);
                }
                if (TextureEntries("chute").Length > 1) 
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Chute texture:", skins.label);
                }
                if (TextureEntries("model").Length > 1) 
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Chute model: ", skins.label);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                #endregion

                #region Selectors
                GUILayout.BeginVertical(skins.box, GUILayout.Height(35 * m));
                //Boxes
                if (TextureEntries("case").Length > 1)
                {
                    GUILayout.FlexibleSpace();
                    caseID = GUILayout.SelectionGrid(caseID, TextureEntries("case"), TextureEntries("case").Length, skins.button);
                }

                if (TextureEntries("chute").Length > 1)
                {
                    GUILayout.FlexibleSpace();
                    chuteID = GUILayout.SelectionGrid(chuteID, TextureEntries("chute"), TextureEntries("chute").Length, skins.button);
                }

                if (TextureEntries("model").Length > 1)
                {
                    GUILayout.FlexibleSpace();
                    modelID = GUILayout.SelectionGrid(modelID, TextureEntries("model"), TextureEntries("model").Length, skins.button);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                #endregion

                GUILayout.EndHorizontal();
            }
            #endregion

            #region General
            //Materials editor
            GUILayout.Space(5);
            if (materials.count > 1)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(20));
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Current material: " + material.name, skins.label);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Change material", skins.button,GUILayout.Width(150)))
                {
                    this.materialsVisible = !this.materialsVisible;
                }
                GUILayout.EndHorizontal();
            }

            //MustGoDown
            GUILayout.Space(5);
            GUILayout.BeginHorizontal(GUILayout.MaxWidth(window.width));
            GUILayout.Label("Must go down to deploy:", skins.label);
            if (GUILayout.Toggle(mustGoDown, "True", skins.toggle)) { mustGoDown = true; }
            GUILayout.FlexibleSpace();
            if (GUILayout.Toggle(!mustGoDown, "False", skins.toggle)) { mustGoDown = false; }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            //Timer
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (RCUtils.CanParseTime(timer) && RCUtils.CheckRange(RCUtils.ParseTime(timer), 0, 3600)) { GUILayout.Label("Deployment timer:", skins.label); }
            else { GUILayout.Label("Deployment timer:", redLabel); }
            GUILayout.FlexibleSpace();
            timer = GUILayout.TextField(timer, 10, skins.textField, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            //Spares
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (RCUtils.CanParseWithEmpty(spares) && RCUtils.CheckRange(RCUtils.ParseWithEmpty(spares), -1, 10) && RCUtils.IsWholeNumber(RCUtils.ParseWithEmpty(spares))) { GUILayout.Label("Spare chutes:", skins.label); }
            else { GUILayout.Label("Spare chutes:", redLabel); }
            GUILayout.FlexibleSpace();
            spares = GUILayout.TextField(spares, 10, skins.textField, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            //CutSpeed
            CreateEntryArea("Autocut speed (m/s):", ref cutSpeed, 0.01f, 100);
            #endregion

            #region Main
            #region Calculations
            //Indicator label
            GUILayout.Space(10);
            GUILayout.Label("________________________________________________", boldLabel);
            GUILayout.Label("Main chute:", boldLabel, GUILayout.Width(150));
            GUILayout.Label("‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾", boldLabel);

            //Selection mode
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Calculations mode:", skins.label);
            if (GUILayout.Toggle(calcSelect, "Automatic", skins.toggle)) { calcSelect = true; }
            GUILayout.FlexibleSpace();
            if (GUILayout.Toggle(!calcSelect, "Manual", skins.toggle)) { calcSelect = false; }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            //Calculations
            parachuteScroll = GUILayout.BeginScrollView(parachuteScroll, false, false, skins.horizontalScrollbar, skins.verticalScrollbar, skins.box, GUILayout.Height(160));

            #region Automatic
            if (calcSelect)
            {
                typeID = GUILayout.SelectionGrid(typeID, RCUtils.types, 3, skins.button);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Toggle(getMass, "Use current craft mass", skins.button, GUILayout.Width(150))) { getMass = true; }
                GUILayout.FlexibleSpace();
                if (GUILayout.Toggle(!getMass, "Input craft mass", skins.button, GUILayout.Width(150))) { getMass = false; }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                if (!getMass)
                {
                    CreateEntryArea("Mass to use (t):", ref mass, 0.1f, 10000, 100);
                }

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                if (typeID == 0)
                {
                    if (RCUtils.CanParse(landingSpeed) && RCUtils.CheckRange(float.Parse(landingSpeed), 0.1f, 300)) { GUILayout.Label("Wanted touchdown speed (m/s):", skins.label); }
                    else { GUILayout.Label("Wanted touchdown speed (m/s):", redLabel); }
                }
                else if (typeID == 1)
                {
                    if (RCUtils.CanParse(landingSpeed) && RCUtils.CheckRange(float.Parse(landingSpeed), 0.1f, 300)) { GUILayout.Label("Wanted speed at full deployment (m/s):", skins.label); }
                    else { GUILayout.Label("Wanted speed at full deployment (m/s):", redLabel); }
                }
                else
                {
                    if (RCUtils.CanParse(landingSpeed) && RCUtils.CheckRange(float.Parse(landingSpeed), 0.1f, 300)) { GUILayout.Label("Planned landing speed (m/s):", skins.label); }
                    else { GUILayout.Label("Planned landing speed (m/s):", redLabel); }
                }
                GUILayout.FlexibleSpace();
                landingSpeed = GUILayout.TextField(landingSpeed, 10, skins.textField, GUILayout.Width(100));
                GUILayout.EndHorizontal();

                if (typeID == 2)
                {
                    CreateEntryArea("Wanted deceleration (m/s²):", ref deceleration, 0.1f, 100, 100);
                }

                if (typeID == 1)
                {
                    CreateEntryArea("Mains planned deployment alt (m):", ref refDepAlt, 10, 45000, 100);
                }

                CreateEntryArea("Parachutes used (parts):", ref chuteCount, 1, 100, 100);
            }
            #endregion

            #region Manual
            else
            {
                //Predeployed diameter
                CreateEntryArea("Predeployed diameter (m):", ref preDepDiam, 0.5f, model.maxDiam / 2, 100);
                if (RCUtils.CanParse(preDepDiam)) { GUILayout.Label("Resulting area: " + RCUtils.GetArea(float.Parse(preDepDiam)).ToString("0.00") + "m²", skins.label); }
                else { GUILayout.Label("Resulting predeployed area: --- m²", skins.label); }

                //Deployed diameter
                CreateEntryArea("Deployed diameter (m):", ref depDiam, 1, model.maxDiam, 100);
                if (RCUtils.CanParse(depDiam)) { GUILayout.Label("Resulting area: " + RCUtils.GetArea(float.Parse(depDiam)).ToString("0.00") + "m²", skins.label); }
                else { GUILayout.Label("Resulting deployed area: --- m²", skins.label); }
            }
            #endregion

            GUILayout.EndScrollView();
            #endregion

            #region Specific
            //Pressure/alt toggle
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(isPressure, "Pressure predeployment", skins.toggle)) { isPressure = true; }
            GUILayout.FlexibleSpace();
            if (GUILayout.Toggle(!isPressure, "Altitude predeployment", skins.toggle)) { isPressure = false; }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (isPressure)
            {
                if (RCUtils.CanParse(predepClause) && RCUtils.CheckRange(float.Parse(predepClause), 0.0001f, 5f)) { GUILayout.Label("Predeployment pressure (atm):", skins.label); }
                else { GUILayout.Label("Predeployment pressure (atm):", redLabel); } 
            }
            else
            {
                if (RCUtils.CanParse(predepClause) && RCUtils.CheckRange(float.Parse(predepClause), 25, 50000)) { GUILayout.Label("Predeployment altitude (m):", skins.label); }
                else { GUILayout.Label("Predeployment altitude (m):", redLabel); } 
            }
            GUILayout.FlexibleSpace();
            predepClause = GUILayout.TextField(predepClause, 10, skins.textField, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            //Deployment altitude
            CreateEntryArea("Deployment altitude", ref deploymentAlt, 10, 45000);

            //Cut altitude
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (RCUtils.CanParseWithEmpty(cutAlt) && RCUtils.CheckRange(RCUtils.ParseWithEmpty(cutAlt), -1, 45000)) { GUILayout.Label("Autocut altitude (m):", skins.label); }
            else { GUILayout.Label("Autocut altitude (m):", redLabel); }
            GUILayout.FlexibleSpace();
            cutAlt = GUILayout.TextField(cutAlt, 10, skins.textField, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            //Predeployment speed
            CreateEntryArea("Pre deployment speed (s):", ref preDepSpeed, 0.5f, 5);

            //Deployment speed
            CreateEntryArea("Deployment speed (s):", ref depSpeed, 1, 10);
            #endregion
            #endregion

            #region Secondary
            if (secondaryChute)
            {
                //Indicator label
                GUILayout.Space(10);
                GUILayout.Label("________________________________________________", boldLabel);
                GUILayout.Label("Secondary chute:", boldLabel, GUILayout.Width(150));
                GUILayout.Label("‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾", boldLabel);

                #region Texture selectors
                GUILayout.Space(5);
                if (TextureEntries("chute").Length > 1 || TextureEntries("model").Length > 1)
                {
                    int m = 0;
                    if (TextureEntries("chute").Length > 1) { m++; }
                    if (TextureEntries("model").Length > 1) { m++; }
                    GUILayout.BeginHorizontal();

                    #region Labels
                    GUILayout.BeginVertical(GUILayout.Height(35 * m));

                    //Labels
                    if (TextureEntries("chute").Length > 1)
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("Chute texture:", skins.label);
                    }
                    if (TextureEntries("model").Length > 1)
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("Chute model: ", skins.label);
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndVertical();
                    #endregion

                    #region Selectors
                    GUILayout.BeginVertical(skins.box, GUILayout.Height(35 * m));
                    //Boxes
                    if (TextureEntries("chute").Length > 1)
                    {
                        GUILayout.FlexibleSpace();
                        secChuteID = GUILayout.SelectionGrid(secChuteID, TextureEntries("chute"), 2, skins.button);
                    }

                    if (TextureEntries("model").Length > 1)
                    {
                        GUILayout.FlexibleSpace();
                        secModelID = GUILayout.SelectionGrid(secModelID, TextureEntries("model"), 2, skins.button);
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndVertical();
                    #endregion

                    GUILayout.EndHorizontal();
                }
                #endregion

                //Materials editor
                GUILayout.Space(5);
                if (materials.count > 1)
                {
                    GUILayout.BeginHorizontal(GUILayout.Height(20));
                    GUILayout.BeginVertical();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Current material: " + secMaterial.name, skins.label);
                    GUILayout.EndVertical();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Change material", skins.button, GUILayout.Width(150)))
                    {
                        this.secMaterialsVisible = !this.secMaterialsVisible;
                    }
                    GUILayout.EndHorizontal();
                }

                //Selection mode
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Calculations mode:", skins.label);
                if (GUILayout.Toggle(secCalcSelect, "Automatic", skins.toggle)) { secCalcSelect = true; }
                GUILayout.FlexibleSpace();
                if (GUILayout.Toggle(!secCalcSelect, "Manual", skins.toggle)) { secCalcSelect = false; }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(5);

                #region Calculations
                secParachuteScroll = GUILayout.BeginScrollView(secParachuteScroll, false, false, skins.horizontalScrollbar, skins.verticalScrollbar, skins.box, GUILayout.Height(160));

                #region Automatic
                if (secCalcSelect)
                {
                    secTypeID = GUILayout.SelectionGrid(secTypeID, RCUtils.types, 3, skins.button);

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Toggle(secGetMass, "Use current craft mass", skins.button, GUILayout.Width(150))) { secGetMass = true; }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Toggle(!secGetMass, "Input craft mass", skins.button, GUILayout.Width(150))) { secGetMass = false; }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    if (!getMass)
                    {
                        CreateEntryArea("Mass to use (t):", ref secMass, 0.1f, 10000, 100);
                    }

                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal();
                    if (secTypeID == 0)
                    {
                        if (RCUtils.CanParse(secLandingSpeed) && RCUtils.CheckRange(float.Parse(secLandingSpeed), 0.1f, 300)) { GUILayout.Label("Wanted touchdown speed (m/s):", skins.label); }
                        else { GUILayout.Label("Wanted touchdown speed (m/s):", redLabel); }
                    }
                    else if (secTypeID == 1)
                    {
                        if (RCUtils.CanParse(secLandingSpeed) && RCUtils.CheckRange(float.Parse(secLandingSpeed), 0.1f, 300)) { GUILayout.Label("Wanted speed at full deployment (m/s):", skins.label); }
                        else { GUILayout.Label("Wanted speed at full deployment (m/s):", redLabel); }
                    }
                    else
                    {
                        if (RCUtils.CanParse(secLandingSpeed) && RCUtils.CheckRange(float.Parse(secLandingSpeed), 0.1f, 300)) { GUILayout.Label("Planned landing speed (m/s):", skins.label); }
                        else { GUILayout.Label("Planned landing speed (m/s):", redLabel); }
                    }
                    GUILayout.FlexibleSpace();
                    secLandingSpeed = GUILayout.TextField(secLandingSpeed, 10, skins.textField, GUILayout.Width(100));
                    GUILayout.EndHorizontal();

                    if (secTypeID == 2)
                    {
                        CreateEntryArea("Wanted deceleration (m/s²):", ref secDeceleration, 0.1f, 100, 100);
                    }

                    if (secTypeID == 1)
                    {
                        CreateEntryArea("Mains planned deployment alt (m):", ref secRefDepAlt, 10, 45000, 100);
                    }

                    CreateEntryArea("Number of parachutes (parts):", ref secChuteCount, 1, 100, 100);
                }
                #endregion

                #region Manual
                else
                {
                    //Predeployed diameter
                    CreateEntryArea("Predeployed diameter (m):", ref secPreDepDiam, 0.5f, secModel.maxDiam / 2, 100);
                    if (RCUtils.CanParse(secPreDepDiam)) { GUILayout.Label("Resulting area: " + RCUtils.GetArea(float.Parse(secPreDepDiam)).ToString("0.00") + "m²", skins.label); }
                    else { GUILayout.Label("Resulting predeployed area: --- m²", skins.label); }

                    //Deployed diameter
                    CreateEntryArea("Deployed diameter (m):", ref secDepDiam, 1, secModel.maxDiam, 100);
                    if (RCUtils.CanParse(secDepDiam)) { GUILayout.Label("Resulting area: " + RCUtils.GetArea(float.Parse(secDepDiam)).ToString("0.00") + "m²", skins.label); }
                    else { GUILayout.Label("Resulting deployed area: --- m²", skins.label); }
                }
                #endregion

                GUILayout.EndScrollView();
                #endregion

                #region Specific
                //Pressure/alt toggle
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                if (GUILayout.Toggle(secIsPressure, "Pressure predeployment", skins.toggle)) { secIsPressure = true; }
                GUILayout.FlexibleSpace();
                if (GUILayout.Toggle(!secIsPressure, "Altitude predeployment", skins.toggle)) { secIsPressure = false; }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                if (isPressure)
                {
                    if (RCUtils.CanParse(secPredepClause) && RCUtils.CheckRange(float.Parse(secPredepClause), 0.0001f, 5f)) { GUILayout.Label("Predeployment pressure (atm):", skins.label); }
                    else { GUILayout.Label("Predeployment pressure (atm):", redLabel); }
                }
                else
                {
                    if (RCUtils.CanParse(secPredepClause) && RCUtils.CheckRange(float.Parse(secPredepClause), 25, 50000)) { GUILayout.Label("Predeployment altitude (m):", skins.label); }
                    else { GUILayout.Label("Predeployment altitude (m):", redLabel); }
                }
                GUILayout.FlexibleSpace();
                secPredepClause = GUILayout.TextField(secPredepClause, 10, skins.textField, GUILayout.MaxWidth(150));
                GUILayout.EndHorizontal();

                //Deployment altitude
                CreateEntryArea("Deployment altitude", ref secDeploymentAlt, 10, 45000);

                //Cut altitude
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                if (RCUtils.CanParseWithEmpty(secCutAlt) && RCUtils.CheckRange(RCUtils.ParseWithEmpty(secCutAlt), -1, 45000)) { GUILayout.Label("Autocut altitude (m):", skins.label); }
                else { GUILayout.Label("Autocut altitude (m):", redLabel); }
                GUILayout.FlexibleSpace();
                secCutAlt = GUILayout.TextField(secCutAlt, 10, skins.textField, GUILayout.MaxWidth(150));
                GUILayout.EndHorizontal();

                //Predeployment speed
                CreateEntryArea("Pre deployment speed (s):", ref secPreDepSpeed, 0, 5f);

                //Deployment speed
                CreateEntryArea("Deployment speed (s):", ref secDepSpeed, 1, 10);
                #endregion
            }
            #endregion

            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();

            #region Application
            GUILayout.Space(5);
            if (GUILayout.Button("Apply settings", skins.button))
            {
                Apply();
            }

            if (part.symmetryCounterparts.Count > 0)
            {
                if (GUILayout.Button("Apply to all symmetry counterparts", skins.button))
                {
                    Apply(true);
                }
            }
            #endregion

            GUILayout.EndVertical();
        }

        private void Materials(int id)
        {
            GUI.DragWindow(new Rect(0, 0, materialsWindow.width, 25));
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            materialsScroll = GUILayout.BeginScrollView(materialsScroll, false, false, skins.horizontalScrollbar, skins.verticalScrollbar, skins.box, GUILayout.MaxHeight(165));
            materialsID = GUILayout.SelectionGrid(materialsID, materials.materials.Values.ToArray(), 1, skins.button);
            GUILayout.EndScrollView();
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Drag coefficient:", skins.label);
            GUILayout.Label(materials.materials.Keys.ToArray()[materialsID].dragCoefficient.ToString("0.###"), skins.label);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Area density:", skins.label);
            GUILayout.Label(materials.materials.Keys.ToArray()[materialsID].areaDensity * 1000 + "kg/m²", skins.label);
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Choose material", skins.button))
            {
                material = materials.materials.Keys.ToArray()[materialsID];
                this.materialsVisible = false;
            }
            if (GUILayout.Button("Cancel", skins.button))
            {
                this.materialsVisible = false;
            }
            GUILayout.EndVertical();

            
        }

        private void SecMaterials(int id)
        {
            GUI.DragWindow(new Rect(0, 0, secMaterialsWindow.width, 25));
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            secMaterialsScroll = GUILayout.BeginScrollView(secMaterialsScroll, false, false, skins.horizontalScrollbar, skins.verticalScrollbar, skins.box, GUILayout.MaxHeight(165));
            secMaterialsID = GUILayout.SelectionGrid(secMaterialsID, materials.materials.Values.ToArray(), 1, skins.button);
            GUILayout.EndScrollView();
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Drag coefficient:", skins.label);
            GUILayout.Label(materials.materials.Keys.ToArray()[materialsID].dragCoefficient.ToString("0.###"), skins.label);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Area density:", skins.label);
            GUILayout.Label(materials.materials.Keys.ToArray()[materialsID].areaDensity * 1000 + "kg/m²", skins.label);
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Choose material", skins.button))
            {
                secMaterial = materials.materials.Keys.ToArray()[materialsID];
                this.secMaterialsVisible = false;
            }
            if (GUILayout.Button("Cancel", skins.button))
            {
                this.secMaterialsVisible = false;
            }
            GUILayout.EndVertical();
        }

        private void ApplicationFailed(int id)
        {
            GUILayout.Label("Some parameters could not be applied", skins.label);

            failedScroll = GUILayout.BeginScrollView(failedScroll, false, false, skins.horizontalScrollbar, skins.verticalScrollbar, skins.box, GUILayout.MaxHeight(200));
            CreateErrors();
            GUILayout.EndScrollView();

            if (GUILayout.Button("Close", skins.button))
            {
                this.failedVisible = false;
            }
        }

        private void ApplicationSucceeded(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("The application of the parameters succeeded!", skins.label);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (warning)
            {
                GUILayout.Label("Warning: The mass of the craft was too high and the parachutes have been set at their limit. Please review the stats to make sure no problem may occur.", redLabel);
            }

            if (GUILayout.Button("Close", skins.button))
            {
                this.successfulVisible = false;
            }
        }
        #endregion
    }
}
