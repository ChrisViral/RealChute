using UnityEngine;

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
        private readonly Vector3 size = Vector3.one;
        /// <summary>
        /// Size of the parachute
        /// </summary>
        public Vector3 Size
        {
            get { return this.size; }
        }

        private readonly string sizeId = string.Empty;
        /// <summary>
        /// Identifier for this size
        /// </summary>
        public string SizeId
        {
            get { return this.sizeId; }
        }

        private readonly float caseMass = 0.1f;
        /// <summary>
        /// Case mass for this size
        /// </summary>
        public float CaseMass
        {
            get { return this.caseMass; }
        }

        private readonly Vector3 topNode = Vector3.zero;
        /// <summary>
        /// Position of the top node if any
        /// </summary>
        public Vector3 TopNode
        {
            get { return this.topNode; }
        }

        private readonly int topNodeSize = -1;
        /// <summary>
        /// Size of the top node if any
        /// </summary>
        public int TopNodeSize
        {
            get { return this.topNodeSize; }
        }

        private readonly Vector3 bottomNode = Vector3.zero;
        /// <summary>
        /// Position of the bottom node if any
        /// </summary>
        public Vector3 BottomNode
        {
            get { return this.bottomNode; }
        }

        private readonly int bottomNodeSize = -1;
        /// <summary>
        /// Size of the bottom node if any
        /// </summary>
        public int BottomNodeSize
        {
            get { return this.bottomNodeSize; }
        }

        private readonly float cost = 400;
        /// <summary>
        /// The cost of this case
        /// </summary>
        public float Cost
        {
            get { return this.cost; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new SizeNode from the given ConfigNode
        /// </summary>
        /// <param name="node">ConfigNode to create the object from</param>
        public SizeNode(ConfigNode node)
        {
            node.TryGetValue("size", ref this.size);
            node.TryGetValue("sizeID", ref this.sizeId);
            node.TryGetValue("caseMass", ref this.caseMass);
            node.TryGetValue("topNode", ref this.topNode);
            node.TryGetValue("topNodeSize", ref this.topNodeSize);
            node.TryGetValue("bottomNode", ref this.bottomNode);
            node.TryGetValue("bottomNodeSize", ref this.bottomNodeSize);
            node.TryGetValue("cost", ref this.cost);
        }
        #endregion
    }
}
