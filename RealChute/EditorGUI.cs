﻿using System;
 using System.Collections.Generic;
 using System.Text;
 using RealChute.Extensions;
 using RealChute.Libraries;
 using RealChute.Libraries.Presets;
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
    public class EditorGUI
    {
        #region Propreties
        private Part Part => this.pChute.part;

        private RealChuteModule RCModule => this.pChute.rcModule;

        private List<ChuteTemplate> Chutes => this.pChute.chutes;

        private List<SizeNode> Sizes => this.pChute.sizes;
        #endregion

        #region Fields
        private readonly ProceduralChute pChute;
        internal Rect window, failedWindow, successfulWindow;
        internal Rect presetsWindow, presetsSaveWindow, presetsWarningWindow;
        private readonly int mainId = Guid.NewGuid().GetHashCode(), failedId = Guid.NewGuid().GetHashCode(), successId = Guid.NewGuid().GetHashCode();
        private readonly int presetSaveId = Guid.NewGuid().GetHashCode(), presetWarningId = Guid.NewGuid().GetHashCode();
        internal int matX = 500, matY = 370;
        private Vector2 mainScroll, failedScroll;
        private Vector2 presetScroll;
        internal string presetName = string.Empty, presetDescription = string.Empty;
        internal bool warning = false;
        internal bool visible = false, failedVisible, successfulVisible;
        private bool presetVisible, presetSaveVisible, presetWarningVisible, saveWarning;
        internal string[] cases = new string[0], canopies = new string[0], models = new string[0];
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
            if (!HighLogic.LoadedSceneIsEditor) { return; }

            GUI.skin = HighLogic.Skin;
            if (this.visible)
            {
                this.window = GUILayout.Window(this.mainId, this.window, Window, "RealChute Parachute Editor " + RCUtils.AssemblyVersion, GUILayout.MaxWidth(420), GUILayout.MaxHeight(Screen.height - 375));
            }
            foreach (ChuteTemplate chute in this.Chutes)
            {
                TemplateGUI gui = chute.templateGUI;
                if (gui.materialsVisible)
                {
                    gui.materialsWindow = GUILayout.Window(gui.matId, gui.materialsWindow, gui.MaterialsWindow, "Parachute material", GUILayout.MaxWidth(375), GUILayout.MaxHeight(275));
                }
            }
            if (this.failedVisible)
            {
                this.failedWindow = GUILayout.Window(this.failedId, this.failedWindow, ApplicationFailed, "Error", GUILayout.MaxWidth(300), GUILayout.MaxHeight(300));
            }
            if (this.successfulVisible)
            {
                this.successfulWindow = GUILayout.Window(this.successId, this.successfulWindow, ApplicationSucceeded, "Success", GUILayout.MaxWidth(300), GUILayout.MaxHeight(200), GUILayout.ExpandHeight(true));
            }
            if (this.presetVisible)
            {
                this.presetsWindow = GUILayout.Window(this.pChute.presetId, this.presetsWindow, Presets, "Presets", GUILayout.MaxWidth(400), GUILayout.MaxHeight(500));
            }
            if (this.presetSaveVisible)
            {
                this.presetsSaveWindow = GUILayout.Window(this.presetSaveId, this.presetsSaveWindow, SavePreset, "Save as preset", GUILayout.MaxWidth(350), GUILayout.MaxHeight(400));
            }
            if (this.presetWarningVisible)
            {
                this.presetsWarningWindow = GUILayout.Window(this.presetWarningId, this.presetsWarningWindow, PresetWarning, "Warning", GUILayout.Width(200), GUILayout.Height(100));
            }
        }

        //Main GUI window
        private void Window(int id)
        {
            GUILayout.BeginVertical();

            #region Info labels
            //Header labels
            StringBuilder builder = new StringBuilder();
            builder.Append("Selected part: ").AppendLine(this.Part.partInfo.title);
            builder.Append("Symmetry counterparts: ").AppendLine(this.Part.symmetryCounterparts.Count.ToString());
            builder.Append("Case mass: ").Append(this.RCModule.caseMass.ToString("0.000")).Append("t");
            if (this.Sizes.Count > 0) { builder.Append("\t\tCase cost: ").Append(this.Sizes[this.pChute.size].Cost.ToString("0.#")).Append("F"); }
            builder.Append("\nTotal part mass: ").Append(this.Part.TotalMass().ToString("0.000")).Append("t");
            builder.Append("\tTotal part cost: ").Append(this.Part.TotalCost().ToString("0.#")).Append("F");
            GUILayout.Label(builder.ToString());
            #endregion

            #region Presets
            //Presets buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select a preset")) { this.presetVisible = !this.presetVisible; }

            if (GUILayout.Button("Save as preset...")) { this.presetSaveVisible = !this.presetSaveVisible; }
            GUILayout.EndHorizontal();
            #endregion

            //Scroll being
            this.mainScroll = GUILayout.BeginScrollView(this.mainScroll);

            #region Planet selector
            //Target planet selection
            GUILayout.Space(10);
            GUILayout.BeginHorizontal(GUILayout.Height(30));
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Target planet:");
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            this.pChute.planets = GUILayout.SelectionGrid(this.pChute.planets, AtmoPlanets.Instance.BodyNames, 4, GUILayout.Width(250));
            GUILayout.EndHorizontal();
            this.pChute.body = AtmoPlanets.Instance.GetBody(this.pChute.planets);
            #endregion

            #region Size cyclers
            //Size selection
            if (this.Sizes.Count > 0)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(20));
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Cycle part size");
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Previous size", GUILayout.Width(125)))
                {
                    this.pChute.size--;
                    if (this.pChute.size < 0) { this.pChute.size = this.Sizes.Count - 1; }
                }
                if (GUILayout.Button("Next size", GUILayout.Width(125)))
                {
                    this.pChute.size++;
                    if (this.pChute.size > this.Sizes.Count - 1) { this.pChute.size = 0; }
                }
                GUILayout.EndHorizontal();
            }
            #endregion

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            //Main chute texture selector
            GUILayout.Space(5);
            this.Chutes[0].templateGUI.TextureSelector();

            #region General
            //Materials editor
            GUILayout.Space(5);
            this.Chutes[0].templateGUI.MaterialsSelector();

            //MustGoDown
            GuiUtils.CreateTwinToggle("Must go down to deploy:", ref this.pChute.mustGoDown, this.window.width);

            //DeployOnGround
            GuiUtils.CreateTwinToggle("Deploy on ground contact:", ref this.pChute.deployOnGround, this.window.width);

            //Timer
            GuiUtils.CreateTimeEntryArea("Deployment timer:", ref this.pChute.timer, 0, 3600);

            //Spares
            GuiUtils.CreateEmptyEntryArea("Spare chutes:", ref this.pChute.spares, -1, 10);

            //CutSpeed
            GuiUtils.CreateEntryArea("Autocut speed (m/s):", ref this.pChute.cutSpeed, 0.01f, 100);

            //LandingAlt
            GuiUtils.CreateEntryArea("Landing alt (m):", ref this.pChute.landingAlt, 0, (float)this.pChute.body.GetMaxAtmosphereAltitude());

            // delayBeforeCut
            GuiUtils.CreateEntryArea("Delay before cutting chutes after landing: ", ref this.pChute.delayBeforeCut, 0, 5f);
            #endregion

            #region Main
            //Indicator label
            GUILayout.Space(10);
            GUILayout.Label("________________________________________________", GuiUtils.BoldLabel);
            GUILayout.Label("Main chute:", GuiUtils.BoldLabel, GUILayout.Width(150));
            GUILayout.Label("‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾", GuiUtils.BoldLabel);

            this.Chutes[0].templateGUI.Calculations();
            #endregion

            #region Secondary
            if (this.pChute.secondaryChute)
            {
                for (int i = 1; i < this.Chutes.Count; i++)
                {
                    ChuteTemplate chute = this.Chutes[i];

                    //Indicator label
                    GUILayout.Space(10);
                    GUILayout.Label("________________________________________________", GuiUtils.BoldLabel);
                    GUILayout.Label(RCUtils.ParachuteNumber(i) + ":", GuiUtils.BoldLabel, GUILayout.Width(150));
                    GUILayout.Label("‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾", GuiUtils.BoldLabel);

                    //Texture selector
                    GUILayout.Space(5);
                    chute.templateGUI.TextureSelector();

                    //Materials editor
                    GUILayout.Space(5);
                    chute.templateGUI.MaterialsSelector();

                    chute.templateGUI.Calculations();
                }
            }
            #endregion

            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
            //Scroll end

            #region Application
            GUILayout.Space(5);
            if (GUILayout.Button("Apply settings"))
            {
                this.pChute.Apply(false);
            }

            if (this.Part.symmetryCounterparts.Count > 0)
            {
                if (GUILayout.Button("Apply to all symmetry counterparts"))
                {
                    this.pChute.Apply(true);
                }
            }
            #endregion

            GUILayout.EndVertical();
        }

        //Failure notice
        private void ApplicationFailed(int id)
        {
            GUILayout.Label("Some parameters could not be applied\nInvalid parameters:");
            GUILayout.Space(10);
            this.failedScroll = GUILayout.BeginScrollView(this.failedScroll, false, false, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.box, GUILayout.MaxHeight(200));
            this.pChute.CreateErrors();
            GUILayout.EndScrollView();
            if (GUILayout.Button("Close"))
            {
                this.failedVisible = false;
            }
        }

        //Success notice
        private void ApplicationSucceeded(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("The application of the parameters succeeded!");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (this.warning)
            {
                GUILayout.Label("Warning: The mass of the craft was too high and the parachutes have been set at their limit. Please review the stats to make sure no problem may occur.", GuiUtils.RedLabel);
            }

            if (GUILayout.Button("Close"))
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
            this.presetScroll = GUILayout.BeginScrollView(this.presetScroll, false, false, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, GUI.skin.box, GUILayout.Width(200));
            string[] current = PresetsLibrary.Instance.GetRelevantPresets(this.Chutes.Count);
            string p = string.Empty;
            if (current.Length > 0)
            {
                this.pChute.presetId = GUILayout.SelectionGrid(this.pChute.presetId, current, 1, GUI.skin.button);
                p = current[this.pChute.presetId];
            }
            else { GUILayout.Label("No saved presets"); }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.BeginVertical(GUILayout.Width(200));
            if (!string.IsNullOrEmpty(p)) { GUILayout.Label("Description: " + PresetsLibrary.Instance.GetPreset(p).Description); }
            else { GUILayout.Label("---"); }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (PresetsLibrary.Instance.Presets.Count > 0)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Select preset"))
                {
                    this.pChute.ApplyPreset();
                    this.presetVisible = false;
                }

                if (GUILayout.Button("Delete preset"))
                {
                    this.saveWarning = false;
                    this.presetWarningVisible = true;
                }
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Cancel")) { this.presetVisible = false; }
            GUILayout.EndVertical();
        }

        //Presets saving window
        private void SavePreset(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Preset name:");
            this.presetName = GUILayout.TextField(this.presetName);
            GUILayout.Label("Preset description");
            this.presetDescription = GUILayout.TextArea(this.presetDescription, GUILayout.Height(100));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save..."))
            {
                if (this.presetName == string.Empty) { RCUtils.PopupDialog("Error!", "Preset name cannot be empty!", "Close"); }
                else if (PresetsLibrary.Instance.ContainsPreset(this.presetName))
                {
                    this.presetWarningVisible = true;
                    this.saveWarning = true;
                }
                else if (this.pChute.GetErrors(true).Count != 0 || this.pChute.GetErrors(false).Count != 0) { this.failedVisible = true; }
                else
                {
                    this.pChute.CreatePreset();
                    this.presetSaveVisible = false;
                }
            }
            if (GUILayout.Button("Cancel")) { this.presetSaveVisible = false; }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        //Preset saving confirmation window
        private void PresetWarning(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.Label(this.saveWarning ? "Warning: there is already a preset saved under this name. Are you sure you wish to proceed?" : "Are you sure you wish to delete this preset?", GuiUtils.RedLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Yes"))
            {
                Preset preset = this.saveWarning ? PresetsLibrary.Instance.GetPreset(this.presetName) : PresetsLibrary.Instance.GetPreset(this.pChute.presetId, this.pChute.chutes.Count);
                Debug.Log("[RealChute]: Deleting the \"" + preset.Name + "\" preset from the database.");
                PresetsLibrary.Instance.DeletePreset(preset);
                if (this.saveWarning)
                { 
                   this.pChute.CreatePreset();
                   this.presetSaveVisible = false;
                }
                else { this.pChute.presetId = 0; }
                this.presetWarningVisible = false;
            }
            if (GUILayout.Button("No")) { this.presetWarningVisible = false; }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        #endregion
    }
}