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

namespace RealChute.Utils
{
    /// <summary>
    /// Various Enum utilities methods, featuring fast parsing/ToString. Should ONLY ever be called through EnumUtils.
    /// </summary>
    /// <typeparam name="TEnum">Type should be Enum. Really, it's gonna throw if you don't.</typeparam>
    public abstract class EnumConstraint<TEnum> where TEnum : class
    {
        /// <summary>
        /// Generic enum conversion utility class
        /// </summary>
        private struct EnumConverter
        {
            #region Fields
            /// <summary>
            /// Stores the enum -> string conversion
            /// </summary>
            private Dictionary<TEnum, string> names;

            /// <summary>
            /// Stores the string -> enum conversion
            /// </summary>
            private Dictionary<string, TEnum> values;

            /// <summary>
            /// Stores the index of each member with the member as the key
            /// </summary>
            private Dictionary<TEnum, int> valueIndexes;

            /// <summary>
            /// Stores the index of each member with it's string representation
            /// </summary>
            private Dictionary<string, int> nameIndexes;

            /// <summary>
            /// The name of the enum values correctly ordered for index search
            /// </summary>
            public string[] orderedNames;

            /// <summary>
            /// The values of the Enum correctly ordered for index search
            /// </summary>
            public TEnum[] orderedValues;
            #endregion

            #region Constructor
            /// <summary>
            /// Creates a new EnumConvertor from the given type
            /// </summary>
            /// <param name="enumType">Type of converter. Must be an enum type.</param>
            public EnumConverter(Type enumType)
            {
                if (enumType == null) { throw new ArgumentNullException("enumType", "Enum conversion type cannot be null"); }

                Array values = Enum.GetValues(enumType);
                this.orderedNames = Enum.GetNames(enumType);
                int length = this.orderedNames.Length;
                this.orderedValues = new TEnum[length];
                this.names = new Dictionary<TEnum, string>(length);
                this.values = new Dictionary<string, TEnum>(length);
                this.valueIndexes = new Dictionary<TEnum, int>(length);
                this.nameIndexes = new Dictionary<string, int>(length);
                for (int i = 0; i < length; i++)
                {
                    TEnum value = (TEnum)values.GetValue(i);
                    string name = this.orderedNames[i];
                    this.orderedValues[i] = value;
                    this.values.Add(name, value);
                    this.names.Add(value, name);
                    this.valueIndexes.Add(value, i);
                    this.nameIndexes.Add(name, i);
                }
            }
            #endregion

            #region Methods
            /// <summary>
            /// Tries to parse the given Enum member and stores the result in the out parameter. Returns false if it fails.
            /// </summary>
            /// <param name="name">String to parse</param>
            /// <param name="value">Value to store the result into</param>
            public bool TryGetValue<T>(string name, out T value) where T : struct, TEnum
            {
                TEnum result;
                if(values.TryGetValue(name, out result))
                {
                    value = (T)result;
                    return true;
                }

                value = default(T);
                return false;

            }

            /// <summary>
            /// Tries to get the string name of the Enum value and stores it in the out parameter. Returns false if it fails.
            /// </summary>
            /// <param name="value">Enum to get the string for</param>
            /// <param name="name">Value to store the result into</param>
            public bool TryGetName<T>(T value, out string name) where T : struct, TEnum
            {
                return this.names.TryGetValue(value, out name);
            }

            /// <summary>
            /// Tries to get the Enum value at the given index
            /// </summary>
            /// <typeparam name="T">Type of the Enum</typeparam>
            /// <param name="index">Index of the value to get</param>
            /// <param name="value">Value to store the result into</param>
            public bool TryGetValueAt<T>(int index, out T value) where T : struct, TEnum
            {
                if(orderedValues.IndexInRange(index))
                {
                    value = (T)orderedValues[index];
                    return true;
                }
                value = default(T);
                return false;
            }

            /// <summary>
            /// Tries to get the Enum member name at the given index
            /// </summary>
            /// <typeparam name="T">Type of the Enum</typeparam>
            /// <param name="index">Index of the name to find</param>
            /// <param name="name">Value to store the result into</param>
            public bool TryGetNameAt<T>(int index, out string name) where T : struct, TEnum
            {
                if (orderedNames.IndexInRange(index))
                {
                    name = orderedNames[index];
                    return true;
                }
                name = string.Empty;
                return false;
            }

            /// <summary>
            /// Finds the index of a given enum name
            /// </summary>
            /// <typeparam name="T">Type of the Enum</typeparam>
            /// <param name="name">Enum member name to find the index of</param>
            public int IndexOf<T>(string name) where T : struct, TEnum
            {
                int index;
                if (!this.nameIndexes.TryGetValue(name, out index)) { return -1; }
                return index;
            }

            /// <summary>
            /// Finds the index of a given Enum member
            /// </summary>
            /// <typeparam name="T">Type of the Enum</typeparam>
            /// <param name="value">Enum value to find the index of</param>
            public int IndexOf<T>(T value) where T : struct, TEnum
            {
                int index;
                if (!this.valueIndexes.TryGetValue(value, out index)) { return -1; }
                return index;
            }
            #endregion
        }

        #region Constructor
        /// <summary>
        /// Prevents external instantiation
        /// </summary>
        internal EnumConstraint() { }
        #endregion

        #region Fields
        /// <summary>
        /// Holds all the known enum converters
        /// </summary>
        private static Dictionary<Type, EnumConverter> converters = new Dictionary<Type, EnumConverter>();
        #endregion

        #region Methods
        /// <summary>
        /// Returns the converter of the given type or creates one if there are none
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <param name="enumType">Type of the enum conversion</param>
        private static EnumConverter GetConverter<T>()
        {
            EnumConverter converter;
            Type enumType = typeof(T);
            if (!converters.TryGetValue(enumType, out converter))
            {
                if (!enumType.IsEnum) { throw new ArgumentException("Type is not an Enum type", "T"); }
                converter = new EnumConverter(enumType);
                converters.Add(enumType, converter);
            }
            return converter;
        }

        /// <summary>
        /// Returns the string value of an Enum
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <param name="value">Enum value to convert to string</param>
        public static string GetName<T>(T value) where T : struct, TEnum
        {
            string result = string.Empty;
            GetConverter<T>().TryGetName(value, out result);
            return result;
        }

        /// <summary>
        /// Parses the given string to the given Enum type 
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <param name="name">String to parse</param>
        public static T GetValue<T>(string name) where T : struct, TEnum
        {
            T result;
            GetConverter<T>().TryGetValue(name, out result);
            return result;
        }

        /// <summary>
        /// Finds the string name of the enum value at the given index
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <param name="index">Index of the name to find</param>
        public static string GetNameAt<T>(int index) where T : struct, TEnum
        {
            string name;
            GetConverter<T>().TryGetNameAt<T>(index, out name);
            return name;
        }

        /// <summary>
        /// Gets the enum value at the given index
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <param name="index">Index of the element to get</param>
        public static T GetValueAt<T>(int index) where T : struct, TEnum
        {
            T result;
            GetConverter<T>().TryGetValueAt(index, out result);
            return result;
        }

        /// <summary>
        /// Returns the string representation of each enum member in order
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        public static string[] GetNames<T>() where T : struct, TEnum
        {
            return GetConverter<T>().orderedNames;
        }

        /// <summary>
        /// Gets an array of all the values of the Enum
        /// </summary>
        /// <typeparam name="T">Type of the Enum</typeparam>
        public static T[] GetValues<T>() where T : struct, TEnum
        {
            return GetConverter<T>().orderedValues.ConvertAll(v => (T)v);
        }

        /// <summary>
        /// Returns the index of the Enum value of the given name
        /// </summary>
        /// <typeparam name="T">Type of the Enum</typeparam>
        /// <param name="name">Name of the element to find</param>
        public static int IndexOf<T>(string name) where T : struct, TEnum
        {
            return GetConverter<T>().IndexOf<T>(name);
        }

        /// <summary>
        /// Returns the index of the Enum member of the given value
        /// </summary>
        /// <typeparam name="T">Type of the Enum</typeparam>
        /// <param name="value">Value to find the index of</param>
        public static int IndexOf<T>(T value) where T : struct, TEnum
        {
            return GetConverter<T>().IndexOf(value);
        }

        /// <summary>
        /// Creates a GUILayout SelectionGrid which shows the names of all the members of an Enum an returns the selected value
        /// </summary>
        /// <typeparam name="T">Type of the Enum</typeparam>
        /// <param name="selected">Currently selected Enum member</param>
        /// <param name="xCount">Amount of boxes on one line</param>
        /// <param name="style">GUIStyle of the boxes</param>
        /// <param name="options">GUILayout options</param>
        public static T SelectionGrid<T>(T selected, int xCount, GUIStyle style, params GUILayoutOption[] options) where T : struct, TEnum
        {
            EnumConverter converter = GetConverter<T>();
            int index = converter.IndexOf(selected);
            index = GUILayout.SelectionGrid(index, converter.orderedNames, xCount, style, options);
            converter.TryGetValueAt(index, out selected);
            return selected;
        }

        /// <summary>
        /// Creates a GUILayout SelectionGrid with the given array of Enum members and returns the selected value
        /// </summary>
        /// <typeparam name="T"><Type of the Enum/typeparam>
        /// <param name="selected">Currently selected Enum member</param>
        /// <param name="elements">Enum members array to select through</param>
        /// <param name="xCount">Amount of selection boxes on one line</param>
        /// <param name="style">GUIStyle of the boxes</param>
        /// <param name="options">GUILayout option</param>
        public static T SelectionGrid<T>(T selected, T[] elements, int xCount, GUIStyle style, params GUILayoutOption[] options) where T : struct, TEnum
        {
            EnumConverter converter = GetConverter<T>();
            string[] names = new string[elements.Length];
            for (int i = 0; i < elements.Length; i++)
            {
                converter.TryGetName(elements[i], out names[i]);
            }
            int index = elements.IndexOf(selected);
            index = GUILayout.SelectionGrid(index, names, xCount, style, options);
            return elements[index];
        }
        #endregion
    }

    /// <summary>
    /// Enum utility class, dummy to force parameters to be Enums. Cannot be instantiated.
    /// </summary>
    public sealed class EnumUtils : EnumConstraint<Enum>
    {
        /* Nothing to see here, this is just a dummy class to force T to be an Enum.
         * The actual implementation is in EnumConstraint */

        #region Constructor
        /// <summary>
        /// Prevents object instantiation, this should act as a static class
        /// </summary>
        private EnumUtils() { }
        #endregion
    }
}