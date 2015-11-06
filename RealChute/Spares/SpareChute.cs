using System;
using System.Collections.Generic;
using System.Linq;
using RealChute.Extensions;
using RealChute.Utils;
using RealChute.Libraries.Materials;
using UnityEngine;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute.Spares
{
    public class SpareChute : IParachute
    {
        public struct Canopy
        {
            #region Properties
            public float deployedArea
            {
                get { return RCUtils.GetArea(this.deployedDiameter); }
            }

            public float mass
            {
                get { return this.material.areaDensity * this.deployedArea; }
            }
            #endregion

            #region Fields
            public float deployedDiameter;
            public MaterialDefinition material;
            #endregion

            #region Constructor
            public Canopy (ConfigNode node)
            {
                float d = 50;
                string m = string.Empty;
                MaterialDefinition mat = MaterialsLibrary.defaultMaterial;
                node.TryGetValue("deployedDiameter", ref d);
                node.TryGetValue("material", ref m);
                MaterialsLibrary.instance.TryGetMaterial(m, ref mat);
                this.deployedDiameter = d;
                this.material = mat;
            }

            public Canopy(Parachute parachute)
            {
                this.deployedDiameter = parachute.deployedDiameter;
                this.material = parachute.mat;
            }

            public Canopy(ParachuteStorageModule.CustomSpare spare)
            {
                this.deployedDiameter = float.Parse(spare.diameter);
                this.material = spare.material;
            }
            #endregion

            #region Methods
            public ConfigNode Save()
            {
                ConfigNode node = new ConfigNode("CANOPY");
                node.AddValue("deployedDiameter", this.deployedDiameter);
                node.AddValue("material", material.name);
                return node;
            }
            #endregion
        }

        #region Properties
        public float deployedArea
        {
            get { return canopies.Sum(c => c.deployedArea); }
        }

        public float chuteMass
        {
            get { return this.canopies.Sum(c => c.mass); }
        }

        private string _name = string.Empty;
        public string name
        {
            get { return this._name + " spare"; }
        }

        private Part _part = null;
        public Part part
        {
            get { return this._part; }
        }

        public Category category
        {
            get { return Category.Spare; }
        }
        #endregion

        #region Fields
        private List<Canopy> canopies = new List<Canopy>();
        #endregion

        #region Constructors
        public SpareChute(ConfigNode node)
        {
            node.TryGetValue("name", ref this._name);
            node.GetNodes("CANOPY").ForEach(n => this.canopies.Add(new Canopy(n)));
        }

        public SpareChute(RealChuteModule module)
        {
            this._part = module.part;
            this._name = this._part.partInfo.title;
            module.parachutes.ForEach(p => this.canopies.Add(new Canopy(p)));
        }

        public SpareChute(SpareChute chute)
        {
            this._name = chute._name; ;
            this.canopies = new List<Canopy>(chute.canopies);
            this._part = chute.part;
        }

        public SpareChute(string name, List<ParachuteStorageModule.CustomSpare> canopies)
        {
            this._name = name;
            canopies.ForEach(c => this.canopies.Add(new Canopy(c)));
        }
        #endregion

        #region Methods
        public ConfigNode Save()
        {
            ConfigNode node = new ConfigNode("SPARE");
            node.AddValue("name", this._name);
            this.canopies.ForEach(c => node.AddNode(c.Save()));
            return node;
        }
        #endregion
    }
}
