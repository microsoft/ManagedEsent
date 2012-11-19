//-----------------------------------------------------------------------
// <copyright file="UnicodePathTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.IO;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test files with Unicode paths (if ESENT supports them)
    /// </summary>
    [TestClass]
    public partial class UnicodePathTests
    {
        /// <summary>
        /// Unicode directory to contain files.
        /// </summary>
        private string directory;

        /// <summary>
        /// Unicode database name.
        /// </summary>
        private string database;

        /// <summary>
        /// Test setup
        /// </summary>
        [TestInitialize]
        [Description("Setup the UnicodePathsTests fixture")]
        public void Setup()
        {
            this.directory = Path.Combine(EseInteropTestHelper.PathGetRandomFileName(), "字会意");
            Cleanup.DeleteDirectoryWithRetry(this.directory);
            this.database = Path.Combine(this.directory, "한글.edb");
        }

        /// <summary>
        /// Delete the test directory, if it was created.
        /// </summary>
        [TestCleanup]
        [Description("Cleanup the UnicodePathsTests fixture")]
        public void Teardown()
        {
            Cleanup.DeleteDirectoryWithRetry(this.directory);
        }

        /// <summary>
        /// Verify that <see cref="Api.JetCreateInstance"/> supports Unicode strings.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that JetCreateInstance supports Unicode strings.")]
        public void JetCreateInstanceSupportsUnicodeStrings()
        {
            JET_INSTANCE instance;
            if (EsentVersion.SupportsUnicodePaths)
            {
                string instanceName = "한글";
                Api.JetCreateInstance(out instance, instanceName);

#if !MANAGEDESENT_ON_METRO // Not exposed in MSDK
                // Now to verify it worked.
                int numInstances;
                JET_INSTANCE_INFO[] instances;
                Api.JetGetInstanceInfo(out numInstances, out instances);
                Assert.AreEqual(1, numInstances);
                Assert.AreEqual(instanceName, instances[0].szInstanceName);
#endif

                Api.JetTerm(instance);
            }
        }

        /// <summary>
        /// Verify that <see cref="Api.JetCreateInstance2"/> supports Unicode strings.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that JetCreateInstance2 supports Unicode strings.")]
        public void JetCreateInstance2SupportsUnicodeStrings()
        {
            JET_INSTANCE instance;
            if (EsentVersion.SupportsUnicodePaths)
            {
                string instanceName = "한글";
                Api.JetCreateInstance2(out instance, instanceName, "한글-display", CreateInstanceGrbit.None);

#if !MANAGEDESENT_ON_METRO // Not exposed in MSDK
                int numInstances;
                JET_INSTANCE_INFO[] instances;
                Api.JetGetInstanceInfo(out numInstances, out instances);
                Assert.AreEqual(1, numInstances);
                Assert.AreEqual(instanceName, instances[0].szInstanceName);
#endif

                Api.JetTerm(instance);
            }
        }

        /// <summary>
        /// When a string can't be converted to ASCII for an API call
        /// an exception should be generated. If this code is converted
        /// to use the Unicode version of all APIs this test should
        /// start failing.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Check that ArgumentException is thrown for unmappable characters")]
        public void ApiThrowsArgumentExceptionOnUnmappableChar()
        {
            string directory = SetupHelper.CreateRandomDirectory();
            string database = Path.Combine(directory, "test.db");

            using (var instance = new Instance("Windows7Createindexes"))
            {
                instance.Parameters.Recovery = false;
                instance.Parameters.NoInformationEvent = true;
                instance.Parameters.MaxTemporaryTables = 0;
                instance.Parameters.TempDirectory = directory;
                instance.Init();
                using (var session = new Session(instance))
                {
                    JET_DBID dbid;
                    Api.JetCreateDatabase(session, database, string.Empty, out dbid, CreateDatabaseGrbit.None);
                    using (var transaction = new Transaction(session))
                    {
                        JET_TABLEID tableid;
                        Api.JetCreateTable(session, dbid, "table", 0, 100, out tableid);
                        JET_COLUMNID columnid;

                        try
                        {
                            Api.JetAddColumn(
                                session,
                                tableid,
                                "한글",
                                new JET_COLUMNDEF { coltyp = JET_coltyp.Long },
                                null,
                                0,
                                out columnid);
                            Assert.Fail("An exception should have been thrown!");
                        }
#if MANAGEDESENT_SUPPORTS_ANSI
                        // The ArgumentException is thrown by the marshalling layer.
                        catch (ArgumentException)
                        {
                        }
#else
                        // What's the more precise error code to catch?
                        catch (Microsoft.Isam.Esent.EsentException)
                        {
                        }
#endif

                        transaction.Commit(CommitTransactionGrbit.LazyFlush);
                    }
                }
            }

            Cleanup.DeleteDirectoryWithRetry(directory);
        }

        /// <summary>
        /// Set the system path.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Set and get a Unicode system path")]
        public void SetAndGetUnicodeSystemPath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
            {
                return;
            }

            using (var instance = new Instance("unicodesystempath"))
            {
                instance.Parameters.SystemDirectory = this.directory;
                StringAssert.Contains(instance.Parameters.SystemDirectory, this.directory);
            }
        }

        /// <summary>
        /// Set the logfile path.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Set and get a Unicode log path")]
        public void SetAndGetUnicodeLogPath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
            {
                return;
            }

            using (var instance = new Instance("unicodelogpath"))
            {
                instance.Parameters.LogFileDirectory = this.directory;
                StringAssert.Contains(instance.Parameters.LogFileDirectory, this.directory);
            }
        }

        /// <summary>
        /// Set the temp database path.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Set and get a Unicode temp directory path")]
        public void SetAndGetUnicodeTempDbPath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
            {
                return;
            }

            using (var instance = new Instance("unicodetempdir"))
            {
                instance.Parameters.TempDirectory = this.directory;
                StringAssert.Contains(instance.Parameters.TempDirectory, this.directory);
            }
        }

        /// <summary>
        /// Create a database with a unicode path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Create a database with a Unicode path")]
        public void CreateDatabaseWithUnicodePath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
            {
                return;
            }

            using (var instance = new Instance("unicodedbcreate"))
            {
                SetupHelper.SetLightweightConfiguration(instance);
                instance.Parameters.CreatePathIfNotExist = true;
                instance.Init();
                using (var session = new Session(instance))
                {
                    JET_DBID dbid;
                    Api.JetCreateDatabase(session, this.database, string.Empty, out dbid, CreateDatabaseGrbit.None);
                    Assert.IsTrue(EseInteropTestHelper.FileExists(this.database));
                }
            }
        }

        /// <summary>
        /// Create a database with a unicode path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Call JetCreateDatabase2 with a Unicode path")]
        public void CreateDatabase2WithUnicodePath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
            {
                return;
            }

            using (var instance = new Instance("unicodedbcreate"))
            {
                SetupHelper.SetLightweightConfiguration(instance);
                instance.Parameters.CreatePathIfNotExist = true;
                instance.Init();
                using (var session = new Session(instance))
                {
                    JET_DBID dbid;
                    Api.JetCreateDatabase2(session, this.database, 512, out dbid, CreateDatabaseGrbit.None);
                    Assert.IsTrue(EseInteropTestHelper.FileExists(this.database));
                    Assert.IsTrue(EseInteropTestHelper.FileExists(this.database));
                }
            }
        }

        /// <summary>
        /// Detach a database with a unicode path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Detach a database with a Unicode path")]
        public void DetachDatabaseWithUnicodePath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
            {
                return;
            }

            using (var instance = new Instance("unicodedbdetach"))
            {
                SetupHelper.SetLightweightConfiguration(instance);
                instance.Parameters.CreatePathIfNotExist = true;
                instance.Init();
                using (var session = new Session(instance))
                {
                    JET_DBID dbid;
                    Api.JetCreateDatabase(session, this.database, string.Empty, out dbid, CreateDatabaseGrbit.None);
                    Api.JetCloseDatabase(session, dbid, CloseDatabaseGrbit.None);
                    Api.JetDetachDatabase(session, this.database);
                }
            }
        }

        /// <summary>
        /// Attach a database with a unicode path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Attach a database with a Unicode path")]
        public void AttachDatabaseWithUnicodePath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
            {
                return;
            }

            using (var instance = new Instance("unicodedbattach"))
            {
                SetupHelper.SetLightweightConfiguration(instance);
                instance.Parameters.CreatePathIfNotExist = true;
                instance.Init();
                using (var session = new Session(instance))
                {
                    JET_DBID dbid;
                    Api.JetCreateDatabase(session, this.database, string.Empty, out dbid, CreateDatabaseGrbit.None);
                    Api.JetCloseDatabase(session, dbid, CloseDatabaseGrbit.None);
                    Api.JetDetachDatabase(session, this.database);

                    Api.JetAttachDatabase(session, this.database, AttachDatabaseGrbit.None);
                }
            }
        }

        /// <summary>
        /// Use JetAttachDatabase2 on a database with a unicode path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Use JetAttachDatabase2 on a database with a unicode path")]
        public void AttachDatabaseWithUnicodePath2()
        {
            if (!EsentVersion.SupportsUnicodePaths)
            {
                return;
            }

            using (var instance = new Instance("unicodedbattach2"))
            {
                SetupHelper.SetLightweightConfiguration(instance);
                instance.Parameters.CreatePathIfNotExist = true;
                instance.Init();
                using (var session = new Session(instance))
                {
                    JET_DBID dbid;
                    Api.JetCreateDatabase(session, this.database, string.Empty, out dbid, CreateDatabaseGrbit.OverwriteExisting);
                    Api.JetCloseDatabase(session, dbid, CloseDatabaseGrbit.None);
                    Api.JetDetachDatabase(session, this.database);

                    Api.JetAttachDatabase2(session, this.database, 512, AttachDatabaseGrbit.None);
                }
            }
        }

        /// <summary>
        /// Open a database with a unicode path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Open a database with a Unicode path")]
        public void OpenDatabaseWithUnicodePath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
            {
                return;
            }

            using (var instance = new Instance("unicodedbopen"))
            {
                SetupHelper.SetLightweightConfiguration(instance);
                instance.Parameters.CreatePathIfNotExist = true;
                instance.Init();
                using (var session = new Session(instance))
                {
                    JET_DBID dbid;
                    Api.JetCreateDatabase(session, this.database, string.Empty, out dbid, CreateDatabaseGrbit.OverwriteExisting);
                    Api.JetCloseDatabase(session, dbid, CloseDatabaseGrbit.None);
                    Api.JetDetachDatabase(session, this.database);

                    Api.JetAttachDatabase(session, this.database, AttachDatabaseGrbit.None);
                    Api.JetOpenDatabase(session, this.database, string.Empty, out dbid, OpenDatabaseGrbit.None);
                }
            }
        }

#if !MANAGEDESENT_ON_METRO
        /// <summary>
        /// Backup and restore a database using unicode paths.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Backup and restore a database with a Unicode path")]
        public void BackupRestoreDatabaseWithUnicodePath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
            {
                return;
            }

            var test = new DatabaseFileTestHelper(this.directory, "한글", false);
            test.TestBackupRestore();
        }

        /// <summary>
        /// Tests for streaming backup using unicode paths.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Perform a streaming backup of a database with a Unicode path")]
        public void StreamingBackupWithUnicodePath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
            {
                return;
            }

            var test = new DatabaseFileTestHelper(this.directory, "한글", false);
            test.TestStreamingBackup();
        }

        /// <summary>
        /// Tests for snapshot backup using unicode paths.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Perform a snapshot backup of a database with a Unicode path")]
        public void SnapshotBackupWithUnicodePath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
            {
                return;
            }

            var test = new DatabaseFileTestHelper(this.directory);
            test.TestSnapshotBackup();
        }

        /// <summary>
        /// Tests for snapshot backup using unicode paths and Server 2K3 APIs.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Perform a snapshot backup of a database with a Unicode path and Server 2K3 APIs")]
        public void SnapshotBackupWithUnicodePathServer2K3Apis()
        {
            if (!EsentVersion.SupportsUnicodePaths)
            {
                return;
            }

            var test = new DatabaseFileTestHelper(this.directory);
            test.TestSnapshotBackupServer2003();
        }

        /// <summary>
        /// Tests for snapshot backup using unicode paths and Vista APIs.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Perform a snapshot backup of a database with a Unicode path and Vista APIs")]
        public void SnapshotBackupWithUnicodePathVistaApis()
        {
            if (!EsentVersion.SupportsUnicodePaths)
            {
                return;
            }

            var test = new DatabaseFileTestHelper(this.directory);
            test.TestSnapshotBackupVista();
        }
#endif // !MANAGEDESENT_ON_METRO

        /// <summary>
        /// Tests for JetInit3 using unicode paths.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Perform a snapshot backup of a database with a Unicode path and Vista APIs")]
        public void AlternatePathRecoveryWithJetInit3()
        {
            if (!EsentVersion.SupportsUnicodePaths)
            {
                return;
            }

            var test = new DatabaseFileTestHelper(Path.Combine(this.directory, EseInteropTestHelper.PathGetRandomFileName()));
            test.TestJetInit3();
        }

#if !MANAGEDESENT_ON_METRO
        /// <summary>
        /// Tests for snapshot backup using unicode paths and Win7 APIs.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Perform a snapshot backup of a database with a Unicode path and Win7 APIs")]
        public void SnapshotBackupWithUnicodePathWin7Apis()
        {
            if (!EsentVersion.SupportsUnicodePaths)
            {
                return;
            }

            var test = new DatabaseFileTestHelper(this.directory);
            test.TestSnapshotBackupWin7();
        }

        /// <summary>
        /// Compact a database using unicode paths.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Compact a database with a Unicode path")]
        public void TestJetCompactDatabaseWithUnicodePath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
            {
                return;
            }

            var test = new DatabaseFileTestHelper(Path.Combine(this.directory, EseInteropTestHelper.PathGetRandomFileName()));
            test.TestCompactDatabase();
        }

        /// <summary>
        /// Tests for JetSetDatabaseSize with a unicode path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test JetSetDatabaseSize on a database with a Unicode path")]
        public void TestJetSetDatabaseSizeDatabaseWithUnicodePath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
            {
                return;
            }

            var test = new DatabaseFileTestHelper(Path.Combine(this.directory, EseInteropTestHelper.PathGetRandomFileName()));
            test.TestSetDatabaseSize();
        }
#endif // !MANAGEDESENT_ON_METRO

        /// <summary>
        /// Test JetGetDatabaseFileInfo with a Unicode path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test JetGetDatabaseFileInfo with a Unicode path.")]
        public void TestJetGetDatabaseFileInfoWithUnicodePath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
            {
                return;
            }

            var test = new DatabaseFileTestHelper(Path.Combine(this.directory, EseInteropTestHelper.PathGetRandomFileName()));
            test.TestGetDatabaseFileInfo();
        }

        /// <summary>
        /// Test JetGetDatabaseInfo with a Unicode path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test JetGetDatabaseInfo with a Unicode path.")]
        public void TestJetGetDatabaseInfoWithUnicodePath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
            {
                return;
            }

            var test = new DatabaseFileTestHelper(Path.Combine(this.directory, EseInteropTestHelper.PathGetRandomFileName()));
            test.TestGetDatabaseInfo();
        }

#if !MANAGEDESENT_ON_METRO // Not exposed in MSDK
        /// <summary>
        /// Test JetGetInstanceInfo with a unicode path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test JetGetInstanceInfo on a database with a Unicode path")]
        public void TestJetGetInstanceInfoWithUnicodePath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
            {
                return;
            }

            const string InstanceName = "unicodegetinstanceinfo";
            using (var instance = new Instance(InstanceName))
            {
                // Don't turn off logging -- JetGetInstanceInfo only returns information for
                // databases that have logging on.
                instance.Parameters.LogFileSize = 384; // 384Kb
                instance.Parameters.NoInformationEvent = true;
                instance.Parameters.MaxTemporaryTables = 0;
                instance.Parameters.CreatePathIfNotExist = true;
                instance.Parameters.LogFileDirectory = this.directory;
                instance.Parameters.SystemDirectory = this.directory;
                instance.Init();
                using (var session = new Session(instance))
                {
                    JET_DBID dbid;
                    Api.JetCreateDatabase(session, this.database, string.Empty, out dbid, CreateDatabaseGrbit.OverwriteExisting);
                    int numInstances;
                    JET_INSTANCE_INFO[] instances;
                    Api.JetGetInstanceInfo(out numInstances, out instances);

                    Assert.AreEqual(1, numInstances);
                    Assert.AreEqual(numInstances, instances.Length);
                    Assert.AreEqual(InstanceName, instances[0].szInstanceName);

                    Assert.AreEqual(1, instances[0].cDatabases);
                    Assert.AreEqual(instances[0].cDatabases, instances[0].szDatabaseFileName.Count);
                    Assert.AreEqual(Path.GetFullPath(this.database), instances[0].szDatabaseFileName[0]);
                }
            }
        }
#endif // !MANAGEDESENT_ON_METRO
    }
}