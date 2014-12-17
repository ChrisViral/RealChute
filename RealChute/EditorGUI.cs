using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RealChute.Extensions;
using RealChute.Libraries;

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
    public class EditorGUI
    {
        #region Propreties
        private Part part
        {
            get { return this.pChute.part; }
        }

        private RealChuteModule rcModule
        {
            get { return this.pChute.rcModule; }
        }

        private List<ChuteTemplate> chutes
        {
            get { return pChute.chutes; }
        }

        private PresetsLibrary presets
        {
            get { return pChute.presets; }
        }

        private List<SizeNode> sizes
        {
            get { return pChute.sizes; }
        }
        #endregion

        #region Fields
        private ProceduralChute pChute = null;
        private GUISkin skins = HighLogic.Skin;
        internal Rect window = new Rect(), failedWindow = new Rect(), successfulWindow = new Rect();
        internal Rect presetsWindow = new Rect(), presetsSaveWindow = new Rect(), presetsWarningWindow = new Rect();
        private int mainId = Guid.NewGuid().GetHashCode(), failedId = Guid.NewGuid().GetHashCode(), successId = Guid.NewGuid().GetHashCode();
        private int presetId = Guid.NewGuid().GetHashCode(), presetSaveId = Guid.NewGuid().GetHashCode(), presetWarningId = Guid.NewGuid().GetHashCode();
        internal int matX = 500, matY = 370;
        private Vector2 mainScroll = new Vector2(), failedScroll = new Vector2();
        private Vector2 presetScroll = new Vector2();
        internal string presetName = string.Empty, presetDescription = string.Empty;
        internal bool warning = false;
        internal bool visible = false, failedVisible = false, successfulVisible = false;
        private bool presetVisible = false, presetSaveVisible = false, presetWarningVisible = false, saveWarning = false;
        internal string[] cases = new string[] { }, canopies = new string[] { }, models = new string[] { };
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new blank EditorGUI object
        /// </summary>
        public EditorGUI() { }

        /// <summary>
        /// Creates a new RCEditorGUI
        /// </summary>
        /// <param name="pChute">ProceduralChute module to create the object from</param>
        public EditorGUI(ProceduralChute pChute)
        {
            this.pChute = pChute;
        }
        #endregion

        #region Methods
        //Renders the RealChute editor GUI
        internal void RenderGUI()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (this.visible)
                {
                    this.window = GUILayout.Window(this.mainId, this.window, Window, "RealChute Parachute Editor " + RCUtils.assemblyVersion, skins.window, GUILayout.MaxWidth(420), GUILayout.MaxHeight(Screen.height - 375));
                }
                foreach (ChuteTemplate chute in chutes)
                {
                    if (chute.materialsVisible)
                    {
                        chute.materialsWindow = GUILayout.Window(chute.matId, chute.materialsWindow, chute.MaterialsWindow, "Parachute material", skins.window, GUILayout.MaxWidth(375), GUILayout.MaxHeight(275));
                    }
                }
                if (this.failedVisible)
                {
                    this.failedWindow = GUILayout.Window(this.failedId, this.failedWindow, ApplicationFailed, "Error", skins.window, GUILayout.MaxWidth(300), GUILayout.MaxHeight(300));
                }
                if (this.successfulVisible)
                {
                    this.successfulWindow = GUILayout.Window(this.successId, this.successfulWindow, ApplicationSucceeded, "Success", skins.window, GUILayout.MaxWidth(300), GUILayout.MaxHeight(200), GUILayout.ExpandHeight(true));
                }
                if (this.presetVisible)
                {
                    this.presetsWindow = GUILayout.Window(this.pChute.presetID, this.presetsWindow, Presets, "Presets", skins.window, GUILayout.MaxWidth(400), GUILayout.MaxHeight(500));
                }
                if (this.presetSaveVisible)
                {
                    this.presetsSaveWindow = GUILayout.Window(this.presetSaveId, this.presetsSaveWindow, SavePreset, "Save as preset", skins.window, GUILayout.MaxWidth(350), GUILayout.MaxHeight(400));
                }
                if (this.presetWarningVisible)
                {
                    this.presetsWarningWindow = GUILayout.Window(this.presetWarningId, this.presetsWarningWindow, PresetWarning, "Warning", skins.window, GUILayout.Width(200), GUILayout.Height(100));
                }
            }
        }

        //Creates a label + text field
        internal void CreateEntryArea(string label, ref string value, float min, float max, float width = 150)
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (RCUtils.CanParse(value) && RCUtils.CheckRange(float.Parse(value), min, max)) { GUILayout.Label(label, skins.label); }
            else { GUILayout.Label(label, RCUtils.redLabel); }
            GUILayout.FlexibleSpace();
            value = GUILayout.TextField(value, 10, skins.textField, GUILayout.Width(width));
            GUILayout.EndHorizontal();
        }

        //Main GUI window
        private void Window(int id)
        {
            GUILayout.BeginVertical();

            #region Info labels
            StringBuilder builder = new StringBuilder();
            builder.Append("Selected part: ").AppendLine(this.part.partInfo.title);
            builder.Append("Symmetry counterparts: ").AppendLine(this.part.symmetryCounterparts.Count.ToString());
            builder.Append("Case mass: ").Append(rcModule.caseMass.ToString()).Append("t");
            if (sizes.Count > 0) { builder.Append("\t\t\t\t\tCase cost: ").Append(this.sizes[pChute.size].cost.ToString()).Append("f"); }
            builder.Append("\nTotal part mass: ").Append(this.part.TotalMass().ToString("0.###")).Append("t");
            builder.Append("\t\t\tTotal case part cost: ").Append(this.part.TotalCost().ToString("0.#")).Append("f");
            GUILayout.Label(builder.ToString(), skins.label);
            #endregion

            #region Presets
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select a preset", skins.button)) { this.presetVisible = !this.presetVisible; }

            if (GUILayout.Button("Save as preset...", skins.button)) { this.presetSaveVisible = !this.presetSaveVisible; }
            GUILayout.EndHorizontal();
            #endregion

            mainScroll = GUILayout.BeginScrollView(mainScroll, false, false, skins.horizontalScrollbar, skins.verticalScrollbar);

            #region Planet selector
            GUILayout.Space(10);
            GUILayout.BeginHorizontal(GUILayout.Height(30));
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Target planet:", skins.label);
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            pChute.planets = GUILayout.SelectionGrid(pChute.planets, pChute.bodies.bodyNames, 4, skins.button, GUILayout.Width(250));
            GUILayout.EndHorizontal();
            pChute.body = pChute.bodies.GetBody(pChute.planets);
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
                if (GUILayout.Button("Previous size", skins.button, GUILayout.Width(125)))
                {
                    pChute.size--;
                    if (pChute.size < 0) { pChute.size = sizes.Count - 1; }
                }
                if (GUILayout.Button("Next size", skins.button, GUILayout.Width(125)))
                {
                    pChute.size++;
                    if (pChute.size > sizes.Count - 1) { pChute.size = 0; }
                }
                GUILayout.EndHorizontal();
            }
            #endregion

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            #region Texture selectors
            GUILayout.Space(5);
            chutes[0].TextureSelector();
            #endregion

            #region General
            //Materials editor
            GUILayout.Space(5);
            chutes[0].MaterialsSelector();

            //MustGoDown
            GUILayout.Space(5);
            GUILayout.BeginHorizontal(GUILayout.MaxWidth(window.width));
            GUILayout.Label("Must go down to deploy:", skins.label);
            if (GUILayout.Toggle(pChute.mustGoDown, "True", skins.toggle)) { pChute.mustGoDown = true; }
            GUILayout.FlexibleSpace();
            if (GUILayout.Toggle(!pChute.mustGoDown, "False", skins.toggle)) { pChute.mustGoDown = false; }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            //DeployOnGround
            GUILayout.Space(5);
            GUILayout.BeginHorizontal(GUILayout.MaxWidth(window.width));
            GUILayout.Label("Deploy on ground contact:", skins.label);
            if (GUILayout.Toggle(pChute.deployOnGround, "True", skins.toggle)) { pChute.deployOnGround = true; }
            GUILayout.FlexibleSpace();
            if (GUILayout.Toggle(!pChute.deployOnGround, "False", skins.toggle)) { pChute.deployOnGround = false; }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            //Timer
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (RCUtils.CanParseTime(pChute.timer) && RCUtils.CheckRange(RCUtils.ParseTime(pChute.timer), 0, 3600)) { GUILayout.Label("Deployment timer:", skins.label); }
            else { GUILayout.Label("Deployment timer:", RCUtils.redLabel); }
            GUILayout.FlexibleSpace();
            pChute.timer = GUILayout.TextField(pChute.timer, 10, skins.textField, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            //Spares
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (RCUtils.CanParseWithEmpty(pChute.spares) && RCUtils.CheckRange(RCUtils.ParseWithEmpty(pChute.spares), -1, 10) && RCUtils.IsWholeNumber(RCUtils.ParseWithEmpty(pChute.spares))) { GUILayout.Label("Spare chutes:", skins.label); }
            else { GUILayout.Label("Spare chutes:", RCUtils.redLabel); }
            GUILayout.FlexibleSpace();
            pChute.spares = GUILayout.TextField(pChute.spares, 10, skins.textField, GUILayout.Width(150));
            GUILayout.EndHorizontal();

            //CutSpeed
            CreateEntryArea("Autocut speed (m/s):", ref pChute.cutSpeed, 0.01f, 100);

            //LandingAlt
            CreateEntryArea("Landing alt (m):", ref pChute.landingAlt, 0, (float)pChute.body.GetMaxAtmosphereAltitude());
            #endregion

            #region Main
            //Indicator label
            GUILayout.Space(10);
            GUILayout.Label("________________________________________________", RCUtils.boldLabel);
            GUILayout.Label("Main chute:", RCUtils.boldLabel, GUILayout.Width(150));
            GUILayout.Label("‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾", RCUtils.boldLabel);

            chutes[0].Calculations();
            #endregion

            #region Secondary
            if (pChute.secondaryChute)
            {
                for (int i = 1; i < chutes.Count; i++)
                {
                    ChuteTemplate chute = chutes[i];

                    //Indicator label
                    GUILayout.Space(10);
                    GUILayout.Label("________________________________________________", RCUtils.boldLabel);
                    GUILayout.Label(RCUtils.ParachuteNumber(i) + ":", RCUtils.boldLabel, GUILayout.Width(150));
                    GUILayout.Label("‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾", RCUtils.boldLabel);

                    #region Texture selectors
                    GUILayout.Space(5);
                    chute.TextureSelector();
                    #endregion

                    //Materials editor
                    GUILayout.Space(5);
                    chute.MaterialsSelector();

                    chute.Calculations();
                }
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
                pChute.Apply(false);
            }

            if (part.symmetryCounterparts.Count > 0)
            {
                if (GUILayout.Button("Apply to all symmetry counterparts", skins.button))
                {
                    pChute.Apply(true);
                }
            }
            #endregion

            GUILayout.EndVertical();
        }

        //Failure notice
        private void ApplicationFailed(int id)
        {
            GUILayout.Label("Some parameters could not be applied", skins.label);
            GUILayout.Label("Invalid parameters:", skins.label);
            GUILayout.Space(10);
            failedScroll = GUILayout.BeginScrollView(failedScroll, false, false, skins.horizontalScrollbar, skins.verticalScrollbar, skins.box, GUILayout.MaxHeight(200));
            pChute.CreateErrors();
            GUILayout.EndScrollView();
            if (GUILayout.Button("Close", skins.button))
            {
                this.failedVisible = false;
            }
        }

        //Success notice
        private void ApplicationSucceeded(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("The application of the parameters succeeded!", skins.label);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (warning)
            {
                GUILayout.Label("Warning: The mass of the craft was too high and the parachutes have been set at their limit. Please review the stats to make sure no problem may occur.", RCUtils.redLabel);
            }

            if (GUILayout.Button("Close", skins.button))
            {
                this.successfulVisible = false;
            }
        }

        //Presets selection window
        private void Presets(int id)
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            presetScroll = GUILayout.BeginScrollView(presetScroll, false, false, skins.horizontalScrollbar, skins.verticalScrollbar, skins.box, GUILayout.Width(200));
            if (presets.GetRelevantPresets(pChute).Length > 0) { pChute.presetID = GUILayout.SelectionGrid(pChute.presetID, presets.GetRelevantPresets(pChute), 1, skins.button); }
            else { GUILayout.Label("No saved presets", skins.label); }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(200));
            if (presets.GetRelevantPresets(pChute).Length > 0) { GUILayout.Label("Description: " + presets.GetPreset(presets.GetRelevantPresets(pChute)[pChute.presetID]).description, skins.label); }
            else { GUILayout.Label("---", skins.label); }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (presets.presets.Count > 0)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Select preset", skins.button))
                {
                    pChute.ApplyPreset();
                    this.presetVisible = false;
                }

                if (GUILayout.Button("Delete preset", skins.button))
                {
                    saveWarning = false;
                    this.presetWarningVisible = true;
                }
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Cancel", skins.button)) { this.presetVisible = false; }
            GUILayout.EndVertical();
        }

        //Presets saving window
        private void SavePreset(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Preset name:", skins.label);
            presetName = GUILayout.TextField(presetName, skins.textField);
            GUILayout.Label("Preset description", skins.label);
            presetDescription = GUILayout.TextArea(presetDescription, skins.textArea, GUILayout.Height(100));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save...", skins.button))
            {
                if (presetName == string.Empty) { PopupDialog.SpawnPopupDialog("Error!", "Preset name cannot be empty!", "Close", false, skins); }
                else if (presets.presetNames.Any(n => n == presetName)) { this.presetWarningVisible = true; saveWarning = true; }
                else if ((pChute.GetErrors("general").Count != 0 || pChute.GetErrors("main").Count != 0 || (pChute.secondaryChute && pChute.GetErrors("secondary").Count != 0))) { this.failedVisible = true; }
                else
                {
                    pChute.CreatePreset();
                    this.presetSaveVisible = false;
                }
            }
            if (GUILayout.Button("Cancel", skins.button)) { this.presetSaveVisible = false; }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        //Preset saving confirmation window
        private void PresetWarning(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.Label(saveWarning ? "Warning: there is already a preset saved under this name. Are you sure you wish to proceed?" : "Are you sure you wish to delete this preset?", RCUtils.redLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Yes", skins.button))
            {
                Preset preset = saveWarning ? presets.GetPreset(presetName) : presets.GetPreset(presets.GetRelevantPresets(pChute)[pChute.presetID]);
                Debug.Log("[RealChute]: Deleting the \"" + preset.name + "\" preset from the database.");
                presets.DeletePreset(preset);
                if (saveWarning) { pChute.CreatePreset(); this.presetSaveVisible = false; }
                else { pChute.presetID = 0; }
                this.presetWarningVisible = false;
            }
            if (GUILayout.Button("No", skins.button)) { this.presetWarningVisible = false; }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        #endregion
    }
}
