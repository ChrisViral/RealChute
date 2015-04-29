using System;
using RealChute.Utils;
using Debug = UnityEngine.Debug;

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
        private static bool disabled = false;
        /// <summary>
        /// Returns the atmospheric density at the given altitude on the given celestial body
        /// </summary>
        /// <param name="alt">Altitude the fetch the density at</param>
        /// <param name="temperature">Ambient temperature</param>
        public static double GetDensityAtAlt(this CelestialBody body, double alt, double temperature)
        {
            if (!body.atmosphere || alt > GetMaxAtmosphereAltitude(body)) { return 0; }
            if (RCUtils.FARLoaded && !disabled)
            {
                try
                {
                    return (double)RCUtils.densityMethod.Invoke(null, new object[] { body, alt, false });
                }
                catch (Exception e)
                {
                    Debug.LogError("[RealChute]: Encountered an error calculating atmospheric density with FAR. Using stock values.\n" + e.StackTrace);
                    disabled = true;
                }
            }
           return FlightGlobals.getAtmDensity(body.GetPressureAtAlt(alt), temperature, body);
        }

        /// <summary>
        /// Returns the atmospheric pressure at this altitude
        /// </summary>
        /// <param name="alt">Altitude to get the pressure at</param>
        public static double GetPressureAtAlt(this CelestialBody body, double alt)
        {
            if (!body.atmosphere || alt > body.GetMaxAtmosphereAltitude()) { return 0; }
            return FlightGlobals.getStaticPressure(alt, body);
        }

        /// <summary>
        /// Gets the atmospheric pressure at seal level on the given body
        /// </summary>
        public static double GetPressureASL(this CelestialBody body)
        {
            if (!body.atmosphere) { return 0; }
            return FlightGlobals.getStaticPressure(0, body);
        }

        /// <summary>
        /// Returns the altitude at which the atmosphere disappears
        /// </summary>
        public static double GetMaxAtmosphereAltitude(this CelestialBody body)
        {
            if (!body.atmosphere) { return 0; }
            return body.atmosphereDepth;
        }

        /// <summary>
        /// Returns the maximum temperature possible on a given body at the given altitude
        /// </summary>
        /// <param name="alt">Alt to get the max temperature at</param>
        public static double GetMaxTemperatureAtAlt(this CelestialBody body, double alt)
        {
            //Thanks NathanKell for this
            return body.GetTemperature(alt) // base temperature
                    + body.atmosphereTemperatureSunMultCurve.Evaluate((float)alt) // altitude-based multiplier to temperature delta
                    * (body.latitudeTemperatureBiasCurve.Evaluate(0) + body.latitudeTemperatureSunMultCurve.Evaluate(1)
                    + body.axialTemperatureSunMultCurve.Evaluate((float)Math.Sin(body.orbit.inclination * (Math.PI / 180))));
        }
        #endregion
    }
}
