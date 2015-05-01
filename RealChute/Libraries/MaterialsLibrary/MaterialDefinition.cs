using RealChute.Extensions;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute.Libraries
{
    public class MaterialDefinition
    {
        #region Propreties
        private string _name = string.Empty;
        /// <summary>
        /// Name of the material
        /// </summary>
        public string name
        {
            get { return this._name; }
        }

        private string _description = "We don't know much about this, it might as well be made of fishnets";
        /// <summary>
        /// The description of this material
        /// </summary>
        public string description
        {
            get { return this._description; }
        }

        private float _areaDensity = 5.65E-5f;
        /// <summary>
        /// Area density of this material
        /// </summary>
        public float areaDensity
        {
            get { return this._areaDensity; }
        }

        private float _dragCoefficient = 1;
        /// <summary>
        /// Drag coefficient of this material
        /// </summary>
        public float dragCoefficient
        {
            get { return this._dragCoefficient; }
        }

        private float _areaCost = 0.075f;
        /// <summary>
        /// Cost of a square meter of this material
        /// </summary>
        public float areaCost
        {
            get { return this._areaCost; }
        }

        private double _maxTemp = 493.15;
        /// <summary>
        /// Maximum temperature this material can withstand (K)
        /// </summary>
        public double maxTemp
        {
            get { return this._maxTemp; }
        }

        private double _specificHeat = 1700;
        /// <summary>
        /// The specific heat of the material (J/kg*K)
        /// </summary>
        public double specificHeat
        {
            get { return this._specificHeat; }
        }

        private double _emissivity = 0.72;
        /// <summary>
        /// The emissivity constant of the chute at (20°C)
        /// </summary>
        public double emissivity
        {
            get { return this._emissivity; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an empty material definition
        /// </summary>
        public MaterialDefinition() { }

        /// <summary>
        /// Creates a material definition from a config node
        /// </summary>
        /// <param name="node">Node to initiate the material from</param>
        public MaterialDefinition(ConfigNode node)
        {
            node.TryGetValue("name", ref this._name);
            node.TryGetValue("description", ref this._description);
            node.TryGetValue("areaDensity", ref this._areaDensity);
            node.TryGetValue("dragCoefficient", ref this._dragCoefficient);
            node.TryGetValue("areaCost", ref this._areaCost);
            node.TryGetValue("maxTemp", ref this._maxTemp);
            node.TryGetValue("specificHeat", ref this._specificHeat);
            node.TryGetValue("emissivity", ref this._emissivity);
        }
        #endregion
    }
}
