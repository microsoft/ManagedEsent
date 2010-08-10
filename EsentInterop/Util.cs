//-----------------------------------------------------------------------
// <copyright file="Util.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;
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
        public static bool ArrayEqual(byte[] a, byte[] b, int offset, int count)
        {
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

            if (offset >= data.Length || offset + count > data.Length)
            {
                return "<invalid>";
            }

            const int MaxBytesToPrint = 8;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}", BitConverter.ToString(data, offset, Math.Min(count, MaxBytesToPrint)));
            if (count > MaxBytesToPrint)
            {
                // The output was truncated
                sb.AppendFormat("... ({0}) bytes", count);
            }

            return sb.ToString();
        }
    }
}