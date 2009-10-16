//-----------------------------------------------------------------------
// <copyright file="DdlRenameTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Vista;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Basic Api tests
    /// </summary>
    [TestClass]
    public class DdlRenameTests
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
            this.instance = SetupHelper.CreateNewInstance(this.directory);

            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.PageTempDBMin, 0, null);
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesid, String.Empty, String.Empty);
            Api.JetCreateDatabase(this.sesid, this.database, String.Empty, out this.dbid, CreateDatabaseGrbit.None);
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

        #endregion

        /// <summary>
        /// Test JetRenameTable.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test JetRenameTable.")]
        public void TestJetRenameTable()
        {
            JET_TABLEID tableid;
            Api.JetCreateTable(this.sesid, this.dbid, "table", 1, 100, out tableid);
            Api.JetCloseTable(this.sesid, tableid);
            Api.JetRenameTable(this.sesid, this.dbid, "table", "newtable");
            Api.JetOpenTable(this.sesid, this.dbid, "newtable", null, 0, OpenTableGrbit.None, out tableid);
        }

        /// <summary>
        /// Test JetRenameColumn.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test JetRenameColumn.")]
        public void TestJetRenameColumn()
        {
            JET_TABLEID tableid;
            Api.JetCreateTable(this.sesid, this.dbid, "table", 1, 100, out tableid);
            JET_COLUMNID columnid;
            Api.JetAddColumn(this.sesid, tableid, "old", new JET_COLUMNDEF { coltyp = JET_coltyp.Long }, null, 0, out columnid);
            Api.JetRenameColumn(this.sesid, tableid, "old", "new", RenameColumnGrbit.None);
            Api.GetTableColumnid(this.sesid, tableid, "new");
            Api.JetCloseTable(this.sesid, tableid);
        }
    }
}
