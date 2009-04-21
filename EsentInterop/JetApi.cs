//-----------------------------------------------------------------------
// <copyright file="JetApi.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Isam.Esent.Interop.Implementation
{
    /// <summary>
    /// Calls to the ESENT interop layer. These calls take the managed types (e.g. JET_SESID) and
    /// return errors.
    /// </summary>
    internal class JetApi : IJetApi
    {
        private readonly TraceSwitch traceSwitch = new TraceSwitch("ESENT P/Invoke", "P/Invoke calls to ESENT");

        #region init/term

        public int JetCreateInstance(out JET_INSTANCE instance, string name)
        {
            this.TraceFunctionCall("JetCreateInstance");
            instance.Value = IntPtr.Zero;
            return this.Err(NativeMethods.JetCreateInstance(ref instance.Value, name));
        }

        /// <summary>
        /// Allocate a new instance of the database engine for use in a single
        /// process, with a display name specified.
        /// </summary>
        /// <param name="instance">Returns the newly create instance.</param>
        /// <param name="name">
        /// Specifies a unique string identifier for the instance to be created.
        /// This string must be unique within a given process hosting the
        /// database engine.
        /// </param>
        /// <param name="displayName">
        /// A display name for the instance to be created. This will be used
        /// in eventlog entries.
        /// </param>
        /// <param name="grbit">Creation options.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetCreateInstance2(out JET_INSTANCE instance, string name, string displayName, CreateInstanceGrbit grbit)
        {
            this.TraceFunctionCall("JetCreateInstance2");
            instance.Value = IntPtr.Zero;
            return this.Err(NativeMethods.JetCreateInstance2(ref instance.Value, name, displayName, (uint)grbit));
        }

        public int JetInit(ref JET_INSTANCE instance)
        {
            this.TraceFunctionCall("JetInit");
            return this.Err(NativeMethods.JetInit(ref instance.Value));
        }

        public int JetTerm(JET_INSTANCE instance)
        {
            this.TraceFunctionCall("JetTerm");
            if (JET_INSTANCE.Nil != instance)
            {
                return this.Err(NativeMethods.JetTerm(instance.Value));
            }

            return (int)JET_err.Success;
        }

        public int JetTerm2(JET_INSTANCE instance, TermGrbit grbit)
        {
            this.TraceFunctionCall("JetTerm2");
            if (JET_INSTANCE.Nil != instance)
            {
                return this.Err(NativeMethods.JetTerm2(instance.Value, (uint) grbit));
            }

            return (int)JET_err.Success;
        }

        public int JetSetSystemParameter(JET_INSTANCE instance, JET_SESID sesid, JET_param paramid, int paramValue, string paramString)
        {
            this.TraceFunctionCall("JetSetSystemParameter");
            if (IntPtr.Zero == instance.Value)
            {
                return this.Err(NativeMethods.JetSetSystemParameter(IntPtr.Zero, sesid.Value, (uint)paramid, (IntPtr)paramValue, paramString));
            }
            else
            {
                GCHandle instanceHandle = GCHandle.Alloc(instance, GCHandleType.Pinned);
                try
                {
                    return this.Err(NativeMethods.JetSetSystemParameter(instanceHandle.AddrOfPinnedObject(), sesid.Value, (uint)paramid, (IntPtr)paramValue, paramString));
                }
                finally
                {
                    instanceHandle.Free();
                }
            }
        }

        public int JetGetSystemParameter(JET_INSTANCE instance, JET_SESID sesid, JET_param paramid, ref int paramValue, out string paramString, int maxParam)
        {
            this.TraceFunctionCall("JetGetSystemParameter");

            var intValue = (IntPtr)paramValue;
            var sb = new StringBuilder(maxParam);
            int err = this.Err(NativeMethods.JetGetSystemParameter(instance.Value, sesid.Value, (uint)paramid, ref intValue, sb, (uint)maxParam));
            paramString = sb.ToString();
            paramValue = intValue.ToInt32();
            return err;
        }

        /// <summary>
        /// Retrieves the version of the database engine.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="version">Returns the version number of the database engine.</param>
        /// <returns>An error code if the call fails.</returns>
        public int JetGetVersion(JET_SESID sesid, out int version)
        {
            this.TraceFunctionCall("JetGetVersion");
            uint nativeVersion = 0;
            int err = this.Err(NativeMethods.JetGetVersion(sesid.Value, ref nativeVersion));
            version = checked((int) nativeVersion);
            return err;
        }

        #endregion

        #region Databases

        public int JetCreateDatabase(JET_SESID sesid, string database, string connect, out JET_DBID dbid, CreateDatabaseGrbit grbit)
        {
            this.TraceFunctionCall("JetCreateDatabase");
            this.CheckNotNull(database, "database");

            dbid = JET_DBID.Nil;
            return this.Err(NativeMethods.JetCreateDatabase(sesid.Value, database, connect, ref dbid.Value, (uint)grbit));
        }

        public int JetAttachDatabase(JET_SESID sesid, string database, AttachDatabaseGrbit grbit)
        {
            this.TraceFunctionCall("JetAttachDatabase");
            this.CheckNotNull(database, "database");

            return this.Err(NativeMethods.JetAttachDatabase(sesid.Value, database, (uint)grbit));
        }

        public int JetOpenDatabase(JET_SESID sesid, string database, string connect, out JET_DBID dbid, OpenDatabaseGrbit grbit)
        {
            this.TraceFunctionCall("JetOpenDatabase");
            dbid = JET_DBID.Nil;
            this.CheckNotNull(database, "database");

            return this.Err(NativeMethods.JetOpenDatabase(sesid.Value, database, connect, ref dbid.Value, (uint)grbit));
        }

        public int JetCloseDatabase(JET_SESID sesid, JET_DBID dbid, CloseDatabaseGrbit grbit)
        {
            this.TraceFunctionCall("JetCloseDatabase");
            return this.Err(NativeMethods.JetCloseDatabase(sesid.Value, dbid.Value, (uint)grbit));
        }

        public int JetDetachDatabase(JET_SESID sesid, string database)
        {
            this.TraceFunctionCall("JetDetachDatabase");
            this.CheckNotNull(database, "database");

            return this.Err(NativeMethods.JetDetachDatabase(sesid.Value, database));
        }

        #endregion

        #region sessions

        public int JetBeginSession(JET_INSTANCE instance, out JET_SESID sesid, string username, string password)
        {
            this.TraceFunctionCall("JetBeginSession");
            sesid = JET_SESID.Nil;
            return this.Err(NativeMethods.JetBeginSession(instance.Value, ref sesid.Value, null, null));
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
        public int JetSetSessionContext(JET_SESID sesid, IntPtr context)
        {
            this.TraceFunctionCall("JetSetSessionContext");
            return this.Err(NativeMethods.JetSetSessionContext(sesid.Value, context));
        }

        /// <summary>
        /// Disassociates a session from the current thread. This should be
        /// used in conjunction with JetSetSessionContext.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <returns>An error if the call fails.</returns>
        public int JetResetSessionContext(JET_SESID sesid)
        {
            this.TraceFunctionCall("JetResetSessionContext");
            return this.Err(NativeMethods.JetResetSessionContext(sesid.Value));
        }

        public int JetEndSession(JET_SESID sesid, EndSessionGrbit grbit)
        {
            this.TraceFunctionCall("JetEndSession");
            return this.Err(NativeMethods.JetEndSession(sesid.Value, (uint)grbit));
        }

        public int JetDupSession(JET_SESID sesid, out JET_SESID newSesid)
        {
            this.TraceFunctionCall("JetDupSession");
            newSesid = JET_SESID.Nil;
            return this.Err(NativeMethods.JetDupSession(sesid.Value, ref newSesid.Value));
        }

        #endregion

        #region tables

        public int JetOpenTable(JET_SESID sesid, JET_DBID dbid, string tablename, byte[] parameters, int parametersLength, OpenTableGrbit grbit, out JET_TABLEID tableid)
        {
            this.TraceFunctionCall("JetOpenTable");
            tableid = JET_TABLEID.Nil;
            this.CheckNotNull(tablename, "tablename");

            return this.Err(NativeMethods.JetOpenTable(sesid.Value, dbid.Value, tablename, IntPtr.Zero, 0, (uint)grbit, ref tableid.Value));
        }

        public int JetCloseTable(JET_SESID sesid, JET_TABLEID tableid)
        {
            this.TraceFunctionCall("JetCloseTable");
            return this.Err(NativeMethods.JetCloseTable(sesid.Value, tableid.Value));
        }

        public int JetDupCursor(JET_SESID sesid, JET_TABLEID tableid, out JET_TABLEID newTableid, DupCursorGrbit grbit)
        {
            this.TraceFunctionCall("JetDupCursor");
            newTableid = JET_TABLEID.Nil;
            return this.Err(NativeMethods.JetDupCursor(sesid.Value, tableid.Value, ref newTableid.Value, (uint)grbit));
        }

        #endregion

        #region transactions

        public int JetBeginTransaction(JET_SESID sesid)
        {
            this.TraceFunctionCall("JetBeginTransaction");
            return this.Err(NativeMethods.JetBeginTransaction(sesid.Value));
        }

        public int JetCommitTransaction(JET_SESID sesid, CommitTransactionGrbit grbit)
        {
            this.TraceFunctionCall("JetCommitTransaction");
            return this.Err(NativeMethods.JetCommitTransaction(sesid.Value, (uint)grbit));
        }

        public int JetRollback(JET_SESID sesid, RollbackTransactionGrbit grbit)
        {
            this.TraceFunctionCall("JetRollback");
            return this.Err(NativeMethods.JetRollback(sesid.Value, (uint)grbit));
        }

        #endregion

        #region DDL

        public int JetCreateTable(JET_SESID sesid, JET_DBID dbid, string table, int pages, int density, out JET_TABLEID tableid)
        {
            this.TraceFunctionCall("JetCreateTable");
            tableid = JET_TABLEID.Nil;
            this.CheckNotNull(table, "table");

            return this.Err(NativeMethods.JetCreateTable(sesid.Value, dbid.Value, table, pages, density, ref tableid.Value));
        }

        public int JetAddColumn(JET_SESID sesid, JET_TABLEID tableid, string column, JET_COLUMNDEF columndef, byte[] defaultValue, int defaultValueSize, out JET_COLUMNID columnid)
        {
            this.TraceFunctionCall("JetAddColumn");
            columnid = JET_COLUMNID.Nil;
            this.CheckNotNull(column, "column");
            this.CheckNotNull(columndef, "columndef");
            this.CheckNotNegative(defaultValueSize, "defaultValueSize");
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
                err = this.Err(NativeMethods.JetAddColumn(
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

        public int JetDeleteColumn(JET_SESID sesid, JET_TABLEID tableid, string column)
        {
            this.TraceFunctionCall("JetDeleteColumn");
            this.CheckNotNull(column, "column");

            return this.Err(NativeMethods.JetDeleteColumn(sesid.Value, tableid.Value, column));
        }

        public int JetDeleteIndex(JET_SESID sesid, JET_TABLEID tableid, string index)
        {
            this.TraceFunctionCall("JetDeleteIndex");
            this.CheckNotNull(index, "index");

            return this.Err(NativeMethods.JetDeleteIndex(sesid.Value, tableid.Value, index));
        }

        public int JetDeleteTable(JET_SESID sesid, JET_DBID dbid, string table)
        {
            this.TraceFunctionCall("JetDeleteTable");
            this.CheckNotNull(table, "table");

            return this.Err(NativeMethods.JetDeleteTable(sesid.Value, dbid.Value, table));
        }

        public int JetCreateIndex(
            JET_SESID sesid,
            JET_TABLEID tableid,
            string indexName,
            CreateIndexGrbit grbit, 
            string keyDescription,
            int keyDescriptionLength,
            int density)
        {
            this.TraceFunctionCall("JetCreateIndex");
            this.CheckNotNull(indexName, "indexName");
            this.CheckNotNegative(keyDescriptionLength, "keyDescriptionLength");
            this.CheckNotNegative(density, "density");
            if (keyDescriptionLength > keyDescription.Length + 1)
            {
                throw new ArgumentException("keyDescriptionLength is greater than keyDescription", "keyDescriptionLength");
            }

            return this.Err(NativeMethods.JetCreateIndex(
                sesid.Value,
                tableid.Value,
                indexName,
                (uint)grbit,
                keyDescription,
                (uint)keyDescriptionLength,
                (uint)density));
        }

        public int JetGetTableColumnInfo(
                JET_SESID sesid,
                JET_TABLEID tableid,
                string columnName,
                out JET_COLUMNDEF columndef)
        {
            this.TraceFunctionCall("JetGetTableColumnInfo");
            columndef = new JET_COLUMNDEF();
            this.CheckNotNull(columnName, "columnName");

            var nativeColumndef = new NATIVE_COLUMNDEF();
            nativeColumndef.cbStruct = (uint)Marshal.SizeOf(nativeColumndef);
            int err = this.Err(NativeMethods.JetGetTableColumnInfo(
                sesid.Value,
                tableid.Value,
                columnName,
                ref nativeColumndef,
                nativeColumndef.cbStruct,
                (uint)JET_ColInfo.Default));
            columndef.SetFromNativeColumndef(nativeColumndef);

            return err;
        }

        public int JetGetTableColumnInfo(
                JET_SESID sesid,
                JET_TABLEID tableid,
                JET_COLUMNID columnid,
                out JET_COLUMNDEF columndef)
        {
            this.TraceFunctionCall("JetGetTableColumnInfo");
            columndef = new JET_COLUMNDEF();

            var nativeColumndef = new NATIVE_COLUMNDEF();
            nativeColumndef.cbStruct = (uint)Marshal.SizeOf(nativeColumndef);
            int err = this.Err(NativeMethods.JetGetTableColumnInfo(
                sesid.Value,
                tableid.Value,
                ref columnid.Value,
                ref nativeColumndef,
                nativeColumndef.cbStruct,
                (uint)JET_ColInfo.ByColid));
            columndef.SetFromNativeColumndef(nativeColumndef);

            return err;
        }

        public int JetGetTableColumnInfo(
                JET_SESID sesid,
                JET_TABLEID tableid,
                string ignored,
                out JET_COLUMNLIST columnlist)
        {
            this.TraceFunctionCall("JetGetTableColumnInfo");
            columnlist = new JET_COLUMNLIST();

            var nativeColumnlist = new NATIVE_COLUMNLIST();
            nativeColumnlist.cbStruct = (uint)Marshal.SizeOf(nativeColumnlist);
            int err = this.Err(NativeMethods.JetGetTableColumnInfo(
                sesid.Value,
                tableid.Value,
                ignored,
                ref nativeColumnlist,
                nativeColumnlist.cbStruct,
                (uint)JET_ColInfo.List));
            columnlist.SetFromNativeColumnlist(nativeColumnlist);

            return err;
        }

        public int JetGetColumnInfo(
                JET_SESID sesid,
                JET_DBID dbid,
                string tablename,
                string columnName,
                out JET_COLUMNDEF columndef)
        {
            this.TraceFunctionCall("JetGetColumnInfo");
            columndef = new JET_COLUMNDEF();
            this.CheckNotNull(tablename, "tablename");
            this.CheckNotNull(columnName, "columnName");

            var nativeColumndef = new NATIVE_COLUMNDEF();
            nativeColumndef.cbStruct = (uint)Marshal.SizeOf(nativeColumndef);
            int err = this.Err(NativeMethods.JetGetColumnInfo(
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

        public int JetGetColumnInfo(
                JET_SESID sesid,
                JET_DBID dbid,
                string tablename,
                string ignored,
                out JET_COLUMNLIST columnlist)
        {
            this.TraceFunctionCall("JetGetColumnInfo");      
            columnlist = new JET_COLUMNLIST();
            this.CheckNotNull(tablename, "tablename");

            var nativeColumnlist = new NATIVE_COLUMNLIST();
            nativeColumnlist.cbStruct = (uint)Marshal.SizeOf(nativeColumnlist);
            int err = this.Err(NativeMethods.JetGetColumnInfo(
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

        public int JetGetObjectInfo(JET_SESID sesid, JET_DBID dbid, out JET_OBJECTLIST objectlist)
        {
            this.TraceFunctionCall("JetGetObjectInfo");
            objectlist = new JET_OBJECTLIST();

            var nativeObjectlist = new NATIVE_OBJECTLIST();
            nativeObjectlist.cbStruct = (uint)Marshal.SizeOf(nativeObjectlist);
            int err = this.Err(NativeMethods.JetGetObjectInfo(
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
        public int JetGetCurrentIndex(JET_SESID sesid, JET_TABLEID tableid, out string indexName, int maxNameLength)
        {
            this.TraceFunctionCall("JetGetCurrentIndex");
            this.CheckNotNegative(maxNameLength, "maxNameLength");

            var name = new StringBuilder(maxNameLength);
            int err = this.Err(NativeMethods.JetGetCurrentIndex(sesid.Value, tableid.Value, name, (uint)maxNameLength));
            indexName = name.ToString();
            return err;
        }

        public int JetGetIndexInfo(
                JET_SESID sesid,
                JET_DBID dbid,
                string tablename,
                string ignored,
                out JET_INDEXLIST indexlist)
        {
            this.TraceFunctionCall("JetGetIndexInfo");
            indexlist = new JET_INDEXLIST();
            this.CheckNotNull(tablename, "tablename");

            var nativeIndexlist = new NATIVE_INDEXLIST();
            nativeIndexlist.cbStruct = (uint)Marshal.SizeOf(nativeIndexlist);
            int err = this.Err(NativeMethods.JetGetIndexInfo(
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

        public int JetGetTableIndexInfo(
                JET_SESID sesid,
                JET_TABLEID tableid,
                string ignored,
                out JET_INDEXLIST indexlist)
        {
            this.TraceFunctionCall("JetGetTableIndexInfo");
            indexlist = new JET_INDEXLIST();

            var nativeIndexlist = new NATIVE_INDEXLIST();
            nativeIndexlist.cbStruct = (uint)Marshal.SizeOf(nativeIndexlist);
            int err = this.Err(NativeMethods.JetGetTableIndexInfo(
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

        public int JetGetBookmark(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize, out int actualBookmarkSize)
        {
            this.TraceFunctionCall("JetGetBookmark");
            actualBookmarkSize = 0;
            this.CheckNotNegative(bookmarkSize, "bookmarkSize");
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
                err = this.Err(NativeMethods.JetGetBookmark(sesid.Value, tableid.Value, bookmarkHandle.AddrOfPinnedObject(), (uint)bookmarkSize, ref cbActual));
            }
            finally
            {
                bookmarkHandle.Free();
            }

            actualBookmarkSize = (int)cbActual;
            return err;
        }

        public int JetGotoBookmark(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize)
        {
            this.TraceFunctionCall("JetGotoBookmark");
            this.CheckNotNull(bookmark, "bookark");
            this.CheckNotNegative(bookmarkSize, "bookmarkSize");
            if (bookmarkSize > bookmark.Length)
            {
                throw new ArgumentException(
                    "bookmarkSize cannot be greater than the length of the bookmark",
                    "bookmarkSize");
            }

            GCHandle bookmarkHandle = GCHandle.Alloc(bookmark, GCHandleType.Pinned);
            try
            {
                return this.Err(NativeMethods.JetGotoBookmark(sesid.Value, tableid.Value, bookmarkHandle.AddrOfPinnedObject(), (uint)bookmarkSize));
            }
            finally
            {
                bookmarkHandle.Free();
            }
        }

        public int JetMakeKey(JET_SESID sesid, JET_TABLEID tableid, byte[] data, int dataSize, MakeKeyGrbit grbit)
        {
            this.TraceFunctionCall("JetMakeKey");
            this.CheckDataSize(data, dataSize);

            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                return this.Err(NativeMethods.JetMakeKey(sesid.Value, tableid.Value, dataHandle.AddrOfPinnedObject(), (uint)dataSize, (uint)grbit));
            }
            finally
            {
                dataHandle.Free();
            }
        }

        public int JetRetrieveKey(JET_SESID sesid, JET_TABLEID tableid, byte[] data, int dataSize, out int actualDataSize, RetrieveKeyGrbit grbit)
        {
            this.TraceFunctionCall("JetRetrieveKey");
            actualDataSize = 0;
            this.CheckDataSize(data, dataSize);

            uint cbActual = 0;
            int err;
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                err = this.Err(NativeMethods.JetRetrieveKey(sesid.Value, tableid.Value, dataHandle.AddrOfPinnedObject(), (uint)dataSize, ref cbActual, (uint)grbit));
            }
            finally
            {
                dataHandle.Free();
            }

            actualDataSize = (int)cbActual;
            return err;
        }

        public int JetSeek(JET_SESID sesid, JET_TABLEID tableid, SeekGrbit grbit)
        {
            this.TraceFunctionCall("JetSeek");
            return this.Err(NativeMethods.JetSeek(sesid.Value, tableid.Value, (uint)grbit));
        }

        public int JetMove(JET_SESID sesid, JET_TABLEID tableid, int numRows, MoveGrbit grbit)
        {
            this.TraceFunctionCall("JetMove");
            return this.Err(NativeMethods.JetMove(sesid.Value, tableid.Value, numRows, (uint)grbit));
        }

        public int JetSetIndexRange(JET_SESID sesid, JET_TABLEID tableid, SetIndexRangeGrbit grbit)
        {
            this.TraceFunctionCall("JetSetIndexRange");
            return this.Err(NativeMethods.JetSetIndexRange(sesid.Value, tableid.Value, (uint)grbit));
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
        public int JetIntersectIndexes(
            JET_SESID sesid,
            JET_TABLEID[] tableids,
            int numTableids,
            out JET_RECORDLIST recordlist,
            IntersectIndexesGrbit grbit)
        {
            this.TraceFunctionCall("JetIntersectIndexes");
            this.CheckNotNull(tableids, "tableids");
            this.CheckDataSize(tableids, numTableids);
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
                int err = this.Err(
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

        public int JetSetCurrentIndex(JET_SESID sesid, JET_TABLEID tableid, string index)
        {
            this.TraceFunctionCall("JetSetCurrentIndex");

            // A null index name is valid here -- it will set the table to the primary index
            return this.Err(NativeMethods.JetSetCurrentIndex(sesid.Value, tableid.Value, index));
        }

        public int JetIndexRecordCount(JET_SESID sesid, JET_TABLEID tableid, out int numRecords, int maxRecordsToCount)
        {
            this.TraceFunctionCall("JetIndexRecordCount");
            this.CheckNotNegative(maxRecordsToCount, "maxRecordsToCount");
            uint crec = 0;
            int err = this.Err(NativeMethods.JetIndexRecordCount(sesid.Value, tableid.Value, ref crec, (uint)maxRecordsToCount));
            numRecords = (int)crec;
            return err;
        }

        public int JetSetTableSequential(JET_SESID sesid, JET_TABLEID tableid, SetTableSequentialGrbit grbit)
        {
            this.TraceFunctionCall("JetSetTableSequential");
            return this.Err(NativeMethods.JetSetTableSequential(sesid.Value, tableid.Value, (uint)grbit));
        }

        public int JetResetTableSequential(JET_SESID sesid, JET_TABLEID tableid, ResetTableSequentialGrbit grbit)
        {
            this.TraceFunctionCall("JetResetTableSequential");
            return this.Err(NativeMethods.JetResetTableSequential(sesid.Value, tableid.Value, (uint)grbit));
        }

        public int JetGetRecordPosition(JET_SESID sesid, JET_TABLEID tableid, out JET_RECPOS recpos)
        {
            this.TraceFunctionCall("JetGetRecordPosition");
            recpos = new JET_RECPOS();
            NATIVE_RECPOS native = recpos.GetNativeRecpos();
            int err = NativeMethods.JetGetRecordPosition(sesid.Value, tableid.Value, ref native, native.cbStruct);
            recpos.SetFromNativeRecpos(native);
            return err;
        }

        public int JetGotoPosition(JET_SESID sesid, JET_TABLEID tableid, JET_RECPOS recpos)
        {
            this.TraceFunctionCall("JetGotoRecordPosition");
            NATIVE_RECPOS native = recpos.GetNativeRecpos();
            return NativeMethods.JetGotoPosition(sesid.Value, tableid.Value, ref native);
        }

        #endregion

        #region DML

        public int JetRetrieveColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, byte[] data, int dataSize, out int actualDataSize, RetrieveColumnGrbit grbit, JET_RETINFO retinfo)
        {
            this.TraceFunctionCall("JetRetrieveColumn");
            actualDataSize = 0;
            this.CheckDataSize(data, dataSize);

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
                        err = this.Err(NativeMethods.JetRetrieveColumn(
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
                    err = this.Err(NativeMethods.JetRetrieveColumn(
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

        public int JetDelete(JET_SESID sesid, JET_TABLEID tableid)
        {
            this.TraceFunctionCall("JetDelete");
            return this.Err(NativeMethods.JetDelete(sesid.Value, tableid.Value));
        }

        public int JetPrepareUpdate(JET_SESID sesid, JET_TABLEID tableid, JET_prep prep)
        {
            this.TraceFunctionCall("JetPrepareUpdate");
            return this.Err(NativeMethods.JetPrepareUpdate(sesid.Value, tableid.Value, (uint)prep));
        }

        public int JetUpdate(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize, out int actualBookmarkSize)
        {
            this.TraceFunctionCall("JetUpdate");
            actualBookmarkSize = 0;
            this.CheckNotNegative(bookmarkSize, "bookmarkSize");
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
                err = this.Err(NativeMethods.JetUpdate(sesid.Value, tableid.Value, gch.AddrOfPinnedObject(), (uint)bookmarkSize, ref cbActual));
            }
            finally
            {
                gch.Free();
            }

            actualBookmarkSize = (int)cbActual;
            return err;
        }

        public int JetSetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, byte[] data, int dataSize, SetColumnGrbit grbit, JET_SETINFO setinfo)
        {
            this.TraceFunctionCall("JetSetColumn");
            this.CheckNotNegative(dataSize, "dataSize");
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
                        return this.Err(NativeMethods.JetSetColumn(sesid.Value, tableid.Value, columnid.Value, dataHandle.AddrOfPinnedObject(), (uint)dataSize, (uint)grbit, setinfoHandle.AddrOfPinnedObject()));
                    }
                    finally
                    {
                        setinfoHandle.Free();
                    }
                }
                else
                {
                    return this.Err(NativeMethods.JetSetColumn(sesid.Value, tableid.Value, columnid.Value, dataHandle.AddrOfPinnedObject(), (uint)dataSize, (uint)grbit, IntPtr.Zero));
                }
            }
            finally
            {
                dataHandle.Free();
            }
        }

        public int JetGetLock(JET_SESID sesid, JET_TABLEID tableid, GetLockGrbit grbit)
        {
            this.TraceFunctionCall("JetGetLock");
            return this.Err(NativeMethods.JetGetLock(sesid.Value, tableid.Value, (uint)grbit));
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
        public int JetEscrowUpdate(
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
            this.TraceFunctionCall("JetEscrowUpdate");
            this.CheckNotNull(delta, "delta");
            this.CheckDataSize(delta, deltaSize);
            this.CheckDataSize(previousValue, previousValueLength);

            actualPreviousValueLength = 0;

            GCHandle deltaHandle = GCHandle.Alloc(delta, GCHandleType.Pinned);
            GCHandle valueHandle = GCHandle.Alloc(previousValue, GCHandleType.Pinned);

            try
            {
                uint cbOldActual = 0;
                int err = this.Err(NativeMethods.JetEscrowUpdate(
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
        private void CheckDataSize<T>(ICollection<T> data, int dataSize)
        {
            this.CheckNotNegative(dataSize, "dataSize");
            if ((null == data && 0 != dataSize) || (null != data && dataSize > data.Count))
            {
                Trace.WriteLineIf(this.traceSwitch.TraceError, "CheckDataSize failed");
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
        private void CheckNotNull(object o, string paramName)
        {
            if (null == o)
            {
                Trace.WriteLineIf(this.traceSwitch.TraceError, "CheckNotNull failed");
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
        private void CheckNotNegative(int i, string paramName)
        {
            if (i < 0)
            {
                Trace.WriteLineIf(this.traceSwitch.TraceError, "CheckNotNegative failed");
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
        private void TraceFunctionCall(string function)
        {
            Trace.WriteLineIf(this.traceSwitch.TraceInfo, function);
        }

        /// <summary>
        /// Can be used to trap ESENT errors.
        /// </summary>
        /// <param name="err">The error being returned.</param>
        /// <returns>The error.</returns>
        private int Err(int err)
        {
            if (0 == err)
            {
                Trace.WriteLineIf(this.traceSwitch.TraceVerbose, "JET_err.Success");
            }
            else if (err > 0)
            {
                Trace.WriteLineIf(this.traceSwitch.TraceWarning, (JET_wrn)err);
            }
            else
            {
                Trace.WriteLineIf(this.traceSwitch.TraceError, (JET_err)err);
            }

            return err;
        }

        #endregion Parameter Checking and Tracing
    }
}