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
                Directory.Delete(this.directory, true);
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
        /// Detach a database with an ASCII path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void DetachDatabaseWithAsciiPath()
        {
            using (var instance = new Instance("ascii"))
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
        /// Attach a database with an ASCII path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void AttachDatabaseWithAsciiPath()
        {
            using (var instance = new Instance("ascii"))
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
        /// Open a database with an ASCII path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void OpenDatabaseWithAsciiPath()
        {
            using (var instance = new Instance("ascii"))
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