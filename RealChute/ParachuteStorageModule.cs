using System;
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
        #endregion

        #region Fields
        public List<IParachute> parachutes = new List<IParachute>();
        public ConfigNode node = new ConfigNode();
        #endregion

        #region Methods
        private void LoadParachutes()
        {
            if (this.parachutes.Count > 0 && !this.node.HasNode()) { return; }
            this.parachutes = new List<IParachute>();
            foreach (ConfigNode n in this.node.nodes)
            {
                switch(n.name)
                {
                    case "EVA":
                        {
                            this.parachutes.Add(new EVAChute(n));
                            break;
                        }
                    case "SPARE":
                        {
                            this.parachutes.Add(new SpareChute(n));
                            break;
                        }
                    default:
                        break;
                }
                if (this.availableSpace < 0)
                {
                    this.parachutes.RemoveLast();
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
    }
}
