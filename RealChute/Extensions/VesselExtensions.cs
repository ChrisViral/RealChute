
/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more information contact me on the forum. */

namespace RealChute.Extensions
{
    public static class VesselExtensions
    {
        #region Methods
        /// <summary>
        /// Returns the AGL altitude from this vessel at the desired position
        /// </summary>
        /// <param name="ASL">Sea level altitude of this vessel</param>
        public static double GetTrueAlt(this Vessel vessel, double ASL)
        {
            if (vessel.mainBody.pqsController == null) { return ASL; }
            double terrainAlt = vessel.pqsAltitude;
            if (vessel.mainBody.ocean && terrainAlt < 0) { terrainAlt = 0; }
            return ASL - terrainAlt;
        }
        #endregion
    }
}