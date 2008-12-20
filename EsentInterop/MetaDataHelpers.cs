//-----------------------------------------------------------------------
// <copyright file="MetaDataHelpers.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Helper methods for the ESENT API. These methods deal with database
    /// meta-data.
    /// </summary>
    public static partial class Api
    {
        /// <summary>
        /// Creates a dictionary which maps column names to their column IDs.
        /// </summary>
        /// <param name="sesid">The sesid to use.</param>
        /// <param name="tableid">The table to retrieve the information for.</param>
        /// <returns>A dictionary mapping column names to column IDs.</returns>
        public static Dictionary<string, JET_COLUMNID> GetColumnDictionary(JET_SESID sesid, JET_TABLEID tableid)
        {
            JET_COLUMNLIST columnlist;
            Api.JetGetTableColumnInfo(sesid, tableid, string.Empty, out columnlist);
            try
            {
                // esent treats column names as case-insensitive, so we want the dictionary to be case insensitive as well
                var dict = new Dictionary<string, JET_COLUMNID>(columnlist.cRecord, StringComparer.InvariantCultureIgnoreCase);
                if (columnlist.cRecord > 0)
                {
                    Api.JetMove(sesid, columnlist.tableid, JET_Move.First, MoveGrbit.None);
                    do
                    {
                        string name = Api.RetrieveColumnAsString(sesid, columnlist.tableid, columnlist.columnidcolumnname, NativeMethods.Encoding);
                        uint columnidValue = (uint)Api.RetrieveColumnAsUInt32(sesid, columnlist.tableid, columnlist.columnidcolumnid);

                        var columnid = new JET_COLUMNID() { Value = columnidValue };
                        dict.Add(name, columnid);
                    } 
                    while (Api.TryMoveNext(sesid, columnlist.tableid));
                }
           
                return dict;
            }
            finally
            {
                // Close the temporary table used to return the results
                Api.JetCloseTable(sesid, columnlist.tableid);
            }
        }

        /// <summary>
        /// Iterates over all the columns in the table, returning information about each one.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to retrieve column information for.</param>
        /// <returns>An iterator over ColumnInfo for each column in the table.</returns>
        public static IEnumerable<ColumnInfo> GetTableColumns(JET_SESID sesid, JET_TABLEID tableid)
        {
            JET_COLUMNLIST columnlist;
            Api.JetGetTableColumnInfo(sesid, tableid, string.Empty, out columnlist);
            try
            {
                if (Api.TryMoveFirst(sesid, columnlist.tableid))
                {
                    do
                    {
                        yield return GetColumnInfoFromColumnlist(sesid, columnlist);
                    }
                    while (Api.TryMoveNext(sesid, columnlist.tableid));
                }
            }
            finally
            {
                // Close the temporary table used to return the results
                Api.JetCloseTable(sesid, columnlist.tableid);
            }
        }

        /// <summary>
        /// Iterates over all the columns in the table, returning information about each one.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database containing the table.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>An iterator over ColumnInfo for each column in the table.</returns>
        public static IEnumerable<ColumnInfo> GetTableColumns(JET_SESID sesid, JET_DBID dbid, string tableName)
        {
            JET_COLUMNLIST columnlist;
            Api.JetGetColumnInfo(sesid, dbid, tableName, string.Empty, out columnlist);
            try
            {
                if (Api.TryMoveFirst(sesid, columnlist.tableid))
                {
                    do
                    {
                        yield return GetColumnInfoFromColumnlist(sesid, columnlist);
                    }
                    while (Api.TryMoveNext(sesid, columnlist.tableid));
                }
            }
            finally
            {
                // Close the temporary table used to return the results
                Api.JetCloseTable(sesid, columnlist.tableid);
            }
        }

        /// <summary>
        /// Returns the names of the tables in the database.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database containing the table.</param>
        /// <returns>An iterator over the names of the tables in the database.</returns>
        public static IEnumerable<string> GetTableNames(JET_SESID sesid, JET_DBID dbid)
        {
            JET_OBJECTLIST objectlist;
            Api.JetGetObjectInfo(sesid, dbid, out objectlist);
            try
            {
                if (Api.TryMoveFirst(sesid, objectlist.tableid))
                {
                    do
                    {
                        uint flags = (uint)Api.RetrieveColumnAsUInt32(sesid, objectlist.tableid, objectlist.columnidflags);
                        if (ObjectInfoFlags.System != ((ObjectInfoFlags)flags & ObjectInfoFlags.System))
                        {
                            yield return Api.RetrieveColumnAsString(sesid, objectlist.tableid, objectlist.columnidobjectname, NativeMethods.Encoding);
                        }
                    }
                    while (Api.TryMoveNext(sesid, objectlist.tableid));
                }
            }
            finally
            {
                // Close the temporary table used to return the results
                Api.JetCloseTable(sesid, objectlist.tableid);
            }
        }

        /// <summary>
        /// Create a ColumnInfo object from the data in the current JET_COLUMNLIST
        /// entry.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="columnlist">The columnlist to take the data from.</param>
        /// <returns>A ColumnInfo object containing the information from that record.</returns>
        private static ColumnInfo GetColumnInfoFromColumnlist(JET_SESID sesid, JET_COLUMNLIST columnlist)
        {
            string name = Api.RetrieveColumnAsString(sesid, columnlist.tableid, columnlist.columnidcolumnname, NativeMethods.Encoding);
            uint columnidValue = (uint)Api.RetrieveColumnAsUInt32(sesid, columnlist.tableid, columnlist.columnidcolumnid);
            uint coltypValue = (uint)Api.RetrieveColumnAsUInt32(sesid, columnlist.tableid, columnlist.columnidcoltyp);
            uint codepageValue = (ushort)Api.RetrieveColumnAsUInt16(sesid, columnlist.tableid, columnlist.columnidCp);
            uint maxLength = (uint)Api.RetrieveColumnAsUInt32(sesid, columnlist.tableid, columnlist.columnidcbMax);
            byte[] defaultValue = Api.RetrieveColumn(sesid, columnlist.tableid, columnlist.columnidDefault);
            uint grbitValue = (uint)Api.RetrieveColumnAsUInt32(sesid, columnlist.tableid, columnlist.columnidgrbit);

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