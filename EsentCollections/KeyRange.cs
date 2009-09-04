//-----------------------------------------------------------------------
// <copyright file="KeyRange.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Text;

namespace Microsoft.Isam.Esent.Collections.Generic
{
    /// <summary>
    /// Represents a range of keys, where each end can be inclusive or
    /// exclusive.
    /// </summary>
    /// <typeparam name="T">The type of the key.</typeparam>
    internal class KeyRange<T> where T : IComparable<T>
    {
        /// <summary>
        /// Initializes a new instance of the KeyRange class.
        /// </summary>
        /// <param name="min">The minimum key. This can be null.</param>
        /// <param name="max">The maximum key. This can be null.</param>
        public KeyRange(Key<T> min, Key<T> max)
        {
            this.Min = min;
            this.Max = max;
        }

        /// <summary>
        /// Gets the minimum key value. This is null if there is
        /// no minumum.
        /// </summary>
        public Key<T> Min { get; private set; }

        /// <summary>
        /// Gets the maximum key value. This is null if there is
        /// no maximum.
        /// </summary>
        public Key<T> Max { get; private set; }

        /// <summary>
        /// Return the intersection of two ranges.
        /// </summary>
        /// <param name="a">The first range.</param>
        /// <param name="b">The second range.</param>
        /// <returns>The intersection of the two ranges.</returns>
        public static KeyRange<T> operator &(KeyRange<T> a, KeyRange<T> b)
        {
            return new KeyRange<T>(LowerBound(a.Min, b.Min), GetUpperBound(a.Max, b.Max));
        }

        /// <summary>
        /// Generate a string representation of the range.
        /// </summary>
        /// <returns>A string representation of the range.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("KeyRange: ");
            if (null != this.Min)
            {
                sb.AppendFormat("min = {0} ({1}), ", this.Min.Value, this.Min.IsInclusive ? "inclusive" : "exclusive");
            }
            else
            {
                sb.Append("min = null, ");
            }

            if (null != this.Max)
            {
                sb.AppendFormat("max = {0} ({1})", this.Max.Value, this.Max.IsInclusive ? "inclusive" : "exclusive");
            }
            else
            {
                sb.Append("max = null");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns the lower bound of two keys. This is the maximum of the
        /// keys, where null represents the minimum value.
        /// </summary>
        /// <param name="a">The first key.</param>
        /// <param name="b">The second key.</param>
        /// <returns>The larger of the two keys.</returns>
        private static Key<T> LowerBound(Key<T> a, Key<T> b)
        {
            Key<T> min;
            if (null == a && null != b)
            {
                min = b;
            }
            else if (null != a && null == b)
            {
                min = a;
            }
            else if (null != a && null != b)
            {
                int compare = a.Value.CompareTo(b.Value);
                if (0 == compare)
                {
                    // Prefer the exclusive range
                    min = a.IsInclusive ? b : a;
                }
                else
                {
                    min = compare > 0 ? a : b;
                }
            }
            else
            {
                min = null;
            }

            return min;
        }

        /// <summary>
        /// Returns the upper bound of two keys. This is the minimum of the
        /// keys, where null represents the maximum value.
        /// </summary>
        /// <param name="a">The first key.</param>
        /// <param name="b">The second key.</param>
        /// <returns>The smaller of the two keys.</returns>
        private static Key<T> GetUpperBound(Key<T> a, Key<T> b)
        {
            Key<T> max;
            if (null == a && null != b)
            {
                max = b;
            }
            else if (null != a && null == b)
            {
                max = a;
            }
            else if (null != a && null != b)
            {
                int compare = a.Value.CompareTo(b.Value);
                if (0 == compare)
                {
                    // Prefer the exclusive range
                    max = a.IsInclusive ? b : a;
                }
                else
                {
                    max = compare < 0 ? a : b;
                }
            }
            else
            {
                max = null;
            }

            return max;
        }
    }
}