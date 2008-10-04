//-----------------------------------------------------------------------
// <copyright file="UncheckedApi.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Esent.Interop
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Calls to the ESENT interop layer. These calls take the managed types (e.g. JET_SESID) and
    /// return errors.
    /// </summary>
    internal static class UncheckedApi
    {
        #region init/term

        public static int JetCreateInstance(out JET_INSTANCE instance, string name)
        {
            instance.Value = IntPtr.Zero;
            return NativeMethods.JetCreateInstance(ref instance.Value, name);
        }

        public static int JetInit(ref JET_INSTANCE instance)
        {
            return NativeMethods.JetInit(ref instance.Value);
        }

        public static int JetTerm(JET_INSTANCE instance)
        {
            return NativeMethods.JetTerm(instance.Value);
        }

        public static int JetSetSystemParameter(JET_INSTANCE instance, JET_SESID sesid, JET_param paramid, int paramValue, string paramString)
        {
            if (IntPtr.Zero == instance.Value)
            {
                return NativeMethods.JetSetSystemParameter(IntPtr.Zero, sesid.Value, (uint)paramid, (IntPtr)paramValue, paramString);
            }
            else
            {
                GCHandle instanceHandle = GCHandle.Alloc(instance, GCHandleType.Pinned);
                return NativeMethods.JetSetSystemParameter(instanceHandle.AddrOfPinnedObject(), sesid.Value, (uint)paramid, (IntPtr)paramValue, paramString);
            }
        }

        public static int JetGetSystemParameter(JET_INSTANCE instance, JET_SESID sesid, JET_param paramid, out int paramValue, out string paramString, int maxParam)
        {
            IntPtr stringValue = Marshal.AllocHGlobal(maxParam);

            // Null terminate the string, in case ESENT doesn't set it
            Marshal.WriteByte(stringValue, 0);
            try
            {
                IntPtr intValue = IntPtr.Zero;
                int err = NativeMethods.JetGetSystemParameter(instance.Value, sesid.Value, (uint)paramid, ref intValue, stringValue, (uint)maxParam);
                paramString = Marshal.PtrToStringAnsi(stringValue);
                paramValue = intValue.ToInt32();
                return err;
            }
            finally
            {
                Marshal.FreeHGlobal(stringValue);
            }
        }

        #endregion

        #region sessions

        public static int JetBeginSession(JET_INSTANCE instance, out JET_SESID sesid, string username, string password)
        {
            sesid = JET_SESID.Nil;
            return NativeMethods.JetBeginSession(instance.Value, ref sesid.Value, username, password);
        }

        public static int JetEndSession(JET_SESID sesid, EndSessionGrbit grbit)
        {
            return NativeMethods.JetEndSession(sesid.Value, (uint)grbit);
        }

        #endregion

        #region tables

        public static int JetOpenTable(JET_SESID sesid, JET_DBID dbid, string tablename, byte[] parameters, int parametersLength, OpenTableGrbit grbit, out JET_TABLEID tableid)
        {
            tableid = JET_TABLEID.Nil;
            return NativeMethods.JetOpenTable(sesid.Value, dbid.Value, tablename, IntPtr.Zero, 0, (uint)grbit, ref tableid.Value);
        }

        public static int JetCloseTable(JET_SESID sesid, JET_TABLEID tableid)
        {
            return NativeMethods.JetCloseTable(sesid.Value, tableid.Value);
        }

        #endregion

        #region transactions

        public static int JetBeginTransaction(JET_SESID sesid)
        {
            return NativeMethods.JetBeginTransaction(sesid.Value);
        }

        public static int JetCommitTransaction(JET_SESID sesid, CommitTransactionGrbit grbit)
        {
            return NativeMethods.JetCommitTransaction(sesid.Value, (uint)grbit);
        }

        public static int JetRollback(JET_SESID sesid, RollbackTransactionGrbit grbit)
        {
            return NativeMethods.JetRollback(sesid.Value, (uint)grbit);
        }

        #endregion

        #region DDL

        public static int JetCreateDatabase(JET_SESID sesid, string database, string connect, out JET_DBID dbid, CreateDatabaseGrbit grbit)
        {
            dbid = JET_DBID.Nil;
            return NativeMethods.JetCreateDatabase(sesid.Value, database, connect, ref dbid.Value, (uint)grbit);
        }

        public static int JetCreateTable(JET_SESID sesid, JET_DBID dbid, string table, int pages, int density, out JET_TABLEID tableid)
        {
            tableid = JET_TABLEID.Nil;
            return NativeMethods.JetCreateTable(sesid.Value, dbid.Value, table, pages, density, ref tableid.Value);
        }

        public static int JetAddColumn(JET_SESID sesid, JET_TABLEID tableid, string column, JET_COLUMNDEF columndef, byte[] defaultValue, int defaultValueSize, out JET_COLUMNID columnid)
        {
            columnid = JET_COLUMNID.Nil;

            if ((null != defaultValue) && defaultValueSize > defaultValue.Length)
            {
                throw new ArgumentException(
                    "defaultValueSize cannot be greater than the length of the defaultValue array",
                    "defaultValueSize");
            }

            var nativeColumndef = columndef.GetNativeColumndef();
            var gch = GCHandle.Alloc(defaultValue, GCHandleType.Pinned);
            return NativeMethods.JetAddColumn(sesid.Value, tableid.Value, column, ref nativeColumndef, gch.AddrOfPinnedObject(), (uint)defaultValueSize, ref columnid.Value);
        }

        #endregion

        #region Navigation

        public static int JetGetBookmark(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize, out int actualBookmarkSize)
        {
            actualBookmarkSize = 0;

            if ((null == bookmark && 0 != bookmarkSize) || bookmarkSize > bookmark.Length)
            {
                throw new ArgumentException(
                    "bookmarkSize cannot be greater than the length of the bookmark",
                    "bookmarkSize");
            }

            uint cbActual = 0;
            var bookmarkHandle = GCHandle.Alloc(bookmark, GCHandleType.Pinned);
            int err = NativeMethods.JetGetBookmark(sesid.Value, tableid.Value, bookmarkHandle.AddrOfPinnedObject(), (uint)bookmarkSize, ref cbActual);
            actualBookmarkSize = (int)cbActual;
            return err;
        }

        public static int JetGotoBookmark(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize)
        {
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
            return NativeMethods.JetGotoBookmark(sesid.Value, tableid.Value, bookmarkHandle.AddrOfPinnedObject(), (uint)bookmarkSize);
        }

        public static int JetRetrieveColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, byte[] data, int dataSize, out int actualDataSize, RetrieveColumnGrbit grbit, JET_RETINFO retinfo)
        {
            actualDataSize = 0;

            if ((null == data && 0 != dataSize) || (null != data && dataSize > data.Length))
            {
                throw new ArgumentException(
                    "dataSize cannot be greater than the length of the data",
                    "dataSize");
            }

            int err;
            uint cbActual = 0;
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            if (null != retinfo)
            {
                var nativeRetinfo = retinfo.GetNativeRetinfo();
                var retinfoHandle = GCHandle.Alloc(nativeRetinfo, GCHandleType.Pinned);
                err = NativeMethods.JetRetrieveColumn(sesid.Value, tableid.Value, columnid.Value, dataHandle.AddrOfPinnedObject(), (uint)dataSize, ref cbActual, (uint)grbit, retinfoHandle.AddrOfPinnedObject());
                retinfo.SetFromNativeRetinfo(nativeRetinfo);
            }
            else
            {
                err = NativeMethods.JetRetrieveColumn(sesid.Value, tableid.Value, columnid.Value, dataHandle.AddrOfPinnedObject(), (uint)dataSize, ref cbActual, (uint)grbit, IntPtr.Zero);
            }

            actualDataSize = (int)cbActual;
            return err;
        }

        public static int JetMove(JET_SESID sesid, JET_TABLEID tableid, int numRows, MoveGrbit grbit)
        {
            return NativeMethods.JetMove(sesid.Value, tableid.Value, numRows, (uint)grbit);
        }

        #endregion

        #region DML

        public static int JetDelete(JET_SESID sesid, JET_TABLEID tableid)
        {
            return NativeMethods.JetDelete(sesid.Value, tableid.Value);
        }

        public static int JetPrepareUpdate(JET_SESID sesid, JET_TABLEID tableid, JET_prep prep)
        {
            return NativeMethods.JetPrepareUpdate(sesid.Value, tableid.Value, (uint)prep);
        }

        public static int JetUpdate(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize, out int actualBookmarkSize)
        {
            actualBookmarkSize = 0;

            if ((null != bookmark) && bookmarkSize > bookmark.Length)
            {
                throw new ArgumentException(
                    "bookmarkSize cannot be greater than the length of the bookmark",
                    "bookmarkSize");
            }

            uint cbActual = 0;
            var gch = GCHandle.Alloc(bookmark, GCHandleType.Pinned);
            int err = NativeMethods.JetUpdate(sesid.Value, tableid.Value, gch.AddrOfPinnedObject(), (uint)bookmarkSize, ref cbActual);
            actualBookmarkSize = (int)cbActual;
            return err;
        }

        public static int JetSetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, byte[] data, int dataSize, SetColumnGrbit grbit, JET_SETINFO setinfo)
        {
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
                return NativeMethods.JetSetColumn(sesid.Value, tableid.Value, columnid.Value, dataHandle.AddrOfPinnedObject(), (uint)dataSize, (uint)grbit, setinfoHandle.AddrOfPinnedObject());
            }
            else
            {
                return NativeMethods.JetSetColumn(sesid.Value, tableid.Value, columnid.Value, dataHandle.AddrOfPinnedObject(), (uint)dataSize, (uint)grbit, IntPtr.Zero);
            }

        #endregion
        }
    }
}