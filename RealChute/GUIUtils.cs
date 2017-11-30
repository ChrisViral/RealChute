using System;
using RealChute.Extensions;
using UnityEngine;

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
    public static class GuiUtils
    {
        #region Properties
        private static GUIStyle redLabel;
        /// <summary>
        /// A red KSP label
        /// </summary>
        public static GUIStyle RedLabel
        {
            get
            {
                if (redLabel == null)
                {
                    GUIStyle style = new GUIStyle(HighLogic.Skin.label)
                    {
                        normal = { textColor = XKCDColors.Red },
                        hover = { textColor = XKCDColors.Red }
                    };
                    redLabel = style;
                }
                return redLabel;
            }
        }

        private static GUIStyle yellowLabel;
        /// <summary>
        /// A yellow KSP label
        /// </summary>
        public static GUIStyle YellowLabel
        {
            get
            {
                if (yellowLabel == null)
                {
                    GUIStyle style = new GUIStyle(HighLogic.Skin.label)
                    {
                        normal = { textColor = XKCDColors.BrightYellow },
                        hover = { textColor = XKCDColors.BrightYellow }
                    };
                    yellowLabel = style;
                }
                return yellowLabel;
            }
        }

        private static GUIStyle boldLabel;
        /// <summary>
        /// A bold KSP style label for RealChute GUI
        /// </summary>
        public static GUIStyle BoldLabel
        {
            get
            {
                if (boldLabel == null)
                {
                    GUIStyle style = new GUIStyle(HighLogic.Skin.label) { fontStyle = FontStyle.Bold };
                    boldLabel = style;
                }
                return boldLabel;
            }
        }
        #endregion

        #region Arrays
        /// <summary>
        /// Represents the time suffixes
        /// </summary>
        private static readonly char[] timeSuffixes = { 's', 'm' };

        /// <summary>
        /// Default toggle labels
        /// </summary>
        private static readonly string[] toggles = { "True", "False" };
        #endregion

        #region Methods
        /// <summary>
        /// Parse a time value
        /// </summary>
        /// <param name="text">String to parse</param>
        public static float ParseTime(string text)
        {
            if (string.IsNullOrEmpty(text)) { throw new ArgumentNullException(nameof(text)); }
            char indicator = text[text.Length - 1];
            float multiplier = 1f;
            if (timeSuffixes.Contains(indicator))
            {
                text = text.Remove(text.Length - 1, 1);
                if (indicator == 'm') { multiplier = 60f; }
            }
            return float.Parse(text) * multiplier;
        }

        /// <summary>
        /// Tries to parse a float, taking into account the last character as a time indicator. If none, seconds are assumed.
        /// </summary>
        /// <param name="text">Time value to parse</param>
        /// <param name="result">Value to store the result in</param>
        public static bool TryParseTime(string text, out float result)
        {
            if (string.IsNullOrEmpty(text)) { result = 0; return false; }
            float multiplier = 1f;
            char indicator = text[text.Length - 1];
            if (timeSuffixes.Contains(indicator))
            {
                text = text.Remove(text.Length - 1);
                if (indicator == 'm') { multiplier = 60f; }
            }
            if (float.TryParse(text, out float f))
            {
                result = f * multiplier;
                return true;
            }
            result = 0;
            return false;
        }

        /// <summary>
        /// Parse the string and returns -1 if it's empty
        /// </summary>
        /// <param name="text">String to parse</param>
        public static float ParseEmpty(string text) => string.IsNullOrEmpty(text) ? -1 : float.Parse(text);

        /// <summary>
        /// Parses the spare chutes. If string is empty, returns true and value becomes -1.
        /// </summary>
        /// <param name="text">Value to parse</param>
        /// <param name="result">Value to store the result in</param>
        public static bool TryParseWithEmpty(string text, out float result)
        {
            if (string.IsNullOrEmpty(text))
            {
                result = -1;
                return true;
            }
            if (float.TryParse(text, out float f))
            {
                result = f;
                return true;
            }
            result = 0;
            return false;
        }

        /// <summary>
        /// Checks if the value is within the given range
        /// </summary>
        /// <param name="f">Value to check</param>
        /// <param name="min">Bottom of the range to check</param>
        /// <param name="max">Top of the range to check</param>
        public static bool CheckRange(float f, float min, float max) => f <= max && f >= min;

        /// <summary>
        /// Creates a label and text area of the specified parameters
        /// </summary>
        /// <param name="label">Label string</param>
        /// <param name="value">Value to store</param>
        /// <param name="min">Minimum possible value</param>
        /// <param name="max">Maximum possible value</param>
        /// <param name="width">Width of the text area</param>
        public static void CreateEntryArea(string label, ref string value, float min, float max, float width = 150)
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (float.TryParse(value, out float f) && CheckRange(f, min, max)) { GUILayout.Label(label); }
            else { GUILayout.Label(label, RedLabel); }
            GUILayout.FlexibleSpace();
            value = GUILayout.TextField(value, 10, GUILayout.Width(width));
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Creates a label and text area of the specified parameters that parses time values correctly
        /// </summary>
        /// <param name="label">Label string</param>
        /// <param name="value">Value to store</param>
        /// <param name="min">Minimum possible value</param>
        /// <param name="max">Maximum possible value</param>
        /// <param name="width">Width of the text area</param>
        public static void CreateTimeEntryArea(string label, ref string value, float min, float max, float width = 150)
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (TryParseTime(value, out float f) && CheckRange(f, min, max)) { GUILayout.Label(label); }
            else { GUILayout.Label(label, RedLabel); }
            GUILayout.FlexibleSpace();
            value = GUILayout.TextField(value, 10, GUILayout.Width(width));
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Creates a label and text area of the specified parameters that parses an empty text as -1
        /// </summary>
        /// <param name="label">Label string</param>
        /// <param name="value">Value to store</param>
        /// <param name="min">Minimum possible value</param>
        /// <param name="max">Maximum possible value</param>
        /// <param name="width">Width of the text area</param>
        public static void CreateEmptyEntryArea(string label, ref string value, float min, float max, float width = 150)
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            if (TryParseWithEmpty(value, out float f) && CheckRange(f, min, max)) { GUILayout.Label(label); }
            else { GUILayout.Label(label, RedLabel); }
            GUILayout.FlexibleSpace();
            value = GUILayout.TextField(value, 10, GUILayout.Width(width));
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Creates two linked toggle buttons of the specified parameters
        /// </summary>
        /// <param name="label">Label of the buttons</param>
        /// <param name="value">Boolean value of the toggles</param>
        /// <param name="maxWidth">Max width of the buttons</param>
        /// <param name="buttons">Name of both buttons</param>
        public static void CreateTwinToggle(string label, ref bool value, float maxWidth, string[] buttons = null)
        {
            if (buttons == null) { buttons = toggles; }
            GUILayout.Space(5);
            GUILayout.BeginHorizontal(GUILayout.MaxWidth(maxWidth));
            GUILayout.Label(label);
            if (GUILayout.Toggle(value, buttons[0])) { value = true; }
            GUILayout.FlexibleSpace();
            if (GUILayout.Toggle(!value, buttons[1])) { value = false; }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Creates a centered GUI button
        /// </summary>
        /// <param name="text">Button text</param>
        /// <param name="callback">Action on button click</param>
        /// <param name="width">Width of the button</param>
        public static void CenteredButton(string text, Callback callback, float width = 150)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(text, HighLogic.Skin.button, GUILayout.Width(width)))
            {
                callback();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        #endregion
    }
}
