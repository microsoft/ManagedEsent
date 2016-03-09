//-----------------------------------------------------------------------
// <copyright file="DatabaseObjectTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using Microsoft.Database.Isam;
    using Microsoft.Database.Isam.Config;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Vista;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Database tests
    /// </summary>
    [TestClass]
    public class DatabaseObjectTests
    {
        /// <summary>
        /// Database name.
        /// </summary>
        private const string DbName = "Database.edb";

        /// <summary>
        /// Ese Engine.
        /// </summary>
        private Database database;

        /// <summary>
        /// Used to restore page size back to original value, to avoid screwing up other tests.
        /// </summary>
        private int pagesize;

        /// <summary>
        /// Expected name of the last log file.
        /// </summary>
        private string edbLogFileName;

        /// <summary>
        /// Initialization method. Called for each test.
        /// </summary>
        [TestInitialize]
        public void TestSetup()
        {
            string dummy;
            Api.JetGetSystemParameter(JET_INSTANCE.Nil, JET_SESID.Nil, JET_param.DatabasePageSize, ref this.pagesize, out dummy, 0);

            int legacyFileNames = 0;
            Api.JetGetSystemParameter(JET_INSTANCE.Nil, JET_SESID.Nil, VistaParam.LegacyFileNames, ref legacyFileNames, out dummy, 0);
            this.edbLogFileName = "edb." + (((legacyFileNames & (long)LegacyFileNames.ESE98FileNames) != 0) ? "log" : "jtx");
        }

        /// <summary>
        /// Cleanup for each test.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            Api.JetSetSystemParameter(JET_INSTANCE.Nil, JET_SESID.Nil, JET_param.DatabasePageSize, this.pagesize, null);
        }

        /// <summary>
        /// Create Database in the current directory.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Create Database in the current directory.")]
        public void CreateInCurrentDir()
        {
            using (this.database = new Database(DatabaseObjectTests.DbName))
            {
                Assert.AreNotEqual(this.database.InstanceHandle, JET_INSTANCE.Nil);
            }

#if !MANAGEDESENT_ON_WSA
            Assert.IsTrue(File.Exists(DatabaseObjectTests.DbName));
            Assert.IsTrue(File.Exists(this.edbLogFileName));
#endif
        }

        /// <summary>
        /// Create Database by passing in a database filename to the constructor.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Create Database by passing in a database filename to the constructor.")]
        public void CreateUsingDatabaseFilename()
        {
            string directory = SetupHelper.CreateRandomDirectory();
            string db = Path.Combine(directory, DatabaseObjectTests.DbName);
            using (this.database = new Database(db))
            {
                Assert.AreNotEqual(this.database.InstanceHandle, JET_INSTANCE.Nil);
            }

#if !MANAGEDESENT_ON_WSA
            Assert.IsTrue(File.Exists(db));
            Assert.IsTrue(File.Exists(Path.Combine(directory, this.edbLogFileName)));
#endif
        }

        /// <summary>
        /// Create Database by passing in a config set to the constructor.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Create Database by passing in a config set to the constructor.")]
        public void CreateUsingConfigSet()
        {
            string directory = SetupHelper.CreateRandomDirectory();
            string db = Path.Combine(directory, DatabaseObjectTests.DbName);
            string log = Path.Combine(directory, this.edbLogFileName);

            var engineConfig = new DatabaseConfig()
            {
                DatabasePageSize = 32 * 1024,
                DatabaseFilename = db,
                SystemPath = directory,
                LogFilePath = directory,
                TempPath = directory,
                CircularLog = true,
                LogFileSize = 1024,
                DisplayName = "DatabaseTest",
            };

            using (this.database = new Database(engineConfig))
            {
                Assert.AreNotEqual(this.database.InstanceHandle, JET_INSTANCE.Nil);

                int intParamVal = 0;
                string strParamVal;

                Api.JetGetSystemParameter(this.database.InstanceHandle, JET_SESID.Nil, JET_param.DatabasePageSize, ref intParamVal, out strParamVal, 256);
                Assert.AreEqual(engineConfig.DatabasePageSize, intParamVal);

                Api.JetGetSystemParameter(this.database.InstanceHandle, JET_SESID.Nil, JET_param.CircularLog, ref intParamVal, out strParamVal, 256);
                Assert.IsTrue(engineConfig.CircularLog == (intParamVal == 1));
            }

#if !MANAGEDESENT_ON_WSA
            Assert.AreEqual(engineConfig.LogFileSize * 1024, new FileInfo(log).Length);
            Assert.IsTrue(File.Exists(db));
            Assert.IsTrue(File.Exists(log));
#endif
        }

        /// <summary>
        /// Create Database by passing in a database filename and a config set to the constructor.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Create Database by passing in a database filename and a config set to the constructor.")]
        public void CreateUsingDatabaseFilenameAndConfigSet()
        {
            string directory = SetupHelper.CreateRandomDirectory();
            string db = Path.Combine(directory, DatabaseObjectTests.DbName);
            string log = Path.Combine(directory, this.edbLogFileName);

            var engineConfig = new DatabaseConfig()
            {
                DatabasePageSize = 32 * 1024,
                CircularLog = true,
                LogFileSize = 1024,
                DisplayName = "DatabaseTest",
            };

            using (this.database = new Database(db, engineConfig))
            {
                Assert.AreNotEqual(this.database.InstanceHandle, JET_INSTANCE.Nil);

                int intParamVal = 0;
                string strParamVal;

                Api.JetGetSystemParameter(this.database.InstanceHandle, JET_SESID.Nil, JET_param.DatabasePageSize, ref intParamVal, out strParamVal, 256);
                Assert.AreEqual(engineConfig.DatabasePageSize, intParamVal);

                Api.JetGetSystemParameter(this.database.InstanceHandle, JET_SESID.Nil, JET_param.CircularLog, ref intParamVal, out strParamVal, 256);
                Assert.IsTrue(engineConfig.CircularLog == (intParamVal == 1));
            }

#if !MANAGEDESENT_ON_WSA
            Assert.AreEqual(engineConfig.LogFileSize * 1024, new FileInfo(log).Length);
            Assert.IsTrue(File.Exists(db));
            Assert.IsTrue(File.Exists(log));
#endif
        }

        /// <summary>
        /// Create Database from an externally initialized instance.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Create Database from an externally initialized instance.")]
        public void CreateUsingInitializedInstance()
        {
            string directory = SetupHelper.CreateRandomDirectory();
            string db = Path.Combine(directory, DatabaseObjectTests.DbName);

            JET_INSTANCE instance;
            JET_SESID sesid;
            JET_DBID dbid;

            instance = SetupHelper.CreateNewInstance(directory);
            Api.JetSetSystemParameter(instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetInit(ref instance);
            Api.JetBeginSession(instance, out sesid, string.Empty, string.Empty);
            Api.JetCreateDatabase(sesid, db, string.Empty, out dbid, CreateDatabaseGrbit.None);
            Api.JetCloseDatabase(sesid, dbid, CloseDatabaseGrbit.None);
            Api.JetEndSession(sesid, EndSessionGrbit.None);
            
            var engineConfig = new DatabaseConfig()
            {
                DatabaseFilename = db,
            };

            using (this.database = new Database(instance, false, engineConfig))
            {
                Assert.AreNotEqual(this.database.InstanceHandle, JET_INSTANCE.Nil);
                Assert.AreEqual(this.database.Config.Recovery, "off");
            }

            using (this.database = new Database(instance, true, engineConfig))
            {
                Assert.AreNotEqual(this.database.InstanceHandle, JET_INSTANCE.Nil);
                Assert.AreEqual(this.database.Config.Recovery, "off");
            }

            try
            {
                Api.JetTerm(instance);
                Assert.Fail("EsentInvalidInstanceException expected !");
            }

            // ISSUE-2014/10/28-UmairA - Debug build returns InvalidInstance, retail returns InvalidParameter. JetTerm() should be fixed.
            catch (EsentInvalidInstanceException)
            {
            }
            catch (EsentInvalidParameterException)
            {
            }
        }

        /// <summary>
        /// Create Database by attaching an existing database.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Create Database by attaching an existing database.")]
        public void AttachExisting()
        {
            string directory = SetupHelper.CreateRandomDirectory();
            string db = Path.Combine(directory, DatabaseObjectTests.DbName);
            using (this.database = new Database(db))
            {
                Assert.AreNotEqual(this.database.InstanceHandle, JET_INSTANCE.Nil);
            }

            using (this.database = new Database(db))
            {
                JET_SESID sesid;
                JET_DBID dbid;

                Api.JetBeginSession(this.database.InstanceHandle, out sesid, string.Empty, string.Empty);
                Api.JetOpenDatabase(sesid, db, string.Empty, out dbid, OpenDatabaseGrbit.None);
                Api.JetCloseDatabase(sesid, dbid, CloseDatabaseGrbit.None);
                Api.JetEndSession(sesid, EndSessionGrbit.None);
            }
        }

        /// <summary>
        /// Create using an empty filename.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Create using an empty filename.")]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateWithEmptyDatabaseFilename()
        {
            var db = new Database(string.Empty);
        }

        /// <summary>
        /// Create using a null config set.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Create using a null config set.")]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateWithEmptyConfigSet()
        {
            var db = new Database((IConfigSet)null);
        }

        /// <summary>
        /// Create with conflicting parameters.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Create with conflicting parameters.")]
        [ExpectedException(typeof(ConfigSetMergeException))]
        public void CreateWithConflictingParams()
        {
            string directory = SetupHelper.CreateRandomDirectory();
            string db = Path.Combine(directory, DatabaseObjectTests.DbName);

            var engineConfig = new DatabaseConfig()
            {
                SystemPath = SetupHelper.CreateRandomDirectory(),
                DatabasePageSize = 32 * 1024,
                CircularLog = true,
                LogFileSize = 1024,
                DisplayName = "DatabaseTest",
            };

            using (var database = new Database(db, engineConfig))
            {
            }
        }
    }
}