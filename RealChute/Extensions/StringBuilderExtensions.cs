using System.Collections.Generic;
using System.Text;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get explicit permission on the matter by opening an issue on the RealChute GitHub repository.
 * If approved, you will be required to distribute through the attribution clause, and must make it clear to anyone using
 * your modification of my work that they must report any problem related to this usage to you, and not to me.
 * This exemption can be approved by anyone managing the RealChute repository at the time, be it the original author or anyone
 * that the project was handed to. Should the repository not exist anymore, or should you not receive an answer to your issue
 * within a period of thirty (30) days, you are granted permission to distribute RealChute under the license CC-BY-NC-SA 4.0 INTL. */

namespace RealChute.Extensions;

public static class StringBuilderExtensions
{
    #region Methods
    /// <param name="builder">StringBuilder to append on</param>
    extension(StringBuilder builder)
    {
        //Big thanks to Majiir here for both methods
        /// <summary>
        /// Appends each element of the sequence with the specified separator to the StringBuilder.
        /// </summary>
        /// <param name="seq">Sequence to concatenate</param>
        /// <param name="separator">String separating each element</param>
        public StringBuilder AppendJoin(IEnumerable<string> seq, string separator)
        {
            using IEnumerator<string> e = seq.GetEnumerator();
            if (e.MoveNext())
            {
                builder.Append(e.Current);
                while (e.MoveNext())
                {
                    builder.Append(separator).Append(e.Current);
                }

            }

            return builder;
        }
    }
    #endregion
}
