using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more informtion contact me on the forum. */

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

        /// <summary>
        /// A red KSP label for ProceduralChute
        /// </summary>
        public static GUIStyle redLabel
        {
            get
            {
                GUIStyle style = new GUIStyle(HighLogic.Skin.label);
                style.normal.textColor = XKCDColors.Red;
                style.hover.textColor = XKCDColors.Red;
                return style;
            }
        }

        /// <summary>
        /// A bold KSP style label for RealChute GUI
        /// </summary>
        public static GUIStyle boldLabel
        {
            get
            {
                GUIStyle style = new GUIStyle(HighLogic.Skin.label);
                style.fontStyle = FontStyle.Bold;
                return style;
            }
        }

        /// <summary>
        /// Gets the current version of the assembly
        /// </summary>
        public static string assemblyVersion
        {
            get
            {
                System.Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                if (version.Revision == 0)
                {
                    if (version.Build == 0) { return version.ToString(2); }
                    return version.ToString(3);
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

            bool cutLast = false;
            char indicator = text[text.Length - 1];
            float empty;
            if (timeSuffixes.Contains(indicator)) { cutLast = true; }

            if (cutLast) { text = text.Remove(text.Length - 1); }

            return float.TryParse(text, out empty);
        }

        /// <summary>
        /// Parse a time value
        /// </summary>
        /// <param name="text">String to parse</param>
        public static float ParseTime(string text)
        {
            if (string.IsNullOrEmpty(text)) { return 0; }

            float multiplier = 1, test = 0;
            bool cutLast = false;
            char indicator = text[text.Length - 1];
            if (timeSuffixes.Contains(indicator))
            {
                cutLast = true;
                if (indicator == 'm') { multiplier = 60; }
            }

            if (cutLast) { text = text.Remove(text.Length - 1); }

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
            if (string.IsNullOrEmpty(text)) { return false; }

            float multiplier = 1, test = 0;
            bool cutLast = false;
            char indicator = text[text.Length - 1];
            if (timeSuffixes.Contains(indicator))
            {
                cutLast = true;
                if (indicator == 'm') { multiplier = 60; }
            }

            if (cutLast) { text = text.Remove(text.Length - 1); }

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
            if (string.IsNullOrEmpty(text)) { return false; }

            float empty;
            return float.TryParse(text, out empty);
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

            float empty;
            return float.TryParse(text, out empty);
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

            float test = 0;
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
        /// <param name="array">Array to parse</param>
        public static string[] ParseArray(string array)
        {
            return array.Split(',').Select(s => s.Trim()).ToArray();
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
            return float.Parse(splits[0].Trim()) + round;
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
