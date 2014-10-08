using System;

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
    public static class CelestialBodyExtensions
    {
        #region Methods
        /// <summary>
        /// Returns the atmospheric density at the given altitude on the given celestial body
        /// </summary>
        /// <param name="alt">Altitude the fetch the density at</param>
        public static double GetDensityAtAlt(this CelestialBody body, double alt)
        {
            if (alt > GetMaxAtmosphereAltitude(body)) { return 0; }
            if (RCUtils.FARLoaded && !RCUtils.disabled)
            {
                try
                {
                    RCUtils.densityMethod.Invoke(null, new object[] { body, alt });
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError("[RealChute]: Encountered an error calculating atmospheric density with FAR. Using stock values.\n" + e.StackTrace);
                    RCUtils.disabled = true;
                }
            }
           return FlightGlobals.getAtmDensity(body.GetPressureAtAlt(alt));
        }

        /// <summary>
        /// Returns the atmospheric density ASL for this CelestialBody
        /// </summary>
        public static double GetDensityASL(this CelestialBody body)
        {
            return body.GetDensityAtAlt(0);
        }

        /// <summary>
        /// Returns the atmospheric pressure at this altitude
        /// </summary>
        /// <param name="alt">Altitude to get the pressure at</param>
        public static double GetPressureAtAlt(this CelestialBody body, double alt)
        {
            if (!body.atmosphere) { return 0d; }
            double pressure = FlightGlobals.getStaticPressure(alt, body);
            return pressure <= 1E-6 ? 0d : pressure;
        }

        /// <summary>
        /// Returns the atmospheric pressure ASL for this body
        /// </summary>
        public static double GetPressureASL(this CelestialBody body)
        {
            if (!body.atmosphere) { return 0d; }
            return FlightGlobals.getStaticPressure(0, body);
        }

        /// <summary>
        /// Returns the altitude at which the atmosphere disappears
        /// </summary>
        public static double GetMaxAtmosphereAltitude(this CelestialBody body)
        {
            if (!body.atmosphere) { return 0d; }
            return -body.atmosphereScaleHeight * Math.Log(1E-6) * 1000;
        }
        #endregion
    }
}
