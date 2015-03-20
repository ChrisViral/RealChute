using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RealChute.Libraries
{
    public class EVAChuteLibrary
    {
        #region Instance
        private static EVAChuteLibrary _instance = null;
        public static EVAChuteLibrary instance
        {
            get
            {
                if (_instance == null) { _instance = new EVAChuteLibrary(); }
                return _instance;
            }
        }
        #endregion

        #region Properties
        private Dictionary<string, EVAChute> _chutes = new Dictionary<string, EVAChute>();
        public Dictionary<string, EVAChute> chutes
        {
            get { return this._chutes; }
        }
        #endregion

        #region Constructor
        public EVAChuteLibrary()
        {
            GameDatabase.Instance.GetConfigNodes("EVACHUTES").Select(n => new EVAChute(n))
                .ToDictionary(c => c.name, c => c);
        }
        #endregion

        #region Methods
        /// <summary>
        /// If the EVAChute of the given name exists
        /// </summary>
        /// <param name="name">Name of the EVAChute to find</param>
        /// <returns></returns>
        public bool ContainsChute(string name)
        {
            return this._chutes.ContainsKey(name);
        }

        /// <summary>
        /// Finds the EVAChute of the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public EVAChute GetChute(string name)
        {
            if (!ContainsChute(name)) { throw new KeyNotFoundException("Could not find the \"" + name + "\" EVAChute in the library"); }
            return this._chutes[name];
        }

        /// <summary>
        /// Tries to find the EVAChute of the given name and stores it in the given value
        /// </summary>
        /// <param name="name">Name of the EVAChute to find</param>
        /// <param name="chute">Value to store the result into</param>
        public bool TryGetChute(string name, ref EVAChute chute)
        {
            if (ContainsChute(name))
            {
                chute = this._chutes[name];
                return true;
            }
            if (!string.IsNullOrEmpty(name)) { Debug.LogError("[RealChute]: Could not find the EVAChute \"" + name + "\" in the library"); }
            return false;
        }
        #endregion
    }
}
