//-----------------------------------------------------------------------
// <copyright file="TableTests.cs" company="Microsoft Corporation">
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
    /// Tests for the disposable Table object that wraps a JET_TABLEID.
    /// </summary>
    [TestClass]
    public class TableTests
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
            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesid, String.Empty, String.Empty);
            Api.JetCreateDatabase(this.sesid, this.database, String.Empty, out this.dbid, CreateDatabaseGrbit.None);

            Api.JetBeginTransaction(this.sesid);
            JET_TABLEID tableid;
            Api.JetCreateTable(this.sesid, this.dbid, this.tableName, 0, 100, out tableid);
            Api.JetCloseTable(this.sesid, tableid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.None);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
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
            Assert.IsNotNull(this.tableName);
            Assert.AreNotEqual(JET_INSTANCE.Nil, this.instance);
            Assert.AreNotEqual(JET_SESID.Nil, this.sesid);
        }

        #endregion Setup/Teardown

        /// <summary>
        /// Creating a table object should open the table.
        /// </summary>
        [TestMethod]
        public void TestTableCreateOpensTable()
        {
            using (Table table = new Table(this.sesid, this.dbid, this.tableName, OpenTableGrbit.None))
            {
                Assert.AreNotEqual(JET_TABLEID.Nil, table.JetTableid);
            }
        }

        /// <summary>
        /// Creating a table object should set the name.
        /// </summary>
        [TestMethod]
        public void TestTableCreateSetsName()
        {
            using (Table table = new Table(this.sesid, this.dbid, this.tableName, OpenTableGrbit.None))
            {
                Assert.AreEqual(this.tableName, table.Name);
            }
        }

        /// <summary>
        /// A Table object can be implicitly converted to a JET_TABLEID
        /// </summary>
        [TestMethod]
        public void TestTableCanConvertToJetTableid()
        {
            using (Table table = new Table(this.sesid, this.dbid, this.tableName, OpenTableGrbit.None))
            {
                JET_TABLEID tableid = table;
                Assert.AreEqual(tableid, table.JetTableid);
            }
        }

        /// <summary>
        /// Allocate a table and close it.
        /// </summary>
        [TestMethod]
        public void TestTableCloseZeroesJetTableid()
        {
            using (Table table = new Table(this.sesid, this.dbid, this.tableName, OpenTableGrbit.None))
            {
                table.Close();
                Assert.AreEqual(JET_TABLEID.Nil, table.JetTableid);
            }
        }

        /// <summary>
        /// Allocate a table and close it.
        /// </summary>
        [TestMethod]
        public void TestTableCloseZeroesName()
        {
            using (Table table = new Table(this.sesid, this.dbid, this.tableName, OpenTableGrbit.None))
            {
                table.Close();
                Assert.AreEqual(JET_TABLEID.Nil, table.JetTableid);
            }
        }

        /// <summary>
        /// Try to close a disposed table, expecting an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestCloseThrowsExceptionIfTableIsDisposed()
        {
            Table table = new Table(this.sesid, this.dbid, this.tableName, OpenTableGrbit.None);
            table.Dispose();
            table.Close();
        }

        /// <summary>
        /// Try to access the JetTableid property of a disposed table,
        /// expecting an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestJetTableidThrowsExceptionIfTableIsDisposed()
        {
            Table table = new Table(this.sesid, this.dbid, this.tableName, OpenTableGrbit.None);
            table.Dispose();
            var x = table.JetTableid;
        }

        /// <summary>
        /// Try to access the Name property of a disposed table,
        /// expecting an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestNamePropertyThrowsExceptionIfTableIsDisposed()
        {
            Table table = new Table(this.sesid, this.dbid, this.tableName, OpenTableGrbit.None);
            table.Dispose();
            var x = table.Name;
        }
    }
}