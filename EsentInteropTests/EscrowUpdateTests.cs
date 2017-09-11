//-----------------------------------------------------------------------
// <copyright file="EscrowUpdateTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.IO;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Vista;
    using Microsoft.Isam.Esent.Interop.Windows10;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for JetEscrowUpdate
    /// </summary>
    [TestClass]
    public class EscrowUpdateTests
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
        /// Columnid of the long escrow update column in the table.
        /// </summary>
        private JET_COLUMNID columnidLong;

        /// <summary>
        /// Columnid of the longlong escrow update column in the table.
        /// </summary>
        private JET_COLUMNID columnidLongLong;

        #region Setup/Teardown

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// All DDL should be done in this method.
        /// </summary>
        [TestInitialize]
        [Description("Setup the EscrowUpdateTests fixture")]
        public void Setup()
        {
            this.directory = SetupHelper.CreateRandomDirectory();
            this.database = Path.Combine(this.directory, "database.edb");
            this.table = "table";
            this.instance = SetupHelper.CreateNewInstance(this.directory);

            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesid, string.Empty, string.Empty);
            Api.JetCreateDatabase(this.sesid, this.database, string.Empty, out this.dbid, CreateDatabaseGrbit.None);
            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateTable(this.sesid, this.dbid, this.table, 0, 100, out this.tableid);

            var columndef = new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Long,
                grbit = ColumndefGrbit.ColumnEscrowUpdate,
            };
            Api.JetAddColumn(this.sesid, this.tableid, "EscrowColumnLong", columndef, BitConverter.GetBytes(0), 4, out this.columnidLong);

            columndef.coltyp = VistaColtyp.LongLong;
            Api.JetAddColumn(this.sesid, this.tableid, "EscrowColumnLongLong", columndef, BitConverter.GetBytes(0L), 8, out this.columnidLongLong);

            Api.JetCloseTable(this.sesid, this.tableid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
            Api.JetOpenTable(this.sesid, this.dbid, this.table, null, 0, OpenTableGrbit.None, out this.tableid);
            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.JetUpdate(this.sesid, this.tableid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
            Api.JetMove(this.sesid, this.tableid, JET_Move.First, MoveGrbit.None);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        [Description("Cleanup the EscrowUpdateTests fixture")]
        public void Teardown()
        {
            Api.JetCloseTable(this.sesid, this.tableid);
            Api.JetEndSession(this.sesid, EndSessionGrbit.None);
            Api.JetTerm(this.instance);
            Cleanup.DeleteDirectoryWithRetry(this.directory);
        }

        /// <summary>
        /// Verify that the test class has setup the test fixture properly.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Verify the EscrowUpdateTests fixture was setup correctly")]
        public void VerifyFixtureSetup()
        {
            // Basic setup has been done
            Assert.IsNotNull(this.table);
            Assert.AreNotEqual(JET_INSTANCE.Nil, this.instance);
            Assert.AreNotEqual(JET_SESID.Nil, this.sesid);
            Assert.AreNotEqual(JET_DBID.Nil, this.dbid);
            Assert.AreNotEqual(JET_TABLEID.Nil, this.tableid);
            Assert.AreNotEqual(JET_COLUMNID.Nil, this.columnidLong);

            // The escrow-update column exists
            JET_COLUMNDEF columndef;

            Api.JetGetTableColumnInfo(this.sesid, this.tableid, this.columnidLong, out columndef);
            Assert.AreEqual(JET_coltyp.Long, columndef.coltyp);
            Assert.AreEqual(ColumndefGrbit.ColumnEscrowUpdate, columndef.grbit & ColumndefGrbit.ColumnEscrowUpdate);

            Api.JetGetTableColumnInfo(this.sesid, this.tableid, this.columnidLongLong, out columndef);
            Assert.AreEqual(VistaColtyp.LongLong, columndef.coltyp);
            Assert.AreEqual(ColumndefGrbit.ColumnEscrowUpdate, columndef.grbit & ColumndefGrbit.ColumnEscrowUpdate);

            // We are on a record and the escrow-update column is zeroed
            Assert.AreEqual(0, Api.RetrieveColumnAsInt32(this.sesid, this.tableid, this.columnidLong).Value);
            Assert.AreEqual(0L, Api.RetrieveColumnAsInt64(this.sesid, this.tableid, this.columnidLongLong).Value);
        }

        #endregion Setup/Teardown

        /// <summary>
        /// Verify that JetEscrowUpdate returns the previous column value.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Verify that JetEscrowUpdate returns the previous column value")]
        public void VerifyJetEscrowUpdateReturnsOldValue()
        {
            var previousValue = new byte[8];
            int actual;

            Api.JetBeginTransaction(this.sesid);

            Api.JetEscrowUpdate(this.sesid, this.tableid, this.columnidLong, BitConverter.GetBytes(1), 4, previousValue, 4, out actual, EscrowUpdateGrbit.None);
            Assert.AreEqual(4, actual);
            Assert.AreEqual(0, BitConverter.ToInt32(previousValue, 0));

            Api.JetEscrowUpdate(this.sesid, this.tableid, this.columnidLongLong, BitConverter.GetBytes(1L), 8, previousValue, 8, out actual, EscrowUpdateGrbit.None);
            Assert.AreEqual(8, actual);
            Assert.AreEqual(0L, BitConverter.ToInt64(previousValue, 0));

            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
        }

        /// <summary>
        /// Verify that JetEscrowUpdate updates the column.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Verify that JetEscrowUpdate updates the column")]
        public void VerifyJetEscrowUpdateUpdatesColumn()
        {
            const int Delta = -9;
            const long LongDelta = -(uint.MaxValue + 9L);
            int ignored;

            Api.JetBeginTransaction(this.sesid);

            Api.JetEscrowUpdate(this.sesid, this.tableid, this.columnidLong, BitConverter.GetBytes(Delta), 4, null, 0, out ignored, EscrowUpdateGrbit.None);
            Assert.AreEqual(Api.RetrieveColumnAsInt32(this.sesid, this.tableid, this.columnidLong).Value, Delta);

            Api.JetEscrowUpdate(this.sesid, this.tableid, this.columnidLongLong, BitConverter.GetBytes(LongDelta), 8, null, 0, out ignored, EscrowUpdateGrbit.None);
            Assert.AreEqual(Api.RetrieveColumnAsInt64(this.sesid, this.tableid, this.columnidLongLong).Value, LongDelta);

            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
        }

        /// <summary>
        /// Verify that EscrowUpdate returns the previous column value.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Verify that EscrowUpdate returns the previous column value")]
        public void VerifyEscrowUpdateReturnsOldValue()
        {
            Api.JetBeginTransaction(this.sesid);

            int actual1 = Api.EscrowUpdate(this.sesid, this.tableid, this.columnidLong, 1);
            Assert.AreEqual(0, actual1);

            long actual3 = Api.EscrowUpdate(this.sesid, this.tableid, this.columnidLongLong, 1L);
            Assert.AreEqual(0L, actual3);

            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
        }

        /// <summary>
        /// Verify that EscrowUpdate updates the column.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Verify that EscrowUpdate updates the column")]
        public void VerifyEscrowUpdateUpdatesColumn()
        {
            const int Delta = 17;
            const long LongDelta = uint.MaxValue + 17L;

            Api.JetBeginTransaction(this.sesid);
            
            Api.EscrowUpdate(this.sesid, this.tableid, this.columnidLong, Delta);
            Assert.AreEqual(Delta, Api.RetrieveColumnAsInt32(this.sesid, this.tableid, this.columnidLong).Value);

            Api.EscrowUpdate(this.sesid, this.tableid, this.columnidLongLong, LongDelta);
            Assert.AreEqual(LongDelta, Api.RetrieveColumnAsInt64(this.sesid, this.tableid, this.columnidLongLong).Value);

            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
        }

        /// <summary>
        /// Test overflow on escrow updates. Overflow should cause the column value to roll over.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test overflow on escrow updates")]
        public void TestEscrowUpdateOverflow()
        {
            Api.JetBeginTransaction(this.sesid);

            Api.EscrowUpdate(this.sesid, this.tableid, this.columnidLong, int.MaxValue);
            Api.EscrowUpdate(this.sesid, this.tableid, this.columnidLong, 1);
            Assert.AreEqual(int.MinValue, Api.RetrieveColumnAsInt32(this.sesid, this.tableid, this.columnidLong).Value);

            Api.EscrowUpdate(this.sesid, this.tableid, this.columnidLongLong, long.MaxValue);
            Api.EscrowUpdate(this.sesid, this.tableid, this.columnidLongLong, 1L);
            Assert.AreEqual(long.MinValue, Api.RetrieveColumnAsInt64(this.sesid, this.tableid, this.columnidLongLong).Value);

            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
        }

        /// <summary>
        /// Test underflow on escrow updates. Underflow should cause the column value to roll over.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test underflow on escrow updates")]
        public void TestEscrowUpdateUnderflow()
        {
            Api.JetBeginTransaction(this.sesid);

            // 32-bit escrow columns don't detect underflow (to confirm with legacy behavior)
            Api.EscrowUpdate(this.sesid, this.tableid, this.columnidLong, int.MinValue);
            Api.EscrowUpdate(this.sesid, this.tableid, this.columnidLong, -1);
            Assert.AreEqual(int.MaxValue, Api.RetrieveColumnAsInt32(this.sesid, this.tableid, this.columnidLong).Value);

            Api.EscrowUpdate(this.sesid, this.tableid, this.columnidLongLong, long.MinValue);
            Api.EscrowUpdate(this.sesid, this.tableid, this.columnidLongLong, -1L);
            Assert.AreEqual(long.MaxValue, Api.RetrieveColumnAsInt64(this.sesid, this.tableid, this.columnidLongLong).Value);

            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
        }
    }
}