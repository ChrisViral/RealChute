using System.Collections.Generic;
using System.Linq;

/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more informtion contact me on the forum. */

namespace RealChute
{
    public class AtmoPlanets
    {
        #region Fetch
        private static AtmoPlanets _fetch = null;
        /// <summary>
        /// Accesses the atmospheric planets library
        /// </summary>
        public static AtmoPlanets fetch
        {
            get
            {
                if (_fetch == null) { _fetch = new AtmoPlanets(true); }
                return _fetch;
            }
        }
        #endregion

        #region Propreties
        private Dictionary<CelestialBody, string> _bodies = new Dictionary<CelestialBody, string>();
        /// <summary>
        /// List of all the celestial bodies who have an atmosphere
        /// </summary>
        public Dictionary<CelestialBody, string> bodies
        {
            get { return this._bodies; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an empty AtmoPlanets
        /// </summary>
        public AtmoPlanets() { }

        /// <summary>
        /// Creates a new instance of PlanetInfo
        /// </summary>
        public AtmoPlanets(bool load)
        {
            if (load) { _bodies = FlightGlobals.Bodies.Where(b => b.atmosphere && b.bodyName != "Jool").ToDictionary(b => b, b => b.bodyName); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns the CelestialBody of the given name
        /// </summary>
        /// <param name="name">Name of the body</param>
        public CelestialBody GetBody(string name)
        {
            return _bodies.Keys.First(key => key.bodyName == name);
        }

        /// <summary>
        /// Returns the CelestialBody according to it's index in the dictionary
        /// </summary>
        /// <param name="index">Index of the body</param>
        public CelestialBody GetBody(int index)
        {
            return _bodies.Keys.ToArray()[index];
        }

        /// <summary>
        /// Returns the index of the CelestialBody within the dictionary
        /// </summary>
        /// <param name="name">CelestialBody searched for</param>
        /// <returns></returns>
        public int GetPlanetIndex(string name)
        {
            return bodies.Values.ToList().IndexOf(name);
        }

        /// <summary>
        /// Returns the the values of the dictionary in an array
        /// </summary>
        public string[] GetNames()
        {
            return bodies.Values.ToArray();
        }
        #endregion
    }
}
