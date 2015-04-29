using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute.EVA
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

        private EVAChute[] _chuteList = new EVAChute[0];
        public EVAChute[] chuteList
        {
            get { return this._chuteList; }
        }

        private string[] _names = new string[0];
        public string[] names
        {
            get { return this._names; }
        }
        #endregion

        #region Constructor
        public EVAChuteLibrary()
        {
            GameDatabase.Instance.GetConfigNodes("EVACHUTES").Select(n => new EVAChute(n))
                .ToDictionary(c => c.name, c => c);
            this._chuteList = this._chutes.Values.ToArray();
            this._names = this._chuteList.Select(c => c.name).ToArray();
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
