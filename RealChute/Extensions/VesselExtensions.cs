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

public static class VesselExtensions
{
    #region Methods
    /// <param name="vessel">Vessel to get the altitude for</param>
    extension(Vessel vessel)
    {
        /// <summary>
        /// Returns the AGL altitude from this vessel at the desired position
        /// </summary>
        /// <param name="asl">Sea level altitude of this vessel</param>
        public double GetTrueAlt(double asl)
        {
            if (vessel.mainBody.pqsController == null) { return asl; }
            double terrainAlt = vessel.pqsAltitude;
            return vessel.mainBody.ocean && terrainAlt <= 0d ? asl : asl - terrainAlt;
        }

        /// <summary>
        /// If the given vessel is a Kerbal engineer
        /// </summary>
        public static bool IsEngineer() => FlightGlobals.ActiveVessel.evaController.part.protoModuleCrew[0]
                                                        .experienceTrait.Config.Name == "Engineer";
    }
    #endregion
}
