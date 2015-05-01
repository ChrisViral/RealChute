using System;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
        /// Parachute starting temperature
        /// </summary>
        public const double startTemp = 300;

        /// <summary>
        /// Absolute zero in °C
        /// </summary>
        public const double absoluteZero = -273.15;

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
        /// Prints relevant info about an AvaialablePart object
        /// </summary>
        /// <param name="partInfo">AvaialablePart object to print</param>
        public static void PrintPartInfo(AvailablePart partInfo)
        {
            bool tmp;
            System.Text.StringBuilder b = new System.Text.StringBuilder("[RealChute]: Printing PartInfo stats\n");
            b.AppendLine("Part name: " + partInfo.name);
            b.AppendLine("Part title: " + partInfo.title);
            tmp = partInfo.partPrefab == null;
            b.AppendLine("PartPrefab null: " + tmp);
            if (!tmp)
            {
                b.AppendLine("PartPrefab module count: " + partInfo.partPrefab.Modules.Count);
            }
            tmp = partInfo.internalConfig == null;
            b.AppendLine("Internal config null: " + tmp);
            if (!tmp)
            {
                b.AppendLine("Internal config values count: " + partInfo.internalConfig.values.Count);
                b.AppendLine("Internal config nodes count: " + partInfo.internalConfig.nodes.Count);
            }
            tmp = partInfo.partConfig == null;
            b.AppendLine("Part config null: " + tmp);
            if (!tmp)
            {
                b.AppendLine("Part config values count: " + partInfo.partConfig.values.Count);
                b.AppendLine("Part config nodes count: " + partInfo.partConfig.nodes.Count);
            }
            tmp = partInfo.moduleInfos == null;
            b.AppendLine("ModuleInfos null: " + tmp);
            if (!tmp)
            {
                b.AppendLine("ModuleInfos count: " + partInfo.moduleInfos.Count);
            }
            tmp = partInfo.resourceInfos == null;
            b.AppendLine("ResourceInfos null: " + tmp);
            if (!tmp)
            {
                b.AppendLine("ResourceInfos count: " + partInfo.resourceInfos.Count);
            }
            b.AppendLine("Part path: " + partInfo.partPath);
            b.AppendLine("Part URL: " + partInfo.partUrl);
            b.AppendLine("Config full name: " + partInfo.configFileFullName);
            tmp = partInfo.partUrlConfig == null;
            b.AppendLine("Part URLConfig null: " + tmp);
            if (!tmp)
            {
                b.AppendLine("Part URLConfig URL: " + partInfo.partUrlConfig.url);
                tmp = partInfo.partUrlConfig.config == null;
                b.AppendLine("Part URLConfig config null: " + tmp);
                if (!tmp)
                {
                    b.AppendLine("Part URLConfig values count: " + partInfo.partUrlConfig.config.values.Count);
                    b.AppendLine("Part URLConfig nodes count: " + partInfo.partUrlConfig.config.nodes.Count);
                }
            }
            UnityEngine.Debug.Log(b.ToString());
        }
        #endregion
    }
}
