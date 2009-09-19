//-----------------------------------------------------------------------
// <copyright file="EmptyTableFixture.cs" company="Microsoft Corporation">
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
    /// Tests that use an empty table fixture
    /// </summary>
    [TestClass]
    public class EmptyTableFixture
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

            // turn off logging so initialization is faster
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesid, String.Empty, String.Empty);
            Api.JetCreateDatabase(this.sesid, this.database, String.Empty, out this.dbid, CreateDatabaseGrbit.None);
            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateTable(this.sesid, this.dbid, this.table, 0, 100, out this.tableid);
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
        [Priority(2)]
        public void VerifyEmptyTableFixtureSetup()
        {
            Assert.AreNotEqual(JET_INSTANCE.Nil, this.instance);
            Assert.AreNotEqual(JET_SESID.Nil, this.sesid);
            Assert.AreNotEqual(JET_TABLEID.Nil, this.tableid);
        }

        #endregion Setup/Teardown

        /// <summary>
        /// Verify that TryMoveFirst returns false when called on an
        /// empty table.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TryMoveFirstOnEmptyTableReturnsFalse()
        {
            Assert.IsFalse(Api.TryMoveFirst(this.sesid, this.tableid));
        }

        /// <summary>
        /// Verify that TryMoveLast returns false when called on an
        /// empty table.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TryMoveLastOnEmptyTableReturnsFalse()
        {
            Assert.IsFalse(Api.TryMoveLast(this.sesid, this.tableid));
        }

        /// <summary>
        /// Verify that TryMoveNext returns false when called on an
        /// empty table.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TryMoveNextOnEmptyTableReturnsFalse()
        {
            Api.MoveBeforeFirst(this.sesid, this.tableid);
            Assert.IsFalse(Api.TryMoveNext(this.sesid, this.tableid));
        }

        /// <summary>
        /// Verify that TryMovePrevious returns false when called on an
        /// empty table.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TryMovePreviousOnEmptyTableReturnsFalse()
        {
            Api.MoveAfterLast(this.sesid, this.tableid);
            Assert.IsFalse(Api.TryMovePrevious(this.sesid, this.tableid));
        }

        /// <summary>
        /// Verify that MoveBeforeFirst does not throw an exception
        /// when the table is empty.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void MoveBeforeFirstOnEmptyTableDoesNotThrowException()
        {
            Api.MoveBeforeFirst(this.sesid, this.tableid);
        }

        /// <summary>
        /// Verify that MoveAfterLast does not throw an exception
        /// when the table is empty.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void MoveAfterLastOnEmptyTableDoesNotThrowException()
        {
            Api.MoveAfterLast(this.sesid, this.tableid);
        }

        /// <summary>
        /// Verify that TrySetIndexRange throws an exception when ESENT
        /// returns an unexpected error;
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(EsentErrorException))]
        public void TrySetIndexRangeThrowsExceptionOnError()
        {
            // No key has been made so this call will fail
            Api.TrySetIndexRange(this.sesid, this.tableid, SetIndexRangeGrbit.RangeInstantDuration);
        }
    }
}
