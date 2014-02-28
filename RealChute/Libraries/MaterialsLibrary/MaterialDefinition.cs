using RealChute.Extensions;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more informtion contact me on the forum. */

namespace RealChute
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

        private float _areaDensity = 0.00005f;
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
            node.TryGetValue("name", ref _name);
            node.TryGetValue("areaDensity", ref _areaDensity);
            node.TryGetValue("dragCoefficient", ref _dragCoefficient);
        }
        #endregion
    }
}
