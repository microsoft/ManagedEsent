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
    using Microsoft.Isam.Esent.Interop.Windows81;
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
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

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

                Assert.IsTrue(actualPages >= databaseSpaceOwned + 100, "Database didn't grow");
            }
            finally
            {
                Api.JetTerm(instance);
                Cleanup.DeleteDirectoryWithRetry(dir);
            }
        }

        #region Windows 8.1
        /// <summary>
        /// Create, resize, and trim a database.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Create, resize, and trim a database")]
        public void CreateResizeAndTrimDatabase()
        {
            if (!EsentVersion.SupportsWindows81Features)
            {
                return;
            }

            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);

            InstanceParameters instanceParameters = new InstanceParameters(instance);
            instanceParameters.EnableShrinkDatabase = ShrinkDatabaseGrbit.On;
            Api.JetInit(ref instance);
            try
            {
                string database = Path.Combine(dir, "CreateAndResizeDatabase.db");

                JET_SESID sesid;
                JET_DBID dbid;
                Api.JetBeginSession(instance, out sesid, string.Empty, string.Empty);

                Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.DbExtensionSize, 256, null);
                Api.JetCreateDatabase(sesid, database, string.Empty, out dbid, CreateDatabaseGrbit.None);

                Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.DbExtensionSize, 1, null);

                int databaseSpaceOwned;
                Api.JetGetDatabaseInfo(sesid, dbid, out databaseSpaceOwned, JET_DbInfo.SpaceOwned);

                // We have to take into account the reserved pages in the database as per the API to get the actual
                // space.
                databaseSpaceOwned += ReservedPages;

                int actualPages;
                Windows8Api.JetResizeDatabase(sesid, dbid, databaseSpaceOwned + 100, out actualPages, ResizeDatabaseGrbit.None);
                EseInteropTestHelper.ConsoleWriteLine("actualPages is {0}.", actualPages);

                Assert.IsTrue(actualPages >= databaseSpaceOwned + 100, "Database didn't grow enough!");

                int actualPagesAfterTrim = 0;
                Windows8Api.JetResizeDatabase(sesid, dbid, 0, out actualPagesAfterTrim, ResizeDatabaseGrbit.None);
                EseInteropTestHelper.ConsoleWriteLine("actualPagesAfterTrim is {0}.", actualPagesAfterTrim);

                Assert.IsTrue(actualPagesAfterTrim < actualPages, "Database didn't shrink!");

                int databaseSizeOnDiskInPages;
                Api.JetGetDatabaseInfo(sesid, dbid, out databaseSizeOnDiskInPages, Windows81DbInfo.FilesizeOnDisk);
                EseInteropTestHelper.ConsoleWriteLine("databaseSizeOnDiskInPages is {0}.", databaseSizeOnDiskInPages);
                Assert.AreEqual(actualPagesAfterTrim, databaseSizeOnDiskInPages);
            }
            finally
            {
                Api.JetTerm(instance);
                Cleanup.DeleteDirectoryWithRetry(dir);
            }
        }

        /// <summary>
        /// Create, resize, and trim a database.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Trim a database fails when disabled.")]
        public void CreateResizeAndTrimDatabaseFailsOnRetail()
        {
            if (!EsentVersion.SupportsWindows81Features)
            {
                return;
            }

            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.DbExtensionSize, 1, null);

            InstanceParameters instanceParameters = new InstanceParameters(instance);
            instanceParameters.EnableShrinkDatabase = ShrinkDatabaseGrbit.Off;
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

                // Growing is still allowed.
                int actualPages;
                Windows8Api.JetResizeDatabase(sesid, dbid, databaseSpaceOwned + 100, out actualPages, ResizeDatabaseGrbit.None);
                EseInteropTestHelper.ConsoleWriteLine("actualPages is {0}.", actualPages);

                Assert.IsTrue(actualPages >= databaseSpaceOwned + 100, "Database didn't grow enough!");

                try
                {
                    int actualPagesAfterTrim = 0;
                    Windows8Api.JetResizeDatabase(sesid, dbid, 0, out actualPagesAfterTrim, ResizeDatabaseGrbit.None);
                    Assert.Fail("JetResizeDatabase() did not return the expected exception: EsentFeatureNotAvailableException.");
                }
                catch (EsentFeatureNotAvailableException)
                {
                    EseInteropTestHelper.ConsoleWriteLine("Caught the expected exception -- EsentFeatureNotAvailableException.");
                }

                // Check the on-disk size.
                int databaseSizeOnDiskInPages;
                Api.JetGetDatabaseInfo(sesid, dbid, out databaseSizeOnDiskInPages, Windows81DbInfo.FilesizeOnDisk);
                EseInteropTestHelper.ConsoleWriteLine("databaseSizeOnDiskInPages is {0}.", databaseSizeOnDiskInPages);
                Assert.AreEqual(actualPages, databaseSizeOnDiskInPages);

                // ...and the logical size (should be the same).
                Api.JetGetDatabaseInfo(sesid, dbid, out databaseSizeOnDiskInPages, JET_DbInfo.Filesize);
                EseInteropTestHelper.ConsoleWriteLine("databaseSizeOnDiskInPages is {0}.", databaseSizeOnDiskInPages);
                Assert.AreEqual(actualPages, databaseSizeOnDiskInPages);
            }
            finally
            {
                Api.JetTerm(instance);
                Cleanup.DeleteDirectoryWithRetry(dir);
            }
        }
        #endregion
    }
}