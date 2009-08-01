//-----------------------------------------------------------------------
// <copyright file="jet_enumcolumnid.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// Enumerates a specific set of columns and, optionally, a specific set
    /// of multiple values for those columns when the JetEnumerateColumns
    /// function is used. JetEnumerateColumns optionally takes an array of
    /// JET_ENUMCOLUMNID structures.
    /// </summary>
    public class JET_ENUMCOLUMNID
    {
        /// <summary>
        /// Gets or sets the columnid ID to enumerate.
        /// </summary>
        /// <remarks>
        /// If the column ID is 0 (zero) then the enumeration of this column is
        /// skipped and a corresponding slot in the output array of JET_ENUMCOLUMN
        /// structures will be generated with a column state of JET_wrnColumnSkipped.
        /// </remarks>
        public JET_COLUMNID columnid { get; set; }

        /// <summary>
        /// Gets or sets the count of column values (by one-based index) to
        /// enumerate for the specified column ID. If ctagSequence is 0 (zero) then
        /// rgtagSequence is ignored and all column values for the specified column
        /// ID will be enumerated.
        /// </summary>
        public int ctagSequence { get; set; }

        /// <summary>
        /// Gets or sets the array of one-based indices into the array of column values for a
        /// given column. A single element is an itagSequence which is defined in
        /// JET_RETRIEVECOLUMN. An itagSequence of 0 (zero) means "skip". An
        /// itagSequence of 1 means return the first column value of the column,
        /// 2 means the second, and so on.
        /// </summary>
        public int[] rgtagSequence { get; set; }
    }
}