//-----------------------------------------------------------------------
// <copyright file="Windows8DatabaseTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.IO;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Windows8;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test creating, opening and closing databases. 
    /// </summary>
    public partial class DatabaseTests
    {
        /// <summary>
        /// Number of reserved pages not taken into account by JetGetDatabaseInfo called
        /// with JET_DbInfo.SpaceOwned.
        /// </summary>
        private const int ReservedPages = 2;

        /// <summary>
        /// Create and resize a database
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Create and resize a database")]
        public void CreateAndResizeDatabase()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.DbExtensionSize, 1, null);
            Api.JetInit(ref instance);
            try
            {
                string database = Path.Combine(dir, "CreateAndResizeDatabase.db");

                JET_SESID sesid;
                JET_DBID dbid;
                Api.JetBeginSession(instance, out sesid, string.Empty, string.Empty);
                Api.JetCreateDatabase(sesid, database, string.Empty, out dbid, CreateDatabaseGrbit.None);

                int databaseSpaceOwned;
                Api.JetGetDatabaseInfo(sesid, dbid, out databaseSpaceOwned, JET_DbInfo.SpaceOwned);

                // We have to take into account the reserved pages in the database as per the API to get the actual
                // space.
                databaseSpaceOwned += ReservedPages;

                int actualPages;
                Windows8Api.JetResizeDatabase(sesid, dbid, databaseSpaceOwned + 100, out actualPages, ResizeDatabaseGrbit.None);
                EseInteropTestHelper.ConsoleWriteLine("{0}", actualPages);

                Assert.AreEqual(databaseSpaceOwned + 100, actualPages, "Database didn't grow");
            }
            finally
            {
                Api.JetTerm(instance);
                Cleanup.DeleteDirectoryWithRetry(dir);
            }
        }
    }
}