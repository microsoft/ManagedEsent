//-----------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Esent.Interop
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Native interop for functions in esent.dll.
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        /// The name of the DLL that the methods should be loaded from.
        /// </summary>
        private const string EsentDLL = "esent.dll";

        #region init/term

        [DllImport(EsentDLL, CharSet = CharSet.Ansi)]
        public static extern int JetCreateInstance(ref IntPtr instance, string szInstanceName);

        [DllImport(EsentDLL)]
        public static extern int JetInit(ref IntPtr instance);

        [DllImport(EsentDLL)]
        public static extern int JetTerm(IntPtr instance);

        [DllImport(EsentDLL, CharSet = CharSet.Ansi)]
        public static extern int JetSetSystemParameter(IntPtr pinstance, IntPtr sesid, uint paramid, IntPtr lParam, string szParam);

        [DllImport(EsentDLL)]
        public static extern int JetGetSystemParameter(IntPtr instance, IntPtr sesid, uint paramid, ref IntPtr plParam, IntPtr szParam, uint cbMax);

        #endregion

        #region sessions

        [DllImport(EsentDLL, CharSet = CharSet.Ansi)]
        public static extern int JetBeginSession(IntPtr instance, ref IntPtr session, string username, string password);

        [DllImport(EsentDLL)]
        public static extern int JetEndSession(IntPtr sesid, uint grbit);

        #endregion

        #region tables

        [DllImport(EsentDLL)]
        public static extern int JetOpenTable(IntPtr sesid, uint dbid, string tablename, IntPtr pvParameters, uint cbParameters, uint grbit, ref IntPtr tableid);

        [DllImport(EsentDLL)]
        public static extern int JetCloseTable(IntPtr sesid, IntPtr tableid);

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

        [DllImport(EsentDLL, CharSet = CharSet.Ansi)]
        public static extern int JetCreateDatabase(IntPtr sesid, string database, string connect, ref uint dbid, uint grbit);

        [DllImport(EsentDLL, CharSet = CharSet.Ansi)]
        public static extern int JetCreateTable(IntPtr sesid, uint dbid, string table, int pages, int density, ref IntPtr tableid);

        [DllImport(EsentDLL, CharSet = CharSet.Ansi)]
        public static extern int JetAddColumn(IntPtr sesid, IntPtr tableid, string column, ref NATIVE_COLUMNDEF columndef, IntPtr pvDefault, uint cbDefault, ref uint columnid);

        #endregion

        #region Navigation

        [DllImport(EsentDLL)]
        public static extern int JetGetBookmark(IntPtr sesid, IntPtr tableid, IntPtr pvBookmark, uint cbMax, ref uint cbActual);

        [DllImport(EsentDLL)]
        public static extern int JetGotoBookmark(IntPtr sesid, IntPtr tableid, IntPtr pvBookmark, uint cbBookmark);

        /*
        JET_ERR JET_API JetRetrieveColumn(
  __in          JET_SESID sesid,
  __in          JET_TABLEID tableid,
  __in          JET_COLUMNID columnid,
  __out_opt     void* pvData,
  __in          unsigned long cbData,
  __out_opt     unsigned long* pcbActual,
  __in          JET_GRBIT grbit,
  __in_out_opt  JET_RETINFO* pretinfo
);
         */

        [DllImport(EsentDLL)]
        public static extern int JetRetrieveColumn(IntPtr sesid, IntPtr tableid, uint columnid, IntPtr pvData, uint cbData, ref uint cbActual, uint grbit, IntPtr pretinfo);

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

        #endregion
    }
}