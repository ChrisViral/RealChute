using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RealChute.Extensions;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute.Libraries
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
                if (_fetch == null) { _fetch = new AtmoPlanets(); }
                return _fetch;
            }
        }
        #endregion

        #region Propreties
        private Dictionary<string, CelestialBody> _bodies = new Dictionary<string, CelestialBody>();
        /// <summary>
        /// Body name/body dictionary of all the CelestialBodies with an atmosphere and a surface for quick name lookup
        /// </summary>
        public Dictionary<string, CelestialBody> bodies
        {
            get { return this._bodies; }
        }

        private string[] _bodyNames = new string[0];
        /// <summary>
        /// CelestialBody list for quick index lookup
        /// </summary>
        public string[] bodyNames
        {
            get { return this._bodyNames; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of PlanetInfo
        /// </summary>
        public AtmoPlanets()
        {
            if (FlightGlobals.Bodies.Count > 0)
            {
                this._bodies = FlightGlobals.Bodies.Where(b => b.atmosphere && b.pqsController != null)
                    .ToDictionary(b => b.bodyName, b => b);
                this._bodyNames = this._bodies.Keys.ToArray();
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Checks if the atmpospheric bodies contains the body of the given name
        /// </summary>
        /// <param name="name">Name of the CelestialBody</param>
        public bool ContainsBody(string name)
        {
            return this._bodies.ContainsKey(name);
        }

        /// <summary>
        /// Finds the atmospheric CelestialBody of the given name
        /// </summary>
        /// <param name="name">Name of the body</param>
        public CelestialBody GetBody(string name)
        {
            if (!ContainsBody(name)) { throw new KeyNotFoundException("Could not find the \"" + name + "\" CelestialBody in the library"); }
            return this._bodies[name];
        }

        /// <summary>
        /// Finds the atmospheric CelestialBody at the given index
        /// </summary>
        /// <param name="index">Index of the body</param>
        public CelestialBody GetBody(int index)
        {
            if (this._bodyNames.IndexInRange(index)) { throw new IndexOutOfRangeException("CelestialBody index [" + index + "] is out of range"); }
            return GetBody(this._bodyNames[index]);
        }

        /// <summary>
        /// Returns the index of the CelestialBody within the dictionary
        /// </summary>
        /// <param name="name">CelestialBody searched for</param>
        /// <returns></returns>
        public bool TryGetBodyIndex(string name, ref int index)
        {
            if (ContainsBody(name))
            {
                index = this._bodyNames.IndexOf(name);
                return true;
            }
            Debug.LogError("[RealChute]: Could not find the atmospheric CelestialBody \"" + name + "\" in the library");
            return false;
        }
        #endregion
    }
}
