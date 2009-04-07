//-----------------------------------------------------------------------
// <copyright file="UncheckedApi.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// Calls to the ESENT interop layer. These calls take the managed types (e.g. JET_SESID) and
    /// return errors.
    /// </summary>
    internal static class ErrApi
    {
        private static readonly TraceSwitch traceSwitch = new TraceSwitch("ESENT P/Invoke", "P/Invoke calls to ESENT");

        #region init/term

        public static int JetCreateInstance(out JET_INSTANCE instance, string name)
        {
            ErrApi.TraceFunctionCall("JetCreateInstance");
            instance.Value = IntPtr.Zero;
            return ErrApi.Err(NativeMethods.JetCreateInstance(ref instance.Value, name));
        }

        public static int JetInit(ref JET_INSTANCE instance)
        {
            ErrApi.TraceFunctionCall("JetInit");
            return ErrApi.Err(NativeMethods.JetInit(ref instance.Value));
        }

        public static int JetTerm(JET_INSTANCE instance)
        {
            ErrApi.TraceFunctionCall("JetTerm");
            return ErrApi.Err(NativeMethods.JetTerm(instance.Value));
        }

        public static int JetTerm2(JET_INSTANCE instance, TermGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetTerm2");
            return ErrApi.Err(NativeMethods.JetTerm2(instance.Value, (uint)grbit));
        }

        public static int JetSetSystemParameter(JET_INSTANCE instance, JET_SESID sesid, JET_param paramid, int paramValue, string paramString)
        {
            ErrApi.TraceFunctionCall("JetSetSystemParameter");
            if (IntPtr.Zero == instance.Value)
            {
                return ErrApi.Err(NativeMethods.JetSetSystemParameter(IntPtr.Zero, sesid.Value, (uint)paramid, (IntPtr)paramValue, paramString));
            }
            else
            {
                GCHandle instanceHandle = GCHandle.Alloc(instance, GCHandleType.Pinned);
                try
                {
                    return ErrApi.Err(NativeMethods.JetSetSystemParameter(instanceHandle.AddrOfPinnedObject(), sesid.Value, (uint)paramid, (IntPtr)paramValue, paramString));
                }
                finally
                {
                    instanceHandle.Free();
                }
            }
        }

        public static int JetGetSystemParameter(JET_INSTANCE instance, JET_SESID sesid, JET_param paramid, ref int paramValue, out string paramString, int maxParam)
        {
            ErrApi.TraceFunctionCall("JetGetSystemParameter");

            var intValue = (IntPtr)paramValue;
            var sb = new StringBuilder(maxParam);
            int err = ErrApi.Err(NativeMethods.JetGetSystemParameter(instance.Value, sesid.Value, (uint)paramid, ref intValue, sb, (uint)maxParam));
            paramString = sb.ToString();
            paramValue = intValue.ToInt32();
            return err;
        }

        #endregion

        #region Databases

        public static int JetCreateDatabase(JET_SESID sesid, string database, string connect, out JET_DBID dbid, CreateDatabaseGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetCreateDatabase");
            ErrApi.CheckNotNull(database, "database");

            dbid = JET_DBID.Nil;
            return ErrApi.Err(NativeMethods.JetCreateDatabase(sesid.Value, database, connect, ref dbid.Value, (uint)grbit));
        }

        public static int JetAttachDatabase(JET_SESID sesid, string database, AttachDatabaseGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetAttachDatabase");
            ErrApi.CheckNotNull(database, "database");

            return ErrApi.Err(NativeMethods.JetAttachDatabase(sesid.Value, database, (uint)grbit));
        }

        public static int JetOpenDatabase(JET_SESID sesid, string database, string connect, out JET_DBID dbid, OpenDatabaseGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetOpenDatabase");
            dbid = JET_DBID.Nil;
            ErrApi.CheckNotNull(database, "database");

            return ErrApi.Err(NativeMethods.JetOpenDatabase(sesid.Value, database, connect, ref dbid.Value, (uint)grbit));
        }

        public static int JetCloseDatabase(JET_SESID sesid, JET_DBID dbid, CloseDatabaseGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetCloseDatabase");
            return ErrApi.Err(NativeMethods.JetCloseDatabase(sesid.Value, dbid.Value, (uint)grbit));
        }

        public static int JetDetachDatabase(JET_SESID sesid, string database)
        {
            ErrApi.TraceFunctionCall("JetDetachDatabase");
            ErrApi.CheckNotNull(database, "database");

            return ErrApi.Err(NativeMethods.JetDetachDatabase(sesid.Value, database));
        }

        #endregion

        #region sessions

        public static int JetBeginSession(JET_INSTANCE instance, out JET_SESID sesid, string username, string password)
        {
            ErrApi.TraceFunctionCall("JetBeginSession");
            sesid = JET_SESID.Nil;
            return ErrApi.Err(NativeMethods.JetBeginSession(instance.Value, ref sesid.Value, null, null));
        }

        /// <summary>
        /// Associates a session with the current thread using the given context
        /// handle. This association overrides the default engine requirement
        /// that a transaction for a given session must occur entirely on the
        /// same thread. 
        /// </summary>
        /// <param name="sesid">The session to set the context on.</param>
        /// <param name="context">The context to set.</param>
        /// <returns>An error if the call fails.</returns>
        public static int JetSetSessionContext(JET_SESID sesid, IntPtr context)
        {
            ErrApi.TraceFunctionCall("JetSetSessionContext");
            return ErrApi.Err(NativeMethods.JetSetSessionContext(sesid.Value, context));
        }

        /// <summary>
        /// Disassociates a session from the current thread. This should be
        /// used in conjunction with JetSetSessionContext.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <returns>An error if the call fails.</returns>
        public static int JetResetSessionContext(JET_SESID sesid)
        {
            ErrApi.TraceFunctionCall("JetResetSessionContext");
            return ErrApi.Err(NativeMethods.JetResetSessionContext(sesid.Value));
        }

        public static int JetEndSession(JET_SESID sesid, EndSessionGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetEndSession");
            return ErrApi.Err(NativeMethods.JetEndSession(sesid.Value, (uint)grbit));
        }

        public static int JetDupSession(JET_SESID sesid, out JET_SESID newSesid)
        {
            ErrApi.TraceFunctionCall("JetDupSession");
            newSesid = JET_SESID.Nil;
            return ErrApi.Err(NativeMethods.JetDupSession(sesid.Value, ref newSesid.Value));
        }

        #endregion

        #region tables

        public static int JetOpenTable(JET_SESID sesid, JET_DBID dbid, string tablename, byte[] parameters, int parametersLength, OpenTableGrbit grbit, out JET_TABLEID tableid)
        {
            ErrApi.TraceFunctionCall("JetOpenTable");
            tableid = JET_TABLEID.Nil;
            ErrApi.CheckNotNull(tablename, "tablename");

            return ErrApi.Err(NativeMethods.JetOpenTable(sesid.Value, dbid.Value, tablename, IntPtr.Zero, 0, (uint)grbit, ref tableid.Value));
        }

        public static int JetCloseTable(JET_SESID sesid, JET_TABLEID tableid)
        {
            ErrApi.TraceFunctionCall("JetCloseTable");
            return ErrApi.Err(NativeMethods.JetCloseTable(sesid.Value, tableid.Value));
        }

        public static int JetDupCursor(JET_SESID sesid, JET_TABLEID tableid, out JET_TABLEID newTableid, DupCursorGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetDupCursor");
            newTableid = JET_TABLEID.Nil;
            return ErrApi.Err(NativeMethods.JetDupCursor(sesid.Value, tableid.Value, ref newTableid.Value, (uint)grbit));
        }

        #endregion

        #region transactions

        public static int JetBeginTransaction(JET_SESID sesid)
        {
            ErrApi.TraceFunctionCall("JetBeginTransaction");
            return ErrApi.Err(NativeMethods.JetBeginTransaction(sesid.Value));
        }

        public static int JetCommitTransaction(JET_SESID sesid, CommitTransactionGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetCommitTransaction");
            return ErrApi.Err(NativeMethods.JetCommitTransaction(sesid.Value, (uint)grbit));
        }

        public static int JetRollback(JET_SESID sesid, RollbackTransactionGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetRollback");
            return ErrApi.Err(NativeMethods.JetRollback(sesid.Value, (uint)grbit));
        }

        #endregion

        #region DDL

        public static int JetCreateTable(JET_SESID sesid, JET_DBID dbid, string table, int pages, int density, out JET_TABLEID tableid)
        {
            ErrApi.TraceFunctionCall("JetCreateTable");
            tableid = JET_TABLEID.Nil;
            ErrApi.CheckNotNull(table, "table");

            return ErrApi.Err(NativeMethods.JetCreateTable(sesid.Value, dbid.Value, table, pages, density, ref tableid.Value));
        }

        public static int JetAddColumn(JET_SESID sesid, JET_TABLEID tableid, string column, JET_COLUMNDEF columndef, byte[] defaultValue, int defaultValueSize, out JET_COLUMNID columnid)
        {
            ErrApi.TraceFunctionCall("JetAddColumn");
            columnid = JET_COLUMNID.Nil;
            ErrApi.CheckNotNull(column, "column");
            ErrApi.CheckNotNull(columndef, "columndef");
            ErrApi.CheckNotNegative(defaultValueSize, "defaultValueSize");
            if ((null == defaultValue && 0 != defaultValueSize) || (null != defaultValue && defaultValueSize > defaultValue.Length))
            {
                throw new ArgumentException(
                    "defaultValueSize cannot be greater than the length of the defaultValue array",
                    "defaultValueSize");
            }

            NATIVE_COLUMNDEF nativeColumndef = columndef.GetNativeColumndef();
            int err;
            GCHandle gch = GCHandle.Alloc(defaultValue, GCHandleType.Pinned);
            try
            {
                err = ErrApi.Err(NativeMethods.JetAddColumn(
                        sesid.Value, 
                        tableid.Value, 
                        column, 
                        ref nativeColumndef,
                        gch.AddrOfPinnedObject(), 
                        (uint) defaultValueSize,
                        ref columnid.Value));
            }
            finally
            {
                gch.Free();
            }

            // esent doesn't actually set the columnid member of the passed in JET_COLUMNDEF, but we will do that here for
            // completeness.
            columndef.columnid = new JET_COLUMNID { Value = columnid.Value };
            return err;
        }

        public static int JetDeleteColumn(JET_SESID sesid, JET_TABLEID tableid, string column)
        {
            ErrApi.TraceFunctionCall("JetDeleteColumn");
            ErrApi.CheckNotNull(column, "column");

            return ErrApi.Err(NativeMethods.JetDeleteColumn(sesid.Value, tableid.Value, column));
        }

        public static int JetDeleteIndex(JET_SESID sesid, JET_TABLEID tableid, string index)
        {
            ErrApi.TraceFunctionCall("JetDeleteIndex");
            ErrApi.CheckNotNull(index, "index");

            return ErrApi.Err(NativeMethods.JetDeleteIndex(sesid.Value, tableid.Value, index));
        }

        public static int JetDeleteTable(JET_SESID sesid, JET_DBID dbid, string table)
        {
            ErrApi.TraceFunctionCall("JetDeleteTable");
            ErrApi.CheckNotNull(table, "table");

            return ErrApi.Err(NativeMethods.JetDeleteTable(sesid.Value, dbid.Value, table));
        }

        public static int JetCreateIndex(
            JET_SESID sesid,
            JET_TABLEID tableid,
            string indexName,
            CreateIndexGrbit grbit, 
            string keyDescription,
            int keyDescriptionLength,
            int density)
        {
            ErrApi.TraceFunctionCall("JetCreateIndex");
            ErrApi.CheckNotNull(indexName, "indexName");
            ErrApi.CheckNotNegative(keyDescriptionLength, "keyDescriptionLength");
            ErrApi.CheckNotNegative(density, "density");
            if (keyDescriptionLength > keyDescription.Length + 1)
            {
                throw new ArgumentException("keyDescriptionLength is greater than keyDescription", "keyDescriptionLength");
            }

            return ErrApi.Err(NativeMethods.JetCreateIndex(
                sesid.Value,
                tableid.Value,
                indexName,
                (uint)grbit,
                keyDescription,
                (uint)keyDescriptionLength,
                (uint)density));
        }

        public static int JetGetTableColumnInfo(
                JET_SESID sesid,
                JET_TABLEID tableid,
                string columnName,
                out JET_COLUMNDEF columndef)
        {
            ErrApi.TraceFunctionCall("JetGetTableColumnInfo");
            columndef = new JET_COLUMNDEF();
            ErrApi.CheckNotNull(columnName, "columnName");

            var nativeColumndef = new NATIVE_COLUMNDEF();
            nativeColumndef.cbStruct = (uint)Marshal.SizeOf(nativeColumndef);
            int err = ErrApi.Err(NativeMethods.JetGetTableColumnInfo(
                sesid.Value,
                tableid.Value,
                columnName,
                ref nativeColumndef,
                nativeColumndef.cbStruct,
                (uint)JET_ColInfo.Default));
            columndef.SetFromNativeColumndef(nativeColumndef);

            return err;
        }

        public static int JetGetTableColumnInfo(
                JET_SESID sesid,
                JET_TABLEID tableid,
                JET_COLUMNID columnid,
                out JET_COLUMNDEF columndef)
        {
            ErrApi.TraceFunctionCall("JetGetTableColumnInfo");
            columndef = new JET_COLUMNDEF();

            var nativeColumndef = new NATIVE_COLUMNDEF();
            nativeColumndef.cbStruct = (uint)Marshal.SizeOf(nativeColumndef);
            int err = ErrApi.Err(NativeMethods.JetGetTableColumnInfo(
                sesid.Value,
                tableid.Value,
                ref columnid.Value,
                ref nativeColumndef,
                nativeColumndef.cbStruct,
                (uint)JET_ColInfo.ByColid));
            columndef.SetFromNativeColumndef(nativeColumndef);

            return err;
        }

        public static int JetGetTableColumnInfo(
                JET_SESID sesid,
                JET_TABLEID tableid,
                string ignored,
                out JET_COLUMNLIST columnlist)
        {
            ErrApi.TraceFunctionCall("JetGetTableColumnInfo");
            columnlist = new JET_COLUMNLIST();

            var nativeColumnlist = new NATIVE_COLUMNLIST();
            nativeColumnlist.cbStruct = (uint)Marshal.SizeOf(nativeColumnlist);
            int err = ErrApi.Err(NativeMethods.JetGetTableColumnInfo(
                sesid.Value,
                tableid.Value,
                ignored,
                ref nativeColumnlist,
                nativeColumnlist.cbStruct,
                (uint)JET_ColInfo.List));
            columnlist.SetFromNativeColumnlist(nativeColumnlist);

            return err;
        }

        public static int JetGetColumnInfo(
                JET_SESID sesid,
                JET_DBID dbid,
                string tablename,
                string columnName,
                out JET_COLUMNDEF columndef)
        {
            ErrApi.TraceFunctionCall("JetGetColumnInfo");
            columndef = new JET_COLUMNDEF();
            ErrApi.CheckNotNull(tablename, "tablename");
            ErrApi.CheckNotNull(columnName, "columnName");

            var nativeColumndef = new NATIVE_COLUMNDEF();
            nativeColumndef.cbStruct = (uint)Marshal.SizeOf(nativeColumndef);
            int err = ErrApi.Err(NativeMethods.JetGetColumnInfo(
               sesid.Value,
               dbid.Value,
               tablename,
               columnName,
               ref nativeColumndef,
               nativeColumndef.cbStruct,
               (uint)JET_ColInfo.Default));
            columndef.SetFromNativeColumndef(nativeColumndef);

            return err;
        }

        public static int JetGetColumnInfo(
                JET_SESID sesid,
                JET_DBID dbid,
                string tablename,
                string ignored,
                out JET_COLUMNLIST columnlist)
        {
            ErrApi.TraceFunctionCall("JetGetColumnInfo");      
            columnlist = new JET_COLUMNLIST();
            ErrApi.CheckNotNull(tablename, "tablename");

            var nativeColumnlist = new NATIVE_COLUMNLIST();
            nativeColumnlist.cbStruct = (uint)Marshal.SizeOf(nativeColumnlist);
            int err = ErrApi.Err(NativeMethods.JetGetColumnInfo(
                sesid.Value,
                dbid.Value,
                tablename,
                ignored,
                ref nativeColumnlist,
                nativeColumnlist.cbStruct,
                (uint)JET_ColInfo.List));
            columnlist.SetFromNativeColumnlist(nativeColumnlist);

            return err;
        }

        public static int JetGetObjectInfo(JET_SESID sesid, JET_DBID dbid, out JET_OBJECTLIST objectlist)
        {
            ErrApi.TraceFunctionCall("JetGetObjectInfo");
            objectlist = new JET_OBJECTLIST();

            var nativeObjectlist = new NATIVE_OBJECTLIST();
            nativeObjectlist.cbStruct = (uint)Marshal.SizeOf(nativeObjectlist);
            int err = ErrApi.Err(NativeMethods.JetGetObjectInfo(
                sesid.Value,
                dbid.Value,
                (uint)JET_objtyp.Table,
                null,
                null,
                ref nativeObjectlist,
                nativeObjectlist.cbStruct,
                (uint)JET_ObjInfo.ListNoStats));
            objectlist.SetFromNativeObjectlist(nativeObjectlist);

            return err;
        }

        /// <summary>
        /// JetGetCurrentIndex function determines the name of the current
        /// index of a given cursor. This name is also used to later re-select
        /// that index as the current index using JetSetCurrentIndex. It can
        /// also be used to discover the properties of that index using
        /// JetGetTableIndexInfo.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to get the index name for.</param>
        /// <param name="indexName">Returns the name of the index.</param>
        /// <param name="maxNameLength">
        /// The maximum length of the index name. Index names are no more than 
        /// Api.MaxNameLength characters.
        /// </param>
        /// <returns>An error if the call fails.</returns>
        public static int JetGetCurrentIndex(JET_SESID sesid, JET_TABLEID tableid, out string indexName, int maxNameLength)
        {
            ErrApi.TraceFunctionCall("JetGetCurrentIndex");
            ErrApi.CheckNotNegative(maxNameLength, "maxNameLength");

            var name = new StringBuilder(maxNameLength);
            int err = ErrApi.Err(NativeMethods.JetGetCurrentIndex(sesid.Value, tableid.Value, name, (uint)maxNameLength));
            indexName = name.ToString();
            return err;
        }

        public static int JetGetIndexInfo(
                JET_SESID sesid,
                JET_DBID dbid,
                string tablename,
                string ignored,
                out JET_INDEXLIST indexlist)
        {
            ErrApi.TraceFunctionCall("JetGetIndexInfo");
            indexlist = new JET_INDEXLIST();
            ErrApi.CheckNotNull(tablename, "tablename");

            var nativeIndexlist = new NATIVE_INDEXLIST();
            nativeIndexlist.cbStruct = (uint)Marshal.SizeOf(nativeIndexlist);
            int err = ErrApi.Err(NativeMethods.JetGetIndexInfo(
                sesid.Value,
                dbid.Value,
                tablename,
                ignored,
                ref nativeIndexlist,
                nativeIndexlist.cbStruct,
                (uint)JET_IdxInfo.InfoList));
            indexlist.SetFromNativeIndexlist(nativeIndexlist);

            return err;
        }

        public static int JetGetTableIndexInfo(
                JET_SESID sesid,
                JET_TABLEID tableid,
                string ignored,
                out JET_INDEXLIST indexlist)
        {
            ErrApi.TraceFunctionCall("JetGetTableIndexInfo");
            indexlist = new JET_INDEXLIST();

            var nativeIndexlist = new NATIVE_INDEXLIST();
            nativeIndexlist.cbStruct = (uint)Marshal.SizeOf(nativeIndexlist);
            int err = ErrApi.Err(NativeMethods.JetGetTableIndexInfo(
                sesid.Value,
                tableid.Value,
                ignored,
                ref nativeIndexlist,
                nativeIndexlist.cbStruct,
                (uint)JET_IdxInfo.InfoList));
            indexlist.SetFromNativeIndexlist(nativeIndexlist);

            return err;
        }

        #endregion

        #region Navigation

        public static int JetGetBookmark(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize, out int actualBookmarkSize)
        {
            ErrApi.TraceFunctionCall("JetGetBookmark");
            actualBookmarkSize = 0;
            ErrApi.CheckNotNegative(bookmarkSize, "bookmarkSize");
            if ((null == bookmark && 0 != bookmarkSize) || (null != bookmark && bookmarkSize > bookmark.Length))
            {
                throw new ArgumentException(
                    "bookmarkSize cannot be greater than the length of the bookmark",
                    "bookmarkSize");
            }

            uint cbActual = 0;
            int err;
            GCHandle bookmarkHandle = GCHandle.Alloc(bookmark, GCHandleType.Pinned);
            try
            {
                err = ErrApi.Err(NativeMethods.JetGetBookmark(sesid.Value, tableid.Value, bookmarkHandle.AddrOfPinnedObject(), (uint)bookmarkSize, ref cbActual));
            }
            finally
            {
                bookmarkHandle.Free();
            }

            actualBookmarkSize = (int)cbActual;
            return err;
        }

        public static int JetGotoBookmark(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize)
        {
            ErrApi.TraceFunctionCall("JetGotoBookmark");
            ErrApi.CheckNotNull(bookmark, "bookark");
            ErrApi.CheckNotNegative(bookmarkSize, "bookmarkSize");
            if (bookmarkSize > bookmark.Length)
            {
                throw new ArgumentException(
                    "bookmarkSize cannot be greater than the length of the bookmark",
                    "bookmarkSize");
            }

            GCHandle bookmarkHandle = GCHandle.Alloc(bookmark, GCHandleType.Pinned);
            try
            {
                return ErrApi.Err(NativeMethods.JetGotoBookmark(sesid.Value, tableid.Value, bookmarkHandle.AddrOfPinnedObject(), (uint)bookmarkSize));
            }
            finally
            {
                bookmarkHandle.Free();
            }
        }

        public static int JetMakeKey(JET_SESID sesid, JET_TABLEID tableid, byte[] data, int dataSize, MakeKeyGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetMakeKey");
            ErrApi.CheckDataSize(data, dataSize);

            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                return ErrApi.Err(NativeMethods.JetMakeKey(sesid.Value, tableid.Value, dataHandle.AddrOfPinnedObject(), (uint)dataSize, (uint)grbit));
            }
            finally
            {
                dataHandle.Free();
            }
        }

        public static int JetRetrieveKey(JET_SESID sesid, JET_TABLEID tableid, byte[] data, int dataSize, out int actualDataSize, RetrieveKeyGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetRetrieveKey");
            actualDataSize = 0;
            ErrApi.CheckDataSize(data, dataSize);

            uint cbActual = 0;
            int err;
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                err = ErrApi.Err(NativeMethods.JetRetrieveKey(sesid.Value, tableid.Value, dataHandle.AddrOfPinnedObject(), (uint)dataSize, ref cbActual, (uint)grbit));
            }
            finally
            {
                dataHandle.Free();
            }

            actualDataSize = (int)cbActual;
            return err;
        }

        public static int JetSeek(JET_SESID sesid, JET_TABLEID tableid, SeekGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetSeek");
            return ErrApi.Err(NativeMethods.JetSeek(sesid.Value, tableid.Value, (uint)grbit));
        }

        public static int JetMove(JET_SESID sesid, JET_TABLEID tableid, int numRows, MoveGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetMove");
            return ErrApi.Err(NativeMethods.JetMove(sesid.Value, tableid.Value, numRows, (uint)grbit));
        }

        public static int JetSetIndexRange(JET_SESID sesid, JET_TABLEID tableid, SetIndexRangeGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetSetIndexRange");
            return ErrApi.Err(NativeMethods.JetSetIndexRange(sesid.Value, tableid.Value, (uint)grbit));
        }

        /// <summary>
        /// Computes the intersection between multiple sets of index entries from different secondary
        /// indices over the same table. This operation is useful for finding the set of records in a
        /// table that match two or more criteria that can be expressed using index ranges. 
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableids">
        /// An array of tableids to intersect. The tableids must have index ranges set on them.
        /// </param>
        /// <param name="numTableids">
        /// The number of tableids.
        /// </param>
        /// <param name="recordlist">
        /// Returns information about the temporary table containing the intersection results.
        /// </param>
        /// <param name="grbit">Intersection options.</param>
        /// <returns>An error if the call fails.</returns>
        public static int JetIntersectIndexes(
            JET_SESID sesid,
            JET_TABLEID[] tableids,
            int numTableids,
            out JET_RECORDLIST recordlist,
            IntersectIndexesGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetIntersectIndexes");
            ErrApi.CheckNotNull(tableids, "tableids");
            ErrApi.CheckDataSize(tableids, numTableids);
            if (numTableids < 2)
            {
                throw new ArgumentException("JetIntersectIndexes requires at least two tables.", "numTableids");
            }

            var indexRanges = new NATIVE_INDEXRANGE[numTableids];
            for (int i = 0; i < numTableids; ++i)
            {
                indexRanges[i] = NATIVE_INDEXRANGE.MakeIndexRangeFromTableid(tableids[i]);
            }

            var nativeRecordlist = new NATIVE_RECORDLIST();
            nativeRecordlist.cbStruct = (uint)Marshal.SizeOf(nativeRecordlist);

            GCHandle ranges = GCHandle.Alloc(indexRanges, GCHandleType.Pinned);
            try
            {
                int err = ErrApi.Err(
                            NativeMethods.JetIntersectIndexes(
                                sesid.Value,
                                ranges.AddrOfPinnedObject(),
                                (uint) numTableids,
                                ref nativeRecordlist,
                                (uint) grbit));
                recordlist = new JET_RECORDLIST();
                recordlist.SetFromNativeRecordlist(nativeRecordlist);
                return err;
            }
            finally
            {
                ranges.Free();
            }
        }

        public static int JetSetCurrentIndex(JET_SESID sesid, JET_TABLEID tableid, string index)
        {
            ErrApi.TraceFunctionCall("JetSetCurrentIndex");

            // A null index name is valid here -- it will set the table to the primary index
            return ErrApi.Err(NativeMethods.JetSetCurrentIndex(sesid.Value, tableid.Value, index));
        }

        public static int JetIndexRecordCount(JET_SESID sesid, JET_TABLEID tableid, out int numRecords, int maxRecordsToCount)
        {
            ErrApi.TraceFunctionCall("JetIndexRecordCount");
            ErrApi.CheckNotNegative(maxRecordsToCount, "maxRecordsToCount");
            uint crec = 0;
            int err = ErrApi.Err(NativeMethods.JetIndexRecordCount(sesid.Value, tableid.Value, ref crec, (uint)maxRecordsToCount));
            numRecords = (int)crec;
            return err;
        }

        public static int JetSetTableSequential(JET_SESID sesid, JET_TABLEID tableid, SetTableSequentialGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetSetTableSequential");
            return ErrApi.Err(NativeMethods.JetSetTableSequential(sesid.Value, tableid.Value, (uint)grbit));
        }

        public static int JetResetTableSequential(JET_SESID sesid, JET_TABLEID tableid, ResetTableSequentialGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetResetTableSequential");
            return ErrApi.Err(NativeMethods.JetResetTableSequential(sesid.Value, tableid.Value, (uint)grbit));
        }

        public static int JetGetRecordPosition(JET_SESID sesid, JET_TABLEID tableid, out JET_RECPOS recpos)
        {
            ErrApi.TraceFunctionCall("JetGetRecordPosition");
            recpos = new JET_RECPOS();
            NATIVE_RECPOS native = recpos.GetNativeRecpos();
            int err = NativeMethods.JetGetRecordPosition(sesid.Value, tableid.Value, ref native, native.cbStruct);
            recpos.SetFromNativeRecpos(native);
            return err;
        }

        public static int JetGotoPosition(JET_SESID sesid, JET_TABLEID tableid, JET_RECPOS recpos)
        {
            ErrApi.TraceFunctionCall("JetGotoRecordPosition");
            NATIVE_RECPOS native = recpos.GetNativeRecpos();
            return NativeMethods.JetGotoPosition(sesid.Value, tableid.Value, ref native);
        }

        #endregion

        #region DML

        public static int JetRetrieveColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, byte[] data, int dataSize, out int actualDataSize, RetrieveColumnGrbit grbit, JET_RETINFO retinfo)
        {
            ErrApi.TraceFunctionCall("JetRetrieveColumn");
            actualDataSize = 0;
            ErrApi.CheckDataSize(data, dataSize);

            int err;
            uint cbActual = 0;
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                if (null != retinfo)
                {
                    NATIVE_RETINFO nativeRetinfo = retinfo.GetNativeRetinfo();
                    GCHandle retinfoHandle = GCHandle.Alloc(nativeRetinfo, GCHandleType.Pinned);
                    try
                    {
                        err = ErrApi.Err(NativeMethods.JetRetrieveColumn(
                                sesid.Value, 
                                tableid.Value, 
                                columnid.Value,
                                dataHandle.AddrOfPinnedObject(), 
                                (uint) dataSize,
                                ref cbActual, 
                                (uint) grbit,
                                retinfoHandle.AddrOfPinnedObject()));
                    }
                    finally
                    {
                        retinfoHandle.Free();
                    }

                retinfo.SetFromNativeRetinfo(nativeRetinfo);
                }
                else
                {
                    err = ErrApi.Err(NativeMethods.JetRetrieveColumn(
                            sesid.Value, 
                            tableid.Value, 
                            columnid.Value,
                            dataHandle.AddrOfPinnedObject(), 
                            (uint) dataSize,
                            ref cbActual, 
                            (uint) grbit, 
                            IntPtr.Zero));
                }
            }
            finally
            {
                dataHandle.Free();
            }

            actualDataSize = (int)cbActual;
            return err;
        }

        public static int JetDelete(JET_SESID sesid, JET_TABLEID tableid)
        {
            ErrApi.TraceFunctionCall("JetDelete");
            return ErrApi.Err(NativeMethods.JetDelete(sesid.Value, tableid.Value));
        }

        public static int JetPrepareUpdate(JET_SESID sesid, JET_TABLEID tableid, JET_prep prep)
        {
            ErrApi.TraceFunctionCall("JetPrepareUpdate");
            return ErrApi.Err(NativeMethods.JetPrepareUpdate(sesid.Value, tableid.Value, (uint)prep));
        }

        public static int JetUpdate(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize, out int actualBookmarkSize)
        {
            ErrApi.TraceFunctionCall("JetUpdate");
            actualBookmarkSize = 0;
            ErrApi.CheckNotNegative(bookmarkSize, "bookmarkSize");
            if ((null == bookmark && 0 != bookmarkSize) || (null != bookmark && bookmarkSize > bookmark.Length))
            {
                throw new ArgumentException(
                    "bookmarkSize cannot be greater than the length of the bookmark",
                    "bookmarkSize");
            }

            uint cbActual = 0;
            int err;
            GCHandle gch = GCHandle.Alloc(bookmark, GCHandleType.Pinned);
            try
            {
                err = ErrApi.Err(NativeMethods.JetUpdate(sesid.Value, tableid.Value, gch.AddrOfPinnedObject(), (uint)bookmarkSize, ref cbActual));
            }
            finally
            {
                gch.Free();
            }

            actualBookmarkSize = (int)cbActual;
            return err;
        }

        public static int JetSetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, byte[] data, int dataSize, SetColumnGrbit grbit, JET_SETINFO setinfo)
        {
            ErrApi.TraceFunctionCall("JetSetColumn");
            ErrApi.CheckNotNegative(dataSize, "dataSize");
            if (null == data)
            {
                if (dataSize > 0 && (SetColumnGrbit.SizeLV != (grbit & SetColumnGrbit.SizeLV)))
                {
                    throw new ArgumentException(
                        "dataSize cannot be greater than the length of the data (unless the SizeLV option is used)",
                        "dataSize");
                }
            }
            else 
            {
                if (dataSize > data.Length && (SetColumnGrbit.SizeLV != (grbit & SetColumnGrbit.SizeLV)))
                {
                    throw new ArgumentException(
                        "dataSize cannot be greater than the length of the data (unless the SizeLV option is used)",
                        "dataSize");
                }
            }

            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                if (null != setinfo)
                {
                    NATIVE_SETINFO nativeSetinfo = setinfo.GetNativeSetinfo();
                    GCHandle setinfoHandle = GCHandle.Alloc(nativeSetinfo, GCHandleType.Pinned);
                    try
                    {
                        return ErrApi.Err(NativeMethods.JetSetColumn(sesid.Value, tableid.Value, columnid.Value, dataHandle.AddrOfPinnedObject(), (uint)dataSize, (uint)grbit, setinfoHandle.AddrOfPinnedObject()));
                    }
                    finally
                    {
                        setinfoHandle.Free();
                    }
                }
                else
                {
                    return ErrApi.Err(NativeMethods.JetSetColumn(sesid.Value, tableid.Value, columnid.Value, dataHandle.AddrOfPinnedObject(), (uint)dataSize, (uint)grbit, IntPtr.Zero));
                }
            }
            finally
            {
                dataHandle.Free();
            }
        }

        public static int JetGetLock(JET_SESID sesid, JET_TABLEID tableid, GetLockGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetGetLock");
            return ErrApi.Err(NativeMethods.JetGetLock(sesid.Value, tableid.Value, (uint)grbit));
        }

        /// <summary>
        /// Performs an atomic addition operation on one column. This function allows
        /// multiple sessions to update the same record concurrently without conflicts.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update.</param>
        /// <param name="columnid">
        /// The column to update. This must be an escrow updatable column.
        /// </param>
        /// <param name="delta">The buffer containing the addend.</param>
        /// <param name="deltaSize">The size of the addend.</param>
        /// <param name="previousValue">
        /// An output buffer that will recieve the current value of the column. This buffer
        /// can be null.
        /// </param>
        /// <param name="previousValueLength">The size of the previousValue buffer.</param>
        /// <param name="actualPreviousValueLength">Returns the actual size of the previousValue.</param>
        /// <param name="grbit">Escrow update options.</param>
        /// <returns>An error code if the operation fails.</returns>
        public static int JetEscrowUpdate(
            JET_SESID sesid,
            JET_TABLEID tableid,
            JET_COLUMNID columnid,
            byte[] delta,
            int deltaSize,
            byte[] previousValue,
            int previousValueLength,
            out int actualPreviousValueLength,
            EscrowUpdateGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetEscrowUpdate");
            ErrApi.CheckNotNull(delta, "delta");
            ErrApi.CheckDataSize(delta, deltaSize);
            ErrApi.CheckDataSize(previousValue, previousValueLength);

            actualPreviousValueLength = 0;

            GCHandle deltaHandle = GCHandle.Alloc(delta, GCHandleType.Pinned);
            GCHandle valueHandle = GCHandle.Alloc(previousValue, GCHandleType.Pinned);

            try
            {
                uint cbOldActual = 0;
                int err = ErrApi.Err(NativeMethods.JetEscrowUpdate(
                                      sesid.Value,
                                      tableid.Value,
                                      columnid.Value,
                                      deltaHandle.AddrOfPinnedObject(),
                                      (uint)deltaSize,
                                      valueHandle.AddrOfPinnedObject(),
                                      (uint)previousValueLength,
                                      ref cbOldActual,
                                      (uint)grbit));
                actualPreviousValueLength = (int)cbOldActual;
                return err;
            }
            finally
            {
                deltaHandle.Free();
                valueHandle.Free();
            }
        }

        #endregion

        #region Parameter Checking and Tracing

        /// <summary>
        /// Make sure the data and dataSize arguments match.
        /// </summary>
        /// <param name="data">The data buffer.</param>
        /// <param name="dataSize">The size of the data.</param>
        /// <typeparam name="T">The type of the data.</typeparam>
        private static void CheckDataSize<T>(ICollection<T> data, int dataSize)
        {
            ErrApi.CheckNotNegative(dataSize, "dataSize");
            if ((null == data && 0 != dataSize) || (null != data && dataSize > data.Count))
            {
                Trace.WriteLineIf(ErrApi.traceSwitch.TraceError, "CheckDataSize failed");
                throw new ArgumentException(
                    "dataSize cannot be greater than the length of the data",
                    "dataSize");
            }
        }

        /// <summary>
        /// Make sure the given object isn't null. If it is
        /// then throw an ArgumentNullException.
        /// </summary>
        /// <param name="o">The object to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        private static void CheckNotNull(object o, string paramName)
        {
            if (null == o)
            {
                Trace.WriteLineIf(ErrApi.traceSwitch.TraceError, "CheckNotNull failed");
                throw new ArgumentNullException(
                    String.Format("{0} cannot be null", paramName),
                    paramName);
            }
        }

        /// <summary>
        /// Make sure the given integer isn't negative. If it is
        /// then throw an ArgumentException.
        /// </summary>
        /// <param name="i">The integer to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        private static void CheckNotNegative(int i, string paramName)
        {
            if (i < 0)
            {
                Trace.WriteLineIf(ErrApi.traceSwitch.TraceError, "CheckNotNegative failed");
                throw new ArgumentException(
                    String.Format("{0} cannot be less than zero", paramName),
                    paramName);
            }
        }

        /// <summary>
        /// Trace a call to an ESENT function.
        /// </summary>
        /// <param name="function">The name of the function being called.</param>
        [Conditional("TRACE")]
        private static void TraceFunctionCall(string function)
        {
            Trace.WriteLineIf(ErrApi.traceSwitch.TraceInfo, function);
        }

        /// <summary>
        /// Can be used to trap ESENT errors.
        /// </summary>
        /// <param name="err">The error being returned.</param>
        /// <returns>The error.</returns>
        private static int Err(int err)
        {
            if (0 == err)
            {
                Trace.WriteLineIf(ErrApi.traceSwitch.TraceVerbose, "JET_err.Success");
            }
            else if (err > 0)
            {
                Trace.WriteLineIf(ErrApi.traceSwitch.TraceWarning, (JET_wrn)err);
            }
            else
            {
                Trace.WriteLineIf(ErrApi.traceSwitch.TraceError, (JET_err)err);
            }

            return err;
        }

        #endregion Parameter Checking and Tracing
    }
}