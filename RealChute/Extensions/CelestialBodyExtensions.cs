﻿using System;

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
        /// <param name="body">Body to get the density for</param>
        /// <param name="alt">Altitude to fetch the density at</param>
        /// <param name="temperature">Ambient temperature</param>
        public static double GetDensityAtAlt(this CelestialBody body, double alt, double temperature)
        {
            return body.atmosphere && alt <= GetMaxAtmosphereAltitude(body) ? FlightGlobals.getAtmDensity(body.GetPressureAtAlt(alt), temperature, body) : 0d;
        }

        /// <summary>
        /// Returns the atmospheric pressure at this altitude
        /// </summary>
        /// <param name="body">body to get the pressure for</param>
        /// <param name="alt">Altitude to get the pressure at</param>
        public static double GetPressureAtAlt(this CelestialBody body, double alt) => body.atmosphere && alt <= body.GetMaxAtmosphereAltitude() ? FlightGlobals.getStaticPressure(alt, body) : 0d;

        /// <summary>
        /// Gets the atmospheric pressure at sea level on the given body
        /// </summary>
        /// <param name="body">body to get the pressure for</param>
        public static double GetPressureAsl(this CelestialBody body) => body.atmosphere ? FlightGlobals.getStaticPressure(0, body) : 0d;

        /// <summary>
        /// Returns the altitude at which the atmosphere disappears
        /// </summary>
        /// <param name="body">body to get the max atmosphere alt for</param>
        public static double GetMaxAtmosphereAltitude(this CelestialBody body) => body.atmosphere ? body.atmosphereDepth : 0d;

        /// <summary>
        /// Returns the maximum temperature possible on a given body at the given altitude
        /// </summary>
        /// <param name="body">Body to get the max temperature for</param>
        /// <param name="alt">Alt to get the max temperature at</param>
        public static double GetMaxTemperatureAtAlt(this CelestialBody body, double alt)
        {
            //Thanks NathanKell for this
            return body.GetTemperature(alt) // base temperature
                    + (body.atmosphereTemperatureSunMultCurve.Evaluate((float)alt) // altitude-based multiplier to temperature delta
                       * (body.latitudeTemperatureBiasCurve.Evaluate(0f) + body.latitudeTemperatureSunMultCurve.Evaluate(1f)
                          + body.axialTemperatureSunMultCurve.Evaluate((float)Math.Sin(body.orbit.inclination * (Math.PI / 180d)))));
        }
        #endregion
    }
}
