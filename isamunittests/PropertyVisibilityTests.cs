//-----------------------------------------------------------------------
// <copyright file="PropertyVisibilityTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace IsamUnitTests
{
    using System;
    using System.IO;
    using Microsoft.Database.Isam;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests to verify that ISAM properties are publicly accessible
    /// </summary>
    [TestClass]
    public class PropertyVisibilityTests
    {
        /// <summary>
        /// The directory being used for the database and its files.
        /// </summary>
        private string directory;

        /// <summary>
        /// The path to the database being used by the test.
        /// </summary>
        private string databaseName;

        /// <summary>
        /// The name of the table.
        /// </summary>
        private string tableName;

        /// <summary>
        /// The instance used by the test.
        /// </summary>
        private IsamInstance instance;

        /// <summary>
        /// The session used by the test.
        /// </summary>
        private IsamSession session;

        /// <summary>
        /// Identifies the database used by the test.
        /// </summary>
        private IsamDatabase database;

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// </summary>
        [TestInitialize]
        [Description("Setup for PropertyVisibilityTests")]
        public void Setup()
        {
            this.directory = SetupHelper.CreateRandomDirectory();
            this.databaseName = Path.Combine(this.directory, "database.edb");
            this.tableName = "TestTable";
            this.instance = SetupHelper.CreateNewInstance(this.directory);

            IsamSystemParameters isamSystemParameters = this.instance.IsamSystemParameters;
            isamSystemParameters.Recovery = "off";

            this.session = this.instance.CreateSession();
            this.session.CreateDatabase(this.databaseName);
            this.session.AttachDatabase(this.databaseName);
            this.database = this.session.OpenDatabase(this.databaseName);

            // Create a simple table for testing
            TableDefinition tableDefinition = new TableDefinition(this.tableName);
            tableDefinition.Columns.Add(new ColumnDefinition("TestColumn") { Type = typeof(string) });
            this.database.CreateTable(tableDefinition);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        [Description("Cleanup for PropertyVisibilityTests")]
        public void Teardown()
        {
            this.database?.Dispose();
            this.session?.Dispose();
            this.instance?.Dispose();
            Cleanup.DeleteDirectoryWithRetry(this.directory);
        }

        /// <summary>
        /// Verify that IsamInstance.Inst property is publicly accessible
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that IsamInstance.Inst is public")]
        public void VerifyIsamInstanceInstIsPublic()
        {
            // Should not throw a compile error or runtime error
            JET_INSTANCE inst = this.instance.Inst;
            Assert.IsNotNull(inst);
        }

        /// <summary>
        /// Verify that IsamSession.Sesid property is publicly accessible
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that IsamSession.Sesid is public")]
        public void VerifyIsamSessionSesidIsPublic()
        {
            // Should not throw a compile error or runtime error
            JET_SESID sesid = this.session.Sesid;
            Assert.IsNotNull(sesid);
        }

        /// <summary>
        /// Verify that IsamDatabase.Dbid property is publicly accessible
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that IsamDatabase.Dbid is public")]
        public void VerifyIsamDatabaseDbidIsPublic()
        {
            // Should not throw a compile error or runtime error
            JET_DBID dbid = this.database.Dbid;
            Assert.IsNotNull(dbid);
        }

        /// <summary>
        /// Verify that TableDefinition.IsamSession property is publicly accessible
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that TableDefinition.IsamSession is public")]
        public void VerifyTableDefinitionIsamSessionIsPublic()
        {
            // Get the table definition
            TableDefinition tableDefinition = this.database.Tables[this.tableName];

            // Should not throw a compile error or runtime error
            IsamSession session = tableDefinition.IsamSession;
            Assert.IsNotNull(session);
            Assert.AreEqual(this.session, session);
        }

        /// <summary>
        /// Verify that TableDefinition.Database property is publicly accessible
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that TableDefinition.Database is public")]
        public void VerifyTableDefinitionDatabaseIsPublic()
        {
            // Get the table definition
            TableDefinition tableDefinition = this.database.Tables[this.tableName];

            // Should not throw a compile error or runtime error
            IsamDatabase db = tableDefinition.Database;
            Assert.IsNotNull(db);
            Assert.AreEqual(this.database, db);
        }
    }
}
