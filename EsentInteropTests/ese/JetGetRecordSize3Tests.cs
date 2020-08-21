//-----------------------------------------------------------------------
// <copyright file="JetGetRecordSize3Tests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.IO;
    using Microsoft.Database.Isam.Config;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Unpublished;
    using Microsoft.Isam.Esent.Interop.Vista;
    using Microsoft.Isam.Esent.Interop.Windows7;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for JetGetThreadStats
    /// </summary>
    [TestClass]
    public partial class JetGetRecordSizeTests
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
        /// Initialization method. Called once when the tests are started.
        /// All DDL should be done in this method.
        /// </summary>
        [TestInitialize]
        [Description("Setup for JetGetThreadStatsTests")]
        public void Setup()
        {
            this.directory = SetupHelper.CreateRandomDirectory();
            this.database = Path.Combine(this.directory, "recsize3.edb");

            var config = new DatabaseConfig()
            {
                Recovery = "off",   // don't need recorvery, speed up the test
                DatabasePageSize = 8 * 1024,
            };

            config.SetGlobalParams();
            this.instance = SetupHelper.CreateNewInstance(this.directory);
            config.SetInstanceParams(this.instance);

            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesid, string.Empty, string.Empty);
            Api.JetCreateDatabase(this.sesid, this.database, string.Empty, out this.dbid, CreateDatabaseGrbit.None);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        [Description("Cleanup for JetGetThreadStatsTests")]
        public void Teardown()
        {
            Api.JetEndSession(this.sesid, EndSessionGrbit.None);
            Api.JetTerm(this.instance);
            Cleanup.DeleteDirectoryWithRetry(this.directory);
            SetupHelper.CheckProcessForInstanceLeaks();
        }

        /// <summary>
        /// Call JetGetRecordSize3() with different schema variations.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Call JetGetRecordSize3() with different schema variations.")]
        public void JetGetRecordSize3()
        {
            var recsize = new JET_RECSIZE();
            var tablecreate = new JET_TABLECREATE()
            {
                szTableName = "table0",
                cColumns = 4,
                rgcolumncreate = new JET_COLUMNCREATE[4]
                {
                        new JET_COLUMNCREATE() { szColumnName = "key", coltyp = JET_coltyp.Long, grbit = ColumndefGrbit.ColumnAutoincrement | ColumndefGrbit.ColumnFixed },
                        new JET_COLUMNCREATE() { szColumnName = "var0", coltyp = JET_coltyp.Binary, grbit = ColumndefGrbit.ColumnMaybeNull },
                        new JET_COLUMNCREATE() { szColumnName = "tagged0", coltyp = JET_coltyp.LongBinary, grbit = ColumndefGrbit.ColumnTagged | ColumndefGrbit.ColumnMultiValued },
                        new JET_COLUMNCREATE() { szColumnName = "tagged1", coltyp = JET_coltyp.Long, grbit = ColumndefGrbit.ColumnTagged | ColumndefGrbit.ColumnMultiValued },
                },
            };

            Api.JetCreateTableColumnIndex3(this.sesid, this.dbid, tablecreate);
            Assert.AreEqual(1 + tablecreate.rgcolumncreate.Length, tablecreate.cCreated);

            JET_TABLEID tableid = tablecreate.tableid;
            JET_COLUMNID colidKey = tablecreate.rgcolumncreate[0].columnid;
            JET_COLUMNID colidVar0 = tablecreate.rgcolumncreate[1].columnid;
            JET_COLUMNID colidTagged0 = tablecreate.rgcolumncreate[2].columnid;
            JET_COLUMNID colidTagged1 = tablecreate.rgcolumncreate[3].columnid;
            byte[] data = this.GetCompressibleBytes(512);

            Action<bool, int> verifyFixed = (bool fKey, int totalMultiplier) =>
            {
                Assert.AreEqual(totalMultiplier * 2, recsize.cNonTaggedColumns);
                Assert.AreEqual(totalMultiplier * 1, recsize.cTaggedColumns);
                Assert.AreEqual(totalMultiplier * (fKey ? 4 : 0), recsize.cbKey);
            };

            Action<int, int, int> verify = (int intLVs, int sepLVs, int totalMultiplier) =>
            {
                Assert.AreEqual(totalMultiplier * intLVs, recsize.cIntrinsicLongValues);
                Assert.AreEqual(totalMultiplier * sepLVs, recsize.cLongValues);
                Assert.AreEqual(totalMultiplier * intLVs * data.Length, recsize.cbIntrinsicLongValueData);
                Assert.AreEqual(totalMultiplier * sepLVs * data.Length, recsize.cbLongValueData);

                // For intLVs, only the first value in the tagged column can be compressed, because the compressed flag is stored in the TAGFLD_HEADER
                Assert.IsTrue(intLVs == 0 || (totalMultiplier * intLVs * data.Length >= recsize.cbIntrinsicLongValueDataCompressed && recsize.cbIntrinsicLongValueDataCompressed > 0));
                Assert.IsTrue(sepLVs == 0 || (totalMultiplier * sepLVs * data.Length > recsize.cbLongValueDataCompressed && recsize.cbLongValueDataCompressed > 0));
            };

            // Test 1 intrinsic LV column
            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, tableid, JET_prep.Insert);

            Api.JetSetColumn(this.sesid, tableid, colidVar0, data, 200, SetColumnGrbit.None, null);
            Api.JetSetColumn(this.sesid, tableid, colidTagged0, data, data.Length, SetColumnGrbit.IntrinsicLV | Windows7Grbits.Compressed, null);

            UnpublishedApi.JetGetRecordSize3(this.sesid, tableid, ref recsize, GetRecordSizeGrbit.InCopyBuffer);
            verifyFixed(false, 1);
            verify(1, 0, 1);

            Api.JetUpdate(this.sesid, tableid);

            UnpublishedApi.JetGetRecordSize3(this.sesid, tableid, ref recsize, GetRecordSizeGrbit.None);
            verifyFixed(true, 1);
            verify(1, 0, 1);
            UnpublishedApi.JetGetRecordSize3(this.sesid, tableid, ref recsize, GetRecordSizeGrbit.RunningTotal);
            verifyFixed(true, 2);
            verify(1, 0, 2);

            // Test adding more intrinsic LVs to the current row.
            Api.JetPrepareUpdate(this.sesid, tableid, JET_prep.Replace);
            Api.JetSetColumn(this.sesid, tableid, colidTagged0, data, data.Length, SetColumnGrbit.IntrinsicLV | Windows7Grbits.Compressed, new JET_SETINFO() { itagSequence = 0 });

            UnpublishedApi.JetGetRecordSize3(this.sesid, tableid, ref recsize, GetRecordSizeGrbit.InCopyBuffer);
            verifyFixed(true, 1);
            verify(2, 0, 1);

            Api.JetSetColumn(this.sesid, tableid, colidTagged0, data, data.Length, SetColumnGrbit.IntrinsicLV | Windows7Grbits.Compressed, new JET_SETINFO() { itagSequence = 0 });
            UnpublishedApi.JetGetRecordSize3(this.sesid, tableid, ref recsize, GetRecordSizeGrbit.InCopyBuffer);
            verify(3, 0, 1);

            Api.JetUpdate(this.sesid, tableid);

            UnpublishedApi.JetGetRecordSize3(this.sesid, tableid, ref recsize, GetRecordSizeGrbit.None);
            verifyFixed(true, 1);
            verify(3, 0, 1);

            // Test 1 separated LV.
            Api.JetPrepareUpdate(this.sesid, tableid, JET_prep.Insert);

            Api.JetSetColumn(this.sesid, tableid, colidVar0, data, 200, SetColumnGrbit.None, null);
            Api.JetSetColumn(this.sesid, tableid, colidTagged0, data, data.Length, SetColumnGrbit.SeparateLV | Windows7Grbits.Compressed, null);

            UnpublishedApi.JetGetRecordSize3(this.sesid, tableid, ref recsize, GetRecordSizeGrbit.InCopyBuffer);
            verifyFixed(false, 1);
            verify(0, 1, 1);

            Api.JetUpdate(this.sesid, tableid);
            Api.JetMove(this.sesid, tableid, JET_Move.Last, MoveGrbit.None);

            UnpublishedApi.JetGetRecordSize3(this.sesid, tableid, ref recsize, GetRecordSizeGrbit.None);
            verifyFixed(true, 1);
            verify(0, 1, 1);
            UnpublishedApi.JetGetRecordSize3(this.sesid, tableid, ref recsize, GetRecordSizeGrbit.RunningTotal);
            verifyFixed(true, 2);
            verify(0, 1, 2);

            // Test adding one more separated LVs to the current row.
            Api.JetPrepareUpdate(this.sesid, tableid, JET_prep.Replace);
            Api.JetSetColumn(this.sesid, tableid, colidTagged0, data, data.Length, SetColumnGrbit.SeparateLV | Windows7Grbits.Compressed, new JET_SETINFO() { itagSequence = 0 });

            UnpublishedApi.JetGetRecordSize3(this.sesid, tableid, ref recsize, GetRecordSizeGrbit.InCopyBuffer);
            verifyFixed(true, 1);
            verify(0, 2, 1);

            Api.JetSetColumn(this.sesid, tableid, colidTagged0, data, data.Length, SetColumnGrbit.SeparateLV | Windows7Grbits.Compressed, new JET_SETINFO() { itagSequence = 0 });
            UnpublishedApi.JetGetRecordSize3(this.sesid, tableid, ref recsize, GetRecordSizeGrbit.InCopyBuffer);
            verify(0, 3, 1);

            Api.JetUpdate(this.sesid, tableid);

            UnpublishedApi.JetGetRecordSize3(this.sesid, tableid, ref recsize, GetRecordSizeGrbit.None);
            verifyFixed(true, 1);
            verify(0, 3, 1);

            // Test Mixed Intrinsic LVs and separated LVs
            Api.JetPrepareUpdate(this.sesid, tableid, JET_prep.Replace);
            Api.JetSetColumn(this.sesid, tableid, colidTagged0, data, data.Length, SetColumnGrbit.IntrinsicLV | Windows7Grbits.Compressed, new JET_SETINFO() { itagSequence = 0 });
            Api.JetSetColumn(this.sesid, tableid, colidTagged0, data, data.Length, SetColumnGrbit.IntrinsicLV | Windows7Grbits.Compressed, new JET_SETINFO() { itagSequence = 0 });
            Api.JetUpdate(this.sesid, tableid);

            UnpublishedApi.JetGetRecordSize3(this.sesid, tableid, ref recsize, GetRecordSizeGrbit.None);
            verifyFixed(true, 1);
            verify(2, 3, 1);
            Assert.AreEqual(2 * data.Length, recsize.cbIntrinsicLongValueDataCompressed);   // only first intrinsic value can be compressed in a tagged column

            // Test GetRecordSizeGrbit.Local
            UnpublishedApi.JetGetRecordSize3(this.sesid, tableid, ref recsize, GetRecordSizeGrbit.Local);
            verifyFixed(true, 1);
            Assert.AreEqual(2, recsize.cIntrinsicLongValues);
            Assert.AreEqual(3, recsize.cLongValues);
            Assert.AreEqual(2 * data.Length, recsize.cbIntrinsicLongValueData);
            Assert.AreEqual(0, recsize.cbLongValueData);

            // Test non-LV column
            Api.JetPrepareUpdate(this.sesid, tableid, JET_prep.Insert);

            Api.JetSetColumn(this.sesid, tableid, colidVar0, data, 200, SetColumnGrbit.None, null);
            Api.SetColumn(this.sesid, tableid, colidTagged1, 42);

            byte[] longData = BitConverter.GetBytes(43);
            Api.JetSetColumn(this.sesid, tableid, colidTagged1, longData, longData.Length, SetColumnGrbit.None, new JET_SETINFO() { itagSequence = 0 });

            UnpublishedApi.JetGetRecordSize3(this.sesid, tableid, ref recsize, GetRecordSizeGrbit.InCopyBuffer);
            verifyFixed(false, 1);
            verify(0, 0, 1);
            Assert.AreEqual(4 + 200 + 4 + 4, recsize.cbData); // key + var0 + tagged1[2]

            Api.JetUpdate(this.sesid, tableid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.None);
            Api.JetCloseTable(this.sesid, tableid);
        }

        /// <summary>
        /// Returns a compressible byte array.
        /// </summary>
        /// <param name="cb">Number of bytes in the array.</param>
        /// <returns>The byte array.</returns>
        private byte[] GetCompressibleBytes(int cb)
        {
            var pattern = new byte[] { 0xa, 0xb, 0xc, 0xd, 0xe, 0xf, 0x0, 0x0 };

            var ret = new byte[cb];
            for (int i = 0; i < cb; i += pattern.Length)
            {
                Buffer.BlockCopy(pattern, 0, ret, i, Math.Min(pattern.Length, cb - i));
            }

            return ret;
        }
    }
}