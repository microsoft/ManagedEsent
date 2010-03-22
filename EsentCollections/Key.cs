// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Key.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//   Code to represent a generic key value.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Collections.Generic
{
    using System;

    /// <summary>
    /// Represents a generic key value.
    /// </summary>
    /// <typeparam name="T">The datatype of the key.</typeparam>
    internal sealed class Key<T> : IEquatable<Key<T>> where T : IComparable<T>
    {
        /// <summary>
        /// Initializes a new instance of the Key class.
        /// </summary>
        /// <param name="value">The value of the key.</param>
        /// <param name="isInclusive">True if this key is inclusive.</param>
        public Key(T value, bool isInclusive)
        {
            this.Value = value;
            this.IsInclusive = isInclusive;
        }

        /// <summary>
        /// Gets a value indicating whether the key is inclusive.
        /// </summary>
        public bool IsInclusive { get; private set; }

        /// <summary>
        /// Gets the value of the key.
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// Determine if this Key matches another Key.
        /// </summary>
        /// <param name="other">The Key to compare with.</param>
        /// <returns>True if the keys are equal, false otherwise.</returns>
        public bool Equals(Key<T> other)
        {
            if (null == other)
            {
                return false;
            }

            return 0 == this.Value.CompareTo(other.Value)
                && this.IsInclusive == other.IsInclusive;
        }
    }
}