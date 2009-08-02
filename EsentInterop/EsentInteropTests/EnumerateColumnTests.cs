//-----------------------------------------------------------------------
// <copyright file="EnumerateColumnTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Isam.Esent.Interop;
using Microsoft.Isam.Esent.Interop.Vista;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
    /// <summary>
    /// Tests for JetEnumerateColumns and helper methods.
    /// </summary>
    [TestClass]
    public class EnumerateColumnTests
    {
        /// <summary>
        /// The directory being used for the database and its files.
        /// </summary>
        private string directory;

        /// <summary>
        /// The path to the database being used by the test.
        /// </summary>
        private string database;

        /// <summary>
        /// The name of the table.
        /// </summary>
        private string table;

        /// <summary>
        /// The instance used by the test.
        /// </summary>
        private JET_INSTANCE instance;

        /// <summary>
        /// The session used by the test.
        /// </summary>
        private JET_SESID sesid;

        /// <summary>
        /// Identifies the database used by the test.
        /// </summary>
        private JET_DBID dbid;

        /// <summary>
        /// The tableid being used by the test.
        /// </summary>
        private JET_TABLEID tableid;

        /// <summary>
        /// A dictionary that maps column names to column ids.
        /// </summary>
        private IDictionary<string, JET_COLUMNID> columnidDict;

        #region Setup/Teardown

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// All DDL should be done in this method.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.directory = SetupHelper.CreateRandomDirectory();
            this.database = Path.Combine(this.directory, "database.edb");
            this.table = "table";
            this.instance = SetupHelper.CreateNewInstance(this.directory);

            // turn off logging so initialization is faster
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.PageTempDBMin, SystemParameters.PageTempDBSmallest, null);
            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesid, String.Empty, String.Empty);
            Api.JetCreateDatabase(this.sesid, this.database, String.Empty, out this.dbid, CreateDatabaseGrbit.None);
            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateTable(this.sesid, this.dbid, this.table, 0, 100, out this.tableid);

            JET_COLUMNID columnid;

            // These columns are all tagged so they are not present in the record
            // by default.

            var columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Bit, grbit = ColumndefGrbit.ColumnTagged };
            Api.JetAddColumn(this.sesid, this.tableid, "Boolean", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.UnsignedByte, grbit = ColumndefGrbit.ColumnTagged };
            Api.JetAddColumn(this.sesid, this.tableid, "Byte", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Short, grbit = ColumndefGrbit.ColumnTagged };
            Api.JetAddColumn(this.sesid, this.tableid, "Int16", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Long, grbit = ColumndefGrbit.ColumnTagged };
            Api.JetAddColumn(this.sesid, this.tableid, "Int32", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Currency, grbit = ColumndefGrbit.ColumnTagged };
            Api.JetAddColumn(this.sesid, this.tableid, "Int64", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.IEEESingle, grbit = ColumndefGrbit.ColumnTagged };
            Api.JetAddColumn(this.sesid, this.tableid, "Float", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.IEEEDouble, grbit = ColumndefGrbit.ColumnTagged };
            Api.JetAddColumn(this.sesid, this.tableid, "Double", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.DateTime, grbit = ColumndefGrbit.ColumnTagged };
            Api.JetAddColumn(this.sesid, this.tableid, "DateTime", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.LongBinary, grbit = ColumndefGrbit.ColumnTagged };
            Api.JetAddColumn(this.sesid, this.tableid, "Binary", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.LongText, cp = JET_CP.ASCII, grbit = ColumndefGrbit.ColumnTagged };
            Api.JetAddColumn(this.sesid, this.tableid, "ASCII", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.LongText, cp = JET_CP.Unicode, grbit = ColumndefGrbit.ColumnTagged };
            Api.JetAddColumn(this.sesid, this.tableid, "Unicode", columndef, null, 0, out columnid);

            if (EsentVersion.SupportsVistaFeatures)
            {
                // Starting with windows Vista esent provides support for these columns.) 
                columndef = new JET_COLUMNDEF() { coltyp = VistaColtyp.UnsignedShort, grbit = ColumndefGrbit.ColumnTagged };
                Api.JetAddColumn(this.sesid, this.tableid, "UInt16", columndef, null, 0, out columnid);

                columndef = new JET_COLUMNDEF() { coltyp = VistaColtyp.UnsignedLong, grbit = ColumndefGrbit.ColumnTagged };
                Api.JetAddColumn(this.sesid, this.tableid, "UInt32", columndef, null, 0, out columnid);

                columndef = new JET_COLUMNDEF() { coltyp = VistaColtyp.GUID, grbit = ColumndefGrbit.ColumnTagged };
                Api.JetAddColumn(this.sesid, this.tableid, "Guid", columndef, null, 0, out columnid);
            }
            else
            {
                // Older version of esent don't support these column types natively so we'll just use binary columns.
                columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Binary, cbMax = 2, grbit = ColumndefGrbit.ColumnTagged };
                Api.JetAddColumn(this.sesid, this.tableid, "UInt16", columndef, null, 0, out columnid);

                columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Binary, cbMax = 4, grbit = ColumndefGrbit.ColumnTagged };
                Api.JetAddColumn(this.sesid, this.tableid, "UInt32", columndef, null, 0, out columnid);

                columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Binary, cbMax = 16, grbit = ColumndefGrbit.ColumnTagged };
                Api.JetAddColumn(this.sesid, this.tableid, "Guid", columndef, null, 0, out columnid);
            }

            // Not natively supported by any version of Esent
            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Binary, cbMax = 8, grbit = ColumndefGrbit.ColumnTagged };
            Api.JetAddColumn(this.sesid, this.tableid, "UInt64", columndef, null, 0, out columnid);

            Api.JetCloseTable(this.sesid, this.tableid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
            Api.JetOpenTable(this.sesid, this.dbid, this.table, null, 0, OpenTableGrbit.None, out this.tableid);

            this.columnidDict = Api.GetColumnDictionary(this.sesid, this.tableid);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            Api.JetCloseTable(this.sesid, this.tableid);
            Api.JetEndSession(this.sesid, EndSessionGrbit.None);
            Api.JetTerm(this.instance);
            Directory.Delete(this.directory, true);
        }

        /// <summary>
        /// Verify that the test class has setup the test fixture properly.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyFixtureSetup()
        {
            Assert.IsNotNull(this.table);
            Assert.AreNotEqual(JET_INSTANCE.Nil, this.instance);
            Assert.AreNotEqual(JET_SESID.Nil, this.sesid);
            Assert.AreNotEqual(JET_DBID.Nil, this.dbid);
            Assert.AreNotEqual(JET_TABLEID.Nil, this.tableid);
            Assert.IsNotNull(this.columnidDict);

            Assert.IsTrue(this.columnidDict.ContainsKey("boolean"));
            Assert.IsTrue(this.columnidDict.ContainsKey("byte"));
            Assert.IsTrue(this.columnidDict.ContainsKey("int16"));
            Assert.IsTrue(this.columnidDict.ContainsKey("int32"));
            Assert.IsTrue(this.columnidDict.ContainsKey("int64"));
            Assert.IsTrue(this.columnidDict.ContainsKey("float"));
            Assert.IsTrue(this.columnidDict.ContainsKey("double"));
            Assert.IsTrue(this.columnidDict.ContainsKey("binary"));
            Assert.IsTrue(this.columnidDict.ContainsKey("ascii"));
            Assert.IsTrue(this.columnidDict.ContainsKey("unicode"));
            Assert.IsTrue(this.columnidDict.ContainsKey("guid"));
            Assert.IsTrue(this.columnidDict.ContainsKey("datetime"));
            Assert.IsTrue(this.columnidDict.ContainsKey("uint16"));
            Assert.IsTrue(this.columnidDict.ContainsKey("uint32"));
            Assert.IsTrue(this.columnidDict.ContainsKey("uint64"));

            Assert.IsFalse(this.columnidDict.ContainsKey("nosuchcolumn"));
        }

        #endregion Setup/Teardown

        #region JetEnumerateColumns tests

        /// <summary>
        /// Enumerate one column.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestEnumerateOneColumn()
        {
            const int Expected = 123;

            this.CreateNewRecord();
            this.SetColumn(this.columnidDict["int32"], BitConverter.GetBytes(Expected), 0);

            int numValues;
            JET_ENUMCOLUMN[] values;
            JET_PFNREALLOC allocator = (context, pv, cb) => IntPtr.Zero == pv ? Marshal.AllocHGlobal(new IntPtr(cb)) : Marshal.ReAllocHGlobal(pv, new IntPtr(cb));
            Api.JetEnumerateColumns(
                this.sesid,
                this.tableid,
                0,
                null,
                out numValues,
                out values,
                allocator,
                IntPtr.Zero,
                0,
                EnumerateColumnsGrbit.EnumerateCompressOutput);

            Assert.AreEqual(1, numValues);
            Assert.IsNotNull(values);
            Assert.AreEqual(this.columnidDict["int32"], values[0].columnid);
            Assert.AreEqual(JET_wrn.ColumnSingleValue, values[0].err);
            Assert.AreEqual(sizeof(int), values[0].cbData);

            int actual = Marshal.ReadInt32(values[0].pvData);
            allocator(IntPtr.Zero, values[0].pvData, 0);    // free the memory
            Assert.AreEqual(Expected, actual);
        }

        /// <summary>
        /// Enumerate one column with multivalues.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestEnumerateOneMultivalueColumn()
        {
            const int Expected1 = 123;
            const int Expected2 = 456;

            this.CreateNewRecord();
            this.SetColumn(this.columnidDict["int32"], BitConverter.GetBytes(Expected1), 0);
            this.SetColumn(this.columnidDict["int32"], BitConverter.GetBytes(Expected2), 0);

            int numValues;
            JET_ENUMCOLUMN[] values;
            JET_PFNREALLOC allocator = (context, pv, cb) => IntPtr.Zero == pv ? Marshal.AllocHGlobal(new IntPtr(cb)) : Marshal.ReAllocHGlobal(pv, new IntPtr(cb));
            Api.JetEnumerateColumns(
                this.sesid,
                this.tableid,
                0,
                null,
                out numValues,
                out values,
                allocator,
                IntPtr.Zero,
                0,
                EnumerateColumnsGrbit.None);

            Assert.AreEqual(1, numValues);
            Assert.IsNotNull(values);
            Assert.AreEqual(this.columnidDict["int32"], values[0].columnid);
            Assert.AreEqual(JET_wrn.Success, values[0].err);
            Assert.AreEqual(2, values[0].cEnumColumnValue);

            int actual1 = Marshal.ReadInt32(values[0].rgEnumColumnValue[0].pvData);
            allocator(IntPtr.Zero, values[0].rgEnumColumnValue[0].pvData, 0);    // free the memory
            int actual2 = Marshal.ReadInt32(values[0].rgEnumColumnValue[1].pvData);
            allocator(IntPtr.Zero, values[0].rgEnumColumnValue[1].pvData, 0);    // free the memory
            Assert.AreEqual(Expected1, actual1);
            Assert.AreEqual(Expected2, actual2);
        }

        /// <summary>
        /// Enumerate specific columns using a JET_ENUMCOLUMNID array.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestEnumerateSpecificColumns()
        {
            const short Expected16 = 2;
            const int Expected32 = 100;
            const long Expected64 = 9999;

            this.CreateNewRecord();
            this.SetColumn(this.columnidDict["int16"], BitConverter.GetBytes(Expected16), 0);
            this.SetColumn(this.columnidDict["int32"], BitConverter.GetBytes(Expected32), 0);
            this.SetColumn(this.columnidDict["int64"], BitConverter.GetBytes(Expected64), 0);
            this.SetColumn(this.columnidDict["unicode"], Encoding.Unicode.GetBytes("test"), 0);
            this.SetColumn(this.columnidDict["ascii"], Encoding.ASCII.GetBytes("test"), 0);

            var columnids = new[]
            {
                new JET_ENUMCOLUMNID { columnid = this.columnidDict["int16"] },
                new JET_ENUMCOLUMNID { columnid = this.columnidDict["int64"] },
            };

            int numValues;
            JET_ENUMCOLUMN[] values;
            JET_PFNREALLOC allocator = (context, pv, cb) => IntPtr.Zero == pv ? Marshal.AllocHGlobal(new IntPtr(cb)) : Marshal.ReAllocHGlobal(pv, new IntPtr(cb));
            Api.JetEnumerateColumns(
                this.sesid,
                this.tableid,
                columnids.Length,
                columnids,
                out numValues,
                out values,
                allocator,
                IntPtr.Zero,
                0,
                EnumerateColumnsGrbit.EnumerateCompressOutput);

            Assert.AreEqual(2, numValues);
            Assert.IsNotNull(values);
            Assert.AreEqual(this.columnidDict["int16"], values[0].columnid);
            Assert.AreEqual(this.columnidDict["int64"], values[1].columnid);
            Assert.AreEqual(JET_wrn.ColumnSingleValue, values[0].err);
            Assert.AreEqual(JET_wrn.ColumnSingleValue, values[1].err);

            short actual16 = Marshal.ReadInt16(values[0].pvData);
            allocator(IntPtr.Zero, values[0].pvData, 0);    // free the memory
            long actual64 = Marshal.ReadInt64(values[1].pvData);
            allocator(IntPtr.Zero, values[1].pvData, 0);    // free the memory
            Assert.AreEqual(Expected16, actual16);
            Assert.AreEqual(Expected64, actual64);
        }

        /// <summary>
        /// Enumerate specific columns using a JET_ENUMCOLUMNID array.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestEnumerateSpecificColumnsAndTags()
        {
            const short Expected16 = 2;
            const int Expected32 = 100;
            const long Expected64 = 9999;

            this.CreateNewRecord();
            this.SetColumn(this.columnidDict["int16"], BitConverter.GetBytes(Int16.MinValue), 0);   // itag 1
            this.SetColumn(this.columnidDict["int16"], BitConverter.GetBytes(Expected16), 0);       // itag 2
            this.SetColumn(this.columnidDict["int16"], BitConverter.GetBytes(Int16.MaxValue), 0);   // itag 3

            this.SetColumn(this.columnidDict["int32"], BitConverter.GetBytes(Expected32), 0);

            this.SetColumn(this.columnidDict["int64"], BitConverter.GetBytes(Int64.MinValue), 0);   // itag 1
            this.SetColumn(this.columnidDict["int64"], BitConverter.GetBytes(Int64.MaxValue), 0);   // itag 2
            this.SetColumn(this.columnidDict["int64"], BitConverter.GetBytes(Expected64), 0);       // itag 3

            this.SetColumn(this.columnidDict["unicode"], Encoding.Unicode.GetBytes("test"), 0);
            this.SetColumn(this.columnidDict["ascii"], Encoding.ASCII.GetBytes("test"), 0);

            var columnids = new[]
            {
                new JET_ENUMCOLUMNID { columnid = this.columnidDict["int16"], ctagSequence = 1, rgtagSequence = new [] { 2 }},
                new JET_ENUMCOLUMNID { columnid = this.columnidDict["int64"], ctagSequence = 1, rgtagSequence = new [] { 3 } },
            };

            int numValues;
            JET_ENUMCOLUMN[] values;
            JET_PFNREALLOC allocator = (context, pv, cb) => IntPtr.Zero == pv ? Marshal.AllocHGlobal(new IntPtr(cb)) : Marshal.ReAllocHGlobal(pv, new IntPtr(cb));
            Api.JetEnumerateColumns(
                this.sesid,
                this.tableid,
                columnids.Length,
                columnids,
                out numValues,
                out values,
                allocator,
                IntPtr.Zero,
                0,
                EnumerateColumnsGrbit.None);

            Assert.AreEqual(2, numValues);
            Assert.IsNotNull(values);
            Assert.AreEqual(this.columnidDict["int16"], values[0].columnid);
            Assert.AreEqual(this.columnidDict["int64"], values[1].columnid);
            Assert.AreEqual(JET_wrn.Success, values[0].err);
            Assert.AreEqual(JET_wrn.Success, values[1].err);
            Assert.AreEqual(1, values[0].cEnumColumnValue);
            Assert.AreEqual(1, values[1].cEnumColumnValue);
            Assert.AreEqual(2, values[0].rgEnumColumnValue[0].itagSequence);
            Assert.AreEqual(3, values[1].rgEnumColumnValue[0].itagSequence);

            short actual16 = Marshal.ReadInt16(values[0].rgEnumColumnValue[0].pvData);
            allocator(IntPtr.Zero, values[0].rgEnumColumnValue[0].pvData, 0);    // free the memory
            long actual64 = Marshal.ReadInt64(values[1].rgEnumColumnValue[0].pvData);
            allocator(IntPtr.Zero, values[1].rgEnumColumnValue[0].pvData, 0);    // free the memory
            Assert.AreEqual(Expected16, actual16);
            Assert.AreEqual(Expected64, actual64);
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Creates a new record. The tableid is positioned on the new record.
        /// </summary>
        private void CreateNewRecord()
        {
            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
        }

        /// <summary>
        /// Updates a record setting the given column set to the specified value.
        /// </summary>
        /// <param name="columnid">The column to set.</param>
        /// <param name="data">The data to set.</param>
        /// <param name="itagSequence">The itag sequence to set.</param>
        private void SetColumn(JET_COLUMNID columnid, byte[] data, int itagSequence)
        {
            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Replace);
            var setinfo = new JET_SETINFO
                {
                ibLongValue = 0,
                itagSequence = itagSequence,
                };
            Api.JetSetColumn(this.sesid, this.tableid, columnid, data, (null == data) ? 0 : data.Length, SetColumnGrbit.None, setinfo);
            Api.JetUpdate(this.sesid, this.tableid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
        }

        /// <summary>
        /// Update the cursor and goto the returned bookmark.
        /// </summary>
        private void UpdateAndGotoBookmark()
        {
            var bookmark = new byte[SystemParameters.BookmarkMost];
            int bookmarkSize;
            Api.JetUpdate(this.sesid, this.tableid, bookmark, bookmark.Length, out bookmarkSize);
            Api.JetGotoBookmark(this.sesid, this.tableid, bookmark, bookmarkSize);
        }

        #endregion Helper methods
    }
}
