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
            private float _deployedDiameter;
            public float deployedDiameter
            {
                get { return this._deployedDiameter; }
            }

            private float _deployedArea;
            public float deployedArea
            {
                get { return this._deployedArea; }
            }

            private ParachuteMaterial _material;
            public ParachuteMaterial material
            {
                get { return this._material; }
            }

            private float _mass;
            public float mass
            {
                get { return this._mass; }
            }

            private float _cost;
            public float cost
            {
                get { return this._cost; }
            }
            #endregion

            #region Constructor
            public Canopy (ConfigNode node)
            {
                float d = 50;
                string m = "Nylon";
                ParachuteMaterial mat = MaterialsLibrary.defaultMaterial;
                node.TryGetValue("deployedDiameter", ref d);
                node.TryGetValue("material", ref m);
                MaterialsLibrary.instance.TryGetMaterial(m, out mat);
                this._deployedDiameter = d;
                this._material = mat;
                this._deployedArea = RCUtils.GetArea(this._deployedDiameter);
                this._mass = this._deployedArea * this._material.areaDensity;
                this._cost = this._deployedArea * this._material.areaCost;
            }

            public Canopy(Parachute parachute)
            {
                this._deployedDiameter = parachute.deployedDiameter;
                this._deployedArea = parachute.deployedArea;
                this._material = parachute.mat;
                this._mass = parachute.chuteMass;
                this._cost = this._deployedArea * this._material.areaCost;
            }

            public Canopy(SparesStorageModule.CustomSpare spare)
            {
                this._deployedDiameter = float.Parse(spare.diameter);
                this._material = spare.material;
                this._deployedArea = RCUtils.GetArea(this._deployedDiameter);
                this._mass = this._deployedArea * this._material.areaDensity;
                this._cost = this._deployedArea * this._material.areaCost;
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
        private string _name = string.Empty;
        public string name
        {
            get { return this._name; }
        }

        public float deployedArea
        {
            get { return canopies.Sum(c => c.deployedArea); }
        }

        public float chuteMass
        {
            get { return this.canopies.Sum(c => c.mass); }
        }

        public float chuteCost
        {
            get { return this.canopies.Sum(c => c.cost); }
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
        private string diameters = string.Empty;
        private List<Canopy> canopies = new List<Canopy>();
        #endregion

        #region Constructors
        public SpareChute(ConfigNode node)
        {
            Load(node);
        }

        public SpareChute(RealChuteModule module, string name)
        {
            this._part = module.part;
            Update(module, name);
        }

        private SpareChute(SpareChute chute)
        {
            this._name = chute._name; ;
            this.canopies = new List<Canopy>(chute.canopies);
            this._part = chute.part;
            this.diameters = chute.diameters;
        }

        public SpareChute(string name, List<SparesStorageModule.CustomSpare> canopies)
        {
            this._name = name;
            canopies.ForEach(c => this.canopies.Add(new Canopy(c)));
            this.diameters = this.canopies.Select(c => c.deployedDiameter.ToString()).Join("m, ");
        }
        #endregion

        #region Methods
        public void Load(ConfigNode node)
        {
            node.TryGetValue("name", ref this._name);
            node.GetNodes("CANOPY").ForEach(n => this.canopies.Add(new Canopy(n)));
            this.diameters = this.canopies.Select(c => c.deployedDiameter.ToString()).Join("m, ");
        }

        public ConfigNode Save()
        {
            ConfigNode node = new ConfigNode("SPARE");
            node.AddValue("name", this._name);
            this.canopies.ForEach(c => node.AddNode(c.Save()));
            return node;
        }

        public IParachute Clone()
        {
            return new SpareChute(this);
        }

        public string GetInfo()
        {
            return String.Format("Name: {0}\nType: Spare\nDiameters: {1}m\nTotal area: {2}m²\nTotal mass: {3}t\nTotal cost: {4}F", this._name, this.diameters, this.deployedArea, this.chuteMass, this.chuteCost);
        }

        public void Update(RealChuteModule module, string name)
        {
            this._name = name + " spare";
            module.parachutes.ForEach(p => this.canopies.Add(new Canopy(p)));
            this.diameters = this.canopies.Select(c => c.deployedDiameter.ToString()).Join("m, ");
        }
        #endregion
    }
}
