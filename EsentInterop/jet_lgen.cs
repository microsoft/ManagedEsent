// ---------------------------------------------------------------------------
// <copyright file="jet_lgen.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    using LGEN = global::System.Int32;

    /// <summary>
    /// Describes an offset in the log sequence.
    /// </summary>
    [SuppressMessage(
        "Microsoft.StyleCop.CSharp.NamingRules",
        "SA1300:ElementMustBeginWithUpperCaseLetter",
        Justification = "This should match the name of the unmanaged structure.")]
    [StructLayout(LayoutKind.Sequential)]
    [Serializable]
    public struct JET_LGEN : IEquatable<JET_LGEN>, IComparable<JET_LGEN>
    {
        /// <summary>
        /// Generation number.
        /// </summary>
        private readonly LGEN value;

        /// <summary>
        /// Gets a null JET_LGEN.
        /// </summary>
        public static readonly JET_LGEN Nil = (JET_LGEN)0;

        /// <summary>
        /// The invalid JET_LGEN value.
        /// </summary>
        public static readonly JET_LGEN Invalid = (JET_LGEN)0x7FFFFFFF;

        /// <summary>
        /// Gets or sets the generation of this log generation.
        /// </summary>
        /// <remarks>
        /// It's exposed to faciliate the existing code which use int or unsigned int. 
        /// They all need to be ready for the 31 -> 32bit switch and start to use JET_LGEN granularlly. 
        /// This property will be retired by then and replaced with the explict cast to int or unsigned int.
        /// </remarks>

        /// <summary>
        /// Private Ctor
        /// </summary>
        /// <param name="lgen"></param>
        private JET_LGEN(LGEN lgen)
        {
            this.value = lgen;
#if DEBUG && ESENT
#pragma warning disable CS0618 // Type or member is obsolete
            // To preserve the compatibility with the existing code in Windows / ESENT,
            // we keep the old public properties and leave them as signed int as always.
            JET_LGEN.ExpectType<JET_LGPOS, Int32>((t) => t.lGeneration);
            JET_LGEN.ExpectType<JET_BKINFO, Int32>((t) => t.genLow);
            JET_LGEN.ExpectType<JET_BKINFO, Int32>((t) => t.genHigh);
            JET_LGEN.ExpectType<JET_DBINFOMISC, Int32>((t) => t.genMinRequired);
            JET_LGEN.ExpectType<JET_DBINFOMISC, Int32>((t) => t.genMaxRequired);
            JET_LGEN.ExpectType<JET_DBINFOMISC, Int32>((t) => t.genCommitted);
#pragma warning restore CS0618 // Type or member is obsolete
#endif
        }

        /// <summary>
        /// An explicit conversion from JET_LGEN to int
        /// </summary>
        /// <param name="lgen"></param>
        public static explicit operator LGEN(JET_LGEN lgen)
        {
            return lgen.value;
        }

        /// <summary>
        /// An explicit conversion from int to JET_LGEN
        /// </summary>
        /// <param name="lgen"></param>
        public static explicit operator JET_LGEN(LGEN lgen)
        {
            return new JET_LGEN(lgen);
        }

        /// <summary>
        /// == operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator ==(JET_LGEN lhs, JET_LGEN rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// != operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator !=(JET_LGEN lhs, JET_LGEN rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// &lt; operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator <(JET_LGEN lhs, JET_LGEN rhs)
        {
            return lhs.CompareTo(rhs) < 0;
        }

        /// <summary>
        /// > operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator >(JET_LGEN lhs, JET_LGEN rhs)
        {
            return lhs.CompareTo(rhs) > 0;
        }

        /// <summary>
        /// &lt;= operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator <=(JET_LGEN lhs, JET_LGEN rhs)
        {
            return lhs.CompareTo(rhs) <= 0;
        }

        /// <summary>
        /// >= operator
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator >=(JET_LGEN lhs, JET_LGEN rhs)
        {
            return lhs.CompareTo(rhs) >= 0;
        }

        /// <summary>
        /// + operator
        /// </summary>
        /// <param name="lgen"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static JET_LGEN operator +(JET_LGEN lgen, int offset)
        {
            return (JET_LGEN)(lgen.value + offset);
        }

        /// <summary>
        /// - operator
        /// </summary>
        /// <param name="lgen"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static JET_LGEN operator -(JET_LGEN lgen, int offset)
        {
            return (JET_LGEN)(lgen.value - offset);
        }

        /// <summary>
        /// - operator
        /// </summary>
        /// <param name="lgenA"></param>
        /// <param name="lgenB"></param>
        /// <returns></returns>
        public static int operator -(JET_LGEN lgenA, JET_LGEN lgenB)
        {
            return lgenA.value - lgenB.value;
        }

        /// <summary>
        /// ++ operator
        /// </summary>
        /// <param name="lgen"></param>
        /// <returns></returns>
        public static JET_LGEN operator ++(JET_LGEN lgen)
        {
            // lgen++ is translated lgen = lgen.operator++(lgen);
            return lgen + 1;
        }

        /// <summary>
        /// -- operator
        /// </summary>
        /// <param name="lgen"></param>
        /// <returns></returns>
        public static JET_LGEN operator --(JET_LGEN lgen)
        {
            // lgen-- is translated lgen = lgen.operator--(lgen);
            return lgen - 1;
        }

        /// <summary>
        /// Returns the maximum of two JET_LGEN values.
        /// </summary>
        /// <param name="lgenA"></param>
        /// <param name="lgenB"></param>
        /// <returns></returns>
        public static JET_LGEN Max(JET_LGEN lgenA, JET_LGEN lgenB)
        {
            return lgenA > lgenB ? lgenA : lgenB;
        }

        /// <summary>
        /// Returns the minimum of two JET_LGEN values.
        /// </summary>
        /// <param name="lgenA"></param>
        /// <param name="lgenB"></param>
        /// <returns></returns>
        public static JET_LGEN Min(JET_LGEN lgenA, JET_LGEN lgenB)
        {
            return lgenA < lgenB ? lgenA : lgenB;
        }

        /// <summary>
        /// Generate a string representation of the structure.
        /// </summary>
        /// <returns>The structure as a string.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "0x{0:X}", this.value);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal
        /// to another instance.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>True if the two instances are equal.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }

            return this.Equals((JET_LGEN)obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return this.value;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal
        /// to another instance.
        /// </summary>
        /// <param name="other">An instance to compare with this instance.</param>
        /// <returns>True if the two instances are equal.</returns>
        public bool Equals(JET_LGEN other)
        {
            return this.value == other.value;
        }

        /// <summary>
        /// Compares this log generation to another log generation.
        /// </summary>
        /// <param name="other">The log generation to compare to the current one.</param>
        /// <returns>
        /// A signed number indicating the relative
        /// </returns>
        public int CompareTo(JET_LGEN other)
        {
            int compare = this.value.CompareTo(other.value);
            return compare;
        }

#if ESENT
        /// <summary>
        /// Use generic constraints to ensure that field type is expected.
        /// </summary>
        /// <typeparam name="TStruct"></typeparam>
        /// <typeparam name="TField"></typeparam>
        /// <param name="fieldSelector"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ExpectType<TStruct, TField>(
            Func<TStruct, TField> fieldSelector)
        {
            // The generic constraint forces both fields to be the same type TField
            // If they're different types, this won't compile
        }
#endif
    }
}