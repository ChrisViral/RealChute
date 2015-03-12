using System.Collections.Generic;
using RealChute.Extensions;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute
{
    public static class EnumUtils
    {
        #region Dictionaries
        /// <summary>
        /// quick string -> DeploymentStates conversion dictionary
        /// </summary>
        private static readonly Dictionary<string, DeploymentStates> states = new Dictionary<string, DeploymentStates>    
        #region States
        {
            { string.Empty, DeploymentStates.NONE },
            { "STOWED", DeploymentStates.STOWED },
            { "PREDEPLOYED", DeploymentStates.PREDEPLOYED },
            { "LOWDEPLOYED", DeploymentStates.LOWDEPLOYED },
            { "DEPLOYED", DeploymentStates.DEPLOYED },
            { "CUT", DeploymentStates.CUT }
        };
        #endregion

        /// <summary>
        /// Quick DeploymentStates -> string conversion dictionary
        /// </summary>
        private static readonly Dictionary<DeploymentStates, string> stateStrings = new Dictionary<DeploymentStates, string>    
        #region States
        {
            { DeploymentStates.NONE, string.Empty },
            { DeploymentStates.STOWED, "STOWED" },
            { DeploymentStates.PREDEPLOYED, "PREDEPLOYED" },
            { DeploymentStates.LOWDEPLOYED, "LOWDEPLOYED" },
            { DeploymentStates.DEPLOYED, "DEPLOYED" },
            { DeploymentStates.CUT, "CUT" }
        };
        #endregion

        /// <summary>
        /// Quick string -> ParachuteType conversion dictionary
        /// </summary>
        private static readonly Dictionary<int, ParachuteType> typesDict = new Dictionary<int, ParachuteType>
        #region Types
        {
            { -1, ParachuteType.NONE},
            { 0, ParachuteType.MAIN},
            { 1, ParachuteType.DROGUE},
            { 2, ParachuteType.DRAG}
        };
        #endregion

        /// <summary>
        /// Quick ParachuteState -> string conversion dictionary
        /// </summary>
        private static readonly Dictionary<ParachuteType, int> typeStrings = new Dictionary<ParachuteType, int>
        #region Types
        {
            { ParachuteType.NONE, -1 },
            { ParachuteType.MAIN, 0 },
            { ParachuteType.DROGUE, 1 },
            { ParachuteType.DRAG, 2 }
        };
        #endregion
        #endregion

        #region Arrays
        public static readonly string[] types = { "Main", "Drogue", "Drag" };
        #endregion

        #region Methods
        /// <summary>
        /// Gets the DeploymentStates equivalent to the given string
        /// </summary>
        /// <param name="state">String element to get the DeploymentStates from</param>
        public static DeploymentStates GetState(string state)
        {
            return states[state];
        }

        /// <summary>
        /// Gets the string equivalent to the given DeploymentStates
        /// </summary>
        /// <param name="state">DeploymentState element to get the string from</param>
        public static string GetStateString(DeploymentStates state)
        {
            return stateStrings[state];
        }

        /// <summary>
        /// If there exists a type of the given name
        /// </summary>
        /// <param name="type">Name of the type</param>
        public static bool ContainsType(string type)
        {
            return types.Contains(type);
        }

        /// <summary>
        /// The int index of the ParachuteType of the given name
        /// </summary>
        /// <param name="type">String representation of the ParachuteType</param>
        public static int IndexOfType(string type)
        {
            return types.IndexOf(type);
        }

        /// <summary>
        /// Get the ParachuteType equivalent to the given index
        /// </summary>
        /// <param name="type">Interget index to get the ParachuteType from</param>
        public static ParachuteType GetType(int type)
        {
            return typesDict[type];
        }

        /// <summary>
        /// Get the index equivalent to the given ParachuteType
        /// </summary>
        /// <param name="type">ParachuteType to get the string from</param>
        public static int GetTypeID(ParachuteType type)
        {
            return typeStrings[type];
        }
        #endregion
    }
}
