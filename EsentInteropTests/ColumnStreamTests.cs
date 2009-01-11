//-----------------------------------------------------------------------
// <copyright file="ColumnStreamTests.cs" company="Microsoft Corporation">
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
    /// Tests for the ColumnStream class
    /// </summary>
    [TestClass]
    public class ColumnStreamTests
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

        #region ColumnStream Tests

        /// <summary>
        /// Test that a ColumnStream supports reading.
        /// </summary>
        [TestMethod]
        public void ColumnStreamSupportsRead()
        {
            using (var t = new Transaction(this.sesid))
            using (var u = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                Assert.IsTrue(stream.CanRead);
            }
        }

        /// <summary>
        /// Test that a ColumnStream supports writing.
        /// </summary>
        [TestMethod]
        public void ColumnStreamSupportsWrite()
        {
            using (var t = new Transaction(this.sesid))
            using (var u = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                Assert.IsTrue(stream.CanWrite);
            }
        }

        /// <summary>
        /// Test that a ColumnStream supports seeking.
        /// </summary>
        [TestMethod]
        public void ColumnStreamSupportsSeek()
        {
            using (var t = new Transaction(this.sesid))
            using (var u = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                Assert.IsTrue(stream.CanSeek);
            }
        }

        /// <summary>
        /// Test setting the length of a ColumnStream.
        /// </summary>
        [TestMethod]
        public void SetColumnStreamLength()
        {
            byte[] bookmark = new byte[Api.BookmarkMost];
            int bookmarkSize;

            long length = 1345;

            using (var transaction = new Transaction(this.sesid))
            using (var update = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                stream.SetLength(length);
                Assert.AreEqual(length, stream.Length);

                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                transaction.Commit(CommitTransactionGrbit.LazyFlush);
            }

            Api.JetGotoBookmark(this.sesid, this.tableid, bookmark, bookmarkSize);
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                Assert.AreEqual(length, stream.Length);
            }
        }

        /// <summary>
        /// Test setting and retrieving a column with the ColumnStream class.
        /// </summary>
        [TestMethod]
        public void SetAndRetrieveColumnStream()
        {
            string s = Any.String;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            using (var writer = new StreamWriter(new ColumnStream(this.sesid, this.tableid, this.columnidLongText)))
            {
                writer.WriteLine(s);
            }

            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            using (var reader = new StreamReader(new ColumnStream(this.sesid, this.tableid, this.columnidLongText)))
            {
                string actual = reader.ReadLine();
                Assert.AreEqual(s, actual);
            }
        }

        /// <summary>
        /// Test that seeking beyond the length of the stream grows the stream.
        /// </summary>
        [TestMethod]
        public void SeekingPastEndOfColumnStreamGrowsStream()
        {
            int offset = 1200;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                stream.Seek(offset, SeekOrigin.Begin);
            }

            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                Assert.AreEqual(offset, stream.Length);
            }
        }

        /// <summary>
        /// Test setting and retrieving a column with the ColumnStream class
        /// and multivalues.
        /// </summary>
        [TestMethod]
        public void SetAndRetrieveMultiValueColumnStream()
        {
            string[] data = { Any.String, Any.String, Any.String, Any.String, Any.String, Any.String };                                

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            for (int i = 0; i < data.Length; ++i)
            {
                var column = new ColumnStream(this.sesid, this.tableid, this.columnidLongText);
                column.Itag = i + 1;
                using (var writer = new StreamWriter(column))
                {
                    writer.WriteLine(data[i]);
                }
            }

            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            for (int i = 0; i < data.Length; ++i)
            {
                var column = new ColumnStream(this.sesid, this.tableid, this.columnidLongText);
                column.Itag = i + 1;
                using (var reader = new StreamReader(column))
                {
                    string actual = reader.ReadLine();
                    Assert.AreEqual(data[i], actual);
                }
            }
        }

        /// <summary>
        /// Trying to seek to an invalid offset generates an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ColumnStreamThrowsExceptionWhenSeekOffsetIsTooLarge()
        {
            using (var t = new Transaction(this.sesid))
            using (var u = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                stream.Seek(0x800000000, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// Setting the size past the maximum LV size generates an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ColumnStreamSetLengthThrowsExceptionWhenLengthIsTooLong()
        {
            using (var t = new Transaction(this.sesid))
            using (var u = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                stream.SetLength(0x800000000);
            }
        }

        /// <summary>
        /// Setting the size to a negative number generates an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ColumnStreamSetLengthThrowsExceptionWhenLengthIsNegative()
        {
            using (var t = new Transaction(this.sesid))
            using (var u = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                stream.SetLength(-1);
            }
        }

        #endregion ColumnStream Tests

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

        #endregion HelperMethods
    }
}
