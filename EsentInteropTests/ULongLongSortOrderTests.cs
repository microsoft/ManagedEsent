//-----------------------------------------------------------------------
// <copyright file="ULongLongSortOrderTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------
namespace InteropApiTests
{
    using System;
    using System.IO;

    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Windows10;
    using Microsoft.Isam.Esent.Interop.Windows8;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test ULongLong sort order
    /// </summary>
    [TestClass]
    public class ULongLongSortOrderTests
    {
        /// <summary>
        /// Length of data column
        /// </summary>
        private const int PageSize = 4096;

        /// <summary>
        /// Length of data column
        /// </summary>
        private const int DataLength = 750;

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
        private readonly string secIndexName = "secondary";

        /// <summary>
        /// The saved min cache size
        /// </summary>
        private int savedCacheSizeMin;

        /// <summary>
        /// The saved max cache size
        /// </summary>
        private int savedCacheSizeMax;

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
        private JET_DBID databaseId;

        /// <summary>
        /// Identifies the table used by the test.
        /// </summary>
        private JET_TABLEID tableId;

        /// <summary>
        /// Column ID of the column that makes up the primary index.
        /// </summary>
        private JET_COLUMNID columnIdKey1;

        /// <summary>
        /// Random number generator.
        /// </summary>
        private Random rnd;

        /// <summary>
        /// Number of randome values in relationr.
        /// </summary>
        private int cuint64Random = 1000;

        #region Setup/Teardown

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// </summary>
        [TestInitialize]
        [Description("ULongLong Sort Order Test")]
        public void TestSetup()
        {
            this.directory = SetupHelper.CreateRandomDirectory();
            this.database = Path.Combine(this.directory, "database.edb");

            SystemParameters.DatabasePageSize = ULongLongSortOrderTests.PageSize;
            this.savedCacheSizeMin = SystemParameters.CacheSizeMin;
            this.savedCacheSizeMax = SystemParameters.CacheSizeMax;
            SystemParameters.CacheSizeMin = 4096;
            SystemParameters.CacheSizeMax = 4096;

            this.instance = SetupHelper.CreateNewInstance(this.directory);
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesId, string.Empty, string.Empty);

            JET_DBID databaseId;
            Api.JetCreateDatabase(this.sesId, this.database, string.Empty, out databaseId, CreateDatabaseGrbit.None);
            Api.JetCloseDatabase(this.sesId, databaseId, CloseDatabaseGrbit.None);
            Api.JetOpenDatabase(this.sesId, this.database, null, out this.databaseId, OpenDatabaseGrbit.None);

            this.rnd = new Random();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        [Description("Cleanup for test")]
        public void TestTeardown()
        {
            Api.JetCloseDatabase(this.sesId, this.databaseId, CloseDatabaseGrbit.None);

            Api.JetEndSession(this.sesId, EndSessionGrbit.None);
            Api.JetTerm(this.instance);
            SystemParameters.CacheSizeMin = this.savedCacheSizeMin;
            SystemParameters.CacheSizeMax = this.savedCacheSizeMax;
            Cleanup.DeleteDirectoryWithRetry(this.directory);
        }

        #endregion ULongLong Sort order tests

        /// <summary>
        /// Test ULongLong sort order
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test ULongLong sort order")]
        public void ULongLongSortOrder()
        {
            // Create table with ULongLong column and index over ULongLong column.
            if (EsentVersion.SupportsWindows10Features)
            {
                this.CreatePopulateAndTestTable();
            }
            else
            {
                try
                {
                    this.CreatePopulateAndTestTable();
                }
                catch (EsentInvalidColumnTypeException)
                {
                    return;
                }
            }

            Api.JetCloseDatabase(this.sesId, this.databaseId, CloseDatabaseGrbit.None);
            Api.JetDetachDatabase(this.sesId, this.database);

#if !MANAGEDESENT_ON_WSA // Not exposed in MSDK
            EseInteropTestHelper.ConsoleWriteLine("Compact database.");
            Api.JetAttachDatabase(this.sesId, this.database, Windows8Grbits.PurgeCacheOnAttach);
            Api.JetCompact(this.sesId, this.database, Path.Combine(this.directory, "defragged.edb"), null, null, CompactGrbit.None);
            Api.JetDetachDatabase(this.sesId, this.database);

            this.database = Path.Combine(this.directory, "defragged.edb");
            Assert.IsTrue(EseInteropTestHelper.FileExists(this.database));
#endif // !MANAGEDESENT_ON_WSA
            Api.JetAttachDatabase(this.sesId, this.database, Windows8Grbits.PurgeCacheOnAttach);

            Api.JetOpenDatabase(this.sesId, this.database, null, out this.databaseId, OpenDatabaseGrbit.None);
            Api.JetOpenTable(this.sesId, this.databaseId, this.tableName, null, 0, OpenTableGrbit.None, out this.tableId);

            // validate order after having closed the database and restarted
            ulong ulongPrev;
            ulong ulongCur;
            Api.JetMove(this.sesId, this.tableId, JET_Move.First, MoveGrbit.None);
            int bytesRead;
            byte[] data = new byte[8];
            Api.JetRetrieveColumn(this.sesId, this.tableId, this.columnIdKey1, data, data.Length, out bytesRead, 0, null);
            ulongPrev = BitConverter.ToUInt64(data, 0);
            for (int i = 1; i < 3; i++)
            {
                Api.JetMove(this.sesId, this.tableId, JET_Move.Next, MoveGrbit.None);
                Api.JetRetrieveColumn(this.sesId, this.tableId, this.columnIdKey1, data, data.Length, out bytesRead, 0, null);
                ulongCur = BitConverter.ToUInt64(data, 0);
                Assert.IsTrue(ulongCur.CompareTo(ulongPrev) > 0);
                ulongPrev = ulongCur;
            }

            EseInteropTestHelper.ConsoleWriteLine("Validated order.");

            // retrieve newly inserted ULongLong from index and compare values
            // to test denormalization logic
            ulong ulongT = 0;
            byte[] keyArray = BitConverter.GetBytes(ulongT);
            Api.JetSetCurrentIndex(this.sesId, this.tableId, this.secIndexName);
            Api.JetMakeKey(this.sesId, this.tableId, keyArray, keyArray.Length, MakeKeyGrbit.NewKey);
            Api.JetSeek(this.sesId, this.tableId, SeekGrbit.SeekEQ);
            JET_wrn err = Api.JetRetrieveColumn(this.sesId, this.tableId, this.columnIdKey1, data, data.Length, out bytesRead, RetrieveColumnGrbit.RetrieveFromIndex, null);
            Assert.AreEqual(data.Length, bytesRead);
            ulongCur = BitConverter.ToUInt64(data, 0);      
            Assert.IsTrue(ulongCur.CompareTo(ulongT) == 0);

            EseInteropTestHelper.ConsoleWriteLine("Retrieved ulong is:");
            EseInteropTestHelper.ConsoleWriteLine(ulongCur.ToString());
            Assert.IsTrue(ulongCur.CompareTo(ulongT) == 0);
            EseInteropTestHelper.ConsoleWriteLine("Retrieve from index matches inserted ULongLong");

            Api.JetCloseTable(this.sesId, this.tableId);
        }

        #region Helpers

        /// <summary>
        /// Create table with 4 records per page
        /// </summary>
        private void CreatePopulateAndTestTable()
        {
            EseInteropTestHelper.ConsoleWriteLine("Create and popluate table.");
            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            // make sure full range is covered
            this.InsertRecord(tc.tableid, 0);
            this.InsertRecord(tc.tableid, 0x7fffffffffffffff);
            this.InsertRecord(tc.tableid, 0xffffffffffffffff);

            // add many random values
            for (int i = 0; i < this.cuint64Random; i++)
            {
                this.InsertRecord(tc.tableid, this.RandomULong());
            }
           
            EseInteropTestHelper.ConsoleWriteLine("Finished inserting records in table.");

            // validate order
            ulong ulongPrev;
            ulong ulongCur;
            Api.JetMove(this.sesId, tc.tableid, JET_Move.First, MoveGrbit.None);
            int bytesRead;
            byte[] data = new byte[8];
            Api.JetRetrieveColumn(this.sesId, tc.tableid, this.columnIdKey1, data, data.Length, out bytesRead, 0, null);
            ulongPrev = BitConverter.ToUInt64(data, 0);
            for (int i = 1; i < this.cuint64Random + 3; i++)
            {
                Api.JetMove(this.sesId, tc.tableid, JET_Move.Next, MoveGrbit.None);
                Api.JetRetrieveColumn(this.sesId, tc.tableid, this.columnIdKey1, data, data.Length, out bytesRead, 0, null);
                ulongCur = BitConverter.ToUInt64(data, 0);
                EseInteropTestHelper.ConsoleWriteLine("Unsigned long long {0} is larger than {1}.", ulongCur, ulongPrev);
                Assert.IsTrue(ulongCur.CompareTo(ulongPrev) > 0);

                ulongPrev = ulongCur;
            }

            EseInteropTestHelper.ConsoleWriteLine("UInt64 order is correct and same value is retrieved from secondary index key.");
            EseInteropTestHelper.ConsoleWriteLine("Finished testing ULongLong sort order on inserted values");
            Api.JetCloseTable(this.sesId, tc.tableid);
        }

        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>The newly created table.</returns>
        private JET_TABLECREATE CreateTable(string tableName)
        {
            const string ColumnKey1Name = "columnkey1";
            const string ColumnULongLongAutoInc = "columnULongLongAutoInc";
            string clustIndexKey = string.Format("+{0}\0\0", ColumnKey1Name);
            string secIndexWithPrimaryKey = string.Format("+{0}\0\0", ColumnKey1Name);

            JET_COLUMNCREATE[] columnCreates =
            {
                new JET_COLUMNCREATE
                {
                    szColumnName = ColumnKey1Name,
                    coltyp = Windows10Coltyp.UnsignedLongLong
                },
                new JET_COLUMNCREATE
                {
                    szColumnName = ColumnULongLongAutoInc,
                    coltyp = Windows10Coltyp.UnsignedLongLong,
                    grbit = ColumndefGrbit.ColumnAutoincrement
                },
            };

            JET_SPACEHINTS spaceHint = new JET_SPACEHINTS
            {
                cbInitial = ULongLongSortOrderTests.PageSize * 50
            };

            JET_INDEXCREATE[] indexCreates = 
            {
                new JET_INDEXCREATE
                {
                    szIndexName = this.clustIndexName,
                    szKey = clustIndexKey,
                    cbKey = clustIndexKey.Length,
                    grbit = CreateIndexGrbit.IndexPrimary,
                    pSpaceHints = spaceHint
                },
                new JET_INDEXCREATE
                {
                    szIndexName = this.secIndexName,
                    szKey = clustIndexKey,
                    cbKey = clustIndexKey.Length,
                    grbit = 0,
                    pSpaceHints = spaceHint
                }
            };

            JET_TABLECREATE tc = new JET_TABLECREATE
            {
                szTableName = tableName,
                rgcolumncreate = columnCreates,
                cColumns = columnCreates.Length,
                rgindexcreate = indexCreates,
                cIndexes = indexCreates.Length,
                grbit = CreateTableColumnIndexGrbit.None,
                ulPages = 110
            };

            Api.JetCreateTableColumnIndex3(this.sesId, this.databaseId, tc);
            Assert.AreEqual<int>(5, tc.cCreated);  // 1 table + 1 colummns + 2 indexes.
            Assert.AreNotEqual<JET_TABLEID>(JET_TABLEID.Nil, tc.tableid);
            this.columnIdKey1 = Api.GetTableColumnid(this.sesId, tc.tableid, ColumnKey1Name);
            Assert.AreNotEqual<JET_COLUMNID>(JET_COLUMNID.Nil, this.columnIdKey1);
            return tc;
        }

        /// <summary>
        /// Inserts the record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="key1">The key1 to insert.</param>
        /// <returns>The bookmark.</returns>
        private byte[] InsertRecord(JET_TABLEID tableId, ulong key1)
        {
            byte[] key1Array = BitConverter.GetBytes(key1);

            Api.JetPrepareUpdate(this.sesId, tableId, JET_prep.Insert);
            Api.JetSetColumn(this.sesId, tableId, this.columnIdKey1, key1Array, key1Array.Length, SetColumnGrbit.None, null);
            int actualBookmarkSize;
            byte[] bookmark = new byte[9];
            Api.JetUpdate(this.sesId, tableId, bookmark, bookmark.Length, out actualBookmarkSize);
            Assert.AreEqual<int>(bookmark.Length, actualBookmarkSize);
            return bookmark;
        }

        /// <summary>
        /// Creates a random ulong.
        /// </summary>
        /// <returns>Random ulong.</returns>
        private ulong RandomULong()
        {
            var buffer = new byte[sizeof(ulong)];
            this.rnd.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        #endregion Helpers
    }
}
