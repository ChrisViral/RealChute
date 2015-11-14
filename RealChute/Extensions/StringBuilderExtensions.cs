using System;
using System.Collections.Generic;
using System.Text;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get my explicit permission on the matter through a private channel, and must also distribute
 * it through the attribution clause, and must make it clear to anyone using your modification of my work that they
 * must report any problem related to this usage to you, and not to me. This clause expires if I happen to be
 * inactive (no connection) for a period of 90 days on the official KSP forums. In that case, the license reverts
 * back to CC-BY-NC-SA 4.0 INTL.*/

namespace RealChute.Extensions
{
    public static class StringBuilderExtensions
    {
        #region Methods
        //Big thanks to Majiir here for both methods
        /// <summary>
        /// Concatenates this sequence of strings together using the specified separator.
        /// </summary>
        /// <param name="separator">String separating elements</param>
        public static string Join(this IEnumerable<string> seq, string separator)
        {
            return new StringBuilder().AppendJoin(seq, separator).ToString();
        }

        /// <summary>
        /// Appends each element of the sequence with the specified separator to the StringBuilder.
        /// </summary>
        /// <param name="seq">Sequence to concatenate</param>
        /// <param name="separator">String separating each element</param>
        public static StringBuilder AppendJoin(this StringBuilder builder, IEnumerable<string> seq, string separator)
        {
            using (IEnumerator<string> e = seq.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    builder.Append(e.Current);
                    while (e.MoveNext())
                    {
                        builder.Append(separator).Append(e.Current);
                    }
                }
            }
            return builder;
        }
        #endregion
    }
}
