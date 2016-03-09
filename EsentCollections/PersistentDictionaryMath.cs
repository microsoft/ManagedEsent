// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersistentDictionaryMath.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//   A collection of math-related functions useful for PersistentDictionary utilization.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Collections.Generic
{
    /// <summary>
    /// A collection of math-related functions useful for PersistentDictionary utilization.
    /// </summary>
    internal static class PersistentDictionaryMath
    {
        /// <summary>
        /// Calculates a hash code for the normalized key.
        /// </summary>
        /// <param name="normalizedKey">A byte array.</param>
        /// <returns>A hash code based on the byte array.</returns>
        /// <remarks>
        /// This is similar to Store's IEqualityComparer&lt;object[]&gt;.GetHashCode(object[] x).
        /// It is not meant to be cryptographically secure.
        /// </remarks>
        public static int GetHashCodeForKey(byte[] normalizedKey)
        {
            // This is similar to Store's IEqualityComparer<object[]>.GetHashCode(object[] x)

            // XOR-together the hash code 4-bytes at a time.
            int hashCode = normalizedKey.Length;
            for (int i = 0; i < normalizedKey.Length; ++i)
            {
                hashCode ^= normalizedKey[i];

                // Rotate the hash by one bit so that arrays with the same
                // elements in a different order will have different hash
                // values
                hashCode = hashCode << 1 | hashCode >> 31;
            }

            return hashCode;
        }
    }
}
