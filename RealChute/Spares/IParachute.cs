
/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute.Spares
{
    public enum Category
    {
        Spare,
        EVA
    }

    public interface IParachute
    {
        #region Properties
        /// <summary>
        /// Parachute fully deployed area
        /// </summary>
        float deployedArea { get; }

        /// <summary>
        /// Total canopy mass
        /// </summary>
        float chuteMass { get; }

        /// <summary>
        /// Name of the parachute
        /// </summary>
        string name { get; }

        /// <summary>
        /// Type of parachute (Spare or EVA)
        /// </summary>
        Category category { get; }
        #endregion

        #region Methods
        /// <summary>
        /// The info Confignode to save to the persistance
        /// </summary>
        ConfigNode Save();
        #endregion
    }
}
