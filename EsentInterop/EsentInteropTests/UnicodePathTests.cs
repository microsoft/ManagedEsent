//-----------------------------------------------------------------------
// <copyright file="UnicodePathTests.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Isam.Esent.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
    /// <summary>
    /// Test files with Unicode paths (if ESENT supports them)
    /// </summary>
    [TestClass]
    public class UnicodePathTests
    {
        private string directory;

        private string database;

        /// <summary>
        /// Test setup
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.directory = "字会意";
            this.database = Path.Combine(this.directory, "한글.edb");
        }

        /// <summary>
        /// Delete the test directory, if it was created.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            if (Directory.Exists(this.directory))
            {
                Directory.Delete(this.directory, true);
            }
        }

        /// <summary>
        /// Set the system path.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void SetAndGetUnicodeSystemPath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
                return;

            using (var instance = new Instance("unicode"))
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
        public void SetAndGetUnicodeLogPath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
                return;

            using (var instance = new Instance("unicode"))
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
        public void SetAndGetUnicodeTempDbPath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
                return;

            using (var instance = new Instance("unicode"))
            {
                instance.Parameters.TempDirectory = this.directory;
                Assert.IsTrue(instance.Parameters.TempDirectory.Contains(this.directory));
            }
        }

        /// <summary>
        /// Create a database with a unicode path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void CreateDatabaseWithUnicodePath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
                return;

            using (var instance = new Instance("unicode"))
            {
                instance.Parameters.MaxTemporaryTables = 0;
                instance.Parameters.Recovery = false;
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
        /// Detach a database with a unicode path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void DetachDatabaseWithUnicodePath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
                return;

            using (var instance = new Instance("unicode"))
            {
                instance.Parameters.MaxTemporaryTables = 0;
                instance.Parameters.Recovery = false;
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
        /// Attach a database with a unicode path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void AttachDatabaseWithUnicodePath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
                return;

            using (var instance = new Instance("unicode"))
            {
                instance.Parameters.MaxTemporaryTables = 0;
                instance.Parameters.Recovery = false;
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
        /// Open a database with a unicode path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void OpenDatabaseWithUnicodePath()
        {
            if (!EsentVersion.SupportsUnicodePaths)
                return;

            using (var instance = new Instance("unicode"))
            {
                instance.Parameters.MaxTemporaryTables = 0;
                instance.Parameters.Recovery = false;
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
    }
}