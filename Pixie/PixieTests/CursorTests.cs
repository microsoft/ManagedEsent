//-----------------------------------------------------------------------
// <copyright file="CursorTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using Microsoft.Isam.Esent;
using Microsoft.Isam.Esent.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PixieTests
{
    /// <summary>
    /// Test the Cursor class
    /// </summary>
    [TestClass]
    public class CursorTests
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
        private string tablename;

        /// <summary>
        /// The instance used by the test.
        /// </summary>
        private JET_INSTANCE instance;

        /// <summary>
        /// The session used by the test.
        /// </summary>
        private Session session;

        /// <summary>
        /// Identifies the database used by the test.
        /// </summary>
        private JET_DBID dbid;

        /// <summary>
        /// Columnid of the Long column in the table.
        /// </summary>
        private JET_COLUMNID columnidLong;

        /// <summary>
        /// Columnid of the Text column in the table.
        /// </summary>
        private JET_COLUMNID columnidText;

        /// <summary>
        /// Columnid of the escrow updatable column in the table.
        /// </summary>
        private JET_COLUMNID columnidEscrow;

        #region Setup/Teardown

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// All DDL should be done in this method.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            JET_TABLEID tableid;

            this.numRecords = 5;

            this.directory = SetupHelper.CreateRandomDirectory();
            this.database = Path.Combine(this.directory, "database.edb");
            this.tablename = "table";
            this.instance = SetupHelper.CreateNewInstance(this.directory);

            // turn off logging so initialization is faster
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.PageTempDBMin, Api.PageTempDBSmallest, null);
            Api.JetInit(ref this.instance);
            this.session = new Session(this.instance);
            Api.JetCreateDatabase(this.session, this.database, String.Empty, out this.dbid, CreateDatabaseGrbit.None);
            Api.JetBeginTransaction(this.session);
            Api.JetCreateTable(this.session, this.dbid, this.tablename, 0, 100, out tableid);

            var columndef = new JET_COLUMNDEF { coltyp = JET_coltyp.Long };
            Api.JetAddColumn(this.session, tableid, "Long", columndef, null, 0, out this.columnidLong);

            columndef = new JET_COLUMNDEF { coltyp = JET_coltyp.Text };
            Api.JetAddColumn(this.session, tableid, "Text", columndef, null, 0, out this.columnidText);

            columndef = new JET_COLUMNDEF { coltyp = JET_coltyp.Long, grbit = ColumndefGrbit.ColumnEscrowUpdate };
            Api.JetAddColumn(this.session, tableid, "Escrow", columndef, BitConverter.GetBytes(0), 4, out this.columnidEscrow);

            string indexDef = "+long\0\0";
            Api.JetCreateIndex(this.session, tableid, "primary", CreateIndexGrbit.IndexPrimary, indexDef, indexDef.Length, 100);

            for (int i = 0; i < this.numRecords; ++i)
            {
                Api.JetPrepareUpdate(this.session, tableid, JET_prep.Insert);
                Api.JetSetColumn(this.session, tableid, this.columnidLong, BitConverter.GetBytes(i), 4, SetColumnGrbit.None, null);
                Api.JetUpdate(this.session, tableid);
            }

            Api.JetCloseTable(this.session, tableid);
            Api.JetCommitTransaction(this.session, CommitTransactionGrbit.LazyFlush);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            Api.JetEndSession(this.session, EndSessionGrbit.None);
            Api.JetTerm(this.instance);
            Directory.Delete(this.directory, true);
        }

        /// <summary>
        /// Verify that the test class has setup the test fixture properly.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void VerifyFixtureSetup()
        {
            Assert.AreNotEqual(JET_INSTANCE.Nil, this.instance);
            Assert.AreNotEqual(JET_SESID.Nil, this.session);
            Assert.IsTrue(this.numRecords > 0);
            Assert.AreNotEqual(JET_COLUMNID.Nil, this.columnidLong);
        }

        #endregion Setup/Teardown

        #region DDL

        [TestMethod]
        [Priority(1)]
        public void VerifyAddColumnCreatesColumn()
        {
            Cursor cursor = this.OpenCursor();
            var columndef = new JET_COLUMNDEF { coltyp = JET_coltyp.Binary };
            JET_COLUMNID columnid;
            cursor.AddColumn("newcolumn", columndef, null, out columnid);
            cursor.PrepareUpdate(JET_prep.Insert);
            cursor.SetColumn(columnid, Any.Bytes, SetColumnGrbit.None);
        }

        [TestMethod]
        [Priority(1)]
        public void VerifyAddColumnWithDefaultValueCreatesColumnWithDefaultValue()
        {
            byte[] defaultValue = Any.Bytes;

            Cursor cursor = this.OpenCursor();
            var columndef = new JET_COLUMNDEF { coltyp = JET_coltyp.Binary };
            JET_COLUMNID columnid;
            cursor.AddColumn("column_with_default", columndef, defaultValue, out columnid);
            cursor.MoveFirst();
            CollectionAssert.AreEqual(defaultValue, cursor.RetrieveColumn(columnid, RetrieveColumnGrbit.None));
        }

        [TestMethod]
        [Priority(1)]
        public void VerifyGetColumnsReturnsColumns()
        {
            Cursor cursor = this.OpenCursor();
            ColumnInfo[] columnInfo = cursor.GetColumns().ToArray();
            Assert.IsTrue(columnInfo.Any(x => x.Name == "Long"));
            Assert.IsTrue(columnInfo.Any(x => x.Name == "Text"));
        }

        #endregion

        #region Lifecycle

        [TestMethod]
        [Priority(1)]
        public void VerifyCursorClosedEventIsCalledWhenCursorIsDisposed()
        {
            bool eventCalled = false;
            Cursor cursor = this.OpenCursor();
            cursor.Disposed += (ignored) => eventCalled = true;

            cursor.Dispose();

            Assert.IsTrue(eventCalled);
        }

        #endregion

        #region Navigation

        [TestMethod]
        [Priority(1)]
        public void VerifyTryMoveFirstMovesToFirstRecord()
        {
            Cursor cursor = this.OpenCursor();
            Assert.IsTrue(cursor.TryMoveFirst());

            this.AssertOnRecord(cursor, 0);
        }

        [TestMethod]
        [Priority(1)]
        public void VerifyTryMoveLastMovesToLastRecord()
        {
            Cursor cursor = this.OpenCursor();
            Assert.IsTrue(cursor.TryMoveLast());

            this.AssertOnRecord(cursor, this.numRecords - 1);
        }

        [TestMethod]
        [Priority(1)]
        public void VerifyMoveFirstMovesToFirstRecord()
        {
            Cursor cursor = this.OpenCursor();
            cursor.MoveFirst();

            this.AssertOnRecord(cursor, 0);
        }

        [TestMethod]
        [Priority(1)]
        public void VerifyMoveLastMovesToLastRecord()
        {
            Cursor cursor = this.OpenCursor();
            cursor.MoveLast();

            this.AssertOnRecord(cursor, this.numRecords - 1);
        }

        [TestMethod]
        [Priority(1)]
        public void VerifyTryMoveNextMovesToNextRecord()
        {
            Cursor cursor = this.OpenCursor();
            Assert.IsTrue(cursor.TryMoveFirst());
            Assert.IsTrue(cursor.TryMoveNext());

            this.AssertOnRecord(cursor, 1);
        }

        [TestMethod]
        [Priority(1)]
        public void VerifyTryMovePreviousMovesToPreviousRecord()
        {
            Cursor cursor = this.OpenCursor();
            Assert.IsTrue(cursor.TryMoveLast());
            Assert.IsTrue(cursor.TryMovePrevious());

            this.AssertOnRecord(cursor, this.numRecords - 2);
        }

        [TestMethod]
        [Priority(1)]
        public void VerifyMoveBeforeFirstMovesBeforeFirstRecord()
        {
            Cursor cursor = this.OpenCursor();
            cursor.MoveBeforeFirst();
            Assert.IsTrue(cursor.TryMoveNext());

            this.AssertOnRecord(cursor, 0);
        }

        [TestMethod]
        [Priority(1)]
        public void VerifyMoveAfterLastMovesAfterLastRecord()
        {
            Cursor cursor = this.OpenCursor();
            cursor.MoveAfterLast();
            Assert.IsTrue(cursor.TryMovePrevious());

            this.AssertOnRecord(cursor, this.numRecords - 1);
        }

        [TestMethod]
        [Priority(1)]
        public void VerifyGotoBookmarkPositionsCursor()
        {
            Cursor cursor = this.OpenCursor();
            cursor.TryMoveFirst();
            Cursor.Bookmark bookmark = cursor.GetBookmark();
            cursor.TryMoveLast();
            cursor.GotoBookmark(bookmark);

            this.AssertOnRecord(cursor, 0);
        }

        #endregion

        #region DML

        [TestMethod]
        [Priority(1)]
        public void VerifyInsertRecordCreatesRecord()
        {
            Cursor cursor = this.OpenCursor();
            cursor.PrepareUpdate(JET_prep.Insert);
            cursor.SetColumn(this.columnidLong, BitConverter.GetBytes(999), SetColumnGrbit.None);
            cursor.Update();

            this.AssertOnRecord(cursor, 999);
        }

        [TestMethod]
        [Priority(1)]
        public void VerifySetColumnWithNullSetsColumnToNull()
        {
            int key = Any.Int32;
            Cursor cursor = this.OpenCursor();
            cursor.PrepareUpdate(JET_prep.Insert);
            cursor.SetColumn(this.columnidLong, BitConverter.GetBytes(key), SetColumnGrbit.None);
            cursor.SetColumn(this.columnidText, null, SetColumnGrbit.None);
            cursor.Update();

            this.AssertOnRecord(cursor, key);
            Assert.IsNull(cursor.RetrieveColumn(this.columnidText, RetrieveColumnGrbit.None));
        }

        [TestMethod]
        [Priority(1)]
        public void VerifySetColumnWithZeroLengthSetsColumnToZeroLength()
        {
            int key = Any.Int32;
            Cursor cursor = this.OpenCursor();
            cursor.PrepareUpdate(JET_prep.Insert);
            cursor.SetColumn(this.columnidLong, BitConverter.GetBytes(key), SetColumnGrbit.None);
            cursor.SetColumn(this.columnidText, new byte[0], SetColumnGrbit.None);
            cursor.Update();

            this.AssertOnRecord(cursor, key);    
            Assert.AreEqual(0, cursor.RetrieveColumn(this.columnidText, RetrieveColumnGrbit.None).Length);
        }

        [TestMethod]
        [Priority(1)]
        public void VerifyEscrowUpdate()
        {
            Cursor cursor = this.OpenCursor();
            cursor.MoveFirst();
            using (var trx = new Microsoft.Isam.Esent.Interop.Transaction(this.session))
            {
                Assert.AreEqual(0, cursor.EscrowUpdate(this.columnidEscrow, 4));
                Assert.AreEqual(4, BitConverter.ToInt32(cursor.RetrieveColumn(this.columnidEscrow, RetrieveColumnGrbit.None), 0));
            }
        }

        [TestMethod]
        [Priority(1)]
        public void VerifyDeleteRecordRemovesRecord()
        {
            Cursor cursor = this.OpenCursor();
            cursor.TryMoveFirst();
            cursor.Delete();
            cursor.TryMoveFirst();

            this.AssertOnRecord(cursor, 1);
        }

        [TestMethod]
        [Priority(1)]
        public void VerifyCancelUpdateStopsUpdate()
        {
            Cursor cursor = this.OpenCursor();
            cursor.TryMoveFirst();
            cursor.PrepareUpdate(JET_prep.Insert);
            cursor.SetColumn(this.columnidLong, BitConverter.GetBytes(999), SetColumnGrbit.None);
            cursor.CancelUpdate();

            this.AssertOnRecord(cursor, 0);
        }

        #endregion

        #region Error Checking

        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(InvalidOperationException))]
        public void VerifySetColumnWithoutUpdateThrowsException()
        {
            Cursor cursor = this.OpenCursor();
            cursor.SetColumn(this.columnidLong, BitConverter.GetBytes(999), SetColumnGrbit.None);
        }

        [TestMethod]
        [Priority(1)]
        public void VerifyRetrieveCopyWithoutCurrencyDoesNotThrowException()
        {
            // We don't have a currency but are inserting a record so RetrieveCopy is valid
            Cursor cursor = this.OpenCursor();
            cursor.MoveAfterLast();
            cursor.PrepareUpdate(JET_prep.Insert);

            Assert.AreEqual(null, cursor.RetrieveColumn(this.columnidLong, RetrieveColumnGrbit.RetrieveCopy));
        }

        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(InvalidOperationException))]
        public void VerifyRetrieveColumnWithoutCurrencyThrowsException()
        {
            Cursor cursor = this.OpenCursor();
            cursor.MoveAfterLast();
            byte[] ignored = cursor.RetrieveColumn(this.columnidLong, RetrieveColumnGrbit.None);
        }

        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(InvalidOperationException))]
        public void VerifyNavigationCancelsUpdate()
        {
            Cursor cursor = this.OpenCursor();
            cursor.PrepareUpdate(JET_prep.Insert);
            cursor.TryMoveNext();
            cursor.SetColumn(this.columnidLong, BitConverter.GetBytes(999), SetColumnGrbit.None);
        }

        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentException))]
        public void VerifyUpdateWithPrepCancelThrowsException()
        {
            Cursor cursor = this.OpenCursor();
            cursor.PrepareUpdate(JET_prep.Cancel);
        }

        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void VerifyUsingDisposedCursorThrowsException()
        {
            Cursor cursor = this.OpenCursor();
            cursor.Dispose();
            cursor.TryMoveFirst();
        }

        [TestMethod]
        [Priority(1)]
        public void DisposeTwice()
        {
            Cursor cursor = this.OpenCursor();
            cursor.Dispose();
            cursor.Dispose();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Opens a new Cursor on the table.
        /// </summary>
        /// <returns>A new Cursor on the test table.</returns>
        private Cursor OpenCursor()
        {
            return new Cursor(this.session, this.dbid, this.tablename);
        }

        /// <summary>
        /// Assert that we are currently positioned on the given record.
        /// </summary>
        /// <param name="cursor">The cursor to check.</param>
        /// <param name="recordId">The expected record ID.</param>
        private void AssertOnRecord(Cursor cursor, int recordId)
        {
            int actualId = this.RetrieveColumn(cursor);
            Assert.AreEqual(recordId, actualId);
        }

        /// <summary>
        /// Return the value of the columnidLong of the cursor.
        /// </summary>
        /// <param name="cursor">The cursor to retrieve the column from.</param>
        /// <returns>The value of the columnid, converted to an int.</returns>
        private int RetrieveColumn(Cursor cursor)
        {
            byte[] data = cursor.RetrieveColumn(this.columnidLong, RetrieveColumnGrbit.None);
            Assert.AreEqual(4, data.Length);
            return BitConverter.ToInt32(data, 0);
        }

        #endregion Helper Methods
    }
}
