//-----------------------------------------------------------------------
// <copyright file="EnumerableColumnInfo.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// Iterates over all the columns in the table, returning information about each one.
    /// </summary>
    internal class EnumerableColumnInfo : IEnumerable<ColumnInfo>
    {
        private readonly JET_SESID sesid;
        private readonly JET_TABLEID tableid;

        /// <summary>
        /// Initializes a new instance of the EnumerableColumnInfo class.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve column information for.</param>
        public EnumerableColumnInfo(JET_SESID sesid, JET_TABLEID tableid)
        {
            this.sesid = sesid;
            this.tableid = tableid;
        }

        #region IEnumerable<ColumnInfo> Members

        /// <summary>
        /// Returns an enumerator that iterates through the ColumnInfo objects describing
        /// the columns in the table.
        /// </summary>
        /// <returns>
        /// An enumerator that iterates through the ColumnInfo objects describing
        /// the columns in the table
        /// </returns>
        public IEnumerator<ColumnInfo> GetEnumerator()
        {
            JET_COLUMNLIST columnlist;
            Api.JetGetTableColumnInfo(this.sesid, this.tableid, string.Empty, out columnlist);
            return new TableEnumerator<ColumnInfo>(
                this.sesid, columnlist.tableid, i => this.GetColumnInfoFromColumnlist(columnlist));
        }

        /// <summary>
        /// Returns an enumerator that iterates through the ColumnInfo objects describing
        /// the columns in the table.
        /// </summary>
        /// <returns>
        /// An enumerator that iterates through the ColumnInfo objects describing
        /// the columns in the table
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Create a ColumnInfo object from the data in the current JET_COLUMNLIST
        /// entry.
        /// </summary>
        /// <param name="columnlist">The columnlist to take the data from.</param>
        /// <returns>A ColumnInfo object containing the information from that record.</returns>
        private ColumnInfo GetColumnInfoFromColumnlist(JET_COLUMNLIST columnlist)
        {
            string name = Api.RetrieveColumnAsString(
                this.sesid, columnlist.tableid, columnlist.columnidcolumnname, NativeMethods.Encoding);
            var columnidValue = (uint)Api.RetrieveColumnAsUInt32(this.sesid, columnlist.tableid, columnlist.columnidcolumnid);
            var coltypValue = (uint)Api.RetrieveColumnAsUInt32(this.sesid, columnlist.tableid, columnlist.columnidcoltyp);
            uint codepageValue = (ushort)Api.RetrieveColumnAsUInt16(this.sesid, columnlist.tableid, columnlist.columnidCp);
            var maxLength = (uint)Api.RetrieveColumnAsUInt32(this.sesid, columnlist.tableid, columnlist.columnidcbMax);
            byte[] defaultValue = Api.RetrieveColumn(this.sesid, columnlist.tableid, columnlist.columnidDefault);
            var grbitValue = (uint)Api.RetrieveColumnAsUInt32(this.sesid, columnlist.tableid, columnlist.columnidgrbit);

            return new ColumnInfo(
                name,
                new JET_COLUMNID() { Value = columnidValue },
                (JET_coltyp)coltypValue,
                (JET_CP)codepageValue,
                (int)maxLength,
                defaultValue,
                (ColumndefGrbit)grbitValue);
        }
    }
}