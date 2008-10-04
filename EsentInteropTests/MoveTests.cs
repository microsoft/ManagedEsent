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
    using Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test JetMove
    /// </summary>
    [TestClass]
    public class MoveTests
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
        /// Columnid of the Long column in the table.
        /// </summary>
        private JET_COLUMNID columnidLong;

        /// <summary>
        /// Number of records inserted in the table.
        /// </summary>
        private readonly int numRecords = 5;

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

            API.JetInit(ref this.instance);
            API.JetBeginSession(this.instance, out this.sesid, String.Empty, String.Empty);
            API.JetCreateDatabase(this.sesid, this.database, String.Empty, out this.dbid, CreateDatabaseGrbit.None);
            API.JetBeginTransaction(this.sesid);
            API.JetCreateTable(this.sesid, this.dbid, this.table, 0, 100, out this.tableid);

            var columndef = new JET_COLUMNDEF()
            {
                cp = JET_CP.Unicode,
                coltyp = JET_coltyp.Long,
            };
            API.JetAddColumn(this.sesid, this.tableid, "Long", columndef, null, 0, out this.columnidLong);

            for (int i = 0; i < this.numRecords; ++i)
            {
                API.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
                API.JetSetColumn(this.sesid, this.tableid, this.columnidLong, BitConverter.GetBytes(i), 4, SetColumnGrbit.None, null);
                int ignored;
                API.JetUpdate(this.sesid, this.tableid, null, 0, out ignored);
            }

            API.JetCloseTable(this.sesid, this.tableid);
            API.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
            API.JetOpenTable(this.sesid, this.dbid, this.table, null, 0, OpenTableGrbit.None, out this.tableid);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            API.JetCloseTable(this.sesid, this.tableid);
            API.JetEndSession(this.sesid, EndSessionGrbit.None);
            API.JetTerm(this.instance);
            Directory.Delete(this.directory, true);
        }

        /// <summary>
        /// Test moving to the first record.
        /// </summary>
        [TestMethod]
        public void MoveFirstTest()
        {
            int expected = 0;
            API.JetMove(this.sesid, this.tableid, JET_Move.First, MoveGrbit.None);
            int actual = this.GetLongColumn();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test moving to the next record.
        /// </summary>
        [TestMethod]
        public void MoveNextTest()
        {
            int expected = 1;
            API.JetMove(this.sesid, this.tableid, JET_Move.First, MoveGrbit.None);
            API.JetMove(this.sesid, this.tableid, JET_Move.Next, MoveGrbit.None);
            int actual = this.GetLongColumn();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test moving several records.
        /// </summary>
        [TestMethod]
        public void MoveTest()
        {
            int expected = 3;
            API.JetMove(this.sesid, this.tableid, JET_Move.First, MoveGrbit.None);
            API.JetMove(this.sesid, this.tableid, expected, MoveGrbit.None);
            int actual = this.GetLongColumn();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test moving to the last record.
        /// </summary>
        [TestMethod]
        public void MoveLastTest()
        {
            int expected = this.numRecords-1;
            API.JetMove(this.sesid, this.tableid, JET_Move.Last, MoveGrbit.None);
            int actual = this.GetLongColumn();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test moving to the previous record.
        /// </summary>
        [TestMethod]
        public void MovePreviousTest()
        {
            int expected = this.numRecords - 2;
            API.JetMove(this.sesid, this.tableid, JET_Move.Last, MoveGrbit.None);
            API.JetMove(this.sesid, this.tableid, JET_Move.Previous, MoveGrbit.None);
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
            API.JetRetrieveColumn(this.sesid, this.tableid, this.columnidLong, data, data.Length, out actualDataSize, RetrieveColumnGrbit.None, null);
            Assert.AreEqual(data.Length, actualDataSize);
            return BitConverter.ToInt32(data, 0);
        }
    }
}
