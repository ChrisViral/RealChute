using System;
using System.Collections.Generic;
using System.Linq;
using RealChute.Extensions;
using UnityEngine;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get explicit permission on the matter by opening an issue on the RealChute GitHub repository.
 * If approved, you will be required to distribute through the attribution clause, and must make it clear to anyone using
 * your modification of my work that they must report any problem related to this usage to you, and not to me.
 * This exemption can be approved by anyone managing the RealChute repository at the time, be it the original author or anyone
 * that the project was handed to. Should the repository not exist anymore, or should you not receive an answer to your issue
 * within a period of thirty (30) days, you are granted permission to distribute RealChute under the license CC-BY-NC-SA 4.0 INTL. */

namespace RealChute.Libraries;

public class AtmoPlanets
{
    #region Fetch
    /// <summary>
    /// Accesses the atmospheric planets library
    /// </summary>
    public static AtmoPlanets Instance => field ??= new AtmoPlanets();
    #endregion

    #region Propreties
    /// <summary>
    /// Body name/body dictionary of all the CelestialBodies with an atmosphere and a surface for quick name lookup
    /// </summary>
    private Dictionary<string, CelestialBody> Bodies { get; } = new();

    /// <summary>
    /// CelestialBody list for quick index lookup
    /// </summary>
    public string[] BodyNames { get; } = [];
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new instance of PlanetInfo
    /// </summary>
    private AtmoPlanets()
    {
        if (FlightGlobals.Bodies.Count > 0)
        {
            this.Bodies = FlightGlobals.Bodies.Where(b => b.atmosphere && b.pqsController != null)
                                       .ToDictionary(b => b.bodyName, b => b);
            this.BodyNames = [..this.Bodies.Keys];
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Checks if the atmospheric bodies contains the body of the given name
    /// </summary>
    /// <param name="name">Name of the CelestialBody</param>
    private bool ContainsBody(string name) => this.Bodies.ContainsKey(name);

    /// <summary>
    /// Finds the atmospheric CelestialBody of the given name
    /// </summary>
    /// <param name="name">Name of the body</param>
    private CelestialBody GetBody(string name) => ContainsBody(name)
                                                      ? this.Bodies[name]
                                                      : throw new KeyNotFoundException($"Could not find the \"{name}\" CelestialBody in the library");

    /// <summary>
    /// Finds the atmospheric CelestialBody at the given index
    /// </summary>
    /// <param name="index">Index of the body</param>
    public CelestialBody GetBody(int index) => this.BodyNames.IndexInRange(index)
                                                   ? GetBody(this.BodyNames[index])
                                                   : throw new IndexOutOfRangeException($"CelestialBody index [{index}] is out of range");

    /// <summary>
    /// Returns the index of the CelestialBody within the dictionary
    /// </summary>
    /// <param name="name">CelestialBody searched for</param>
    /// <param name="index">Index of the body</param>
    /// <returns></returns>
    public bool TryGetBodyIndex(string name, ref int index)
    {
        if (ContainsBody(name))
        {
            index = this.BodyNames.IndexOf(name);
            return true;
        }
        if (!string.IsNullOrEmpty(name) && this.BodyNames.Length > 0) { Debug.LogError($"[RealChute]: Could not find the atmospheric CelestialBody \"{name}\" in the library"); }
        return false;
    }
    #endregion
}
