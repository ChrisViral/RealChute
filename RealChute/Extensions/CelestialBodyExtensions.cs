using System;
using System.Linq;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more information contact me on the forum. */

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
                FARMethod method = RCUtils.densityMethod;
                if (method != null)
                {
                    return method.Invoke(body, alt);
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
