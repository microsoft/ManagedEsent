//-----------------------------------------------------------------------
// <copyright file="Windows7CompatibilityTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

#if !MANAGEDESENT_ON_METRO // The Metro version of the DLL always exposes all features.
namespace InteropApiTests
{
    using System;
    using System.IO;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Implementation;
    using Microsoft.Isam.Esent.Interop.Vista;
    using Microsoft.Isam.Esent.Interop.Windows8;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the Api class functionality when we have an Windows7 version of Esent.
    /// </summary>
    [TestClass]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Microsoft.StyleCop.CSharp.DocumentationRules",
        "SA1649:FileHeaderFileNameDocumentationMustMatchTypeName",
        Justification = "The file name is misspelled, and correcting it will negatively affect source history.")]
    public class Windows7CompatibilityTests
    {
        /// <summary>
        /// The saved API, replaced when finished.
        /// </summary>
        private IJetApi savedImpl;

        /// <summary>
        /// Setup the mock object repository.
        /// </summary>
        [TestInitialize]
        [Description("Setup the Windows7CompatibilityTests fixture")]
        public void Setup()
        {
            this.savedImpl = Api.Impl;

            // If we aren't running with a version of ESENT that does
            // support Windows7 features then we can't run these tests.
            if (EsentVersion.SupportsWindows7Features)
            {
                Api.Impl = new JetApi(Constants.Windows7Version);
            }
        }

        /// <summary>
        /// Cleanup after the test.
        /// </summary>
        [TestCleanup]
        [Description("Cleanup the Windows7CompatibilityTests fixture")]
        public void Teardown()
        {
            Api.Impl = this.savedImpl;
        }

        /// <summary>
        /// Verify that the Windows7 version of ESENT does support
        /// large keys.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that the Windows7 version of ESENT does support large keys")]
        public void VerifyWindows7DoesSupportLargeKeys()
        {
            if (!EsentVersion.SupportsWindows7Features)
            {
                return;
            }

            Assert.IsTrue(EsentVersion.SupportsLargeKeys);
        }

        /// <summary>
        /// Verify that the Windows7 version of ESENT does support
        /// Windows Server 2003 features.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that the Windows7 version of ESENT does support Windows Server 2003 features")]
        public void VerifyWindows7DoesSupportServer2003Features()
        {
            if (!EsentVersion.SupportsWindows7Features)
            {
                return;
            }

            Assert.IsTrue(EsentVersion.SupportsServer2003Features);
        }

        /// <summary>
        /// Verify that the Windows7 version of ESENT does support
        /// Unicode paths.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that the Windows7 version of ESENT does support Unicode paths")]
        public void VerifyWindows7DoesSupportUnicodePaths()
        {
            if (!EsentVersion.SupportsWindows7Features)
            {
                return;
            }

            Assert.IsTrue(EsentVersion.SupportsUnicodePaths);
        }

        /// <summary>
        /// Verify that the Windows7 version of ESENT does support
        /// Windows Windows7 features.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that the Windows7 version of ESENT does support Windows Windows7 features")]
        public void VerifyWindows7DoesSupportWindows7Features()
        {
            if (!EsentVersion.SupportsWindows7Features)
            {
                return;
            }

            Assert.IsTrue(EsentVersion.SupportsWindows7Features);
        }

        /// <summary>
        /// Verify that the Windows7 version of ESENT doesn't support
        /// Windows 8 features.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that the Windows7 version of ESENT doesn't support Windows 8 features")]
        public void VerifyWindows7DoesNotSupportWindows8Features()
        {
            Assert.IsFalse(EsentVersion.SupportsWindows8Features);
        }

        /// <summary>
        /// Verify that JetStopServiceInstance2 throws an exception on Win7.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Verify that JetStopServiceInstance2 throws an exception on Win7.")]
        public void CheckStopServiceInstance2OnWIndows7ThrowsException()
        {
            try
            {
                Windows8Api.JetStopServiceInstance2(JET_INSTANCE.Nil, (StopServiceGrbit)0x1);
                Assert.Fail("JetStopServiceInstance2 should have thrown UnsupportedApiException on Win7.");
            }
            catch (InvalidOperationException)
            {
            }
        }

        /// <summary>
        /// Use JetGetDatabaseFileInfo on Windows7 to test the compatibility path for JET_DBINFOMISC.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Use JetGetDatabaseFileInfo on Windows7 to test the compatibility path")]
        public void GetDatabaseFileInfoOnWindows7()
        {
            if (!EsentVersion.SupportsWindows7Features)
            {
                return;
            }

            string directory = SetupHelper.CreateRandomDirectory();
            string database = Path.Combine(directory, "test.db");

            using (var instance = new Instance("Windows7JetGetDatabaseFileInfo"))
            {
                SetupHelper.SetLightweightConfiguration(instance);
                instance.Init();
                using (var session = new Session(instance))
                {
                    JET_DBID dbid;
                    Api.JetCreateDatabase(session, database, string.Empty, out dbid, CreateDatabaseGrbit.None);
                }
            }

            JET_DBINFOMISC dbinfomisc;
            Api.JetGetDatabaseFileInfo(database, out dbinfomisc, JET_DbInfo.Misc);
            Assert.AreEqual(SystemParameters.DatabasePageSize, dbinfomisc.cbPageSize);

            Cleanup.DeleteDirectoryWithRetry(directory);
        }

        /// <summary>
        /// Use JetGetDatabaseInfo on Windows7 to test the compatibility path for JET_DBINFOMISC.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Use JetGetDatabaseInfo on Windows7 to test the compatibility path")]
        public void GetDatabaseInfoOnWindows7()
        {
            string directory = SetupHelper.CreateRandomDirectory();
            string database = Path.Combine(directory, "test.db");

            using (var instance = new Instance("Windows7JetGetDatabaseInfo"))
            {
                SetupHelper.SetLightweightConfiguration(instance);
                instance.Init();
                using (var session = new Session(instance))
                {
                    JET_DBID dbid;
                    Api.JetCreateDatabase(session, database, string.Empty, out dbid, CreateDatabaseGrbit.None);

                    JET_DBINFOMISC dbinfomisc;
                    Api.JetGetDatabaseInfo(session, dbid, out dbinfomisc, JET_DbInfo.Misc);
                    Assert.AreEqual(SystemParameters.DatabasePageSize, dbinfomisc.cbPageSize);
                }
            }

            Cleanup.DeleteDirectoryWithRetry(directory);
        }

        /// <summary>
        /// Use JetCreateIndex2 on Windows7 to test the compatibility path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Use JetCreateIndex2 on Windows7 to test the compatibility path")]
        public void CreateIndexesOnWindows7()
        {
            if (!EsentVersion.SupportsWindows7Features)
            {
                return;
            }

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

        /// <summary>
        /// Use JetGetRecordSize on Windows7 to test the compatibility path. This also tests
        /// the handling of the running total option.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Use JetGetRecordSize on Windows7 to test the compatibility path")]
        public void GetRecordSizeOnWindows7()
        {
            if (!EsentVersion.SupportsWindows7Features)
            {
                return;
            }

            string directory = SetupHelper.CreateRandomDirectory();
            string database = Path.Combine(directory, "test.db");

            using (var instance = new Instance("Windows7GetRecordSize"))
            {
                instance.Parameters.Recovery = false;
                instance.Parameters.NoInformationEvent = true;
                instance.Parameters.MaxTemporaryTables = 0;
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
                        Api.JetAddColumn(
                            session,
                            tableid,
                            "column1",
                            new JET_COLUMNDEF { coltyp = JET_coltyp.LongBinary },
                            null,
                            0,
                            out columnid);

                        var size = new JET_RECSIZE();
                        byte[] data = Any.Bytes;

                        using (var update = new Update(session, tableid, JET_prep.Insert))
                        {
                            Api.SetColumn(session, tableid, columnid, data);
                            VistaApi.JetGetRecordSize(session, tableid, ref size, GetRecordSizeGrbit.Local | GetRecordSizeGrbit.InCopyBuffer);
                            update.SaveAndGotoBookmark();
                        }

                        VistaApi.JetGetRecordSize(session, tableid, ref size, GetRecordSizeGrbit.RunningTotal);

                        Assert.AreEqual(data.Length * 2, size.cbData, "cbData");
                        Assert.AreEqual(data.Length * 2, size.cbDataCompressed, "cbDataCompressed");
                        Assert.AreEqual(0, size.cbLongValueData, "cbLongValueData");
                        Assert.AreEqual(0, size.cbLongValueDataCompressed, "cbLongValueDataCompressed");
                        Assert.AreEqual(0, size.cbLongValueOverhead, "cbLongValueOverhead");
                        Assert.AreNotEqual(0, size.cbOverhead, "cbOverhead");
                        Assert.AreEqual(0, size.cCompressedColumns, "cCompressedColumns");
                        Assert.AreEqual(0, size.cLongValues, "cLongValues");
                        Assert.AreEqual(0, size.cMultiValues, "cMultiValues");
                        Assert.AreEqual(0, size.cNonTaggedColumns, "cTaggedColumns");
                        Assert.AreEqual(2, size.cTaggedColumns, "cTaggedColumns");

                        transaction.Commit(CommitTransactionGrbit.LazyFlush);
                    }
                }
            }

            Cleanup.DeleteDirectoryWithRetry(directory);
        }

        /// <summary>
        /// Creates a table with JetCreateTableColumnIndex4 on Win7 throws an exception.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Creates a table with JetCreateTableColumnIndex4 on Win7 throws an exception.")]
        public void CreateTableColumnIndex4OnWindows7ThrowsException()
        {
            if (!EsentVersion.SupportsWindows7Features)
            {
                return;
            }

            var columncreates = new JET_COLUMNCREATE[]
            {
                new JET_COLUMNCREATE()
                {
                    szColumnName = "col1_short",
                    coltyp = JET_coltyp.Short,
                    cbMax = 2,
                },
                new JET_COLUMNCREATE()
                {
                    szColumnName = "col2_longtext",
                    coltyp = JET_coltyp.LongText,
                    cp = JET_CP.Unicode,
                },
            };

            const string Index1Name = "firstIndex";
            const string Index1Description = "+col1_short\0-col2_longtext\0";

            const string Index2Name = "secondIndex";
            const string Index2Description = "+col2_longtext\0-col1_short\0";

            var indexcreates = new JET_INDEXCREATE[]
            {
                  new JET_INDEXCREATE
                {
                    szIndexName = Index1Name,
                    szKey = Index1Description,
                    cbKey = Index1Description.Length + 1,
                    grbit = CreateIndexGrbit.None,
                    ulDensity = 99,
                },
                new JET_INDEXCREATE
                {
                    szIndexName = Index2Name,
                    szKey = Index2Description,
                    cbKey = Index2Description.Length + 1,
                    grbit = CreateIndexGrbit.None,
                    ulDensity = 79,
                },
            };

            var tablecreate = new JET_TABLECREATE()
            {
                szTableName = "tableBigBang",
                ulPages = 23,
                ulDensity = 75,
                cColumns = columncreates.Length,
                rgcolumncreate = columncreates,
                rgindexcreate = indexcreates,
                cIndexes = indexcreates.Length,
                cbSeparateLV = 100,
                cbtyp = JET_cbtyp.Null,
                grbit = CreateTableColumnIndexGrbit.None,
            };

            string directory = SetupHelper.CreateRandomDirectory();
            string database = Path.Combine(directory, "test.db");

            using (var instance = new Instance("Windows7CreateTableColumnIndex4"))
            {
                instance.Parameters.Recovery = false;
                instance.Parameters.NoInformationEvent = true;
                instance.Parameters.MaxTemporaryTables = 0;
                instance.Init();
                using (var session = new Session(instance))
                {
                    JET_DBID dbid;
                    Api.JetCreateDatabase(session, database, string.Empty, out dbid, CreateDatabaseGrbit.None);
                    using (var transaction = new Transaction(session))
                    {
                        try
                        {
                            Windows8Api.JetCreateTableColumnIndex4(session, dbid, tablecreate);
                            Assert.Fail("JetCreateTableColumnIndex4() is supposed to throw an exception on Win7.");
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a table with JetCreateTableColumnIndex4 on Win7 throws an exception.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Creates a table with JetCreateTableColumnIndex4 on Win7 throws an exception.")]
        public void CreateBasicTableColumnIndex4OnWindows7ThrowsException()
        {
            if (!EsentVersion.SupportsWindows7Features)
            {
                return;
            }

            var tablecreate = new JET_TABLECREATE { szTableName = "table" };

            string directory = SetupHelper.CreateRandomDirectory();
            string database = Path.Combine(directory, "test.db");

            using (var instance = new Instance("Windows7CreateBasicTableColumnIndex4"))
            {
                instance.Parameters.Recovery = false;
                instance.Parameters.NoInformationEvent = true;
                instance.Parameters.MaxTemporaryTables = 0;
                instance.Init();
                using (var session = new Session(instance))
                {
                    JET_DBID dbid;
                    Api.JetCreateDatabase(session, database, string.Empty, out dbid, CreateDatabaseGrbit.None);
                    using (var transaction = new Transaction(session))
                    {
                        try
                        {
                            Windows8Api.JetCreateTableColumnIndex4(session, dbid, tablecreate);
                            Assert.Fail("JetCreateTableColumnIndex4() is supposed to throw an exception on Win7.");
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }
                }
            }
        }
    }
}
#endif // !MANAGEDESENT_ON_METRO