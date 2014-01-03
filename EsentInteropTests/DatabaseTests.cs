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
    public partial class DatabaseTests
    {
        #region Setup/Teardown

        /// <summary>
        /// Verifies no instances are leaked.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            SetupHelper.CheckProcessForInstanceLeaks();
        }

        #endregion

#if !MANAGEDESENT_ON_WSA // Not exposed in MSDK
        /// <summary>
        /// Create a database, attach, open, close and detach
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Create a database, attach, open, close and detach")]
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
                Api.JetBeginSession(instance, out sesid, string.Empty, string.Empty);
                Api.JetCreateDatabase(sesid, database, string.Empty, out dbid, CreateDatabaseGrbit.None);

                // BUG: ESENT requires that JetGrowDatabase be in a transaction (Win7 and below)
                using (Transaction transaction = new Transaction(sesid))
                {
                    int actualPages;
                    Api.JetGrowDatabase(sesid, dbid, 512, out actualPages);
                    transaction.Commit(CommitTransactionGrbit.None);
                    Assert.IsTrue(actualPages >= 512, "Database didn't grow");
                }
            }
#if MANAGEDESENT_ON_CORECLR
            catch (EsentFeatureNotAvailableException)
            {
                // JetGrowDatabase does not work on CoreClr.
            }
#endif
            finally
            {
                Api.JetTerm(instance);
                Cleanup.DeleteDirectoryWithRetry(dir);
            }
        }

        /// <summary>
        /// JetGrowDatabase throws exception when desired pages is negative.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("JetGrowDatabase throws exception when desired pages is negative")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetGrowDatabaseThrowsExceptionWhenDesiredPagesIsNegative()
        {
            bool transactionStarted = false;
            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            Api.JetInit(ref instance);
            string database = Path.Combine(dir, "test.db");
            
            JET_SESID sesid;
            JET_DBID dbid;
            Api.JetBeginSession(instance, out sesid, string.Empty, string.Empty);

            try
            {
                Api.JetCreateDatabase(sesid, database, string.Empty, out dbid, CreateDatabaseGrbit.None);

                // BUG: ESENT requires that JetGrowDatabase be in a transaction (Win7 and below)
                Api.JetBeginTransaction(sesid);
                transactionStarted = true;

                int actualPages;
                Api.JetGrowDatabase(sesid, dbid, -10, out actualPages);
                Api.JetCommitTransaction(sesid, CommitTransactionGrbit.None);
            }
            finally
            {
                if (transactionStarted)
                {
                    Api.JetRollback(sesid, RollbackTransactionGrbit.None);
                }

                Api.JetTerm(instance);
                Cleanup.DeleteDirectoryWithRetry(dir);
            }
        }

        /// <summary>
        /// JetSetDatabaseSize throws exception when desired pages is negative.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("JetSetDatabaseSize throws exception when desired pages is negative")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetSetDatabaseSizeThrowsExceptionWhenDesiredPagesIsNegative()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            Api.JetInit(ref instance);
            string database = Path.Combine(dir, "test.db");
            
            JET_SESID sesid;
            JET_DBID dbid;
            Api.JetBeginSession(instance, out sesid, string.Empty, string.Empty);

            try
            {
                 Api.JetCreateDatabase(sesid, database, string.Empty, out dbid, CreateDatabaseGrbit.None);

                 int actualPages;
             
                 Api.JetSetDatabaseSize(sesid, database, -1, out actualPages);
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
        [Description("Create a database and set its size")]
        public void CreateDatabaseAndSetSize()
        {
            var test = new DatabaseFileTestHelper(Path.Combine(EseInteropTestHelper.PathGetRandomFileName(), "database"));
            test.TestSetDatabaseSize();
        }
#endif // !MANAGEDESENT_ON_WSA

        /// <summary>
        /// Create a database, attach, open, close and detach
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Create a database, attach, open, close and detach")]
        public void CreateAndOpenDatabase()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetInit(ref instance);
            try
            {
                string database = Path.Combine(dir, "test.db");

                JET_SESID sesid;
                JET_DBID dbid;
                Api.JetBeginSession(instance, out sesid, string.Empty, string.Empty);
                Api.JetCreateDatabase(sesid, database, string.Empty, out dbid, CreateDatabaseGrbit.None);
                Api.JetCloseDatabase(sesid, dbid, CloseDatabaseGrbit.None);
                Api.JetDetachDatabase2(sesid, database, DetachDatabaseGrbit.None);

                Api.JetAttachDatabase(sesid, database, AttachDatabaseGrbit.None);
                Api.JetOpenDatabase(sesid, database, string.Empty, out dbid, OpenDatabaseGrbit.None);
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
        [Description("Create a database, open read-only")]
        public void CreateDatabaseAndOpenReadOnly()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetInit(ref instance);
            try
            {
                string database = Path.Combine(dir, "test.db");

                JET_SESID sesid;
                JET_DBID dbid;
                Api.JetBeginSession(instance, out sesid, string.Empty, string.Empty);
                Api.JetCreateDatabase(sesid, database, string.Empty, out dbid, CreateDatabaseGrbit.None);
                Api.JetCloseDatabase(sesid, dbid, CloseDatabaseGrbit.None);
                Api.JetDetachDatabase(sesid, database);

                Api.JetAttachDatabase(sesid, database, AttachDatabaseGrbit.ReadOnly);
                Api.JetOpenDatabase(sesid, database, string.Empty, out dbid, OpenDatabaseGrbit.ReadOnly);
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
        /// Create a database and attach with JetAttachDatabase2.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Create a database and attach with JetAttachDatabase2")]
        public void CreateAndOpenDatabaseWithMaxSize()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetInit(ref instance);
            try
            {
                string database = Path.Combine(dir, "test.db");

                JET_SESID sesid;
                JET_DBID dbid;
                Api.JetBeginSession(instance, out sesid, string.Empty, string.Empty);
                Api.JetCreateDatabase(sesid, database, string.Empty, out dbid, CreateDatabaseGrbit.None);
                Api.JetCloseDatabase(sesid, dbid, CloseDatabaseGrbit.None);
                Api.JetDetachDatabase(sesid, database);

                Api.JetAttachDatabase2(sesid, database, 512, AttachDatabaseGrbit.None);
            }
            finally
            {
                Api.JetTerm(instance);
                Cleanup.DeleteDirectoryWithRetry(dir);
            }
        }
    }
}