using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RealChute.Libraries;
using RealChute.Extensions;

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
    public class TemplateGUI
    {
        #region Properties
        private ProceduralChute pChute
        {
            get { return this.template.pChute; }
        }

        private Parachute parachute
        {
            get { return this.template.parachute; }
        }

        private EditorGUI editorGUI
        {
            get { return this.template.editorGUI; }
        }

        private bool secondary
        {
            get { return this.template.secondary; }
        }

        private MaterialDefinition material
        {
            get { return this.template.material; }
            set { this.template.material = value; }
        }

        private ModelConfig model
        {
            get { return this.template.model; }
        }

        private CelestialBody body
        {
            get { return this.template.body; }
        }

        public List<string> errors
        {
            get
            {
                List<string> main = new List<string>();
                if (calcSelect)
                {
                    if (!getMass && !RCUtils.CanParse(mass) || !RCUtils.CheckRange(float.Parse(mass), 0.1f, 10000)) { main.Add("Craft mass"); }
                    if (!RCUtils.CanParse(landingSpeed) || ((typeID == 1 && !RCUtils.CheckRange(float.Parse(landingSpeed), 0.1f, 5000)) || (typeID != 1 && !RCUtils.CheckRange(float.Parse(landingSpeed), 0.1f, 300)))) { main.Add("Landing speed"); }
                    if (typeID == 2 && !RCUtils.CanParse(deceleration) || !RCUtils.CheckRange(float.Parse(deceleration), 0.1f, 100)) { main.Add("Wanted deceleration"); }
                    if (typeID == 1 && !RCUtils.CanParse(refDepAlt) || !RCUtils.CheckRange(float.Parse(refDepAlt), 10, (float)body.GetMaxAtmosphereAltitude())) { main.Add("Mains planned deployment alt"); }
                    if (!RCUtils.CanParse(chuteCount) || !RCUtils.CheckRange(float.Parse(chuteCount), 1, 100)) { main.Add("Parachute count"); }
                }
                else
                {
                    if (!RCUtils.CanParse(preDepDiam) || !RCUtils.CheckRange(float.Parse(preDepDiam), 0.5f, model.maxDiam / 2)) { main.Add("Predeployed diameter"); }
                    if (!RCUtils.CanParse(depDiam) || !RCUtils.CheckRange(float.Parse(depDiam), 1, model.maxDiam)) { main.Add("Deployed diameter"); }
                }
                if (!RCUtils.CanParse(predepClause) || (isPressure && !RCUtils.CheckRange(float.Parse(predepClause), 0.0001f, (float)body.GetPressureASL())) || (!isPressure && !RCUtils.CheckRange(float.Parse(predepClause), 10, (float)body.GetMaxAtmosphereAltitude())))
                {
                    if (isPressure) { main.Add("Predeployment pressure"); }
                    else { main.Add("Predeployment altitude"); }
                }
                if (!RCUtils.CanParse(deploymentAlt) || !RCUtils.CheckRange(float.Parse(deploymentAlt), 10, (float)body.GetMaxAtmosphereAltitude())) { main.Add("Deployment altitude"); }
                if (!RCUtils.CanParseEmpty(cutAlt) || !RCUtils.CheckRange(RCUtils.ParseEmpty(cutAlt), -1, (float)body.GetMaxAtmosphereAltitude())) { main.Add("Autocut altitude"); }
                if (!RCUtils.CanParse(preDepSpeed) || !RCUtils.CheckRange(float.Parse(preDepSpeed), 0.5f, 5)) { main.Add("Predeployment speed"); }
                if (!RCUtils.CanParse(depSpeed) || !RCUtils.CheckRange(float.Parse(depSpeed), 1, 10)) { main.Add("Deployment speed"); }
                return main;
            }
        }
        #endregion

        #region Fields
        private ChuteTemplate template = null;
        private GUISkin skins = HighLogic.Skin;
        internal Rect materialsWindow = new Rect();
        internal int matId = Guid.NewGuid().GetHashCode();
        internal bool materialsVisible = false;
        internal Vector2 parachuteScroll = new Vector2(), materialsScroll = new Vector2();
        public int chuteID = -1, modelID = -1, materialsID = 0;
        public int typeID = -1, lastTypeID = 0;
        public bool isPressure = false, calcSelect = true;
        public bool getMass = true, useDry = true;
        public string preDepDiam = string.Empty, depDiam = string.Empty, predepClause = string.Empty;
        public string mass = "10", landingSpeed = "6", deceleration = "10", refDepAlt = "700", chuteCount = "1";
        public string deploymentAlt = string.Empty, landingAlt = "0", cutAlt = string.Empty;
        public string preDepSpeed = string.Empty, depSpeed = string.Empty;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an empty Template GUI
        /// </summary>
        public TemplateGUI() { }

        /// <summary>
        /// Generates a new TemplateGUI from the given ChuteTemplate
        /// </summary>
        /// <param name="template"></param>
        public TemplateGUI(ChuteTemplate template)
        {
            this.template = template;
        }
        #endregion

        #region Methods
        //Type switchup
        internal void SwitchType()
        {
            if (lastTypeID != typeID)
            {
                switch (typeID)
                {
                    case 0:
                        {
                            landingSpeed = "6";
                            deploymentAlt = "700";
                            predepClause = isPressure ? "0.01" : "25000";
                            preDepSpeed = "2";
                            depSpeed = "6";
                            break;
                        }

                    case 1:
                        {
                            landingSpeed = "80";
                            deploymentAlt = "2500";
                            predepClause = isPressure ? "0.007" : "30000";
                            preDepSpeed = "1";
                            depSpeed = "3";
                            break;
                        }

                    case 2:
                        {
                            landingSpeed = "100";
                            deploymentAlt = "10";
                            predepClause = isPressure ? "0.5" : "50";
                            preDepSpeed = "1";
                            depSpeed = "2";
                            break;
                        }
                    default:
                        break;
                }
                lastTypeID = typeID;
            }
        }

        //Texture selector GUI code
        internal void TextureSelector()
        {
            if ((!secondary && this.pChute.TextureEntries("case").Length > 1) || this.pChute.TextureEntries("chute").Length > 1 || this.pChute.TextureEntries("model").Length > 1)
            {
                int m = 0;
                if (!secondary && this.pChute.TextureEntries("case").Length > 1) { m++; }
                if (this.pChute.TextureEntries("chute").Length > 1) { m++; }
                if (this.pChute.TextureEntries("model").Length > 1) { m++; }

                GUILayout.BeginHorizontal();

                #region Labels
                GUILayout.BeginVertical(GUILayout.Height(35 * m));

                //Labels
                if (!secondary && this.pChute.TextureEntries("case").Length > 1)
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Case texture:", skins.label);
                }
                if (this.pChute.TextureEntries("chute").Length > 1)
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Chute texture:", skins.label);
                }
                if (this.pChute.TextureEntries("model").Length > 1)
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
                if (!secondary && this.pChute.TextureEntries("case").Length > 1)
                {
                    GUILayout.FlexibleSpace();
                    this.pChute.caseID = GUILayout.SelectionGrid(this.pChute.caseID, this.pChute.TextureEntries("case"), this.pChute.TextureEntries("case").Length, skins.button);
                }

                if (this.pChute.TextureEntries("chute").Length > 1)
                {
                    GUILayout.FlexibleSpace();
                    chuteID = GUILayout.SelectionGrid(chuteID, this.pChute.TextureEntries("chute"), this.pChute.TextureEntries("chute").Length, skins.button);
                }

                if (this.pChute.TextureEntries("model").Length > 1)
                {
                    GUILayout.FlexibleSpace();
                    modelID = GUILayout.SelectionGrid(modelID, this.pChute.TextureEntries("model"), this.pChute.TextureEntries("model").Length, skins.button);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                #endregion

                GUILayout.EndHorizontal();
            }
        }

        //Materials selector GUI code
        internal void MaterialsSelector()
        {
            if (this.pChute.materials.count > 1)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(20));
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Current material: " + material.name, skins.label);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Change material", skins.button, GUILayout.Width(150)))
                {
                    this.materialsVisible = !this.materialsVisible;
                }
                GUILayout.EndHorizontal();
            }
        }

        //Calculations GUI core
        internal void Calculations()
        {
            #region Calculations
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

                if (getMass)
                {
                    GUILayout.Label("Currently using " + (useDry ? "dry mass" : "wet mass"), skins.label);
                    if (useDry) { if (GUILayout.Button("Switch to wet mass", skins.button, GUILayout.Width(125))) { useDry = false; } }
                    else { if (GUILayout.Button("Switch to dry mass", skins.button, GUILayout.Width(125))) { useDry = true; } }
                }

                else
                {
                    string m = mass;
                    this.editorGUI.CreateEntryArea("Mass to use (t):", ref m, 0.1f, 10000, 100);
                    mass = m;
                }

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                if (typeID == 0)
                {
                    if (RCUtils.CanParse(landingSpeed) && RCUtils.CheckRange(float.Parse(landingSpeed), 0.1f, 300)) { GUILayout.Label("Wanted touchdown speed (m/s):", skins.label); }
                    else { GUILayout.Label("Wanted touchdown speed (m/s):", RCUtils.redLabel); }
                }
                else if (typeID == 1)
                {
                    if (RCUtils.CanParse(landingSpeed) && RCUtils.CheckRange(float.Parse(landingSpeed), 0.1f, 5000)) { GUILayout.Label("Wanted speed at target alt (m/s):", skins.label); }
                    else { GUILayout.Label("Wanted speed at target alt (m/s):", RCUtils.redLabel); }
                }
                else
                {
                    if (RCUtils.CanParse(landingSpeed) && RCUtils.CheckRange(float.Parse(landingSpeed), 0.1f, 300)) { GUILayout.Label("Planned landing speed (m/s):", skins.label); }
                    else { GUILayout.Label("Planned landing speed (m/s):", RCUtils.redLabel); }
                }
                GUILayout.FlexibleSpace();
                landingSpeed = GUILayout.TextField(landingSpeed, 10, skins.textField, GUILayout.Width(100));
                GUILayout.EndHorizontal();

                if (typeID == 2)
                {
                    string decel = deceleration;
                    this.editorGUI.CreateEntryArea("Wanted deceleration (m/s²):", ref decel, 0.1f, 100, 100);
                    deceleration = decel;
                }

                if (typeID == 1)
                {
                    string depAlt = refDepAlt;
                    this.editorGUI.CreateEntryArea("Target altitude (m):", ref depAlt, 10, (float)body.GetMaxAtmosphereAltitude(), 100);
                    refDepAlt = depAlt;
                }

                string chutes = chuteCount;
                this.editorGUI.CreateEntryArea("Parachutes used (parts):", ref chutes, 1, 100, 100);
                chuteCount = chutes;
            }
            #endregion

            #region Manual
            else
            {
                string preDep = preDepDiam, dep = depDiam;
                float diam;
                if (!float.TryParse(dep, out diam)) { diam = model.maxDiam / 2; }

                //Predeployed diameter
                this.editorGUI.CreateEntryArea("Predeployed diameter (m):", ref preDep, 0.5f, diam, 100);
                if (RCUtils.CanParse(preDepDiam)) { GUILayout.Label("Resulting area: " + RCUtils.GetArea(float.Parse(preDepDiam)).ToString("0.00") + "m²", skins.label); }
                else { GUILayout.Label("Resulting predeployed area: --- m²", skins.label); }

                //Deployed diameter
                this.editorGUI.CreateEntryArea("Deployed diameter (m):", ref dep, 1, model.maxDiam, 100);
                if (RCUtils.CanParse(depDiam)) { GUILayout.Label("Resulting area: " + RCUtils.GetArea(float.Parse(depDiam)).ToString("0.00") + "m²", skins.label); }
                else { GUILayout.Label("Resulting deployed area: --- m²", skins.label); }
                preDepDiam = preDep;
                depDiam = dep;
            }
            #endregion

            GUILayout.EndScrollView();
            #endregion

            #region Specific
            //Pressure/alt toggle
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(isPressure, "Pressure predeployment", skins.toggle))
            {
                if (!isPressure)
                {
                    isPressure = true;
                    this.predepClause = this.parachute.minPressure.ToString();
                }
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Toggle(!isPressure, "Altitude predeployment", skins.toggle))
            {
                if (isPressure)
                {
                    isPressure = false;
                    this.predepClause = this.parachute.minDeployment.ToString();
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (isPressure)
            {
                if (RCUtils.CanParse(predepClause) && RCUtils.CheckRange(float.Parse(predepClause), 0.0001f, (float)body.GetPressureASL())) { GUILayout.Label("Predeployment pressure (atm):", skins.label); }
                else { GUILayout.Label("Predeployment pressure (atm):", RCUtils.redLabel); }
            }
            else
            {
                if (RCUtils.CanParse(predepClause) && RCUtils.CheckRange(float.Parse(predepClause), 10, (float)body.GetMaxAtmosphereAltitude())) { GUILayout.Label("Predeployment altitude (m):", skins.label); }
                else { GUILayout.Label("Predeployment altitude (m):", RCUtils.redLabel); }
            }
            GUILayout.FlexibleSpace();
            predepClause = GUILayout.TextField(predepClause, 10, skins.textField, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            //Deployment altitude
            this.editorGUI.CreateEntryArea("Deployment altitude", ref deploymentAlt, 10, (float)body.GetMaxAtmosphereAltitude());

            //Cut altitude
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (RCUtils.CanParseEmpty(cutAlt) && RCUtils.CheckRange(RCUtils.ParseEmpty(cutAlt), -1, (float)body.GetMaxAtmosphereAltitude())) { GUILayout.Label("Autocut altitude (m):", skins.label); }
            else { GUILayout.Label("Autocut altitude (m):", RCUtils.redLabel); }
            GUILayout.FlexibleSpace();
            cutAlt = GUILayout.TextField(cutAlt, 10, skins.textField, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            //Predeployment speed
            this.editorGUI.CreateEntryArea("Pre deployment speed (s):", ref preDepSpeed, 0.5f, 5);

            //Deployment speed
            this.editorGUI.CreateEntryArea("Deployment speed (s):", ref depSpeed, 1, 10);
            #endregion
        }

        //Materials window GUI code
        internal void MaterialsWindow(int id)
        {
            GUI.DragWindow(new Rect(0, 0, materialsWindow.width, 25));
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            materialsScroll = GUILayout.BeginScrollView(materialsScroll, false, false, skins.horizontalScrollbar, skins.verticalScrollbar, skins.box, GUILayout.MaxHeight(200), GUILayout.Width(140));
            materialsID = GUILayout.SelectionGrid(materialsID, this.pChute.materials.materialNames, 1, skins.button);
            GUILayout.EndScrollView();
            GUILayout.BeginVertical();
            MaterialDefinition material = this.pChute.materials.GetMaterial(materialsID);
            StringBuilder builder = new StringBuilder();
            builder.Append("Description:  ").AppendLine(material.description);
            builder.Append("\nDrag coefficient:  ").AppendLine(material.dragCoefficient.ToString("0.00#"));
            builder.Append("\nArea density:  ").Append(material.areaDensity * 1000).AppendLine("kg/m²\n");
            builder.Append("Area cost:  ").Append(material.areaCost.ToString()).Append("f/m²");
            GUILayout.Label(builder.ToString(), skins.label);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Choose material", skins.button, GUILayout.Width(150)))
            {
                this.material = material;
                this.materialsVisible = false;
            }
            if (GUILayout.Button("Cancel", skins.button, GUILayout.Width(150)))
            {
                this.materialsVisible = false;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        #endregion
    }
}