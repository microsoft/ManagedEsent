//-----------------------------------------------------------------------
// <copyright file="jet_columnlist.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// The native version of the JET_COLUMNLIST structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NATIVE_COLUMNLIST
    {
        public uint cbStruct;
        public IntPtr tableid;
        public uint cRecord;
        public uint columnidPresentationOrder;
        public uint columnidcolumnname;
        public uint columnidcolumnid;
        public uint columnidcoltyp;
        public uint columnidCountry;
        public uint columnidLangid;
        public uint columnidCp;
        public uint columnidCollate;
        public uint columnidcbMax;
        public uint columnidgrbit;
        public uint columnidDefault;
        public uint columnidBaseTableName;
        public uint columnidBaseColumnName;
        public uint columnidDefinitionName;
    }

    /// <summary>
    /// Information about a temporary table containing information
    /// about all columns for a given table.
    /// </summary>
    public class JET_COLUMNLIST
    {
        /// <summary>
        /// Gets tableid of the temporary table. This should be closed
        /// when the table is no longer needed.
        /// </summary>
        public JET_TABLEID tableid { get; internal set; }

        /// <summary>
        /// Gets the number of records in the temporary table.
        /// </summary>
        public int cRecord { get; internal set; }

        /// <summary>
        /// Gets the columnid of the column in the temporary table which
        /// stores the name of the column.
        /// </summary>
        public JET_COLUMNID columnidcolumnname { get; internal set; }

        /// <summary>
        /// Gets the columnid of the column in the temporary table which
        /// stores the id of the column.
        /// </summary>
        public JET_COLUMNID columnidcolumnid { get; internal set; }

        /// <summary>
        /// Gets the columnid of the column in the temporary table which
        /// stores the type of the column.
        /// </summary>
        public JET_COLUMNID columnidcoltyp { get; internal set; }

        /// <summary>
        /// Gets the columnid of the column in the temporary table which
        /// stores the code page of the column.
        /// </summary>
        public JET_COLUMNID columnidCp { get; internal set; }

        /// <summary>
        /// Gets the columnid of the column in the temporary table which
        /// stores the maximum length of the column.
        /// </summary>
        public JET_COLUMNID columnidcbMax { get; internal set; }

        /// <summary>
        /// Gets the columnid of the column in the temporary table which
        /// stores the grbit of the column.
        /// </summary>
        public JET_COLUMNID columnidgrbit { get; internal set; }

        /// <summary>
        /// Gets the columnid of the column in the temporary table which
        /// stores the default value of the column.
        /// </summary>
        public JET_COLUMNID columnidDefault { get; internal set; }

        /// <summary>
        /// Sets the fields of the object from a native JET_COLUMNLIST struct.
        /// </summary>
        /// <param name="value">
        /// The native columnlist to set the values from.
        /// </param>
        internal void SetFromNativeColumnlist(NATIVE_COLUMNLIST value)
        {
            this.tableid = new JET_TABLEID { Value = value.tableid };
            this.cRecord = (int)value.cRecord;
            this.columnidcolumnname = new JET_COLUMNID { Value = value.columnidcolumnname };
            this.columnidcolumnid = new JET_COLUMNID { Value = value.columnidcolumnid };
            this.columnidcoltyp = new JET_COLUMNID { Value = value.columnidcoltyp };
            this.columnidCp = new JET_COLUMNID { Value = value.columnidCp };
            this.columnidcbMax = new JET_COLUMNID { Value = value.columnidcbMax };
            this.columnidgrbit = new JET_COLUMNID { Value = value.columnidgrbit };
            this.columnidDefault = new JET_COLUMNID { Value = value.columnidDefault };
        }
    }
}