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
        private int numRecords;

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

        #region Setup/Teardown

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// All DDL should be done in this method.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            var random = new Random();
            this.numRecords = random.Next(5, 20);

            this.directory = SetupHelper.CreateRandomDirectory();
            this.database = Path.Combine(this.directory, "database.edb");
            this.table = "table";
            this.instance = SetupHelper.CreateNewInstance(this.directory);

            // turn off logging so initialization is faster
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesid, String.Empty, String.Empty);
            Api.JetCreateDatabase(this.sesid, this.database, String.Empty, out this.dbid, CreateDatabaseGrbit.None);
            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateTable(this.sesid, this.dbid, this.table, 0, 100, out this.tableid);

            var columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Long };
            Api.JetAddColumn(this.sesid, this.tableid, "Long", columndef, null, 0, out this.columnidLong);

            var indexDef = "+long\0\0";
            Api.JetCreateIndex(this.sesid, this.tableid, "primary", CreateIndexGrbit.IndexPrimary, indexDef, indexDef.Length, 100);

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
        /// Verify that the test class has setup the test fixture properly.
        /// </summary>
        [TestMethod]
        public void VerifyFixtureSetup()
        {
            Assert.AreNotEqual(JET_INSTANCE.Nil, this.instance);
            Assert.AreNotEqual(JET_SESID.Nil, this.sesid);
            Assert.IsTrue(this.numRecords > 0);
            Assert.AreNotEqual(JET_COLUMNID.Nil, this.columnidLong);

            JET_COLUMNDEF columndef;
            Api.JetGetTableColumnInfo(this.sesid, this.tableid, this.columnidLong, out columndef);
            Assert.AreEqual(JET_coltyp.Long, columndef.coltyp);
        }

        #endregion Setup/Teardown

        #region JetSeek Tests

        /// <summary>
        /// Seek for a record with SeekLT
        /// </summary>
        [TestMethod]
        public void SeekLT()
        {
            int expected = this.numRecords / 2;
            this.MakeKeyForRecord(expected + 1);    // use the next higher key
            Api.JetSeek(this.sesid, this.tableid, SeekGrbit.SeekLT);
            int actual = this.GetLongColumn();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Seek for a record with SeekLE
        /// </summary>
        [TestMethod]
        public void SeekLE()
        {
            int expected = this.numRecords / 2;
            this.MakeKeyForRecord(expected);
            Api.JetSeek(this.sesid, this.tableid, SeekGrbit.SeekLE);
            int actual = this.GetLongColumn();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Seek for a record with SeekEQ
        /// </summary>
        [TestMethod]
        public void SeekEQ()
        {
            int expected = this.numRecords / 2;
            this.MakeKeyForRecord(expected);
            Api.JetSeek(this.sesid, this.tableid, SeekGrbit.SeekEQ);
            int actual = this.GetLongColumn();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Seek for a record with SeekGE
        /// </summary>
        [TestMethod]
        public void SeekGE()
        {
            int expected = this.numRecords / 2;
            this.MakeKeyForRecord(expected);
            Api.JetSeek(this.sesid, this.tableid, SeekGrbit.SeekGE);
            int actual = this.GetLongColumn();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Seek for a record with SeekGT
        /// </summary>
        [TestMethod]
        public void SeekGT()
        {
            int expected = this.numRecords / 2;
            this.MakeKeyForRecord(expected - 1);    // use the previous key
            Api.JetSeek(this.sesid, this.tableid, SeekGrbit.SeekGT);
            int actual = this.GetLongColumn();
            Assert.AreEqual(expected, actual);
        }

        #endregion JetSeek Tests

        #region JetMove Tests

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

        #endregion JetMove Tests

        #region JetGotoPosition Tests

        /// <summary>
        /// Test using JetGotoPosition to go to the first record
        /// </summary>
        [TestMethod]
        public void GotoFirstPosition()
        {
            var recpos = new JET_RECPOS() { centriesLT = 0, centriesTotal = 10 };
            Api.JetGotoPosition(this.sesid, this.tableid, recpos);
            int actual = this.GetLongColumn();
            Assert.AreEqual(0, actual);
        }

        /// <summary>
        /// Test using JetGotoPosition to go to the last record
        /// </summary>
        [TestMethod]
        public void GotoLastPosition()
        {
            var recpos = new JET_RECPOS() { centriesLT = 4, centriesTotal = 4 };
            Api.JetGotoPosition(this.sesid, this.tableid, recpos);
            int actual = this.GetLongColumn();
            Assert.AreEqual(this.numRecords - 1, actual);
        }

        #endregion JetGotoPosition Tests

        #region JetSetIndexRange Tests

        /// <summary>
        /// Create an ascending inclusive index range
        /// </summary>
        [TestMethod]
        public void AscendingInclusiveIndexRange()
        {
            int first = 1;
            int last = this.numRecords - 1;

            this.MakeKeyForRecord(first);
            Api.JetSeek(this.sesid, this.tableid, SeekGrbit.SeekEQ);

            this.MakeKeyForRecord(last);
            Api.JetSetIndexRange(this.sesid, this.tableid, SetIndexRangeGrbit.RangeUpperLimit | SetIndexRangeGrbit.RangeInclusive);

            for (int i = first; i <= last; ++i)
            {
                int actual = this.GetLongColumn();
                Assert.AreEqual(i, actual);
                if (last != i)
                {
                    Api.JetMove(this.sesid, this.tableid, JET_Move.Next, MoveGrbit.None);
                }
            }

            Assert.IsFalse(Api.TryMoveNext(this.sesid, this.tableid));
        }

        /// <summary>
        /// Create an ascending exclusive index range
        /// </summary>
        [TestMethod]
        public void AscendingExclusiveIndexRange()
        {
            int first = 1;
            int last = this.numRecords - 1;

            this.MakeKeyForRecord(first);
            Api.JetSeek(this.sesid, this.tableid, SeekGrbit.SeekEQ);

            this.MakeKeyForRecord(last);
            Api.JetSetIndexRange(this.sesid, this.tableid, SetIndexRangeGrbit.RangeUpperLimit);

            for (int i = first; i < last; ++i)
            {
                int actual = this.GetLongColumn();
                Assert.AreEqual(i, actual);
                if (last - 1 != i)
                {
                    Api.JetMove(this.sesid, this.tableid, JET_Move.Next, MoveGrbit.None);
                }
            }

            Assert.IsFalse(Api.TryMoveNext(this.sesid, this.tableid));
        }

        /// <summary>
        /// Create a descending inclusive index range
        /// </summary>
        [TestMethod]
        public void DescendingInclusiveIndexRange()
        {
            int first = 1;
            int last = this.numRecords - 1;

            this.MakeKeyForRecord(last);
            Api.JetSeek(this.sesid, this.tableid, SeekGrbit.SeekEQ);

            this.MakeKeyForRecord(first);
            Api.JetSetIndexRange(this.sesid, this.tableid, SetIndexRangeGrbit.RangeInclusive);

            for (int i = last; i >= first; --i)
            {
                int actual = this.GetLongColumn();
                Assert.AreEqual(i, actual);
                if (first != i)
                {
                    Api.JetMove(this.sesid, this.tableid, JET_Move.Previous, MoveGrbit.None);
                }
            }

            Assert.IsFalse(Api.TryMovePrevious(this.sesid, this.tableid));
        }

        /// <summary>
        /// Create a descending exclusive index range
        /// </summary>
        [TestMethod]
        public void DescendingExclusiveIndexRange()
        {
            int first = 1;
            int last = this.numRecords - 1;

            this.MakeKeyForRecord(last);
            Api.JetSeek(this.sesid, this.tableid, SeekGrbit.SeekEQ);

            this.MakeKeyForRecord(first);
            Api.JetSetIndexRange(this.sesid, this.tableid, SetIndexRangeGrbit.None);

            for (int i = last; i > first; --i)
            {
                int actual = this.GetLongColumn();
                Assert.AreEqual(i, actual);
                if (first + 1 != i)
                {
                    Api.JetMove(this.sesid, this.tableid, JET_Move.Previous, MoveGrbit.None);
                }
            }

            Assert.IsFalse(Api.TryMovePrevious(this.sesid, this.tableid));
        }

        /// <summary>
        /// Count the records in an index range with JetGetIndexRecordCount
        /// </summary>
        [TestMethod]
        public void CountIndexRangeRecords()
        {
            int first = 2;
            int last = this.numRecords - 2;

            this.MakeKeyForRecord(first);
            Api.JetSeek(this.sesid, this.tableid, SeekGrbit.SeekEQ);

            this.MakeKeyForRecord(last);
            Api.JetSetIndexRange(this.sesid, this.tableid, SetIndexRangeGrbit.RangeUpperLimit);

            int countedRecords;
            Api.JetIndexRecordCount(this.sesid, this.tableid, out countedRecords, 0);
            Assert.AreEqual(last - first, countedRecords);
        }

        #endregion

        /// <summary>
        /// Count the records in the table with JetIndexRecordCount
        /// </summary>
        [TestMethod]
        public void GetIndexRecordCount()
        {
            int countedRecords;
            Api.JetIndexRecordCount(this.sesid, this.tableid, out countedRecords, 0);
            Assert.AreEqual(this.numRecords, countedRecords);
        }

        /// <summary>
        /// Test using JetGetRecord position
        /// </summary>
        [TestMethod]
        public void GetRecordPosition()
        {
            Api.JetMove(this.sesid, this.tableid, JET_Move.Last, MoveGrbit.None);
            JET_RECPOS recpos;
            Api.JetGetRecordPosition(this.sesid, this.tableid, out recpos);
            Assert.AreEqual(recpos.centriesLT, recpos.centriesTotal - 1);
        }

        /// <summary>
        /// Scan all the records
        /// </summary>
        [TestMethod]
        public void ScanRecords()
        {
            Api.JetMove(this.sesid, this.tableid, JET_Move.First, MoveGrbit.None);
            Api.JetSetTableSequential(this.sesid, this.tableid, SetTableSequentialGrbit.None);
            for (int i = 0; i < this.numRecords; ++i)
            {
                int actual = this.GetLongColumn();
                Assert.AreEqual(i, actual);
                if (this.numRecords - 1 != i)
                {
                    Api.JetMove(this.sesid, this.tableid, JET_Move.Next, MoveGrbit.None);
                }
            }

            Api.JetResetTableSequential(this.sesid, this.tableid, ResetTableSequentialGrbit.None);
        }

        #region MoveHelper Tests

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

        #endregion MoveHelper Tests

        #region Helper Methods

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

        /// <summary>
        /// Make a key for a record with the given ID
        /// </summary>
        /// <param name="id">The id of the record.</param>
        private void MakeKeyForRecord(int id)
        {
            byte[] data = BitConverter.GetBytes(id);
            Api.JetMakeKey(this.sesid, this.tableid, data, data.Length, MakeKeyGrbit.NewKey);
        }

        #endregion Helper Methods
    }
}
