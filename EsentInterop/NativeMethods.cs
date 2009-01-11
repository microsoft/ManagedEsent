//-----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// Native interop for functions in esent.dll.
    /// </summary>
    internal static class NativeMethods
    {
        #region Win32 Constants

        public const uint NORM_IGNORECASE = 0x00000001;  // ignore case
        public const uint NORM_IGNORENONSPACE = 0x00000002;  // ignore nonspacing chars
        public const uint NORM_IGNORESYMBOLS = 0x00000004;  // ignore symbols
        public const uint NORM_IGNOREKANATYPE = 0x00010000;  // ignore kanatype
        public const uint NORM_IGNOREWIDTH = 0x00020000;  // ignore width

        #endregion Win32 Constants

        #region Configuration Constants

        /// <summary>
        /// The name of the DLL that the methods should be loaded from.
        /// </summary>
        private const string EsentDLL = "esent.dll";

        /// <summary>
        /// The CharSet for the methods in the DLL.
        /// </summary>
        private const CharSet EsentCharSet = CharSet.Ansi;

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

        [DllImport(EsentDLL, CharSet = EsentCharSet)]
        public static extern int JetCreateInstance(ref IntPtr instance, string szInstanceName);

        [DllImport(EsentDLL)]
        public static extern int JetInit(ref IntPtr instance);

        [DllImport(EsentDLL)]
        public static extern int JetTerm(IntPtr instance);

        [DllImport(EsentDLL, CharSet = EsentCharSet)]
        public static extern int JetSetSystemParameter(IntPtr pinstance, IntPtr sesid, uint paramid, IntPtr lParam, string szParam);

        [DllImport(EsentDLL)]
        public static extern int JetGetSystemParameter(IntPtr instance, IntPtr sesid, uint paramid, ref IntPtr plParam, StringBuilder szParam, uint cbMax);

        #endregion

        #region Databases

        [DllImport(EsentDLL, CharSet = EsentCharSet)]
        public static extern int JetCreateDatabase(IntPtr sesid, string szFilename, string szConnect, ref uint dbid, uint grbit);

        [DllImport(EsentDLL, CharSet = EsentCharSet)]
        public static extern int JetAttachDatabase(IntPtr sesid, string szFilename, uint grbit);

        [DllImport(EsentDLL, CharSet = EsentCharSet)]
        public static extern int JetDetachDatabase(IntPtr sesid, string szFilename);

        [DllImport(EsentDLL, CharSet = EsentCharSet)]
        public static extern int JetOpenDatabase(IntPtr sesid, string database, string szConnect, ref uint dbid, uint grbit);

        [DllImport(EsentDLL)]
        public static extern int JetCloseDatabase(IntPtr sesid, uint dbid, uint grbit);

        #endregion

        #region sessions

        [DllImport(EsentDLL, CharSet = EsentCharSet)]
        public static extern int JetBeginSession(IntPtr instance, ref IntPtr session, string username, string password);

        [DllImport(EsentDLL)]
        public static extern int JetEndSession(IntPtr sesid, uint grbit);

        [DllImport(EsentDLL)]
        public static extern int JetDupSession(IntPtr sesid, ref IntPtr newSesid);

        #endregion

        #region tables

        [DllImport(EsentDLL)]
        public static extern int JetOpenTable(IntPtr sesid, uint dbid, string tablename, IntPtr pvParameters, uint cbParameters, uint grbit, ref IntPtr tableid);

        [DllImport(EsentDLL)]
        public static extern int JetCloseTable(IntPtr sesid, IntPtr tableid);

        [DllImport(EsentDLL)]
        public static extern int JetDupCursor(IntPtr sesid, IntPtr tableid, ref IntPtr tableidNew, uint grbit);

        #endregion

        #region transactions

        [DllImport(EsentDLL)]
        public static extern int JetBeginTransaction(IntPtr sesid);

        [DllImport(EsentDLL)]
        public static extern int JetCommitTransaction(IntPtr sesid, uint grbit);

        [DllImport(EsentDLL)]
        public static extern int JetRollback(IntPtr sesid, uint grbit);

        #endregion

        #region DDL

        [DllImport(EsentDLL, CharSet = EsentCharSet)]
        public static extern int JetCreateTable(IntPtr sesid, uint dbid, string szTableName, int pages, int density, ref IntPtr tableid);

        [DllImport(EsentDLL, CharSet = EsentCharSet)]
        public static extern int JetAddColumn(IntPtr sesid, IntPtr tableid, string szColumnName, ref NATIVE_COLUMNDEF columndef, IntPtr pvDefault, uint cbDefault, ref uint columnid);

        [DllImport(EsentDLL, CharSet = EsentCharSet)]
        public static extern int JetDeleteColumn(IntPtr sesid, IntPtr tableid, string szColumnName);

        [DllImport(EsentDLL, CharSet = EsentCharSet)]
        public static extern int JetDeleteIndex(IntPtr sesid, IntPtr tableid, string szIndexName);

        [DllImport(EsentDLL, CharSet = EsentCharSet)]
        public static extern int JetDeleteTable(IntPtr sesid, uint dbid, string szTableName);

        [DllImport(EsentDLL, CharSet = EsentCharSet)]
        public static extern int JetCreateIndex(IntPtr sesid, IntPtr tableid, string szIndexName, uint grbit, string szKey, uint cbKey, uint lDensity);
        
        [DllImport(EsentDLL, CharSet = EsentCharSet)]
        public static extern int JetGetTableColumnInfo(IntPtr sesid, IntPtr tableid, string szColumnName, ref NATIVE_COLUMNDEF columndef, uint cbMax, uint InfoLevel);

        [DllImport(EsentDLL, CharSet = EsentCharSet)]
        public static extern int JetGetTableColumnInfo(IntPtr sesid, IntPtr tableid, ref uint pcolumnid, ref NATIVE_COLUMNDEF columndef, uint cbMax, uint InfoLevel);

        [DllImport(EsentDLL, CharSet = EsentCharSet)]
        public static extern int JetGetTableColumnInfo(IntPtr sesid, IntPtr tableid, string szIgnored, ref NATIVE_COLUMNLIST columnlist, uint cbMax, uint InfoLevel);

        [DllImport(EsentDLL, CharSet = EsentCharSet)]
        public static extern int JetGetColumnInfo(IntPtr sesid, uint dbid, string szTableName, string szColumnName, ref NATIVE_COLUMNDEF columndef, uint cbMax, uint InfoLevel);

        [DllImport(EsentDLL, CharSet = EsentCharSet)]
        public static extern int JetGetColumnInfo(IntPtr sesid, uint dbid, string szTableName, string szColumnName, ref NATIVE_COLUMNLIST columnlist, uint cbMax, uint InfoLevel);

        [DllImport(EsentDLL, CharSet = EsentCharSet)]
        public static extern int JetGetObjectInfo(IntPtr sesid, uint dbid, uint objtyp, string szContainerName, string szObjectName, ref NATIVE_OBJECTLIST objectlist, uint cbMax, uint InfoLevel);

        [DllImport(EsentDLL, CharSet = EsentCharSet)]
        public static extern int JetGetIndexInfo(IntPtr sesid, uint dbid, string szTableName, string szIndexName, ref NATIVE_INDEXLIST indexlist, uint cbResult, uint InfoLevel);

        [DllImport(EsentDLL, CharSet = EsentCharSet)]
        public static extern int JetGetTableIndexInfo(IntPtr sesid, IntPtr tableid, string szIndexName, ref NATIVE_INDEXLIST indexlist, uint cbResult, uint InfoLevel);

        #endregion

        #region Navigation

        [DllImport(EsentDLL)]
        public static extern int JetGetBookmark(IntPtr sesid, IntPtr tableid, IntPtr pvBookmark, uint cbMax, ref uint cbActual);

        [DllImport(EsentDLL)]
        public static extern int JetGotoBookmark(IntPtr sesid, IntPtr tableid, IntPtr pvBookmark, uint cbBookmark);

        [DllImport(EsentDLL)]
        public static extern int JetRetrieveColumn(IntPtr sesid, IntPtr tableid, uint columnid, IntPtr pvData, uint cbData, ref uint cbActual, uint grbit, IntPtr pretinfo);

        [DllImport(EsentDLL)]
        public static extern int JetMove(IntPtr sesid, IntPtr tableid, int cRow, uint grbit);

        [DllImport(EsentDLL)]
        public static extern int JetMakeKey(IntPtr sesid, IntPtr tableid, IntPtr pvData, uint cbData, uint grbit);

        [DllImport(EsentDLL)]
        public static extern int JetRetrieveKey(IntPtr sesid, IntPtr tableid, IntPtr pvData, uint cbMax, ref uint cbActual, uint grbit);

        [DllImport(EsentDLL)]
        public static extern int JetSeek(IntPtr sesid, IntPtr tableid, uint grbit);

        [DllImport(EsentDLL)]
        public static extern int JetSetIndexRange(IntPtr sesid, IntPtr tableid, uint grbit);

        [DllImport(EsentDLL)]
        public static extern int JetSetCurrentIndex(IntPtr sesid, IntPtr tableid, string szIndexName);

        [DllImport(EsentDLL)]
        public static extern int JetIndexRecordCount(IntPtr sesid, IntPtr tableid, ref uint crec, uint crecMax);

        [DllImport(EsentDLL)]
        public static extern int JetSetTableSequential(IntPtr sesid, IntPtr tableid, uint grbit);

        [DllImport(EsentDLL)]
        public static extern int JetResetTableSequential(IntPtr sesid, IntPtr tableid, uint grbit);

        [DllImport(EsentDLL)]
        public static extern int JetGetRecordPosition(IntPtr sesid, IntPtr tableid, ref NATIVE_RECPOS precpos, uint cbRecpos);

        [DllImport(EsentDLL)]
        public static extern int JetGotoPosition(IntPtr sesid, IntPtr tableid, ref NATIVE_RECPOS precpos);

        #endregion

        #region DML

        [DllImport(EsentDLL)]
        public static extern int JetDelete(IntPtr sesid, IntPtr tableid);

        [DllImport(EsentDLL)]
        public static extern int JetPrepareUpdate(IntPtr sesid, IntPtr tableid, uint prep);

        [DllImport(EsentDLL)]
        public static extern int JetUpdate(IntPtr sesid, IntPtr tableid, IntPtr pvBookmark, uint cbBookmark, ref uint cbActual);

        [DllImport(EsentDLL)]
        public static extern int JetSetColumn(IntPtr sesid, IntPtr tableid, uint columnid, IntPtr pvData, uint cbData, uint grbit, IntPtr psetinfo);

        [DllImport(EsentDLL)]
        public static extern int JetGetLock(IntPtr sesid, IntPtr tableid, uint grbit);

        #endregion
    }
}