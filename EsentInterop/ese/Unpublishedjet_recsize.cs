//-----------------------------------------------------------------------
// <copyright file="Unpublishedjet_recsize.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

#if !MANAGEDESENT_ON_WSA // Not exposed in MSDK
namespace Microsoft.Isam.Esent.Interop.Vista
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;

    using Microsoft.Isam.Esent.Interop.Vista;

    // FUTURE-2020/07/13-UmairA - JetGetRecordSize3() is unpublished as of yet.
    // When it is published, remove this file and merge with jet_recsize.cs.

    /// <summary>
    /// Used by <see cref="VistaApi.JetGetRecordSize"/> to return information about a record's usage
    /// requirements in user data space, number of set columns, number of
    /// values, and ESENT record structure overhead space.
    /// </summary>
    [SuppressMessage(
        "Microsoft.StyleCop.CSharp.NamingRules",
        "SA1300:ElementMustBeginWithUpperCaseLetter",
        Justification = "This should match the unmanaged API, which isn't capitalized.")]
    public partial struct JET_RECSIZE : IEquatable<JET_RECSIZE>
    {
        /// <summary>
        /// User data stored in intrinsic LVs (in the record).
        /// </summary>
        public long cbIntrinsicLongValueData { get; internal set; }

        /// <summary>
        /// Compressed size of user data stored in intrinsic LVs (in the record). Same as
        /// cbIntrinsicLongValueData if no intrinsic LVs are compressed.
        /// </summary>
        public long cbIntrinsicLongValueDataCompressed { get; internal set; }

        /// <summary>
        /// Total number of intrinsic LVs stored in the record.
        /// </summary>
        public long cIntrinsicLongValues { get; internal set; }

        /// <summary>
        /// Key size in bytes. Doesn't include storage overhead. Does include key normalization overhead.
        /// </summary>
        public long cbKey { get; internal set; }

        /// <summary>
        /// Sets the fields of the object from a NATIVE_RECSIZE3 struct.
        /// </summary>
        /// <param name="value">
        /// The native recsize to set the values from.
        /// </param>
        internal void SetFromNativeRecsize(NATIVE_RECSIZE3 value)
        {
            checked
            {
                this.cbData = (long)value.cbData;
                this.cbDataCompressed = (long)value.cbDataCompressed;
                this.cbLongValueData = (long)value.cbLongValueData;
                this.cbLongValueDataCompressed = (long)value.cbLongValueDataCompressed;
                this.cbLongValueOverhead = (long)value.cbLongValueOverhead;
                this.cbOverhead = (long)value.cbOverhead;
                this.cCompressedColumns = (long)value.cCompressedColumns;
                this.cLongValues = (long)value.cLongValues;
                this.cMultiValues = (long)value.cMultiValues;
                this.cNonTaggedColumns = (long)value.cNonTaggedColumns;
                this.cTaggedColumns = (long)value.cTaggedColumns;
                this.cbIntrinsicLongValueData = (long)value.cbIntrinsicLongValueData;
                this.cbIntrinsicLongValueDataCompressed = (long)value.cbIntrinsicLongValueDataCompressed;
                this.cIntrinsicLongValues = (long)value.cIntrinsicLongValues;
                this.cbKey = (long)value.cbKey;
            }
        }

        /// <summary>
        /// Gets a NATIVE_RECSIZE3 containing the values in this object.
        /// </summary>
        /// <returns>
        /// A NATIVE_RECSIZE3 initialized with the values in the object.
        /// </returns>
        internal NATIVE_RECSIZE3 GetNativeRecsize3()
        {
            unchecked
            {
                return new NATIVE_RECSIZE3
                {
                    cbData = (ulong)this.cbData,
                    cbDataCompressed = (ulong)this.cbDataCompressed,
                    cbLongValueData = (ulong)this.cbLongValueData,
                    cbLongValueDataCompressed = (ulong)this.cbLongValueDataCompressed,
                    cbLongValueOverhead = (ulong)this.cbLongValueOverhead,
                    cbOverhead = (ulong)this.cbOverhead,
                    cCompressedColumns = (ulong)this.cCompressedColumns,
                    cLongValues = (ulong)this.cLongValues,
                    cMultiValues = (ulong)this.cMultiValues,
                    cNonTaggedColumns = (ulong)this.cNonTaggedColumns,
                    cTaggedColumns = (ulong)this.cTaggedColumns,
                    cbIntrinsicLongValueData = (ulong)this.cbIntrinsicLongValueData,
                    cbIntrinsicLongValueDataCompressed = (ulong)this.cbIntrinsicLongValueDataCompressed,
                    cIntrinsicLongValues = (ulong)this.cIntrinsicLongValues,
                    cbKey = (ulong)this.cbKey,
                };
            }
        }

        /// <summary>
        /// Extends Add for JET_RECSIZE3 members.
        /// </summary>
        /// <param name="result">The result of the addition.</param>
        /// <param name="s1">The first JET_RECSIZE.</param>
        /// <param name="s2">The second JET_RECSIZE.</param>
        static partial void AddPartial(ref JET_RECSIZE result, JET_RECSIZE s1, JET_RECSIZE s2)
        {
            checked
            {
                result.cbIntrinsicLongValueData = s1.cbIntrinsicLongValueData + s2.cbIntrinsicLongValueData;
                result.cbIntrinsicLongValueDataCompressed = s1.cbIntrinsicLongValueDataCompressed + s2.cbIntrinsicLongValueDataCompressed;
                result.cIntrinsicLongValues = s1.cIntrinsicLongValues + s2.cIntrinsicLongValues;
                result.cbKey = s1.cbKey + s2.cbKey;
            }
        }

        /// <summary>
        /// Extends Subtract for JET_RECSIZE3 members.
        /// </summary>
        /// <param name="result">The result of the subtraction.</param>
        /// <param name="s1">The first JET_RECSIZE.</param>
        /// <param name="s2">The second JET_RECSIZE.</param>
        static partial void SubtractPartial(ref JET_RECSIZE result, JET_RECSIZE s1, JET_RECSIZE s2)
        {
            checked
            {
                result.cbIntrinsicLongValueData = s1.cbIntrinsicLongValueData - s2.cbIntrinsicLongValueData;
                result.cbIntrinsicLongValueDataCompressed = s1.cbIntrinsicLongValueDataCompressed - s2.cbIntrinsicLongValueDataCompressed;
                result.cIntrinsicLongValues = s1.cIntrinsicLongValues - s2.cIntrinsicLongValues;
                result.cbKey = s1.cbKey - s2.cbKey;
            }
        }

        /// <summary>
        /// Extends Equals for JET_RECSIZE3 members.
        /// </summary>
        /// <param name="result">The result of the comparison.</param>
        /// <param name="other">An object to compare with this instance.</param>
        partial void EqualsPartial(ref bool result, JET_RECSIZE other)
        {
            result = result
                   && this.cbIntrinsicLongValueData == other.cbIntrinsicLongValueData
                   && this.cbIntrinsicLongValueDataCompressed == other.cbIntrinsicLongValueDataCompressed
                   && this.cIntrinsicLongValues == other.cIntrinsicLongValues
                   && this.cbKey == other.cbKey;
        }
    }

    /// <summary>
    /// The native version of the JET_RECSIZE3 structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules",
        "SA1305:FieldNamesMustNotUseHungarianNotation",
        Justification = "This should match the unmanaged API, which isn't capitalized.")]
    [SuppressMessage(
        "Microsoft.StyleCop.CSharp.NamingRules",
        "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter",
        Justification = "This should match the unmanaged API, which isn't capitalized.")]
    internal struct NATIVE_RECSIZE3
    {
        /// <summary>
        /// User data in record.
        /// </summary>
        public ulong cbData;

        /// <summary>
        /// User data associated with the record but stored in the long-value
        /// tree. Does NOT count intrinsic long-values.
        /// </summary>
        public ulong cbLongValueData;

        /// <summary>
        /// Record overhead.
        /// </summary>
        public ulong cbOverhead;

        /// <summary>
        /// Overhead of long-value data. Does not count intrinsic long-values.
        /// </summary>
        public ulong cbLongValueOverhead;

        /// <summary>
        /// Total number of fixed/variable columns.
        /// </summary>
        public ulong cNonTaggedColumns;

        /// <summary>
        /// Total number of tagged columns.
        /// </summary>
        public ulong cTaggedColumns;

        /// <summary>
        /// Total number of values stored in the long-value tree for this record.
        /// Does NOT count intrinsic long-values.
        /// </summary>
        public ulong cLongValues;

        /// <summary>
        /// Total number of values beyond the first for each column in the record.
        /// </summary>
        public ulong cMultiValues;

        /// <summary>
        /// Total number of columns which are compressed.
        /// </summary>
        public ulong cCompressedColumns;

        /// <summary>
        /// Compressed size of user data in record. Same as cbData if no
        /// long-values are compressed.
        /// </summary>
        public ulong cbDataCompressed;

        /// <summary>
        /// Compressed size of user data in the long-value tree. Same as
        /// cbLongValue data if no separated long values are compressed.
        /// </summary>
        public ulong cbLongValueDataCompressed;

        /// <summary>
        /// User data stored in intrinsic LVs (in the record).
        /// </summary>
        public ulong cbIntrinsicLongValueData;

        /// <summary>
        /// Compressed size of user data stored in intrinsic LVs (in the record). Same as
        /// cbIntrinsicLongValueData if no intrinsic LVs are compressed.
        /// </summary>
        public ulong cbIntrinsicLongValueDataCompressed;

        /// <summary>
        /// Total number of intrinsic LVs stored in the record.
        /// </summary>
        public ulong cIntrinsicLongValues;

        /// <summary>
        /// Key size in bytes. Doesn't include storage overhead. Does include key normalization overhead.
        /// </summary>
        public ulong cbKey;
    }
}
#endif // !MANAGEDESENT_ON_WSA