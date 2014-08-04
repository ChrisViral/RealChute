using System.Collections.Generic;
using System.Linq;
using RealChute.Extensions;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more information contact me on the forum. */

namespace RealChute.Libraries
{
    public class ModelConfig
    {
        public class ModelParameters
        {
            #region Propreties
            private string _modelURL = string.Empty;
            /// <summary>
            /// The string URL of the GameObject
            /// </summary>
            public string modelURL
            {
                get { return this._modelURL; }
            }

            private string _transformName = string.Empty;
            /// <summary>
            /// The name of the parachute transform
            /// </summary>
            public string transformName
            {
                get { return this._transformName; }
            }

            private string _preDepAnim = string.Empty;
            /// <summary>
            /// The name of the predeployment animation
            /// </summary>
            public string preDepAnim
            {
                get { return this._preDepAnim; }
            }

            private string _depAnim = string.Empty;
            /// <summary>
            /// The name of the deployment animation
            /// </summary>
            public string depAnim
            {
                get { return this._depAnim; }
            }
            #endregion

            #region Constructor
            /// <summary>
            /// Creates a new ModelParameters
            /// </summary>
            /// <param name="node">ConfigNode to get the values from</param>
            public ModelParameters(ConfigNode node)
            {
                node.TryGetValue("modelURL", ref _modelURL);
                node.TryGetValue("transformName", ref _transformName);
                node.TryGetValue("preDepAnim", ref _preDepAnim);
                node.TryGetValue("depAnim", ref _depAnim);
            }
            #endregion
        }

        #region Propreties
        private string _name = string.Empty;
        /// <summary>
        /// Name of the model
        /// </summary>
        public string name
        {
            get { return this._name; }
        }

        private float _diameter = 10f;
        /// <summary>
        /// Diameter of the parachutes at 1, 1, 1
        /// </summary>
        public float diameter
        {
            get { return this._diameter; }
        }

        private int _count = 1;
        /// <summary>
        /// Number of parachutes visually per transform
        /// </summary>
        public int count
        {
            get { return this._count; }
        }

        private float _maxDiam = 70;
        /// <summary>
        /// Maximum diameter this parachute can have
        /// </summary>
        public float maxDiam
        {
            get { return this._maxDiam; }
        }

        private List<ModelParameters> _parameters = new List<ModelParameters>();
        /// <summary>
        /// Parameters for all potential chutes to be used with this model
        /// </summary>
        public List<ModelParameters> parameters
        {
            get { return this._parameters; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an empty ModelConfig
        /// </summary>
        public ModelConfig() { }

        /// <summary>
        /// Creates a ModelConfig from the given ConfigNode
        /// </summary>
        /// <param name="node">ConfigNode to get the values from</param>
        public ModelConfig(ConfigNode node)
        {
            node.TryGetValue("name", ref _name);
            node.TryGetValue("diameter", ref _diameter);
            node.TryGetValue("count", ref _count);
            node.TryGetValue("maxDiam", ref _maxDiam);
            _parameters = new List<ModelParameters>(node.GetNodes("PARAMETERS").Select(n => new ModelParameters(n)));
        }
        #endregion
    }
}
