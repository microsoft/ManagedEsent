//-----------------------------------------------------------------------
// <copyright file="jet_enumcolumnvalue.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// Native (unmanaged) version of the JET_ENUMCOLUMNVALUE class.
    /// </summary>
    internal struct NATIVE_ENUMCOLUMNVALUE
    {
        public uint itagSequence;
        public int err;
        public uint cbData;
        public IntPtr pvData;
    }

    /// <summary>
    /// Enumerates the column values of a record using the JetEnumerateColumns
    /// function. <see cref="Api.JetEnumerateColumns"/> returns an array of JET_ENUMCOLUMNVALUE
    /// structures. The array is returned in memory that was allocated using
    /// the callback that was supplied to that function.
    /// </summary>
    public class JET_ENUMCOLUMNVALUE
    {
        /// <summary>
        /// Gets the column value (by one-based index) that was enumerated.
        /// </summary>
        public int itagSequence { get; internal set; }

        /// <summary>
        /// Gets the column status code resulting from the enumeration of the
        /// column value.
        /// </summary>
        /// <seealso cref="JET_wrn.ColumnNull"/>
        /// <seealso cref="JET_wrn.ColumnSkipped"/>
        /// <seealso cref="JET_wrn.ColumnTruncated"/>
        public JET_wrn err { get; internal set; }

        /// <summary>
        /// Gets the size of the column value for the column.
        /// </summary>
        public int cbData { get; internal set; }

        /// <summary>
        /// Gets the value that was enumerated for the column.
        /// </summary>
        public IntPtr pvData { get; internal set; }

        /// <summary>
        /// Sets the fields of the object from a native JET_ENUMCOLUMN struct.
        /// </summary>
        /// <param name="value">
        /// The native enumcolumn to set the values from.
        /// </param>
        internal void SetFromNativeEnumColumnValue(NATIVE_ENUMCOLUMNVALUE value)
        {
            this.itagSequence = checked((int) value.itagSequence);
            this.err = (JET_wrn) value.err;
            this.cbData = checked((int) value.cbData);
            this.pvData = value.pvData;
        }
    }
}