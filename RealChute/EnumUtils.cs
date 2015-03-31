using System;
using System.Collections.Generic;
using RealChute.Extensions;

namespace RealChute
{
    public abstract class EnumConstraint<TEnum> where TEnum : class
    {
        /// <summary>
        /// Generic enum conversion utility class
        /// </summary>
        private class EnumConverter
        {
            #region Fields
            /// <summary>
            /// Stores the string -> enum conversion
            /// </summary>
            private Dictionary<string, TEnum> values = new Dictionary<string, TEnum>();

            /// <summary>
            /// Stores the enum -> string conversion
            /// </summary>
            private Dictionary<TEnum, string> names = new Dictionary<TEnum, string>();

            /// <summary>
            /// The name of the enum values correctly ordered for index search
            /// </summary>
            public string[] orderedNames = new string[0];

            /// <summary>
            /// The values of the Enum correctly ordered for index search
            /// </summary>
            public TEnum[] orderedValues = new TEnum[0];
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
                this.orderedNames = new string[values.Length];
                this.orderedValues = new TEnum[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    TEnum value = (TEnum)values.GetValue(i);
                    string name = Enum.GetName(enumType, value);
                    this.orderedNames[i] = name;
                    this.orderedValues[i] = value;
                    this.values.Add(name, value);
                    this.names.Add(value, name);
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
                bool success = values.TryGetValue(name, out result);
                value = (T)result;
                return success;

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
            #endregion
        }

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
        /// <param name="enumType">Type of the enum conversion</param>
        private static EnumConverter GetConverter(Type enumType)
        {
            EnumConverter converter;
            if (!converters.TryGetValue(enumType, out converter))
            {
                converter = new EnumConverter(enumType);
                converters.Add(enumType, converter);
            }
            return converter;
        }

        /// <summary>
        /// Returns the string value of an Enum
        /// </summary>
        /// <param name="value">Enum value to convert to string</param>
        public static string GetName<T>(T value) where T : struct, TEnum
        {
            string result;
            GetConverter(typeof(T)).TryGetName(value, out result);
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
            GetConverter(typeof(T)).TryGetValue(name, out result);
            return result;
        }

        /// <summary>
        /// Gets the enum value at the given index
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <param name="index">Index of the element to get</param>
        public static T GetValueAt<T>(int index) where T : struct, TEnum
        {
            EnumConverter converter = GetConverter(typeof(T));
            if (!converter.orderedNames.IndexInRange(index)) { return default(T); }
            T result;
            converter.TryGetValue(converter.orderedNames[index], out result);
            return result;
        }

        /// <summary>
        /// Finds the string name of the enum value at the given index
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        /// <param name="index">Index of the name to find</param>
        public static string GetNameAt<T>(int index) where T : struct, TEnum
        {
            EnumConverter converter = GetConverter(typeof(T));
            if (!converter.orderedNames.IndexInRange(index)) { return null; }
            return converter.orderedNames[index];
        }

        /// <summary>
        /// Returns the string representation of each enum member in order
        /// </summary>
        /// <typeparam name="T">Type of the enum</typeparam>
        public static string[] GetNames<T>() where T : struct, TEnum
        {
            return GetConverter(typeof(T)).orderedNames;
        }

        /// <summary>
        /// Gets an array of all the values of the Enum
        /// </summary>
        /// <typeparam name="T">Type of the Enum</typeparam>
        public static T[] GetValues<T>() where T : struct, TEnum
        {
            return Array.ConvertAll(GetConverter(typeof(T)).orderedValues, v => (T)v);
        }

        /// <summary>
        /// Returns the index of the Enum value of the given name
        /// </summary>
        /// <typeparam name="T">Type of the Enum</typeparam>
        /// <param name="name">Name of the element to find</param>
        public static int IndexOf<T>(string name) where T : struct, TEnum
        {
            return GetNames<T>().IndexOf(name);
        }

        /// <summary>
        /// Returns the index of the Enum member of the given value
        /// </summary>
        /// <typeparam name="T">Type of the Enum</typeparam>
        /// <param name="value">Value to find the index of</param>
        public static int IndexOf<T>(T value) where T : struct, TEnum
        {
            return GetValues<T>().IndexOf(value);
        }
        #endregion
    }

    public class EnumUtils : EnumConstraint<Enum>
    {
        /* Nothing to see here, this is just a dummy class to force T to be an Enum.
         * The actual implementation is in EnumConstraint */
    }
}