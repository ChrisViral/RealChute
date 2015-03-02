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
    public static class ConfigNodeExtensions
    {
        #region Methods
        /// <summary>
        /// Sees if the ConfigNode has a named node and stores it in the ref value
        /// </summary>
        /// <param name="name">Name of the node to find</param>
        /// <param name="node">Value to store the result in</param>
        public static bool TryGetNode(this ConfigNode config, string name, ref ConfigNode node)
        {
            if (config.HasNode(name))
            {
                node = config.GetNode(name);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the ConfigNode has the specified value and stores it within the ref. Ref is untouched if value not present.
        /// </summary>
        /// <param name="name">Name of the value searched for</param>
        /// <param name="value">Value to assign</param>
        public static bool TryGetValue(this ConfigNode node, string name, ref string value)
        {
            if (node.HasValue(name))
            {
                value = node.GetValue(name);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the ConfigNode has a value of the given name and stores it as an array within the given ref value. Does not touch the ref if not.
        /// </summary>
        /// <param name="name">Name of the value to look for</param>
        /// <param name="value">Value to store the result in</param>
        public static bool TryGetValue(this ConfigNode node, string name, ref string[] value)
        {
            if (node.HasValue(name))
            {
                value = RCUtils.ParseArray(node.GetValue(name));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the ConfigNode has the specified value and stores it in the ref value if it is parsable. Value is left unchanged if not.
        /// </summary>
        /// <param name="name">Name of the value to searched for</param>
        /// <param name="value">Value to assign</param>
        public static bool TryGetValue(this ConfigNode node, string name, ref float value)
        {
            float result = 0;
            if (node.HasValue(name) && float.TryParse(node.GetValue(name), out result))
            {
                value = result;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the ConfigNode contains the value and sotres it in the ref if it is parsable. Value is left unchanged if not.
        /// </summary>
        /// <param name="name">Name of the value to look for</param>
        /// <param name="value">Value to store the result in</param>
        public static bool TryGetValue(this ConfigNode node, string name, ref int value)
        {
            int result = 0;
            if (node.HasValue(name) && int.TryParse(node.GetValue(name), out result))
            {
                value = result;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the ConfigNode has the specified value and stores it in the ref value if it is parsable. Value is left unchanged if not.
        /// </summary>
        /// <param name="name">Name of the value to searched for</param>
        /// <param name="value">Value to assign</param>
        public static bool TryGetValue(this ConfigNode node, string name, ref double value)
        {
            double result = 0;
            if (node.HasValue(name) && double.TryParse(node.GetValue(name), out result))
            {
                value = result;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sees if the ConfigNode has a given value, and tries to store it in the ref if it's parseable
        /// </summary>
        /// <param name="name">Name of the value to get</param>
        /// <param name="value">Value to store the result in</param>
        public static bool TryGetValue(this ConfigNode node, string name, ref bool value)
        {
            bool result = false;
            if (node.HasValue(name) && bool.TryParse(node.GetValue(name), out result))
            {
                value = result;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the ConfigNode has the given value, and stores it in the ref value
        /// </summary>
        /// <param name="name">Name of the value to find</param>
        /// <param name="value">Value to store the result in</param>
        public static bool TryGetValue(this ConfigNode node, string name, ref Vector3 value)
        {
            return node.HasValue(name) && RCUtils.TryParseVector3(node.GetValue(name), ref value);
        }
        #endregion
    }
}
