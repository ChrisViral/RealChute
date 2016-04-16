using System.Collections.Generic;
using System.Linq;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute.Libraries.TextureLibrary
{
    public class ModelConfig
    {
        public class ModelParameters
        {
            #region Propreties
            private readonly string modelURL = string.Empty;
            /// <summary>
            /// The string URL of the GameObject
            /// </summary>
            public string ModelURL
            {
                get { return this.modelURL; }
            }

            private readonly string transformName = string.Empty;
            /// <summary>
            /// The name of the parachute transform
            /// </summary>
            public string TransformName
            {
                get { return this.transformName; }
            }

            private readonly string preDepAnim = string.Empty;
            /// <summary>
            /// The name of the predeployment animation
            /// </summary>
            public string PreDepAnim
            {
                get { return this.preDepAnim; }
            }

            private readonly string depAnim = string.Empty;
            /// <summary>
            /// The name of the deployment animation
            /// </summary>
            public string DepAnim
            {
                get { return this.depAnim; }
            }
            #endregion

            #region Constructor
            /// <summary>
            /// Creates a new ModelParameters
            /// </summary>
            /// <param name="node">ConfigNode to get the values from</param>
            public ModelParameters(ConfigNode node)
            {
                node.TryGetValue("modelURL", ref this.modelURL);
                node.TryGetValue("transformName", ref this.transformName);
                node.TryGetValue("preDepAnim", ref this.preDepAnim);
                node.TryGetValue("depAnim", ref this.depAnim);
            }
            #endregion
        }

        #region Propreties
        private readonly string name = string.Empty;
        /// <summary>
        /// Name of the model
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        private readonly float diameter = 10;
        /// <summary>
        /// Diameter of the parachutes at 1, 1, 1
        /// </summary>
        public float Diameter
        {
            get { return this.diameter; }
        }

        private readonly int count = 1;
        /// <summary>
        /// Number of parachutes visually per transform
        /// </summary>
        public int Count
        {
            get { return this.count; }
        }

        private readonly float maxDiam = 70;
        /// <summary>
        /// Maximum diameter this parachute can have
        /// </summary>
        public float MaxDiam
        {
            get { return this.maxDiam; }
        }

        private readonly List<ModelParameters> parameters = new List<ModelParameters>();
        /// <summary>
        /// Parameters for all potential chutes to be used with this model
        /// </summary>
        public List<ModelParameters> Parameters
        {
            get { return this.parameters; }
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
            node.TryGetValue("name", ref this.name);
            node.TryGetValue("diameter", ref this.diameter);
            node.TryGetValue("count", ref this.count);
            node.TryGetValue("maxDiam", ref this.maxDiam);
            this.parameters = new List<ModelParameters>(node.GetNodes("PARAMETERS").Select(n => new ModelParameters(n)));
        }
        #endregion
    }
}
