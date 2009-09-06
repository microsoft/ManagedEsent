//-----------------------------------------------------------------------
// <copyright file="Key.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Collections.Generic
{
    /// <summary>
    /// Represents a generic key value.
    /// </summary>
    /// <typeparam name="T">The datatype of the key.</typeparam>
    internal sealed class Key<T>
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
    }
}