//-----------------------------------------------------------------------
// <copyright file="VistaCompatabilityTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
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
    /// Test the Api class functionality when we have an Vista version of Esent.
    /// </summary>
    [TestClass]
    public class VistaCompatabilityTests
    {
        /// <summary>
        /// The saved API, replaced when finished.
        /// </summary>
        private IJetApi savedImpl;

        /// <summary>
        /// Setup the mock object repository.
        /// </summary>
        [TestInitialize]
        [Description("Setup the VistaCompatabilityTests fixture")]
        public void Setup()
        {
            this.savedImpl = Api.Impl;
            Api.Impl = new JetApi(Constants.VistaVersion);
        }

        /// <summary>
        /// Cleanup after the test.
        /// </summary>
        [TestCleanup]
        [Description("Cleanup the VistaCompatabilityTests fixture")]
        public void Teardown()
        {
            Api.Impl = this.savedImpl;
        }

        /// <summary>
        /// Verify that the Vista version of ESENT does support
        /// large keys.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that the Vista version of ESENT does support large keys")]
        public void VerifyVistaDoesSupportLargeKeys()
        {
            Assert.IsTrue(EsentVersion.SupportsLargeKeys);
        }

        /// <summary>
        /// Verify that the Vista version of ESENT does support
        /// Windows Server 2003 features.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that the Vista version of ESENT does support Windows Server 2003 features")]
        public void VerifyVistaDoesSupportServer2003Features()
        {
            Assert.IsTrue(EsentVersion.SupportsServer2003Features);
        }

        /// <summary>
        /// Verify that the Vista version of ESENT does support
        /// Unicode paths.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that the Vista version of ESENT does support Unicode paths")]
        public void VerifyVistaDoesSupportUnicodePaths()
        {
            Assert.IsTrue(EsentVersion.SupportsUnicodePaths);
        }

        /// <summary>
        /// Verify that the Vista version of ESENT does support
        /// Windows Vista features.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that the Vista version of ESENT does support Windows Vista features")]
        public void VerifyVistaDoesSupportVistaFeatures()
        {
            Assert.IsTrue(EsentVersion.SupportsVistaFeatures);
        }

        /// <summary>
        /// Verify that the Vista version of ESENT doesn't support
        /// Windows 7 features.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that the Vista version of ESENT doesn't support Windows 7 features")]
        public void VerifyVistaDoesNotSupportWindows7Features()
        {
            Assert.IsFalse(EsentVersion.SupportsWindows7Features);
        }

        /// <summary>
        /// Use JetGetDatabaseFileInfo on Vista to test the compatability path for JET_DBINFOMISC.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Use JetGetDatabaseFileInfo on Vista to test the compatability path")]
        public void GetDatabaseFileInfoOnVista()
        {
            string directory = SetupHelper.CreateRandomDirectory();
            string database = Path.Combine(directory, "test.db");

            using (var instance = new Instance("VistaJetGetDatabaseFileInfo"))
            {
                SetupHelper.SetLightweightConfiguration(instance);
                instance.Init();
                using (var session = new Session(instance))
                {
                    JET_DBID dbid;
                    Api.JetCreateDatabase(session, database, String.Empty, out dbid, CreateDatabaseGrbit.None);
                }
            }

            JET_DBINFOMISC dbinfomisc;
            Api.JetGetDatabaseFileInfo(database, out dbinfomisc, JET_DbInfo.Misc);
            Assert.AreEqual(SystemParameters.DatabasePageSize, dbinfomisc.cbPageSize);

            Cleanup.DeleteDirectoryWithRetry(directory);
        }

        /// <summary>
        /// Use JetCreateIndex2 on Vista to test the compatability path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Use JetCreateIndex2 on Vista to test the compatability path")]
        public void CreateIndexesOnVista()
        {
            string directory = SetupHelper.CreateRandomDirectory();
            string database = Path.Combine(directory, "test.db");

            using (var instance = new Instance("VistaCreateindexes"))
            {
                instance.Parameters.Recovery = false;
                instance.Parameters.NoInformationEvent = true;
                instance.Parameters.MaxTemporaryTables = 0;
                instance.Parameters.TempDirectory = directory;
                instance.Init();
                using (var session = new Session(instance))
                {
                    JET_DBID dbid;
                    Api.JetCreateDatabase(session, database, String.Empty, out dbid, CreateDatabaseGrbit.None);
                    using (var transaction = new Transaction(session))
                    {
                        JET_TABLEID tableid;
                        Api.JetCreateTable(session, dbid, "table", 0, 100, out tableid);
                        JET_COLUMNID columnid;
                        Api.JetAddColumn(
                            session,
                            tableid,
                            "column1",
                            new JET_COLUMNDEF { coltyp = JET_coltyp.Long },
                            null,
                            0,
                            out columnid);

                        var indexcreates = new[]
                        {
                            new JET_INDEXCREATE
                            {
                                szKey = "+column1\0",
                                cbKey = 10,
                                szIndexName = "index1",
                                pidxUnicode = new JET_UNICODEINDEX { lcid = 1033 },
                            },
                        };

                        Api.JetCreateIndex2(session, tableid, indexcreates, indexcreates.Length);
                        transaction.Commit(CommitTransactionGrbit.LazyFlush);
                    }
                }
            }

            Cleanup.DeleteDirectoryWithRetry(directory);
        }
    }
}