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
        /// DEPRECATED per discussion with ferram4; FAR is no longer needed as stock KSP handles this adequately now.
        /// </summary>
        /// <param name="body">Body to get the density for</param>
        /// <param name="alt">Altitude to fetch the density at</param>
        /// <param name="temperature">Ambient temperature</param>
        /// <param name="vessel">Optional vessel to pass to FARAeroUtil.GetCurrentDensity(vessel)</param>
        public static double GetDensityAtAlt(this CelestialBody body, double alt, double temperature, Vessel vessel = null) => !body.atmosphere || alt > GetMaxAtmosphereAltitude(body) ? 0d : FlightGlobals.getAtmDensity(body.GetPressureAtAlt(alt), temperature, body);

        /// <summary>
        /// Returns the atmospheric pressure at this altitude
        /// </summary>
        /// <param name="body">Body to get the pressure for for</param>
        /// <param name="alt">Altitude to get the pressure at</param>
        public static double GetPressureAtAlt(this CelestialBody body, double alt) => !body.atmosphere || alt > body.GetMaxAtmosphereAltitude() ? 0d : FlightGlobals.getStaticPressure(alt, body);

        /// <summary>
        /// Gets the atmospheric pressure at sea level on the given body
        /// <param name="body">Body to get the pressure for for</param>
        /// </summary>
        public static double GetPressureASL(this CelestialBody body) => !body.atmosphere ? 0 : FlightGlobals.getStaticPressure(0, body);

        /// <summary>
        /// Returns the altitude at which the atmosphere disappears
        /// </summary>
        public static double GetMaxAtmosphereAltitude(this CelestialBody body) => !body.atmosphere ? 0d : body.atmosphereDepth;

        /// <summary>
        /// Returns the maximum temperature possible on a given body at the given altitude
        /// </summary>
        /// <param name="body">Body to get the temperature for</param>
        /// <param name="alt">Alt to get the max temperature at</param>
        public static double GetMaxTemperatureAtAlt(this CelestialBody body, double alt)
        {
            //Thanks NathanKell for this
            return body.GetTemperature(alt) // base temperature
                    + (body.atmosphereTemperatureSunMultCurve.Evaluate((float)alt) // altitude-based multiplier to temperature delta
                       * (body.latitudeTemperatureBiasCurve.Evaluate(0) + body.latitudeTemperatureSunMultCurve.Evaluate(1)
                          + body.axialTemperatureSunMultCurve.Evaluate((float)Math.Sin(body.orbit.inclination * (Math.PI / 180d)))));
        }
        #endregion
    }
}
