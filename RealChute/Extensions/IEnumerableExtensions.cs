using System;
using System.Collections.Generic;

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

public static class EnumerableExtensions
{
    #region Methods
    /// <param name="array">Array to look into</param>
    /// <typeparam name="T">Type of the array</typeparam>
    extension<T>(T[] array)
    {
        /// <summary>
        /// If the given array contains the value looked for
        /// </summary>
        /// <param name="value">Element to find in the array</param>
        public bool Contains(T value) => Array.IndexOf(array, value) != -1;

        /// <summary>
        /// Returns true if one of the elements of the array matches the given predicate
        /// </summary>
        /// <param name="match">Match for the element to find</param>
        public bool Exists(Predicate<T> match) => Array.Exists(array, match);

        /// <summary>
        /// Returns true only if the given index is within the range of the array (above 0 and under the lenght)
        /// </summary>
        /// <param name="index">Index to use</param>
        public bool IndexInRange(int index) => index >= 0 && index < array.Length;

        /// <summary>
        /// Executes the given action on all the elements of the array
        /// </summary>
        /// <param name="action">Action to execute on each member of the array</param>
        public void ForEach(Action<T> action) => Array.ForEach(array, action);
    }

    /// <param name="seq">Sequence to execute in</param>
    /// <typeparam name="T">Type of the elements</typeparam>
    extension<T>(IEnumerable<T> seq)
    {
        /// <summary>
        /// Executes the given action on each member of the IEnumerable
        /// </summary>
        /// <param name="action">Action to execute</param>
        public void ForEach(Action<T> action)
        {
            if (seq == null) { throw new ArgumentNullException(nameof(seq)); }
            if (action == null) { throw new ArgumentNullException(nameof(action)); }

            foreach (T t in seq)
            {
                action(t);
            }
        }
    }
    #endregion
}
