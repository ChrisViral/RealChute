using System.Collections;
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

namespace RealChute.Extensions
{
    public static class PartExtensions
    {
        #region Methods
        /// <summary>
        /// Gets all the children of a part and their children
        /// </summary>
        public static List<Part> GetAllChildren(this Part part)
        {
            if (part.children.Count <= 0) { return new List<Part>(); }
            //Thanks to Padishar here
            List<Part> result = new List<Part>(part.children);
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
        public static bool TryGetAttachNodeById(this Part part, string nodeId, out AttachNode node)
        {
            node = part.findAttachNode(nodeId);
            return node != null;
        }

        /// <summary>
        /// Gets the children transforms of this specific part
        /// </summary>
        public static IEnumerable<Renderer> GetPartRenderers(this Part part, RealChuteModule module)
        {
            List<Renderer> toRemove = new List<Renderer>(part.children.SelectMany(p => p.transform.GetComponentsInChildren<Renderer>()));
            toRemove.AddRange(module.parachutes.SelectMany(p => p.parachute.GetComponents<Renderer>()));
            return part.transform.GetComponentsInChildren<Renderer>().Except(toRemove);
        }

        /// <summary>
        /// Returns the total mass of the part
        /// </summary>
        public static float TotalMass(this Part part)
        {
            return part.physicalSignificance != Part.PhysicalSignificance.NONE ? part.mass + part.GetResourceMass() : 0;
        }

        /// <summary>
        /// Total cost of this part
        /// </summary>
        public static float TotalCost(this Part part)
        {
            float cost = part.partInfo.cost;
            return part.GetModuleCosts(cost) + cost;
        }

        /// <summary>
        /// Initiates an animation for later use
        /// </summary>
        /// <param name="animationName">Name of the animation</param>
        public static void InitiateAnimation(this Part part, string animationName)
        {
            foreach (Animation animation in part.FindModelAnimators(animationName))
            {
                AnimationState state = animation[animationName];
                state.normalizedTime = 0;
                state.normalizedSpeed = 0;
                state.enabled = false;
                state.wrapMode = WrapMode.Clamp;
                state.layer = 1;
            }
        }

        /// <summary>
        /// Plays an animation at a given speed
        /// </summary>
        /// <param name="animationName">Name of the animation</param>
        /// <param name="animationSpeed">Speed to play the animation at</param>
        public static void PlayAnimation(this Part part, string animationName, float animationSpeed)
        {
            foreach (Animation animation in part.FindModelAnimators(animationName))
            {
                AnimationState state = animation[animationName];
                state.normalizedTime = 0;
                state.normalizedSpeed = animationSpeed;
                state.enabled = true;
                animation.Play(animationName);
            }
        }

        /// <summary>
        /// Skips directly to the end of the animation
        /// </summary>
        /// <param name="animationName">Name of the animation</param>
        public static void SkipToAnimationEnd(this Part part, string animationName)
        {
            foreach (Animation animation in part.FindModelAnimators(animationName))
            {
                AnimationState state = animation[animationName];
                state.normalizedTime = 1;
                state.normalizedSpeed = 1;
                state.enabled = true;
                animation.Play(animationName);
            }
        }

        /// <summary>
        /// Skips directly to the end of the animation
        /// </summary>
        /// <param name="animationName">Name of the animation</param>
        public static void SkipToAnimationTime(this Part part, string animationName, float animationSpeed, float animationTime)
        {
            foreach (Animation animation in part.FindModelAnimators(animationName))
            {
                AnimationState state = animation[animationName];
                state.normalizedTime = animationTime;
                state.normalizedSpeed = animationSpeed;
                state.enabled = true;
                animation.Play(animationName);
            }
        }

        /// <summary>
        /// Finds all the PartModules implementing the T interface or module
        /// </summary>
        /// <typeparam name="T">Interface or PartModule type</typeparam>
        public static List<T> FindPartModulesImplementing<T>(this List<Part> parts) where T : class
        {
            List<T> list = new List<T>();
            foreach (Part p in parts)
            {
                foreach (PartModule m in p.Modules)
                {
                    if (m is T) { list.Add(m as T); }
                }
            }
            return list;
        }
        #endregion
    }
}
