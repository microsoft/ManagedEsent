//-----------------------------------------------------------------------
// <copyright file="BasicTableTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Basic Api tests
    /// </summary>
    [TestClass]
    public class BasicTableTests
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
        /// Columnid of the LongText column in the table.
        /// </summary>
        private JET_COLUMNID columnidLongText;

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

            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesid, String.Empty, String.Empty);
            Api.JetCreateDatabase(this.sesid, this.database, String.Empty, out this.dbid, CreateDatabaseGrbit.None);
            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateTable(this.sesid, this.dbid, this.table, 0, 100, out this.tableid);

            var columndef = new JET_COLUMNDEF()
            {
                cp = JET_CP.Unicode,
                coltyp = JET_coltyp.LongText,
            };
            Api.JetAddColumn(this.sesid, this.tableid, "TestColumn", columndef, null, 0, out this.columnidLongText);

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
            Assert.IsNotNull(this.table);
            Assert.AreNotEqual(JET_INSTANCE.Nil, this.instance);
            Assert.AreNotEqual(JET_SESID.Nil, this.sesid);
            Assert.AreNotEqual(JET_DBID.Nil, this.dbid);
            Assert.AreNotEqual(JET_TABLEID.Nil, this.tableid);
            Assert.AreNotEqual(JET_COLUMNID.Nil, this.columnidLongText);

            JET_COLUMNDEF columndef;
            Api.JetGetTableColumnInfo(this.sesid, this.tableid, this.columnidLongText, out columndef);
            Assert.AreEqual(JET_coltyp.LongText, columndef.coltyp);
        }

        #endregion Setup/Teardown

        #region JetDupCursor

        /// <summary>
        /// JetDupCursor should return a different tableid.
        /// </summary>
        [TestMethod]
        public void JetDupCursorReturnsDifferentTableid()
        {
            JET_TABLEID newTableid;
            Api.JetDupCursor(this.sesid, this.tableid, out newTableid, DupCursorGrbit.None);
            Assert.AreNotEqual(newTableid, this.tableid);
            Api.JetCloseTable(this.sesid, newTableid);
        }

        /// <summary>
        /// JetDupCursor should return a tableid on the same table.
        /// </summary>
        [TestMethod]
        public void JetDupCursorReturnsCursorOnSameTable()
        {
            string expected = Any.String;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            this.SetColumnFromString(expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            JET_TABLEID newTableid;
            Api.JetDupCursor(this.sesid, this.tableid, out newTableid, DupCursorGrbit.None);
            Api.JetMove(this.sesid, newTableid, JET_Move.First, MoveGrbit.None);
            string actual = Api.RetrieveColumnAsString(this.sesid, newTableid, this.columnidLongText, Encoding.Unicode);
            Assert.AreEqual(expected, actual);
            Api.JetCloseTable(this.sesid, newTableid);
        }

        #endregion JetDupCursor

        #region DDL Tests

        /// <summary>
        /// Create one column of each type
        /// </summary>
        [TestMethod]
        public void CreateOneColumnOfEachType()
        {
            Api.JetBeginTransaction(this.sesid);
            foreach (JET_coltyp coltyp in Enum.GetValues(typeof(JET_coltyp)))
            {
                if (JET_coltyp.Nil != coltyp)
                {
                    var columndef = new JET_COLUMNDEF() { coltyp = coltyp };
                    JET_COLUMNID columnid;
                    Api.JetAddColumn(this.sesid, this.tableid, coltyp.ToString(), columndef, null, 0, out columnid);
                    Assert.AreEqual(columnid, columndef.columnid);
                }
            }

            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
        }

        /// <summary>
        /// Add a column and retrieve its information using JetGetTableColumnInfo
        /// </summary>
        [TestMethod]
        public void JetGetTableColumnInfo()
        {
            string columnName = "column1";
            Api.JetBeginTransaction(this.sesid);
            var columndef = new JET_COLUMNDEF()
            {
                cbMax = 4096,
                cp = JET_CP.Unicode,
                coltyp = JET_coltyp.LongText,
                grbit = ColumndefGrbit.None,                
            };
            
            JET_COLUMNID columnid;
            Api.JetAddColumn(this.sesid, this.tableid, columnName, columndef, null, 0, out columnid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            JET_COLUMNDEF retrievedColumndef;
            Api.JetGetTableColumnInfo(this.sesid, this.tableid, columnName, out retrievedColumndef);

            Assert.AreEqual(columndef.cbMax, retrievedColumndef.cbMax);
            Assert.AreEqual(columndef.cp, retrievedColumndef.cp);
            Assert.AreEqual(columndef.coltyp, retrievedColumndef.coltyp);
            Assert.AreEqual(columnid, retrievedColumndef.columnid);

            // The grbit isn't asserted as esent will add some options by default
        }

        /// <summary>
        /// Add a column and retrieve its information using JetGetTableColumnInfo
        /// </summary>
        [TestMethod]
        public void JetGetTableColumnInfoByColumnid()
        {
            string columnName = "column2";
            Api.JetBeginTransaction(this.sesid);
            var columndef = new JET_COLUMNDEF()
            {
                cbMax = 8192,
                cp = JET_CP.ASCII,
                coltyp = JET_coltyp.LongText,
                grbit = ColumndefGrbit.None,
            };

            JET_COLUMNID columnid;
            Api.JetAddColumn(this.sesid, this.tableid, columnName, columndef, null, 0, out columnid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            JET_COLUMNDEF retrievedColumndef;
            Api.JetGetTableColumnInfo(this.sesid, this.tableid, columnid, out retrievedColumndef);

            Assert.AreEqual(columndef.cbMax, retrievedColumndef.cbMax);
            Assert.AreEqual(columndef.cp, retrievedColumndef.cp);
            Assert.AreEqual(columndef.coltyp, retrievedColumndef.coltyp);
            Assert.AreEqual(columnid, retrievedColumndef.columnid);

            // The grbit isn't asserted as esent will add some options by default
        }

        /// <summary>
        /// Add a column and retrieve its information using JetGetColumnInfo
        /// </summary>
        [TestMethod]
        public void JetGetColumnInfo()
        {
            string columnName = "column3";
            Api.JetBeginTransaction(this.sesid);
            var columndef = new JET_COLUMNDEF()
            {
                cbMax = 200,
                cp = JET_CP.ASCII,
                coltyp = JET_coltyp.LongText,
                grbit = ColumndefGrbit.None,
            };

            JET_COLUMNID columnid;
            Api.JetAddColumn(this.sesid, this.tableid, columnName, columndef, null, 0, out columnid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
            JET_COLUMNDEF retrievedColumndef;
            Api.JetGetColumnInfo(this.sesid, this.dbid, this.table, columnName, out retrievedColumndef);

            Assert.AreEqual(columndef.cbMax, retrievedColumndef.cbMax);
            Assert.AreEqual(columndef.cp, retrievedColumndef.cp);
            Assert.AreEqual(columndef.coltyp, retrievedColumndef.coltyp);
            Assert.AreEqual(columnid, retrievedColumndef.columnid);

            // The grbit isn't asserted as esent will add some options by default
        }

        /// <summary>
        /// Add a column and retrieve its information using GetColumnDictionary
        /// </summary>
        [TestMethod]
        public void GetColumnDictionary()
        {
            string columnName = "column4";
            Api.JetBeginTransaction(this.sesid);
            var columndef = new JET_COLUMNDEF()
            {
                cbMax = 10000,
                cp = JET_CP.Unicode,
                coltyp = JET_coltyp.LongText,
                grbit = ColumndefGrbit.None,
            };

            JET_COLUMNID columnid;
            Api.JetAddColumn(this.sesid, this.tableid, columnName, columndef, null, 0, out columnid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            Dictionary<string, JET_COLUMNID> dict = Api.GetColumnDictionary(this.sesid, this.tableid);
            Assert.AreEqual(columnid, dict[columnName]);
        }

        /// <summary>
        /// Check that the dictionary returned by GetColumnDictionary is case-insensitive
        /// </summary>
        [TestMethod]
        public void GetColumnDictionaryIsCaseInsensitive()
        {
            Dictionary<string, JET_COLUMNID> dict = Api.GetColumnDictionary(this.sesid, this.tableid);
            Assert.AreEqual(this.columnidLongText, dict["tEsTcOLuMn"]);
        }

        /// <summary>
        /// Creates an index
        /// </summary>
        [TestMethod]
        public void CreateIndex()
        {
            string indexDescription = "+TestColumn\0";
            string indexName = "new_index";
            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateIndex(this.sesid, this.tableid, indexName, CreateIndexGrbit.None, indexDescription, indexDescription.Length + 1, 100); 
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            Api.JetSetCurrentIndex(this.sesid, this.tableid, indexName);
        }

        /// <summary>
        /// Delete an index and make sure we can't use it afterwards
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(EsentErrorException))]
        public void DeleteIndex()
        {
            string indexDescription = "+TestColumn\0";
            string indexName = "index_to_delete";
            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateIndex(this.sesid, this.tableid, indexName, CreateIndexGrbit.None, indexDescription, indexDescription.Length + 1, 100);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            Api.JetBeginTransaction(this.sesid);
            Api.JetDeleteIndex(this.sesid, this.tableid, indexName);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            Api.JetSetCurrentIndex(this.sesid, this.tableid, indexName);
        }

        /// <summary>
        /// Delete a column and make sure we can't see it afterwards
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(EsentErrorException))]
        public void DeleteColumn()
        {
            string columnName = "column_to_delete";
            Api.JetBeginTransaction(this.sesid);
            var columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Long };
            JET_COLUMNID columnid;
            Api.JetAddColumn(this.sesid, this.tableid, columnName, columndef, null, 0, out columnid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            Api.JetBeginTransaction(this.sesid);
            Api.JetDeleteColumn(this.sesid, this.tableid, columnName);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            Api.JetGetTableColumnInfo(this.sesid, this.tableid, columnName, out columndef);
        }

        /// <summary>
        /// Delete a table and make sure we can't see it afterwards
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(EsentErrorException))]
        public void DeleteTable()
        {
            string tableName = "table_to_delete";
            Api.JetBeginTransaction(this.sesid);
            JET_TABLEID tableid;
            Api.JetCreateTable(this.sesid, this.dbid, tableName, 16, 100, out tableid);
            Api.JetCloseTable(this.sesid, tableid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            Api.JetBeginTransaction(this.sesid);
            Api.JetDeleteTable(this.sesid, this.dbid, tableName);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            Api.JetOpenTable(this.sesid, this.dbid, tableName, null, 0, OpenTableGrbit.None, out tableid);
        }

        #endregion DDL Tests

        #region DML Tests

        /// <summary>
        /// Inserts a record and retrieve it.
        /// </summary>
        [TestMethod]
        public void InsertRecord()
        {
            string s = Any.String;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            this.SetColumnFromString(s);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
            Assert.AreEqual(s, this.RetrieveColumnAsString());
        }

        /// <summary>
        /// Inserts a record and update it.
        /// </summary>
        [TestMethod]
        public void ReplaceRecord()
        {
            string before = Any.String;
            string after = Any.String;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            this.SetColumnFromString(before);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Replace);
            this.SetColumnFromString(after);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            Assert.AreEqual(after, this.RetrieveColumnAsString());
        }

        /// <summary>
        /// Inserts a record and update it. This uses the Transaction helper
        /// class.
        /// </summary>
        [TestMethod]
        public void ReplaceRecordWithTransactionClass()
        {
            string before = Any.String;
            string after = Any.String;

            using (Transaction transaction = new Transaction(this.sesid))
            {
                Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
                this.SetColumnFromString(before);
                this.UpdateAndGotoBookmark();
                transaction.Commit(CommitTransactionGrbit.LazyFlush);
            }

            using (Transaction transaction = new Transaction(this.sesid))
            {
                Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Replace);
                this.SetColumnFromString(after);
                this.UpdateAndGotoBookmark();
                transaction.Commit(CommitTransactionGrbit.LazyFlush);
            }

            Assert.AreEqual(after, this.RetrieveColumnAsString());
        }

        /// <summary>
        /// Inserts a record, update it and rollback the update.
        /// </summary>
        [TestMethod]
        public void ReplaceAndRollback()
        {
            string before = Any.String;
            string after = Any.String;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            this.SetColumnFromString(before);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Replace);
            this.SetColumnFromString(after);
            this.UpdateAndGotoBookmark();
            Api.JetRollback(this.sesid, RollbackTransactionGrbit.None);

            Assert.AreEqual(before, this.RetrieveColumnAsString());
        }

        /// <summary>
        /// Inserts a record, updates it and rollback the transaction.
        /// This uses the Transaction helper class.
        /// </summary>
        [TestMethod]
        public void ReplaceAndRollbackWithTransactionClass()
        {
            string before = Any.String;
            string after = Any.String;

            using (Transaction transaction = new Transaction(this.sesid))
            {
                Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
                this.SetColumnFromString(before);
                this.UpdateAndGotoBookmark();
                transaction.Commit(CommitTransactionGrbit.LazyFlush);
            }

            using (Transaction transaction = new Transaction(this.sesid))
            {
                Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Replace);
                this.SetColumnFromString(after);
                this.UpdateAndGotoBookmark();

                // the transaction isn't committed
            }

            Assert.AreEqual(before, this.RetrieveColumnAsString());
        }

        /// <summary>
        /// Insert a record and delete it.
        /// </summary>
        [TestMethod]
        public void InsertRecordAndDelete()
        {
            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            Api.JetBeginTransaction(this.sesid);
            Api.JetDelete(this.sesid, this.tableid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            try
            {
                var x = this.RetrieveColumnAsString();
                Assert.Fail("Expected an EsentErrorException");
            }
            catch (EsentErrorException ex)
            {
                Assert.AreEqual(JET_err.RecordDeleted, ex.Error);
            }
        }

        /// <summary>
        /// Test JetGetLock()
        /// </summary>
        [TestMethod]
        public void GetLock()
        {
            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            Api.JetBeginTransaction(this.sesid);
            Api.JetGetLock(this.sesid, this.tableid, GetLockGrbit.Read);
            Api.JetGetLock(this.sesid, this.tableid, GetLockGrbit.Write);
            Api.JetRollback(this.sesid, RollbackTransactionGrbit.None);
        }

        /// <summary>
        /// Test that JetGetLock throws an exception when incompatible
        /// locks are requested.
        /// </summary>
        [TestMethod]
        public void GetLockThrowsExceptionOnWriteConflict()
        {
            byte[] bookmark = new byte[Api.BookmarkMost];
            int bookmarkSize;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.JetUpdate(this.sesid, this.tableid, bookmark, bookmark.Length, out bookmarkSize);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            JET_SESID otherSesid;
            JET_DBID otherDbid;
            JET_TABLEID otherTableid;
            Api.JetDupSession(this.sesid, out otherSesid);
            Api.JetOpenDatabase(otherSesid, this.database, null, out otherDbid, OpenDatabaseGrbit.None);
            Api.JetOpenTable(otherSesid, otherDbid, this.table, null, 0, OpenTableGrbit.None, out otherTableid);

            Api.JetGotoBookmark(this.sesid, this.tableid, bookmark, bookmarkSize);
            Api.JetGotoBookmark(otherSesid, otherTableid, bookmark, bookmarkSize);

            Api.JetBeginTransaction(this.sesid);
            Api.JetBeginTransaction(otherSesid);

            Api.JetGetLock(this.sesid, this.tableid, GetLockGrbit.Read);
            try
            {
                Api.JetGetLock(otherSesid, otherTableid, GetLockGrbit.Write);
                Assert.Fail("Expected an EsentErrorException");
            }
            catch (EsentErrorException ex)
            {
                Assert.AreEqual(JET_err.WriteConflict, ex.Error);
            }

            Api.JetRollback(this.sesid, RollbackTransactionGrbit.None);
            Api.JetRollback(otherSesid, RollbackTransactionGrbit.None);

            Api.JetCloseTable(otherSesid, otherTableid);
            Api.JetCloseDatabase(otherSesid, otherDbid, CloseDatabaseGrbit.None);
            Api.JetEndSession(otherSesid, EndSessionGrbit.None);
        }

        #endregion DML Tests

        #region Navigation Tests

        /// <summary>
        /// Inserts a record and retrieve its bookmark
        /// </summary>
        [TestMethod]
        public void JetGetBookmark()
        {
            byte[] expectedBookmark = new byte[256];
            int expectedBookmarkSize;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.JetUpdate(this.sesid, this.tableid, expectedBookmark, expectedBookmark.Length, out expectedBookmarkSize);
            Api.JetGotoBookmark(this.sesid, this.tableid, expectedBookmark, expectedBookmarkSize);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            byte[] actualBookmark = new byte[256];
            int actualBookmarkSize;
            Api.JetGetBookmark(this.sesid, this.tableid, actualBookmark, actualBookmark.Length, out actualBookmarkSize);

            Assert.AreEqual(expectedBookmarkSize, actualBookmarkSize);
            for (int i = 0; i < expectedBookmarkSize; ++i)
            {
                Assert.AreEqual(expectedBookmark[i], actualBookmark[i]);
            }
        }

        /// <summary>
        /// Inserts a record and retrieve its bookmark
        /// </summary>
        [TestMethod]
        public void GetBookmark()
        {
            byte[] expectedBookmark = new byte[256];
            int expectedBookmarkSize;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.JetUpdate(this.sesid, this.tableid, expectedBookmark, expectedBookmark.Length, out expectedBookmarkSize);
            Api.JetGotoBookmark(this.sesid, this.tableid, expectedBookmark, expectedBookmarkSize);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            byte[] actualBookmark = Api.GetBookmark(this.sesid, this.tableid);

            Assert.AreEqual(expectedBookmarkSize, actualBookmark.Length);
            for (int i = 0; i < expectedBookmarkSize; ++i)
            {
                Assert.AreEqual(expectedBookmark[i], actualBookmark[i]);
            }
        }

        /// <summary>
        /// Insert a record and retrieve its key
        /// </summary>
        [TestMethod]
        public void RetrieveKey()
        {
            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            this.UpdateAndGotoBookmark();

            byte[] key = Api.RetrieveKey(this.sesid, this.tableid, RetrieveKeyGrbit.None);
        }

        #endregion Navigation Tests

        #region Helper Methods

        /// <summary>
        /// Update the cursor and goto the returned bookmark.
        /// </summary>
        private void UpdateAndGotoBookmark()
        {
            byte[] bookmark = new byte[Api.BookmarkMost];
            int bookmarkSize;
            Api.JetUpdate(this.sesid, this.tableid, bookmark, bookmark.Length, out bookmarkSize);
            Api.JetGotoBookmark(this.sesid, this.tableid, bookmark, bookmarkSize);
        }

        /// <summary>
        /// Sets the LongText column in the table from a string. An update must be prepared.
        /// </summary>
        /// <param name="s">The string to set.</param>
        private void SetColumnFromString(string s)
        {
            byte[] data = Encoding.Unicode.GetBytes(s);
            Api.JetSetColumn(this.sesid, this.tableid, this.columnidLongText, data, data.Length, SetColumnGrbit.None, null);
        }

        /// <summary>
        /// Returns the value in the LongText column as a string. The cursor must be on a record.
        /// </summary>
        /// <returns>The value of the LongText column as a string.</returns>
        private string RetrieveColumnAsString()
        {
            return Api.RetrieveColumnAsString(this.sesid, this.tableid, this.columnidLongText, Encoding.Unicode);
        }

        #endregion HelperMethods
    }
}
