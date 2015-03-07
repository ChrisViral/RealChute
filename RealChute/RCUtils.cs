using System;
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

namespace RealChute
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

        #region Arrays
        /// <summary>
        /// Represents the time suffixes
        /// </summary>
        public static readonly char[] timeSuffixes = { 's', 'm' };

        /// <summary>
        /// Represent the types of parachutes
        /// </summary>
        public static readonly string[] types = { "Main", "Drogue", "Drag" };
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

        private static GUIStyle _redLabel = null;
        /// <summary>
        /// A red KSP label for ProceduralChute
        /// </summary>
        public static GUIStyle redLabel
        {
            get
            {
                if (_redLabel == null)
                {
                    GUIStyle style = new GUIStyle(HighLogic.Skin.label);
                    style.normal.textColor = XKCDColors.Red;
                    style.hover.textColor = XKCDColors.Red;
                    _redLabel = style;
                }
                return _redLabel;
            }
        }

        private static GUIStyle _boldLabel = null;
        /// <summary>
        /// A bold KSP style label for RealChute GUI
        /// </summary>
        public static GUIStyle boldLabel
        {
            get
            {
                if (_boldLabel == null)
                {
                    GUIStyle style = new GUIStyle(HighLogic.Skin.label);
                    style.fontStyle = FontStyle.Bold;
                    _boldLabel = style;
                }
                return _boldLabel;
            }
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
                        if (version.Build == 0) { _assemblyVersion = "v" + version.ToString(2); }
                        _assemblyVersion = "v" + version.ToString(3);
                    }
                    _assemblyVersion = "v" + version.ToString();
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
        /// Returns true if the time is parseable, returns false otherwise
        /// </summary>
        /// <param name="text">Time value to parse</param>
        public static bool CanParseTime(string text)
        {
            float f = 0;
            return TryParseTime(text, ref f);
        }

        /// <summary>
        /// Parse a time value
        /// </summary>
        /// <param name="text">String to parse</param>
        public static float ParseTime(string text)
        {
            float result = 0;
            TryParseTime(text, ref result);
            return result;
        }

        /// <summary>
        /// Tries to parse a float, taking into account the last character as a time indicator. If none, seconds are assumed.
        /// </summary>
        /// <param name="text">Time value to parse</param>
        /// <param name="result">Value to store the result in</param>
        public static bool TryParseTime(string text, ref float result)
        {
            if (string.IsNullOrEmpty(text)) { return false; }
            float multiplier = 1, test = 0;
            char indicator = text[text.Length - 1];
            if (timeSuffixes.Contains(indicator))
            {
                text = text.Remove(text.Length - 1);
                if (indicator == 'm') { multiplier = 60; }
            }
            if (float.TryParse(text, out test))
            {
                result = test * multiplier;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the value is parsable without actually parsing it
        /// </summary>
        /// <param name="text">String to parse</param>
        public static bool CanParse(string text)
        {
            float f = 0;
            return TryParse(text, ref f);
        }

        /// <summary>
        /// Tries parsing a float from text. IF it fails, the ref value is left unchanged.
        /// </summary>
        /// <param name="text">String to parse</param>
        /// <param name="result">Value to store the result in</param>
        public static bool TryParse(string text, ref float result)
        {
            if (string.IsNullOrEmpty(text)) { return false; }
            float f = 0;
            if (float.TryParse(text, out f))
            {
                result = f;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the spares can be parsed
        /// </summary>
        /// <param name="text">Value to parse</param>
        public static bool CanParseWithEmpty(string text)
        {
            if (string.IsNullOrEmpty(text)) { return true; }
            float test;
            return float.TryParse(text, out test);
        }

        /// <summary>
        /// Parse the string and returns -1 if it's empty
        /// </summary>
        /// <param name="text">String to parse</param>
        public static float ParseWithEmpty(string text)
        {
            if (string.IsNullOrEmpty(text)) { return -1; }
            return float.Parse(text);
        }

        /// <summary>
        /// Parses the spare chutes. If string is empty, returns true and value becomes -1.
        /// </summary>
        /// <param name="text">Value to parse</param>
        /// <param name="result">Value to store the result in</param>
        public static bool TryParseWithEmpty(string text, ref float result)
        {
            if (string.IsNullOrEmpty(text))
            {
                result = -1;
                return true;
            }
            float test;
            if (float.TryParse(text, out test))
            {
                result = test;
                return true;
            }
            return false;
        }

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
            string[] splits = ParseArray(text);
            if (splits.Length != 3) { return false; }
            float x, y, z;
            if (!float.TryParse(splits[0], out x)) { return false; }
            if (!float.TryParse(splits[1], out y)) { return false; }
            if (!float.TryParse(splits[2], out z)) { return false; }
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
        public static float Round(float f)
        {
            return (float)Math.Max(Math.Round(f, 1, MidpointRounding.AwayFromZero), 0.1);
        }

        /// <summary>
        /// Checks if the value is within the given range
        /// </summary>
        /// <param name="f">Value to check</param>
        /// <param name="min">Bottom of the range to check</param>
        /// <param name="max">Top of the range to check</param>
        public static bool CheckRange(float f, float min, float max)
        {
            return f <= max && f >= min;
        }

        /// <summary>
        /// Transform the given time value in seconds to minutes and seconds
        /// </summary>
        /// <param name="time">Time value to transform</param>
        public static string ToMinutesSeconds(float time)
        {
            float minutes = 0, seconds = time;
            while (seconds >= 60)
            {
                seconds -= 60;
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
        /// Creates a centered GUI button
        /// </summary>
        /// <param name="text">Button text</param>
        /// <param name="action">Action on button click</param>
        /// <param name="width">Width of the button</param>
        public static void CenteredButton(string text, Action action, float width = 150)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(text, HighLogic.Skin.button, GUILayout.Width(width)))
            {
                action();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        #endregion
    }
}
