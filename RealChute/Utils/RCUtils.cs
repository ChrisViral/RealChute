using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = System.Random;
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
        /// Gravitational acceleration of Kerbin in m/s²
        /// </summary>
        public const double g = 9.80665;

        /// <summary>
        /// URL of the RealChute settings config from the GameData folder
        /// </summary>
        public const string localSettingsURL = @"GameData\RealChute\Plugins\PluginData\RealChute_Settings.cfg";

        /// <summary>
        /// URL of the RealChute PluginData folder from the GameData folder
        /// </summary>
        public const string localPluginDataURL = @"GameData\RealChute\Plugins\PluginData";

        /// <summary>
        /// Debug log header
        /// </summary>
        public const string logHeader = "[RealChute]: ";
        #endregion

        #region Propreties
        private static readonly string _settingsURL;
        /// <summary>
        /// String URL to the RealChute settings config
        /// </summary>
        public static string settingsURL
        {
            get { return _settingsURL; }
        }

        private static readonly string _pluginDataURL;
        /// <summary>
        /// Returns the RealChute PluginData folder
        /// </summary>
        public static string pluginDataURL
        {
            get { return _pluginDataURL; }
        }

        private static readonly string _assemblyVersion;
        /// <summary>
        /// Gets the current version of the assembly
        /// </summary>
        public static string assemblyVersion
        {
            get { return _assemblyVersion; }
        }

        private static readonly bool _FARLoaded = false;
        /// <summary>
        /// Returns if FAR is currently loaded in the game
        /// </summary>
        public static bool FARLoaded
        {
            get { return _FARLoaded; }
        }

        private static MethodInfo _densityMethod;
        /// <summary>
        /// A delegate to the FAR GetCurrentDensity method
        /// </summary>
        public static MethodInfo densityMethod
        {
            get { return _densityMethod; }
        }

        private static Random _random;
        /// <summary>
        /// Time seeded random number generator instance
        /// </summary>
        public static Random random
        {
            get { return _random; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initiates the static values of the class
        /// </summary>
        static RCUtils()
        {
            //URLs
            string root = KSPUtil.ApplicationRootPath;
            _settingsURL = Path.Combine(root, localSettingsURL);
            _pluginDataURL = Path.Combine(root, localPluginDataURL);

            //Random generator
            _random = new Random();

            //Version string
            Version version = new Version(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion);
            if (version.Revision == 0)
            {
                _assemblyVersion = "v" + (version.Build == 0 ? version.ToString(2) : version.ToString(3));
            }
            else { _assemblyVersion = "v" + version.ToString(); }

            //FAR detection
            try
            {
                _densityMethod = AssemblyLoader.loadedAssemblies.Single(a => a.dllName == "FerramAerospaceResearch").assembly
                    .GetTypes().Single(t => t.Name == "FARAeroUtil").GetMethods().Where(m => m.IsPublic && m.IsStatic)
                    .Where(m => m.ReturnType == typeof(double) && m.Name == "GetCurrentDensity").ToDictionary(m => m, m => m.GetParameters())
                    .Single(m => m.Value[0].ParameterType == typeof(CelestialBody) && m.Value[1].ParameterType == typeof(double)).Key;
                _FARLoaded = true;
            }
            catch (Exception e)
            {
                LogWarning("FAR not detected or incorrectly setup.\nError: " + e.Message);
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
            string[] array = text.Split(',');
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = array[i].Trim();
            }
            return array;
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

            return String.Format("{0}m {1}s", minutes, seconds.ToString("0.0"));
        }

        /// <summary>
        /// Returns true if the number is a whole number (no decimals).
        /// Note: this is only true because the numbers processed through this are manually entered,
        /// else we would need to check for floating point errors.
        /// </summary>
        /// <param name="n">Float to check</param>
        public static bool IsWholeNumber(float n)
        {
            return (n % 1) == 0;
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
        public static void PrintChildren(Transform transform, StringBuilder builder = null)
        {
            if (builder == null) { builder = new StringBuilder(); }
            PrintChildrenRecursive(transform, builder, 0, "\n");
        }

        /// <summary>
        /// Prints all the children of a transform recursively
        /// </summary>
        /// <param name="transform">Transform to get the children from</param>
        /// <param name="builder">StringBuider to store the text into</param>
        /// <param name="index">Index of the current transform in the tree</param>
        /// <param name="offset">Tab offset for tree structure</param>
        private static void PrintChildrenRecursive(Transform transform, StringBuilder builder, int index, string offset)
        {
            builder.Append(offset).AppendFormat("{0}: {1}", index, transform.name);
            for (int i = 0; i < transform.childCount; i++)
            {
                PrintChildrenRecursive(transform.GetChild(i), builder, i, offset + "\t");
            }
        }

        /// <summary>
        /// Returns a random double value between zero and the specified maximum
        /// </summary>
        /// <param name="max">Maximum value</param>
        public static double NextDouble(int max)
        {
            return _random.NextDouble() * max;
        }

        /// <summary>
        /// Returns a random value between the specified minimum and maximum
        /// </summary>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        public static double NextDouble(int min, int max)
        {
            return (_random.NextDouble() * (max - min)) + min;
        }
        #endregion

        #region Logging
        /// <summary>
        /// Prints a message to the debug log
        /// </summary>
        /// <param name="message">Message to print</param>
        public static void Log(string message)
        {
            Debug.Log(logHeader + message);
        }

        /// <summary>
        /// Prints a warning to the debug log
        /// </summary>
        /// <param name="message">Message to print</param>
        public static void LogWarning(string message)
        {
            Debug.LogWarning(logHeader + message);
        }

        /// <summary>
        /// Prints an error to the debug log
        /// </summary>
        /// <param name="message">Message to print</param>
        public static void LogError(string message)
        {
            Debug.LogError(logHeader + message);
        }
        #endregion
    }
}
