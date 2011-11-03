//-----------------------------------------------------------------------
// <copyright file="DMLCurrencyTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using Microsoft.Isam.Esent;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// DML and currency tests.
    /// </summary>
    [TestClass]
    [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules",
        "SA1305:FieldNamesMustNotUseHungarianNotation",
        Justification = "Reviewed. Suppression is OK here.")]
    public class DmlCurrencyTests
    {
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
        /// Secondary index name to create (does not primary key column).
        /// </summary>
        private readonly string secIndexWithoutPrimaryName = "secondarywithoutprimary";

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
        /// Column ID of the column that makes up the primary index.
        /// </summary>
        private JET_COLUMNID columnIdKey;

        /// <summary>
        /// Column ID of the column that makes up the data portion of the record.
        /// </summary>
        private JET_COLUMNID columnIdData1;

        /// <summary>
        /// Column ID of the column that makes up the second part of the data portion of the record.
        /// </summary>
        private JET_COLUMNID columnIdData2;

        #region Setup/Teardown

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// </summary>
        [TestInitialize]
        [Description("Setup for each test in DmlCurrencyTests")]
        public void TestSetup()
        {
            this.directory = SetupHelper.CreateRandomDirectory();
            this.database = Path.Combine(this.directory, "database.edb");
            this.instance = SetupHelper.CreateNewInstance(this.directory);

            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesId, string.Empty, string.Empty);

            JET_DBID dbId;
            Api.JetCreateDatabase(this.sesId, this.database, string.Empty, out dbId, CreateDatabaseGrbit.None);
            Api.JetCloseDatabase(this.sesId, dbId, CloseDatabaseGrbit.None);

            Api.JetOpenDatabase(this.sesId, this.database, null, out this.dbId, OpenDatabaseGrbit.None);
            Api.JetBeginTransaction(this.sesId);
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
                    Api.JetCommitTransaction(this.sesId, CommitTransactionGrbit.LazyFlush);
                }
                catch (EsentNotInTransactionException)
                {
                    break;
                }
            }

            Api.JetCloseDatabase(this.sesId, this.dbId, CloseDatabaseGrbit.None);

            Api.JetEndSession(this.sesId, EndSessionGrbit.None);
            Api.JetTerm(this.instance);
            Cleanup.DeleteDirectoryWithRetry(this.directory);
        }

        #endregion Setup/Teardown

        #region DML and currency Tests

        /// <summary>
        /// Navigation: move-first, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: move-first, clustered index")]
        public void NavigationMoveFirstClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            this.InsertRecord(tc.tableid, 2);
            this.InsertRecord(tc.tableid, 1);
            this.InsertRecord(tc.tableid, 3);

            this.MoveCursor(tc.tableid, JET_Move.First);

            this.VerifyCurrentRecord(tc.tableid, 1);
        }

        /// <summary>
        /// Navigation: move-last, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: move-last, clustered index")]
        public void NavigationMoveLastClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            this.InsertRecord(tc.tableid, 2);
            this.InsertRecord(tc.tableid, 3);
            this.InsertRecord(tc.tableid, 1);

            this.MoveCursor(tc.tableid, JET_Move.Last);

            this.VerifyCurrentRecord(tc.tableid, 3);
        }

        /// <summary>
        /// Navigation: move-next, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: move-next, clustered index")]
        public void NavigationMoveNextClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            this.InsertRecord(tc.tableid, 2);
            this.InsertRecord(tc.tableid, 1);
            this.InsertRecord(tc.tableid, 3);
            this.MoveCursor(tc.tableid, JET_Move.First);

            this.MoveCursor(tc.tableid, JET_Move.Next);

            this.VerifyCurrentRecord(tc.tableid, 2);
        }

        /// <summary>
        /// Navigation: move-previous, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: move-previous, clustered index")]
        public void NavigationMovePreviousClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            this.InsertRecord(tc.tableid, 2);
            this.InsertRecord(tc.tableid, 3);
            this.InsertRecord(tc.tableid, 1);
            this.MoveCursor(tc.tableid, JET_Move.Last);

            this.MoveCursor(tc.tableid, JET_Move.Previous);

            this.VerifyCurrentRecord(tc.tableid, 2);
        }

        /// <summary>
        /// Navigation: move-first, secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: move-first, secondary index")]
        public void NavigationMoveFirstSecondary()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            this.InsertRecord(tc.tableid, 1, 10);
            this.InsertRecord(tc.tableid, 2, 10);
            this.InsertRecord(tc.tableid, -3, 30);
            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithPrimaryName);

            this.MoveCursor(tc.tableid, JET_Move.First);

            this.VerifyCurrentRecord(tc.tableid, 2, 10);
        }

        /// <summary>
        /// Navigation: move-last, secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: move-last, secondary index")]
        public void NavigationMoveLastSecondary()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            this.InsertRecord(tc.tableid, -2, 30);
            this.InsertRecord(tc.tableid, -3, 30);
            this.InsertRecord(tc.tableid, 1, 10);
            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithPrimaryName);

            this.MoveCursor(tc.tableid, JET_Move.Last);

            this.VerifyCurrentRecord(tc.tableid, -3, 30);
        }

        /// <summary>
        /// Navigation: move-next, secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: move-next, secondary index")]
        public void NavigationMoveNextSecondary()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            this.InsertRecord(tc.tableid, 1, 10);
            this.InsertRecord(tc.tableid, 2, 10);
            this.InsertRecord(tc.tableid, -3, 30);
            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithPrimaryName);
            this.MoveCursor(tc.tableid, JET_Move.First);

            this.MoveCursor(tc.tableid, JET_Move.Next);

            this.VerifyCurrentRecord(tc.tableid, 1, 10);
        }

        /// <summary>
        /// Navigation: move-previous, secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: move-previous, secondary index")]
        public void NavigationMovePreviousSecondary()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            this.InsertRecord(tc.tableid, -2, 30);
            this.InsertRecord(tc.tableid, -3, 30);
            this.InsertRecord(tc.tableid, 1, 10);
            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithPrimaryName);
            this.MoveCursor(tc.tableid, JET_Move.Last);

            this.MoveCursor(tc.tableid, JET_Move.Previous);

            this.VerifyCurrentRecord(tc.tableid, -2, 30);
        }

        /// <summary>
        /// Navigation: seek-eq, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: seek-eq, clustered index")]
        public void NavigationSeekEqClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            this.SeekToRecordClustered(tc.tableid, 1, SeekGrbit.SeekEQ, JET_wrn.Success, typeof(EsentRecordNotFoundException));

            this.InsertRecord(tc.tableid, 1);

            this.SeekToRecordClustered(tc.tableid, 1, SeekGrbit.SeekEQ, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 1);

            this.InsertRecord(tc.tableid, 3);

            this.SeekToRecordClustered(tc.tableid, 0, SeekGrbit.SeekEQ, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.SeekToRecordClustered(tc.tableid, 1, SeekGrbit.SeekEQ, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 1);
            this.SeekToRecordClustered(tc.tableid, 2, SeekGrbit.SeekEQ, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.SeekToRecordClustered(tc.tableid, 3, SeekGrbit.SeekEQ, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 3);
            this.SeekToRecordClustered(tc.tableid, 4, SeekGrbit.SeekEQ, JET_wrn.Success, typeof(EsentRecordNotFoundException));
        }

        /// <summary>
        /// Navigation: seek-lt, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: seek-lt, clustered index")]
        public void NavigationSeekLtClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            this.SeekToRecordClustered(tc.tableid, 1, SeekGrbit.SeekLT, JET_wrn.Success, typeof(EsentRecordNotFoundException));

            this.InsertRecord(tc.tableid, 1);

            this.SeekToRecordClustered(tc.tableid, 1, SeekGrbit.SeekLT, JET_wrn.Success, typeof(EsentRecordNotFoundException));

            this.InsertRecord(tc.tableid, 3);

            this.SeekToRecordClustered(tc.tableid, 0, SeekGrbit.SeekLT, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.SeekToRecordClustered(tc.tableid, 1, SeekGrbit.SeekLT, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.SeekToRecordClustered(tc.tableid, 2, SeekGrbit.SeekLT, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 1);
            this.SeekToRecordClustered(tc.tableid, 3, SeekGrbit.SeekLT, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 1);
            this.SeekToRecordClustered(tc.tableid, 4, SeekGrbit.SeekLT, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 3);
            this.SeekToRecordClustered(tc.tableid, 5, SeekGrbit.SeekLT, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 3);
        }

        /// <summary>
        /// Navigation: seek-le, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: seek-le, clustered index")]
        public void NavigationSeekLeClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            this.SeekToRecordClustered(tc.tableid, 1, SeekGrbit.SeekLE, JET_wrn.Success, typeof(EsentRecordNotFoundException));

            this.InsertRecord(tc.tableid, 1);

            this.SeekToRecordClustered(tc.tableid, 1, SeekGrbit.SeekLE, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 1);

            this.InsertRecord(tc.tableid, 3);

            this.SeekToRecordClustered(tc.tableid, 0, SeekGrbit.SeekLE, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.SeekToRecordClustered(tc.tableid, 1, SeekGrbit.SeekLE, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 1);
            this.SeekToRecordClustered(tc.tableid, 2, SeekGrbit.SeekLE, JET_wrn.SeekNotEqual, null);
            this.VerifyCurrentRecord(tc.tableid, 1);
            this.SeekToRecordClustered(tc.tableid, 3, SeekGrbit.SeekLE, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 3);
            this.SeekToRecordClustered(tc.tableid, 4, SeekGrbit.SeekLE, JET_wrn.SeekNotEqual, null);
            this.VerifyCurrentRecord(tc.tableid, 3);
            this.SeekToRecordClustered(tc.tableid, 5, SeekGrbit.SeekLE, JET_wrn.SeekNotEqual, null);
            this.VerifyCurrentRecord(tc.tableid, 3);
        }

        /// <summary>
        /// Navigation: seek-gt, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: seek-gt, clustered index")]
        public void NavigationSeekGtClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            this.SeekToRecordClustered(tc.tableid, 1, SeekGrbit.SeekGT, JET_wrn.Success, typeof(EsentRecordNotFoundException));

            this.InsertRecord(tc.tableid, 1);

            this.SeekToRecordClustered(tc.tableid, 1, SeekGrbit.SeekGT, JET_wrn.Success, typeof(EsentRecordNotFoundException));

            this.InsertRecord(tc.tableid, 3);

            this.SeekToRecordClustered(tc.tableid, 0, SeekGrbit.SeekGT, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 1);
            this.SeekToRecordClustered(tc.tableid, 1, SeekGrbit.SeekGT, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 3);
            this.SeekToRecordClustered(tc.tableid, 2, SeekGrbit.SeekGT, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 3);
            this.SeekToRecordClustered(tc.tableid, 3, SeekGrbit.SeekGT, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.SeekToRecordClustered(tc.tableid, 4, SeekGrbit.SeekGT, JET_wrn.Success, typeof(EsentRecordNotFoundException));
        }

        /// <summary>
        /// Navigation: seek-ge, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: seek-ge, clustered index")]
        public void NavigationSeekGeClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            this.SeekToRecordClustered(tc.tableid, 1, SeekGrbit.SeekGE, JET_wrn.Success, typeof(EsentRecordNotFoundException));

            this.InsertRecord(tc.tableid, 1);

            this.SeekToRecordClustered(tc.tableid, 1, SeekGrbit.SeekGE, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 1);

            this.InsertRecord(tc.tableid, 3);

            this.SeekToRecordClustered(tc.tableid, 0, SeekGrbit.SeekGE, JET_wrn.SeekNotEqual, null);
            this.VerifyCurrentRecord(tc.tableid, 1);
            this.SeekToRecordClustered(tc.tableid, 1, SeekGrbit.SeekGE, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 1);
            this.SeekToRecordClustered(tc.tableid, 2, SeekGrbit.SeekGE, JET_wrn.SeekNotEqual, null);
            this.VerifyCurrentRecord(tc.tableid, 3);
            this.SeekToRecordClustered(tc.tableid, 3, SeekGrbit.SeekGE, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 3);
            this.SeekToRecordClustered(tc.tableid, 4, SeekGrbit.SeekGE, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.SeekToRecordClustered(tc.tableid, 5, SeekGrbit.SeekGE, JET_wrn.Success, typeof(EsentRecordNotFoundException));
        }

        /// <summary>
        /// Navigation: seek-set-index-range, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: seek-set-index-range, clustered index")]
        public void NavigationSeekSetIndexRangeClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            this.InsertRecord(tc.tableid, 1);
            this.InsertRecord(tc.tableid, 2);
            this.InsertRecord(tc.tableid, 3);

            this.SeekToRecordClustered(tc.tableid, 2, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 2);
            this.MoveCursor(tc.tableid, JET_Move.Next, typeof(EsentNoCurrentRecordException));
            this.VerifyCurrentRecord(tc.tableid, 2, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Next, typeof(EsentNoCurrentRecordException));
            this.VerifyCurrentRecord(tc.tableid, 2, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 3);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 2);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 3);
        }

        /// <summary>
        /// Navigation: seek-eq, secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: seek-eq, secondary index")]
        public void NavigationSeekEqSecondary()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithoutPrimaryName);

            this.SeekToRecordSecondary(tc.tableid, 10, 100, SeekGrbit.SeekEQ, JET_wrn.Success, typeof(EsentRecordNotFoundException));

            this.InsertRecord(tc.tableid, 5, 20, 200);
            this.InsertRecord(tc.tableid, 2, 20, 200);

            this.SeekToRecordSecondary(tc.tableid, 20, 200, SeekGrbit.SeekEQ, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 2, 20, 200);

            this.InsertRecord(tc.tableid, 3, 30, 200);
            this.InsertRecord(tc.tableid, 1, 10, 100);
            this.InsertRecord(tc.tableid, 4, 20, 201);

            this.SeekToRecordSecondary(tc.tableid, 20, 100, SeekGrbit.SeekEQ, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.SeekToRecordSecondary(tc.tableid, 20, 200, SeekGrbit.SeekEQ, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 2, 20, 200);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 5, 20, 200);
            this.SeekToRecordSecondary(tc.tableid, 20, 300, SeekGrbit.SeekEQ, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.VerifyCurrentRecord(tc.tableid, 2, 20, 200, typeof(EsentNoCurrentRecordException));
        }

        /// <summary>
        /// Navigation: seek-lt, secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: seek-lt, secondary index")]
        public void NavigationSeekLtSecondary()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithoutPrimaryName);

            this.SeekToRecordSecondary(tc.tableid, 10, 100, SeekGrbit.SeekLT, JET_wrn.Success, typeof(EsentRecordNotFoundException));

            this.InsertRecord(tc.tableid, 5, 20, 200);
            this.InsertRecord(tc.tableid, 2, 20, 200);

            this.SeekToRecordSecondary(tc.tableid, 20, 200, SeekGrbit.SeekLT, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.VerifyCurrentRecord(tc.tableid, 2, 20, 200, typeof(EsentNoCurrentRecordException));

            this.InsertRecord(tc.tableid, 3, 30, 200);
            this.InsertRecord(tc.tableid, 1, 10, 100);
            this.InsertRecord(tc.tableid, 4, 20, 201);

            this.SeekToRecordSecondary(tc.tableid, 9, 100, SeekGrbit.SeekLT, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.VerifyCurrentRecord(tc.tableid, 1, 10, 100, typeof(EsentNoCurrentRecordException));
            this.SeekToRecordSecondary(tc.tableid, 20, 100, SeekGrbit.SeekLT, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 1, 10, 100);
            this.SeekToRecordSecondary(tc.tableid, 20, 200, SeekGrbit.SeekLT, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 1, 10, 100);
            this.SeekToRecordSecondary(tc.tableid, 20, 201, SeekGrbit.SeekLT, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 5, 20, 200);
            this.SeekToRecordSecondary(tc.tableid, 20, 300, SeekGrbit.SeekLT, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 4, 20, 201);
            this.SeekToRecordSecondary(tc.tableid, 40, 100, SeekGrbit.SeekLT, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 3, 30, 200);
        }

        /// <summary>
        /// Navigation: seek-le, secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: seek-le, secondary index")]
        public void NavigationSeekLeSecondary()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithoutPrimaryName);

            this.SeekToRecordSecondary(tc.tableid, 10, 100, SeekGrbit.SeekLE, JET_wrn.Success, typeof(EsentRecordNotFoundException));

            this.InsertRecord(tc.tableid, 5, 20, 200);
            this.InsertRecord(tc.tableid, 2, 20, 200);

            this.SeekToRecordSecondary(tc.tableid, 20, 200, SeekGrbit.SeekLE, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 2, 20, 200);

            this.InsertRecord(tc.tableid, 3, 30, 200);
            this.InsertRecord(tc.tableid, 1, 10, 100);
            this.InsertRecord(tc.tableid, 4, 20, 201);

            this.SeekToRecordSecondary(tc.tableid, 9, 100, SeekGrbit.SeekLE, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.VerifyCurrentRecord(tc.tableid, 1, 10, 100, typeof(EsentNoCurrentRecordException));
            this.SeekToRecordSecondary(tc.tableid, 20, 100, SeekGrbit.SeekLE, JET_wrn.SeekNotEqual, null);
            this.VerifyCurrentRecord(tc.tableid, 1, 10, 100);
            this.SeekToRecordSecondary(tc.tableid, 20, 200, SeekGrbit.SeekLE, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 2, 20, 200);
            this.SeekToRecordSecondary(tc.tableid, 20, 201, SeekGrbit.SeekLE, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 4, 20, 201);
            this.SeekToRecordSecondary(tc.tableid, 20, 300, SeekGrbit.SeekLE, JET_wrn.SeekNotEqual, null);
            this.VerifyCurrentRecord(tc.tableid, 4, 20, 201);
            this.SeekToRecordSecondary(tc.tableid, 40, 100, SeekGrbit.SeekLE, JET_wrn.SeekNotEqual, null);
            this.VerifyCurrentRecord(tc.tableid, 3, 30, 200);
        }

        /// <summary>
        /// Navigation: seek-gt, secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: seek-gt, secondary index")]
        public void NavigationSeekGtSecondary()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithoutPrimaryName);

            this.SeekToRecordSecondary(tc.tableid, 10, 100, SeekGrbit.SeekGT, JET_wrn.Success, typeof(EsentRecordNotFoundException));

            this.InsertRecord(tc.tableid, 5, 20, 200);
            this.InsertRecord(tc.tableid, 2, 20, 200);

            this.SeekToRecordSecondary(tc.tableid, 20, 200, SeekGrbit.SeekGT, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.VerifyCurrentRecord(tc.tableid, 2, 20, 200, typeof(EsentNoCurrentRecordException));

            this.InsertRecord(tc.tableid, 3, 30, 200);
            this.InsertRecord(tc.tableid, 1, 10, 100);
            this.InsertRecord(tc.tableid, 4, 20, 201);

            this.SeekToRecordSecondary(tc.tableid, 9, 100, SeekGrbit.SeekGT, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 1, 10, 100);
            this.SeekToRecordSecondary(tc.tableid, 20, 100, SeekGrbit.SeekGT, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 2, 20, 200);
            this.SeekToRecordSecondary(tc.tableid, 20, 200, SeekGrbit.SeekGT, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 4, 20, 201);
            this.SeekToRecordSecondary(tc.tableid, 20, 201, SeekGrbit.SeekGT, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 3, 30, 200);
            this.SeekToRecordSecondary(tc.tableid, 20, 300, SeekGrbit.SeekGT, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 3, 30, 200);
            this.SeekToRecordSecondary(tc.tableid, 40, 100, SeekGrbit.SeekGT, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.VerifyCurrentRecord(tc.tableid, 3, 30, 200, typeof(EsentNoCurrentRecordException));
        }

        /// <summary>
        /// Navigation: seek-ge, secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: seek-ge, secondary index")]
        public void NavigationSeekGeSecondary()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithoutPrimaryName);

            this.SeekToRecordSecondary(tc.tableid, 10, 100, SeekGrbit.SeekGE, JET_wrn.Success, typeof(EsentRecordNotFoundException));

            this.InsertRecord(tc.tableid, 5, 20, 200);
            this.InsertRecord(tc.tableid, 2, 20, 200);

            this.SeekToRecordSecondary(tc.tableid, 20, 200, SeekGrbit.SeekGE, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 2, 20, 200);

            this.InsertRecord(tc.tableid, 3, 30, 200);
            this.InsertRecord(tc.tableid, 1, 10, 100);
            this.InsertRecord(tc.tableid, 4, 20, 201);

            this.SeekToRecordSecondary(tc.tableid, 9, 100, SeekGrbit.SeekGE, JET_wrn.SeekNotEqual, null);
            this.VerifyCurrentRecord(tc.tableid, 1, 10, 100);
            this.SeekToRecordSecondary(tc.tableid, 20, 100, SeekGrbit.SeekGE, JET_wrn.SeekNotEqual, null);
            this.VerifyCurrentRecord(tc.tableid, 2, 20, 200);
            this.SeekToRecordSecondary(tc.tableid, 20, 200, SeekGrbit.SeekGE, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 2, 20, 200);
            this.SeekToRecordSecondary(tc.tableid, 20, 201, SeekGrbit.SeekGE, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 4, 20, 201);
            this.SeekToRecordSecondary(tc.tableid, 20, 300, SeekGrbit.SeekGE, JET_wrn.SeekNotEqual, null);
            this.VerifyCurrentRecord(tc.tableid, 3, 30, 200);
            this.SeekToRecordSecondary(tc.tableid, 40, 100, SeekGrbit.SeekGE, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.VerifyCurrentRecord(tc.tableid, 3, 30, 200, typeof(EsentNoCurrentRecordException));
        }

        /// <summary>
        /// Navigation: seek-set-index-range, secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation: seek-set-index-range, secondary index")]
        public void NavigationSeekSetIndexRangeSecondary()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithoutPrimaryName);

            this.InsertRecord(tc.tableid, 5, 20, 200);
            this.InsertRecord(tc.tableid, 2, 20, 200);
            this.InsertRecord(tc.tableid, 3, 30, 200);
            this.InsertRecord(tc.tableid, 1, 10, 100);
            this.InsertRecord(tc.tableid, 4, 20, 201);

            this.SeekToRecordSecondary(tc.tableid, 20, 200, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange, JET_wrn.Success, null);
            this.VerifyCurrentRecord(tc.tableid, 2, 20, 200);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 5, 20, 200);
            this.MoveCursor(tc.tableid, JET_Move.Next, typeof(EsentNoCurrentRecordException));
            this.VerifyCurrentRecord(tc.tableid, 5, 2, 200, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Next, typeof(EsentNoCurrentRecordException));
            this.VerifyCurrentRecord(tc.tableid, 5, 2, 200, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 3, 30, 200);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 4, 20, 201);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 5, 20, 200);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 4, 20, 201);
        }

        /// <summary>
        /// Navigation on empty table: clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation on empty table: clustered index")]
        public void NavigationEmptyTableClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            this.MoveCursor(tc.tableid, JET_Move.First, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Last, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Previous, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Next, typeof(EsentNoCurrentRecordException));
            this.SeekToRecordClustered(tc.tableid, 1, SeekGrbit.SeekEQ, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.SeekToRecordClustered(tc.tableid, 1, SeekGrbit.SeekGE, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.SeekToRecordClustered(tc.tableid, 1, SeekGrbit.SeekGT, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.SeekToRecordClustered(tc.tableid, 1, SeekGrbit.SeekLT, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.SeekToRecordClustered(tc.tableid, 1, SeekGrbit.SeekLE, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.SeekToRecordClustered(tc.tableid, 1, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.MakeKeyAndSetIndexRangeClustered(tc.tableid, 1, SetIndexRangeGrbit.None, typeof(EsentNoCurrentRecordException));
        }

        /// <summary>
        /// Navigation on empty table: secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Navigation on empty table: secondary index")]
        public void NavigationEmptyTableSecondary()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithoutPrimaryName);

            this.MoveCursor(tc.tableid, JET_Move.First, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Last, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Previous, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Next, typeof(EsentNoCurrentRecordException));
            this.SeekToRecordSecondary(tc.tableid, 10, 100, SeekGrbit.SeekEQ, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.SeekToRecordSecondary(tc.tableid, 10, 100, SeekGrbit.SeekGE, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.SeekToRecordSecondary(tc.tableid, 10, 100, SeekGrbit.SeekGT, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.SeekToRecordSecondary(tc.tableid, 10, 100, SeekGrbit.SeekLT, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.SeekToRecordSecondary(tc.tableid, 10, 100, SeekGrbit.SeekLE, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.SeekToRecordSecondary(tc.tableid, 10, 100, SeekGrbit.SeekEQ | SeekGrbit.SetIndexRange, JET_wrn.Success, typeof(EsentRecordNotFoundException));
            this.MakeKeyAndSetIndexRangeSecondary(tc.tableid, 10, 100, SetIndexRangeGrbit.None, typeof(EsentNoCurrentRecordException));
        }

        /// <summary>
        /// Bookmarks: clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Bookmarks: clustered index")]
        public void BookmarksClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            this.InsertRecord(tc.tableid, 1);
            byte[] bookmark = this.InsertRecord(tc.tableid, 2);
            this.InsertRecord(tc.tableid, 3);

            Api.JetGotoBookmark(this.sesId, tc.tableid, bookmark, bookmark.Length);
            this.VerifyCurrentRecord(tc.tableid, 2);

            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 1);

            int actualBookmarkSize;
            Api.JetGetBookmark(this.sesId, tc.tableid, bookmark, bookmark.Length, out actualBookmarkSize);
            Assert.AreEqual<int>(bookmark.Length, actualBookmarkSize);

            this.MoveCursor(tc.tableid, JET_Move.Last);
            this.VerifyCurrentRecord(tc.tableid, 3);

            Api.JetGotoBookmark(this.sesId, tc.tableid, bookmark, bookmark.Length);
            this.VerifyCurrentRecord(tc.tableid, 1);
        }

        /// <summary>
        /// Bookmarks: secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Bookmarks: secondary index")]
        public void BookmarksSecondary()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            this.InsertRecord(tc.tableid, 2, 10);
            byte[] bookmark = this.InsertRecord(tc.tableid, 1, 10);
            this.InsertRecord(tc.tableid, -3, 30);
            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithPrimaryName);

            Api.JetGotoBookmark(this.sesId, tc.tableid, bookmark, bookmark.Length);
            this.VerifyCurrentRecord(tc.tableid, 1, 10);

            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 2, 10);

            int actualBookmarkSize;
            Api.JetGetBookmark(this.sesId, tc.tableid, bookmark, bookmark.Length, out actualBookmarkSize);
            Assert.AreEqual<int>(bookmark.Length, actualBookmarkSize);

            this.MoveCursor(tc.tableid, JET_Move.Last);
            this.VerifyCurrentRecord(tc.tableid, -3, 30);

            Api.JetGotoBookmark(this.sesId, tc.tableid, bookmark, bookmark.Length);
            this.VerifyCurrentRecord(tc.tableid, 2, 10);
        }

        /// <summary>
        /// JetMove with MoveKeyNE: clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("JetMove with MoveKeyNE: clustered index")]
        public void JetMoveWithMoveKeyNeClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            this.InsertRecord(tc.tableid, 1);
            this.InsertRecord(tc.tableid, 2);
            this.InsertRecord(tc.tableid, 3);

            this.VerifyCurrentRecord(tc.tableid, 1);

            Api.JetMove(this.sesId, tc.tableid, JET_Move.Next, MoveGrbit.MoveKeyNE);
            this.VerifyCurrentRecord(tc.tableid, 2);

            Api.JetMove(this.sesId, tc.tableid, JET_Move.Next, MoveGrbit.MoveKeyNE);
            this.VerifyCurrentRecord(tc.tableid, 3);

            for (int i = 0; i < 2; i++)
            {
                try
                {
                    Api.JetMove(this.sesId, tc.tableid, JET_Move.Next, MoveGrbit.MoveKeyNE);
                    Assert.Fail("Should have thrown EsentNoCurrentRecordException");
                }
                catch (EsentNoCurrentRecordException)
                {
                }

                this.VerifyCurrentRecord(tc.tableid, 3, typeof(EsentNoCurrentRecordException));
            }

            Api.JetMove(this.sesId, tc.tableid, JET_Move.Previous, MoveGrbit.MoveKeyNE);
            this.VerifyCurrentRecord(tc.tableid, 3);

            Api.JetMove(this.sesId, tc.tableid, JET_Move.Previous, MoveGrbit.MoveKeyNE);
            this.VerifyCurrentRecord(tc.tableid, 2);

            Api.JetMove(this.sesId, tc.tableid, JET_Move.Previous, MoveGrbit.MoveKeyNE);
            this.VerifyCurrentRecord(tc.tableid, 1);

            for (int i = 0; i < 2; i++)
            {
                try
                {
                    Api.JetMove(this.sesId, tc.tableid, JET_Move.Previous, MoveGrbit.MoveKeyNE);
                    Assert.Fail("Should have thrown EsentNoCurrentRecordException");
                }
                catch (EsentNoCurrentRecordException)
                {
                }

                this.VerifyCurrentRecord(tc.tableid, 1, typeof(EsentNoCurrentRecordException));
            }

            Api.JetMove(this.sesId, tc.tableid, JET_Move.Next, MoveGrbit.MoveKeyNE);
            this.VerifyCurrentRecord(tc.tableid, 1);
        }

        /// <summary>
        /// JetMove with MoveKeyNE: secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("JetMove with MoveKeyNE: secondary index")]
        public void JetMoveWithMoveKeyNeSecondary()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            this.InsertRecord(tc.tableid, 3, 30, 300);
            this.InsertRecord(tc.tableid, 6, 20, 200);
            this.InsertRecord(tc.tableid, 2, 10, 100);
            this.InsertRecord(tc.tableid, 4, 20, 200);
            this.InsertRecord(tc.tableid, 1, 20, 200);
            this.InsertRecord(tc.tableid, 5, 30, 300);

            this.VerifyCurrentRecord(tc.tableid, 1, 20, 200);

            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithoutPrimaryName);
            this.VerifyCurrentRecord(tc.tableid, 2, 10, 100);

            Api.JetMove(this.sesId, tc.tableid, JET_Move.Next, MoveGrbit.MoveKeyNE);
            this.VerifyCurrentRecord(tc.tableid, 1, 20, 200);

            Api.JetMove(this.sesId, tc.tableid, JET_Move.Next, MoveGrbit.None);
            this.VerifyCurrentRecord(tc.tableid, 4, 20, 200);

            Api.JetMove(this.sesId, tc.tableid, JET_Move.Next, MoveGrbit.MoveKeyNE);
            this.VerifyCurrentRecord(tc.tableid, 3, 30, 300);

            for (int i = 0; i < 2; i++)
            {
                try
                {
                    Api.JetMove(this.sesId, tc.tableid, JET_Move.Next, MoveGrbit.MoveKeyNE);
                    Assert.Fail("Should have thrown EsentNoCurrentRecordException");
                }
                catch (EsentNoCurrentRecordException)
                {
                }

                this.VerifyCurrentRecord(tc.tableid, 3, 30, 300, typeof(EsentNoCurrentRecordException));
            }

            Api.JetMove(this.sesId, tc.tableid, JET_Move.Previous, MoveGrbit.MoveKeyNE);
            this.VerifyCurrentRecord(tc.tableid, 5, 30, 300);

            Api.JetMove(this.sesId, tc.tableid, JET_Move.Previous, MoveGrbit.None);
            this.VerifyCurrentRecord(tc.tableid, 3, 30, 300);

            Api.JetMove(this.sesId, tc.tableid, JET_Move.Previous, MoveGrbit.MoveKeyNE);
            this.VerifyCurrentRecord(tc.tableid, 6, 20, 200);

            Api.JetMove(this.sesId, tc.tableid, JET_Move.Previous, MoveGrbit.MoveKeyNE);
            this.VerifyCurrentRecord(tc.tableid, 2, 10, 100);

            for (int i = 0; i < 2; i++)
            {
                try
                {
                    Api.JetMove(this.sesId, tc.tableid, JET_Move.Previous, MoveGrbit.MoveKeyNE);
                    Assert.Fail("Should have thrown EsentNoCurrentRecordException");
                }
                catch (EsentNoCurrentRecordException)
                {
                }

                this.VerifyCurrentRecord(tc.tableid, 2, 10, 100, typeof(EsentNoCurrentRecordException));
            }

            Api.JetMove(this.sesId, tc.tableid, JET_Move.Next, MoveGrbit.MoveKeyNE);
            this.VerifyCurrentRecord(tc.tableid, 2, 10, 100);
        }

        /// <summary>
        /// JetSetColumn without JetPrepareUpdate should throw EsentUpdateNotPreparedException. 
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(EsentUpdateNotPreparedException))]
        [Description("JetSetColumn without JetPrepareUpdate should throw EsentUpdateNotPreparedException")]
        public void JetSetColumnWithoutJetPrepareUpdateExpectEsentUpdateNotPreparedException()
        {
            int data1 = 10;
            byte[] data1Array = BitConverter.GetBytes(data1);

            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            Api.JetSetColumn(this.sesId, tc.tableid, this.columnIdData1, data1Array, data1Array.Length, SetColumnGrbit.None, null);
        }

        /// <summary>
        /// JetUpdate without JetPrepareUpdate should throw EsentUpdateNotPreparedException. 
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(EsentUpdateNotPreparedException))]
        [Description("JetUpdate without JetPrepareUpdate should throw EsentUpdateNotPreparedException")]
        public void JetUpdateWithoutJetPrepareUpdateExpectEsentUpdateNotPreparedException()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            Api.JetUpdate(this.sesId, tc.tableid);
        }

        /// <summary>
        /// JetPrepareUpdate flags: Insert.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("JetPrepareUpdate flags: Insert")]
        public void JetPrepareUpdateFlagsInsert()
        {
            int key = 10;
            int data1 = 100;
            int data2 = 1000;
            byte[] keyArray = BitConverter.GetBytes(key);
            byte[] data1Array = BitConverter.GetBytes(data1);
            byte[] data2Array = BitConverter.GetBytes(data2);
            byte[] bookmark = new byte[sizeof(int) + 1]; // +1 for ascending/descending info.

            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            Api.JetPrepareUpdate(this.sesId, tc.tableid, JET_prep.Insert);
            Api.JetSetColumn(this.sesId, tc.tableid, this.columnIdKey, keyArray, keyArray.Length, SetColumnGrbit.None, null);
            Api.JetSetColumn(this.sesId, tc.tableid, this.columnIdData1, data1Array, data1Array.Length, SetColumnGrbit.None, null);
            Api.JetSetColumn(this.sesId, tc.tableid, this.columnIdData2, data2Array, data2Array.Length, SetColumnGrbit.None, null);

            int actualBookmarkSize;
            Api.JetUpdate(this.sesId, tc.tableid, bookmark, bookmark.Length, out actualBookmarkSize);
            Api.JetGotoBookmark(this.sesId, tc.tableid, bookmark, bookmark.Length);

            this.VerifyCurrentRecord(tc.tableid, key, data1, data2);
        }

        /// <summary>
        /// JetPrepareUpdate flags: Replace.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("JetPrepareUpdate flags: Replace")]
        public void JetPrepareUpdateFlagsReplace()
        {
            int keyOld = 10;
            int data1 = 200;
            int data2 = 2000;
            byte[] data1Array = BitConverter.GetBytes(data1);
            byte[] data2Array = BitConverter.GetBytes(data2);
            JET_TABLEID tableId1;

            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            byte[] bookmark = this.InsertRecord(tc.tableid, keyOld, 100, 1000);
            Api.JetCloseTable(this.sesId, tc.tableid);
            Api.JetCommitTransaction(this.sesId, CommitTransactionGrbit.None);
            Api.JetBeginTransaction(this.sesId);
            Api.JetOpenTable(this.sesId, this.dbId, this.tableName, null, 0, OpenTableGrbit.None, out tableId1);

            JET_SESID sesId2;
            JET_DBID dbId2;
            JET_TABLEID tableId2;
            Api.JetBeginSession(this.instance, out sesId2, string.Empty, string.Empty);
            Api.JetOpenDatabase(sesId2, this.database, string.Empty, out dbId2, OpenDatabaseGrbit.None);
            Api.JetBeginTransaction(sesId2);
            Api.JetOpenTable(sesId2, dbId2, this.tableName, null, 0, OpenTableGrbit.None, out tableId2);
            Api.JetGotoBookmark(sesId2, tableId2, bookmark, bookmark.Length);

            Api.JetGotoBookmark(this.sesId, tableId1, bookmark, bookmark.Length);
            Api.JetPrepareUpdate(this.sesId, tableId1, JET_prep.Replace);
            try
            {
                Api.JetPrepareUpdate(sesId2, tableId2, JET_prep.Replace);
                Assert.Fail("Should have thrown EsentWriteConflictException");
            }
            catch (EsentWriteConflictException)
            {
            }

            Api.JetSetColumn(this.sesId, tableId1, this.columnIdData1, data1Array, data1Array.Length, SetColumnGrbit.None, null);
            Api.JetSetColumn(this.sesId, tableId1, this.columnIdData2, data2Array, data2Array.Length, SetColumnGrbit.None, null);
            Api.JetUpdate(this.sesId, tableId1);

            this.VerifyCurrentRecord(tableId1, keyOld, data1, data2);

            Api.JetCloseDatabase(sesId2, dbId2, CloseDatabaseGrbit.None);
            Api.JetEndSession(sesId2, EndSessionGrbit.None);
        }

        /// <summary>
        /// JetPrepareUpdate flags: ReplaceNoLock.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("JetPrepareUpdate flags: ReplaceNoLock")]
        public void JetPrepareUpdateFlagsReplaceNoLock()
        {
            int keyOld = 10;
            int data1Ses1 = 200;
            int data2Ses1 = 2000;
            int data1Ses2 = 300;
            int data2Ses2 = 3000;
            byte[] data1Ses1Array = BitConverter.GetBytes(data1Ses1);
            byte[] data2Ses1Array = BitConverter.GetBytes(data2Ses1);
            byte[] data1Ses2Array = BitConverter.GetBytes(data1Ses2);
            byte[] data2Ses2Array = BitConverter.GetBytes(data2Ses2);
            JET_TABLEID tableId1;

            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            byte[] bookmark = this.InsertRecord(tc.tableid, keyOld, 100, 1000);
            Api.JetCloseTable(this.sesId, tc.tableid);
            Api.JetCommitTransaction(this.sesId, CommitTransactionGrbit.None);
            Api.JetBeginTransaction(this.sesId);
            Api.JetOpenTable(this.sesId, this.dbId, this.tableName, null, 0, OpenTableGrbit.None, out tableId1);

            JET_SESID sesId2;
            JET_DBID dbId2;
            JET_TABLEID tableId2;
            Api.JetBeginSession(this.instance, out sesId2, string.Empty, string.Empty);
            Api.JetOpenDatabase(sesId2, this.database, string.Empty, out dbId2, OpenDatabaseGrbit.None);
            Api.JetBeginTransaction(sesId2);
            Api.JetOpenTable(sesId2, dbId2, this.tableName, null, 0, OpenTableGrbit.None, out tableId2);
            Api.JetGotoBookmark(sesId2, tableId2, bookmark, bookmark.Length);

            Api.JetGotoBookmark(this.sesId, tableId1, bookmark, bookmark.Length);
            Api.JetPrepareUpdate(this.sesId, tableId1, JET_prep.ReplaceNoLock);
            Api.JetPrepareUpdate(sesId2, tableId2, JET_prep.ReplaceNoLock);
            Api.JetSetColumn(sesId2, tableId2, this.columnIdData1, data1Ses2Array, data1Ses2Array.Length, SetColumnGrbit.None, null);
            Api.JetSetColumn(sesId2, tableId2, this.columnIdData2, data2Ses2Array, data2Ses2Array.Length, SetColumnGrbit.None, null);
            Api.JetSetColumn(this.sesId, tableId1, this.columnIdData1, data1Ses1Array, data1Ses1Array.Length, SetColumnGrbit.None, null);
            Api.JetSetColumn(this.sesId, tableId1, this.columnIdData2, data2Ses1Array, data2Ses1Array.Length, SetColumnGrbit.None, null);
            Api.JetUpdate(sesId2, tableId2);

            try
            {
                Api.JetUpdate(this.sesId, tableId1);
                Assert.Fail("Should have thrown EsentWriteConflictException");
            }
            catch (EsentWriteConflictException)
            {
            }

            Api.JetCommitTransaction(this.sesId, CommitTransactionGrbit.None);
            Api.JetCommitTransaction(sesId2, CommitTransactionGrbit.None);
            Api.JetBeginTransaction(this.sesId);

            this.VerifyCurrentRecord(tableId1, keyOld, data1Ses2, data2Ses2);

            Api.JetCloseDatabase(sesId2, dbId2, CloseDatabaseGrbit.None);
            Api.JetEndSession(sesId2, EndSessionGrbit.None);
        }

        /// <summary>
        /// JetPrepareUpdate flags: InsertCopy.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("JetPrepareUpdate flags: InsertCopy")]
        public void JetPrepareUpdateFlagsInsertCopy()
        {
            int keyOld = 10, data1Old = 100, data2Old = 1000;
            int keyNew = 20, data1New = 200, data2New = 2000;
            byte[] keyNewArray = BitConverter.GetBytes(keyNew);
            byte[] data1NewArray = BitConverter.GetBytes(data1New);
            byte[] data2NewArray = BitConverter.GetBytes(data2New);

            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            byte[] bookmark = this.InsertRecord(tc.tableid, keyOld, data1Old, data2Old);
            Api.JetGotoBookmark(this.sesId, tc.tableid, bookmark, bookmark.Length);

            Api.JetPrepareUpdate(this.sesId, tc.tableid, JET_prep.InsertCopy);
            Api.JetSetColumn(this.sesId, tc.tableid, this.columnIdKey, keyNewArray, keyNewArray.Length, SetColumnGrbit.None, null);
            Api.JetSetColumn(this.sesId, tc.tableid, this.columnIdData1, data1NewArray, data1NewArray.Length, SetColumnGrbit.None, null);
            Api.JetSetColumn(this.sesId, tc.tableid, this.columnIdData2, data2NewArray, data2NewArray.Length, SetColumnGrbit.None, null);
            Api.JetUpdate(this.sesId, tc.tableid);

            this.MoveCursor(tc.tableid, JET_Move.First);
            this.VerifyCurrentRecord(tc.tableid, keyOld, data1Old, data2Old);

            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, keyNew, data1New, data2New);
        }

        /// <summary>
        /// JetPrepareUpdate flags: InsertCopyDeleteOriginal.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("JetPrepareUpdate flags: InsertCopyDeleteOriginal ")]
        public void JetPrepareUpdateFlagsInsertCopyDeleteOriginal()
        {
            int keyOld = 10, data1Old = 100, data2Old = 1000;
            int keyNew = 20, data1New = 200, data2New = 2000;
            byte[] keyNewArray = BitConverter.GetBytes(keyNew);
            byte[] data1NewArray = BitConverter.GetBytes(data1New);
            byte[] data2NewArray = BitConverter.GetBytes(data2New);

            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            byte[] bookmark = this.InsertRecord(tc.tableid, keyOld, data1Old, data2Old);
            Api.JetGotoBookmark(this.sesId, tc.tableid, bookmark, bookmark.Length);

            Api.JetPrepareUpdate(this.sesId, tc.tableid, JET_prep.InsertCopyDeleteOriginal);
            Api.JetSetColumn(this.sesId, tc.tableid, this.columnIdKey, keyNewArray, keyNewArray.Length, SetColumnGrbit.None, null);
            Api.JetSetColumn(this.sesId, tc.tableid, this.columnIdData1, data1NewArray, data1NewArray.Length, SetColumnGrbit.None, null);
            Api.JetSetColumn(this.sesId, tc.tableid, this.columnIdData2, data2NewArray, data2NewArray.Length, SetColumnGrbit.None, null);
            Api.JetUpdate(this.sesId, tc.tableid);

            this.MoveCursor(tc.tableid, JET_Move.First);
            this.VerifyCurrentRecord(tc.tableid, keyNew, data1New, data2New);

            this.MoveCursor(tc.tableid, JET_Move.Next, typeof(EsentNoCurrentRecordException));
        }

        /// <summary>
        /// JetPrepareUpdate flags: Cancel.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("JetPrepareUpdate flags: Cancel")]
        public void JetPrepareUpdateFlagsCancel()
        {
            int keyOld = 10, data1Old = 100, data2Old = 1000;
            int data1 = 200, data2 = 2000;
            byte[] data1Array = BitConverter.GetBytes(data1);
            byte[] data2Array = BitConverter.GetBytes(data2);

            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            byte[] bookmark = this.InsertRecord(tc.tableid, keyOld, data1Old, data2Old);
            Api.JetGotoBookmark(this.sesId, tc.tableid, bookmark, bookmark.Length);

            Api.JetPrepareUpdate(this.sesId, tc.tableid, JET_prep.Replace);
            Api.JetSetColumn(this.sesId, tc.tableid, this.columnIdData1, data1Array, data1Array.Length, SetColumnGrbit.None, null);
            Api.JetSetColumn(this.sesId, tc.tableid, this.columnIdData2, data2Array, data2Array.Length, SetColumnGrbit.None, null);
            Api.JetPrepareUpdate(this.sesId, tc.tableid, JET_prep.Cancel);

            try
            {
                Api.JetSetColumn(this.sesId, tc.tableid, this.columnIdData2, data2Array, data2Array.Length, SetColumnGrbit.None, null);
                Assert.Fail("Should have thron EsentUpdateNotPreparedException");
            }
            catch (EsentUpdateNotPreparedException)
            {
            }

            this.VerifyCurrentRecord(tc.tableid, keyOld, data1Old, data2Old);
        }

        /// <summary>
        /// JetSetIndexRange without making a key, expect EsentKeyNotMadeException.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(EsentKeyNotMadeException))]
        [Description("JetSetIndexRange without making a key, expect EsentKeyNotMadeException")]
        public void JetSetIndexRangeWithoutKeyExpectEsentKeyNotMadeException()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            Api.JetSetIndexRange(this.sesId, tc.tableid, SetIndexRangeGrbit.None);
        }

        /// <summary>
        /// Removing an index range that has not been established, expect EsentInvalidOperationException.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(EsentInvalidOperationException))]
        [Description("Removing an index range that has not been established, expect EsentInvalidOperationException")]
        public void RemovesNonEstablishedIndexRangeExpectEsentInvalidOperationException()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            Api.JetSetIndexRange(this.sesId, tc.tableid, SetIndexRangeGrbit.RangeRemove);
        }

        /// <summary>
        /// JetSetIndexRange: exclusive, lower-limit, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("JetSetIndexRange: exclusive, lower-limit, clustered index")]
        public void JetSetIndexRangeExclusiveLowerLimitClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            this.MakeKeyAndSetIndexRangeClustered(tc.tableid, 1, SetIndexRangeGrbit.None, typeof(EsentNoCurrentRecordException));

            this.InsertRecord(tc.tableid, 1);
            this.InsertRecord(tc.tableid, 2);
            this.InsertRecord(tc.tableid, 3);
            this.InsertRecord(tc.tableid, 4);

            this.SeekToRecordClustered(tc.tableid, 3);
            this.MakeKeyAndSetIndexRangeClustered(tc.tableid, 1, SetIndexRangeGrbit.None, null);

            this.VerifyCurrentRecord(tc.tableid, 3);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 2);
            this.MoveCursor(tc.tableid, JET_Move.Previous, typeof(EsentNoCurrentRecordException));
            this.VerifyCurrentRecord(tc.tableid, 2, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 1);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 2);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 1);
        }

        /// <summary>
        /// JetSetIndexRange: exclusive, upper-limit, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("JetSetIndexRange: exclusive, upper-limit, clustered index")]
        public void JetSetIndexRangeExclusiveUpperLimitClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            this.MakeKeyAndSetIndexRangeClustered(tc.tableid, 4, SetIndexRangeGrbit.RangeUpperLimit, typeof(EsentNoCurrentRecordException));

            this.InsertRecord(tc.tableid, 1);
            this.InsertRecord(tc.tableid, 2);
            this.InsertRecord(tc.tableid, 3);
            this.InsertRecord(tc.tableid, 4);

            this.SeekToRecordClustered(tc.tableid, 2);
            this.MakeKeyAndSetIndexRangeClustered(tc.tableid, 4, SetIndexRangeGrbit.RangeUpperLimit, null);

            this.VerifyCurrentRecord(tc.tableid, 2);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 3);
            this.MoveCursor(tc.tableid, JET_Move.Next, typeof(EsentNoCurrentRecordException));
            this.VerifyCurrentRecord(tc.tableid, 4, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 4);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 3);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 4);
        }

        /// <summary>
        /// JetSetIndexRange: inclusive, lower-limit, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("JetSetIndexRange: inclusive, lower-limit, clustered index")]
        public void JetSetIndexRangeInclusiveLowerLimitClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            this.MakeKeyAndSetIndexRangeClustered(tc.tableid, 2, SetIndexRangeGrbit.RangeInclusive, typeof(EsentNoCurrentRecordException));

            this.InsertRecord(tc.tableid, 1);
            this.InsertRecord(tc.tableid, 2);
            this.InsertRecord(tc.tableid, 3);
            this.InsertRecord(tc.tableid, 4);

            this.SeekToRecordClustered(tc.tableid, 3);
            this.MakeKeyAndSetIndexRangeClustered(tc.tableid, 2, SetIndexRangeGrbit.RangeInclusive, null);

            this.VerifyCurrentRecord(tc.tableid, 3);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 2);
            this.MoveCursor(tc.tableid, JET_Move.Previous, typeof(EsentNoCurrentRecordException));
            this.VerifyCurrentRecord(tc.tableid, 2, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 1);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 2);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 1);
        }

        /// <summary>
        /// JetSetIndexRange: inclusive, upper-limit, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("JetSetIndexRange: inclusive, upper-limit, clustered index")]
        public void JetSetIndexRangeInclusiveUpperLimitClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            this.MakeKeyAndSetIndexRangeClustered(tc.tableid, 3, SetIndexRangeGrbit.RangeInclusive | SetIndexRangeGrbit.RangeUpperLimit, typeof(EsentNoCurrentRecordException));

            this.InsertRecord(tc.tableid, 1);
            this.InsertRecord(tc.tableid, 2);
            this.InsertRecord(tc.tableid, 3);
            this.InsertRecord(tc.tableid, 4);

            this.SeekToRecordClustered(tc.tableid, 2);
            this.MakeKeyAndSetIndexRangeClustered(tc.tableid, 3, SetIndexRangeGrbit.RangeInclusive | SetIndexRangeGrbit.RangeUpperLimit, null);

            this.VerifyCurrentRecord(tc.tableid, 2);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 3);
            this.MoveCursor(tc.tableid, JET_Move.Next, typeof(EsentNoCurrentRecordException));
            this.VerifyCurrentRecord(tc.tableid, 4, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 4);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 3);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 4);
        }

        /// <summary>
        /// JetSetIndexRange: removing index range, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("JetSetIndexRange: removing index range, clustered index")]
        public void JetSetIndexRangeRemoveIndexRangeClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            this.InsertRecord(tc.tableid, 1);
            this.InsertRecord(tc.tableid, 2);
            this.InsertRecord(tc.tableid, 3);
            this.InsertRecord(tc.tableid, 4);

            this.SeekToRecordClustered(tc.tableid, 3);
            this.MakeKeyAndSetIndexRangeClustered(tc.tableid, 1, SetIndexRangeGrbit.None, null);

            this.VerifyCurrentRecord(tc.tableid, 3);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 2);
            Api.JetSetIndexRange(this.sesId, tc.tableid, SetIndexRangeGrbit.RangeRemove);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 1);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 2);
        }

        /// <summary>
        /// JetSetIndexRange: removing index range instantly, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("JetSetIndexRange: removing index range instantly, clustered index")]
        public void JetSetIndexRangeRemoveIndexRangeInstantlyClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            this.InsertRecord(tc.tableid, 1);
            this.InsertRecord(tc.tableid, 2);
            this.InsertRecord(tc.tableid, 3);
            this.InsertRecord(tc.tableid, 4);

            this.SeekToRecordClustered(tc.tableid, 3);
            this.MakeKeyAndSetIndexRangeClustered(tc.tableid, 1, SetIndexRangeGrbit.RangeInstantDuration, null);

            this.VerifyCurrentRecord(tc.tableid, 3);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 2);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 1);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 2);
        }

        /// <summary>
        /// JetSetIndexRange: exclusive, lower-limit, secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("JetSetIndexRange: exclusive, lower-limit, secondary index")]
        public void JetSetIndexRangeExclusiveLowerLimitSecondary()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithoutPrimaryName);

            this.MakeKeyAndSetIndexRangeSecondary(tc.tableid, 10, 100, SetIndexRangeGrbit.None, typeof(EsentNoCurrentRecordException));

            this.InsertRecord(tc.tableid, 3, 10, 100);
            this.InsertRecord(tc.tableid, 4, 20, 200);
            this.InsertRecord(tc.tableid, 1, 20, 200);
            this.InsertRecord(tc.tableid, 2, 30, 300);
            this.InsertRecord(tc.tableid, 5, 40, 400);

            this.SeekToRecordSecondary(tc.tableid, 30, 300);
            this.MakeKeyAndSetIndexRangeSecondary(tc.tableid, 10, 100, SetIndexRangeGrbit.None, null);

            this.VerifyCurrentRecord(tc.tableid, 2, 30, 300);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 4, 20, 200);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 1, 20, 200);
            this.MoveCursor(tc.tableid, JET_Move.Previous, typeof(EsentNoCurrentRecordException));
            this.VerifyCurrentRecord(tc.tableid, 3, 10, 100, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 3, 10, 100);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 1, 20, 200);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 3, 10, 100);
        }

        /// <summary>
        /// JetSetIndexRange: exclusive, upper-limit, secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("JetSetIndexRange: exclusive, upper-limit, secondary index")]
        public void JetSetIndexRangeExclusiveUpperLimitSecondary()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithoutPrimaryName);

            this.MakeKeyAndSetIndexRangeSecondary(tc.tableid, 40, 400, SetIndexRangeGrbit.RangeUpperLimit, typeof(EsentNoCurrentRecordException));

            this.InsertRecord(tc.tableid, 3, 10, 100);
            this.InsertRecord(tc.tableid, 4, 20, 200);
            this.InsertRecord(tc.tableid, 1, 20, 200);
            this.InsertRecord(tc.tableid, 2, 30, 300);
            this.InsertRecord(tc.tableid, 5, 40, 400);

            this.SeekToRecordSecondary(tc.tableid, 20, 200);
            this.MakeKeyAndSetIndexRangeSecondary(tc.tableid, 40, 400, SetIndexRangeGrbit.RangeUpperLimit, null);

            this.VerifyCurrentRecord(tc.tableid, 1, 20, 200);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 4, 20, 200);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 2, 30, 300);
            this.MoveCursor(tc.tableid, JET_Move.Next, typeof(EsentNoCurrentRecordException));
            this.VerifyCurrentRecord(tc.tableid, 5, 40, 400, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 5, 40, 400);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 2, 30, 300);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 5, 40, 400);
        }

        /// <summary>
        /// JetSetIndexRange: inclusive, lower-limit, secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("JetSetIndexRange: inclusive, lower-limit, secondary index")]
        public void JetSetIndexRangeInclusiveLowerLimitSecondary()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithoutPrimaryName);

            this.MakeKeyAndSetIndexRangeSecondary(tc.tableid, 20, 200, SetIndexRangeGrbit.RangeInclusive, typeof(EsentNoCurrentRecordException));

            this.InsertRecord(tc.tableid, 3, 10, 100);
            this.InsertRecord(tc.tableid, 4, 20, 200);
            this.InsertRecord(tc.tableid, 1, 20, 200);
            this.InsertRecord(tc.tableid, 2, 30, 300);
            this.InsertRecord(tc.tableid, 5, 40, 400);

            this.SeekToRecordSecondary(tc.tableid, 30, 300);
            this.MakeKeyAndSetIndexRangeSecondary(tc.tableid, 20, 200, SetIndexRangeGrbit.RangeInclusive, null);

            this.VerifyCurrentRecord(tc.tableid, 2, 30, 300);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 4, 20, 200);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 1, 20, 200);
            this.MoveCursor(tc.tableid, JET_Move.Previous, typeof(EsentNoCurrentRecordException));
            this.VerifyCurrentRecord(tc.tableid, 3, 10, 100, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 3, 10, 100);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 1, 20, 200);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 3, 10, 100);
        }

        /// <summary>
        /// JetSetIndexRange: inclusive, upper-limit, secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("JetSetIndexRange: inclusive, upper-limit, secondary index")]
        public void JetSetIndexRangeInclusiveUpperLimitSecondary()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithoutPrimaryName);

            this.MakeKeyAndSetIndexRangeSecondary(tc.tableid, 30, 300, SetIndexRangeGrbit.RangeInclusive | SetIndexRangeGrbit.RangeUpperLimit, typeof(EsentNoCurrentRecordException));

            this.InsertRecord(tc.tableid, 3, 10, 100);
            this.InsertRecord(tc.tableid, 4, 20, 200);
            this.InsertRecord(tc.tableid, 1, 20, 200);
            this.InsertRecord(tc.tableid, 2, 30, 300);
            this.InsertRecord(tc.tableid, 5, 40, 400);

            this.SeekToRecordSecondary(tc.tableid, 20, 200);
            this.MakeKeyAndSetIndexRangeSecondary(tc.tableid, 30, 300, SetIndexRangeGrbit.RangeInclusive | SetIndexRangeGrbit.RangeUpperLimit, null);

            this.VerifyCurrentRecord(tc.tableid, 1, 20, 200);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 4, 20, 200);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 2, 30, 300);
            this.MoveCursor(tc.tableid, JET_Move.Next, typeof(EsentNoCurrentRecordException));
            this.VerifyCurrentRecord(tc.tableid, 5, 40, 400, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 5, 40, 400);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 2, 30, 300);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 5, 40, 400);
        }

        /// <summary>
        /// JetSetIndexRange: removing index range, secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("JetSetIndexRange: removing index range, secondary index")]
        public void JetSetIndexRangeRemoveIndexRangeSecondary()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithoutPrimaryName);

            this.MakeKeyAndSetIndexRangeSecondary(tc.tableid, 10, 100, SetIndexRangeGrbit.None, typeof(EsentNoCurrentRecordException));

            this.InsertRecord(tc.tableid, 3, 10, 100);
            this.InsertRecord(tc.tableid, 4, 20, 200);
            this.InsertRecord(tc.tableid, 1, 20, 200);
            this.InsertRecord(tc.tableid, 2, 30, 300);
            this.InsertRecord(tc.tableid, 5, 40, 400);

            this.SeekToRecordSecondary(tc.tableid, 30, 300);
            this.MakeKeyAndSetIndexRangeSecondary(tc.tableid, 10, 100, SetIndexRangeGrbit.None, null);

            this.VerifyCurrentRecord(tc.tableid, 2, 30, 300);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 4, 20, 200);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 1, 20, 200);
            Api.JetSetIndexRange(this.sesId, tc.tableid, SetIndexRangeGrbit.RangeRemove);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 3, 10, 100);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 1, 20, 200);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 3, 10, 100);
        }

        /// <summary>
        /// JetSetIndexRange: removing index range instantly, secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("JetSetIndexRange: removing index range instantly, secondary index")]
        public void JetSetIndexRangeRemoveIndexRangeInstantlySecondary()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithoutPrimaryName);

            this.MakeKeyAndSetIndexRangeSecondary(tc.tableid, 10, 100, SetIndexRangeGrbit.None, typeof(EsentNoCurrentRecordException));

            this.InsertRecord(tc.tableid, 3, 10, 100);
            this.InsertRecord(tc.tableid, 4, 20, 200);
            this.InsertRecord(tc.tableid, 1, 20, 200);
            this.InsertRecord(tc.tableid, 2, 30, 300);
            this.InsertRecord(tc.tableid, 5, 40, 400);

            this.SeekToRecordSecondary(tc.tableid, 30, 300);
            this.MakeKeyAndSetIndexRangeSecondary(tc.tableid, 10, 100, SetIndexRangeGrbit.RangeInstantDuration, null);

            this.VerifyCurrentRecord(tc.tableid, 2, 30, 300);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 4, 20, 200);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 1, 20, 200);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 3, 10, 100);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 1, 20, 200);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 3, 10, 100);
        }

        /// <summary>
        /// Cursor position after insert, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Cursor position after insert, clustered index")]
        public void CursorPositionAfterInsertClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            this.InsertRecord(tc.tableid, 3);
            this.InsertRecord(tc.tableid, 1);
            this.InsertRecord(tc.tableid, 2);
            this.VerifyCurrentRecord(tc.tableid, 1);

            this.InsertRecord(tc.tableid, 0);
            this.VerifyCurrentRecord(tc.tableid, 1);
        }

        /// <summary>
        /// Cursor position after delete, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Cursor position after delete, clustered index")]
        public void CursorPositionAfterDeleteClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            this.InsertRecord(tc.tableid, 3);
            this.InsertRecord(tc.tableid, 1);
            this.InsertRecord(tc.tableid, 2);

            this.SeekToRecordClustered(tc.tableid, 1);
            this.DeleteCurrentRecord(tc.tableid);
            this.VerifyCurrentRecord(tc.tableid, 1, typeof(EsentRecordDeletedException));
            this.InsertRecord(tc.tableid, 1);
            this.VerifyCurrentRecord(tc.tableid, 1);
            this.DeleteCurrentRecord(tc.tableid);
            this.MoveCursor(tc.tableid, JET_Move.Previous, typeof(EsentNoCurrentRecordException));
            this.VerifyCurrentRecord(tc.tableid, 1, typeof(EsentNoCurrentRecordException));
            this.InsertRecord(tc.tableid, 1);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 1);
            this.DeleteCurrentRecord(tc.tableid);
            this.MoveCursor(tc.tableid, JET_Move.Previous, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 2);
            this.InsertRecord(tc.tableid, 1);

            this.SeekToRecordClustered(tc.tableid, 2);
            this.DeleteCurrentRecord(tc.tableid);
            this.VerifyCurrentRecord(tc.tableid, 2, typeof(EsentRecordDeletedException));
            this.InsertRecord(tc.tableid, 2);
            this.VerifyCurrentRecord(tc.tableid, 2);
            this.DeleteCurrentRecord(tc.tableid);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 1);
            this.InsertRecord(tc.tableid, 2);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 2);
            this.DeleteCurrentRecord(tc.tableid);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 3);
            this.InsertRecord(tc.tableid, 2);

            this.SeekToRecordClustered(tc.tableid, 3);
            this.DeleteCurrentRecord(tc.tableid);
            this.VerifyCurrentRecord(tc.tableid, 3, typeof(EsentRecordDeletedException));
            this.InsertRecord(tc.tableid, 3);
            this.VerifyCurrentRecord(tc.tableid, 3);
            this.DeleteCurrentRecord(tc.tableid);
            this.MoveCursor(tc.tableid, JET_Move.Next, typeof(EsentNoCurrentRecordException));
            this.VerifyCurrentRecord(tc.tableid, 3, typeof(EsentNoCurrentRecordException));
            this.InsertRecord(tc.tableid, 3);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 3);
            this.DeleteCurrentRecord(tc.tableid);
            this.MoveCursor(tc.tableid, JET_Move.Next, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 2);
            this.InsertRecord(tc.tableid, 3);
        }

        /// <summary>
        /// Cursor position after update, clustered index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Cursor position after update, clustered index")]
        public void CursorPositionAfterUpdateClustered()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);

            this.InsertRecord(tc.tableid, 3, 300);
            this.InsertRecord(tc.tableid, 1, 100);
            this.InsertRecord(tc.tableid, 2, 200);

            this.SeekToRecordClustered(tc.tableid, 1);
            this.VerifyCurrentRecord(tc.tableid, 1, 100);
            this.UpdateCurrentRecord(tc.tableid, 101);
            this.VerifyCurrentRecord(tc.tableid, 1, 101);

            this.SeekToRecordClustered(tc.tableid, 2);
            this.VerifyCurrentRecord(tc.tableid, 2, 200);
            this.UpdateCurrentRecord(tc.tableid, 201);
            this.VerifyCurrentRecord(tc.tableid, 2, 201);

            this.SeekToRecordClustered(tc.tableid, 3);
            this.VerifyCurrentRecord(tc.tableid, 3, 300);
            this.UpdateCurrentRecord(tc.tableid, 301);
            this.VerifyCurrentRecord(tc.tableid, 3, 301);
        }

        /// <summary>
        /// Cursor position after insert, secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Cursor position after insert, secondary index")]
        public void CursorPositionAfterInsertSecondary()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithoutPrimaryName);

            this.InsertRecord(tc.tableid, 1, 20, 200);
            this.InsertRecord(tc.tableid, 3, 10, 100);
            this.InsertRecord(tc.tableid, 2, 10, 100);
            this.InsertRecord(tc.tableid, 4, 40, 400);
            this.VerifyCurrentRecord(tc.tableid, 2, 10, 100);

            this.InsertRecord(tc.tableid, 0, 0, 0);
            this.VerifyCurrentRecord(tc.tableid, 2, 10, 100);
        }

        /// <summary>
        /// Cursor position after delete, secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Cursor position after delete, secondary index")]
        public void CursorPositionAfterDeleteSecondary()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithoutPrimaryName);

            this.InsertRecord(tc.tableid, 1, 20, 200);
            this.InsertRecord(tc.tableid, 3, 10, 100);
            this.InsertRecord(tc.tableid, 2, 10, 100);
            this.InsertRecord(tc.tableid, 4, 40, 400);

            this.SeekToRecordSecondary(tc.tableid, 10, 100);
            this.DeleteCurrentRecord(tc.tableid);
            this.VerifyCurrentRecord(tc.tableid, 2, 10, 100, typeof(EsentRecordDeletedException));
            this.InsertRecord(tc.tableid, 2, 10, 100);
            this.VerifyCurrentRecord(tc.tableid, 2, 10, 100);
            this.DeleteCurrentRecord(tc.tableid);
            this.MoveCursor(tc.tableid, JET_Move.Previous, typeof(EsentNoCurrentRecordException));
            this.VerifyCurrentRecord(tc.tableid, 2, 10, 100, typeof(EsentNoCurrentRecordException));
            this.InsertRecord(tc.tableid, 2, 10, 100);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 2, 10, 100);
            this.DeleteCurrentRecord(tc.tableid);
            this.MoveCursor(tc.tableid, JET_Move.Previous, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 3, 10, 100);
            this.InsertRecord(tc.tableid, 2, 10, 100);

            this.SeekToRecordSecondary(tc.tableid, 20, 200);
            this.DeleteCurrentRecord(tc.tableid);
            this.VerifyCurrentRecord(tc.tableid, 1, 20, 200, typeof(EsentRecordDeletedException));
            this.InsertRecord(tc.tableid, 1, 20, 200);
            this.VerifyCurrentRecord(tc.tableid, 1, 20, 200);
            this.DeleteCurrentRecord(tc.tableid);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 3, 10, 100);
            this.InsertRecord(tc.tableid, 1, 20, 200);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 1, 20, 200);
            this.DeleteCurrentRecord(tc.tableid);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 4, 40, 400);
            this.InsertRecord(tc.tableid, 1, 20, 200);

            this.SeekToRecordSecondary(tc.tableid, 40, 400);
            this.DeleteCurrentRecord(tc.tableid);
            this.VerifyCurrentRecord(tc.tableid, 4, 40, 400, typeof(EsentRecordDeletedException));
            this.InsertRecord(tc.tableid, 4, 40, 400);
            this.VerifyCurrentRecord(tc.tableid, 4, 40, 400);
            this.DeleteCurrentRecord(tc.tableid);
            this.MoveCursor(tc.tableid, JET_Move.Next, typeof(EsentNoCurrentRecordException));
            this.VerifyCurrentRecord(tc.tableid, 4, 40, 400, typeof(EsentNoCurrentRecordException));
            this.InsertRecord(tc.tableid, 4, 40, 400);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 4, 40, 400);
            this.DeleteCurrentRecord(tc.tableid);
            this.MoveCursor(tc.tableid, JET_Move.Next, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 1, 20, 200);
            this.InsertRecord(tc.tableid, 4, 40, 400);
        }

        /// <summary>
        /// Cursor position after update, secondary index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Cursor position after update, secondary index")]
        public void CursorPositionAfterUpdateSecondary()
        {
            JET_TABLECREATE tc = this.CreateTable(this.tableName);
            Api.JetSetCurrentIndex(this.sesId, tc.tableid, this.secIndexWithoutPrimaryName);

            this.InsertRecord(tc.tableid, 1, 20, 200);
            this.InsertRecord(tc.tableid, 3, 10, 100);
            this.InsertRecord(tc.tableid, 2, 10, 100);
            this.InsertRecord(tc.tableid, 4, 40, 400);

            this.SeekToRecordSecondary(tc.tableid, 10, 100);
            this.VerifyCurrentRecord(tc.tableid, 2, 10, 100);
            this.UpdateCurrentRecord(tc.tableid, -10, -100);
            this.VerifyCurrentRecord(tc.tableid, 2, -10, -100);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 2, -10, -100);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 3, 10, 100);
            this.SeekToRecordSecondary(tc.tableid, -10, -100);
            this.VerifyCurrentRecord(tc.tableid, 2, -10, -100);
            this.UpdateCurrentRecord(tc.tableid, 10, 100);

            this.SeekToRecordSecondary(tc.tableid, 10, 100);
            this.VerifyCurrentRecord(tc.tableid, 2, 10, 100);
            this.UpdateCurrentRecord(tc.tableid, 30, 300);
            this.VerifyCurrentRecord(tc.tableid, 2, 30, 300);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 3, 10, 100);
            this.MoveCursor(tc.tableid, JET_Move.Previous, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 2, 30, 300);
            this.SeekToRecordSecondary(tc.tableid, 30, 300);
            this.VerifyCurrentRecord(tc.tableid, 2, 30, 300);
            this.UpdateCurrentRecord(tc.tableid, 10, 100);

            this.SeekToRecordSecondary(tc.tableid, 20, 200);
            this.VerifyCurrentRecord(tc.tableid, 1, 20, 200);
            this.UpdateCurrentRecord(tc.tableid, 40, 400);
            this.VerifyCurrentRecord(tc.tableid, 1, 40, 400);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 1, 40, 400);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 3, 10, 100);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 1, 40, 400);
            this.SeekToRecordSecondary(tc.tableid, 40, 400);
            this.VerifyCurrentRecord(tc.tableid, 1, 40, 400);
            this.UpdateCurrentRecord(tc.tableid, 20, 200);

            this.SeekToRecordSecondary(tc.tableid, 40, 400);
            this.VerifyCurrentRecord(tc.tableid, 4, 40, 400);
            this.UpdateCurrentRecord(tc.tableid, 50, 500);
            this.VerifyCurrentRecord(tc.tableid, 4, 50, 500);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 4, 50, 500);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 1, 20, 200);
            this.SeekToRecordSecondary(tc.tableid, 50, 500);
            this.VerifyCurrentRecord(tc.tableid, 4, 50, 500);
            this.UpdateCurrentRecord(tc.tableid, 40, 400);

            this.SeekToRecordSecondary(tc.tableid, 40, 400);
            this.VerifyCurrentRecord(tc.tableid, 4, 40, 400);
            this.UpdateCurrentRecord(tc.tableid, 10, 100);
            this.VerifyCurrentRecord(tc.tableid, 4, 10, 100);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 1, 20, 200);
            this.MoveCursor(tc.tableid, JET_Move.Next, typeof(EsentNoCurrentRecordException));
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.MoveCursor(tc.tableid, JET_Move.Previous);
            this.VerifyCurrentRecord(tc.tableid, 4, 10, 100);
            this.SeekToRecordSecondary(tc.tableid, 10, 100);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.MoveCursor(tc.tableid, JET_Move.Next);
            this.VerifyCurrentRecord(tc.tableid, 4, 10, 100);
            this.UpdateCurrentRecord(tc.tableid, 40, 400);
        }

        #endregion DML and currency Tests

        #region Helpers

        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>The newly created table.</returns>
        private JET_TABLECREATE CreateTable(string tableName)
        {
            const string ColumnKeyName = "columnkey";
            const string ColumnData1Name = "columndata1";
            const string ColumnData2Name = "columndata2";
            string clustIndexKey = string.Format("+{0}\0\0", ColumnKeyName);
            string secIndexWithPrimaryKey = string.Format("+{0}\0-{1}\0\0", ColumnData1Name, ColumnKeyName);
            string secIndexWithoutPrimaryKey = string.Format("+{0}\0+{1}\0\0", ColumnData1Name, ColumnData2Name);

            JET_COLUMNCREATE[] columnCreates =
            {
                new JET_COLUMNCREATE
                {
                    szColumnName = ColumnKeyName,
                    coltyp = JET_coltyp.Long
                },

                new JET_COLUMNCREATE
                {
                    szColumnName = ColumnData1Name,
                    coltyp = JET_coltyp.Long
                },

                new JET_COLUMNCREATE
                {
                    szColumnName = ColumnData2Name,
                    coltyp = JET_coltyp.Long
                }
            };

            JET_INDEXCREATE[] indexCreates = 
            {
                new JET_INDEXCREATE
                {
                    szIndexName = this.clustIndexName,
                    szKey = clustIndexKey,
                    cbKey = clustIndexKey.Length,
                    grbit = CreateIndexGrbit.IndexPrimary,
                },

                new JET_INDEXCREATE
                {
                    szIndexName = this.secIndexWithPrimaryName,
                    szKey = secIndexWithPrimaryKey,
                    cbKey = secIndexWithPrimaryKey.Length,
                    grbit = CreateIndexGrbit.None
                },

                new JET_INDEXCREATE
                {
                    szIndexName = this.secIndexWithoutPrimaryName,
                    szKey = secIndexWithoutPrimaryKey,
                    cbKey = secIndexWithoutPrimaryKey.Length,
                    grbit = CreateIndexGrbit.None
                }
            };

            JET_TABLECREATE tc = new JET_TABLECREATE
            {
                szTableName = tableName,
                rgcolumncreate = columnCreates,
                cColumns = columnCreates.Length,
                rgindexcreate = indexCreates,
                cIndexes = indexCreates.Length,
                grbit = CreateTableColumnIndexGrbit.None
            };

            Api.JetCreateTableColumnIndex3(this.sesId, this.dbId, tc);

            Assert.AreEqual<int>(7, tc.cCreated);  // 1 table + 3 colummns + 3 indexes.
            Assert.AreNotEqual<JET_TABLEID>(JET_TABLEID.Nil, tc.tableid);

            this.columnIdKey = Api.GetTableColumnid(this.sesId, tc.tableid, ColumnKeyName);
            this.columnIdData1 = Api.GetTableColumnid(this.sesId, tc.tableid, ColumnData1Name);
            this.columnIdData2 = Api.GetTableColumnid(this.sesId, tc.tableid, ColumnData2Name);

            Assert.AreNotEqual<JET_COLUMNID>(JET_COLUMNID.Nil, this.columnIdKey);
            Assert.AreNotEqual<JET_COLUMNID>(JET_COLUMNID.Nil, this.columnIdData1);
            Assert.AreNotEqual<JET_COLUMNID>(JET_COLUMNID.Nil, this.columnIdData1);
            Assert.AreNotEqual<JET_COLUMNID>(this.columnIdKey, this.columnIdData1);
            Assert.AreNotEqual<JET_COLUMNID>(this.columnIdKey, this.columnIdData2);

            return tc;
        }

        /// <summary>
        /// Gets the data1 default.
        /// </summary>
        /// <param name="key">The key to retrieve.</param>
        /// <returns>The default data.</returns>
        private int GetData1Default(int key)
        {
            return 10 * key;
        }

        /// <summary>
        /// Gets the data2 default.
        /// </summary>
        /// <param name="data1">The data1.</param>
        /// <returns>The default data.</returns>
        private int GetData2Default(int data1)
        {
            return 10 * data1;
        }

        /// <summary>
        /// Inserts the record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="key">The key to insert.</param>
        /// <param name="data1">The data1.</param>
        /// <param name="data2">The data2.</param>
        /// <returns>The bookmark.</returns>
        private byte[] InsertRecord(JET_TABLEID tableId, int key, int data1, int data2)
        {
            byte[] keyArray = BitConverter.GetBytes(key);
            byte[] data1Array = BitConverter.GetBytes(data1);
            byte[] data2Array = BitConverter.GetBytes(data2);
            byte[] bookmark = new byte[sizeof(int) + 1]; // +1 for ascending/descending info.

            Api.JetPrepareUpdate(this.sesId, tableId, JET_prep.Insert);
            Api.JetSetColumn(this.sesId, tableId, this.columnIdKey, keyArray, keyArray.Length, SetColumnGrbit.None, null);
            Api.JetSetColumn(this.sesId, tableId, this.columnIdData1, data1Array, data1Array.Length, SetColumnGrbit.None, null);
            Api.JetSetColumn(this.sesId, tableId, this.columnIdData2, data2Array, data2Array.Length, SetColumnGrbit.None, null);

            int actualBookmarkSize;
            Api.JetUpdate(this.sesId, tableId, bookmark, bookmark.Length, out actualBookmarkSize);
            Assert.AreEqual<int>(bookmark.Length, actualBookmarkSize);

            return bookmark;
        }

        /// <summary>
        /// Inserts the record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="key">The key to insert.</param>
        /// <param name="data1">The data1.</param>
        /// <returns>The bookmark.</returns>
        private byte[] InsertRecord(JET_TABLEID tableId, int key, int data1)
        {
            int data2 = this.GetData2Default(data1);

            return this.InsertRecord(tableId, key, data1, data2);
        }

        /// <summary>
        /// Inserts the record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="key">The key to insert.</param>
        /// <returns>The bookmark of the record.</returns>
        private byte[] InsertRecord(JET_TABLEID tableId, int key)
        {
            int data1 = this.GetData1Default(key);
            int data2 = this.GetData2Default(data1);

            return this.InsertRecord(tableId, key, data1, data2);
        }

        /// <summary>
        /// Moves the cursor.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="exTypeExpected">The ex type expected.</param>
        private void MoveCursor(JET_TABLEID tableId, JET_Move offset, Type exTypeExpected)
        {
            try
            {
                Api.JetMove(this.sesId, tableId, offset, MoveGrbit.None);
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
        /// <param name="tableId">The table id.</param>
        /// <param name="offset">The offset.</param>
        private void MoveCursor(JET_TABLEID tableId, JET_Move offset)
        {
            this.MoveCursor(tableId, offset, null);
        }

        /// <summary>
        /// Makes the key clustered.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="key">The key to make.</param>
        private void MakeKeyClustered(JET_TABLEID tableId, int key)
        {
            byte[] keyArray = BitConverter.GetBytes(key);
            Api.JetMakeKey(this.sesId, tableId, keyArray, keyArray.Length, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Seeks to record clustered.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="key">The key to seek.</param>
        /// <param name="seekGrbit">The seek grbit.</param>
        /// <param name="errExpected">The err expected.</param>
        /// <param name="exTypeExpected">The ex type expected.</param>
        private void SeekToRecordClustered(JET_TABLEID tableId, int key, SeekGrbit seekGrbit, JET_wrn errExpected, Type exTypeExpected)
        {
            this.MakeKeyClustered(tableId, key);

            try
            {
                JET_wrn err = Api.JetSeek(this.sesId, tableId, seekGrbit);
                if (exTypeExpected != null)
                {
                    Assert.Fail("Should have thrown {0}", exTypeExpected);
                }

                Assert.AreEqual<JET_wrn>(errExpected, err);
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
        /// Seeks to record clustered.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="key">The key to seek.</param>
        private void SeekToRecordClustered(JET_TABLEID tableId, int key)
        {
            this.SeekToRecordClustered(tableId, key, SeekGrbit.SeekEQ, JET_wrn.Success, null);
        }

        /// <summary>
        /// Makes the key secondary.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="data1">The data1.</param>
        /// <param name="data2">The data2.</param>
        private void MakeKeySecondary(JET_TABLEID tableId, int data1, int data2)
        {
            byte[] data1Array = BitConverter.GetBytes(data1);
            byte[] data2Array = BitConverter.GetBytes(data2);
            Api.JetMakeKey(this.sesId, tableId, data1Array, data1Array.Length, MakeKeyGrbit.NewKey);
            Api.JetMakeKey(this.sesId, tableId, data2Array, data2Array.Length, MakeKeyGrbit.None);
        }

        /// <summary>
        /// Seeks to record secondary.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="data1">The data1.</param>
        /// <param name="data2">The data2.</param>
        /// <param name="seekGrbit">The seek grbit.</param>
        /// <param name="errExpected">The err expected.</param>
        /// <param name="exTypeExpected">The ex type expected.</param>
        private void SeekToRecordSecondary(JET_TABLEID tableId, int data1, int data2, SeekGrbit seekGrbit, JET_wrn errExpected, Type exTypeExpected)
        {
            this.MakeKeySecondary(tableId, data1, data2);

            try
            {
                JET_wrn err = Api.JetSeek(this.sesId, tableId, seekGrbit);
                if (exTypeExpected != null)
                {
                    Assert.Fail("Should have thrown {0}", exTypeExpected);
                }

                Assert.AreEqual<JET_wrn>(errExpected, err);
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
        /// Seeks to record secondary.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="data1">The data1.</param>
        /// <param name="data2">The data2.</param>
        private void SeekToRecordSecondary(JET_TABLEID tableId, int data1, int data2)
        {
            this.SeekToRecordSecondary(tableId, data1, data2, SeekGrbit.SeekEQ, JET_wrn.Success, null);
        }

        /// <summary>
        /// Updates the current record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="data1">The data1.</param>
        /// <param name="data2">The data2.</param>
        private void UpdateCurrentRecord(JET_TABLEID tableId, int data1, int data2)
        {
            byte[] data1Array = BitConverter.GetBytes(data1);
            byte[] data2Array = BitConverter.GetBytes(data2);

            Api.JetPrepareUpdate(this.sesId, tableId, JET_prep.Replace);
            Api.JetSetColumn(this.sesId, tableId, this.columnIdData1, data1Array, data1Array.Length, SetColumnGrbit.None, null);
            Api.JetSetColumn(this.sesId, tableId, this.columnIdData2, data2Array, data2Array.Length, SetColumnGrbit.None, null);
            Api.JetUpdate(this.sesId, tableId);
        }

        /// <summary>
        /// Updates the current record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="data1">The data1.</param>
        private void UpdateCurrentRecord(JET_TABLEID tableId, int data1)
        {
            int data2 = this.GetData2Default(data1);

            this.UpdateCurrentRecord(tableId, data1, data2);
        }

        /// <summary>
        /// Deletes the current record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        private void DeleteCurrentRecord(JET_TABLEID tableId)
        {
            Api.JetDelete(this.sesId, tableId);
        }

        /// <summary>
        /// Verifies the current record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="keyExpected">The key expected.</param>
        /// <param name="data1Expected">The data1 expected.</param>
        /// <param name="data2Expected">The data2 expected.</param>
        /// <param name="exTypeExpected">The ex type expected.</param>
        private void VerifyCurrentRecord(JET_TABLEID tableId, int keyExpected, int data1Expected, int data2Expected, Type exTypeExpected)
        {
            byte[] keyArray = new byte[sizeof(int)];
            byte[] data1Array = new byte[sizeof(int)];
            byte[] data2Array = new byte[sizeof(int)];

            JET_RETRIEVECOLUMN[] retrieveColumns = new[]
            {
                new JET_RETRIEVECOLUMN
                {
                    columnid = this.columnIdKey,
                    pvData = keyArray,
                    cbData = keyArray.Length,
                    itagSequence = 1
                },

                new JET_RETRIEVECOLUMN
                {
                    columnid = this.columnIdData1,
                    pvData = data1Array,
                    cbData = data1Array.Length,
                    itagSequence = 1
                },

                new JET_RETRIEVECOLUMN
                {
                    columnid = this.columnIdData2,
                    pvData = data2Array,
                    cbData = data2Array.Length,
                    itagSequence = 1
                }
            };

            try
            {
                JET_wrn err = Api.JetRetrieveColumns(this.sesId, tableId, retrieveColumns, retrieveColumns.Length);

                if (exTypeExpected != null)
                {
                    Assert.Fail("Should have thrown {0}", exTypeExpected);
                }

                Assert.AreEqual<JET_wrn>(JET_wrn.Success, err);
                Assert.AreEqual<int>(keyArray.Length, retrieveColumns[0].cbActual);
                Assert.AreEqual<int>(data1Array.Length, retrieveColumns[1].cbActual);
                Assert.AreEqual<int>(data1Array.Length, retrieveColumns[2].cbActual);

                int keyActual = BitConverter.ToInt32(keyArray, 0);
                int data1Actual = BitConverter.ToInt32(data1Array, 0);
                int data2Actual = BitConverter.ToInt32(data2Array, 0);

                Assert.AreEqual<int>(keyExpected, keyActual);
                Assert.AreEqual<int>(data1Expected, data1Actual);
                Assert.AreEqual<int>(data2Expected, data2Actual);
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
        /// <param name="tableId">The table id.</param>
        /// <param name="keyExpected">The key expected.</param>
        /// <param name="data1Expected">The data1 expected.</param>
        /// <param name="data2Expected">The data2 expected.</param>
        private void VerifyCurrentRecord(JET_TABLEID tableId, int keyExpected, int data1Expected, int data2Expected)
        {
            this.VerifyCurrentRecord(tableId, keyExpected, data1Expected, data2Expected, null);
        }

        /// <summary>
        /// Verifies the current record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="keyExpected">The key expected.</param>
        /// <param name="data1Expected">The data1 expected.</param>
        /// <param name="exTypeExpected">The ex type expected.</param>
        private void VerifyCurrentRecord(JET_TABLEID tableId, int keyExpected, int data1Expected, Type exTypeExpected)
        {
            int data2Expected = this.GetData2Default(data1Expected);

            this.VerifyCurrentRecord(tableId, keyExpected, data1Expected, data2Expected, exTypeExpected);
        }

        /// <summary>
        /// Verifies the current record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="keyExpected">The key expected.</param>
        /// <param name="data1Expected">The data1 expected.</param>
        private void VerifyCurrentRecord(JET_TABLEID tableId, int keyExpected, int data1Expected)
        {
            this.VerifyCurrentRecord(tableId, keyExpected, data1Expected, null);
        }

        /// <summary>
        /// Verifies the current record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="keyExpected">The key expected.</param>
        /// <param name="exTypeExpected">The ex type expected.</param>
        private void VerifyCurrentRecord(JET_TABLEID tableId, int keyExpected, Type exTypeExpected)
        {
            int data1Expected = this.GetData2Default(keyExpected);
            int data2Expected = this.GetData2Default(data1Expected);

            this.VerifyCurrentRecord(tableId, keyExpected, data1Expected, data2Expected, exTypeExpected);
        }

        /// <summary>
        /// Verifies the current record.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="keyExpected">The key expected.</param>
        private void VerifyCurrentRecord(JET_TABLEID tableId, int keyExpected)
        {
            this.VerifyCurrentRecord(tableId, keyExpected, null);
        }

        /// <summary>
        /// Sets the index range.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="setIndexRangeGrbit">The set index range grbit.</param>
        /// <param name="exTypeExpected">The ex type expected.</param>
        private void SetIndexRange(JET_TABLEID tableId, SetIndexRangeGrbit setIndexRangeGrbit, Type exTypeExpected)
        {
            try
            {
                Api.JetSetIndexRange(this.sesId, tableId, setIndexRangeGrbit);

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
        /// Makes the key and set index range clustered.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="key">The key to make.</param>
        /// <param name="setIndexRangeGrbit">The set index range grbit.</param>
        /// <param name="exTypeExpected">The ex type expected.</param>
        private void MakeKeyAndSetIndexRangeClustered(JET_TABLEID tableId, int key, SetIndexRangeGrbit setIndexRangeGrbit, Type exTypeExpected)
        {
            this.MakeKeyClustered(tableId, key);
            this.SetIndexRange(tableId, setIndexRangeGrbit, exTypeExpected);
        }

        /// <summary>
        /// Makes the key and set index range secondary.
        /// </summary>
        /// <param name="tableId">The table id.</param>
        /// <param name="data1">The data1.</param>
        /// <param name="data2">The data2.</param>
        /// <param name="setIndexRangeGrbit">The set index range grbit.</param>
        /// <param name="exTypeExpected">The ex type expected.</param>
        private void MakeKeyAndSetIndexRangeSecondary(JET_TABLEID tableId, int data1, int data2, SetIndexRangeGrbit setIndexRangeGrbit, Type exTypeExpected)
        {
            this.MakeKeySecondary(tableId, data1, data2);
            this.SetIndexRange(tableId, setIndexRangeGrbit, exTypeExpected);
        }

        #endregion Helpers
    }
}