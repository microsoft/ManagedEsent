//-----------------------------------------------------------------------
// <copyright file="FilteredDMLCurrencyTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using Microsoft.Isam.Esent;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Windows8;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// DML and currency tests for filtered moves.
    /// </summary>
    [TestClass]
    [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules",
        "SA1305:FieldNamesMustNotUseHungarianNotation",
        Justification = "Reviewed. Suppression is OK here.")]
    public class FilteredDmlCurrencyTests
    {
        /// <summary>
        /// Table name to create.
        /// </summary>
        private readonly string tableName = "table";

        /// <summary>
        /// Clustered index name to create.
        /// </summary>
        private readonly string clustIndexName = "clustered";

        /// <summary>
        /// Secondary index name to create (contains primary key column).
        /// </summary>
        private readonly string secIndexWithPrimaryName = "secondarywithprimary";

        /// <summary>
        /// The directory being used for the database and its files.
        /// </summary>
        private string directory;

        /// <summary>
        /// The path to the database being used by the test.
        /// </summary>
        private string database;

        /// <summary>
        /// The instance used by the test.
        /// </summary>
        private JET_INSTANCE instance;

        /// <summary>
        /// The session used by the test.
        /// </summary>
        private JET_SESID sesId;

        /// <summary>
        /// Identifies the database used by the test.
        /// </summary>
        private JET_DBID dbId;

        /// <summary>
        /// Column ID of the column that makes up the primary index.
        /// </summary>
        private JET_COLUMNID columnIdKey;

        /// <summary>
        /// Column ID of the column that makes up the data portion of the record.
        /// </summary>
        private JET_COLUMNID columnIdData1;

        /// <summary>
        /// Column ID of the column that makes up the second part of the data portion of the record.
        /// </summary>
        private JET_COLUMNID columnIdData2;

        #region Setup/Teardown

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// </summary>
        [TestInitialize]
        [Description("Setup for each test in DmlCurrencyTests")]
        public void TestSetup()
        {
            this.directory = SetupHelper.CreateRandomDirectory();
            this.database = Path.Combine(this.directory, "database.edb");
            this.instance = SetupHelper.CreateNewInstance(this.directory);

            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesId, string.Empty, string.Empty);

            JET_DBID dbId;
            Api.JetCreateDatabase(this.sesId, this.database, string.Empty, out dbId, CreateDatabaseGrbit.None);
            Api.JetCloseDatabase(this.sesId, dbId, CloseDatabaseGrbit.None);

            Api.JetOpenDatabase(this.sesId, this.database, null, out this.dbId, OpenDatabaseGrbit.None);
            Api.JetBeginTransaction(this.sesId);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        [Description("Cleanup for each test in DmlCurrencyTests")]
        public void TestTeardown()
        {
            while (true)
            {
                try
                {
                    Api.JetCommitTransaction(this.sesId, CommitTransactionGrbit.LazyFlush);
                }
                catch (EsentNotInTransactionException)
                {
                    break;
                }
            }

            Api.JetCloseDatabase(this.sesId, this.dbId, CloseDatabaseGrbit.None);

            Api.JetEndSession(this.sesId, EndSessionGrbit.None);
            Api.JetTerm(this.instance);
            Cleanup.DeleteDirectoryWithRetry(this.directory);
        }

        #endregion Setup/Teardown

        #region DML and currency Tests

        /// <summary>
        /// Navigation: move-first, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: move-first, clustered index")]
        public void NavigationMoveFirstClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            this.InsertRecord(tc.tableid, 2);
            this.InsertRecord(tc.tableid, 1);
            this.InsertRecord(tc.tableid, 3);

            JET_INDEX_COLUMN filter1 = this.CreateFilter(this.columnIdKey, 2, JetRelop.Equals);
            JET_INDEX_COLUMN[] filters = { filter1 };

            this.MoveCursor(tc.tableid, JET_Move.First, filters);

            this.VerifyCurrentRecord(tc.tableid, 2);
        }

        /// <summary>
        /// Navigation: clearing filter
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: clearing filter")]
        public void NavigationClearFilter()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            this.InsertRecord(tc.tableid, 1);
            this.InsertRecord(tc.tableid, 2);
            this.InsertRecord(tc.tableid, 3);
            this.InsertRecord(tc.tableid, 5);
            this.InsertRecord(tc.tableid, 6);

            JET_INDEX_COLUMN filter = this.CreateFilter(this.columnIdKey, 1, JetRelop.BitmaskEqualsZero);
            JET_INDEX_COLUMN[] filters = { filter };

            this.MoveCursor(tc.tableid, JET_Move.First, filters);
            this.VerifyCurrentRecord(tc.tableid, 2);
            this.MoveCursor(tc.tableid, JET_Move.Next, null);
            this.VerifyCurrentRecord(tc.tableid, 3);
            this.MoveCursor(tc.tableid, JET_Move.Next, filters);
            this.VerifyCurrentRecord(tc.tableid, 6);
        }

        /// <summary>
        /// Navigation: move-last, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: move-last, clustered index")]
        public void NavigationMoveLastClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            this.InsertRecord(tc.tableid, 2);
            this.InsertRecord(tc.tableid, 3);
            this.InsertRecord(tc.tableid, 1);

            JET_INDEX_COLUMN filter1 = this.CreateFilter(this.columnIdKey, 2, JetRelop.LessThan);
            JET_INDEX_COLUMN[] filters = { filter1 };

            this.MoveCursor(tc.tableid, JET_Move.Last, filters);

            this.VerifyCurrentRecord(tc.tableid, 1);
        }

        /// <summary>
        /// Navigation: move-next, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: move-next, clustered index")]
        public void NavigationMoveNextClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            this.InsertRecord(tc.tableid, 2);
            this.InsertRecord(tc.tableid, 1);
            this.InsertRecord(tc.tableid, 3);
            this.MoveCursor(tc.tableid, JET_Move.First, null);

            JET_INDEX_COLUMN filter1 = this.CreateFilter(this.columnIdKey, 2, JetRelop.GreaterThan);
            JET_INDEX_COLUMN[] filters = { filter1 };

            this.MoveCursor(tc.tableid, JET_Move.Next, filters);

            this.VerifyCurrentRecord(tc.tableid, 3);
        }

        /// <summary>
        /// Navigation: move-next, check byte order
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: move-next, check byte order")]
        public void NavigationMoveNextByteOrder()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            this.InsertRecord(tc.tableid, 1, 10);
            this.InsertRecord(tc.tableid, 2, 260);
            this.InsertRecord(tc.tableid, 3, 20);

            JET_INDEX_COLUMN filter1 = this.CreateFilter(this.columnIdData1, 15, JetRelop.LessThan);
            JET_INDEX_COLUMN[] filters = { filter1 };

            this.MoveCursor(tc.tableid, JET_Move.First, filters);
            this.VerifyCurrentRecord(tc.tableid, 1);
            this.MoveCursor(tc.tableid, JET_Move.Next, filters, typeof(EsentNoCurrentRecordException));
        }

        /// <summary>
        /// Navigation: move-previous, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: move-previous, clustered index")]
        public void NavigationMovePreviousClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            this.InsertRecord(tc.tableid, 2);
            this.InsertRecord(tc.tableid, 3);
            this.InsertRecord(tc.tableid, 1);
            this.MoveCursor(tc.tableid, JET_Move.Last, null);

            JET_INDEX_COLUMN filter1 = this.CreateFilter(this.columnIdKey, 1, JetRelop.LessThanOrEqual);
            JET_INDEX_COLUMN[] filters = { filter1 };

            this.MoveCursor(tc.tableid, JET_Move.Previous, filters);

            this.VerifyCurrentRecord(tc.tableid, 1);
        }

        /// <summary>
        /// Navigation: multiple filters
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: multiple filters")]
        public void NavigationMultipleFilters()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            this.InsertRecord(tc.tableid, 1, 20);
            this.InsertRecord(tc.tableid, 2, 30);
            this.InsertRecord(tc.tableid, 3, 30);

            JET_INDEX_COLUMN filter1 = this.CreateFilter(this.columnIdKey, 2, JetRelop.NotEquals);
            JET_INDEX_COLUMN filter2 = this.CreateFilter(this.columnIdData1, 30, JetRelop.GreaterThanOrEqual);
            JET_INDEX_COLUMN[] filters = { filter1, filter2 };

            this.MoveCursor(tc.tableid, JET_Move.First, filters);

            this.VerifyCurrentRecord(tc.tableid, 3);
        }

        /// <summary>
        /// Navigation: bitmask operators
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: bitmask operators")]
        public void NavigationBitmask()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            this.InsertRecord(tc.tableid, 1, 15);
            this.InsertRecord(tc.tableid, 3, 11);
            this.InsertRecord(tc.tableid, 5, 10);
            this.InsertRecord(tc.tableid, 9, 8);

            JET_INDEX_COLUMN filter1 = this.CreateFilter(this.columnIdKey, 8, JetRelop.BitmaskEqualsZero);
            JET_INDEX_COLUMN filter2 = this.CreateFilter(this.columnIdData1, 1, JetRelop.BitmaskNotEqualsZero);
            JET_INDEX_COLUMN[] filters = { filter1, filter2 };

            this.MoveCursor(tc.tableid, JET_Move.Last, filters);

            this.VerifyCurrentRecord(tc.tableid, 3, 11);
        }

        /// <summary>
        /// Navigation: move-first, secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: filtered move not supported with secondary index")]
        public void NavigationMoveSecondaryNotSupported()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            this.InsertRecord(tc.tableid, 1, 10);
            this.InsertRecord(tc.tableid, 2, 10);
            this.InsertRecord(tc.tableid, -3, 30);
            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithPrimaryName);
            JET_INDEX_COLUMN filter = this.CreateFilter(this.columnIdKey, 2, JetRelop.Equals);
            JET_INDEX_COLUMN[] filters = { filter };

            this.MoveCursor(tc.tableid, JET_Move.First, filters, typeof(EsentFilteredMoveNotSupportedException));
        }

        /// <summary>
        /// Navigation: seek with filtering
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: seek with filtering")]
        public void NavigationSeekWithFiltering()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            this.InsertRecord(tc.tableid, 1, 300);
            this.InsertRecord(tc.tableid, 2, 100);
            this.InsertRecord(tc.tableid, 3, 200);

            JET_INDEX_COLUMN filter = this.CreateFilter(this.columnIdData1, 250, JetRelop.LessThanOrEqual);
            JET_INDEX_COLUMN[] filters = { filter };

            Windows8Api.JetSetCursorFilter(this.sesId, tc.tableid, filters, CursorFilterGrbit.None);
            this.SeekToRecordClustered(tc.tableid, 1, SeekGrbit.SeekGE);
            this.VerifyCurrentRecord(tc.tableid, 2, 100);
        }

        /// <summary>
        /// JetMove with MoveKeyNE: clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("JetMove with MoveKeyNE: clustered index")]
        public void JetMoveWithMoveKeyNeClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            this.InsertRecord(tc.tableid, 1);
            this.InsertRecord(tc.tableid, 2);
            this.InsertRecord(tc.tableid, 3);
            this.InsertRecord(tc.tableid, 4);

            this.VerifyCurrentRecord(tc.tableid, 1);

            JET_INDEX_COLUMN filter = this.CreateFilter(this.columnIdKey, 2, JetRelop.NotEquals);
            JET_INDEX_COLUMN[] filters = { filter };

            Windows8Api.JetSetCursorFilter(this.sesId, tc.tableid, filters, CursorFilterGrbit.None);
            Api.JetMove(this.sesId, tc.tableid, JET_Move.Next, MoveGrbit.MoveKeyNE);
            this.VerifyCurrentRecord(tc.tableid, 3);

            filter = this.CreateFilter(this.columnIdKey, 4, JetRelop.Equals);
            filters = new JET_INDEX_COLUMN[] { filter };

            Windows8Api.JetSetCursorFilter(this.sesId, tc.tableid, filters, CursorFilterGrbit.None);
            Api.JetMove(this.sesId, tc.tableid, JET_Move.Next, MoveGrbit.MoveKeyNE);
            this.VerifyCurrentRecord(tc.tableid, 4);

            Windows8Api.JetSetCursorFilter(this.sesId, tc.tableid, null, CursorFilterGrbit.None);
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    Api.JetMove(this.sesId, tc.tableid, JET_Move.Next, MoveGrbit.MoveKeyNE);
                    Assert.Fail("Should have thrown EsentNoCurrentRecordException");
                }
                catch (EsentNoCurrentRecordException)
                {
                }

                this.VerifyCurrentRecord(tc.tableid, 4, typeof(EsentNoCurrentRecordException));
            }

            filter = this.CreateFilter(this.columnIdKey, 2, JetRelop.Equals);
            filters = new JET_INDEX_COLUMN[] { filter };

            Windows8Api.JetSetCursorFilter(this.sesId, tc.tableid, filters, CursorFilterGrbit.None);
            Api.JetMove(this.sesId, tc.tableid, JET_Move.Previous, MoveGrbit.MoveKeyNE);
            this.VerifyCurrentRecord(tc.tableid, 2);

            Windows8Api.JetSetCursorFilter(this.sesId, tc.tableid, null, CursorFilterGrbit.None);
            Api.JetMove(this.sesId, tc.tableid, JET_Move.Previous, MoveGrbit.MoveKeyNE);
            this.VerifyCurrentRecord(tc.tableid, 1);

            for (int i = 0; i < 2; i++)
            {
                try
                {
                    Api.JetMove(this.sesId, tc.tableid, JET_Move.Previous, MoveGrbit.MoveKeyNE);
                    Assert.Fail("Should have thrown EsentNoCurrentRecordException");
                }
                catch (EsentNoCurrentRecordException)
                {
                }

                this.VerifyCurrentRecord(tc.tableid, 1, typeof(EsentNoCurrentRecordException));
            }

            Windows8Api.JetSetCursorFilter(this.sesId, tc.tableid, filters, CursorFilterGrbit.None);
            Api.JetMove(this.sesId, tc.tableid, JET_Move.Next, MoveGrbit.MoveKeyNE);
            this.VerifyCurrentRecord(tc.tableid, 2);
        }

        #endregion DML and currency Tests

        #region Helpers

        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>The newly created table.</returns>
        private JET_TABLECREATE CreateTable(string tableName)
        {
            const string ColumnKeyName = "columnkey";
            const string ColumnData1Name = "columndata1";
            const string ColumnData2Name = "columndata2";
            string clustIndexKey = string.Format("+{0}\0\0", ColumnKeyName);
            string secIndexWithPrimaryKey = string.Format("+{0}\0-{1}\0\0", ColumnData1Name, ColumnKeyName);

            JET_COLUMNCREATE[] columnCreates =
            {
                new JET_COLUMNCREATE
                {
                    szColumnName = ColumnKeyName,
                    coltyp = JET_coltyp.Long
                },

                new JET_COLUMNCREATE
                {
                    szColumnName = ColumnData1Name,
                    coltyp = JET_coltyp.Long
                },

                new JET_COLUMNCREATE
                {
                    szColumnName = ColumnData2Name,
                    coltyp = JET_coltyp.Long
                }
            };

            JET_INDEXCREATE[] indexCreates = 
            {
                new JET_INDEXCREATE
                {
                    szIndexName = this.clustIndexName,
                    szKey = clustIndexKey,
                    cbKey = clustIndexKey.Length,
                    grbit = CreateIndexGrbit.IndexPrimary,
                },

                new JET_INDEXCREATE
                {
                    szIndexName = this.secIndexWithPrimaryName,
                    szKey = secIndexWithPrimaryKey,
                    cbKey = secIndexWithPrimaryKey.Length,
                    grbit = CreateIndexGrbit.None
                }
            };

            JET_TABLECREATE tc = new JET_TABLECREATE
            {
                szTableName = tableName,
                rgcolumncreate = columnCreates,
                cColumns = columnCreates.Length,
                rgindexcreate = indexCreates,
                cIndexes = indexCreates.Length,
                grbit = CreateTableColumnIndexGrbit.None
            };

            Api.JetCreateTableColumnIndex3(this.sesId, this.dbId, tc);

            Assert.AreEqual<int>(6, tc.cCreated);  // 1 table + 3 colummns + 2 indexes.
            Assert.AreNotEqual<JET_TABLEID>(JET_TABLEID.Nil, tc.tableid);

            this.columnIdKey = Api.GetTableColumnid(this.sesId, tc.tableid, ColumnKeyName);
            this.columnIdData1 = Api.GetTableColumnid(this.sesId, tc.tableid, ColumnData1Name);
            this.columnIdData2 = Api.GetTableColumnid(this.sesId, tc.tableid, ColumnData2Name);

            Assert.AreNotEqual<JET_COLUMNID>(JET_COLUMNID.Nil, this.columnIdKey);
            Assert.AreNotEqual<JET_COLUMNID>(JET_COLUMNID.Nil, this.columnIdData1);
            Assert.AreNotEqual<JET_COLUMNID>(JET_COLUMNID.Nil, this.columnIdData1);
            Assert.AreNotEqual<JET_COLUMNID>(this.columnIdKey, this.columnIdData1);
            Assert.AreNotEqual<JET_COLUMNID>(this.columnIdKey, this.columnIdData2);

            return tc;
        }

        /// <summary>
        /// Gets the data1 default.
        /// </summary>
        /// <param name="key">The key to retrieve.</param>
        /// <returns>The default data.</returns>
        private int GetData1Default(int key)
        {
            return 10 * key;
        }

        /// <summary>
        /// Gets the data2 default.
        /// </summary>
        /// <param name="data1">The data1.</param>
        /// <returns>The default data.</returns>
        private int GetData2Default(int data1)
        {
            return 10 * data1;
        }

        /// <summary>
        /// Inserts the record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="key">The key to insert.</param>
        /// <param name="data1">The data1.</param>
        /// <param name="data2">The data2.</param>
        /// <returns>The bookmark.</returns>
        private byte[] InsertRecord(JET_TABLEID tableId, int key, int data1, int data2)
        {
            byte[] keyArray = BitConverter.GetBytes(key);
            byte[] data1Array = BitConverter.GetBytes(data1);
            byte[] data2Array = BitConverter.GetBytes(data2);
            byte[] bookmark = new byte[sizeof(int) + 1]; // +1 for ascending/descending info.

            Api.JetPrepareUpdate(this.sesId, tableId, JET_prep.Insert);
            Api.JetSetColumn(this.sesId, tableId, this.columnIdKey, keyArray, keyArray.Length, SetColumnGrbit.None, null);
            Api.JetSetColumn(this.sesId, tableId, this.columnIdData1, data1Array, data1Array.Length, SetColumnGrbit.None, null);
            Api.JetSetColumn(this.sesId, tableId, this.columnIdData2, data2Array, data2Array.Length, SetColumnGrbit.None, null);

            int actualBookmarkSize;
            Api.JetUpdate(this.sesId, tableId, bookmark, bookmark.Length, out actualBookmarkSize);
            Assert.AreEqual<int>(bookmark.Length, actualBookmarkSize);

            return bookmark;
        }

        /// <summary>
        /// Inserts the record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="key">The key to insert.</param>
        /// <param name="data1">The data1.</param>
        /// <returns>The bookmark.</returns>
        private byte[] InsertRecord(JET_TABLEID tableId, int key, int data1)
        {
            int data2 = this.GetData2Default(data1);

            return this.InsertRecord(tableId, key, data1, data2);
        }

        /// <summary>
        /// Inserts the record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="key">The key to insert.</param>
        /// <returns>The bookmark of the record.</returns>
        private byte[] InsertRecord(JET_TABLEID tableId, int key)
        {
            int data1 = this.GetData1Default(key);
            int data2 = this.GetData2Default(data1);

            return this.InsertRecord(tableId, key, data1, data2);
        }

        /// <summary>
        /// Create a filter.
        /// </summary>
        /// <param name="columnid">The column id.</param>
        /// <param name="value">The value.</param>
        /// <param name="relop">The operator.</param>
        /// <returns>The filter.</returns>
        private JET_INDEX_COLUMN CreateFilter(JET_COLUMNID columnid, int value, JetRelop relop)
        {
            return new JET_INDEX_COLUMN()
            {
                columnid = columnid,
                relop = relop,
                pvData = BitConverter.GetBytes(value)
            };
        }

        /// <summary>
        /// Moves the cursor.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="filters">Filters to apply to the move.</param>
        /// <param name="exTypeExpected">The ex type expected.</param>
        private void MoveCursor(JET_TABLEID tableId, JET_Move offset, JET_INDEX_COLUMN[] filters, Type exTypeExpected)
        {
            try
            {
                Windows8Api.JetSetCursorFilter(this.sesId, tableId, filters, CursorFilterGrbit.None);
                Api.JetMove(this.sesId, tableId, offset, MoveGrbit.None);
                if (exTypeExpected != null)
                {
                    Assert.Fail("Should have thrown {0}", exTypeExpected);
                }
            }
            catch (EsentException ex)
            {
                if (exTypeExpected != null)
                {
                    Assert.AreEqual<Type>(exTypeExpected, ex.GetType());
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Moves the cursor.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="filters">Filters to apply to the move.</param>
        private void MoveCursor(JET_TABLEID tableId, JET_Move offset, JET_INDEX_COLUMN[] filters)
        {
            this.MoveCursor(tableId, offset, filters, null);
        }

        /// <summary>
        /// Verifies the current record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="keyExpected">The key expected.</param>
        /// <param name="data1Expected">The data1 expected.</param>
        /// <param name="data2Expected">The data2 expected.</param>
        /// <param name="exTypeExpected">The ex type expected.</param>
        private void VerifyCurrentRecord(JET_TABLEID tableId, int keyExpected, int data1Expected, int data2Expected, Type exTypeExpected)
        {
            byte[] keyArray = new byte[sizeof(int)];
            byte[] data1Array = new byte[sizeof(int)];
            byte[] data2Array = new byte[sizeof(int)];

            JET_RETRIEVECOLUMN[] retrieveColumns = new[]
            {
                new JET_RETRIEVECOLUMN
                {
                    columnid = this.columnIdKey,
                    pvData = keyArray,
                    cbData = keyArray.Length,
                    itagSequence = 1
                },

                new JET_RETRIEVECOLUMN
                {
                    columnid = this.columnIdData1,
                    pvData = data1Array,
                    cbData = data1Array.Length,
                    itagSequence = 1
                },

                new JET_RETRIEVECOLUMN
                {
                    columnid = this.columnIdData2,
                    pvData = data2Array,
                    cbData = data2Array.Length,
                    itagSequence = 1
                }
            };

            try
            {
                JET_wrn err = Api.JetRetrieveColumns(this.sesId, tableId, retrieveColumns, retrieveColumns.Length);

                if (exTypeExpected != null)
                {
                    Assert.Fail("Should have thrown {0}", exTypeExpected);
                }

                Assert.AreEqual<JET_wrn>(JET_wrn.Success, err);
                Assert.AreEqual<int>(keyArray.Length, retrieveColumns[0].cbActual);
                Assert.AreEqual<int>(data1Array.Length, retrieveColumns[1].cbActual);
                Assert.AreEqual<int>(data1Array.Length, retrieveColumns[2].cbActual);

                int keyActual = BitConverter.ToInt32(keyArray, 0);
                int data1Actual = BitConverter.ToInt32(data1Array, 0);
                int data2Actual = BitConverter.ToInt32(data2Array, 0);

                Assert.AreEqual<int>(keyExpected, keyActual);
                Assert.AreEqual<int>(data1Expected, data1Actual);
                Assert.AreEqual<int>(data2Expected, data2Actual);
            }
            catch (EsentException ex)
            {
                if (exTypeExpected != null)
                {
                    Assert.AreEqual<Type>(exTypeExpected, ex.GetType());
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Verifies the current record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="keyExpected">The key expected.</param>
        /// <param name="data1Expected">The data1 expected.</param>
        /// <param name="data2Expected">The data2 expected.</param>
        private void VerifyCurrentRecord(JET_TABLEID tableId, int keyExpected, int data1Expected, int data2Expected)
        {
            this.VerifyCurrentRecord(tableId, keyExpected, data1Expected, data2Expected, null);
        }

        /// <summary>
        /// Verifies the current record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="keyExpected">The key expected.</param>
        /// <param name="data1Expected">The data1 expected.</param>
        /// <param name="exTypeExpected">The ex type expected.</param>
        private void VerifyCurrentRecord(JET_TABLEID tableId, int keyExpected, int data1Expected, Type exTypeExpected)
        {
            int data2Expected = this.GetData2Default(data1Expected);

            this.VerifyCurrentRecord(tableId, keyExpected, data1Expected, data2Expected, exTypeExpected);
        }

        /// <summary>
        /// Verifies the current record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="keyExpected">The key expected.</param>
        /// <param name="data1Expected">The data1 expected.</param>
        private void VerifyCurrentRecord(JET_TABLEID tableId, int keyExpected, int data1Expected)
        {
            this.VerifyCurrentRecord(tableId, keyExpected, data1Expected, null);
        }

        /// <summary>
        /// Verifies the current record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="keyExpected">The key expected.</param>
        /// <param name="exTypeExpected">The ex type expected.</param>
        private void VerifyCurrentRecord(JET_TABLEID tableId, int keyExpected, Type exTypeExpected)
        {
            int data1Expected = this.GetData2Default(keyExpected);
            int data2Expected = this.GetData2Default(data1Expected);

            this.VerifyCurrentRecord(tableId, keyExpected, data1Expected, data2Expected, exTypeExpected);
        }

        /// <summary>
        /// Verifies the current record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="keyExpected">The key expected.</param>
        private void VerifyCurrentRecord(JET_TABLEID tableId, int keyExpected)
        {
            this.VerifyCurrentRecord(tableId, keyExpected, null);
        }

        /// <summary>
        /// Makes the key clustered.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="key">The key to make.</param>
        private void MakeKeyClustered(JET_TABLEID tableId, int key)
        {
            byte[] keyArray = BitConverter.GetBytes(key);
            Api.JetMakeKey(this.sesId, tableId, keyArray, keyArray.Length, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Seeks to record clustered.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="key">The key to seek.</param>
        /// <param name="seekGrbit">The seek grbit.</param>
        private void SeekToRecordClustered(JET_TABLEID tableId, int key, SeekGrbit seekGrbit)
        {
            this.MakeKeyClustered(tableId, key);
            Api.JetSeek(this.sesId, tableId, seekGrbit);
        }

        #endregion Helpers
    }
}