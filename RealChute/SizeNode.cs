using UnityEngine;
using RealChute.Extensions;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more information contact me on the forum. */

namespace RealChute
{
    public class SizeNode
    {
        #region Propreties
        private Vector3 _size = Vector3.one;
        /// <summary>
        /// Size of the parachute
        /// </summary>
        public Vector3 size
        {
            get { return this._size; }
        }

        private string _sizeID = string.Empty;
        /// <summary>
        /// Identifier for this size
        /// </summary>
        public string sizeID
        {
            get { return this._sizeID; }
        }

        private float _caseMass = 0.1f;
        /// <summary>
        /// Case mass for this size
        /// </summary>
        public float caseMass
        {
            get { return this._caseMass; }
        }

        private Vector3 _topNode = Vector3.zero;
        /// <summary>
        /// Position of the top node if any
        /// </summary>
        public Vector3 topNode
        {
            get { return this._topNode; }
        }

        private int _topNodeSize = 0;
        /// <summary>
        /// Size of the top node if any
        /// </summary>
        public int topNodeSize
        {
            get { return this._topNodeSize; }
        }

        private Vector3 _bottomNode = Vector3.zero;
        /// <summary>
        /// Position of the bottom node if any
        /// </summary>
        public Vector3 bottomNode
        {
            get { return this._bottomNode; }
        }

        private int _bottomNodeSize = 0;
        /// <summary>
        /// Size of the bottom node if any
        /// </summary>
        public int bottomNodeSize
        {
            get { return this._bottomNodeSize; }
        }

        private float _cost = 400;
        /// <summary>
        /// The cost of this case
        /// </summary>
        public float cost
        {
            get { return this._cost; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new SizeNode from the given ConfigNode
        /// </summary>
        /// <param name="node">ConfigNode to create the object from</param>
        public SizeNode(ConfigNode node)
        {
            node.TryGetValue("size", ref _size);
            node.TryGetValue("sizeID", ref _sizeID);
            node.TryGetValue("caseMass", ref _caseMass);
            node.TryGetValue("topNode", ref _topNode);
            node.TryGetValue("topNodeSize", ref _topNodeSize);
            node.TryGetValue("bottomNode", ref _bottomNode);
            node.TryGetValue("bottomNodeSize", ref _bottomNodeSize);
            node.TryGetValue("cost", ref _cost);
        }
        #endregion
    }
}
