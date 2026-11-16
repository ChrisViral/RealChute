using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute.Extensions;

public static class PartExtensions
{
    #region Methods
    /// <param name="part">Part to get the children for</param>
    extension(Part part)
    {
        /// <summary>
        /// Gets all the children of a part and their children
        /// </summary>
        public List<Part> GetAllChildren()
        {
            if (part.children.Count <= 0) { return []; }
            //Thanks to Padishar here
            List<Part> result = [..part.children];
            for (int i = 0; i < result.Count; i++)
            {
                Part p = result[i];
                if (p.children.Count > 0) { result.AddRange(p.children); }
            }
            return result;
        }

        /// <summary>
        /// Sees if the part has the given AttachNode and stores it in the out value. Returns false if the node is null.
        /// </summary>
        /// <param name="nodeId">Name of the node to find</param>
        /// <param name="node">Value to store the result into</param>
        public bool TryGetAttachNodeById(string nodeId, out AttachNode node) => (node = part.FindAttachNode(nodeId)) != null;

        /// <summary>
        /// Gets the children transforms of this specific part
        /// </summary>
        /// <param name="module">RealChuteModule associated to the part</param>
        public IEnumerable<Renderer> GetPartRenderers(RealChuteModule module)
        {
            List<Renderer> toRemove =
            [
                ..part.children.SelectMany(p => p.transform.GetComponentsInChildren<Renderer>()),
                ..module.parachutes.SelectMany(p => p.parachute.GetComponents<Renderer>())
            ];

            return part.transform.GetComponentsInChildren<Renderer>().Except(toRemove);
        }

        /// <summary>
        /// Returns the total mass of the part
        /// </summary>
        public float TotalMass(bool dry = false)
        {
            if (part.physicalSignificance is Part.PhysicalSignificance.NONE) return 0f;

            return dry ? part.mass : part.mass + part.GetResourceMass();
        }

        /// <summary>
        /// Total cost of this part
        /// </summary>
        public float TotalCost() => part.GetModuleCosts(part.partInfo.cost) + part.partInfo.cost;

        /// <summary>
        /// Initiates an animation for later use
        /// </summary>
        /// <param name="animationName">Name of the animation</param>
        public void InitiateAnimation(string animationName)
        {
            foreach (Animation animation in part.FindModelAnimators(animationName))
            {
                AnimationState state = animation[animationName];
                state.normalizedTime  = 0f;
                state.normalizedSpeed = 0f;
                state.enabled         = false;
                state.wrapMode        = WrapMode.Clamp;
                state.layer           = 1;
            }
        }

        /// <summary>
        /// Plays an animation at a given speed
        /// </summary>
        /// <param name="animationName">Name of the animation</param>
        /// <param name="animationSpeed">Speed to play the animation at</param>
        public void PlayAnimation(string animationName, float animationSpeed)
        {
            foreach (Animation animation in part.FindModelAnimators(animationName))
            {
                AnimationState state = animation[animationName];
                state.normalizedTime  = 0f;
                state.normalizedSpeed = animationSpeed;
                state.enabled         = true;
                animation.Play(animationName);
            }
        }

        /// <summary>
        /// Skips directly to the end of the animation
        /// </summary>
        /// <param name="animationName">Name of the animation</param>
        public void SkipToAnimationEnd(string animationName)
        {
            foreach (Animation animation in part.FindModelAnimators(animationName))
            {
                AnimationState state = animation[animationName];
                state.normalizedTime  = 1f;
                state.normalizedSpeed = 1f;
                state.enabled         = true;
                animation.Play(animationName);
            }
        }

        /// <summary>
        /// Returns if the animation is playing
        /// </summary>
        /// <param name="animationName">Name of the animation to check</param>
        public bool CheckAnimationPlaying(string animationName) => part.FindModelAnimators(animationName)
                                                                       .Exists(a => a[animationName].normalizedTime is > 0f and < 1f);
    }
    #endregion
}
