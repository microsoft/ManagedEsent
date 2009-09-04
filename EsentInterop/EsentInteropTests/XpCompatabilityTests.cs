//-----------------------------------------------------------------------
// <copyright file="XpCompatabilityTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Isam.Esent.Interop;
using Microsoft.Isam.Esent.Interop.Implementation;
using Microsoft.Isam.Esent.Interop.Vista;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
    /// <summary>
    /// Test the Api class functionality when we have an XP version of Esent.
    /// </summary>
    [TestClass]
    public class XpCompatabilityTests
    {
        /// <summary>
        /// The saved API, replaced when finished.
        /// </summary>
        private IJetApi savedImpl;

        /// <summary>
        /// Setup the mock object repository.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.savedImpl = Api.Impl;
            Api.Impl = new JetApi(Constants.XpVersion);
        }

        /// <summary>
        /// Cleanup after the test.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            Api.Impl = this.savedImpl;
        }

        /// <summary>
        /// Verify that the XP version of ESENT doesn't support
        /// large keys.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyXpDoesNotSupportLargeKeys()
        {
            Assert.IsFalse(EsentVersion.SupportsLargeKeys);
        }

        /// <summary>
        /// Verify that the XP version of ESENT doesn't support
        /// Windows Server 2003 features.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyXpDoesNotSupportServer2003Features()
        {
            Assert.IsFalse(EsentVersion.SupportsServer2003Features);
        }

        /// <summary>
        /// Verify that the XP version of ESENT doesn't support
        /// Unicode paths.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyXpDoesNotSupportUnicodePaths()
        {
            Assert.IsFalse(EsentVersion.SupportsUnicodePaths);
        }

        /// <summary>
        /// Verify that the XP version of ESENT doesn't support
        /// Windows Vista features.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyXpDoesNotSupportVistaFeatures()
        {
            Assert.IsFalse(EsentVersion.SupportsVistaFeatures);
        }

        /// <summary>
        /// Verify that the XP version of ESENT doesn't support
        /// Windows 7 features.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyXpDoesNotSupportWindows7Features()
        {
            Assert.IsFalse(EsentVersion.SupportsWindows7Features);
        }

        /// <summary>
        /// Verify that JetGetThreadStats throws an exception when using the
        /// XP version of ESENT.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(InvalidOperationException))]
        public void VerifyXpThrowsExceptionOnJetGetThreadStats()
        {
            JET_THREADSTATS threadstats;
            VistaApi.JetGetThreadStats(out threadstats);
        }

        /// <summary>
        /// Verify that JetOpenTemporaryTable throws an exception when using the
        /// XP version of ESENT.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(InvalidOperationException))]
        public void VerifyXpThrowsExceptionOnJetOpenTemporaryTable()
        {
            var sesid = new JET_SESID();
            var temporarytable = new JET_OPENTEMPORARYTABLE();
            VistaApi.JetOpenTemporaryTable(sesid, temporarytable);
        }

        /// <summary>
        /// Getting the LVChunk size should return a default value.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyXpReturnsCorrectLVChunkSize()
        {
            Assert.AreEqual(SystemParameters.DatabasePageSize - 82, SystemParameters.LVChunkSizeMost);
        }

        /// <summary>
        /// Getting the cached closed tables system parameter should return 0 on XP.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyXpReturns0ForCachedClosedTables()
        {
            using (var instance = new Instance("XP"))
            {
                instance.Parameters.CachedClosedTables = 10;
                Assert.AreEqual(0, instance.Parameters.CachedClosedTables);
            }
        }

        /// <summary>
        /// Getting the waypoint system parameter should return 0 on XP.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyXpReturns0ForWaypoint()
        {
            using (var instance = new Instance("XP"))
            {
                instance.Parameters.WaypointLatency = 10;
                Assert.AreEqual(0, instance.Parameters.WaypointLatency);
            }
        }

        /// <summary>
        /// Getting the waypoint system parameter should return 0 on XP.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyXpReturnsNullForAlternateRecoveryDirectory()
        {
            using (var instance = new Instance("XP"))
            {
                instance.Parameters.AlternateDatabaseRecoveryDirectory = @"c:\foo";
                Assert.IsNull(instance.Parameters.AlternateDatabaseRecoveryDirectory);
            }
        }

        /// <summary>
        /// Getting the configuration system parameter should return 0 on XP.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyXpReturns1ForConfiguration()
        {
            SystemParameters.Configuration = 0;
            Assert.AreEqual(1, SystemParameters.Configuration);
        }

        /// <summary>
        /// Getting the enable advanced system parameter should return true on XP.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyXpReturnsTrueForEnableAdvanced()
        {
            SystemParameters.EnableAdvanced = false;
            Assert.AreEqual(true, SystemParameters.EnableAdvanced);
        }

        /// <summary>
        /// Getting the key most system parameter should return 255 on XP.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyXpReturns255ForKeyMost()
        {
            Assert.AreEqual(255, SystemParameters.KeyMost);
        }

        /// <summary>
        /// Use JetCreateIndex2 on Xp to test the compatability path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void CreateIndexesOnXp()
        {
            string directory = SetupHelper.CreateRandomDirectory();
            string database = Path.Combine(directory, "test.db");

            using (var instance = new Instance("XpCompatability"))
            {
                instance.Parameters.Recovery = false;
                instance.Parameters.NoInformationEvent = true;
                instance.Parameters.PageTempDBMin = SystemParameters.PageTempDBSmallest;
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

            Directory.Delete(directory, true);
        }
    }
}