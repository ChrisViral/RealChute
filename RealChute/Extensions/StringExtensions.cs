using System.Collections.Generic;
using System.Text;


/* RealChute was made by Christophe Savard (stupid_chris) and is licensed under CC-BY-NC-SA. You can remix, modify and
 * redistribute the work, but you must give attribution to the original author (me) and you cannot sell your derivatives.
 * For more information contact me on the forum. */

namespace RealChute.Extensions
{
    public static class StringExtensions
    {
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
            var e = seq.GetEnumerator();
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
}
