//-----------------------------------------------------------------------
// <copyright file="DatabaseTests.cs" company="Microsoft Corporation">
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
    /// Test creating, opening and closing databases. 
    /// </summary>
    [TestClass]
    public class DatabaseTests
    {
        /// <summary>
        /// Create a database, attach, open, close and detach
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void CreateAndGrowDatabase()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            Api.JetInit(ref instance);
            try
            {
                string database = Path.Combine(dir, "test.db");

                JET_SESID sesid;
                JET_DBID dbid;
                Api.JetBeginSession(instance, out sesid, String.Empty, String.Empty);
                Api.JetCreateDatabase(sesid, database, String.Empty, out dbid, CreateDatabaseGrbit.None);

                // BUG: ESENT requires that JetGrowDatabase be in a transaction (Win7 and below)
                Api.JetBeginTransaction(sesid);
                int actualPages;
                Api.JetGrowDatabase(sesid, dbid, 512, out actualPages);
                Api.JetCommitTransaction(sesid, CommitTransactionGrbit.None);
                Assert.IsTrue(actualPages >= 512, "Database didn't grow");
            }
            finally
            {
                Api.JetTerm(instance);
                Cleanup.DeleteDirectoryWithRetry(dir);
            }
        }

        /// <summary>
        /// Create a database and set its size
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void CreateDatabaseAndSetSize()
        {
            var test = new DatabaseFileTestHelper("database");
            test.TestSetDatabaseSize();
        }

        /// <summary>
        /// Create a database, attach, open, close and detach
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void CreateAndOpenDatabase()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            Api.JetInit(ref instance);
            try
            {
                string database = Path.Combine(dir, "test.db");

                JET_SESID sesid;
                JET_DBID dbid;
                Api.JetBeginSession(instance, out sesid, String.Empty, String.Empty);
                Api.JetCreateDatabase(sesid, database, String.Empty, out dbid, CreateDatabaseGrbit.None);
                Api.JetCloseDatabase(sesid, dbid, CloseDatabaseGrbit.None);
                Api.JetDetachDatabase(sesid, database);

                Api.JetAttachDatabase(sesid, database, AttachDatabaseGrbit.None);
                Api.JetOpenDatabase(sesid, database, String.Empty, out dbid, OpenDatabaseGrbit.None);
                Api.JetCloseDatabase(sesid, dbid, CloseDatabaseGrbit.None);
                Api.JetDetachDatabase(sesid, database);
            }
            finally
            {
                Api.JetTerm(instance);
                Cleanup.DeleteDirectoryWithRetry(dir);
            }
        }

        /// <summary>
        /// Create a database, open read-only
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void CreateDatabaseAndOpenReadOnly()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            Api.JetInit(ref instance);
            try
            {
                string database = Path.Combine(dir, "test.db");

                JET_SESID sesid;
                JET_DBID dbid;
                Api.JetBeginSession(instance, out sesid, String.Empty, String.Empty);
                Api.JetCreateDatabase(sesid, database, String.Empty, out dbid, CreateDatabaseGrbit.None);
                Api.JetCloseDatabase(sesid, dbid, CloseDatabaseGrbit.None);
                Api.JetDetachDatabase(sesid, database);

                Api.JetAttachDatabase(sesid, database, AttachDatabaseGrbit.ReadOnly);
                Api.JetOpenDatabase(sesid, database, String.Empty, out dbid, OpenDatabaseGrbit.ReadOnly);
                Api.JetCloseDatabase(sesid, dbid, CloseDatabaseGrbit.None);
                Api.JetDetachDatabase(sesid, database);
            }
            finally
            {
                Api.JetTerm(instance);
                Cleanup.DeleteDirectoryWithRetry(dir);
            }
        }
    }
}
