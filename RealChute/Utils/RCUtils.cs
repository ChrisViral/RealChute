using System;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Version = System.Version;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute.Utils
{
    public static class RCUtils
    {
        #region Constants
        /// <summary>
        /// Transforms from gees to m/s²
        /// </summary>
        public const double geeToAcc = 9.80665;

        /// <summary>
        /// URL of the RealChute settings config from the GameData folder
        /// </summary>
        public const string localSettingsURL = "GameData/RealChute/Plugins/PluginData/RealChute_Settings.cfg";

        /// <summary>
        /// URL of the RealChute PluginData folder from the GameData folder
        /// </summary>
        public const string localPluginDataURL = "GameData/RealChute/Plugins/PluginData";
        #endregion

        #region Propreties
        /// <summary>
        /// String URL to the RealChute settings config
        /// </summary>
        public static string settingsURL
        {
            get { return Path.Combine(KSPUtil.ApplicationRootPath, localSettingsURL); }
        }

        /// <summary>
        /// Returns the RealChute PluginData folder
        /// </summary>
        public static string pluginDataURL
        {
            get { return Path.Combine(KSPUtil.ApplicationRootPath, localPluginDataURL); }
        }

        private static string _assemblyVersion = string.Empty;
        /// <summary>
        /// Gets the current version of the assembly
        /// </summary>
        public static string assemblyVersion
        {
            get
            {
                if (string.IsNullOrEmpty(_assemblyVersion))
                {
                    Version version = new Version(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion);
                    if (version.Revision == 0)
                    {
                        _assemblyVersion = "v" + (version.Build == 0 ? version.ToString(2) : version.ToString(3));
                    }
                    else { _assemblyVersion = "v" + version.ToString(); }
                }
                return _assemblyVersion;
            }
        }

        private static bool _FARLoaded = false, check = true;
        /// <summary>
        /// Returns if FAR is currently loaded in the game
        /// </summary>
        public static bool FARLoaded
        {
            get 
            {
                if (check)
                {
                    _FARLoaded = AssemblyLoader.loadedAssemblies.Any(a => a.dllName == "FerramAerospaceResearch");
                    check = false;
                }
                return _FARLoaded;
            }
        }

        private static MethodInfo _densityMethod = null;
        /// <summary>
        /// A delegate to the FAR GetCurrentDensity method
        /// </summary>
        public static MethodInfo densityMethod
        {
            get
            {
                if (_densityMethod == null)
                {
                    _densityMethod = AssemblyLoader.loadedAssemblies.FirstOrDefault(a => a.dllName == "FerramAerospaceResearch").assembly
                        .GetTypes().Single(t => t.Name == "FARAeroUtil").GetMethods().Where(m => m.IsPublic && m.IsStatic)
                        .Where(m => m.ReturnType == typeof(double) && m.Name == "GetCurrentDensity").ToDictionary(m => m, m => m.GetParameters())
                        .Single(m => m.Value[0].ParameterType == typeof(CelestialBody) && m.Value[1].ParameterType == typeof(double)).Key;
                }
                return _densityMethod;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns the array of values contained within a string
        /// </summary>
        /// <param name="text">Array to parse</param>
        public static string[] ParseArray(string text)
        {
            return text.Split(',').Select(s => s.Trim()).ToArray();
        }

        /// <summary>
        /// Returns true if the string can be parsed and stores it into the ref value.
        /// </summary>
        /// <param name="text">String to parse</param>
        /// <param name="result">Value to store the result in</param>
        public static bool TryParseVector3(string text, ref Vector3 result)
        {
            if (string.IsNullOrEmpty(text)) { return false; }
            string[] elements = ParseArray(text);
            if (elements.Length != 3) { return false; }
            float x, y, z;
            if (!float.TryParse(elements[0], out x)) { return false; }
            if (!float.TryParse(elements[1], out y)) { return false; }
            if (!float.TryParse(elements[2], out z)) { return false; }
            result = new Vector3(x, y, z);
            return true;
        }

        /// <summary>
        /// Returns the area of a circular parachute of the given diameter
        /// </summary>
        /// <param name="diameter">Diameter of the chute</param>
        public static float GetArea(float diameter)
        {
            return (float)((diameter * diameter * Math.PI) / 4d);
        }

        /// <summary>
        /// Returns the diameter of a given area
        /// </summary>
        /// <param name="area">Area to determine dthe diameter of</param>
        public static float GetDiameter(float area)
        {
            return (float)(Math.Sqrt(area / Math.PI) * 2);
        }

        /// <summary>
        /// Rounds the float to the closets half
        /// </summary>
        /// <param name="f">Number to round</param>
        public static float Round(double d)
        {
            return (float)Math.Max(Math.Round(d, 1, MidpointRounding.AwayFromZero), 0.1);
        }

        /// <summary>
        /// Transform the given time value in seconds to minutes and seconds
        /// </summary>
        /// <param name="time">Time value to transform</param>
        public static string ToMinutesSeconds(float time)
        {
            float minutes = 0, seconds;
            for (seconds = time; seconds >= 60; seconds -= 60)
            {
                minutes++;
            }

            return String.Concat(minutes, "m ", seconds.ToString("0.0"), "s");
        }

        /// <summary>
        /// Returns true if the number is a whole number (no decimals)
        /// </summary>
        /// <param name="f">Float to check</param>
        public static bool IsWholeNumber(float f)
        {
            return Math.Truncate(f) == f;
        }

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
        /// Prints all the children transforms of a given transform and appends them to a StringBuilder
        /// </summary>
        /// <param name="transform">Transform to get the children of</param>
        /// <param name="builder">StringBuilder to append the text to</param>
        public static void PrintChildren(Transform transform, StringBuilder builder)
        {
            if (builder == null) { builder = new StringBuilder(); }
            PrintChildren(transform, builder, 0, "\n");
        }

        /// <summary>
        /// Prints all the children of a transform recursively
        /// </summary>
        /// <param name="transform">Transform to get the children from</param>
        /// <param name="builder">StringBuider to store the text into</param>
        /// <param name="index">Index of the current transform in the tree</param>
        /// <param name="offset">Tab offset for tree structure</param>
        private static void PrintChildren(Transform transform, StringBuilder builder, int index, string offset)
        {
            builder.Append(offset).AppendFormat("{0}: {1}", index, transform.name);
            for (int i = 0; i < transform.childCount; i++)
            {
                PrintChildren(transform.GetChild(i), builder, i, offset + "\t");
            }
        }
        #endregion
    }
}
