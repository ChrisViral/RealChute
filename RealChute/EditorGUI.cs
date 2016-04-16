﻿using System;
using System.Text;
using System.Collections.Generic;
﻿using UnityEngine;
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
    public class EditorGui
    {
        #region Propreties
        private Part Part
        {
            get { return this.pChute.part; }
        }

        private RealChuteModule RCModule
        {
            get { return this.pChute.rcModule; }
        }

        private List<ChuteTemplate> Chutes
        {
            get { return this.pChute.chutes; }
        }

        private PresetsLibrary presets
        {
            get { return this.pChute.presets; }
        }

        private List<SizeNode> Sizes
        {
            get { return this.pChute.sizes; }
        }
        #endregion

        #region Fields
        private ProceduralChute pChute;
        private GUISkin skins = HighLogic.Skin;
        internal Rect window, failedWindow, successfulWindow;
        internal Rect presetsWindow, presetsSaveWindow, presetsWarningWindow;
        private int mainId = Guid.NewGuid().GetHashCode(), failedId = Guid.NewGuid().GetHashCode(), successId = Guid.NewGuid().GetHashCode();
        private int presetId = Guid.NewGuid().GetHashCode(), presetSaveId = Guid.NewGuid().GetHashCode(), presetWarningId = Guid.NewGuid().GetHashCode();
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
        public EditorGui() { }

        /// <summary>
        /// Creates a new RCEditorGUI
        /// </summary>
        /// <param name="pChute">ProceduralChute module to create the object from</param>
        public EditorGui(ProceduralChute pChute)
        {
            this.pChute = pChute;
        }
        #endregion

        #region Methods
        //Renders the RealChute editor GUI
        internal void RenderGui()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (this.visible)
                {
                    this.window = GUILayout.Window(this.mainId, this.window, Window, "RealChute Parachute Editor " + RCUtils.AssemblyVersion, this.skins.window, GUILayout.MaxWidth(420), GUILayout.MaxHeight(Screen.height - 375));
                }
                foreach (ChuteTemplate chute in this.Chutes)
                {
                    TemplateGui gui = chute.templateGui;
                    if (gui.materialsVisible)
                    {
                        gui.materialsWindow = GUILayout.Window(gui.matId, gui.materialsWindow, gui.MaterialsWindow, "Parachute material", this.skins.window, GUILayout.MaxWidth(375), GUILayout.MaxHeight(275));
                    }
                }
                if (this.failedVisible)
                {
                    this.failedWindow = GUILayout.Window(this.failedId, this.failedWindow, ApplicationFailed, "Error", this.skins.window, GUILayout.MaxWidth(300), GUILayout.MaxHeight(300));
                }
                if (this.successfulVisible)
                {
                    this.successfulWindow = GUILayout.Window(this.successId, this.successfulWindow, ApplicationSucceeded, "Success", this.skins.window, GUILayout.MaxWidth(300), GUILayout.MaxHeight(200), GUILayout.ExpandHeight(true));
                }
                if (this.presetVisible)
                {
                    this.presetsWindow = GUILayout.Window(this.pChute.presetId, this.presetsWindow, Presets, "Presets", this.skins.window, GUILayout.MaxWidth(400), GUILayout.MaxHeight(500));
                }
                if (this.presetSaveVisible)
                {
                    this.presetsSaveWindow = GUILayout.Window(this.presetSaveId, this.presetsSaveWindow, SavePreset, "Save as preset", this.skins.window, GUILayout.MaxWidth(350), GUILayout.MaxHeight(400));
                }
                if (this.presetWarningVisible)
                {
                    this.presetsWarningWindow = GUILayout.Window(this.presetWarningId, this.presetsWarningWindow, PresetWarning, "Warning", this.skins.window, GUILayout.Width(200), GUILayout.Height(100));
                }
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
            GUILayout.Label(builder.ToString(), this.skins.label);
            #endregion

            #region Presets
            //Presets buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select a preset", this.skins.button)) { this.presetVisible = !this.presetVisible; }

            if (GUILayout.Button("Save as preset...", this.skins.button)) { this.presetSaveVisible = !this.presetSaveVisible; }
            GUILayout.EndHorizontal();
            #endregion

            //Scroll being
            this.mainScroll = GUILayout.BeginScrollView(this.mainScroll, false, false, this.skins.horizontalScrollbar, this.skins.verticalScrollbar);

            #region Planet selector
            //Target planet selection
            GUILayout.Space(10);
            GUILayout.BeginHorizontal(GUILayout.Height(30));
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Target planet:", this.skins.label);
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            this.pChute.planets = GUILayout.SelectionGrid(this.pChute.planets, this.pChute.bodies.BodyNames, 4, this.skins.button, GUILayout.Width(250));
            GUILayout.EndHorizontal();
            this.pChute.body = this.pChute.bodies.GetBody(this.pChute.planets);
            #endregion

            #region Size cyclers
            //Size selection
            if (this.Sizes.Count > 0)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(20));
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                GUILayout.Label("Cycle part size", this.skins.label);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Previous size", this.skins.button, GUILayout.Width(125)))
                {
                    this.pChute.size--;
                    if (this.pChute.size < 0) { this.pChute.size = this.Sizes.Count - 1; }
                }
                if (GUILayout.Button("Next size", this.skins.button, GUILayout.Width(125)))
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
            this.Chutes[0].templateGui.TextureSelector();

            #region General
            //Materials editor
            GUILayout.Space(5);
            this.Chutes[0].templateGui.MaterialsSelector();

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
            #endregion

            #region Main
            //Indicator label
            GUILayout.Space(10);
            GUILayout.Label("________________________________________________", GuiUtils.BoldLabel);
            GUILayout.Label("Main chute:", GuiUtils.BoldLabel, GUILayout.Width(150));
            GUILayout.Label("‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾‾", GuiUtils.BoldLabel);

            this.Chutes[0].templateGui.Calculations();
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
                    chute.templateGui.TextureSelector();

                    //Materials editor
                    GUILayout.Space(5);
                    chute.templateGui.MaterialsSelector();

                    chute.templateGui.Calculations();
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
            if (GUILayout.Button("Apply settings", this.skins.button))
            {
                this.pChute.Apply(false);
            }

            if (this.Part.symmetryCounterparts.Count > 0)
            {
                if (GUILayout.Button("Apply to all symmetry counterparts", this.skins.button))
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
            GUILayout.Label("Some parameters could not be applied\nInvalid parameters:", this.skins.label);
            GUILayout.Space(10);
            this.failedScroll = GUILayout.BeginScrollView(this.failedScroll, false, false, this.skins.horizontalScrollbar, this.skins.verticalScrollbar, this.skins.box, GUILayout.MaxHeight(200));
            this.pChute.CreateErrors();
            GUILayout.EndScrollView();
            if (GUILayout.Button("Close", this.skins.button))
            {
                this.failedVisible = false;
            }
        }

        //Success notice
        private void ApplicationSucceeded(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("The application of the parameters succeeded!", this.skins.label);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (this.warning)
            {
                GUILayout.Label("Warning: The mass of the craft was too high and the parachutes have been set at their limit. Please review the stats to make sure no problem may occur.", GuiUtils.RedLabel);
            }

            if (GUILayout.Button("Close", this.skins.button))
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
            this.presetScroll = GUILayout.BeginScrollView(this.presetScroll, false, false, this.skins.horizontalScrollbar, this.skins.verticalScrollbar, this.skins.box, GUILayout.Width(200));
            string[] current = this.presets.GetRelevantPresets(this.Chutes.Count);
            string p = string.Empty;
            if (current.Length > 0)
            {
                this.pChute.presetId = GUILayout.SelectionGrid(this.pChute.presetId, current, 1, this.skins.button);
                p = current[this.pChute.presetId];
            }
            else { GUILayout.Label("No saved presets", this.skins.label); }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.BeginVertical(GUILayout.Width(200));
            if (!string.IsNullOrEmpty(p)) { GUILayout.Label("Description: " + this.presets.GetPreset(p).Description, this.skins.label); }
            else { GUILayout.Label("---", this.skins.label); }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (this.presets.Presets.Count > 0)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Select preset", this.skins.button))
                {
                    this.pChute.ApplyPreset();
                    this.presetVisible = false;
                }

                if (GUILayout.Button("Delete preset", this.skins.button))
                {
                    this.saveWarning = false;
                    this.presetWarningVisible = true;
                }
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Cancel", this.skins.button)) { this.presetVisible = false; }
            GUILayout.EndVertical();
        }

        //Presets saving window
        private void SavePreset(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Preset name:", this.skins.label);
            this.presetName = GUILayout.TextField(this.presetName, this.skins.textField);
            GUILayout.Label("Preset description", this.skins.label);
            this.presetDescription = GUILayout.TextArea(this.presetDescription, this.skins.textArea, GUILayout.Height(100));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save...", this.skins.button))
            {
                if (this.presetName == string.Empty) { RCUtils.PopupDialog("Error!", "Preset name cannot be empty!", "Close"); }
                else if (this.presets.ContainsPreset(this.presetName))
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
            if (GUILayout.Button("Cancel", this.skins.button)) { this.presetSaveVisible = false; }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        //Preset saving confirmation window
        private void PresetWarning(int id)
        {
            GUILayout.BeginVertical();
            GUILayout.Label(this.saveWarning ? "Warning: there is already a preset saved under this name. Are you sure you wish to proceed?" : "Are you sure you wish to delete this preset?", GuiUtils.RedLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Yes", this.skins.button))
            {
                Preset preset = this.saveWarning ? this.presets.GetPreset(this.presetName) : this.presets.GetPreset(this.pChute.presetId, this.pChute.chutes.Count);
                Debug.Log("[RealChute]: Deleting the \"" + preset.Name + "\" preset from the database.");
                this.presets.DeletePreset(preset);
                if (this.saveWarning)
                { 
                   this.pChute.CreatePreset();
                   this.presetSaveVisible = false;
                }
                else { this.pChute.presetId = 0; }
                this.presetWarningVisible = false;
            }
            if (GUILayout.Button("No", this.skins.button)) { this.presetWarningVisible = false; }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        #endregion
    }
}