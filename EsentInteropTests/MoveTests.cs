//-----------------------------------------------------------------------
// <copyright file="MoveTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.IO;
    using System.Text;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test JetMove
    /// </summary>
    [TestClass]
    public class MoveTests
    {
        /// <summary>
        /// Number of records inserted in the table.
        /// </summary>
        private readonly int numRecords = 5;

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
        /// Columnid of the Long column in the table.
        /// </summary>
        private JET_COLUMNID columnidLong;

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

            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesid, String.Empty, String.Empty);
            Api.JetCreateDatabase(this.sesid, this.database, String.Empty, out this.dbid, CreateDatabaseGrbit.None);
            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateTable(this.sesid, this.dbid, this.table, 0, 100, out this.tableid);

            var columndef = new JET_COLUMNDEF()
            {
                cp = JET_CP.Unicode,
                coltyp = JET_coltyp.Long,
            };
            Api.JetAddColumn(this.sesid, this.tableid, "Long", columndef, null, 0, out this.columnidLong);

            for (int i = 0; i < this.numRecords; ++i)
            {
                Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
                Api.JetSetColumn(this.sesid, this.tableid, this.columnidLong, BitConverter.GetBytes(i), 4, SetColumnGrbit.None, null);
                int ignored;
                Api.JetUpdate(this.sesid, this.tableid, null, 0, out ignored);
            }

            Api.JetCloseTable(this.sesid, this.tableid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
            Api.JetOpenTable(this.sesid, this.dbid, this.table, null, 0, OpenTableGrbit.None, out this.tableid);
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
        /// Test moving to the first record.
        /// </summary>
        [TestMethod]
        public void MoveFirst()
        {
            int expected = 0;
            Api.JetMove(this.sesid, this.tableid, JET_Move.First, MoveGrbit.None);
            int actual = this.GetLongColumn();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test moving previous to the first record.
        /// This should generate an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(EsentException))]
        public void MoveBeforeFirst()
        {
            Api.JetMove(this.sesid, this.tableid, JET_Move.First, MoveGrbit.None);
            Api.JetMove(this.sesid, this.tableid, JET_Move.Previous, MoveGrbit.None);
        }

        /// <summary>
        /// Test moving to the next record.
        /// </summary>
        [TestMethod]
        public void MoveNext()
        {
            int expected = 1;
            Api.JetMove(this.sesid, this.tableid, JET_Move.First, MoveGrbit.None);
            Api.JetMove(this.sesid, this.tableid, JET_Move.Next, MoveGrbit.None);
            int actual = this.GetLongColumn();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test moving several records.
        /// </summary>
        [TestMethod]
        public void Move()
        {
            int expected = 3;
            Api.JetMove(this.sesid, this.tableid, JET_Move.First, MoveGrbit.None);
            Api.JetMove(this.sesid, this.tableid, expected, MoveGrbit.None);
            int actual = this.GetLongColumn();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test moving to the last record.
        /// </summary>
        [TestMethod]
        public void MoveLast()
        {
            int expected = this.numRecords - 1;
            Api.JetMove(this.sesid, this.tableid, JET_Move.Last, MoveGrbit.None);
            int actual = this.GetLongColumn();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test moving after the last record.
        /// This should generate an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(EsentException))]
        public void MoveAfterLast()
        {
            Api.JetMove(this.sesid, this.tableid, JET_Move.Last, MoveGrbit.None);
            Api.JetMove(this.sesid, this.tableid, JET_Move.Next, MoveGrbit.None);
        }

        /// <summary>
        /// Test moving to the previous record.
        /// </summary>
        [TestMethod]
        public void MovePrevious()
        {
            int expected = this.numRecords - 2;
            Api.JetMove(this.sesid, this.tableid, JET_Move.Last, MoveGrbit.None);
            Api.JetMove(this.sesid, this.tableid, JET_Move.Previous, MoveGrbit.None);
            int actual = this.GetLongColumn();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Scan all the records
        /// </summary>
        [TestMethod]
        public void ScanRecords()
        {
            Api.JetMove(this.sesid, this.tableid, JET_Move.First, MoveGrbit.None);
            Api.JetSetTableSequential(this.sesid, this.tableid, SetTableSequentialGrbit.None);
            for (int i = 0; i < this.numRecords - 1; ++i)
            {
                int actual = this.GetLongColumn();
                Assert.AreEqual(i, actual);
                Api.JetMove(this.sesid, this.tableid, JET_Move.Next, MoveGrbit.None);
            }

            Api.JetResetTableSequential(this.sesid, this.tableid, ResetTableSequentialGrbit.None);
        }

        /// <summary>
        /// Try moving to the first record.
        /// </summary>
        [TestMethod]
        public void TryMoveFirst()
        {
            int expected = 0;
            Assert.AreEqual(true, Api.TryMoveFirst(this.sesid, this.tableid));
            int actual = this.GetLongColumn();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Try moving previous to the first record.
        /// </summary>
        [TestMethod]
        public void TryMoveBeforeFirst()
        {
            Assert.AreEqual(true, Api.TryMoveFirst(this.sesid, this.tableid));
            Assert.AreEqual(false, Api.TryMovePrevious(this.sesid, this.tableid));
        }

        /// <summary>
        /// Try moving to the next record.
        /// </summary>
        [TestMethod]
        public void TryMoveNext()
        {
            int expected = 1;
            Assert.AreEqual(true, Api.TryMoveFirst(this.sesid, this.tableid));
            Assert.AreEqual(true, Api.TryMoveNext(this.sesid, this.tableid));
            int actual = this.GetLongColumn();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Try moving to the last record.
        /// </summary>
        [TestMethod]
        public void TryMoveLast()
        {
            int expected = this.numRecords - 1;
            Assert.AreEqual(true, Api.TryMoveLast(this.sesid, this.tableid));
            int actual = this.GetLongColumn();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test moving after the last record.
        /// This should generate an exception.
        /// </summary>
        [TestMethod]
        public void TryMoveAfterLast()
        {
            Assert.AreEqual(true, Api.TryMoveLast(this.sesid, this.tableid));
            Assert.AreEqual(false, Api.TryMoveNext(this.sesid, this.tableid));
        }

        /// <summary>
        /// Test moving to the previous record.
        /// </summary>
        [TestMethod]
        public void TryMovePrevious()
        {
            int expected = this.numRecords - 2;
            Assert.AreEqual(true, Api.TryMoveLast(this.sesid, this.tableid));
            Assert.AreEqual(true, Api.TryMovePrevious(this.sesid, this.tableid));
            int actual = this.GetLongColumn();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Return the value of the columnidLong of the current record.
        /// </summary>
        /// <returns>The value of the columnid, converted to an int.</returns>
        private int GetLongColumn()
        {
            byte[] data = new byte[4];
            int actualDataSize;
            Api.JetRetrieveColumn(this.sesid, this.tableid, this.columnidLong, data, data.Length, out actualDataSize, RetrieveColumnGrbit.None, null);
            Assert.AreEqual(data.Length, actualDataSize);
            return BitConverter.ToInt32(data, 0);
        }
    }
}
