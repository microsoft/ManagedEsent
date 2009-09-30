//-----------------------------------------------------------------------
// <copyright file="UpdateTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.IO;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the disposable Update object that wraps
    /// JetPrepareUpdate and JetUpdate.
    /// </summary>
    [TestClass]
    public class UpdateTests
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
        private string tableName;

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
        /// Identifies the table used by the test.
        /// </summary>
        private JET_TABLEID tableid;

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
            this.tableName = "table";
            this.instance = SetupHelper.CreateNewInstance(this.directory);

            // turn off logging so initialization is faster
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesid, String.Empty, String.Empty);
            Api.JetCreateDatabase(this.sesid, this.database, String.Empty, out this.dbid, CreateDatabaseGrbit.None);

            Api.JetBeginTransaction(this.sesid);
            JET_TABLEID tableid;
            Api.JetCreateTable(this.sesid, this.dbid, this.tableName, 0, 100, out tableid);
            Api.JetCloseTable(this.sesid, tableid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.None);

            Api.JetOpenTable(this.sesid, this.dbid, this.tableName, null, 0, OpenTableGrbit.None, out this.tableid);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            Api.JetEndSession(this.sesid, EndSessionGrbit.None);
            Api.JetTerm(this.instance);
            Cleanup.DeleteDirectoryWithRetry(this.directory);
        }

        /// <summary>
        /// Verify that the test class has setup the test fixture properly.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyFixtureSetup()
        {
            Assert.IsNotNull(this.tableName);
            Assert.AreNotEqual(JET_INSTANCE.Nil, this.instance);
            Assert.AreNotEqual(JET_SESID.Nil, this.sesid);
        }

        #endregion Setup/Teardown

        /// <summary>
        /// Start an update and insert the record.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestSaveUpdate()
        {
            Assert.IsFalse(Api.TryMoveFirst(this.sesid, this.tableid));
            using (var update = new Update(this.sesid, this.tableid, JET_prep.Insert))
            {
                update.Save();
            }
            
            // the table shouldn't be empty any more
            Assert.IsTrue(Api.TryMoveFirst(this.sesid, this.tableid));
        }

        /// <summary>
        /// Start an update, insert the record and goto the bookmark.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestSaveUpdateGetsBookmark()
        {
            var bookmark = new byte[SystemParameters.BookmarkMost];
            int bookmarkSize;
            using (var update = new Update(this.sesid, this.tableid, JET_prep.Insert))
            {
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
            }

            Api.JetGotoBookmark(this.sesid, this.tableid, bookmark, bookmarkSize);
        }

        /// <summary>
        /// Start an update, insert the record, save while goto the bookmark.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestSaveAndGotoBookmarkPositionsCursor()
        {
            using (var update = new Update(this.sesid, this.tableid, JET_prep.Insert))
            {
                update.SaveAndGotoBookmark();
            }

            Api.RetrieveKey(this.sesid, this.tableid, RetrieveKeyGrbit.None);
        }

        /// <summary>
        /// Start an update and cancel the insert.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestCancelUpdate()
        {
            Assert.IsFalse(Api.TryMoveFirst(this.sesid, this.tableid));
            using (var update = new Update(this.sesid, this.tableid, JET_prep.Insert))
            {
                update.Cancel();
            }

            // the table should still be empty
            Assert.IsFalse(Api.TryMoveFirst(this.sesid, this.tableid));
        }

        /// <summary>
        /// Start an update and cancel the insert.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestAutoCancelUpdate()
        {
            Assert.IsFalse(Api.TryMoveFirst(this.sesid, this.tableid));
            using (var update = new Update(this.sesid, this.tableid, JET_prep.Insert))
            {
            }

            // the table should still be empty
            Assert.IsFalse(Api.TryMoveFirst(this.sesid, this.tableid));
        }

        /// <summary>
        /// Create an Update with JET_prep.Cancel, expecting an exception
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentException))]
        public void TestPrepCancelThrowsException()
        {
            var update = new Update(this.sesid, this.tableid, JET_prep.Cancel);
        }

        /// <summary>
        /// Call Cancel on a disposed object, expecting an exception.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestCancelThrowsExceptionWhenUpdateIsDisposed()
        {
            var update = new Update(this.sesid, this.tableid, JET_prep.Insert);
            update.Dispose();
            update.Cancel();
        }

        /// <summary>
        /// Call Save on a disposed object, expecting an exception.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestSaveThrowsExceptionWhenUpdateIsDisposed()
        {
            var update = new Update(this.sesid, this.tableid, JET_prep.Insert);
            update.Dispose();
            update.Save();
        }

        /// <summary>
        /// Call Save on a cancelled update, expecting an exception
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSaveThrowsExceptionWhenUpdateIsCancelled()
        {
            var update = new Update(this.sesid, this.tableid, JET_prep.Insert);
            update.Cancel();
            update.Save();
        }

        /// <summary>
        /// Call Cancel on a cancelled update, expecting an exception
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestCancelThrowsExceptionWhenUpdateIsCancelled()
        {
            var update = new Update(this.sesid, this.tableid, JET_prep.Insert);
            update.Cancel();
            update.Cancel();
        }

        /// <summary>
        /// Call SaveAndGotoBookmark on a disposed object, expecting an exception.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestSaveAndGotoBookmarkThrowsExceptionWhenUpdateIsDisposed()
        {
            var update = new Update(this.sesid, this.tableid, JET_prep.Insert);
            update.Dispose();
            update.SaveAndGotoBookmark();
        }

        /// <summary>
        /// Call SaveAndGotoBookmark on a cancelled update, expecting an exception
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSaveAndGotoBookmarkThrowsExceptionWhenUpdateIsCancelled()
        {
            var update = new Update(this.sesid, this.tableid, JET_prep.Insert);
            update.Cancel();
            update.SaveAndGotoBookmark();
        }
    }
}