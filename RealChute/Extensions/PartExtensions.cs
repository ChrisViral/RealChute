using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more information contact me on the forum. */

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
            if (part.children.Count > 0)
            {
                List<Part> children = new List<Part>(part.children);
                children.ForEach(p => children.AddRange(p.GetAllChildren()));
                return children;
            }
            return new List<Part>();
        }

        /// <summary>
        /// Gets the children transforms of this specific part
        /// </summary>
        public static List<Renderer> GetPartRenderers(this Part part, RealChuteModule module)
        {
            List<Renderer> renderers = new List<Renderer>(part.transform.GetComponentsInChildren<Renderer>());
            if (part.children.Count > 0)
            {
                foreach (Part p in part.children)
                {
                    p.transform.GetComponentsInChildren<Renderer>().Where(r => renderers.Contains(r)).ToList().ForEach(r => renderers.Remove(r));
                }
            }
            module.main.parachute.GetComponents<Renderer>().ToList().ForEach(r => renderers.Remove(r));
            if (module.secondaryChute) { module.secondary.parachute.GetComponents<Renderer>().ToList().ForEach(r => renderers.Remove(r)); }
            return renderers;
        }

        /// <summary>
        /// Returns the total mass of the part
        /// </summary>
        public static float TotalMass(this Part part)
        {
            return part.physicalSignificance != Part.PhysicalSignificance.NONE ? part.mass + part.GetResourceMass() : 0f;
        }

        /// <summary>
        /// Initiates an animation for later use
        /// </summary>
        /// <param name="animationName">Name of the animation</param>
        public static void InitiateAnimation(this Part part, string animationName)
        {
            //Initiates the default values for animations
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
            //Plays the animation
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
        /// Returns if the animation is playing
        /// </summary>
        /// <param name="animationName">Name of the animation to check</param>
        public static bool CheckAnimationPlaying(this Part part, string animationName)
        {
            //Checks if a given animation is playing
            foreach (Animation animation in part.FindModelAnimators(animationName)) { return animation.IsPlaying(animationName); }
            return false;
        }
        #endregion
    }
}
