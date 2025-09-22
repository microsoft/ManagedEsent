//-----------------------------------------------------------------------
// <copyright file="jet_recpos2.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.InteropServices;

    /// <summary>
    /// The native version of the JET_RETINFO structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage(
        "Microsoft.StyleCop.CSharp.NamingRules",
        "SA1305:FieldNamesMustNotUseHungarianNotation",
        Justification = "This should match the unmanaged API, which isn't capitalized.")]
    [SuppressMessage(
        "Microsoft.StyleCop.CSharp.NamingRules",
        "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter",
        Justification = "This should match the unmanaged API, which isn't capitalized.")]
    internal struct NATIVE_RECPOS2
    {
        /// <summary>
        /// Size of NATIVE_RECPOS2 structures.
        /// </summary>
        public static readonly int Size = Marshal.SizeOf(typeof(NATIVE_RECPOS2));

        /// <summary>
        /// Size of this structure.
        /// </summary>
        public uint cbStruct;

        /// <summary>
        /// Approximate number of index entries less than the key.  Must be 0 when used with
        /// JetGotoPosition.
        /// </summary>
        public uint centriesLTDeprecated;

        /// <summary>
        /// Approximate number of entries in the index range.  Must be 0 when used with
        /// JetGotoPosition.
        /// </summary>
        public uint centriesInRange;

        /// <summary>
        /// Approximate number of entries in the index.
        /// </summary>
        public uint centriesTotalDeprecated;

        /// <summary>
        /// Approximate number of index entries less than the key.
        /// </summary>
        public ulong centriesLT;

        /// <summary>
        /// Approximate number of entries in the index.
        /// </summary>
        public ulong centriesTotal;
    }

    /// <summary>
    /// Represents a fractional position within an index. This is used by JetGotoPosition
    /// and JetGetRecordPosition.
    /// </summary>
    [SuppressMessage(
        "Microsoft.StyleCop.CSharp.NamingRules",
        "SA1300:ElementMustBeginWithUpperCaseLetter",
        Justification = "This should match the unmanaged API, which isn't capitalized.")]
    [Serializable]
    public sealed class JET_RECPOS2 : IContentEquatable<JET_RECPOS2>, IDeepCloneable<JET_RECPOS2>
    {
        /// <summary>
        /// The number of entries before the key.
        /// </summary>
        private long entriesBeforeKey;

        /// <summary>
        /// Total number of entries.
        /// </summary>
        private long totalEntries;

        /// <summary>
        /// Gets or sets the approximate number of index entries less than the key.
        /// </summary>
        public long centriesLT
        {
            [DebuggerStepThrough]
            get
            {
                return this.entriesBeforeKey;
            }

            set
            {
                this.entriesBeforeKey = value;
            }
        }

        /// <summary>
        /// Gets or sets the approximate number of entries in the index.
        /// </summary>
        public long centriesTotal
        {
            [DebuggerStepThrough]
            get
            {
                return this.totalEntries;
            }

            set
            {
                this.totalEntries = value;
            }
        }

        /// <summary>
        /// Returns a deep copy of the object.
        /// </summary>
        /// <returns>A deep copy of the object.</returns>
        public JET_RECPOS2 DeepClone()
        {
            return (JET_RECPOS2)this.MemberwiseClone();
        }

        /// <summary>
        /// Generate a string representation of the instance.
        /// </summary>
        /// <returns>The structure as a string.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "JET_RECPOS2({0}/{1})", this.entriesBeforeKey, this.totalEntries);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal
        /// to another instance.
        /// </summary>
        /// <param name="other">An instance to compare with this instance.</param>
        /// <returns>True if the two instances are equal.</returns>
        public bool ContentEquals(JET_RECPOS2 other)
        {
            if (null == other)
            {
                return false;
            }

            return this.entriesBeforeKey == other.entriesBeforeKey && this.totalEntries == other.totalEntries;
        }

        /// <summary>
        /// Get a NATIVE_RECPOS2 structure representing the object.
        /// </summary>
        /// <returns>A NATIVE_RECPOS2 whose members match the class.</returns>
        internal NATIVE_RECPOS2 GetNativeRecpos2()
        {
            var recpos2 = default(NATIVE_RECPOS2);
            recpos2.cbStruct = checked((uint)NATIVE_RECPOS2.Size);
            recpos2.centriesLTDeprecated = 0;
            recpos2.centriesTotalDeprecated = 0;
            recpos2.centriesLT = checked((ulong)this.centriesLT);
            recpos2.centriesTotal = checked((ulong)this.centriesTotal);

            return recpos2;
        }

        /// <summary>
        /// Sets the fields of the object from a NATIVE_RECPOS2 structure.
        /// </summary>
        /// <param name="value">The NATIVE_RECPOS2 which will be used to set the fields.</param>
        internal void SetFromNativeRecpos2(NATIVE_RECPOS2 value)
        {
            this.centriesLT = checked((long)value.centriesLT);
            this.centriesTotal = checked((long)value.centriesTotal);
        }
    }
}
