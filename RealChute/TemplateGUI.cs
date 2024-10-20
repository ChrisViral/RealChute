using System;
using System.Collections.Generic;
using System.Text;
using RealChute.Extensions;
using RealChute.Libraries.MaterialsLibrary;
using RealChute.Libraries.TextureLibrary;
using UnityEngine;

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
    /// <summary>
    /// Parachute calculation type
    /// </summary>
    public enum ParachuteType
    {
        MAIN,
        DROGUE,
        DRAG
    }

    public class TemplateGUI
    {
        #region Properties
        private ProceduralChute PChute => this.template.pChute;

        private Parachute Parachute => this.template.parachute;

        private bool Secondary => this.template.Secondary;

        private MaterialDefinition Material
        {
            get => this.template.material;
            set => this.template.material = value;
        }

        private ModelConfig Model => this.template.model;

        private CelestialBody Body => this.template.Body;

        public List<string> Errors
        {
            get
            {
                List<string> errors = [];
                float f, max = (float)this.Body.GetMaxAtmosphereAltitude();
                if (this.calcSelect)
                {
                    if (!this.getMass && (!float.TryParse(this.mass, out f) || !GUIUtils.CheckRange(f, 0.1f, 10000))) { errors.Add("Craft mass"); }
                    switch (this.Type)
                    {
                        case ParachuteType.MAIN:
                            {
                                if (!float.TryParse(this.landingSpeed, out f) || !GUIUtils.CheckRange(f, 0.1f, 300)) { errors.Add("Landing speed"); }
                                break;
                            }
                        case ParachuteType.DROGUE:
                            {
                                if (!float.TryParse(this.landingSpeed, out f) && !GUIUtils.CheckRange(f, 0.1f, 5000)) { errors.Add("Landing speed"); }
                                if (!float.TryParse(this.refDepAlt, out f) || !GUIUtils.CheckRange(f, 10, max)) { errors.Add("Mains planned deployment alt"); }
                                break;
                            }
                        case ParachuteType.DRAG:
                            {
                                if (!float.TryParse(this.landingSpeed, out f) || !GUIUtils.CheckRange(f, 0.1f, 300)) { errors.Add("Landing speed"); }
                                if (!float.TryParse(this.deceleration, out f) || !GUIUtils.CheckRange(f, 0.1f, 100)) { errors.Add("Wanted deceleration"); }
                                break;
                            }
                    }
                    if (!float.TryParse(this.chuteCount, out f) || !GUIUtils.CheckRange(f, 1, 100)) { errors.Add("Parachute count"); }
                }
                else
                {
                    if (!float.TryParse(this.preDepDiam, out float p)) { p = 0; }
                    if (!float.TryParse(this.depDiam, out float d)) { d = 0; }
                    if (!GUIUtils.CheckRange(p, 0.5f, d)) { errors.Add("Predeployed diameter"); }
                    if (!GUIUtils.CheckRange(d, 1, this.PChute.textures is null ? this.Parachute.maxDiameter : this.Model.MaxDiam)) { errors.Add("Deployed diameter"); }
                }
                if (!float.TryParse(this.predepClause, out f) || (this.isPressure ? !GUIUtils.CheckRange(f, 0.0001f, (float)this.Body.GetPressureAsl()) : !GUIUtils.CheckRange(f, 10, max)))
                {
                    errors.Add(this.isPressure ? "Predeployment pressure" : "Predeployment altitude");
                }
                if (!float.TryParse(this.deploymentAlt, out f) || !GUIUtils.CheckRange(f, 10, max)) { errors.Add("Deployment altitude"); }
                if (!GUIUtils.TryParseWithEmpty(this.cutAlt, out f) || !GUIUtils.CheckRange(f, -1, max)) { errors.Add("Autocut altitude"); }
                if (!float.TryParse(this.preDepSpeed, out f) || !GUIUtils.CheckRange(f, 0.5f, 5)) { errors.Add("Predeployment speed"); }
                if (!float.TryParse(this.depSpeed, out f) || !GUIUtils.CheckRange(f, 1, 10)) { errors.Add("Deployment speed"); }
                return errors;
            }
        }

        public int TypeId
        {
            get
            {
                if (this.tId == -1) { this.TypeId = 0; }
                return this.tId;
            }
            set
            {
                this.tId = value;
                this.Type= EnumUtils.GetValueAt<ParachuteType>(value);
            }
        }

        public int LastTypeId
        {
            get
            {
                if (this.ltId == -1) { this.LastTypeId = 0; }
                return this.ltId;
            }
            set
            {
                this.ltId = value;
                this.LastType = EnumUtils.GetValueAt<ParachuteType>(value);
            }
        }

        public ParachuteType Type { get; private set; } = ParachuteType.MAIN;

        public ParachuteType LastType { get; private set; } = ParachuteType.MAIN;
        #endregion

        #region Fields
        private readonly ChuteTemplate template;
        internal Rect materialsWindow = new Rect(), drag = new Rect();
        internal int matId = Guid.NewGuid().GetHashCode();
        internal bool materialsVisible;
        internal Vector2 parachuteScroll, materialsScroll;
        public int chuteId = -1, modelId = -1, materialsId;
        private int tId = -1, ltId;
        public bool isPressure, calcSelect = true;
        public bool getMass = true, useDry = true;
        public string preDepDiam = string.Empty, depDiam = string.Empty, predepClause = string.Empty;
        public string mass = "10", landingSpeed = "6", deceleration = "10", refDepAlt = "700", chuteCount = "1";
        public string deploymentAlt = string.Empty, landingAlt = "0", cutAlt = string.Empty;
        public string preDepSpeed = string.Empty, depSpeed = string.Empty;
        private static readonly string[] calculationModes = { "Automatic", "Manual" };
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
            if (this.LastType != this.Type)
            {
                switch (this.Type)
                {
                    case ParachuteType.MAIN:
                        {
                            this.landingSpeed = "6";
                            this.deploymentAlt = "700";
                            this.predepClause = this.isPressure ? "0.01" : "25000";
                            this.preDepSpeed = "2";
                            this.depSpeed = "6";
                            break;
                        }

                    case ParachuteType.DROGUE:
                        {
                            this.landingSpeed = "80";
                            this.deploymentAlt = "2500";
                            this.predepClause = this.isPressure ? "0.007" : "30000";
                            this.preDepSpeed = "1";
                            this.depSpeed = "3";
                            break;
                        }

                    case ParachuteType.DRAG:
                        {
                            this.landingSpeed = "100";
                            this.deploymentAlt = "10";
                            this.predepClause = this.isPressure ? "0.5" : "50";
                            this.preDepSpeed = "1";
                            this.depSpeed = "2";
                            break;
                        }
                }
                this.LastTypeId = this.TypeId;
            }
        }

        //Texture selector GUI code
        internal void TextureSelector()
        {
            string[] cases = this.PChute.TextureEntries(ProceduralChute.SelectorType.CASE), chutes = this.PChute.TextureEntries(ProceduralChute.SelectorType.CHUTE), models = this.PChute.TextureEntries(ProceduralChute.SelectorType.MODEL);
            bool a = false, b = false, c = false;
            int h = 0;
            if (!this.Secondary && cases.Length > 1) { h++; a = true; }
            if (chutes.Length > 1) { h++; b = true; }
            if (models.Length > 1) { h++; c = true; }
            if (h == 0) { return; }

            GUILayout.BeginHorizontal();

            #region Labels
            GUILayout.BeginVertical(GUILayout.Height(35 * h * GameSettings.UI_SCALE));

            //Labels
            if (a)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("Case texture:", GUIUtils.ScaledLabel);
            }
            if (b)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("Chute texture:", GUIUtils.ScaledLabel);
            }
            if (c)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("Chute model: ", GUIUtils.ScaledLabel);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            #endregion

            #region Selectors
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Height(35 * h* GameSettings.UI_SCALE));
            //Boxes
            if (a)
            {
                GUILayout.FlexibleSpace();
                this.PChute.caseId = GUILayout.SelectionGrid(this.PChute.caseId, cases, cases.Length, GUIUtils.ScaledButton);
            }

            if (b)
            {
                GUILayout.FlexibleSpace();
                this.chuteId = GUILayout.SelectionGrid(this.chuteId, chutes, chutes.Length, GUIUtils.ScaledButton);
            }

            if (c)
            {
                GUILayout.FlexibleSpace();
                this.modelId = GUILayout.SelectionGrid(this.modelId, models, models.Length, GUIUtils.ScaledButton);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            #endregion

            GUILayout.EndHorizontal();
        }

        //Materials selector GUI code
        internal void MaterialsSelector()
        {
            if (MaterialsLibrary.Instance.Count >= 1)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(20f * GameSettings.UI_SCALE));
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Current material: " + this.Material.Name, GUIUtils.ScaledLabel);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Change material", GUIUtils.ScaledButton, GUILayout.Width(150f * GameSettings.UI_SCALE)))
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
            GUIUtils.CreateTwinToggle("Calculations mode:", ref this.calcSelect, 300f, calculationModes);
            GUILayout.Space(5f * GameSettings.UI_SCALE);

            //Calculations
            this.parachuteScroll = GUILayout.BeginScrollView(this.parachuteScroll, false, false, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.box, GUILayout.Height(160f * GameSettings.UI_SCALE));
            string label;
            float max, min;

            #region Automatic
            if (this.calcSelect)
            {
                this.TypeId = GUILayout.SelectionGrid(this.TypeId, EnumUtils.GetNames<ParachuteType>(), 3, GUIUtils.ScaledButton);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Toggle(this.getMass, "Use current craft mass", GUIUtils.ScaledToggle, GUILayout.Width(150f * GameSettings.UI_SCALE))) { this.getMass = true; }
                GUILayout.FlexibleSpace();
                if (GUILayout.Toggle(!this.getMass, "Input craft mass", GUIUtils.ScaledToggle, GUILayout.Width(150f * GameSettings.UI_SCALE))) { this.getMass = false; }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                if (this.getMass)
                {
                    GUILayout.Label("Currently using " + (this.useDry ? "dry mass" : "wet mass"), GUIUtils.ScaledLabel);
                    if (GUILayout.Button("Switch to " + (this.useDry ? "wet mass" : "dry mass"), GUIUtils.ScaledButton, GUILayout.Width(175f * GameSettings.UI_SCALE))) { this.useDry = !this.useDry; }
                }

                else
                {
                    GUIUtils.CreateEntryArea("Mass to use (t):", ref this.mass, 0.1f, 10000f, 100f);
                }
                max = 300;
                switch (this.Type)
                {
                    case ParachuteType.MAIN:
                        label = "Wanted touchdown speed (m/s):"; break;
                    case ParachuteType.DROGUE:
                        label = "Wanted speed at target alt (m/s):"; max = 5000f; break;
                    case ParachuteType.DRAG:
                        label = "Planned landing speed (m/s):"; break;
                    default:
                        label = string.Empty; break;
                }
                GUIUtils.CreateEntryArea(label, ref this.landingSpeed, 0.1f, max, 100f);

                if (this.Type == ParachuteType.DROGUE)
                {
                    GUIUtils.CreateEntryArea("Target altitude (m):", ref this.refDepAlt, 10f, (float)this.Body.GetMaxAtmosphereAltitude(), 100f);
                }

                if (this.Type == ParachuteType.DRAG)
                {
                    GUIUtils.CreateEntryArea("Wanted deceleration (m/s²):", ref this.deceleration, 0.1f, 100f, 100f);
                }

                GUIUtils.CreateEntryArea("Parachutes used (parachutes):", ref this.chuteCount, 1f, 100f, 100f);
            }
            #endregion

            #region Manual
            else
            {
                if (!float.TryParse(this.preDepDiam, out float p)) { p = -1; }
                if (!float.TryParse(this.depDiam, out float d)) { d = -1; }

                //Predeployed diameter
                GUIUtils.CreateEntryArea("Predeployed diameter (m):", ref this.preDepDiam, 0.1f, d, 100f);
                if (p != -1) { GUILayout.Label("Resulting area: " + RCUtils.GetArea(p).ToString("0.00") + "m²", GUIUtils.ScaledLabel); }
                else { GUILayout.Label("Resulting predeployed area: --- m²", GUIUtils.ScaledLabel); }

                //Deployed diameter
                GUIUtils.CreateEntryArea("Deployed diameter (m):", ref this.depDiam, p, this.PChute.textures == null ? this.Parachute.maxDiameter : this.Model.MaxDiam, 100f);
                if (d != 1) { GUILayout.Label("Resulting area: " + RCUtils.GetArea(d).ToString("0.00") + "m²", GUIUtils.ScaledLabel); }
                else { GUILayout.Label("Resulting deployed area: --- m²", GUIUtils.ScaledLabel); }
            }
            #endregion

            GUILayout.EndScrollView();
            #endregion

            #region Specific
            //Pressure/alt toggle
            GUILayout.Space(5f * GameSettings.UI_SCALE);
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(this.isPressure, "Pressure predeployment", GUIUtils.ScaledToggle))
            {
                if (!this.isPressure)
                {
                    this.isPressure = true;
                    this.predepClause = this.Parachute.minPressure.ToString();
                }
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Toggle(!this.isPressure, "Altitude predeployment", GUIUtils.ScaledToggle))
            {
                if (this.isPressure)
                {
                    this.isPressure = false;
                    this.predepClause = this.Parachute.minDeployment.ToString();
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            //Pressure/alt selection
            if (this.isPressure)
            {
                label = "Predeployment pressure (atm):";
                min = 0.0001f;
                max = (float)this.Body.GetPressureAsl();
            }
            else
            {
                label = "Predeployment altitude (m):";
                min = 10;
                max = (float)this.Body.GetMaxAtmosphereAltitude();
            }
            GUIUtils.CreateEntryArea(label, ref this.predepClause, min, max);

            //Deployment altitude
            GUIUtils.CreateEntryArea("Deployment altitude", ref this.deploymentAlt, 10f, (float)this.Body.GetMaxAtmosphereAltitude());

            //Cut altitude
            GUIUtils.CreateEmptyEntryArea("Autocut altitude (m):", ref this.cutAlt, -1f, (float)this.Body.GetMaxAtmosphereAltitude());

            //Predeployment speed
            GUIUtils.CreateEntryArea("Pre deployment speed (s):", ref this.preDepSpeed, 0.5f, 5f);

            //Deployment speed
            GUIUtils.CreateEntryArea("Deployment speed (s):", ref this.depSpeed, 1f, 10f);
            #endregion
        }

        //Materials window GUI code
        internal void MaterialsWindow(int id)
        {
            GUI.DragWindow(this.drag);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            this.materialsScroll = GUILayout.BeginScrollView(this.materialsScroll, false, false, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.box, GUILayout.MaxHeight(200f * GameSettings.UI_SCALE), GUILayout.Width(140f * GameSettings.UI_SCALE));
            this.materialsId = GUILayout.SelectionGrid(this.materialsId, MaterialsLibrary.Instance.MaterialNames, 1, GUIUtils.ScaledButton);
            GUILayout.EndScrollView();
            GUILayout.BeginVertical();
            MaterialDefinition material = null;
            if (MaterialsLibrary.Instance.MaterialNames.IndexInRange(this.materialsId))
            {
                string name = MaterialsLibrary.Instance.MaterialNames[this.materialsId];
                MaterialsLibrary.Instance.TryGetMaterial(name, ref material);
            }

            if (material == null)
            {
                material = new MaterialDefinition();
            }

            StringBuilder builder = StringBuilderCache.Acquire();
            builder.Append("Description:  ").AppendLine(material.Description);
            builder.Append("\nDrag coefficient:  ").AppendLine(material.DragCoefficient.ToString("0.00#"));
            builder.Append("\nArea density:  ").Append(material.AreaDensity * 1000).AppendLine("kg/m²");
            builder.Append("\nArea cost:  ").Append(material.AreaCost.ToString()).Append("F/m²");
            builder.Append("\nMax temperature: ").Append((material.MaxTemp + RCUtils.AbsoluteZero).ToString()).Append("°C");
            builder.Append("\nSpecific heat: ").Append(material.SpecificHeat.ToString()).Append("J/kg∙K");
            builder.Append("\nEmissivity constant: ").Append(material.Emissivity.ToString());
            GUILayout.Label(builder.ToStringAndRelease(), GUIUtils.ScaledLabel);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Choose material", GUIUtils.ScaledButton, GUILayout.Width(150f * GameSettings.UI_SCALE)))
            {
                this.Material = material;
                this.materialsVisible = false;
            }
            if (GUILayout.Button("Cancel", GUIUtils.ScaledButton, GUILayout.Width(150f * GameSettings.UI_SCALE)))
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