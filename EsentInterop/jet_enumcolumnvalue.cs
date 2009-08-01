//-----------------------------------------------------------------------
// <copyright file="jet_enumcolumnvalue.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// Enumerates the column values of a record using the JetEnumerateColumns
    /// function. JetEnumerateColumns returns an array of JET_ENUMCOLUMNVALUE
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
    }
}