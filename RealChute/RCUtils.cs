using System;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more information contact me on the forum. */

namespace RealChute
{
    public static class RCUtils
    {
        #region Constants
        /// <summary>
        /// Transforms from gees to m/s²
        /// </summary>
        public const double geeToAcc = 9.80665d;

        /// <summary>
        /// URL of the RealChute settings config from the GameData folder
        /// </summary>
        public const string localSettingsURL = "GameData/RealChute/RealChute_Settings.cfg";

        /// <summary>
        /// DeploymentStates with their string equivalent
        /// </summary>
        public static readonly Dictionary<DeploymentStates, string> states = new Dictionary<DeploymentStates, string>(5)
        {
            { DeploymentStates.STOWED, "STOWED" },
            { DeploymentStates.PREDEPLOYED, "PREDEPLOYED" },
            { DeploymentStates.LOWDEPLOYED, "LOWDEPLOYED" },
            { DeploymentStates.DEPLOYED, "DEPLOYED" },
            { DeploymentStates.CUT, "CUT" }
        };
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
            get { return System.IO.Path.Combine(KSPUtil.ApplicationRootPath, localSettingsURL); }
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

        /// <summary>
        /// Gets the current version of the assembly
        /// </summary>
        public static string assemblyVersion
        {
            get
            {
                System.Version version = new System.Version(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion);
                if (version.Revision == 0)
                {
                    if (version.Build == 0) { return "v" + version.ToString(2); }
                    return "v" + version.ToString(3);
                }
                return "v" + version.ToString();
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
            if (string.IsNullOrEmpty(text)) { return false; }
            char indicator = text[text.Length - 1];
            float test;
            if (timeSuffixes.Contains(indicator)) { text = text.Remove(text.Length - 1); }

            return float.TryParse(text, out test);
        }

        /// <summary>
        /// Parse a time value
        /// </summary>
        /// <param name="text">String to parse</param>
        public static float ParseTime(string text)
        {
            if (string.IsNullOrEmpty(text)) { return 0; }
            float multiplier = 1, test = 0;
            char indicator = text[text.Length - 1];
            if (timeSuffixes.Contains(indicator))
            {
                text = text.Remove(text.Length - 1);
                if (indicator == 'm') { multiplier = 60; }
            }

            if (float.TryParse(text, out test))
            {
                return test * multiplier;       
            }
            return 0;
        }

        /// <summary>
        /// Tries to parse a float, taking into account the last character as a time indicator. If none, seconds are assumed.
        /// </summary>
        /// <param name="text">Time value to parse</param>
        /// <param name="result">Value to store the result in</param>
        public static bool TryParseTime(string text, ref float result)
        {
            if (CanParseTime(text))
            {
                result = ParseTime(text);
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
            if (string.IsNullOrEmpty(text)) { return false; }
            float test;
            return float.TryParse(text, out test);
        }

        /// <summary>
        /// Tries parsing a float from text. IF it fails, the ref value is left unchanged.
        /// </summary>
        /// <param name="text">String to parse</param>
        /// <param name="result">Value to store the result in</param>
        public static bool TryParse(string text, ref float result)
        {
            if (CanParse(text))
            {
                result = float.Parse(text);
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
            if (KSPUtil.ParseVector3(text) != null)
            {
                result = KSPUtil.ParseVector3(text);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the area of a circular parachute of the given diameter
        /// </summary>
        /// <param name="diameter">Diameter of the chute</param>
        public static float GetArea(float diameter)
        {
            return Mathf.Pow(diameter, 2f) * Mathf.PI / 4f;
        }

        /// <summary>
        /// Returns the diameter of a given area
        /// </summary>
        /// <param name="area">Area to determine dthe diameter of</param>
        public static float GetDiameter(float area)
        {
            return Mathf.Sqrt(area / Mathf.PI) * 2f;
        }

        /// <summary>
        /// Returns the atmospheric density at the given altitude on the given celestial body
        /// </summary>
        /// <param name="body">Name of the body</param>
        /// <param name="alt">Altitude the fetch the density at</param>
        public static float GetDensityAtAlt(CelestialBody body, float alt)
        {
            return (float)FlightGlobals.getAtmDensity(FlightGlobals.getStaticPressure(alt, body));
        }

        /// <summary>
        /// Returns the altitude at which the atmosphere disappears
        /// </summary>
        /// <param name="body">Celestial body to check</param>
        public static float GetMaxAtmosphereAltitude(CelestialBody body)
        {
            if (!body.atmosphere) { return 0; }
            return -(float)(body.atmosphereScaleHeight * Math.Log(1e-6)) * 1000;
        }

        /// <summary>
        /// Rounds the float to the closets half
        /// </summary>
        /// <param name="f">Number to round</param>
        public static float Round(float f)
        {
            string[] splits = f.ToString().Split('.');
            if (splits.Length != 2) { return f; }
            float round = 0, decimals = float.Parse("0." + splits[1].Trim());
            if (decimals >= 0.25f && decimals < 0.75) { round = 0.5f; }
            else if (decimals >= 0.75) { round = 1; }
            return Mathf.Max(float.Parse(splits[0].Trim()) + round, 0.5f);
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
            float minutes = 0, seconds = 0;
            while (time >= 60)
            {
                time -= 60;
                minutes++;
            }
            seconds = time;
            return minutes.ToString("0") + "m " + seconds.ToString("0.#") + "s";
        }

        /// <summary>
        /// Returns true if the number is a whole number (no decimals)
        /// </summary>
        /// <param name="f">Float to check</param>
        public static bool IsWholeNumber(float f)
        {
            return !f.ToString().Contains('.');
        }
        #endregion
    }
}
