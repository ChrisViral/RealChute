using UnityEngine;
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

        private int _topNodeSize = -1;
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

        private int _bottomNodeSize = -1;
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
            node.TryGetValue("size", ref this._size);
            node.TryGetValue("sizeID", ref this._sizeID);
            node.TryGetValue("caseMass", ref this._caseMass);
            node.TryGetValue("topNode", ref this._topNode);
            node.TryGetValue("topNodeSize", ref this._topNodeSize);
            node.TryGetValue("bottomNode", ref this._bottomNode);
            node.TryGetValue("bottomNodeSize", ref this._bottomNodeSize);
            node.TryGetValue("cost", ref this._cost);
        }
        #endregion
    }
}
