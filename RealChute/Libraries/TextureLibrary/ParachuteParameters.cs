using RealChute.Extensions;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more informtion contact me on the forum. */

namespace RealChute
{
    public class ParachuteParameters
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
        /// Creates an empty ParachuteParameters
        /// </summary>
        public ParachuteParameters() { }

        /// <summary>
        /// Creates a new ParachuteParameters
        /// </summary>
        /// <param name="node">ConfigNode to get the values from</param>
        public ParachuteParameters(ConfigNode node)
        {
            node.TryGetValue("modelURL", ref _modelURL);
            node.TryGetValue("transformName", ref _transformName);
            node.TryGetValue("preDepAnim", ref _preDepAnim);
            node.TryGetValue("depAnim", ref _depAnim);
        }
        #endregion
    }
}
