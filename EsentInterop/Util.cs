//-----------------------------------------------------------------------
// <copyright file="Util.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Static utility methods.
    /// </summary>
    internal static class Util
    {
        /// <summary>
        /// Compare two byte arrays to see if they have the same content.
        /// </summary>
        /// <param name="a">The first array.</param>
        /// <param name="b">The second array.</param>
        /// <param name="offset">The offset to start comparing at.</param>
        /// <param name="count">The number of bytes to compare.</param>
        /// <returns>True if the arrays are equal, false otherwise.</returns>
        public static bool ArrayEqual(IList<byte> a, IList<byte> b, int offset, int count)
        {
            if (a == null && b == null)
            {
                return true;
            }

            for (int i = 0; i < count; ++i)
            {
                if (a[offset + i] != b[offset + i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Return a string containing (some of) the bytes.
        /// </summary>
        /// <param name="data">The data to dump.</param>
        /// <param name="offset">The starting offset.</param>
        /// <param name="count">The count.</param>
        /// <returns>A string version of the data.</returns>
        public static string DumpBytes(byte[] data, int offset, int count)
        {
            if (null == data)
            {
                return "<null>";
            }

            if (0 == count)
            {
                return String.Empty;
            }

            if (offset < 0 || count < 0 || offset >= data.Length || offset + count > data.Length)
            {
                return "<invalid>";
            }

            const int MaxBytesToPrint = 8;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}", BitConverter.ToString(data, offset, Math.Min(count, MaxBytesToPrint)));
            if (count > MaxBytesToPrint)
            {
                // The output was truncated
                sb.AppendFormat("... ({0} bytes)", count);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Compares two objects with ContentEquals.
        /// If both are null, there are considered equal.
        /// </summary>
        /// <typeparam name="T">A type that implements IContentEquatable.</typeparam>
        /// <param name="left">First object to compare.</param>
        /// <param name="right">Second object to compare.</param>
        /// <returns>Whether the two objects are equal.</returns>
        public static bool ObjectContentEquals<T>(T left, T right)
            where T : class, IContentEquatable<T>
        {
            if (null == left || null == right)
            {
                return Object.ReferenceEquals(left, right);
            }

            return left.ContentEquals(right);
        }

        /// <summary>
        /// Compares two objects with ContentEquals.
        /// If both are null, there are considered equal.
        /// </summary>
        /// <typeparam name="T">A type that implements IContentEquatable.</typeparam>
        /// <param name="left">First object to compare.</param>
        /// <param name="right">Second object to compare.</param>
        /// <returns>Whether the two objects are equal.</returns>
        public static bool ArrayObjectContentEquals<T>(T[] left, T[] right)
            where T : class, IContentEquatable<T>
        {
            if (null == left || null == right)
            {
                return Object.ReferenceEquals(left, right);
            }

            if (left.Length != right.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Length; ++i)
            {
                if (!ObjectContentEquals(left[i], right[i]))
                {
                    return false;
                }
            }

            // All the individual members are equal, all of the elements of the arrays are
            // equal, so they must be equal!
            return true;
        }

        /// <summary>
        /// Given a list of hash codes calculate a hash of the hashes.
        /// </summary>
        /// <param name="hashes">The sub hash codes.</param>
        /// <returns>A hash of the hash codes.</returns>
        public static int CalculateHashCode(params int[] hashes)
        {
            int hash = 0;
            foreach (int h in hashes)
            {
                hash ^= h;
                unchecked
                {
                    hash *= 33;
                }
            }

            return hash;
        }
    }
}