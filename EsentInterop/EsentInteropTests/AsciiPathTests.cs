//-----------------------------------------------------------------------
// <copyright file="AsciiPathTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.IO;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Implementation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test files with ASCII paths (forcing the version to the XP version)
    /// </summary>
    [TestClass]
    public class AsciiPathTests
    {
        /// <summary>
        /// The directory used for the database and logfiles.
        /// </summary>
        private string directory;

        /// <summary>
        /// The name of the database.
        /// </summary>
        private string database;

        /// <summary>
        /// The saved API, restored after the test.
        /// </summary>
        private IJetApi savedImpl;

        /// <summary>
        /// Test setup
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.directory = "ascii_directory";
            this.database = Path.Combine(this.directory, "ascii.edb");
            this.savedImpl = Api.Impl;
            Api.Impl = new JetApi(Constants.XpVersion);
        }

        /// <summary>
        /// Restore the default implementation and delete the test
        /// directory, if it was created.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            Api.Impl = this.savedImpl;
            if (Directory.Exists(this.directory))
            {
                Cleanup.DeleteDirectoryWithRetry(this.directory);
            }
        }

        /// <summary>
        /// Set the system path.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void SetAndGetAsciiSystemPath()
        {
            using (var instance = new Instance("ascii"))
            {
                instance.Parameters.SystemDirectory = this.directory;
                Assert.IsTrue(instance.Parameters.SystemDirectory.Contains(this.directory));
            }
        }

        /// <summary>
        /// Set the logfile path.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void SetAndGetAsciiLogPath()
        {
            using (var instance = new Instance("ascii"))
            {
                instance.Parameters.LogFileDirectory = this.directory;
                Assert.IsTrue(instance.Parameters.LogFileDirectory.Contains(this.directory));
            }
        }

        /// <summary>
        /// Set the temp database path.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void SetAndGetAsciiTempDbPath()
        {
            using (var instance = new Instance("ascii"))
            {
                instance.Parameters.TempDirectory = this.directory;
                Assert.IsTrue(instance.Parameters.TempDirectory.Contains(this.directory));
            }
        }

        /// <summary>
        /// Create a database with an ASCII path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void CreateDatabaseWithAsciiPath()
        {
            using (var instance = new Instance("ascii"))
            {
                SetupHelper.SetLightweightConfiguration(instance);
                instance.Parameters.CreatePathIfNotExist = true;
                instance.Init();
                using (var session = new Session(instance))
                {
                    JET_DBID dbid;
                    Api.JetCreateDatabase(session, this.database, String.Empty, out dbid, CreateDatabaseGrbit.None);
                    Assert.IsTrue(File.Exists(this.database));
                }
            }
        }

        /// <summary>
        /// Detach a database with an ASCII path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void DetachDatabaseWithAsciiPath()
        {
            using (var instance = new Instance("ascii"))
            {
                SetupHelper.SetLightweightConfiguration(instance);
                instance.Parameters.CreatePathIfNotExist = true;
                instance.Init();
                using (var session = new Session(instance))
                {
                    JET_DBID dbid;
                    Api.JetCreateDatabase(session, this.database, String.Empty, out dbid, CreateDatabaseGrbit.None);
                    Api.JetCloseDatabase(session, dbid, CloseDatabaseGrbit.None);
                    Api.JetDetachDatabase(session, this.database);
                }
            }
        }

        /// <summary>
        /// Attach a database with an ASCII path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void AttachDatabaseWithAsciiPath()
        {
            using (var instance = new Instance("ascii"))
            {
                SetupHelper.SetLightweightConfiguration(instance);
                instance.Parameters.CreatePathIfNotExist = true;
                instance.Init();
                using (var session = new Session(instance))
                {
                    JET_DBID dbid;
                    Api.JetCreateDatabase(session, this.database, String.Empty, out dbid, CreateDatabaseGrbit.None);
                    Api.JetCloseDatabase(session, dbid, CloseDatabaseGrbit.None);
                    Api.JetDetachDatabase(session, this.database);

                    Api.JetAttachDatabase(session, this.database, AttachDatabaseGrbit.None);
                }
            }
        }

        /// <summary>
        /// Open a database with an ASCII path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void OpenDatabaseWithAsciiPath()
        {
            using (var instance = new Instance("ascii"))
            {
                SetupHelper.SetLightweightConfiguration(instance);
                instance.Parameters.CreatePathIfNotExist = true;
                instance.Init();
                using (var session = new Session(instance))
                {
                    JET_DBID dbid;
                    Api.JetCreateDatabase(session, this.database, String.Empty, out dbid, CreateDatabaseGrbit.None);
                    Api.JetCloseDatabase(session, dbid, CloseDatabaseGrbit.None);
                    Api.JetDetachDatabase(session, this.database);

                    Api.JetAttachDatabase(session, this.database, AttachDatabaseGrbit.None);
                    Api.JetOpenDatabase(session, this.database, String.Empty, out dbid, OpenDatabaseGrbit.None);
                }
            }
        }

        /// <summary>
        /// Tests for JetBackupInstance and JetRestoreInstance with an ASCII path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void BackupRestoreDatabaseWithAsciiPath()
        {
            var test = new DatabaseFileTestHelper("database", "backup", false);
            test.TestBackupRestore();
        }

        /// <summary>
        /// Tests for snapshot backups with an ASCII path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SnapshotBackupWithAsciiPath()
        {
            var test = new DatabaseFileTestHelper("database");
            test.TestSnapshotBackup();
        }

        /// <summary>
        /// Tests for streaming backups with an ASCII path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void StreamingBackupWithAsciiPath()
        {
            var test = new DatabaseFileTestHelper("database", "backup", false);
            test.TestStreamingBackup();
        }

        /// <summary>
        /// Tests for JetCompactDatabase with an ASCII path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestJetCompactDatabaseWithAsciiPath()
        {
            var test = new DatabaseFileTestHelper("database");
            test.TestCompactDatabase();
        }

        /// <summary>
        /// Tests for JetCompactDatabase with an ASCII path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestJetSetDatabaseSizeDatabaseWithAsciiPath()
        {
            var test = new DatabaseFileTestHelper("database");
            test.TestSetDatabaseSize();
        }

        /// <summary>
        /// Test JetGetInstanceInfo with ASCII path.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void TestJetGetInstanceInfo()
        {
            const string InstanceName = "MyInstance";
            string database1 = Path.GetFullPath(Path.Combine(this.directory, "instanceinfo1.edb"));
            string database2 = Path.GetFullPath(Path.Combine(this.directory, "instanceinfo2.edb"));
            using (var instance = new Instance(InstanceName))
            {
                instance.Parameters.CreatePathIfNotExist = true;
                instance.Init();
                using (var session = new Session(instance))
                {
                    JET_DBID dbid;
                    Api.JetCreateDatabase(session, database1, String.Empty, out dbid, CreateDatabaseGrbit.None);
                    Api.JetCreateDatabase(session, database2, String.Empty, out dbid, CreateDatabaseGrbit.None);
                    int numInstances;
                    JET_INSTANCE_INFO[] instances;
                    Api.JetGetInstanceInfo(out numInstances, out instances);

                    Assert.AreEqual(1, numInstances);
                    Assert.AreEqual(numInstances, instances.Length);
                    Assert.AreEqual(InstanceName, instances[0].szInstanceName);

                    Assert.AreEqual(2, instances[0].cDatabases);
                    Assert.AreEqual(instances[0].cDatabases, instances[0].szDatabaseFileName.Length);
                    CollectionAssert.AreEquivalent(
                        new[] { database1, database2 },
                        instances[0].szDatabaseFileName);
                }
            }
        }
    }
}