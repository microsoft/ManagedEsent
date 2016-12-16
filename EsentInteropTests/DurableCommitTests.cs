//-----------------------------------------------------------------------
// <copyright file="DurableCommitTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using Microsoft.Isam.Esent;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Windows8;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Log flush callback tests.
    /// </summary>
    [TestClass]
    [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules",
        "SA1305:FieldNamesMustNotUseHungarianNotation",
        Justification = "Reviewed. Suppression is OK here.")]
    public class DurableCommitTests
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

        /// <summary>
        /// Log flush callback
        /// </summary>
        private DurableCommitCallback callback;

        /// <summary>
        /// Last commit-id reported by callback
        /// </summary>
        private JET_COMMIT_ID lastCommitIdFlushed;

        /// <summary>
        /// Time of the last callback
        /// </summary>
        private DateTime lastCallbackTime;

        #region Setup/Teardown

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// </summary>
        [TestInitialize]
        [Description("Setup for each test in DurableCommitTests")]
        public void TestSetup()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            this.directory = SetupHelper.CreateRandomDirectory();
            this.database = Path.Combine(this.directory, "database.edb");
            this.instance = SetupHelper.CreateNewInstance(this.directory);

////            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, UnpublishedParam.EnablePeriodicShrinkDatabase, 0, null);
            this.callback = new DurableCommitCallback(this.instance, this.TestCallback);

            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesid, string.Empty, string.Empty);

            Api.JetCreateDatabase(this.sesid, this.database, string.Empty, out this.dbid, CreateDatabaseGrbit.None);
            Api.JetCloseDatabase(this.sesid, this.dbid, CloseDatabaseGrbit.None);

            Api.JetOpenDatabase(this.sesid, this.database, null, out this.dbid, OpenDatabaseGrbit.None);
            Api.JetBeginTransaction(this.sesid);
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            Api.JetCloseTable(this.sesid, tc.tableid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.None);
            Api.JetOpenTable(this.sesid, this.dbid, this.tableName, null, 0, OpenTableGrbit.None, out this.tableid);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        [Description("Cleanup for each test in DurableCommitTests")]
        public void TestTeardown()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            Api.JetCloseTable(this.sesid, this.tableid);
            Api.JetCloseDatabase(this.sesid, this.dbid, CloseDatabaseGrbit.None);
            Api.JetEndSession(this.sesid, EndSessionGrbit.None);
            Api.JetTerm(this.instance);
            this.callback.End();
            this.callback = null;
            Cleanup.DeleteDirectoryWithRetry(this.directory);
        }

        #endregion Setup/Teardown

        #region Log flush Tests

        /// <summary>
        /// Get commit-id from JetCommitTransaction2, should immediately be reported by callback
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Get a commit-id")]
        public void CommitIdFromCommitTransaction2()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            Api.JetBeginTransaction(this.sesid);
            this.InsertRecord(this.tableid, 2);
            this.InsertRecord(this.tableid, 1);
            this.InsertRecord(this.tableid, 3);
            JET_COMMIT_ID commitId;
            Windows8Api.JetCommitTransaction2(this.sesid, CommitTransactionGrbit.None, new TimeSpan(0), out commitId);
            Assert.IsTrue(commitId > null);
            Assert.IsTrue(commitId < this.lastCommitIdFlushed);
        }

        /// <summary>
        /// Get commit-id from Transaction.Commit(), should immediately be reported by callback.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Get commit-id from Transaction.Commit(), should immediately be reported by callback.")]
        public void CommitIdFromCommitTransactionWrapper()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            using (var transaction = new Transaction(this.sesid))
            {
                this.InsertRecord(this.tableid, 2);
                this.InsertRecord(this.tableid, 1);
                this.InsertRecord(this.tableid, 3);
                JET_COMMIT_ID commitId;
                transaction.Commit(CommitTransactionGrbit.None, new TimeSpan(0), out commitId);
                Assert.IsTrue(commitId > null);
                Assert.IsTrue(commitId < this.lastCommitIdFlushed);
            }
        }

        /// <summary>
        /// Lazy commit with duration should get flushed in approximately right amount of time
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Lazy commit with duration")]
        public void LazyCommitWithDuration()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            Api.JetBeginTransaction(this.sesid);
            this.InsertRecord(this.tableid, 2);
            this.InsertRecord(this.tableid, 1);
            this.InsertRecord(this.tableid, 3);
            JET_COMMIT_ID commitId;
            Windows8Api.JetCommitTransaction2(this.sesid, CommitTransactionGrbit.LazyFlush, new TimeSpan(0, 0, 2), out commitId);
            DateTime commitTime = DateTime.Now;
            Assert.IsTrue(commitId >= this.lastCommitIdFlushed);
            EseInteropTestHelper.ThreadSleep(2500);
            Assert.IsTrue(commitId < this.lastCommitIdFlushed);
            TimeSpan timeToFlush = this.lastCallbackTime - commitTime;
            Assert.IsTrue(timeToFlush.TotalMilliseconds < 2500);
        }

        /// <summary>
        /// Lazy commit with smaller duration causes faster flush
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Lazy commit with smaller duration")]
        public void LazyCommitWithSmallerDuration()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            Api.JetBeginTransaction(this.sesid);
            this.InsertRecord(this.tableid, 2);
            this.InsertRecord(this.tableid, 1);
            this.InsertRecord(this.tableid, 3);
            JET_COMMIT_ID commitId1;
            Windows8Api.JetCommitTransaction2(this.sesid, CommitTransactionGrbit.LazyFlush, new TimeSpan(0, 0, 5), out commitId1);
            Api.JetBeginTransaction(this.sesid);
            this.InsertRecord(this.tableid, 4);
            this.InsertRecord(this.tableid, 5);
            JET_COMMIT_ID commitId2;
            Windows8Api.JetCommitTransaction2(this.sesid, CommitTransactionGrbit.LazyFlush, new TimeSpan(0, 0, 2), out commitId2);
            DateTime commitTime = DateTime.Now;
            Assert.IsTrue(commitId2 > commitId1);
            EseInteropTestHelper.ThreadSleep(2500);
            TimeSpan timeToFlush = this.lastCallbackTime - commitTime;
            Assert.IsTrue(commitId2 < this.lastCommitIdFlushed);
            Assert.IsTrue(timeToFlush.TotalMilliseconds < 2500);
        }

        /// <summary>
        /// Transaction.Commit() throws when it's not in a transaction.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Transaction.Commit() thows when it's not in a transaction.")]
        public void LazyTransactionCommitThrowsWhenNotInTransaction()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            using (var transaction = new Transaction(this.sesid))
            {
                JET_COMMIT_ID commitId;
                transaction.Commit(CommitTransactionGrbit.None, new TimeSpan(0), out commitId);
                try
                {
                    transaction.Commit(CommitTransactionGrbit.None, new TimeSpan(0), out commitId);
                    Assert.Fail("Expected exception was not thrown! Expected is InvalidOperationException.");
                }
                catch (InvalidOperationException)
                {
                    // This was the expected exception.
                }
            }
        }

        /// <summary>
        /// Lazy commit followed by read-only transaction still allows WaitLastLevel0Commit to work
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Lazy commit followed by read-only transaction still allows WaitLastLevel0Commit to work")]
        public void LazyCommitFollowedByReadOnlyCanCommitLazy()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            Api.JetBeginTransaction(this.sesid);
            this.InsertRecord(this.tableid, 1);
            JET_COMMIT_ID commitId;
            Windows8Api.JetCommitTransaction2(this.sesid, CommitTransactionGrbit.LazyFlush, new TimeSpan(0, 0, 0), out commitId);

            Api.JetBeginTransaction(this.sesid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.None);

            Assert.IsTrue(commitId >= this.lastCommitIdFlushed);

            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.WaitLastLevel0Commit);

            Assert.IsTrue(commitId < this.lastCommitIdFlushed);
        }

        #endregion Log flush Tests

        #region Helpers

        /// <summary>
        /// Helper method to create a JET_COMMIT_ID using reflection.
        /// </summary>
        /// <param name="random">Random number.</param>
        /// <param name="time">Time to use.</param>
        /// <param name="computer">Computer name.</param>
        /// <param name="commitId">Commit id to use.</param>
        /// <returns>Jet-commit-id created.</returns>
        internal static JET_COMMIT_ID CreateJetCommitId(int random, DateTime? time, string computer, long commitId)
        {
            var sign = new JET_SIGNATURE(random, time, computer);
#if MANAGEDESENT_ON_CORECLR
            // Use a special test-only constructor.
            return new JET_COMMIT_ID(sign, commitId);
#else
            object nativeSign = sign.GetType().GetMethod("GetNativeSignature", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(sign, null);

            string assemblyName = sign.GetType().Assembly.FullName;
            object nativeCommitId = Activator.CreateInstance(assemblyName, "Microsoft.Isam.Esent.Interop.Windows8.NATIVE_COMMIT_ID").Unwrap();
            nativeCommitId.GetType().GetField("signLog").SetValue(nativeCommitId, nativeSign);
            nativeCommitId.GetType().GetField("commitId").SetValue(nativeCommitId, commitId);
            object[] args = { nativeCommitId };
            return (JET_COMMIT_ID)Activator.CreateInstance(assemblyName, "Microsoft.Isam.Esent.Interop.Windows8.JET_COMMIT_ID", false, BindingFlags.Instance | BindingFlags.NonPublic, null, args, null, null).Unwrap();
#endif
        }

        /// <summary>
        /// Test callback method
        /// </summary>
        /// <param name="instance">Current instance.</param>
        /// <param name="commitId">Commit id seen.</param>
        /// <param name="grbit">Grbit - reserved.</param>
        /// <returns>Success or error.</returns>
        private JET_err TestCallback(
            JET_INSTANCE instance,
            JET_COMMIT_ID commitId,
            DurableCommitCallbackGrbit grbit)
        {
            this.lastCommitIdFlushed = commitId;
            this.lastCallbackTime = DateTime.Now;
            return JET_err.Success;
        }

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
            string secIndexWithoutPrimaryKey = string.Format("+{0}\0+{1}\0\0", ColumnData1Name, ColumnData2Name);

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

            Api.JetCreateTableColumnIndex3(this.sesid, this.dbid, tc);

            Assert.AreEqual<int>(5, tc.cCreated);  // 1 table + 3 colummns + 1 index.
            Assert.AreNotEqual<JET_TABLEID>(JET_TABLEID.Nil, tc.tableid);

            this.columnIdKey = Api.GetTableColumnid(this.sesid, tc.tableid, ColumnKeyName);
            this.columnIdData1 = Api.GetTableColumnid(this.sesid, tc.tableid, ColumnData1Name);
            this.columnIdData2 = Api.GetTableColumnid(this.sesid, tc.tableid, ColumnData2Name);

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

            Api.JetPrepareUpdate(this.sesid, tableId, JET_prep.Insert);
            Api.JetSetColumn(this.sesid, tableId, this.columnIdKey, keyArray, keyArray.Length, SetColumnGrbit.None, null);
            Api.JetSetColumn(this.sesid, tableId, this.columnIdData1, data1Array, data1Array.Length, SetColumnGrbit.None, null);
            Api.JetSetColumn(this.sesid, tableId, this.columnIdData2, data2Array, data2Array.Length, SetColumnGrbit.None, null);

            int actualBookmarkSize;
            Api.JetUpdate(this.sesid, tableId, bookmark, bookmark.Length, out actualBookmarkSize);
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
        /// <returns>The bookmark.</returns>
        private byte[] InsertRecord(JET_TABLEID tableId, int key)
        {
            int data1 = this.GetData1Default(key);
            int data2 = this.GetData2Default(data1);

            return this.InsertRecord(tableId, key, data1, data2);
        }

        #endregion Helpers
    }
}
