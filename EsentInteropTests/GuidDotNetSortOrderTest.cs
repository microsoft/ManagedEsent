//-----------------------------------------------------------------------
// <copyright file="GuidDotNetSortOrderTest.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Globalization;
    using System.IO;

    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Vista;
    using Microsoft.Isam.Esent.Interop.Windows8;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test .Net guid sort order
    /// </summary>
    [TestClass]
    public class GuidDotNetSortOrderTest
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

        #region Setup/Teardown

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// </summary>
        [TestInitialize]
        [Description("Guid .Net Sort Order Test")]
        public void TestSetup()
        {
            this.directory = SetupHelper.CreateRandomDirectory();
            this.database = Path.Combine(this.directory, "database.edb");

            SystemParameters.DatabasePageSize = GuidDotNetSortOrderTest.PageSize;
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
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        [Description("Cleanup for test")]
        public void TestTeardown()
        {
            while (true)
            {
                try
                {
                    Api.JetCommitTransaction(this.sesId, CommitTransactionGrbit.None);
                }
                catch (EsentNotInTransactionException)
                {
                    break;
                }
            }

            Api.JetCloseDatabase(this.sesId, this.databaseId, CloseDatabaseGrbit.None);

            Api.JetEndSession(this.sesId, EndSessionGrbit.None);
            Api.JetTerm(this.instance);
            SystemParameters.CacheSizeMin = this.savedCacheSizeMin;
            SystemParameters.CacheSizeMax = this.savedCacheSizeMax;
            Cleanup.DeleteDirectoryWithRetry(this.directory);
        }

        #endregion Guid .Net Sort order tests

        /// <summary>
        /// Test .Net Guid sort order
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test .Net Guid sort order")]
        public void TestDotNetGuidSortOrder()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            // Create table with GUID column and index over GUID column.
            this.CreatePopulateAndTestTable();

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

            Api.JetBeginTransaction(this.sesId);
            EseInteropTestHelper.ConsoleWriteLine("Insert more values in index.");
            for (int i = 0; i < 10000; i++)
            {
                if ((i % 2000) == 0)
                {
                    EseInteropTestHelper.ConsoleWriteLine("Added another 2000 Guids.");
                }

                Api.JetCommitTransaction(this.sesId, CommitTransactionGrbit.None); 
                Api.JetBeginTransaction(this.sesId);
                this.InsertRecord(this.tableId, System.Guid.NewGuid());
            }
      
            Api.JetCommitTransaction(this.sesId, CommitTransactionGrbit.None);
            EseInteropTestHelper.ConsoleWriteLine("Finished inserting more values in index.");
 
            // validate order after having closed the database and restarted
            Guid guidPrev;
            Guid guidCur;
            Api.JetMove(this.sesId, this.tableId, JET_Move.First, MoveGrbit.None);
            int bytesRead;
            byte[] data = new byte[16];
            Api.JetRetrieveColumn(this.sesId, this.tableId, this.columnIdKey1, data, data.Length, out bytesRead, 0, null);
            guidPrev = new System.Guid(data);
            for (int i = 1; i < 10000; i++)
            {
                Api.JetMove(this.sesId, this.tableId, JET_Move.Next, MoveGrbit.None);
                Api.JetRetrieveColumn(this.sesId, this.tableId, this.columnIdKey1, data, data.Length, out bytesRead, 0, null);
                guidCur = new System.Guid(data);
                Assert.IsTrue(guidCur.CompareTo(guidPrev) > 0);
                guidPrev = guidCur;
            }

            EseInteropTestHelper.ConsoleWriteLine("Validated order.");

            // retrieve newly inserted GUID from index and compare values
            // to test denormalization logic
            Guid guidT = System.Guid.NewGuid();
            EseInteropTestHelper.ConsoleWriteLine("Allocate random GUID...");
            EseInteropTestHelper.ConsoleWriteLine(guidT.ToString());
            this.InsertRecord(this.tableId, guidT);
            Api.JetSetCurrentIndex(this.sesId, this.tableId, this.secIndexName);
            EseInteropTestHelper.ConsoleWriteLine("Guid inserted is....");
            EseInteropTestHelper.ConsoleWriteLine("{0}", guidT);
            byte[] keyArray = guidT.ToByteArray();
            Api.JetMakeKey(this.sesId, this.tableId, keyArray, keyArray.Length, MakeKeyGrbit.NewKey);
            Api.JetSeek(this.sesId, this.tableId, SeekGrbit.SeekEQ);

            Api.JetSetCurrentIndex(this.sesId, this.tableId, this.secIndexName);
            keyArray = guidT.ToByteArray();
            Api.JetMakeKey(this.sesId, this.tableId, keyArray, keyArray.Length, MakeKeyGrbit.NewKey);
            Api.JetSeek(this.sesId, this.tableId, SeekGrbit.SeekEQ);
            JET_wrn err = Api.JetRetrieveColumn(this.sesId, this.tableId, this.columnIdKey1, data, data.Length, out bytesRead, RetrieveColumnGrbit.RetrieveFromIndex, null);
            Assert.AreEqual(data.Length, bytesRead);
            EseInteropTestHelper.ConsoleWriteLine("Found random GUID in index...");
            Guid guidTT = new System.Guid(data);
            Assert.AreEqual(guidT, guidTT);
            EseInteropTestHelper.ConsoleWriteLine("Found specific GUID in index");

            // check retrieve from index for denormalization
            // by comparing guid inserted.  They should match.
            Api.JetRetrieveColumn(this.sesId, this.tableId, this.columnIdKey1, data, data.Length, out bytesRead, RetrieveColumnGrbit.RetrieveFromIndex, null);
            guidCur = new System.Guid(data);
            EseInteropTestHelper.ConsoleWriteLine("Retrieved Guid is:");
            EseInteropTestHelper.ConsoleWriteLine(guidCur.ToString());
            Assert.IsTrue(guidCur.CompareTo(guidT) == 0);
            EseInteropTestHelper.ConsoleWriteLine("Retrieve from index matches inserted GUID");

            Api.JetCloseTable(this.sesId, this.tableId);

            this.TestTempTableWithGuidDotNetSortOrder();
        }

        #region Helpers

        /// <summary>
        /// Create table with 4 records per page
        /// </summary>
        private void CreatePopulateAndTestTable()
        {
            Api.JetBeginTransaction(this.sesId);
            EseInteropTestHelper.ConsoleWriteLine("Create and popluate table.");
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            for (int i = 0; i < 10000; i++)
            {
                this.InsertRecord(tc.tableid, System.Guid.NewGuid());
                if ((i % 100) == 0)
                {
                   if ((i % 2000) == 0)
                   {
                       EseInteropTestHelper.ConsoleWriteLine("Added another 2000 Guids.");
                   }

                   Api.JetCommitTransaction(this.sesId, CommitTransactionGrbit.None);
                   Api.JetBeginTransaction(this.sesId);
                }
            }

            EseInteropTestHelper.ConsoleWriteLine("Finished inserting first set of values in index.");

            Guid guidPrev;
            Guid guidCur;
            Api.JetMove(this.sesId, tc.tableid, JET_Move.First, MoveGrbit.None);
            int bytesRead;
            byte[] data = new byte[16];
            Api.JetRetrieveColumn(this.sesId, tc.tableid, this.columnIdKey1, data, data.Length, out bytesRead, 0, null);
            guidPrev = new System.Guid(data);
            for (int i = 1; i < 10000; i++)
            {
                Api.JetMove(this.sesId, tc.tableid, JET_Move.Next, MoveGrbit.None);
                Api.JetRetrieveColumn(this.sesId, tc.tableid, this.columnIdKey1, data, data.Length, out bytesRead, 0, null);
                guidCur = new System.Guid(data);
                Assert.IsTrue(guidCur.CompareTo(guidPrev) > 0);
                guidPrev = guidCur;
            }

            Api.JetCommitTransaction(this.sesId, CommitTransactionGrbit.None);

            EseInteropTestHelper.ConsoleWriteLine("Finished testing .Net Guid sort order on inserted values");
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
            string clustIndexKey = string.Format("+{0}\0\0", ColumnKey1Name);

            JET_COLUMNCREATE[] columnCreates =
            {
                new JET_COLUMNCREATE
                {
                    szColumnName = ColumnKey1Name,
                    coltyp = VistaColtyp.GUID
                },
            };

            JET_SPACEHINTS spaceHint = new JET_SPACEHINTS
            {
                cbInitial = GuidDotNetSortOrderTest.PageSize * 50
            };

            JET_INDEXCREATE[] indexCreates = 
            {
                new JET_INDEXCREATE
                {
                    szIndexName = this.clustIndexName,
                    szKey = clustIndexKey,
                    cbKey = clustIndexKey.Length,
                    grbit = CreateIndexGrbit.IndexPrimary | Windows8Grbits.IndexDotNetGuid,
                    pSpaceHints = spaceHint
                },
                new JET_INDEXCREATE
                {
                    szIndexName = this.secIndexName,
                    szKey = clustIndexKey,
                    cbKey = clustIndexKey.Length,
                    grbit = Windows8Grbits.IndexDotNetGuid,
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
            Assert.AreEqual<int>(4, tc.cCreated);  // 1 table + 1 colummns + 2 indexes.
            Assert.AreNotEqual<JET_TABLEID>(JET_TABLEID.Nil, tc.tableid);
            this.columnIdKey1 = Api.GetTableColumnid(this.sesId, tc.tableid, ColumnKey1Name);
            Assert.AreNotEqual<JET_COLUMNID>(JET_COLUMNID.Nil, this.columnIdKey1);
            return tc;
        }

        /// <summary>
        /// Creates a temp table with GUID column and tests .Net sort order.
        /// </summary>
        private void TestTempTableWithGuidDotNetSortOrder()
        {
            // check temp table logic
            EseInteropTestHelper.ConsoleWriteLine("Create temp table on GUID column.");

            var columns = new[]
            {
                new JET_COLUMNDEF { coltyp = VistaColtyp.GUID, cp = JET_CP.Unicode, grbit = ColumndefGrbit.TTKey },
            };
            var columnids = new JET_COLUMNID[columns.Length];

            var idxunicode = new JET_UNICODEINDEX
            {
                dwMapFlags = Conversions.LCMapFlagsFromCompareOptions(CompareOptions.None),
                szLocaleName = "pt-br",
            };

            var opentemporarytable = new JET_OPENTEMPORARYTABLE
            {
                cbKeyMost = SystemParameters.KeyMost,
                ccolumn = columns.Length,
                grbit = TempTableGrbit.Scrollable | Windows8Grbits.TTDotNetGuid,
                pidxunicode = idxunicode,
                prgcolumndef = columns,
                prgcolumnid = columnids,
            };
            Windows8Api.JetOpenTemporaryTable2(this.sesId, opentemporarytable);
            Guid g = System.Guid.NewGuid();
            EseInteropTestHelper.ConsoleWriteLine("Insert values in temp table.");
            for (int i = 0; i < 10000; i++)
            {
                if ((i % 2000) == 0)
                {
                    EseInteropTestHelper.ConsoleWriteLine("Added another 2000 Guids.");
                }
             
                using (var update = new Update(this.sesId, opentemporarytable.tableid, JET_prep.Insert))
                {
                    Api.SetColumn(this.sesId, opentemporarytable.tableid, columnids[0], g);
                    update.Save();
                }
                
                g = System.Guid.NewGuid();
            }

            EseInteropTestHelper.ConsoleWriteLine("Finished inserting values in temp table.");
 
            // validate order after having closed the database and restarted
            Api.JetMove(this.sesId, opentemporarytable.tableid, JET_Move.First, MoveGrbit.None);
            int bytesRead;
            byte[] data = new byte[16];
            Api.JetRetrieveColumn(this.sesId, opentemporarytable.tableid, columnids[0], data, data.Length, out bytesRead, 0, null);
            Guid guidPrev = new System.Guid(data);
            EseInteropTestHelper.ConsoleWriteLine("Retrieved first value from temp table.");
            Guid guidCur;
            for (int i = 1; i < 10000; i++)
            {
                Api.JetMove(this.sesId, opentemporarytable.tableid, JET_Move.Next, MoveGrbit.None);
                Api.JetRetrieveColumn(this.sesId, opentemporarytable.tableid, columnids[0], data, data.Length, out bytesRead, 0, null);
               
                guidCur = new System.Guid(data);
                Assert.IsTrue(guidCur.CompareTo(guidPrev) > 0);
                guidPrev = guidCur;
            }

            EseInteropTestHelper.ConsoleWriteLine("Validated temp table order.");
        }

        /// <summary>
        /// Inserts the record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="key1">The key1 to insert.</param>
        /// <returns>The bookmark.</returns>
        private byte[] InsertRecord(JET_TABLEID tableId, Guid key1)
        {
            byte[] key1Array = key1.ToByteArray();

            Api.JetPrepareUpdate(this.sesId, tableId, JET_prep.Insert);
            Api.JetSetColumn(this.sesId, tableId, this.columnIdKey1, key1Array, key1Array.Length, SetColumnGrbit.None, null);
            int actualBookmarkSize;
            byte[] bookmark = new byte[17];
            Api.JetUpdate(this.sesId, tableId, bookmark, bookmark.Length, out actualBookmarkSize);
            Assert.AreEqual<int>(bookmark.Length, actualBookmarkSize);
            return bookmark;
        }

        #endregion Helpers
    }
}