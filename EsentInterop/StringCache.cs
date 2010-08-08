//-----------------------------------------------------------------------
// <copyright file="StringCache.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;

    /// <summary>
    /// Class that helps cache strings.
    /// </summary>
    internal static class StringCache
    {
        /// <summary>
        /// Return the interned version of a string, or the original
        /// string if it isn't interned.
        /// </summary>
        /// <param name="s">The string to try to intern.</param>
        /// <returns>An interned copy of the string or the original string.</returns>
        public static string TryToIntern(string s)
        {
            return String.IsInterned(s) ?? s;
        }

        /// <summary>
        /// Convert a byte array to a string.
        /// </summary>
        /// <param name="value">The bytes to convert.</param>
        /// <param name="startIndex">The starting index of the data to convert.</param>
        /// <param name="count">The number of bytes to convert.</param>
        /// <returns>A string converted from the data.</returns>
        public static string GetString(byte[] value, int startIndex, int count)
        {
            unsafe
            {
                fixed (byte* data = value)
                {
                    char* chars = (char*)(data + startIndex);
                    return GetString(chars, 0, count / sizeof(char));
                }
            }            
        }

        /// <summary>
        /// Convert a char array to a string.
        /// </summary>
        /// <param name="value">The characters to convert.</param>
        /// <param name="startIndex">The starting index of the data to convert.</param>
        /// <param name="count">The number of characters to convert.</param>
        /// <returns>A string converted from the data.</returns>
        public static unsafe string GetString(char* value, int startIndex, int count)
        {
            if (0 == count)
            {
                return String.Empty;
            }

            // Encoding.Unicode.GetString copies the data to an array of chars and then
            // makes a string from it, copying the data twice. Use the more efficient
            // char* constructor.
            return new string(value, startIndex, count);
        }
    }
}