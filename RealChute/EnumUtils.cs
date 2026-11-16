using System;
using System.Collections.Generic;
using RealChute.Extensions;

/* RealChute was made by Christophe Savard (stupid_chris). You are free to copy, fork, and modify RealChute as you see
 * fit. However, redistribution is only permitted for unmodified versions of RealChute, and under attribution clause.
 * If you want to distribute a modified version of RealChute, be it code, textures, configs, or any other asset and
 * piece of work, you must get explicit permission on the matter by opening an issue on the RealChute GitHub repository.
 * If approved, you will be required to distribute through the attribution clause, and must make it clear to anyone using
 * your modification of my work that they must report any problem related to this usage to you, and not to me.
 * This excemption can be approved by anyone managing the RealChute repository at the time, be it the original author or anyone
 * that the project was handed to. Should the repository not exist anymore, or should you not receive an answer to your issue
 * within a period of thirty (30) days, you are granted permission to distribute RealChute under the license CC-BY-NC-SA 4.0 INTL. */

namespace RealChute;

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
        private readonly Dictionary<string, TEnum> values = new();

        /// <summary>
        /// Stores the enum -> string conversion
        /// </summary>
        private readonly Dictionary<TEnum, string> names = new();

        /// <summary>
        /// The name of the enum values correctly ordered for index search
        /// </summary>
        public readonly string[] orderedNames;

        /// <summary>
        /// The values of the Enum correctly ordered for index search
        /// </summary>
        public readonly TEnum[] orderedValues;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new EnumConvertor from the given type
        /// </summary>
        /// <param name="enumType">Type of converter. Must be an enum type.</param>
        public EnumConverter(Type enumType)
        {
            if (enumType == null) { throw new ArgumentNullException(nameof(enumType), "Enum conversion type cannot be null"); }
            Array val = Enum.GetValues(enumType);
            this.orderedNames  = new string[val.Length];
            this.orderedValues = new TEnum[val.Length];
            for (int i = 0; i < val.Length; i++)
            {
                TEnum value = (TEnum)val.GetValue(i);
                string name = Enum.GetName(enumType, value)!;
                this.orderedNames[i]  = name;
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
        public void TryGetValue<T>(string name, out T value) where T : struct, TEnum
        {
            this.values.TryGetValue(name, out TEnum result);
            value = (T)result!;
        }

        /// <summary>
        /// Tries to get the string name of the Enum value and stores it in the out parameter. Returns false if it fails.
        /// </summary>
        /// <param name="value">Enum to get the string for</param>
        /// <param name="name">Value to store the result into</param>
        public void TryGetName<T>(T value, out string name) where T : struct, TEnum => this.names.TryGetValue(value, out name);
        #endregion
    }

    #region Fields
    /// <summary>
    /// Holds all the known enum converters
    /// </summary>
    private static readonly Dictionary<Type, EnumConverter> Converters = new();
    #endregion

    #region Methods
    /// <summary>
    /// Returns the converter of the given type or creates one if there are none
    /// </summary>
    /// <typeparam name="T">Type of the enum</typeparam>
    private static EnumConverter GetConverter<T>() where T : struct, TEnum
    {
        Type enumType = typeof(T);
        if (!Converters.TryGetValue(enumType, out EnumConverter converter))
        {
            converter = new EnumConverter(enumType);
            Converters.Add(enumType, converter);
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
        GetConverter<T>().TryGetName(value, out string result);
        return result;
    }

    /// <summary>
    /// Parses the given string to the given Enum type
    /// </summary>
    /// <typeparam name="T">Type of the enum</typeparam>
    /// <param name="name">String to parse</param>
    public static T GetValue<T>(string name) where T : struct, TEnum
    {
        GetConverter<T>().TryGetValue(name, out T result);
        return result;
    }

    /// <summary>
    /// Gets the enum value at the given index
    /// </summary>
    /// <typeparam name="T">Type of the enum</typeparam>
    /// <param name="index">Index of the element to get</param>
    public static T GetValueAt<T>(int index) where T : struct, TEnum
    {
        EnumConverter converter = GetConverter<T>();
        if (!converter.orderedNames.IndexInRange(index)) { return default(T); }
        converter.TryGetValue(converter.orderedNames[index], out T result);
        return result;
    }

    /// <summary>
    /// Finds the string name of the enum value at the given index
    /// </summary>
    /// <typeparam name="T">Type of the enum</typeparam>
    /// <param name="index">Index of the name to find</param>
    public static string GetNameAt<T>(int index) where T : struct, TEnum
    {
        EnumConverter converter = GetConverter<T>();
        return !converter.orderedNames.IndexInRange(index) ? null : converter.orderedNames[index];
    }

    /// <summary>
    /// Returns the string representation of each enum member in order
    /// </summary>
    /// <typeparam name="T">Type of the enum</typeparam>
    public static string[] GetNames<T>() where T : struct, TEnum => GetConverter<T>().orderedNames;

    /// <summary>
    /// Gets an array of all the values of the Enum
    /// </summary>
    /// <typeparam name="T">Type of the Enum</typeparam>
    public static T[] GetValues<T>() where T : struct, TEnum => Array.ConvertAll(GetConverter<T>().orderedValues, v => (T)v);

    /// <summary>
    /// Returns the index of the Enum value of the given name
    /// </summary>
    /// <typeparam name="T">Type of the Enum</typeparam>
    /// <param name="name">Name of the element to find</param>
    public static int IndexOf<T>(string name) where T : struct, TEnum => GetNames<T>().IndexOf(name);

    /// <summary>
    /// Returns the index of the Enum member of the given value
    /// </summary>
    /// <typeparam name="T">Type of the Enum</typeparam>
    /// <param name="value">Value to find the index of</param>
    public static int IndexOf<T>(T value) where T : struct, TEnum => GetValues<T>().IndexOf(value);
    #endregion
}

// ReSharper disable once ClassCannotBeInstantiated
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class EnumUtils : EnumConstraint<Enum>
{
    /* Nothing to see here, this is just a dummy class to force T to be an Enum.
     * The actual implementation is in EnumConstraint */

    #region Constructors
    /// <summary>
    /// Prevents object instantiation
    /// </summary>
    private EnumUtils() { }
    #endregion
}
