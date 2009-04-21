//-----------------------------------------------------------------------
// <copyright file="IntersectIndexesTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using Microsoft.Isam.Esent.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
    /// <summary>
    /// Test JetMove
    /// </summary>
    [TestClass]
    public class IntersectIndexesTests
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
        /// Columnid of the Int32 column in the table.
        /// </summary>
        private JET_COLUMNID columnid1;

        /// <summary>
        /// Columnid of the Text column in the table.
        /// </summary>
        private JET_COLUMNID columnid2;

        #region Setup/Teardown

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// All DDL should be done in this method.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            JET_TABLEID tableid;

            this.directory = SetupHelper.CreateRandomDirectory();
            this.database = Path.Combine(this.directory, "database.edb");
            this.table = "table";
            this.instance = SetupHelper.CreateNewInstance(this.directory);

            // turn off logging so initialization is faster
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesid, String.Empty, String.Empty);
            Api.JetCreateDatabase(this.sesid, this.database, String.Empty, out this.dbid, CreateDatabaseGrbit.None);
            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateTable(this.sesid, this.dbid, this.table, 0, 100, out tableid);

            var columndef = new JET_COLUMNDEF { coltyp = JET_coltyp.Long };
            Api.JetAddColumn(this.sesid, tableid, "Column1", columndef, null, 0, out this.columnid1);
            Api.JetAddColumn(this.sesid, tableid, "Column2", columndef, null, 0, out this.columnid2);

            var indexDef = "+Column1\0\0";
            Api.JetCreateIndex(this.sesid, tableid, "index1", CreateIndexGrbit.None, indexDef, indexDef.Length, 100);

            indexDef = "+Column2\0\0";
            Api.JetCreateIndex(this.sesid, tableid, "index2", CreateIndexGrbit.None, indexDef, indexDef.Length, 100);

            // Create a cross-product of records. Index intersection can be used to select a subset.
            for (int i = 0; i < 10; ++i)
            {
                for (int j = 0; j < 10; ++j)
                {
                    Api.JetPrepareUpdate(this.sesid, tableid, JET_prep.Insert);
                    Api.SetColumn(this.sesid, tableid, this.columnid1, i);
                    Api.SetColumn(this.sesid, tableid, this.columnid2, j);
                    Api.JetUpdate(this.sesid, tableid);
                }
            }

            Api.JetCloseTable(this.sesid, tableid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            Api.JetEndSession(this.sesid, EndSessionGrbit.None);
            Api.JetTerm(this.instance);
            Directory.Delete(this.directory, true);
        }

        /// <summary>
        /// Verify that the test class has setup the test fixture properly.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void VerifyFixtureSetup()
        {
            Assert.AreNotEqual(JET_INSTANCE.Nil, this.instance);
            Assert.AreNotEqual(JET_SESID.Nil, this.sesid);
            Assert.AreNotEqual(JET_COLUMNID.Nil, this.columnid1);
            Assert.AreNotEqual(JET_COLUMNID.Nil, this.columnid2);
            Assert.AreNotEqual(this.columnid1, this.columnid2);
        }

        #endregion Setup/Teardown

        /// <summary>
        /// Verify that index intersection returns the correct number of records.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void VerifyIndexIntersectionReturnsCorrectNumberOfRecords()
        {
            JET_TABLEID tableid1 = this.OpenTable();
            JET_TABLEID tableid2 = this.OpenTable();

            Api.JetSetCurrentIndex(this.sesid, tableid1, "index1");
            this.SetIndexRange(tableid1, 4, 6);

            Api.JetSetCurrentIndex(this.sesid, tableid2, "index2");
            this.SetIndexRange(tableid2, 1, 3);

            var tableids = new[] { tableid1, tableid2 };

            JET_RECORDLIST recordlist;
            Api.JetIntersectIndexes(this.sesid, tableids, 2, out recordlist, IntersectIndexesGrbit.None);

            Assert.AreEqual(9, recordlist.cRecords);
            Api.JetCloseTable(this.sesid, recordlist.tableid);
        }

        /// <summary>
        /// Verify that index intersection returns records with the correct criteria.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void VerifyIndexIntersectionReturnsCorrectRecords()
        {
            JET_TABLEID tableid1 = this.OpenTable();
            JET_TABLEID tableid2 = this.OpenTable();

            Api.JetSetCurrentIndex(this.sesid, tableid1, "index1");
            this.SetIndexRange(tableid1, 8, 9);

            Api.JetSetCurrentIndex(this.sesid, tableid2, "index2");
            this.SetIndexRange(tableid2, 1, 2);

            var tableids = new[] { tableid1, tableid2 };

            var bookmarks = Api.IntersectIndexes(this.sesid, tableids);
            foreach (byte[] bookmark in bookmarks)
            {
                Api.JetGotoBookmark(this.sesid, tableid1, bookmark, bookmark.Length);
                int i = (int)Api.RetrieveColumnAsInt32(this.sesid, tableid1, this.columnid1);
                int j = (int)Api.RetrieveColumnAsInt32(this.sesid, tableid1, this.columnid2);
                Assert.IsTrue(8 <= i && i <= 9);
                Assert.IsTrue(1 <= j && j <= 2);
            }
        }

        #region Helper methods

        private JET_TABLEID OpenTable()
        {
            JET_TABLEID tableid;
            Api.JetOpenTable(this.sesid, this.dbid, this.table, null, 0, OpenTableGrbit.None, out tableid);
            return tableid;
        }

        private void SetIndexRange(JET_TABLEID tableid, int min, int max)
        {
            Api.MakeKey(this.sesid, tableid, min, MakeKeyGrbit.NewKey);
            Api.JetSeek(this.sesid, tableid, SeekGrbit.SeekEQ);
            Api.MakeKey(this.sesid, tableid, max, MakeKeyGrbit.NewKey);
            Api.JetSetIndexRange(this.sesid, tableid, SetIndexRangeGrbit.RangeInclusive | SetIndexRangeGrbit.RangeUpperLimit);
        }
        #endregion
    }
}