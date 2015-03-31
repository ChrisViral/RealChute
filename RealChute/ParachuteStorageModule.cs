using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
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
    public class ParachuteStorageModule : PartModule, IModuleInfo
    {
        #region KSPFields
        [KSPField]
        public float storageSpace = 250;
        #endregion

        #region Properties
        public float usedSpace
        {
            get { return this.parachutes.Sum(p => p.deployedArea); }
        }

        public float availableSpace
        {
            get { return this.storageSpace - this.usedSpace; }
        }

        public float storageMass
        {
            get { return this.parachutes.Sum(p => p.chuteMass); }
        }

        private List<IParachute> _parachutes = new List<IParachute>();
        public List<IParachute> parachutes
        {
            get { return this._parachutes; }
        }
        #endregion

        #region Fields
        //General
        private EVAChuteLibrary lib = EVAChuteLibrary.instance;
        public ConfigNode node = new ConfigNode();

        //GUI
        private GUISkin skins = HighLogic.Skin;
        private Rect window = new Rect(), drag = new Rect();
        private Vector2 scrollAvailable = new Vector2(), scrollStored = new Vector2();
        private int id = Guid.NewGuid().GetHashCode();
        private bool visible = false;
        #endregion

        #region Methods
        private void LoadParachutes()
        {
            if (this.parachutes.Count > 0 && !this.node.HasNode()) { return; }
            this._parachutes = new List<IParachute>();
            foreach (ConfigNode n in this.node.nodes)
            {
                switch(n.name)
                {
                    case "EVA":
                        {
                            this._parachutes.Add(new EVAChute(n));
                            break;
                        }
                    case "SPARE":
                        {
                            this._parachutes.Add(new SpareChute(n));
                            break;
                        }
                    default:
                        break;
                }
                if (this.availableSpace < 0)
                {
                    this._parachutes.RemoveLast();
                    continue;
                }
            }
        }

        public Callback<Rect> GetDrawModulePanelCallback()
        {
            return null;
        }

        public string GetModuleTitle()
        {
            return "Parachute Storage";
        }

        public string GetPrimaryField()
        {
            return String.Format("<b>Storage area:</b> {0}m²", this.storageSpace);
        }
        #endregion

        #region Overrides
        public override void OnStart(PartModule.StartState state)
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }

        }

        public override void OnLoad(ConfigNode node)
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            this.node = node;
            LoadParachutes();
        }

        public override string GetInfo()
        {
            string info = "This part can store spare chutes and EVA chutes.\n";
            info += String.Format("Storage space: {0}m²", this.storageSpace);
            return info;
        }

        public override void OnSave(ConfigNode node)
        {
            if (!CompatibilityChecker.IsAllCompatible()) { return; }
            this.parachutes.ForEach(p => node.AddNode(p.Save()));
        }
        #endregion

        #region GUI
        private void OnGUI()
        {
            if (this.visible)
            {
                GUILayout.Window(this.id, this.window, Window, "Storage Module Selection", this.skins.window);
            }
        }

        private void Window(int id)
        {
            GUI.DragWindow(drag);
            GUILayout.BeginVertical();

            StringBuilder b = new StringBuilder();
            b.AppendFormat("Total storage space: {0}m²", this.storageSpace);

            GUILayout.BeginHorizontal();
            this.scrollAvailable = GUILayout.BeginScrollView(this.scrollAvailable, false, false, this.skins.horizontalScrollbar, this.skins.verticalScrollbar, this.skins.box);

            //Availble chutes

            GUILayout.EndScrollView();

            this.scrollStored = GUILayout.BeginScrollView(this.scrollStored, false, false, this.skins.horizontalScrollbar, this.skins.verticalScrollbar, this.skins.box);

            //Stored chutes

            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();

            //Buttons

            GUILayout.EndVertical();
        }
        #endregion
    }
}
