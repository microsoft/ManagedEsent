//-----------------------------------------------------------------------
// <copyright file="jet_recordlist.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// The native version of the JET_RECORDLIST structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NATIVE_RECORDLIST
    {
        public uint cbStruct;
        public IntPtr tableid;
        public uint cRecords;
        public uint columnidBookmark;
    }

    /// <summary>
    /// Information about a temporary table containing information
    /// about all indexes for a given table.
    /// </summary>
    public class JET_RECORDLIST
    {
        /// <summary>
        /// Gets tableid of the temporary table. This should be closed
        /// when the table is no longer needed.
        /// </summary>
        public JET_TABLEID tableid { get; internal set; }

        /// <summary>
        /// Gets the number of records in the temporary table.
        /// </summary>
        public int cRecords { get; internal set; }

        /// <summary>
        /// Gets the columnid of the column in the temporary table which
        /// stores the bookmark of the record.
        /// The column is of type JET_coltyp.Text.
        /// </summary>
        public JET_COLUMNID columnidBookmark { get; internal set; }

        /// <summary>
        /// Sets the fields of the object from a native JET_RECORDLIST struct.
        /// </summary>
        /// <param name="value">
        /// The native recordlist to set the values from.
        /// </param>
        internal void SetFromNativeRecordlist(NATIVE_RECORDLIST value)
        {
            this.tableid = new JET_TABLEID { Value = value.tableid };
            this.cRecords = checked((int) value.cRecords);
            this.columnidBookmark = new JET_COLUMNID { Value = value.columnidBookmark };
        }
    }
}

