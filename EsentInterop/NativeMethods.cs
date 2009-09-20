//-----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using Microsoft.Isam.Esent.Interop.Vista;

    /// <summary>
    /// Native interop for functions in esent.dll.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    [BestFitMapping(false, ThrowOnUnmappableChar = false)]
    internal static class NativeMethods
    {
        #region Configuration Constants

        /// <summary>
        /// The CharSet for the methods in the DLL.
        /// </summary>
        private const CharSet EsentCharSet = CharSet.Ansi;

        /// <summary>
        /// The name of the DLL that the methods should be loaded from.
        /// </summary>
        private const string EsentDll = "esent.dll";

        /// <summary>
        /// Initializes static members of the NativeMethods class.
        /// </summary>
        static NativeMethods()
        {
            // This must be changed when the CharSet is changed.
            NativeMethods.Encoding = Encoding.ASCII;
        }

        /// <summary>
        /// Gets encoding to be used when converting data to/from byte arrays.
        /// This should match the CharSet above.
        /// </summary>
        public static Encoding Encoding { get; private set; }

        #endregion Configuration Constants

        #region init/term

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetCreateInstance(out IntPtr instance, string szInstanceName);

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetCreateInstance2(out IntPtr instance, string szInstanceName, string szDisplayName, uint grbit);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetInit(ref IntPtr instance);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetInit2(ref IntPtr instance, uint grbit);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetTerm(IntPtr instance);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetTerm2(IntPtr instance, uint grbit);
        
        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static unsafe extern int JetSetSystemParameter(IntPtr* pinstance, IntPtr sesid, uint paramid, IntPtr lParam, string szParam);

        [DllImport(EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static unsafe extern int JetSetSystemParameterW(IntPtr* pinstance, IntPtr sesid, uint paramid, IntPtr lParam, string szParam);

        // The param is ref because it is an 'in' parameter when getting error text
        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetSystemParameter(IntPtr instance, IntPtr sesid, uint paramid, ref IntPtr plParam, StringBuilder szParam, uint cbMax);

        [DllImport(EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetGetSystemParameterW(IntPtr instance, IntPtr sesid, uint paramid, ref IntPtr plParam, StringBuilder szParam, uint cbMax);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetGetVersion(IntPtr sesid, out uint dwVersion);

        #endregion

        #region Databases

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetCreateDatabase(IntPtr sesid, string szFilename, string szConnect, out uint dbid, uint grbit);

        [DllImport(EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetCreateDatabaseW(IntPtr sesid, string szFilename, string szConnect, out uint dbid, uint grbit);

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetAttachDatabase(IntPtr sesid, string szFilename, uint grbit);

        [DllImport(EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetAttachDatabaseW(IntPtr sesid, string szFilename, uint grbit);

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetDetachDatabase(IntPtr sesid, string szFilename);

        [DllImport(EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetDetachDatabaseW(IntPtr sesid, string szFilename);

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetOpenDatabase(IntPtr sesid, string database, string szConnect, out uint dbid, uint grbit);

        [DllImport(EsentDll, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int JetOpenDatabaseW(IntPtr sesid, string database, string szConnect, out uint dbid, uint grbit);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetCloseDatabase(IntPtr sesid, uint dbid, uint grbit);

        #endregion

        #region sessions

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetBeginSession(IntPtr instance, out IntPtr session, string username, string password);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetSetSessionContext(IntPtr session, IntPtr context);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetResetSessionContext(IntPtr session);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetEndSession(IntPtr sesid, uint grbit);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetDupSession(IntPtr sesid, out IntPtr newSesid);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetGetThreadStats(out NATIVE_THREADSTATS pvResult, uint cbMax);

        #endregion

        #region tables

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetOpenTable(IntPtr sesid, uint dbid, string tablename, IntPtr pvParameters, uint cbParameters, uint grbit, out IntPtr tableid);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetCloseTable(IntPtr sesid, IntPtr tableid);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetDupCursor(IntPtr sesid, IntPtr tableid, out IntPtr tableidNew, uint grbit);

        #endregion

        #region transactions

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetBeginTransaction(IntPtr sesid);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetCommitTransaction(IntPtr sesid, uint grbit);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetRollback(IntPtr sesid, uint grbit);

        #endregion

        #region DDL

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetCreateTable(IntPtr sesid, uint dbid, string szTableName, int pages, int density, out IntPtr tableid);

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetAddColumn(IntPtr sesid, IntPtr tableid, string szColumnName, [In] ref NATIVE_COLUMNDEF columndef, [In] byte[] pvDefault, uint cbDefault, out uint columnid);

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetDeleteColumn(IntPtr sesid, IntPtr tableid, string szColumnName);

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetDeleteIndex(IntPtr sesid, IntPtr tableid, string szIndexName);

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetDeleteTable(IntPtr sesid, uint dbid, string szTableName);

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetCreateIndex(IntPtr sesid, IntPtr tableid, string szIndexName, uint grbit, string szKey, uint cbKey, uint lDensity);

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetCreateIndex2(
            IntPtr sesid, IntPtr tableid, [In] NATIVE_INDEXCREATE[] pindexcreate, uint cIndexCreate);

        // Introduced in Windows Vista, this versions takes the larger NATIVE_INDEXCREATE2 structure
        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetCreateIndex2(
            IntPtr sesid, IntPtr tableid, [In] NATIVE_INDEXCREATE2[] pindexcreate, uint cIndexCreate);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetOpenTempTable(
            IntPtr sesid,
            [In] NATIVE_COLUMNDEF[] rgcolumndef,
            uint ccolumn,
            uint grbit,
            out IntPtr ptableid,
            [Out] uint[] rgcolumnid);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetOpenTempTable2(
            IntPtr sesid,
            [In] NATIVE_COLUMNDEF[] rgcolumndef,
            uint ccolumn,
            uint lcid,
            uint grbit,
            out IntPtr ptableid,
            [Out] uint[] rgcolumnid);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetOpenTempTable3(
            IntPtr sesid,
            [In] NATIVE_COLUMNDEF[] rgcolumndef,
            uint ccolumn,
            [In] ref NATIVE_UNICODEINDEX pidxunicode,
            uint grbit,
            out IntPtr ptableid,
            [Out] uint[] rgcolumnid);

        // Introduced in Windows Vista
        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetOpenTemporaryTable(IntPtr sesid, [In] [Out] ref NATIVE_OPENTEMPORARYTABLE popentemporarytable);

        // Overload to allow for null pidxunicode
        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetOpenTempTable3(
            IntPtr sesid,
            [In] NATIVE_COLUMNDEF[] rgcolumndef,
            uint ccolumn,
            IntPtr pidxunicode,
            uint grbit,
            out IntPtr ptableid,
            [Out] uint[] rgcolumnid);

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetTableColumnInfo(IntPtr sesid, IntPtr tableid, string szColumnName, ref NATIVE_COLUMNDEF columndef, uint cbMax, uint InfoLevel);

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetTableColumnInfo(IntPtr sesid, IntPtr tableid, ref uint pcolumnid, ref NATIVE_COLUMNDEF columndef, uint cbMax, uint InfoLevel);

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetTableColumnInfo(IntPtr sesid, IntPtr tableid, string szIgnored, ref NATIVE_COLUMNLIST columnlist, uint cbMax, uint InfoLevel);

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetColumnInfo(IntPtr sesid, uint dbid, string szTableName, string szColumnName, ref NATIVE_COLUMNDEF columndef, uint cbMax, uint InfoLevel);

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetColumnInfo(IntPtr sesid, uint dbid, string szTableName, string szColumnName, ref NATIVE_COLUMNLIST columnlist, uint cbMax, uint InfoLevel);

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetObjectInfo(
            IntPtr sesid,
            uint dbid,
            uint objtyp,
            string szContainerName,
            string szObjectName,
            [In] [Out] ref NATIVE_OBJECTLIST objectlist,
            uint cbMax,
            uint InfoLevel);

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetCurrentIndex(IntPtr sesid, IntPtr tableid, StringBuilder szIndexName, uint cchIndexName);

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetIndexInfo(
            IntPtr sesid,
            uint dbid,
            string szTableName,
            string szIndexName,
            [In] [Out] ref NATIVE_INDEXLIST indexlist,
            uint cbResult,
            uint InfoLevel);

        [DllImport(EsentDll, CharSet = EsentCharSet, ExactSpelling = true)]
        public static extern int JetGetTableIndexInfo(
            IntPtr sesid,
            IntPtr tableid,
            string szIndexName,
            [In] [Out] ref NATIVE_INDEXLIST indexlist,
            uint cbResult,
            uint InfoLevel);

        #endregion

        #region Navigation

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetGotoBookmark(IntPtr sesid, IntPtr tableid, [In] byte[] pvBookmark, uint cbBookmark);

        // This has IntPtr and NATIVE_RETINFO versions because the parameter can be null
        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetMove(IntPtr sesid, IntPtr tableid, int cRow, uint grbit);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetMakeKey(IntPtr sesid, IntPtr tableid, IntPtr pvData, uint cbData, uint grbit);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetSeek(IntPtr sesid, IntPtr tableid, uint grbit);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetSetIndexRange(IntPtr sesid, IntPtr tableid, uint grbit);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetIntersectIndexes(
            IntPtr sesid,
            [In] NATIVE_INDEXRANGE[] rgindexrange,
            uint cindexrange,
            [In] [Out] ref NATIVE_RECORDLIST recordlist,
            uint grbit);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetSetCurrentIndex(IntPtr sesid, IntPtr tableid, string szIndexName);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetIndexRecordCount(IntPtr sesid, IntPtr tableid, out uint crec, uint crecMax);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetSetTableSequential(IntPtr sesid, IntPtr tableid, uint grbit);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetResetTableSequential(IntPtr sesid, IntPtr tableid, uint grbit);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetGetRecordPosition(IntPtr sesid, IntPtr tableid, out NATIVE_RECPOS precpos, uint cbRecpos);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetGotoPosition(IntPtr sesid, IntPtr tableid, [In] ref NATIVE_RECPOS precpos);

        #endregion

        #region Data Retrieval

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetGetBookmark(IntPtr sesid, IntPtr tableid, [Out] byte[] pvBookmark, uint cbMax, out uint cbActual);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetRetrieveColumn(IntPtr sesid, IntPtr tableid, uint columnid, IntPtr pvData, uint cbData, out uint cbActual, uint grbit, IntPtr pretinfo);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetRetrieveColumn(
            IntPtr sesid,
            IntPtr tableid,
            uint columnid,
            IntPtr pvData,
            uint cbData,
            out uint cbActual,
            uint grbit,
            [In] [Out] ref NATIVE_RETINFO pretinfo);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static unsafe extern int JetRetrieveColumns(
            IntPtr sesid, IntPtr tableid, [In] [Out] NATIVE_RETRIEVECOLUMN* psetcolumn, uint csetcolumn);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetRetrieveKey(IntPtr sesid, IntPtr tableid, [Out] byte[] pvData, uint cbMax, out uint cbActual, uint grbit);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern unsafe int JetEnumerateColumns(
            IntPtr sesid,
            IntPtr tableid,
            uint cEnumColumnId,
            NATIVE_ENUMCOLUMNID* rgEnumColumnId,
            out uint pcEnumColumn,
            out NATIVE_ENUMCOLUMN* prgEnumColumn,
            JET_PFNREALLOC pfnRealloc,
            IntPtr pvReallocContext,
            uint cbDataMost,
            uint grbit);

        #endregion

        #region DML

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetDelete(IntPtr sesid, IntPtr tableid);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetPrepareUpdate(IntPtr sesid, IntPtr tableid, uint prep);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetUpdate(IntPtr sesid, IntPtr tableid, [Out] byte[] pvBookmark, uint cbBookmark, out uint cbActual);

        // This has IntPtr and NATIVE_SETINFO versions because the parameter can be null
        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetSetColumn(IntPtr sesid, IntPtr tableid, uint columnid, IntPtr pvData, uint cbData, uint grbit, IntPtr psetinfo);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetSetColumn(IntPtr sesid, IntPtr tableid, uint columnid, IntPtr pvData, uint cbData, uint grbit, [In] ref NATIVE_SETINFO psetinfo);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static unsafe extern int JetSetColumns(
            IntPtr sesid, IntPtr tableid, [In] [Out] NATIVE_SETCOLUMN* psetcolumn, uint csetcolumn);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetGetLock(IntPtr sesid, IntPtr tableid, uint grbit);

        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetEscrowUpdate(
            IntPtr sesid,
            IntPtr tableid,
            uint columnid,
            [In] byte[] pv,
            uint cbMax,
            [Out] byte[] pvOld,
            uint cbOldMax,
            out uint cbOldActual,
            uint grbit);

        #endregion

        #region Misc
        [DllImport(EsentDll, ExactSpelling = true)]
        public static extern int JetIdle(IntPtr sesid, uint grbit);
        #endregion
    }
}