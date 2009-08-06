//-----------------------------------------------------------------------
// <copyright file="ParameterCheckingTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Isam.Esent.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
    /// <summary>
    /// Test for API parameter validation code
    /// </summary>
    [TestClass]
    public class ParameterCheckingTests
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
        private JET_COLUMNID columnid;

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

            // turn off logging and temporary tables so initialization is faster
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesid, String.Empty, String.Empty);
            Api.JetCreateDatabase(this.sesid, this.database, String.Empty, out this.dbid, CreateDatabaseGrbit.None);
            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateTable(this.sesid, this.dbid, this.table, 0, 100, out this.tableid);

            var columndef = new JET_COLUMNDEF
                {
                    coltyp = JET_coltyp.Long,
                    grbit = ColumndefGrbit.ColumnEscrowUpdate
                };

            Api.JetAddColumn(this.sesid, this.tableid, "column", columndef, BitConverter.GetBytes(0), 4, out this.columnid);

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
        [Priority(2)]
        public void VerifyFixtureSetup()
        {
            Assert.AreNotEqual(JET_INSTANCE.Nil, this.instance);
            Assert.AreNotEqual(JET_SESID.Nil, this.sesid);
            Assert.AreNotEqual(JET_COLUMNID.Nil, this.columnid);

            JET_COLUMNDEF columndef;
            Api.JetGetTableColumnInfo(this.sesid, this.tableid, this.columnid, out columndef);
            Assert.AreEqual(JET_coltyp.Long, columndef.coltyp);
        }

        #endregion Setup/Teardown

        #region EsentErrorException tests

        /// <summary>
        /// Check that an exception is thrown when an API calls fails
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(EsentErrorException))]
        public void EsentExceptionIsThrownOnApiError()
        {
            // The session shouldn't be in a transaction so this will
            // generate an error.
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.None);
        }

        /// <summary>
        /// Check that an exception contains the right error code.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void EsentExceptionHasErrorCode()
        {
            try
            {
                JET_TABLEID tableid;
                Api.JetOpenTable(this.sesid, this.dbid, "nosuchtable", null, 0, OpenTableGrbit.None, out tableid);
                Assert.Fail("Should have thrown an exception");
            }
            catch (EsentErrorException ex)
            {
                Assert.AreEqual(JET_err.ObjectNotFound, ex.Error);
            }
        }

        #endregion EsentErrorException tests

        #region System Parameter tests

        /// <summary>
        /// Check that an exception is thrown when JetCreateDatabase gets a 
        /// null database name.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetGetSystemParameterThrowsExceptionWhenMaxParamIsNegative()
        {
            int ignored = 0;
            string value;
            Api.JetGetSystemParameter(this.instance, this.sesid, JET_param.SystemPath, ref ignored, out value, -1);
        }

        #endregion

        #region Database API

        /// <summary>
        /// Check that an exception is thrown when JetCreateDatabase gets a 
        /// null database name.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetCreateDatabaseThrowsExceptionWhenDatabaseNameIsNull()
        {
            JET_DBID dbid;
            Api.JetCreateDatabase(this.sesid, null, null, out dbid, CreateDatabaseGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetAttachDatabase gets a 
        /// null database name.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetAttachDatabaseThrowsExceptionWhenDatabaseNameIsNull()
        {
            Api.JetAttachDatabase(this.sesid, null, AttachDatabaseGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetOpenDatabase gets a 
        /// null database name.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetOpenDatabaseThrowsExceptionWhenDatabaseNameIsNull()
        {
            JET_DBID dbid;
            Api.JetOpenDatabase(this.sesid, null, null, out dbid, OpenDatabaseGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetDetachDatabase gets a 
        /// null database name.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetDetachDatabaseThrowsExceptionWhenDatabaseNameIsNull()
        {
            Api.JetDetachDatabase(this.sesid, null);
        }

        /// <summary>
        /// Check that an exception is thrown when JetOpenTable gets a 
        /// null table name.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetOpenTableThrowsExceptionWhenTableNameIsNull()
        {
            JET_TABLEID tableid;
            Api.JetOpenTable(this.sesid, this.dbid, null, null, 0, OpenTableGrbit.None, out tableid);
        }

        #endregion Database API

        #region DDL

        /// <summary>
        /// Check that an exception is thrown when JetCreateTable gets a 
        /// null table name.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetCreateTableThrowsExceptionWhenTableNameIsNull()
        {
            JET_TABLEID tableid;
            Api.JetCreateTable(this.sesid, this.dbid, null, 0, 100, out tableid);
        }

        /// <summary>
        /// Check that an exception is thrown when JetDeleteTable gets a 
        /// null table name.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetDeleteTableThrowsExceptionWhenTableNameIsNull()
        {
            Api.JetDeleteTable(this.sesid, this.dbid, null);
        }

        /// <summary>
        /// Check that an exception is thrown when JetGetColumnInfo gets a 
        /// null table name.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetGetColumnInfoThrowsExceptionWhenTableNameIsNull()
        {
            JET_COLUMNDEF columndef;
            Api.JetGetColumnInfo(this.sesid, this.dbid, null, "column", out columndef);
        }

        /// <summary>
        /// Check that an exception is thrown when JetGetColumnInfo gets a 
        /// null column name.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetGetColumnInfoThrowsExceptionWhenColumnNameIsNull()
        {
            JET_COLUMNDEF columndef;
            Api.JetGetColumnInfo(this.sesid, this.dbid, "table", null, out columndef);
        }

        /// <summary>
        /// Check that an exception is thrown when JetGetColumnInfo gets a 
        /// null table name.
        /// </summary>
        /// <remarks>
        /// This tests the version of the API that takes a JET_COLUMNLIST.
        /// </remarks>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetGetColumnInfoThrowsExceptionWhenTableNameIsNull2()
        {
            JET_COLUMNLIST columnlist;
            Api.JetGetColumnInfo(this.sesid, this.dbid, null, null, out columnlist);
        }

        #endregion
    }
}
