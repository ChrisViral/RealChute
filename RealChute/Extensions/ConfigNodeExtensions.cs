using UnityEngine;

/* RealChute was made by Christophe Savard (stupid_chris). This specific file only is
 * licensed under CC-BY-SA 4.0 INTL.
 */

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
