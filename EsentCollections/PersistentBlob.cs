// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersistentBlob.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//   Code that implements a collection of the values in a PersistentDictionary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Collections.Generic
{
    using System;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Represents a blob that can be persisted in a PersistentDictionary.
    /// </summary>
    public sealed class PersistentBlob : IEquatable<PersistentBlob>
    {
        /// <summary>
        /// Byte array containing the Blob.
        /// </summary>
        private readonly byte[] blobData;

        /// <summary>
        /// Hash code to detect illegal changes to the Blob.
        /// </summary>
        private readonly int blobHashCode;

        /// <summary>
        /// Initializes a new instance of the PersistentBlob class.
        /// </summary>
        /// <param name="fromBytes">The byte array to construct the Blob from.</param>
        public PersistentBlob(byte[] fromBytes)
        {
            this.blobData = fromBytes;
            if (fromBytes != null)
            {
                this.blobHashCode = fromBytes.GetHashCode();
            }
        }

        /// <summary>
        /// Test for equality.
        /// </summary>
        /// <param name="lhs">Left hand side.</param>
        /// <param name="rhs">Right hand side.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public static bool operator ==(PersistentBlob lhs, PersistentBlob rhs)
        {
            if (object.ReferenceEquals(lhs, rhs))
            {
                return true;
            }
            else if ((object)lhs == null || (object)rhs == null)
            {
                return false;
            }
            else
            {
                return lhs.Equals(rhs);
            }
        }

        /// <summary>
        /// Test for inequality.
        /// </summary>
        /// <param name="lhs">Left hand side.</param>
        /// <param name="rhs">Right hand side.</param>
        /// <returns>true if the current object is not equal to the other parameter; otherwise, false.</returns>
        public static bool operator !=(PersistentBlob lhs, PersistentBlob rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Returns the byte array representing the blob.
        /// </summary>
        /// <returns>The byte[] array.</returns>
        public byte[] GetBytes()
        {
            return this.blobData;
        }

        /// <summary>
        /// Returns a stream that can be used to read and write to the blob.
        /// </summary>
        /// <returns>The stream.</returns>
        public Stream GetStream()
        {
            throw new NotImplementedException("GetStream() not yet implemented");
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(PersistentBlob other)
        {
            // Null checks.
            if ((this == null) && (other == null))
            {
                // True if they're both null.
                return true;
            }
            else if ((this == null) || (other == null))
            {
                // False if one is null, and the other isn't.
                return false;
            }

            if (object.ReferenceEquals(this.blobData, other.blobData))
            {
                return true;
            }
            else if ((object)this.blobData == null || (object)other.blobData == null)
            {
                return false;
            }

            return this.blobData.SequenceEqual(other.blobData);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public override bool Equals(object other)
        {
            PersistentBlob blob = other as PersistentBlob;
            if (blob == null)
            {
                return false;
            }

            return this.Equals(blob);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A Int32 that contains the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return this.blobData.GetHashCode();
        }

        /// <summary>
        /// Checks that this instance hasn't changed illegaly, throws an exception if it did.
        /// </summary>
        internal void CheckImmutability()
        {
            int currentHashCode = this.blobData != null ? this.blobData.GetHashCode() : 0;
            if (currentHashCode != this.blobHashCode)
            {
                throw new InvalidOperationException("A PersistentBlob was changed in memory without being changed in the associated PersistentDictionary.");
            }
        }
    }
}
