using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using static System.Math;
using Debug = UnityEngine.Debug;

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
    public static class RCUtils
    {
        #region Constants
        /// <summary>
        /// Transforms from gees to m/s²
        /// </summary>
        public const double GeeToAcc = 9.80665;

        /// <summary>
        /// Parachute starting temperature
        /// </summary>
        public const double StartTemp = 300d;

        /// <summary>
        /// Absolute zero in °C
        /// </summary>
        public const double AbsoluteZero = -273.15d;

        /// <summary>
        /// URL of the RealChute settings config from the GameData folder
        /// </summary>
        public const string LocalSettingsURL = "GameData/RealChute/Plugins/PluginData/RealChute_Settings.cfg";

        /// <summary>
        /// URL of the RealChute PluginData folder from the GameData folder
        /// </summary>
        public const string LocalPluginDataURL = "GameData/RealChute/Plugins/PluginData";

        /// <summary>
        /// Relative URL to the Nyan Cat parachute texture
        /// </summary>
        public const string NyanTextureURL = "RealChute/Parts/nyan_texture";

        /// <summary>
        /// RealChute toolbar icon URL
        /// </summary>
        public const string ToolbarIconURL = "RealChute/Plugins/PluginData/RC_Icon.png";

        /// <summary>
        /// PopupDialog anchor vector
        /// </summary>
        private static readonly Vector2 anchor = new Vector2(0.5f, 0.5f);
        #endregion

        #region Propreties
        /// <summary>
        /// String URL to the RealChute settings config
        /// </summary>
        public static string SettingsURL => Path.Combine(KSPUtil.ApplicationRootPath, LocalSettingsURL);

        /// <summary>
        /// Returns the RealChute PluginData folder
        /// </summary>
        public static string PluginDataURL => Path.Combine(KSPUtil.ApplicationRootPath, LocalPluginDataURL);

        private static string assemblyVersion;
        /// <summary>
        /// Gets the current version of the assembly
        /// </summary>
        public static string AssemblyVersion
        {
            get
            {
                if (string.IsNullOrEmpty(assemblyVersion))
                {
                    Version version = new Version(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion);
                    if (version.Revision == 0)
                    {
                        assemblyVersion = "v" + (version.Build == 0 ? version.ToString(2) : version.ToString(3));
                    }
                    else { assemblyVersion = "v" + version; }
                }
                return assemblyVersion;
            }
        }
        #endregion

        #region Methods
        public static string[] ParseArray(string text)
        {
            string[] array = text.Split(',');
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = array[i].Trim();
            }
            return array;
        }

        /// <summary>
        /// Returns the area of a circular parachute of the given diameter
        /// </summary>
        /// <param name="diameter">Diameter of the chute</param>
        public static float GetArea(float diameter) => (float)((PI * diameter * diameter) / 4d);

        /// <summary>
        /// Returns the diameter of a given area
        /// </summary>
        /// <param name="area">Area to determine dthe diameter of</param>
        public static float GetDiameter(float area) => (float)(Sqrt(area / PI) * 2d);

        /// <summary>
        /// Rounds the float to the closets half
        /// </summary>
        /// <param name="d">Number to round</param>
        public static float Round(double d) => (float)Max(Math.Round(d, 1, MidpointRounding.AwayFromZero), 0.1);

        /// <summary>
        /// Transform the given time value in seconds to minutes and seconds
        /// </summary>
        /// <param name="time">Time value to transform</param>
        public static string ToMinutesSeconds(float time)
        {
            int minutes = (int)Floor(time % 60d);
            float seconds = time - (60 * minutes);
            return $"{minutes}m {seconds:0.0}s";
        }

        /// <summary>
        /// Returns true if the number is a whole number (no decimals)
        /// </summary>
        /// <param name="f">Float to check</param>
        public static bool IsWholeNumber(float f) => Truncate(f) == f;

        /// <summary>
        /// Returns a simplified string for the chute number
        /// </summary>
        /// <param name="id">ID of the parachute</param>
        public static string ParachuteNumber(int id)
        {
            switch (id)
            {
                case 0:
                    return "Main chute";

                case 1:
                    return "Secondary chute";

                default:
                    return "Chute #" + (id + 1);
            }
        }

        /// <summary>
        /// Posts a correctly formatted PopupDialog
        /// </summary>
        /// <param name="title">Title of the PopupDialog</param>
        /// <param name="message">Message of the PopupDialog</param>
        /// <param name="button">Button text of the PopupDialog</param>
        public static void PopupDialog(string title, string message, string button) => global::PopupDialog.SpawnPopupDialog(anchor, anchor, title, title, message, button, false, HighLogic.UISkin);

        /// <summary>
        /// Finds the child of a GameObject recursively
        /// </summary>
        /// <param name="obj">Object to search from</param>
        /// <param name="name">Name of the object to find</param>
        /// <returns>The found child object, or null if none was found</returns>
        public static GameObject FindChildRecursive(GameObject obj, string name)
        {
            static Transform FindTransform(Transform transform, string name)
            {
                Transform match = transform.Find(name);
                if (match) return match;

                foreach (Transform child in transform)
                {
                    match = FindTransform(child, name);
                    if (match) return match;
                }

                return null;
            }

            if (obj.name == name) return obj;

            Transform match = FindTransform(obj.transform, name);
            return match ? match.gameObject : null;
        }

        [Conditional("DEBUG")]
        public static void PrintHierarchy(GameObject obj)
        {
            static void PrintRecursive(Transform transform, StringBuilder builder, int indent = 0)
            {
                builder.Append('|', indent).AppendLine(transform.name);
                foreach (Transform child in transform)
                {
                    PrintRecursive(child, builder, indent + 1);
                }
            }

            StringBuilder builder = StringBuilderCache.Acquire();
            builder.AppendLine($"[RealChute]: Object Hierarchy ({obj.name})");
            PrintRecursive(obj.transform, builder);
            Debug.Log(builder.ToStringAndRelease());
        }
        #endregion
    }
}
