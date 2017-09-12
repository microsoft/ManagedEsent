//-----------------------------------------------------------------------
// <copyright file="IndexInfoTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Vista;
    using Microsoft.Isam.Esent.Interop.Windows8;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for JetGetIndexInfo and JetGetTableIndexInfo.
    /// </summary>
    [TestClass]
    public partial class IndexInfoTests
    {
        /// <summary>
        /// The directory being used for the database and its files.
        /// </summary>
        private string directory;

        /// <summary>
        /// The path to the database being used by the test.
        /// </summary>
        private string database;

        /// <summary>
        /// The name of the table.
        /// </summary>
        private string table;

        /// <summary>
        /// The instance used by the test.
        /// </summary>
        private JET_INSTANCE instance;

        /// <summary>
        /// The session used by the test.
        /// </summary>
        private JET_SESID sesid;

        /// <summary>
        /// Identifies the database used by the test.
        /// </summary>
        private JET_DBID dbid;

        /// <summary>
        /// The tableid being used by the test.
        /// </summary>
        private JET_TABLEID tableid;

        private IList<JET_INDEXCREATE> indexcreates;

        #region Setup/Teardown

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// All DDL should be done in this method.
        /// </summary>
        [TestInitialize]
        [Description("Setup for IndexInfoTests")]
        public void Setup()
        {
            this.directory = SetupHelper.CreateRandomDirectory();
            this.database = Path.Combine(this.directory, "database.edb");
            this.table = "table";
            this.instance = SetupHelper.CreateNewInstance(this.directory);
            this.indexcreates = new List<JET_INDEXCREATE>(10);

            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesid, string.Empty, string.Empty);
            Api.JetCreateDatabase(this.sesid, this.database, string.Empty, out this.dbid, CreateDatabaseGrbit.None);
            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateTable(this.sesid, this.dbid, this.table, 0, 100, out this.tableid);

            JET_COLUMNID ignored;
            var columndef = new JET_COLUMNDEF { coltyp = JET_coltyp.Text, cp = JET_CP.Unicode };

            Api.JetAddColumn(this.sesid, this.tableid, "C1", columndef, null, 0, out ignored);
            Api.JetAddColumn(this.sesid, this.tableid, "C2", columndef, null, 0, out ignored);
            Api.JetAddColumn(this.sesid, this.tableid, "C3", columndef, null, 0, out ignored);

            Api.JetCreateIndex(this.sesid, this.tableid, "Primary", CreateIndexGrbit.IndexPrimary, "+C1\0\0", 5, 100);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            JET_INDEXCREATE[] indexcreates = new[]
            {
                // Multi-segment indices cause failures! JET_INDEXLIST.cRecords is inaccurate. Possibly because cRecords is incremented
                // as the key segment rows are traversed in the catalog, even though no rows are added
                // to the temptable.
                // new JET_INDEXCREATE { szIndexName = "Index2", cbKey = 9, szKey = "+C2\0+C3\0\0" },
                new JET_INDEXCREATE { szIndexName = "Index2", cbKey = 5, szKey = "+C2\0\0" },
                new JET_INDEXCREATE { szIndexName = "Index3", cbKey = 5, szKey = "+C3\0\0", cbVarSegMac = 100 },
            };
            Api.JetCreateIndex2(this.sesid, this.tableid, indexcreates, indexcreates.Length);
            this.indexcreates.Add(indexcreates[0]);
            this.indexcreates.Add(indexcreates[1]);

            if (EsentVersion.SupportsWindows8Features)
            {
                var unicode = new JET_UNICODEINDEX()
                {
                    szLocaleName = "pt-br",
                    dwMapFlags = Conversions.LCMapFlagsFromCompareOptions(CompareOptions.None),
                };

                var indexcreate = new JET_INDEXCREATE
                {
                    szIndexName = "win8BrazilIndex",
                    szKey = "+C2\0\0",
                    cbKey = 5,
                    pidxUnicode = unicode,
                    grbit = CreateIndexGrbit.IndexIgnoreAnyNull,
                    ulDensity = 100,
                };
                Windows8Api.JetCreateIndex4(this.sesid, this.tableid, new[] { indexcreate }, 1);
                this.indexcreates.Add(indexcreate);
            }

            Api.JetCloseTable(this.sesid, this.tableid);
            Api.JetOpenTable(this.sesid, this.dbid, this.table, null, 0, OpenTableGrbit.None, out this.tableid);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        [Description("Cleanup for IndexInfoTests")]
        public void Teardown()
        {
            Api.JetCloseTable(this.sesid, this.tableid);
            Api.JetEndSession(this.sesid, EndSessionGrbit.None);
            Api.JetTerm(this.instance);
            Cleanup.DeleteDirectoryWithRetry(this.directory);
        }

        #endregion

        #region JetGetIndexInfo

        /// <summary>
        /// Test the overload of JetGetIndexInfo that returns a ushort.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test the overload of JetGetIndexInfo that returns a ushort")]
        public void TestJetGetIndexInfoUshort()
        {
            ushort result;
            Api.JetGetIndexInfo(this.sesid, this.dbid, this.table, "Index3", out result, JET_IdxInfo.VarSegMac);
            Assert.AreEqual((ushort)100, result);
        }

        /// <summary>
        /// Test that JetGetIndexInfo throws exception when index name is invalid.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test that JetGetIndexInfo throws exception when index name is invalid")]
        public void TestJetGetIndexInfoThrowsExceptionWhenIndexNameIsInvalid()
        {
            ushort result;
            try
            {
                Api.JetGetIndexInfo(this.sesid, this.dbid, this.table, "NoSuchIndex", out result, JET_IdxInfo.VarSegMac);
                Assert.IsTrue(false, "EsentIndexNotFoundException should have been thrown!");
            }
            catch (EsentIndexNotFoundException)
            {
            }
        }

        /// <summary>
        /// Test the overload of JetGetIndexInfo that returns an int.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test the overload of JetGetIndexInfo that returns an int")]
        public void TestJetGetIndexInfoInt()
        {
            int result;
            Api.JetGetIndexInfo(this.sesid, this.dbid, this.table, null, out result, JET_IdxInfo.Count);
            int expectedIndexCount = EsentVersion.SupportsWindows8Features ? 4 : 3;
            Assert.AreEqual(expectedIndexCount, result);
        }

        /// <summary>
        /// Test the overload of JetGetIndexInfo that returns a JET_INDEXID.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test the overload of JetGetIndexInfo that returns a JET_INDEXID")]
        public void TestJetGetIndexInfoIndexId()
        {
            JET_INDEXID result;
            Api.JetGetIndexInfo(this.sesid, this.dbid, this.table, "Index2", out result, JET_IdxInfo.IndexId);
        }

        /// <summary>
        /// Test the overload of JetGetIndexInfo that returns a locale name.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test the overload of JetGetIndexInfo that returns a locale name")]
        public void TestJetGetIndexInfoLocaleName()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            string result;
            Api.JetGetIndexInfo(this.sesid, this.dbid, this.table, "win8BrazilIndex", out result, Windows8IdxInfo.LocaleName);
            Assert.AreEqual("pt-br", result, false);
        }

        /// <summary>
        /// Test the overload of JetGetIndexInfo that returns a JET_INDEXLIST.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test the overload of JetGetIndexInfo that returns a JET_INDEXLIST")]
        public void TestJetGetIndexInfoIndexList()
        {
            JET_INDEXLIST result;
            var indices = Api.GetTableIndexes(this.sesid, this.dbid, this.table).ToList();

            Api.JetGetIndexInfo(this.sesid, this.dbid, this.table, null, out result, JET_IdxInfo.List);
            int expectedIndexCount = EsentVersion.SupportsWindows8Features ? 4 : 3;

            Assert.AreEqual(indices.Count, result.cRecord);
            Assert.AreEqual(expectedIndexCount, result.cRecord);
            Api.JetCloseTable(this.sesid, result.tableid);
        }


        /// <summary>
        /// Test the obsolete overload of JetGetIndexInfo that returns a JET_INDEXLIST.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test the obsolete overload of JetGetIndexInfo that returns a JET_INDEXLIST")]
        public void TestJetGetIndexInfoIndexListObsolete()
        {
            JET_INDEXLIST result;
#pragma warning disable 612,618
            Api.JetGetIndexInfo(this.sesid, this.dbid, this.table, null, out result);
#pragma warning restore 612,618
            int expectedIndexCount = EsentVersion.SupportsWindows8Features ? 4 : 3;
            Assert.AreEqual(expectedIndexCount, result.cRecord);
            Api.JetCloseTable(this.sesid, result.tableid);
        }

        /// <summary>
        /// Test the overload of JetGetIndexInfo that returns a JET_INDEXCREATE.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test the overload of JetGetIndexInfo that returns a JET_INDEXCREATE")]
        public void TestJetGetIndexInfoIndexCreate()
        {
            JET_INDEXCREATE result;

            Api.JetGetIndexInfo(this.sesid, this.dbid, this.table, "Primary", out result, Windows8IdxInfo.InfoCreateIndex3);

            Assert.AreEqual("Primary", result.szIndexName);
            Assert.AreEqual(100, result.ulDensity);
            Assert.AreEqual("+C1\0\0", result.szKey);
            Assert.AreEqual((CreateIndexGrbit.IndexPrimary | CreateIndexGrbit.IndexUnique | VistaGrbits.IndexUnicode), result.grbit);

            Api.JetGetIndexInfo(this.sesid, this.dbid, this.table, "Index2", out result, Windows8IdxInfo.InfoCreateIndex3);
            CompareIndexCreateWithOutput(this.indexcreates.ElementAt(0), result);

            Api.JetGetIndexInfo(this.sesid, this.dbid, this.table, "Index3", out result, Windows8IdxInfo.InfoCreateIndex3);
            CompareIndexCreateWithOutput(this.indexcreates.ElementAt(1), result);

            if (EsentVersion.SupportsWindows8Features)
            {
                Api.JetGetIndexInfo(this.sesid, this.dbid, this.table, "Win8BrazilIndex", out result, Windows8IdxInfo.InfoCreateIndex3);
                CompareIndexCreateWithOutput(this.indexcreates.ElementAt(2), result);
            }
        }

        /// <summary>
        /// Tests that GetTableIndexes() works when specifying the table name.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Tests that GetTableIndexes() works when specifying the table name.")]
        public void TestGetTableIndexesWithTableName()
        {
            int foundIndices = 0;
            foreach (IndexInfo indexInfo in Api.GetTableIndexes(this.sesid, this.dbid, this.table))
            {
                if (indexInfo.Name.Equals("Primary"))
                {
                    Assert.AreEqual(CreateIndexGrbit.IndexPrimary | CreateIndexGrbit.IndexUnique, indexInfo.Grbit);
                    ++foundIndices;
                }
                else if (indexInfo.Name.Equals("win8BrazilIndex"))
                {
                    // This index might not be present when running on pre-win8.
                    Assert.AreEqual("pt-br", indexInfo.CultureInfo.Name, true);
                    ++foundIndices;
                }
            }

            int expectedIndexCount = EsentVersion.SupportsWindows8Features ? 2 : 1;
            Assert.AreEqual(expectedIndexCount, foundIndices);
        }

        #endregion

        #region JetGetTableIndexInfo

        /// <summary>
        /// Test the overload of JetGetTableIndexInfo that returns a ushort.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test the overload of JetGetTableIndexInfo that returns a ushort")]
        public void TestJetGetTableIndexInfoUshort()
        {
            ushort result;
            Api.JetGetTableIndexInfo(this.sesid, this.tableid, "Index3", out result, JET_IdxInfo.VarSegMac);
            Assert.AreEqual((ushort)100, result);
        }

        /// <summary>
        /// Test the overload of JetGetTableIndexInfo that returns an int.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test the overload of JetGetTableIndexInfo that returns an int")]
        public void TestJetGetTableIndexInfoInt()
        {
            int result;
            Api.JetGetTableIndexInfo(this.sesid, this.tableid, null, out result, JET_IdxInfo.Count);
            int expectedIndexCount = EsentVersion.SupportsWindows8Features ? 4 : 3;
            Assert.AreEqual(expectedIndexCount, result);
        }

        /// <summary>
        /// Test the overload of JetGetTableIndexInfo that returns a JET_INDEXID.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test the overload of JetGetTableIndexInfo that returns a JET_INDEXID")]
        public void TestJetGetTableIndexInfoIndexId()
        {
            JET_INDEXID result;
            Api.JetGetTableIndexInfo(this.sesid, this.tableid, "Index2", out result, JET_IdxInfo.IndexId);
        }

        /// <summary>
        /// Test that TryJetGetTableIndexInfo (JET_INDEXID) returns false when index name is invalid.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test that TryJetGetTableIndexInfo (JET_INDEXID) returns false when index name is invalid")]
        public void TestTryJetGetTableIndexInfoIndexIdReturnsFalseWhenIndexNameIsInvalid()
        {
            JET_INDEXID result;

            Assert.IsTrue(!Api.TryJetGetTableIndexInfo(this.sesid, this.tableid, "NoSuchIndex", out result, JET_IdxInfo.IndexId));
        }

        /// <summary>
        /// Test that TryJetGetTableIndexInfo (JET_INDEXID) returns true when index name is valid.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test that TryJetGetTableIndexInfo (JET_INDEXID) returns true when index name is valid")]
        public void TestTryJetGetTableIndexInfoIndexIdReturnsTrueWhenIndexNameIsValid()
        {
            JET_INDEXID result;

            Assert.IsTrue(Api.TryJetGetTableIndexInfo(this.sesid, this.tableid, "Index2", out result, JET_IdxInfo.IndexId));
        }

        /// <summary>
        /// Test that TryJetGetTableIndexInfo (JET_INDEXID) throws when a bad param is passed.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test that TryJetGetTableIndexInfo (JET_INDEXID) throws when a bad param is passed")]
        public void TestTryJetGetTableIndexInfoIndexIdThrowsWhenBadParam()
        {
            JET_INDEXID result;

            try
            {
                Api.TryJetGetTableIndexInfo(this.sesid, this.tableid, "Index2", out result, (JET_IdxInfo)int.MaxValue);
                Assert.Fail("Expected an EsentInvalidParameterException");
            }
            catch (EsentInvalidParameterException ex)
            {
                Assert.AreEqual(JET_err.InvalidParameter, ex.Error);
            }
        }

        /// <summary>
        /// Test the overload of JetGetTableIndexInfo that returns a locale name.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test the overload of JetGetIndexInfo that returns a locale name")]
        public void TestJetGetTableIndexInfoLocaleName()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            string result;
            Api.JetGetTableIndexInfo(this.sesid, this.tableid, "win8BrazilIndex", out result, Windows8IdxInfo.LocaleName);
            Assert.AreEqual("pt-br", result, false);
        }

        /// <summary>
        /// Test the overload of JetGetTableIndexInfo that returns a JET_INDEXLIST.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test the overload of JetGetTableIndexInfo that returns a JET_INDEXLIST")]
        public void TestJetGetTableIndexInfoIndexList()
        {
            JET_INDEXLIST result;
            Api.JetGetTableIndexInfo(this.sesid, this.tableid, null, out result, JET_IdxInfo.List);
            int expectedIndexCount = EsentVersion.SupportsWindows8Features ? 4 : 3;
            Assert.AreEqual(expectedIndexCount, result.cRecord);
            Api.JetCloseTable(this.sesid, result.tableid);
        }

        /// <summary>
        /// Test the obsolete overload of JetGetTableIndexInfo that returns a JET_INDEXLIST.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test the obsolete overload of JetGetTableIndexInfo that returns a JET_INDEXLIST")]
        public void TestJetGetTableIndexInfoIndexListObsolete()
        {
            JET_INDEXLIST result;
#pragma warning disable 612,618
            Api.JetGetTableIndexInfo(this.sesid, this.tableid, null, out result);
#pragma warning restore 612,618
            int expectedIndexCount = EsentVersion.SupportsWindows8Features ? 4 : 3;
            Assert.AreEqual(expectedIndexCount, result.cRecord);
            Api.JetCloseTable(this.sesid, result.tableid);
        }

        /// <summary>
        /// Test the overload of JetGetTableIndexInfo that returns a JET_INDEXCREATE.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test the overload of JetGetTableIndexInfo that returns a JET_INDEXCREATE")]
        public void TestJetGetTableIndexInfoIndexCreate()
        {
            JET_INDEXCREATE result;

            Api.JetGetTableIndexInfo(this.sesid, this.tableid, "Primary", out result, Windows8IdxInfo.InfoCreateIndex3);

            Assert.AreEqual("Primary", result.szIndexName);
            Assert.AreEqual(100, result.ulDensity);
            Assert.AreEqual("+C1\0\0", result.szKey);
            Assert.AreEqual((CreateIndexGrbit.IndexPrimary | CreateIndexGrbit.IndexUnique | VistaGrbits.IndexUnicode), result.grbit);

            Api.JetGetTableIndexInfo(this.sesid, this.tableid, "Index2", out result, Windows8IdxInfo.InfoCreateIndex3);
            CompareIndexCreateWithOutput(this.indexcreates.ElementAt(0), result);

            Api.JetGetTableIndexInfo(this.sesid, this.tableid, "Index3", out result, Windows8IdxInfo.InfoCreateIndex3);
            CompareIndexCreateWithOutput(this.indexcreates.ElementAt(1), result);

            if (EsentVersion.SupportsWindows8Features)
            {
                Api.JetGetTableIndexInfo(this.sesid, this.tableid, "Win8BrazilIndex", out result, Windows8IdxInfo.InfoCreateIndex3);
                CompareIndexCreateWithOutput(this.indexcreates.ElementAt(2), result);
            }
        }

        /// <summary>
        /// Tests that GetTableIndexes() works when specifying the table id.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Tests that GetTableIndexes() works when specifying the table id.")]
        public void TestGetTableIndexesWithTableId()
        {
            int foundIndices = 0;
            foreach (IndexInfo indexInfo in Api.GetTableIndexes(this.sesid, this.tableid))
            {
                if (indexInfo.Name.Equals("Primary"))
                {
                    Assert.AreEqual(CreateIndexGrbit.IndexPrimary | CreateIndexGrbit.IndexUnique, indexInfo.Grbit);
                    ++foundIndices;
                }
                else if (indexInfo.Name.Equals("win8BrazilIndex"))
                {
                    // This index might not be present when running on pre-win8.
                    Assert.AreEqual("pt-br", indexInfo.CultureInfo.Name, true);
                    ++foundIndices;
                }
            }

            int expectedIndexCount = EsentVersion.SupportsWindows8Features ? 2 : 1;
            Assert.AreEqual(expectedIndexCount, foundIndices);
        }

        #endregion

        #region Helper function
        private static void CompareIndexCreateWithOutput(
            JET_INDEXCREATE orig,
            JET_INDEXCREATE actual)
        {
            Assert.AreEqual(orig.szIndexName, actual.szIndexName);
            Assert.AreEqual(orig.cConditionalColumn, actual.cConditionalColumn);
            Assert.AreEqual(orig.cbKey, actual.cbKey);
            Assert.AreEqual(orig.szKey, actual.szKey);
            if (orig.cbKeyMost == 0)
            {
                Assert.AreEqual(255, actual.cbKeyMost);
            }
            else
            {
                Assert.AreEqual(orig.cbKeyMost, actual.cbKeyMost);
            }

            if (orig.cbVarSegMac == 0)
            {
                Assert.AreEqual(255, actual.cbVarSegMac);
            }
            else
            {
                Assert.AreEqual(orig.cbVarSegMac, actual.cbVarSegMac);
            }

            if (orig.ulDensity == 0)
            {
                Assert.AreEqual(100, actual.ulDensity);
            }
            else
            {
                Assert.AreEqual(orig.ulDensity, actual.ulDensity);
            }

            // Clear the bits that might get set:
            CreateIndexGrbit originalGrbit = orig.grbit;
            CreateIndexGrbit actualGrbit = actual.grbit & ~(CreateIndexGrbit.IndexUnique | VistaGrbits.IndexUnicode);
            if (originalGrbit.HasFlag(CreateIndexGrbit.IndexIgnoreAnyNull))
            {
                originalGrbit &= ~(CreateIndexGrbit.IndexIgnoreAnyNull | CreateIndexGrbit.IndexIgnoreFirstNull |
                                CreateIndexGrbit.IndexIgnoreNull);
                actualGrbit &= ~(CreateIndexGrbit.IndexIgnoreAnyNull | CreateIndexGrbit.IndexIgnoreFirstNull |
                                CreateIndexGrbit.IndexIgnoreNull);
            }

            Assert.AreEqual(originalGrbit, actualGrbit);

            if (orig.pSpaceHints == null)
            {
                JET_SPACEHINTS defaultSpaceHints = new JET_SPACEHINTS()
                {
                    ulInitialDensity = 100, // Or is it actual.ulDensity ?
                };
                Assert.IsTrue(defaultSpaceHints.ContentEquals(actual.pSpaceHints));
            }
            else
            {
                Assert.IsTrue(orig.pSpaceHints.ContentEquals(actual.pSpaceHints));
            }

            if (orig.pidxUnicode == null)
            {
                JET_UNICODEINDEX defaultUnicodeIndex = new JET_UNICODEINDEX()
                {
                    dwMapFlags = 0x30401,
                    szLocaleName = "en-us",
                };
                Assert.IsTrue(defaultUnicodeIndex.ContentEquals(actual.pidxUnicode));
            }
        }
        #endregion
    }
}
