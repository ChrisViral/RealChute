using System;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get explicit permission on the matter by opening an issue on the RealChute GitHub repository.
 * If approved, you will be required to distribute through the attribution clause, and must make it clear to anyone using
 * your modification of my work that they must report any problem related to this usage to you, and not to me.
 * This excemption can be approved by anyone managing the RealChute repository at the time, be it the original author or anyone
 * that the project was handed to. Should the repository not exist anymore, or should you not receive an answer to your issue
 * within a period of thirty (30) days, you are granted permission to distribute RealChute under the license CC-BY-NC-SA 4.0 INTL. */

namespace RealChute.Extensions;

public static class CelestialBodyExtensions
{
    #region Methods
    /// <param name="body">Body to get the density for</param>
    extension(CelestialBody body)
    {
        /// <summary>
        /// Returns the atmospheric density at the given altitude on the given celestial body
        /// </summary>
        /// <param name="alt">Altitude to fetch the density at</param>
        /// <param name="temperature">Ambient temperature</param>
        public double GetDensityAtAlt(double alt, double temperature)
        {
            return body.atmosphere && alt <= body.GetMaxAtmosphereAltitude() ? FlightGlobals.getAtmDensity(body.GetPressureAtAlt(alt), temperature, body) : 0d;
        }

        /// <summary>
        /// Returns the atmospheric pressure at this altitude
        /// </summary>
        /// <param name="alt">Altitude to get the pressure at</param>
        private double GetPressureAtAlt(double alt) => body.atmosphere && alt <= body.GetMaxAtmosphereAltitude() ? FlightGlobals.getStaticPressure(alt, body) : 0d;

        /// <summary>
        /// Gets the atmospheric pressure at sea level on the given body
        /// </summary>
        public double GetPressureAsl() => body.atmosphere ? FlightGlobals.getStaticPressure(0, body) : 0d;

        /// <summary>
        /// Returns the altitude at which the atmosphere disappears
        /// </summary>
        public double GetMaxAtmosphereAltitude() => body.atmosphere ? body.atmosphereDepth : 0d;

        /// <summary>
        /// Returns the maximum temperature possible on a given body at the given altitude
        /// </summary>
        /// <param name="alt">Alt to get the max temperature at</param>
        public double GetMaxTemperatureAtAlt(double alt)
        {
            //Thanks NathanKell for this
            return body.GetTemperature(alt)                                     // base temperature
                 + (body.atmosphereTemperatureSunMultCurve.Evaluate((float)alt) // altitude-based multiplier to temperature delta
                  * (body.latitudeTemperatureBiasCurve.Evaluate(0f) + body.latitudeTemperatureSunMultCurve.Evaluate(1f)
                                                                    + body.axialTemperatureSunMultCurve.Evaluate((float)Math.Sin(body.orbit.inclination * (Math.PI / 180d)))));
        }
    }
    #endregion
}
