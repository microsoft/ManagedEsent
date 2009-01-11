//-----------------------------------------------------------------------
// <copyright file="Conversions.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Provide methods to convert data and flags between 
    /// Win32 and the .NET Framework.
    /// </summary>
    internal static class Conversions
    {
        /// <summary>
        /// Maps a CompareOption enumeration to the corresponding LCMapString flag.
        /// </summary>
        private static readonly Dictionary<CompareOptions, uint> compareOptionsToLcmapFlags;

        /// <summary>
        /// Maps an LCMapString flag to the corresponding CompareOption enumeration.
        /// </summary>
        private static readonly Dictionary<uint, CompareOptions> lcmapFlagsToCompareOptions;

        /// <summary>
        /// Initializes static members of the Conversions class. This sets up the
        /// conversion mapping dictionaries.
        /// </summary>
        static Conversions()
        {
            // Rather than creating both dictionaries, define one as the inverse of the other.
            Conversions.compareOptionsToLcmapFlags = new Dictionary<CompareOptions, uint>()
            {
                { CompareOptions.IgnoreCase, NativeMethods.NORM_IGNORECASE },
                { CompareOptions.IgnoreKanaType, NativeMethods.NORM_IGNOREKANATYPE },
                { CompareOptions.IgnoreNonSpace, NativeMethods.NORM_IGNORENONSPACE },
                { CompareOptions.IgnoreSymbols, NativeMethods.NORM_IGNORESYMBOLS },
                { CompareOptions.IgnoreWidth, NativeMethods.NORM_IGNOREWIDTH },
            };

            Conversions.lcmapFlagsToCompareOptions = Conversions.InvertDictionary(Conversions.compareOptionsToLcmapFlags);
        }

        /// <summary>
        /// Given flags for LCMapFlags, turn them into compare options. Unknown options 
        /// are ignored.
        /// </summary>
        /// <param name="lcmapFlags">LCMapString flags.</param>
        /// <returns>CompareOptions describing the (known) flags.</returns>
        public static CompareOptions CompareOptionsFromLCmapFlags(uint lcmapFlags)
        {
            // This should be a template, but there isn't an elegant way to express than with C# generics
            CompareOptions options = CompareOptions.None;
            foreach (var flag in Conversions.lcmapFlagsToCompareOptions.Keys)
            {
                if (flag == (lcmapFlags & flag))
                {
                    options |= Conversions.lcmapFlagsToCompareOptions[flag];
                }
            }

            return options;
        }

        /// <summary>
        /// Give CompareOptions, turn them into flags from LCMapString. Unknown options are ignored.
        /// </summary>
        /// <param name="compareOptions">The options to convert.</param>
        /// <returns>The LCMapString flags that match the compare options. Unsupported options are ignored.</returns>
        public static uint LCMapFlagsFromCompareOptions(CompareOptions compareOptions)
        {
            // This should be a template, but there isn't an elegant way to express than with C# generics
            uint flags = 0;
            foreach (var option in Conversions.compareOptionsToLcmapFlags.Keys)
            {
                if (option == (compareOptions & option))
                {
                    flags |= Conversions.compareOptionsToLcmapFlags[option];
                }
            }

            return flags;
        }

        /// <summary>
        /// Given a Key=>Value dictionary create an inverted dictionary that maps Value=>Key
        /// </summary>
        /// <typeparam name="TValue">The new value type (the key of the current dictionary)</typeparam>
        /// <typeparam name="TKey">The new key type (the value if the current dictionary)</typeparam>
        /// <param name="dict">The dictionary to invert.</param>
        /// <returns>An inverted dictionary.</returns>
        private static Dictionary<TKey, TValue> InvertDictionary<TValue, TKey>(Dictionary<TValue, TKey> dict)
        {
            var invertedDict = new Dictionary<TKey, TValue>();
            foreach (var entry in dict)
            {
                invertedDict.Add(entry.Value, entry.Key);
            }

            return invertedDict;
        }
    }
}
