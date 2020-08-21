//-----------------------------------------------------------------------
// <copyright file="PrereadRangesTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Threading;
    using Microsoft.Isam.Esent;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Vista;
    using Microsoft.Isam.Esent.Interop.Windows8;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// DML and currency tests for filtered moves.
    /// </summary>
    [TestClass]
    [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules",
        "SA1305:FieldNamesMustNotUseHungarianNotation",
        Justification = "Reviewed. Suppression is OK here.")]
    public partial class PrereadRangesTests
    {
        /// <summary>
        /// Length of data column
        /// </summary>
        private const int PageSize = 8192;

        /// <summary>
        /// Length of data column
        /// </summary>
        private const int DataLength = 1500;

        /// <summary>
        /// Table name to create.
        /// </summary>
        private readonly string tableName = "table";

        /// <summary>
        /// Clustered index name to create.
        /// </summary>
        private readonly string clustIndexName = "clustered";

        /// <summary>
        /// Secondary index name to create (contains primary key column).
        /// </summary>
        private readonly string secIndexWithPrimaryName = "secondarywithprimary";

        /// <summary>
        /// The saved min cache size
        /// </summary>
        private int savedCacheSizeMin;

        /// <summary>
        /// The saved max cache size
        /// </summary>
        private int savedCacheSizeMax;

        /// <summary>
        /// The directory being used for the database and its files.
        /// </summary>
        private string directory;

        /// <summary>
        /// The path to the database being used by the test.
        /// </summary>
        private string database;

        /// <summary>
        /// The instance used by the test.
        /// </summary>
        private JET_INSTANCE instance;

        /// <summary>
        /// The session used by the test.
        /// </summary>
        private JET_SESID sesId;

        /// <summary>
        /// Identifies the database used by the test.
        /// </summary>
        private JET_DBID dbId;

        /// <summary>
        /// Identifies the table used by the test.
        /// </summary>
        private JET_TABLEID tableId;

        /// <summary>
        /// Column ID of the column that makes up the primary index.
        /// </summary>
        private JET_COLUMNID columnIdKey1;

        /// <summary>
        /// Column ID of the column that makes up the primary index.
        /// </summary>
        private JET_COLUMNID columnIdKey2;

        /// <summary>
        /// Column ID of the column that makes up the primary index.
        /// </summary>
        private JET_COLUMNID columnIdKey3;

        /// <summary>
        /// Column ID of the column that makes up the data portion of the record.
        /// </summary>
        private JET_COLUMNID columnIdData;

        /// <summary>
        /// Column ID of the column that makes up the extrinsic LV of the record.
        /// </summary>
        private JET_COLUMNID columnIdExLV;

        /// <summary>
        /// Column IDs to preread.
        /// </summary>
        private JET_COLUMNID[] columnIdPreread;

        #region Setup/Teardown

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// </summary>
        [TestInitialize]
        [Description("Setup for each test in PrereadRangesTests")]
        public void TestSetup()
        {
            this.directory = SetupHelper.CreateRandomDirectory();
            this.database = Path.Combine(this.directory, "database.edb");

            SystemParameters.DatabasePageSize = PrereadRangesTests.PageSize;

            // ISSUE-2014/10/20-BrettSh - Weird issue where we don't count one of the prereading requests in
            // regular mode, but do in view cache causing the main test check in TestFilteredMoveBeyondIndexRange()
            // to fail.  Avoiding the issue for now by turning off ViewCache here.
            SystemParameters.EnableFileCache = false;
            SystemParameters.EnableViewCache = false;
            this.savedCacheSizeMin = SystemParameters.CacheSizeMin;
            this.savedCacheSizeMax = SystemParameters.CacheSizeMax;
            SystemParameters.CacheSizeMin = 128;
            SystemParameters.CacheSizeMax = 128;

            this.instance = SetupHelper.CreateNewInstance(this.directory);
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesId, string.Empty, string.Empty);

            JET_DBID dbId;
            Api.JetCreateDatabase(this.sesId, this.database, string.Empty, out dbId, CreateDatabaseGrbit.None);
            Api.JetCloseDatabase(this.sesId, dbId, CloseDatabaseGrbit.None);

            Api.JetOpenDatabase(this.sesId, this.database, null, out this.dbId, OpenDatabaseGrbit.None);
            this.CreateAndPopulateTable();
            Api.JetCloseDatabase(this.sesId, this.dbId, CloseDatabaseGrbit.None);
            Api.JetDetachDatabase(this.sesId, this.database);
            this.AttachDatabase(this.sesId, this.database, Windows8Grbits.PurgeCacheOnAttach);
            Api.JetOpenDatabase(this.sesId, this.database, null, out this.dbId, OpenDatabaseGrbit.None);
            Api.JetOpenTable(this.sesId, this.dbId, this.tableName, null, 0, OpenTableGrbit.None, out this.tableId);

            Api.JetBeginTransaction(this.sesId);

            // purge the cache and free space for pre-reads
            this.ForceFlushCache();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        [Description("Cleanup for each test in DmlCurrencyTests")]
        public void TestTeardown()
        {
            while (true)
            {
                try
                {
                    Api.JetCommitTransaction(this.sesId, CommitTransactionGrbit.None);
                }
                catch (EsentNotInTransactionException)
                {
                    break;
                }
            }

            Api.JetCloseDatabase(this.sesId, this.dbId, CloseDatabaseGrbit.None);

            Api.JetEndSession(this.sesId, EndSessionGrbit.None);
            Api.JetTerm(this.instance);
            SystemParameters.CacheSizeMin = this.savedCacheSizeMin;
            SystemParameters.CacheSizeMax = this.savedCacheSizeMax;
            Cleanup.DeleteDirectoryWithRetry(this.directory);
        }

        #endregion Setup/Teardown

        #region Preread Ranges tests

        /// <summary>
        /// Test PrereadRanges with invalid relop
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges with invalid relop")]
        public void TestReadRangeInvalidRelop()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn = new JET_INDEX_COLUMN[1];
            startColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 6, JetRelop.GreaterThanOrEqual);
            JET_INDEX_COLUMN[] endColumn = new JET_INDEX_COLUMN[1];
            endColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 8, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn,
                    endColumns = endColumn
                }
            };

            int rangesRead;
            try
            {
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Forward);
                Assert.Fail("Expected: " + typeof(EsentInvalidOperationException).Name);
            }
            catch (EsentInvalidParameterException)
            {
            }

            try
            {
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, this.columnIdPreread, PrereadIndexRangesGrbit.Forward);
                Assert.Fail("Expected: " + typeof(EsentInvalidOperationException).Name);
            }
            catch (EsentInvalidParameterException)
            {
            }
        }

        /// <summary>
        /// Test PrereadRanges of one range with all keys
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of one range with all keys")]
        public void TestReadRangeAllKeys()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn = new JET_INDEX_COLUMN[3];
            startColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 6, JetRelop.Equals);
            startColumn[1] = this.CreateKeyColumn(this.columnIdKey2, 60, JetRelop.Equals);
            startColumn[2] = this.CreateKeyColumn(this.columnIdKey3, 600, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn = new JET_INDEX_COLUMN[3];
            endColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 8, JetRelop.Equals);
            endColumn[1] = this.CreateKeyColumn(this.columnIdKey2, 80, JetRelop.Equals);
            endColumn[2] = this.CreateKeyColumn(this.columnIdKey3, 800, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn,
                    endColumns = endColumn
                }
            };
            
            // Without LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Forward);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 1, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }
            
            // With LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, this.columnIdPreread, PrereadIndexRangesGrbit.Forward | PrereadIndexRangesGrbit.FirstPageOnly);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 3, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }
        }

        /// <summary>
        /// Test PrereadRanges of one range only spanning one page
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of one range only spanning one page")]
        public void TestReadRangeSpanningOnePage()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn = new JET_INDEX_COLUMN[1];
            startColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 6, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn = new JET_INDEX_COLUMN[1];
            endColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 8, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn,
                    endColumns = endColumn
                }
            };

            // Without LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Forward);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 1, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }

            // With LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, this.columnIdPreread, PrereadIndexRangesGrbit.Forward | PrereadIndexRangesGrbit.FirstPageOnly);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 3, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }
        }

        /// <summary>
        /// Test PrereadRanges of one range too high
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of one range too high")]
        public void TestReadRangeTooHigh()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn = new JET_INDEX_COLUMN[1];
            startColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 150, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn = new JET_INDEX_COLUMN[1];
            endColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 160, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn,
                    endColumns = endColumn
                }
            };

            // Without LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Forward);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 1, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }

            // With LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, this.columnIdPreread, PrereadIndexRangesGrbit.Forward | PrereadIndexRangesGrbit.FirstPageOnly);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 0, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }
        }

        /// <summary>
        /// Test PrereadRanges of one range to end of table
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of one range to end of table")]
        public void TestReadKeyRangeToEndOfTable()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            Api.MakeKey(this.sesId, this.tableId, 10, MakeKeyGrbit.NewKey | MakeKeyGrbit.FullColumnStartLimit);
            byte[] startKey = new byte[SystemParameters.KeyMost];
            int startKeyLength;
            Api.JetRetrieveKey(this.sesId, this.tableId, startKey, startKey.Length, out startKeyLength, RetrieveKeyGrbit.RetrieveCopy);
            byte[][] startKeys = { startKey };
            int[] startKeysLength = { startKeyLength };

            Api.MakeKey(this.sesId, this.tableId, int.MaxValue, MakeKeyGrbit.NewKey | MakeKeyGrbit.FullColumnEndLimit);
            byte[] endKey = new byte[SystemParameters.KeyMost];
            int endKeyLength;
            Api.JetRetrieveKey(this.sesId, this.tableId, endKey, endKey.Length, out endKeyLength, RetrieveKeyGrbit.RetrieveCopy);
            byte[][] endKeys = { endKey };
            int[] endKeysLength = { endKeyLength };

            // Without LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.PrereadKeyRanges(this.sesId, this.tableId, startKeys, startKeysLength, endKeys, endKeysLength, 0, 1, out rangesRead, null, PrereadIndexRangesGrbit.Forward);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 23, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }

            // With LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.PrereadKeyRanges(this.sesId, this.tableId, startKeys, startKeysLength, endKeys, endKeysLength, 0, 1, out rangesRead, this.columnIdPreread, PrereadIndexRangesGrbit.Forward | PrereadIndexRangesGrbit.FirstPageOnly);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 15, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));   // LVs are only present upto key1 = 24 (14 LV pages, 1 preread from seek)
            }
        }

        /// <summary>
        /// Test PrereadRanges of one range to end of table
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of one range to end of table")]
        public void TestReadRangeToEndOfTable()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn = new JET_INDEX_COLUMN[1];
            startColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 10, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn = new JET_INDEX_COLUMN[1];
            endColumn[0] = this.CreateKeyColumn(this.columnIdKey1, int.MaxValue, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn,
                    endColumns = endColumn
                }
            };

            // Without LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Forward);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 23, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }

            // With LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, this.columnIdPreread, PrereadIndexRangesGrbit.Forward | PrereadIndexRangesGrbit.FirstPageOnly);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 15, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));   // LVs are only present upto key1 = 24 (14 LV pages, 1 preread from seek)
            }
        }

        /// <summary>
        /// Test PrereadRanges of one range only start
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of one range only start")]
        public void TestReadRangeOnlyStart()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn = new JET_INDEX_COLUMN[1];
            startColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 23, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn,
                }
            };

            // Without LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Forward);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 1, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }

            /*
            // With LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, this.columnIdPreread, PrereadIndexRangesGrbit.Forward | PrereadIndexRangesGrbit.FirstPageOnly);
                Assert.AreEqual(rangesRead, 1);

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 2);
            }
            */
        }

        // Enable 2 test cases below once O15 #2552539 is fixed.
        /*
        /// <summary>
        /// Test PrereadRanges of one range only end
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of one range only end")]
        public void TestReadRangeOnlyEnd()
        {
            JET_INDEX_COLUMN[] endColumn = new JET_INDEX_COLUMN[1];
            endColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 24, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    endColumns = endColumn,
                }
            };

            // Without LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Forward);
                Assert.AreEqual(rangesRead, 1);

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 6);
            }

            // With LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, this.columnIdPreread, PrereadIndexRangesGrbit.Forward | PrereadIndexRangesGrbit.FirstPageOnly);
                Assert.AreEqual(rangesRead, 1);

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 25);
            }
        }

        /// <summary>
        /// Test an open-ended pre-read range
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test an open-ended pre-read range")]
        public void TestReadRangeOpenEnded()
        {
            JET_INDEX_COLUMN[] endColumn = new JET_INDEX_COLUMN[1];
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                }
            };

            // Without LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Forward);
                Assert.AreEqual(rangesRead, 1);

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 25);
            }

            // With LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, this.columnIdPreread, PrereadIndexRangesGrbit.Forward | PrereadIndexRangesGrbit.FirstPageOnly);
                Assert.AreEqual(rangesRead, 1);

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 24);
            }
        }
        */

        /// <summary>
        /// Test PrereadRanges of one range too low
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of one range too low")]
        public void TestReadRangeTooLow()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn = new JET_INDEX_COLUMN[1];
            startColumn[0] = this.CreateKeyColumn(this.columnIdKey1, -60, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn = new JET_INDEX_COLUMN[1];
            endColumn[0] = this.CreateKeyColumn(this.columnIdKey1, -50, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn,
                    endColumns = endColumn
                }
            };

            // Without LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Forward);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 1, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }

            // With LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, this.columnIdPreread, PrereadIndexRangesGrbit.Forward | PrereadIndexRangesGrbit.FirstPageOnly);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 0, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }
        }

        /// <summary>
        /// Test PrereadRanges of one range out of order
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of one range out of order")]
        public void TestReadRangeOutOfOrder()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn = new JET_INDEX_COLUMN[1];
            startColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 8, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn = new JET_INDEX_COLUMN[1];
            endColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 6, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn,
                    endColumns = endColumn
                }
            };

            try
            {
                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Forward);
                Assert.Fail("Expect out of order PrereadRanges to fail");
            }
            catch (EsentInvalidParameterException)
            {
            }
        }

        /// <summary>
        /// Test PrereadRanges of one range spanning multiple pages
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of one range spanning multiple page")]
        public void TestReadRangeSpanningMultiplePage()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn = new JET_INDEX_COLUMN[1];
            startColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 16, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn = new JET_INDEX_COLUMN[1];
            endColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 40, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn,
                    endColumns = endColumn
                }
            };

            // Without LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Forward);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 7, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }

            // With LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, this.columnIdPreread, PrereadIndexRangesGrbit.Forward | PrereadIndexRangesGrbit.FirstPageOnly);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 10, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));    // LVs are only present upto key1 = 24
            }
        }

        /// <summary>
        /// Test PrereadRanges of one range only spanning one page backwards
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of one range only spanning one page backwards")]
        public void TestReadRangeSpanningOnePageBackwards()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn = new JET_INDEX_COLUMN[1];
            startColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 8, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn = new JET_INDEX_COLUMN[1];
            endColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 6, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn,
                    endColumns = endColumn
                }
            };

            // Without LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Backwards);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 1, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }

            // With LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, this.columnIdPreread, PrereadIndexRangesGrbit.Backwards | PrereadIndexRangesGrbit.FirstPageOnly);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 3, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }
        }

        /// <summary>
        /// Test PrereadRanges of one range out of order backwards
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of one range out of order backwards")]
        public void TestReadRangeOutOfOrderBackwards()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn = new JET_INDEX_COLUMN[1];
            startColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 6, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn = new JET_INDEX_COLUMN[1];
            endColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 8, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn,
                    endColumns = endColumn
                }
            };

            try
            {
                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Backwards);
                Assert.Fail("Expect out of order PrereadRanges to fail");
            }
            catch (EsentInvalidParameterException)
            {
            }
        }

        /// <summary>
        /// Test PrereadRanges of one range spanning multiple page backwards
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of one range spanning multiple page backwards")]
        public void TestReadRangeSpanningMultiplePageBackwards()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn = new JET_INDEX_COLUMN[1];
            startColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 40, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn = new JET_INDEX_COLUMN[1];
            endColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 16, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn,
                    endColumns = endColumn
                }
            };

            // Without LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Backwards);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 7, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }

            // With LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, this.columnIdPreread, PrereadIndexRangesGrbit.Backwards | PrereadIndexRangesGrbit.FirstPageOnly);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 10, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));    // LVs are only present upto key1 = 24
            }
        }

        /// <summary>
        /// Test PrereadRanges of one partial range
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of one partial range")]
        public void TestReadPartialRange()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn = new JET_INDEX_COLUMN[1];
            startColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 8, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn = new JET_INDEX_COLUMN[1];
            endColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 88, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn,
                    endColumns = endColumn
                }
            };

            // Without LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Forward);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.IsTrue((stat2.cPagePreread - stat1.cPagePreread) == 21, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }

            // With LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, this.columnIdPreread, PrereadIndexRangesGrbit.Forward | PrereadIndexRangesGrbit.FirstPageOnly);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.IsTrue((stat2.cPagePreread - stat1.cPagePreread) == 18, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }
        }

        /// <summary>
        /// Test PrereadRanges of multiple overlapping ranges
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of multiple overlapping ranges")]
        public void TestReadMultipleOverlappingRanges()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn1 = new JET_INDEX_COLUMN[1];
            startColumn1[0] = this.CreateKeyColumn(this.columnIdKey1, 5, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn1 = new JET_INDEX_COLUMN[1];
            endColumn1[0] = this.CreateKeyColumn(this.columnIdKey1, 10, JetRelop.Equals);
            JET_INDEX_COLUMN[] startColumn2 = new JET_INDEX_COLUMN[1];
            startColumn2[0] = this.CreateKeyColumn(this.columnIdKey1, 11, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn2 = new JET_INDEX_COLUMN[1];
            endColumn2[0] = this.CreateKeyColumn(this.columnIdKey1, 18, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn1,
                    endColumns = endColumn1
                },

                new JET_INDEX_RANGE
                {
                    startColumns = startColumn2,
                    endColumns = endColumn2
                }
            };

            // Without LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Forward);
                Assert.AreEqual(rangesRead, 2, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 4, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }

            // With LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, this.columnIdPreread, PrereadIndexRangesGrbit.Forward | PrereadIndexRangesGrbit.FirstPageOnly);
                Assert.AreEqual(rangesRead, 2, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 15, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }
        }

        /// <summary>
        /// Test PrereadRanges of multiple ranges in one page
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of multiple ranges in one page")]
        public void TestReadMultipleRangesOnePage()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn1 = new JET_INDEX_COLUMN[1];
            startColumn1[0] = this.CreateKeyColumn(this.columnIdKey1, 14, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn1 = new JET_INDEX_COLUMN[1];
            endColumn1[0] = this.CreateKeyColumn(this.columnIdKey1, 15, JetRelop.Equals);
            JET_INDEX_COLUMN[] startColumn2 = new JET_INDEX_COLUMN[1];
            startColumn2[0] = this.CreateKeyColumn(this.columnIdKey1, 16, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn2 = new JET_INDEX_COLUMN[1];
            endColumn2[0] = this.CreateKeyColumn(this.columnIdKey1, 16, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn1,
                    endColumns = endColumn1
                },

                new JET_INDEX_RANGE
                {
                    startColumns = startColumn2,
                    endColumns = endColumn2
                }
            };
            
            // Without LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Forward);
                Assert.AreEqual(rangesRead, 2, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 1, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }

            // With LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, this.columnIdPreread, PrereadIndexRangesGrbit.Forward | PrereadIndexRangesGrbit.FirstPageOnly);
                Assert.AreEqual(rangesRead, 2, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 3, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }
        }

        /// <summary>
        /// Test PrereadRanges of multiple ranges out of order
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of multiple ranges out of order")]
        public void TestReadMultipleRangesOutOfOrder()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn1 = new JET_INDEX_COLUMN[1];
            startColumn1[0] = this.CreateKeyColumn(this.columnIdKey1, 16, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn1 = new JET_INDEX_COLUMN[1];
            endColumn1[0] = this.CreateKeyColumn(this.columnIdKey1, 32, JetRelop.Equals);
            JET_INDEX_COLUMN[] startColumn2 = new JET_INDEX_COLUMN[1];
            startColumn2[0] = this.CreateKeyColumn(this.columnIdKey1, 31, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn2 = new JET_INDEX_COLUMN[1];
            endColumn2[0] = this.CreateKeyColumn(this.columnIdKey1, 40, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn1,
                    endColumns = endColumn1
                },

                new JET_INDEX_RANGE
                {
                    startColumns = startColumn2,
                    endColumns = endColumn2
                }
            };

            try
            {
                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Forward);
                Assert.Fail("Expect out of order PrereadRanges to fail");
            }
            catch (EsentInvalidParameterException)
            {
            }
        }

        /// <summary>
        /// Test PrereadRanges of multiple non-overlapping ranges
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of multiple non-overlapping ranges")]
        public void TestReadMultipleNonOverlappingRanges()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn1 = new JET_INDEX_COLUMN[1];
            startColumn1[0] = this.CreateKeyColumn(this.columnIdKey1, 16, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn1 = new JET_INDEX_COLUMN[1];
            endColumn1[0] = this.CreateKeyColumn(this.columnIdKey1, 23, JetRelop.Equals);
            JET_INDEX_COLUMN[] startColumn2 = new JET_INDEX_COLUMN[1];
            startColumn2[0] = this.CreateKeyColumn(this.columnIdKey1, 32, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn2 = new JET_INDEX_COLUMN[1];
            endColumn2[0] = this.CreateKeyColumn(this.columnIdKey1, 40, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn1,
                    endColumns = endColumn1
                },

                new JET_INDEX_RANGE
                {
                    startColumns = startColumn2,
                    endColumns = endColumn2
                }
            };

            // Without LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Forward);
                Assert.AreEqual(rangesRead, 2, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 6, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }

            // With LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, this.columnIdPreread, PrereadIndexRangesGrbit.Forward | PrereadIndexRangesGrbit.FirstPageOnly);
                Assert.AreEqual(rangesRead, 2, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 10, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));   // LVs are only present upto key1 = 24 (8 lv pages, 2 prereads from seeks)
            }
        }

        /// <summary>
        /// Test partial PrereadRanges of multiple ranges
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test partial PrereadRanges of multiple ranges")]
        public void TestReadPartialMultipleRanges()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn1 = new JET_INDEX_COLUMN[1];
            startColumn1[0] = this.CreateKeyColumn(this.columnIdKey1, 8, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn1 = new JET_INDEX_COLUMN[1];
            endColumn1[0] = this.CreateKeyColumn(this.columnIdKey1, 23, JetRelop.Equals);
            JET_INDEX_COLUMN[] startColumn2 = new JET_INDEX_COLUMN[1];
            startColumn2[0] = this.CreateKeyColumn(this.columnIdKey1, 32, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn2 = new JET_INDEX_COLUMN[1];
            endColumn2[0] = this.CreateKeyColumn(this.columnIdKey1, 96, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn1,
                    endColumns = endColumn1
                },

                new JET_INDEX_RANGE
                {
                    startColumns = startColumn2,
                    endColumns = endColumn2
                }
            };

            // Without LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Forward);
                Assert.AreEqual(rangesRead, 2, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.IsTrue((stat2.cPagePreread - stat1.cPagePreread) == 22, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }

            // With LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Forward | PrereadIndexRangesGrbit.FirstPageOnly);
                Assert.AreEqual(rangesRead, 2, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.IsTrue((stat2.cPagePreread - stat1.cPagePreread) == 0, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }
        }

        /// <summary>
        /// Test PrereadRanges of multiple overlapping ranges backwards
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of multiple overlapping ranges backwards")]
        public void TestReadMultipleOverlappingRangesBackwards()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn1 = new JET_INDEX_COLUMN[1];
            startColumn1[0] = this.CreateKeyColumn(this.columnIdKey1, 18, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn1 = new JET_INDEX_COLUMN[1];
            endColumn1[0] = this.CreateKeyColumn(this.columnIdKey1, 11, JetRelop.Equals);
            JET_INDEX_COLUMN[] startColumn2 = new JET_INDEX_COLUMN[1];
            startColumn2[0] = this.CreateKeyColumn(this.columnIdKey1, 10, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn2 = new JET_INDEX_COLUMN[1];
            endColumn2[0] = this.CreateKeyColumn(this.columnIdKey1, 5, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn1,
                    endColumns = endColumn1
                },

                new JET_INDEX_RANGE
                {
                    startColumns = startColumn2,
                    endColumns = endColumn2
                }
            };

            // Without LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Backwards);
                Assert.AreEqual(rangesRead, 2, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 4, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }

            // With LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, this.columnIdPreread, PrereadIndexRangesGrbit.Backwards | PrereadIndexRangesGrbit.FirstPageOnly);
                Assert.AreEqual(rangesRead, 2, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 15, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }
        }

        /// <summary>
        /// Test PrereadRanges of multiple ranges out of order backwards
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of multiple ranges out of order backwards")]
        public void TestReadMultipleRangesOutOfOrderBackwards()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn1 = new JET_INDEX_COLUMN[1];
            startColumn1[0] = this.CreateKeyColumn(this.columnIdKey1, 40, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn1 = new JET_INDEX_COLUMN[1];
            endColumn1[0] = this.CreateKeyColumn(this.columnIdKey1, 31, JetRelop.Equals);
            JET_INDEX_COLUMN[] startColumn2 = new JET_INDEX_COLUMN[1];
            startColumn2[0] = this.CreateKeyColumn(this.columnIdKey1, 32, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn2 = new JET_INDEX_COLUMN[1];
            endColumn2[0] = this.CreateKeyColumn(this.columnIdKey1, 16, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn1,
                    endColumns = endColumn1
                },

                new JET_INDEX_RANGE
                {
                    startColumns = startColumn2,
                    endColumns = endColumn2
                }
            };

            try
            {
                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Backwards);
                Assert.Fail("Expect out of order PrereadRanges to fail");
            }
            catch (EsentInvalidParameterException)
            {
            }
        }

        /// <summary>
        /// Test PrereadRanges of multiple non-overlapping ranges backwards
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of multiple non-overlapping ranges backwards")]
        public void TestReadMultipleNonOverlappingRangesBackwards()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn1 = new JET_INDEX_COLUMN[1];
            startColumn1[0] = this.CreateKeyColumn(this.columnIdKey1, 40, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn1 = new JET_INDEX_COLUMN[1];
            endColumn1[0] = this.CreateKeyColumn(this.columnIdKey1, 32, JetRelop.Equals);
            JET_INDEX_COLUMN[] startColumn2 = new JET_INDEX_COLUMN[1];
            startColumn2[0] = this.CreateKeyColumn(this.columnIdKey1, 23, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn2 = new JET_INDEX_COLUMN[1];
            endColumn2[0] = this.CreateKeyColumn(this.columnIdKey1, 16, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn1,
                    endColumns = endColumn1
                },

                new JET_INDEX_RANGE
                {
                    startColumns = startColumn2,
                    endColumns = endColumn2
                }
            };

            // Without LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Backwards);
                Assert.AreEqual(rangesRead, 2, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 6, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }

            // With LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, this.columnIdPreread, PrereadIndexRangesGrbit.Backwards | PrereadIndexRangesGrbit.FirstPageOnly);
                Assert.AreEqual(rangesRead, 2, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 10, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));   // LVs are only present upto key1 = 24
            }
        }

        /// <summary>
        /// Test PrereadRanges of part of the range array
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of part of the range array")]
        public void TestReadPartOfRangeArray()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn1 = new JET_INDEX_COLUMN[1];
            startColumn1[0] = this.CreateKeyColumn(this.columnIdKey1, 16, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn1 = new JET_INDEX_COLUMN[1];
            endColumn1[0] = this.CreateKeyColumn(this.columnIdKey1, 19, JetRelop.Equals);
            JET_INDEX_COLUMN[] startColumn2 = new JET_INDEX_COLUMN[1];
            startColumn2[0] = this.CreateKeyColumn(this.columnIdKey1, 20, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn2 = new JET_INDEX_COLUMN[1];
            endColumn2[0] = this.CreateKeyColumn(this.columnIdKey1, 28, JetRelop.Equals);
            JET_INDEX_COLUMN[] startColumn3 = new JET_INDEX_COLUMN[1];
            startColumn3[0] = this.CreateKeyColumn(this.columnIdKey1, 29, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn3 = new JET_INDEX_COLUMN[1];
            endColumn3[0] = this.CreateKeyColumn(this.columnIdKey1, 40, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn1,
                    endColumns = endColumn1
                },

                new JET_INDEX_RANGE
                {
                    startColumns = startColumn2,
                    endColumns = endColumn2
                },

                new JET_INDEX_RANGE
                {
                    startColumns = startColumn3,
                    endColumns = endColumn3
                }
            };

            // Without LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 1, 1, out rangesRead, null, PrereadIndexRangesGrbit.Forward);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 3, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }

            // With LV preread
            {
                JET_THREADSTATS stat1;
                VistaApi.JetGetThreadStats(out stat1);

                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 1, 1, out rangesRead, this.columnIdPreread, PrereadIndexRangesGrbit.Forward | PrereadIndexRangesGrbit.FirstPageOnly);
                Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

                JET_THREADSTATS stat2;
                VistaApi.JetGetThreadStats(out stat2);

                Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 6, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
            }
        }

        /// <summary>
        /// Test PrereadRanges of one range on secondary index
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test PrereadRanges of one range on secondary index")]
        public void TestReadRangeOnSecondaryIndex()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            Api.JetSetCurrentIndex(this.sesId, this.tableId, this.secIndexWithPrimaryName);

            // secondary index has 24 records on first page, 23 on rest
            // (verified with JetPageViewer)
            JET_INDEX_COLUMN[] startColumn = new JET_INDEX_COLUMN[1];
            startColumn[0] = this.CreateKeyColumn(this.columnIdKey2, 160, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn = new JET_INDEX_COLUMN[1];
            endColumn[0] = this.CreateKeyColumn(this.columnIdKey2, 700, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn,
                    endColumns = endColumn
                }
            };

            JET_THREADSTATS stat1;
            VistaApi.JetGetThreadStats(out stat1);

            int rangesRead;
            Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, null, PrereadIndexRangesGrbit.Forward);
            Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

            JET_THREADSTATS stat2;
            VistaApi.JetGetThreadStats(out stat2);

            Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 4, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));
        }

        /// <summary>
        /// MoveNext with filter does not keep reading beyond end of IndexRange
        /// Really belongs in FilteredDmlCurrency suite, but this test has a
        /// db more suited for this.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("MoveNext with filter does not keep reading beyond end of IndexRange")]
        public void TestFilteredMoveBeyondIndexRange()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            this.SeekToRecordClustered(16, SeekGrbit.SeekGE);
            this.VerifyCurrentRecord(16);
            this.MakeKeyAndSetIndexRangeClustered(24, SetIndexRangeGrbit.RangeUpperLimit);

            JET_INDEX_COLUMN filter = this.CreateKeyColumn(this.columnIdKey1, 20, JetRelop.LessThan);
            JET_INDEX_COLUMN[] filters = { filter };
            Windows8Api.JetSetCursorFilter(this.sesId, this.tableId, filters, CursorFilterGrbit.None);

            JET_THREADSTATS stat1;
            VistaApi.JetGetThreadStats(out stat1);

            this.MoveCursor(JET_Move.Next);
            this.VerifyCurrentRecord(17);
            this.MoveCursor(JET_Move.Next);
            this.VerifyCurrentRecord(18);
            this.MoveCursor(JET_Move.Next);
            this.VerifyCurrentRecord(19);
            this.MoveCursor(JET_Move.Next, typeof(EsentNoCurrentRecordException));
            this.VerifyCurrentRecord(20, typeof(EsentNoCurrentRecordException));

            JET_THREADSTATS stat2;
            VistaApi.JetGetThreadStats(out stat2);

            Assert.IsTrue(stat2.cPageRead - stat1.cPageRead <= 1, string.Format("stat2.cPageRead = {0}, stat1.cPageRead = {1}", stat2.cPageRead, stat1.cPageRead));
        }

        /// <summary>
        /// Test LV pre-reads with secondary index. Should fail
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("LV pre-reads with secondary index")]
        public void TestLvPrereadsWithSecIndex()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            Api.JetSetCurrentIndex(this.sesId, this.tableId, this.secIndexWithPrimaryName);

            JET_INDEX_COLUMN[] startColumn = new JET_INDEX_COLUMN[1];
            startColumn[0] = this.CreateKeyColumn(this.columnIdKey2, 160, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn = new JET_INDEX_COLUMN[1];
            endColumn[0] = this.CreateKeyColumn(this.columnIdKey2, 700, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn,
                    endColumns = endColumn
                }
            };

            try
            {
                int rangesRead;
                Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, this.columnIdPreread, PrereadIndexRangesGrbit.Forward);
                Assert.Fail("Expected: " + typeof(EsentInvalidPrereadException).Name);
            }
            catch (EsentInvalidPrereadException)
            {
            }
        }

        /// <summary>
        /// Verify that a failing preread with the 'Try' Preread overload does not throw an exception.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Verify that a failing preread with the 'Try' Preread overload does not throw an exception.")]
        public void TestTryLvPrereadsWithSecIndex()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            Api.JetSetCurrentIndex(this.sesId, this.tableId, this.secIndexWithPrimaryName);

            JET_INDEX_COLUMN[] startColumn = new JET_INDEX_COLUMN[1];
            startColumn[0] = this.CreateKeyColumn(this.columnIdKey2, 160, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn = new JET_INDEX_COLUMN[1];
            endColumn[0] = this.CreateKeyColumn(this.columnIdKey2, 700, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn,
                    endColumns = endColumn
                }
            };

            int rangesRead;
            Assert.IsFalse(Windows8Api.JetTryPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, this.columnIdPreread, PrereadIndexRangesGrbit.Forward));
        }

        /// <summary>
        /// Test LV pre-reads with intrinsic LV. Should skip intrinsic Lvs
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("LV pre-reads with intrinsic LV")]
        public void TestLvPrereadsWithIntrinsicLv()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            for (int i = 25; i < 30; i++)
            {
                this.UpdateExLv(this.tableId, i, 4, true);
            }
            
            JET_INDEX_COLUMN[] startColumn = new JET_INDEX_COLUMN[1];
            startColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 10, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn = new JET_INDEX_COLUMN[1];
            endColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 40, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn,
                    endColumns = endColumn
                }
            };

            JET_THREADSTATS stat1;
            VistaApi.JetGetThreadStats(out stat1);

            int rangesRead;
            JET_COLUMNID[] colIdPreread = { this.columnIdExLV, this.columnIdData };
            Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, colIdPreread, PrereadIndexRangesGrbit.Forward | PrereadIndexRangesGrbit.FirstPageOnly);
            Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

            JET_THREADSTATS stat2;
            VistaApi.JetGetThreadStats(out stat2);

            Assert.AreEqual(stat2.cPagePreread - stat1.cPagePreread, 22, string.Format("stat2.cPagePreread = {0}, stat1.cPagePreread = {1}", stat2.cPagePreread, stat1.cPagePreread));   // 15 lv pages + 7 prereads for seeking
        }

        /// <summary>
        /// Test LV pre-reads with default flags. Should read 8 pages per Lv
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("LV pre-reads with default flags")]
        public void TestLvPrereadsWithDefaultFlags()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            JET_INDEX_COLUMN[] startColumn = new JET_INDEX_COLUMN[1];
            startColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 6, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn = new JET_INDEX_COLUMN[1];
            endColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 12, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn,
                    endColumns = endColumn
                }
            };

            JET_THREADSTATS stat1;
            VistaApi.JetGetThreadStats(out stat1);

            int rangesRead;
            JET_COLUMNID[] colIdPreread = { this.columnIdExLV, this.columnIdData };
            Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, colIdPreread, PrereadIndexRangesGrbit.Forward);
            Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

            JET_THREADSTATS stat2;
            VistaApi.JetGetThreadStats(out stat2);

            Assert.IsTrue(stat2.cPagePreread - stat1.cPagePreread >= 53, string.Format("Expected >= 53, Actual: {0}", stat2.cPagePreread - stat1.cPagePreread));   // 53 lv pages + some prereads for seeking
        }

        /// <summary>
        /// Test max LV pre-reads limit. Should read 256 pages total
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test max LV pre-reads limit")]
        public void TestMaxLvPrereadLimit()
        {
            if (!EsentVersion.SupportsWindows8Features)
            {
                return;
            }

            for (int i = 25; i < 40; i++)
            {
                this.UpdateExLv(this.tableId, i, 8 * SystemParameters.LVChunkSizeMost, false);
            }
            
            // Force flush cache
            this.ForceFlushCache();

            // Increase cache size so the complete range can fit in
            this.SetCacheSize(1024);

             JET_INDEX_COLUMN[] startColumn = new JET_INDEX_COLUMN[1];
            startColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 0, JetRelop.Equals);
            JET_INDEX_COLUMN[] endColumn = new JET_INDEX_COLUMN[1];
            endColumn[0] = this.CreateKeyColumn(this.columnIdKey1, 50, JetRelop.Equals);
            JET_INDEX_RANGE[] indexRanges =
            {
                new JET_INDEX_RANGE
                {
                    startColumns = startColumn,
                    endColumns = endColumn
                }
            };

            JET_THREADSTATS stat1;
            VistaApi.JetGetThreadStats(out stat1);

            int rangesRead;
            JET_COLUMNID[] colIdPreread = { this.columnIdExLV, this.columnIdData };
            Windows8Api.JetPrereadIndexRanges(this.sesId, this.tableId, indexRanges, 0, indexRanges.Length, out rangesRead, colIdPreread, PrereadIndexRangesGrbit.Forward);
            Assert.AreEqual(rangesRead, 1, string.Format("rangesRead = {0}", rangesRead));

            JET_THREADSTATS stat2;
            VistaApi.JetGetThreadStats(out stat2);

            var cPreread = stat2.cPagePreread - stat1.cPagePreread;
            Assert.IsTrue(cPreread >= 250 && cPreread <= 274, string.Format("Expected between 250 & 274, Actual = {0}", cPreread));   // max 256 pages of Lv + some seeks
        }

        #endregion Preread Ranges tests

        #region Helpers

        /// <summary>
        /// Attaches a database
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="databasePath">The database path.</param>
        /// <param name="grbit">The grbit.</param>
        private void AttachDatabase(JET_SESID session, string databasePath, AttachDatabaseGrbit grbit)
        {
#if MANAGEDESENT_ON_WSA || MANAGEDESENT_EXTERNAL_RELEASE
            Api.JetAttachDatabase(session, databasePath, grbit);
#else
            this.AttachDatabaseInternal(session, databasePath, grbit);
#endif
        }

        /// <summary>
        /// Create table with 4 records per page
        /// </summary>
        private void CreateAndPopulateTable()
        {
            Api.JetBeginTransaction(this.sesId);

            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            int i;
            for (i = 0; i < 100; i++)
            {
                this.InsertRecord(tc.tableid, i);
            }

            Api.JetCloseTable(this.sesId, tc.tableid);
            Api.JetCommitTransaction(this.sesId, CommitTransactionGrbit.None);
        }

        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>The newly created table.</returns>
        private JET_TABLECREATE CreateTable(string tableName)
        {
            const string ColumnKey1Name = "columnkey1";
            const string ColumnKey2Name = "columnkey2";
            const string ColumnKey3Name = "columnkey3";
            const string ColumnDataName = "columndata";
            const string ColumnExLVName = "columnExLV";
            string clustIndexKey = string.Format("+{0}\0+{1}\0+{2}\0\0", ColumnKey1Name, ColumnKey2Name, ColumnKey3Name);
            string secIndexWithPrimaryKey = string.Format("+{0}\0-{1}\0\0", ColumnKey2Name, ColumnDataName);

            JET_COLUMNCREATE[] columnCreates =
            {
                new JET_COLUMNCREATE
                {
                    szColumnName = ColumnKey1Name,
                    coltyp = JET_coltyp.Long
                },

                new JET_COLUMNCREATE
                {
                    szColumnName = ColumnKey2Name,
                    coltyp = JET_coltyp.Long
                },

                new JET_COLUMNCREATE
                {
                    szColumnName = ColumnKey3Name,
                    coltyp = JET_coltyp.Long
                },

                new JET_COLUMNCREATE
                {
                    szColumnName = ColumnDataName,
                    coltyp = JET_coltyp.LongBinary
                },

                new JET_COLUMNCREATE
                {
                    szColumnName = ColumnExLVName,
                    coltyp = JET_coltyp.LongBinary
                },
            };

            JET_SPACEHINTS spaceHint = new JET_SPACEHINTS
            {
                cbInitial = PrereadRangesTests.PageSize * 50
            };

            JET_INDEXCREATE[] indexCreates = 
            {
                new JET_INDEXCREATE
                {
                    szIndexName = this.clustIndexName,
                    szKey = clustIndexKey,
                    cbKey = clustIndexKey.Length,
                    grbit = CreateIndexGrbit.IndexPrimary,
                    pSpaceHints = spaceHint
                },

                new JET_INDEXCREATE
                {
                    szIndexName = this.secIndexWithPrimaryName,
                    szKey = secIndexWithPrimaryKey,
                    cbKey = secIndexWithPrimaryKey.Length,
                    grbit = CreateIndexGrbit.None,
                    pSpaceHints = spaceHint
                }
            };

            JET_TABLECREATE tc = new JET_TABLECREATE
            {
                szTableName = tableName,
                rgcolumncreate = columnCreates,
                cColumns = columnCreates.Length,
                rgindexcreate = indexCreates,
                cIndexes = indexCreates.Length,
                grbit = CreateTableColumnIndexGrbit.None,
                ulPages = 110
            };

            Api.JetCreateTableColumnIndex3(this.sesId, this.dbId, tc);

            Assert.AreEqual<int>(8, tc.cCreated);  // 1 table + 5 colummns + 2 indexes.
            Assert.AreNotEqual<JET_TABLEID>(JET_TABLEID.Nil, tc.tableid);

            this.columnIdKey1 = Api.GetTableColumnid(this.sesId, tc.tableid, ColumnKey1Name);
            this.columnIdKey2 = Api.GetTableColumnid(this.sesId, tc.tableid, ColumnKey2Name);
            this.columnIdKey3 = Api.GetTableColumnid(this.sesId, tc.tableid, ColumnKey3Name);
            this.columnIdData = Api.GetTableColumnid(this.sesId, tc.tableid, ColumnDataName);
            this.columnIdExLV = Api.GetTableColumnid(this.sesId, tc.tableid, ColumnExLVName);

            this.columnIdPreread = new JET_COLUMNID[] { this.columnIdExLV };

            Assert.AreNotEqual<JET_COLUMNID>(JET_COLUMNID.Nil, this.columnIdKey1);
            Assert.AreNotEqual<JET_COLUMNID>(JET_COLUMNID.Nil, this.columnIdKey2);
            Assert.AreNotEqual<JET_COLUMNID>(JET_COLUMNID.Nil, this.columnIdKey3);
            Assert.AreNotEqual<JET_COLUMNID>(JET_COLUMNID.Nil, this.columnIdData);
            Assert.AreNotEqual<JET_COLUMNID>(JET_COLUMNID.Nil, this.columnIdExLV);
            Assert.AreNotEqual<JET_COLUMNID>(this.columnIdKey1, this.columnIdData);
            Assert.AreNotEqual<JET_COLUMNID>(this.columnIdKey2, this.columnIdData);
            Assert.AreNotEqual<JET_COLUMNID>(this.columnIdKey3, this.columnIdData);
            Assert.AreNotEqual<JET_COLUMNID>(this.columnIdExLV, this.columnIdData);

            return tc;
        }

        /// <summary>
        /// Gets the key2 default.
        /// </summary>
        /// <param name="key1">The key to retrieve.</param>
        /// <returns>The default data.</returns>
        private int GetKey2Default(int key1)
        {
            return 10 * key1;
        }

        /// <summary>
        /// Gets the key3 default.
        /// </summary>
        /// <param name="key2">The key2 to look up.</param>
        /// <returns>The default key3.</returns>
        private int GetKey3Default(int key2)
        {
            return 10 * key2;
        }

        /// <summary>
        /// Inserts the record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="key1">The key1 to insert.</param>
        /// <param name="key2">The key2 to insert.</param>
        /// <param name="key3">The key3 to insert.</param>
        /// <returns>The bookmark.</returns>
        private byte[] InsertRecord(JET_TABLEID tableId, int key1, int key2, int key3)
        {
            byte[] key1Array = BitConverter.GetBytes(key1);
            byte[] key2Array = BitConverter.GetBytes(key2);
            byte[] key3Array = BitConverter.GetBytes(key3);
            byte[] dataArray = new byte[DataLength];
            byte[] exLVArray = Any.BytesOfLength(Math.Min(key1 * SystemParameters.LVChunkSizeMost, 10 * SystemParameters.DatabasePageSize));   // number of chunks in the LV equals the record key, upto a max of 10 pages
            byte[] bookmark = new byte[(3 * sizeof(int)) + 3]; // +1 for ascending/descending info.

            Api.JetPrepareUpdate(this.sesId, tableId, JET_prep.Insert);
            Api.JetSetColumn(this.sesId, tableId, this.columnIdKey1, key1Array, key1Array.Length, SetColumnGrbit.None, null);
            Api.JetSetColumn(this.sesId, tableId, this.columnIdKey2, key2Array, key2Array.Length, SetColumnGrbit.None, null);
            Api.JetSetColumn(this.sesId, tableId, this.columnIdKey3, key3Array, key3Array.Length, SetColumnGrbit.None, null);
            Api.JetSetColumn(this.sesId, tableId, this.columnIdData, dataArray, dataArray.Length, SetColumnGrbit.IntrinsicLV, null);
            
            // Keep db small for faster tests
            if (key1 < 25)
            {
                Api.JetSetColumn(this.sesId, tableId, this.columnIdExLV, exLVArray, exLVArray.Length, SetColumnGrbit.SeparateLV, null);
            }

            int actualBookmarkSize;
            Api.JetUpdate(this.sesId, tableId, bookmark, bookmark.Length, out actualBookmarkSize);
            Assert.AreEqual<int>(bookmark.Length, actualBookmarkSize);

            return bookmark;
        }

        /// <summary>
        /// Inserts the record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="key1">The key1 to insert.</param>
        /// <param name="key2">The key2 to insert.</param>
        /// <returns>The bookmark.</returns>
        private byte[] InsertRecord(JET_TABLEID tableId, int key1, int key2)
        {
            int key3 = this.GetKey3Default(key2);

            return this.InsertRecord(tableId, key1, key2, key3);
        }

        /// <summary>
        /// Inserts the record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="key1">The key to insert.</param>
        /// <returns>The bookmark of the record.</returns>
        private byte[] InsertRecord(JET_TABLEID tableId, int key1)
        {
            int key2 = this.GetKey2Default(key1);

            return this.InsertRecord(tableId, key1, key2);
        }

        /// <summary>
        /// Update the extrinsic Lv of a record
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="key1">The key to update.</param>
        /// <param name="lvSize">Size in bytes of the lv.</param>
        /// <param name="isIntrinsic">Force Lv to be intrinsic or extrinsic.</param>
        private void UpdateExLv(JET_TABLEID tableId, int key1, int lvSize, bool isIntrinsic)
        {
            this.SeekToRecordClustered(key1, SeekGrbit.SeekGE);
            byte[] lv = Any.BytesOfLength(lvSize);

            Api.JetPrepareUpdate(this.sesId, tableId, JET_prep.Replace);
            Api.JetSetColumn(
                this.sesId,
                tableId,
                this.columnIdExLV,
                lv,
                lv.Length,
                SetColumnGrbit.OverwriteLV | (isIntrinsic ? SetColumnGrbit.IntrinsicLV : SetColumnGrbit.SeparateLV),
                null);
            Api.JetUpdate(this.sesId, tableId);
        }

        /// <summary>
        /// Create a filter.
        /// </summary>
        /// <param name="columnid">The column id.</param>
        /// <param name="value">The value.</param>
        /// <param name="relop">The operator.</param>
        /// <returns>The filter.</returns>
        private JET_INDEX_COLUMN CreateKeyColumn(JET_COLUMNID columnid, int value, JetRelop relop)
        {
            return new JET_INDEX_COLUMN()
            {
                columnid = columnid,
                relop = relop,
                pvData = BitConverter.GetBytes(value)
            };
        }

        /// <summary>
        /// Moves the cursor.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="exTypeExpected">The ex type expected.</param>
        private void MoveCursor(JET_Move offset, Type exTypeExpected)
        {
            try
            {
                Api.JetMove(this.sesId, this.tableId, offset, MoveGrbit.None);
                if (exTypeExpected != null)
                {
                    Assert.Fail("Should have thrown {0}", exTypeExpected);
                }
            }
            catch (EsentException ex)
            {
                if (exTypeExpected != null)
                {
                    Assert.AreEqual<Type>(exTypeExpected, ex.GetType());
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Moves the cursor.
        /// </summary>
        /// <param name="offset">The offset.</param>
        private void MoveCursor(JET_Move offset)
        {
            this.MoveCursor(offset, null);
        }

        /// <summary>
        /// Verifies the current record.
        /// </summary>
        /// <param name="keyExpected">The key expected.</param>
        /// <param name="exTypeExpected">The ex type expected.</param>
        private void VerifyCurrentRecord(int keyExpected, Type exTypeExpected)
        {
            byte[] keyArray = new byte[sizeof(int)];

            JET_RETRIEVECOLUMN[] retrieveColumns = new[]
            {
                new JET_RETRIEVECOLUMN
                {
                    columnid = this.columnIdKey1,
                    pvData = keyArray,
                    cbData = keyArray.Length,
                    itagSequence = 1
                }
            };

            try
            {
                JET_wrn err = Api.JetRetrieveColumns(this.sesId, this.tableId, retrieveColumns, retrieveColumns.Length);

                if (exTypeExpected != null)
                {
                    Assert.Fail("Should have thrown {0}", exTypeExpected);
                }

                Assert.AreEqual<JET_wrn>(JET_wrn.Success, err);
                Assert.AreEqual<int>(keyArray.Length, retrieveColumns[0].cbActual);
                int keyActual = BitConverter.ToInt32(keyArray, 0);
                Assert.AreEqual<int>(keyExpected, keyActual);
            }
            catch (EsentException ex)
            {
                if (exTypeExpected != null)
                {
                    Assert.AreEqual<Type>(exTypeExpected, ex.GetType());
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Verifies the current record.
        /// </summary>
        /// <param name="keyExpected">The key expected.</param>
        private void VerifyCurrentRecord(int keyExpected)
        {
            this.VerifyCurrentRecord(keyExpected, null);
        }

        /// <summary>
        /// Makes the key clustered.
        /// </summary>
        /// <param name="key">The key to make.</param>
        /// <param name="start">Is the key for start or end limit.</param>
        private void MakeKeyClustered(int key, bool start)
        {
            byte[] keyArray = BitConverter.GetBytes(key);
            Api.JetMakeKey(this.sesId, this.tableId, keyArray, keyArray.Length, MakeKeyGrbit.NewKey | (start ? MakeKeyGrbit.FullColumnStartLimit : MakeKeyGrbit.FullColumnEndLimit));
        }

        /// <summary>
        /// Seeks to record clustered.
        /// </summary>
        /// <param name="key">The key to seek.</param>
        /// <param name="seekGrbit">The seek grbit.</param>
        private void SeekToRecordClustered(int key, SeekGrbit seekGrbit)
        {
            this.MakeKeyClustered(key, true);
            Api.JetSeek(this.sesId, this.tableId, seekGrbit);
        }

        /// <summary>
        /// Makes the key and set index range clustered.
        /// </summary>
        /// <param name="key">The key to make.</param>
        /// <param name="setIndexRangeGrbit">The set index range grbit.</param>
        private void MakeKeyAndSetIndexRangeClustered(int key, SetIndexRangeGrbit setIndexRangeGrbit)
        {
            this.MakeKeyClustered(key, false);
            Api.JetSetIndexRange(this.sesId, this.tableId, setIndexRangeGrbit);
        }

        /// <summary>
        /// Force ese cache to flush all its contents to disk
        /// </summary>
        private void ForceFlushCache()
        {
            Api.JetCommitTransaction(this.sesId, CommitTransactionGrbit.None);

            Api.JetCloseTable(this.sesId, this.tableId);
            Api.JetCloseDatabase(this.sesId, this.dbId, CloseDatabaseGrbit.None);
            Api.JetDetachDatabase(this.sesId, this.database);
            this.AttachDatabase(this.sesId, this.database, Windows8Grbits.PurgeCacheOnAttach);
            Api.JetOpenDatabase(this.sesId, this.database, null, out this.dbId, OpenDatabaseGrbit.None);
            Api.JetOpenTable(this.sesId, this.dbId, this.tableName, null, 0, OpenTableGrbit.None, out this.tableId);

            Api.JetBeginTransaction(this.sesId);
        }

        /// <summary>
        /// Sets the cache size to a fixed number
        /// </summary>
        /// <param name="cacheSize">Cache size in pages</param>
        private void SetCacheSize(int cacheSize)
        {
            SystemParameters.CacheSizeMin = cacheSize;
            SystemParameters.CacheSizeMax = cacheSize;
            while (SystemParameters.CacheSize != cacheSize)
            {
                EseInteropTestHelper.ThreadSleep(1);
            }
        }

        #endregion Helpers
    }
}
