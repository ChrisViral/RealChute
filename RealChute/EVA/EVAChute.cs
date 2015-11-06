using RealChute.Libraries.Materials;
using RealChute.Utils;
using RealChute.Spares;
using RealChute.Extensions;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute.EVA
{
    public class EVAChute : IParachute
    {
        #region Properties
        private string _name = string.Empty;
        /// <summary>
        /// Name of the EVAChute
        /// </summary>
        public string name
        {
            get { return this._name; }
        }

        private string _description = string.Empty;
        /// <summary>
        /// Description of the EVAChute
        /// </summary>
        public string description
        {
            get { return this._description; }
        }

        private ConfigNode _module = null;
        /// <summary>
        /// Reference to the RealChuteEVA module ConfigNode
        /// </summary>
        public ConfigNode module
        {
            get { return this._module; }
        }

        private float _deployedArea = 10;
        /// <summary>
        /// Deployed area of the EVA chute
        /// </summary>
        public float deployedArea
        {
            get { return this._deployedArea; }
        }

        private float _chuteMass = 0.1f;
        /// <summary>
        /// Mass of the parachute canopy
        /// </summary>
        public float chuteMass
        {
            get { return this._chuteMass; }
        }

        /// <summary>
        /// Indicates this is an EVA chute
        /// </summary>
        public Category category
        {
            get { return Category.EVA; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initiates the EVAChute object
        /// </summary>
        /// <param name="node">ConfigNode to create the object from</param>
        public EVAChute(ConfigNode node)
        {
            node.TryGetValue("name", ref this._name);
            node.TryGetValue("description", ref this._description);
            if (node.TryGetNode("MODULE", ref this._module))
            {
                float diameter = 0;
                if (module.TryGetValue("deployedDiameter", ref diameter))
                {
                    this._deployedArea = RCUtils.GetArea(diameter);
                }
                MaterialDefinition mat = new MaterialDefinition();
                string material = string.Empty;
                if (module.TryGetValue("material", ref material) && MaterialsLibrary.instance.TryGetMaterial(material, ref mat))
                {
                    this._chuteMass = mat.areaDensity * this._deployedArea;
                }
            }
        }

        /// <summary>
        /// Clones an EVAChute
        /// </summary>
        /// <param name="chute"></param>
        public EVAChute(EVAChute chute)
        {
            this._name = chute._name;
            this._description = chute._description;
            this._module = chute._module;
            this._deployedArea = chute._deployedArea;
            this._chuteMass = chute._chuteMass;
        }
        #endregion

        #region Methods
        public ConfigNode Save()
        {
            ConfigNode node = new ConfigNode("EVA");
            node.AddValue("name", this._name);
            node.AddValue("description", this._description);
            node.AddNode(this.module);
            return node;
        }
        #endregion
    }
}
