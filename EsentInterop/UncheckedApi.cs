//-----------------------------------------------------------------------
// <copyright file="UncheckedApi.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// Calls to the ESENT interop layer. These calls take the managed types (e.g. JET_SESID) and
    /// return errors.
    /// </summary>
    internal static class ErrApi
    {
        // This trace switch can be enabled by adding this to the configuration file:
        //  <system.diagnostics>
        //      <switches>
        //          <add name="ESENT P/Invoke" value="4" />
        //      </switches>
        //  </system.diagnostics>
        private static TraceSwitch traceSwitch = new TraceSwitch("ESENT P/Invoke", "P/Invoke calls to ESENT");

        #region init/term

        public static int JetCreateInstance(out JET_INSTANCE instance, string name)
        {
            ErrApi.TraceFunctionCall("JetCreateInstance");
            instance.Value = IntPtr.Zero;
            return Err(NativeMethods.JetCreateInstance(ref instance.Value, name));
        }

        public static int JetInit(ref JET_INSTANCE instance)
        {
            ErrApi.TraceFunctionCall("JetInit");
            return Err(NativeMethods.JetInit(ref instance.Value));
        }

        public static int JetTerm(JET_INSTANCE instance)
        {
            ErrApi.TraceFunctionCall("JetTerm");
            return Err(NativeMethods.JetTerm(instance.Value));
        }

        public static int JetSetSystemParameter(JET_INSTANCE instance, JET_SESID sesid, JET_param paramid, int paramValue, string paramString)
        {
            ErrApi.TraceFunctionCall("JetSetSystemParameter");
            if (IntPtr.Zero == instance.Value)
            {
                return Err(NativeMethods.JetSetSystemParameter(IntPtr.Zero, sesid.Value, (uint)paramid, (IntPtr)paramValue, paramString));
            }
            else
            {
                GCHandle instanceHandle = GCHandle.Alloc(instance, GCHandleType.Pinned);
                return Err(NativeMethods.JetSetSystemParameter(instanceHandle.AddrOfPinnedObject(), sesid.Value, (uint)paramid, (IntPtr)paramValue, paramString));
            }
        }

        public static int JetGetSystemParameter(JET_INSTANCE instance, JET_SESID sesid, JET_param paramid, ref int paramValue, out string paramString, int maxParam)
        {
            ErrApi.TraceFunctionCall("JetGetSystemParameter");

            IntPtr intValue = (IntPtr)paramValue;
            StringBuilder sb = new StringBuilder(maxParam);
            int err = Err(NativeMethods.JetGetSystemParameter(instance.Value, sesid.Value, (uint)paramid, ref intValue, sb, (uint)maxParam));
            paramString = sb.ToString();
            paramValue = intValue.ToInt32();
            return err;
        }

        #endregion

        #region Databases

        public static int JetCreateDatabase(JET_SESID sesid, string database, string connect, out JET_DBID dbid, CreateDatabaseGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetCreateDatabase");
            dbid = JET_DBID.Nil;
            return Err(NativeMethods.JetCreateDatabase(sesid.Value, database, connect, ref dbid.Value, (uint)grbit));
        }

        public static int JetAttachDatabase(JET_SESID sesid, string database, AttachDatabaseGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetAttachDatabase");
            return Err(NativeMethods.JetAttachDatabase(sesid.Value, database, (uint)grbit));
        }

        public static int JetOpenDatabase(JET_SESID sesid, string database, string connect, out JET_DBID dbid, OpenDatabaseGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetOpenDatabase");
            dbid = JET_DBID.Nil;
            return Err(NativeMethods.JetOpenDatabase(sesid.Value, database, connect, ref dbid.Value, (uint)grbit));
        }

        public static int JetCloseDatabase(JET_SESID sesid, JET_DBID dbid, CloseDatabaseGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetCloseDatabase");
            return Err(NativeMethods.JetCloseDatabase(sesid.Value, dbid.Value, (uint)grbit));
        }

        public static int JetDetachDatabase(JET_SESID sesid, string database)
        {
            ErrApi.TraceFunctionCall("JetDetachDatabase");
            return Err(NativeMethods.JetDetachDatabase(sesid.Value, database));
        }

        #endregion

        #region sessions

        public static int JetBeginSession(JET_INSTANCE instance, out JET_SESID sesid, string username, string password)
        {
            ErrApi.TraceFunctionCall("JetBeginSession");
            sesid = JET_SESID.Nil;
            return Err(NativeMethods.JetBeginSession(instance.Value, ref sesid.Value, username, password));
        }

        public static int JetEndSession(JET_SESID sesid, EndSessionGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetEndSession");
            return Err(NativeMethods.JetEndSession(sesid.Value, (uint)grbit));
        }

        public static int JetDupSession(JET_SESID sesid, out JET_SESID newSesid)
        {
            ErrApi.TraceFunctionCall("JetDupSession");
            newSesid = JET_SESID.Nil;
            return Err(NativeMethods.JetDupSession(sesid.Value, ref newSesid.Value));
        }

        #endregion

        #region tables

        public static int JetOpenTable(JET_SESID sesid, JET_DBID dbid, string tablename, byte[] parameters, int parametersLength, OpenTableGrbit grbit, out JET_TABLEID tableid)
        {
            ErrApi.TraceFunctionCall("JetOpenTable");
            tableid = JET_TABLEID.Nil;
            return Err(NativeMethods.JetOpenTable(sesid.Value, dbid.Value, tablename, IntPtr.Zero, 0, (uint)grbit, ref tableid.Value));
        }

        public static int JetCloseTable(JET_SESID sesid, JET_TABLEID tableid)
        {
            ErrApi.TraceFunctionCall("JetCloseTable");
            return Err(NativeMethods.JetCloseTable(sesid.Value, tableid.Value));
        }

        public static int JetDupCursor(JET_SESID sesid, JET_TABLEID tableid, out JET_TABLEID newTableid, DupCursorGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetDupCursor");
            newTableid = JET_TABLEID.Nil;
            return Err(NativeMethods.JetDupCursor(sesid.Value, tableid.Value, ref newTableid.Value, (uint)grbit));
        }

        #endregion

        #region transactions

        public static int JetBeginTransaction(JET_SESID sesid)
        {
            ErrApi.TraceFunctionCall("JetBeginTransaction");
            return Err(NativeMethods.JetBeginTransaction(sesid.Value));
        }

        public static int JetCommitTransaction(JET_SESID sesid, CommitTransactionGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetCommitTransaction");
            return Err(NativeMethods.JetCommitTransaction(sesid.Value, (uint)grbit));
        }

        public static int JetRollback(JET_SESID sesid, RollbackTransactionGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetRollback");
            return Err(NativeMethods.JetRollback(sesid.Value, (uint)grbit));
        }

        #endregion

        #region DDL

        public static int JetCreateTable(JET_SESID sesid, JET_DBID dbid, string table, int pages, int density, out JET_TABLEID tableid)
        {
            ErrApi.TraceFunctionCall("JetCreateTable");
            tableid = JET_TABLEID.Nil;
            return Err(NativeMethods.JetCreateTable(sesid.Value, dbid.Value, table, pages, density, ref tableid.Value));
        }

        public static int JetAddColumn(JET_SESID sesid, JET_TABLEID tableid, string column, JET_COLUMNDEF columndef, byte[] defaultValue, int defaultValueSize, out JET_COLUMNID columnid)
        {
            ErrApi.TraceFunctionCall("JetAddColumn");
            columnid = JET_COLUMNID.Nil;

            if ((null != defaultValue) && defaultValueSize > defaultValue.Length)
            {
                throw new ArgumentException(
                    "defaultValueSize cannot be greater than the length of the defaultValue array",
                    "defaultValueSize");
            }

            var nativeColumndef = columndef.GetNativeColumndef();
            var gch = GCHandle.Alloc(defaultValue, GCHandleType.Pinned);
            return Err(NativeMethods.JetAddColumn(sesid.Value, tableid.Value, column, ref nativeColumndef, gch.AddrOfPinnedObject(), (uint)defaultValueSize, ref columnid.Value));
        }

        public static int JetDeleteColumn(JET_SESID sesid, JET_TABLEID tableid, string column)
        {
            ErrApi.TraceFunctionCall("JetDeleteColumn");
            return Err(NativeMethods.JetDeleteColumn(sesid.Value, tableid.Value, column));
        }

        public static int JetDeleteIndex(JET_SESID sesid, JET_TABLEID tableid, string index)
        {
            ErrApi.TraceFunctionCall("JetDeleteIndex");
            return Err(NativeMethods.JetDeleteIndex(sesid.Value, tableid.Value, index));
        }

        public static int JetDeleteTable(JET_SESID sesid, JET_DBID dbid, string table)
        {
            ErrApi.TraceFunctionCall("JetDeleteTable");
            return Err(NativeMethods.JetDeleteTable(sesid.Value, dbid.Value, table));
        }

        #endregion

        #region Navigation

        public static int JetGetBookmark(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize, out int actualBookmarkSize)
        {
            ErrApi.TraceFunctionCall("JetGetBookmark");
            actualBookmarkSize = 0;

            CheckDataSize(bookmark, bookmarkSize);

            uint cbActual = 0;
            var bookmarkHandle = GCHandle.Alloc(bookmark, GCHandleType.Pinned);
            int err = Err(NativeMethods.JetGetBookmark(sesid.Value, tableid.Value, bookmarkHandle.AddrOfPinnedObject(), (uint)bookmarkSize, ref cbActual));
            actualBookmarkSize = (int)cbActual;
            return err;
        }

        public static int JetGotoBookmark(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize)
        {
            ErrApi.TraceFunctionCall("JetGotoBookmark");
            if (null == bookmark)
            {
                throw new ArgumentNullException("bookmark");
            }

            if (bookmarkSize > bookmark.Length)
            {
                throw new ArgumentException(
                    "bookmarkSize cannot be greater than the length of the bookmark",
                    "bookmarkSize");
            }

            var bookmarkHandle = GCHandle.Alloc(bookmark, GCHandleType.Pinned);
            return Err(NativeMethods.JetGotoBookmark(sesid.Value, tableid.Value, bookmarkHandle.AddrOfPinnedObject(), (uint)bookmarkSize));
        }

        public static int JetRetrieveColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, byte[] data, int dataSize, out int actualDataSize, RetrieveColumnGrbit grbit, JET_RETINFO retinfo)
        {
            ErrApi.TraceFunctionCall("JetRetrieveColumn");
            actualDataSize = 0;

            CheckDataSize(data, dataSize);

            int err;
            uint cbActual = 0;
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            if (null != retinfo)
            {
                var nativeRetinfo = retinfo.GetNativeRetinfo();
                var retinfoHandle = GCHandle.Alloc(nativeRetinfo, GCHandleType.Pinned);
                err = Err(NativeMethods.JetRetrieveColumn(sesid.Value, tableid.Value, columnid.Value, dataHandle.AddrOfPinnedObject(), (uint)dataSize, ref cbActual, (uint)grbit, retinfoHandle.AddrOfPinnedObject()));
                retinfo.SetFromNativeRetinfo(nativeRetinfo);
            }
            else
            {
                err = Err(NativeMethods.JetRetrieveColumn(sesid.Value, tableid.Value, columnid.Value, dataHandle.AddrOfPinnedObject(), (uint)dataSize, ref cbActual, (uint)grbit, IntPtr.Zero));
            }

            actualDataSize = (int)cbActual;
            return err;
        }

        public static int JetMove(JET_SESID sesid, JET_TABLEID tableid, int numRows, MoveGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetMove");
            return Err(NativeMethods.JetMove(sesid.Value, tableid.Value, numRows, (uint)grbit));
        }

        public static int JetMakeKey(JET_SESID sesid, JET_TABLEID tableid, byte[] data, int dataSize, MakeKeyGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetMakeKey");
            CheckDataSize(data, dataSize);

            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            return Err(NativeMethods.JetMakeKey(sesid.Value, tableid.Value, dataHandle.AddrOfPinnedObject(), (uint)dataSize, (uint)grbit));
        }

        public static int JetRetrieveKey(JET_SESID sesid, JET_TABLEID tableid, byte[] data, int dataSize, out int actualDataSize, RetrieveKeyGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetRetrieveKey");
            actualDataSize = 0;

            CheckDataSize(data, dataSize);

            uint cbActual = 0;
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            int err = Err(NativeMethods.JetRetrieveKey(sesid.Value, tableid.Value, dataHandle.AddrOfPinnedObject(), (uint)dataSize, ref cbActual, (uint)grbit));
            actualDataSize = (int)cbActual;
            return err;
        }

        public static int JetSeek(JET_SESID sesid, JET_TABLEID tableid, SeekGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetSeek");
            return Err(NativeMethods.JetSeek(sesid.Value, tableid.Value, (uint)grbit));
        }

        public static int JetSetIndexRange(JET_SESID sesid, JET_TABLEID tableid, SetIndexRangeGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetSetIndexRange");
            return Err(NativeMethods.JetSetIndexRange(sesid.Value, tableid.Value, (uint)grbit));
        }

        public static int JetSetCurrentIndex(JET_SESID sesid, JET_TABLEID tableid, string index)
        {
            ErrApi.TraceFunctionCall("JetSetCurrentIndex");
            return Err(NativeMethods.JetSetCurrentIndex(sesid.Value, tableid.Value, index));
        }

        public static int JetIndexRecordCount(JET_SESID sesid, JET_TABLEID tableid, out int numRecords, int maxRecordsToCount)
        {
            ErrApi.TraceFunctionCall("JetIndexRecordCount");
            numRecords = 0;
            uint crec = 0;
            int err = Err(NativeMethods.JetIndexRecordCount(sesid.Value, tableid.Value, ref crec, (uint)maxRecordsToCount));
            numRecords = (int)crec;
            return err;
        }

        public static int JetSetTableSequential(JET_SESID sesid, JET_TABLEID tableid, SetTableSequentialGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetSetTableSequential");
            return Err(NativeMethods.JetSetTableSequential(sesid.Value, tableid.Value, (uint)grbit));
        }

        public static int JetResetTableSequential(JET_SESID sesid, JET_TABLEID tableid, ResetTableSequentialGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetResetTableSequential");
            return Err(NativeMethods.JetResetTableSequential(sesid.Value, tableid.Value, (uint)grbit));
        }

        public static int JetGetRecordPosition(JET_SESID sesid, JET_TABLEID tableid, out JET_RECPOS recpos)
        {
            ErrApi.TraceFunctionCall("JetGetRecordPosition");
            recpos = new JET_RECPOS();
            var native = recpos.GetNativeRecpos();
            int err = NativeMethods.JetGetRecordPosition(sesid.Value, tableid.Value, ref native, native.cbStruct);
            recpos.SetFromNativeRecpos(native);
            return err;
        }

        public static int JetGotoPosition(JET_SESID sesid, JET_TABLEID tableid, JET_RECPOS recpos)
        {
            ErrApi.TraceFunctionCall("JetGotoRecordPosition");
            var native = recpos.GetNativeRecpos();
            return NativeMethods.JetGotoPosition(sesid.Value, tableid.Value, ref native);
        }

        #endregion

        #region DML

        public static int JetDelete(JET_SESID sesid, JET_TABLEID tableid)
        {
            ErrApi.TraceFunctionCall("JetDelete");
            return Err(NativeMethods.JetDelete(sesid.Value, tableid.Value));
        }

        public static int JetPrepareUpdate(JET_SESID sesid, JET_TABLEID tableid, JET_prep prep)
        {
            ErrApi.TraceFunctionCall("JetPrepareUpdate");
            return Err(NativeMethods.JetPrepareUpdate(sesid.Value, tableid.Value, (uint)prep));
        }

        public static int JetUpdate(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize, out int actualBookmarkSize)
        {
            ErrApi.TraceFunctionCall("JetUpdate");
            actualBookmarkSize = 0;

            if ((null != bookmark) && bookmarkSize > bookmark.Length)
            {
                throw new ArgumentException(
                    "bookmarkSize cannot be greater than the length of the bookmark",
                    "bookmarkSize");
            }

            uint cbActual = 0;
            var gch = GCHandle.Alloc(bookmark, GCHandleType.Pinned);
            int err = Err(NativeMethods.JetUpdate(sesid.Value, tableid.Value, gch.AddrOfPinnedObject(), (uint)bookmarkSize, ref cbActual));
            actualBookmarkSize = (int)cbActual;
            return err;
        }

        public static int JetSetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, byte[] data, int dataSize, SetColumnGrbit grbit, JET_SETINFO setinfo)
        {
            ErrApi.TraceFunctionCall("JetSetColumn");
            if (null == data)
            {
                if (data.Length > 0 && (SetColumnGrbit.SizeLV != (grbit & SetColumnGrbit.SizeLV)))
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

            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            if (null != setinfo)
            {
                var nativeSetinfo = setinfo.GetNativeSetinfo();
                var setinfoHandle = GCHandle.Alloc(nativeSetinfo, GCHandleType.Pinned);
                return Err(NativeMethods.JetSetColumn(sesid.Value, tableid.Value, columnid.Value, dataHandle.AddrOfPinnedObject(), (uint)dataSize, (uint)grbit, setinfoHandle.AddrOfPinnedObject()));
            }
            else
            {
                return Err(NativeMethods.JetSetColumn(sesid.Value, tableid.Value, columnid.Value, dataHandle.AddrOfPinnedObject(), (uint)dataSize, (uint)grbit, IntPtr.Zero));
            }
        }

        public static int JetGetLock(JET_SESID sesid, JET_TABLEID tableid, GetLockGrbit grbit)
        {
            ErrApi.TraceFunctionCall("JetGetLock");
            return Err(NativeMethods.JetGetLock(sesid.Value, tableid.Value, (uint)grbit));
        }

        #endregion

        /// <summary>
        /// Make sure the data and dataSize arguments match.
        /// </summary>
        /// <param name="data">The data buffer.</param>
        /// <param name="dataSize">The size of the data.</param>
        private static void CheckDataSize(byte[] data, int dataSize)
        {
            if ((null == data && 0 != dataSize) || (null != data && dataSize > data.Length))
            {
                throw new ArgumentException(
                    "dataSize cannot be greater than the length of the data",
                    "dataSize");
            }
        }

        /// <summary>
        /// Trace a call to an ESENT function.
        /// </summary>
        /// <param name="function">The name of the function being called.</param>
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
            if (0 == err && ErrApi.traceSwitch.TraceVerbose)
            {
                Trace.WriteLine("JET_err.Success");
            }
            else if (err > 0 && ErrApi.traceSwitch.TraceWarning)
            {
                Trace.WriteLine((JET_wrn)err);
            }
            else if (ErrApi.traceSwitch.TraceError)
            {
                Trace.WriteLine((JET_err)err);
            }

            return err;
        }
    }
}