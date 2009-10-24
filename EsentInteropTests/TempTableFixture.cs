//-----------------------------------------------------------------------
// <copyright file="TempTableFixture.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Server2003;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests that use the temporary table fixture, which is very quick to setup.
    /// </summary>
    [TestClass]
    public class TempTableFixture
    {
        /// <summary>
        /// The instance used by the test.
        /// </summary>
        private Instance instance;

        /// <summary>
        /// The session used by the test.
        /// </summary>
        private Session session;

        /// <summary>
        /// The tableid being used by the test.
        /// </summary>
        private JET_TABLEID tableid;

        /// <summary>
        /// A dictionary that maps column types to column ids.
        /// </summary>
        private IDictionary<JET_coltyp, JET_COLUMNID> coltypDict;

        #region Setup/Teardown

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// All DDL should be done in this method.
        /// </summary>
        [TestInitialize]
        [Description("Setup the TempTableFixture fixture")]
        public void Setup()
        {
            this.instance = new Instance("TempTableFixture");
            this.instance.Parameters.Recovery = false;
            this.instance.Parameters.PageTempDBMin = SystemParameters.PageTempDBSmallest;
            this.instance.Parameters.NoInformationEvent = true;
            this.instance.Init();

            this.session = new Session(this.instance);

            var columndefs = new[]
            {
                new JET_COLUMNDEF { coltyp = JET_coltyp.Binary },
                new JET_COLUMNDEF { coltyp = JET_coltyp.Bit },
                new JET_COLUMNDEF { coltyp = JET_coltyp.Currency },
                new JET_COLUMNDEF { coltyp = JET_coltyp.DateTime },
                new JET_COLUMNDEF { coltyp = JET_coltyp.IEEEDouble },
                new JET_COLUMNDEF { coltyp = JET_coltyp.IEEESingle },
                new JET_COLUMNDEF { coltyp = JET_coltyp.Long },
                new JET_COLUMNDEF { coltyp = JET_coltyp.LongBinary },
                new JET_COLUMNDEF { coltyp = JET_coltyp.LongText, cp = JET_CP.Unicode },
                new JET_COLUMNDEF { coltyp = JET_coltyp.Short },
                new JET_COLUMNDEF { coltyp = JET_coltyp.Text, cp = JET_CP.Unicode },
                new JET_COLUMNDEF { coltyp = JET_coltyp.UnsignedByte },
            };

            var columnids = new JET_COLUMNID[columndefs.Length];
            Api.JetOpenTempTable(this.session, columndefs, columndefs.Length, TempTableGrbit.ForceMaterialization, out this.tableid, columnids);

            this.coltypDict = new Dictionary<JET_coltyp, JET_COLUMNID>(columndefs.Length);
            for (int i = 0; i < columndefs.Length; ++i)
            {
                this.coltypDict[columndefs[i].coltyp] = columnids[i];
            }
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        [Description("Cleanup the TempTableFixture fixture")]
        public void Teardown()
        {
            this.instance.Term();
        }

        #endregion Setup/Teardown

        /// <summary>
        /// Test JetUpdate2.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test JetUpdate2")]
        public void TestJetUpdate2()
        {
            if (!EsentVersion.SupportsServer2003Features)
            {
                return;
            }

            JET_COLUMNID columnid = this.coltypDict[JET_coltyp.Long];
            int value = Any.Int32;

            using (var trx = new Transaction(this.session))
            {
                Api.JetPrepareUpdate(this.session, this.tableid, JET_prep.Insert);
                Api.SetColumn(this.session, this.tableid, columnid, value);
                int ignored;
                Server2003Api.JetUpdate2(this.session, this.tableid, null, 0, out ignored, UpdateGrbit.None);
                trx.Commit(CommitTransactionGrbit.None);
            }

            Api.TryMoveFirst(this.session, this.tableid);
            Assert.AreEqual(value, Api.RetrieveColumnAsInt32(this.session, this.tableid, columnid));
        }

        #region Set and Retrieve Columns

        /// <summary>
        /// Test JetSetColumns.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test JetSetColumns")]
        public void JetSetColumns()
        {
            byte b = Any.Byte;
            short s = Any.Int16;
            int i = Any.Int32;
            long l = Any.Int64;
            string str = Any.String;
            float f = Any.Float;
            double d = Any.Double;
            byte[] data = Any.BytesOfLength(1023);

            var setcolumns = new[]
            {
                new JET_SETCOLUMN { cbData = sizeof(byte), columnid = this.coltypDict[JET_coltyp.UnsignedByte], pvData = BitConverter.GetBytes(b) },
                new JET_SETCOLUMN { cbData = sizeof(short), columnid = this.coltypDict[JET_coltyp.Short], pvData = BitConverter.GetBytes(s) },
                new JET_SETCOLUMN { cbData = sizeof(int), columnid = this.coltypDict[JET_coltyp.Long], pvData = BitConverter.GetBytes(i) },
                new JET_SETCOLUMN { cbData = sizeof(long), columnid = this.coltypDict[JET_coltyp.Currency], pvData = BitConverter.GetBytes(l) },
                new JET_SETCOLUMN { cbData = sizeof(float), columnid = this.coltypDict[JET_coltyp.IEEESingle], pvData = BitConverter.GetBytes(f) },
                new JET_SETCOLUMN { cbData = sizeof(double), columnid = this.coltypDict[JET_coltyp.IEEEDouble], pvData = BitConverter.GetBytes(d) },
                new JET_SETCOLUMN { cbData = str.Length * sizeof(char), columnid = this.coltypDict[JET_coltyp.LongText], pvData = Encoding.Unicode.GetBytes(str) },
                new JET_SETCOLUMN { cbData = data.Length, columnid = this.coltypDict[JET_coltyp.LongBinary], pvData = data },
            };

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                Api.JetSetColumns(this.session, this.tableid, setcolumns, setcolumns.Length);
                update.Save();
                trx.Commit(CommitTransactionGrbit.None);
            }

            Api.TryMoveFirst(this.session, this.tableid);

            Assert.AreEqual(b, Api.RetrieveColumnAsByte(this.session, this.tableid, this.coltypDict[JET_coltyp.UnsignedByte]));
            Assert.AreEqual(s, Api.RetrieveColumnAsInt16(this.session, this.tableid, this.coltypDict[JET_coltyp.Short]));
            Assert.AreEqual(i, Api.RetrieveColumnAsInt32(this.session, this.tableid, this.coltypDict[JET_coltyp.Long]));
            Assert.AreEqual(l, Api.RetrieveColumnAsInt64(this.session, this.tableid, this.coltypDict[JET_coltyp.Currency]));
            Assert.AreEqual(f, Api.RetrieveColumnAsFloat(this.session, this.tableid, this.coltypDict[JET_coltyp.IEEESingle]));
            Assert.AreEqual(d, Api.RetrieveColumnAsDouble(this.session, this.tableid, this.coltypDict[JET_coltyp.IEEEDouble]));
            Assert.AreEqual(str, Api.RetrieveColumnAsString(this.session, this.tableid, this.coltypDict[JET_coltyp.LongText]));
            CollectionAssert.AreEqual(data, Api.RetrieveColumn(this.session, this.tableid, this.coltypDict[JET_coltyp.LongBinary]));
        }

        /// <summary>
        /// Test JetRetrieveColumns.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test JetRetrieveColumns")]
        public void JetRetrieveColumns()
        {
            short s = Any.Int16;
            string str = Any.String;
            double d = Any.Double;

            var setcolumns = new[]
            {
                new JET_SETCOLUMN { cbData = sizeof(short), columnid = this.coltypDict[JET_coltyp.Short], pvData = BitConverter.GetBytes(s) },
                new JET_SETCOLUMN { cbData = sizeof(double), columnid = this.coltypDict[JET_coltyp.IEEEDouble], pvData = BitConverter.GetBytes(d) },
                new JET_SETCOLUMN { cbData = str.Length * sizeof(char), columnid = this.coltypDict[JET_coltyp.LongText], pvData = Encoding.Unicode.GetBytes(str) },
                new JET_SETCOLUMN { cbData = 0, columnid = this.coltypDict[JET_coltyp.LongBinary], pvData = null },
            };

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                Api.JetSetColumns(this.session, this.tableid, setcolumns, setcolumns.Length);
                update.Save();
                trx.Commit(CommitTransactionGrbit.None);
            }

            Api.TryMoveFirst(this.session, this.tableid);

            var retrievecolumns = new[]
            {
                new JET_RETRIEVECOLUMN { cbData = sizeof(short), columnid = this.coltypDict[JET_coltyp.Short], pvData  = new byte[sizeof(short)] },
                new JET_RETRIEVECOLUMN { cbData = sizeof(double), columnid = this.coltypDict[JET_coltyp.IEEEDouble], pvData = new byte[sizeof(double)] },
                new JET_RETRIEVECOLUMN { cbData = str.Length * sizeof(char) * 2, columnid = this.coltypDict[JET_coltyp.LongText], pvData = new byte[str.Length * sizeof(char) * 2] },
                new JET_RETRIEVECOLUMN { cbData = 10, columnid = this.coltypDict[JET_coltyp.LongBinary], pvData = new byte[10] },
            };

            for (int i = 0; i < retrievecolumns.Length; ++i)
            {
                retrievecolumns[i].itagSequence = 1;    
            }

            Api.JetRetrieveColumns(this.session, this.tableid, retrievecolumns, retrievecolumns.Length);

            // retrievecolumns[0] = short
            Assert.AreEqual(sizeof(short), retrievecolumns[0].cbActual);
            Assert.AreEqual(JET_wrn.Success, retrievecolumns[0].err);
            Assert.AreEqual(s, BitConverter.ToInt16(retrievecolumns[0].pvData, 0));

            // retrievecolumns[1] = double
            Assert.AreEqual(sizeof(double), retrievecolumns[1].cbActual);
            Assert.AreEqual(JET_wrn.Success, retrievecolumns[1].err);
            Assert.AreEqual(d, BitConverter.ToDouble(retrievecolumns[1].pvData, 0));

            // retrievecolumns[2] = string
            Assert.AreEqual(str.Length * sizeof(char), retrievecolumns[2].cbActual);
            Assert.AreEqual(JET_wrn.Success, retrievecolumns[2].err);
            Assert.AreEqual(str, Encoding.Unicode.GetString(retrievecolumns[2].pvData, 0, retrievecolumns[2].cbActual));

            // retrievecolumns[3] = null
            Assert.AreEqual(0, retrievecolumns[3].cbActual);
            Assert.AreEqual(JET_wrn.ColumnNull, retrievecolumns[3].err);
        }

        /// <summary>
        /// Test JetRetrieveColumns with a null buffer.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test JetRetrieveColumns with a null buffer")]
        public void JetRetrieveColumnsNullBuffer()
        {
            byte[] data = Any.Bytes;

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                Api.SetColumn(this.session, this.tableid, this.coltypDict[JET_coltyp.LongBinary], data);
                update.SaveAndGotoBookmark();
                trx.Commit(CommitTransactionGrbit.None);
            }

            var retrievecolumns = new[]
            {
                new JET_RETRIEVECOLUMN { columnid = this.coltypDict[JET_coltyp.LongBinary], itagSequence = 1 },
            };

            Assert.AreEqual(
                JET_wrn.BufferTruncated,
                Api.JetRetrieveColumns(this.session, this.tableid, retrievecolumns, retrievecolumns.Length));

            Assert.AreEqual(data.Length, retrievecolumns[0].cbActual);
            Assert.AreEqual(JET_wrn.BufferTruncated, retrievecolumns[0].err);
        }

        /// <summary>
        /// Test JetSetColumns.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test JetSetColumns")]
        public void SetColumns()
        {
            bool bit = true;
            byte b = Any.Byte;
            short i16 = Any.Int16;
            int i32 = Any.Int32;
            long i64 = Any.Int64;
            float f = Any.Float;
            double d = Any.Double;
            DateTime date = Any.DateTime;
            string s = Any.String;
            byte[] bytes = Any.BytesOfLength(1023);

            var columnValues = new ColumnValue[]
            {
                new BoolColumnValue { Columnid = this.coltypDict[JET_coltyp.Bit], Value = bit },
                new ByteColumnValue { Columnid = this.coltypDict[JET_coltyp.UnsignedByte], Value = b },
                new Int16ColumnValue { Columnid = this.coltypDict[JET_coltyp.Short], Value = i16 },
                new Int32ColumnValue { Columnid = this.coltypDict[JET_coltyp.Long], Value = i32 },
                new Int64ColumnValue { Columnid = this.coltypDict[JET_coltyp.Currency], Value = i64 },
                new FloatColumnValue { Columnid = this.coltypDict[JET_coltyp.IEEESingle], Value = f },
                new DoubleColumnValue { Columnid = this.coltypDict[JET_coltyp.IEEEDouble], Value = d },
                new DateTimeColumnValue { Columnid = this.coltypDict[JET_coltyp.DateTime], Value = date },
                new StringColumnValue { Columnid = this.coltypDict[JET_coltyp.LongText], Value = s },
                new BytesColumnValue { Columnid = this.coltypDict[JET_coltyp.LongBinary], Value = bytes },
            };

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                Api.SetColumns(this.session, this.tableid, columnValues);
                update.Save();
                trx.Commit(CommitTransactionGrbit.None);
            }

            Api.TryMoveFirst(this.session, this.tableid);

            Assert.AreEqual(bit, Api.RetrieveColumnAsBoolean(this.session, this.tableid, this.coltypDict[JET_coltyp.Bit]));
            Assert.AreEqual(b, Api.RetrieveColumnAsByte(this.session, this.tableid, this.coltypDict[JET_coltyp.UnsignedByte]));
            Assert.AreEqual(i16, Api.RetrieveColumnAsInt16(this.session, this.tableid, this.coltypDict[JET_coltyp.Short]));
            Assert.AreEqual(i32, Api.RetrieveColumnAsInt32(this.session, this.tableid, this.coltypDict[JET_coltyp.Long]));
            Assert.AreEqual(i64, Api.RetrieveColumnAsInt64(this.session, this.tableid, this.coltypDict[JET_coltyp.Currency]));
            Assert.AreEqual(f, Api.RetrieveColumnAsFloat(this.session, this.tableid, this.coltypDict[JET_coltyp.IEEESingle]));
            Assert.AreEqual(d, Api.RetrieveColumnAsDouble(this.session, this.tableid, this.coltypDict[JET_coltyp.IEEEDouble]));
            Assert.AreEqual(date, Api.RetrieveColumnAsDateTime(this.session, this.tableid, this.coltypDict[JET_coltyp.DateTime]));
            Assert.AreEqual(s, Api.RetrieveColumnAsString(this.session, this.tableid, this.coltypDict[JET_coltyp.LongText]));
            CollectionAssert.AreEqual(bytes, Api.RetrieveColumn(this.session, this.tableid, this.coltypDict[JET_coltyp.LongBinary]));
        }

        /// <summary>
        /// Test JetSetColumns with null and zero-length data.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Test JetSetColumns with null and zero-length data")]
        public void SetColumnsWithNullAndZeroLength()
        {
            var columnValues = new ColumnValue[]
            {
                new BoolColumnValue { Columnid = this.coltypDict[JET_coltyp.Bit], Value = null },
                new ByteColumnValue { Columnid = this.coltypDict[JET_coltyp.UnsignedByte], Value = null },
                new Int16ColumnValue { Columnid = this.coltypDict[JET_coltyp.Short], Value = null },
                new Int32ColumnValue { Columnid = this.coltypDict[JET_coltyp.Long], Value = null },
                new Int64ColumnValue { Columnid = this.coltypDict[JET_coltyp.Currency], Value = null },
                new FloatColumnValue { Columnid = this.coltypDict[JET_coltyp.IEEESingle], Value = null },
                new DoubleColumnValue { Columnid = this.coltypDict[JET_coltyp.IEEEDouble], Value = null },
                new DateTimeColumnValue { Columnid = this.coltypDict[JET_coltyp.DateTime], Value = null },
                new StringColumnValue { Columnid = this.coltypDict[JET_coltyp.LongText], Value = null },
                new BytesColumnValue { Columnid = this.coltypDict[JET_coltyp.LongBinary], Value = null },
                new StringColumnValue { Columnid = this.coltypDict[JET_coltyp.Text], Value = String.Empty },
                new BytesColumnValue { Columnid = this.coltypDict[JET_coltyp.Binary], Value = new byte[0] },
            };

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                Api.SetColumns(this.session, this.tableid, columnValues);
                update.Save();
                trx.Commit(CommitTransactionGrbit.None);
            }

            Api.TryMoveFirst(this.session, this.tableid);

            Assert.IsNull(Api.RetrieveColumnAsBoolean(this.session, this.tableid, this.coltypDict[JET_coltyp.Bit]));
            Assert.IsNull(Api.RetrieveColumnAsByte(this.session, this.tableid, this.coltypDict[JET_coltyp.UnsignedByte]));
            Assert.IsNull(Api.RetrieveColumnAsInt16(this.session, this.tableid, this.coltypDict[JET_coltyp.Short]));
            Assert.IsNull(Api.RetrieveColumnAsInt32(this.session, this.tableid, this.coltypDict[JET_coltyp.Long]));
            Assert.IsNull(Api.RetrieveColumnAsInt64(this.session, this.tableid, this.coltypDict[JET_coltyp.Currency]));
            Assert.IsNull(Api.RetrieveColumnAsFloat(this.session, this.tableid, this.coltypDict[JET_coltyp.IEEESingle]));
            Assert.IsNull(Api.RetrieveColumnAsDouble(this.session, this.tableid, this.coltypDict[JET_coltyp.IEEEDouble]));
            Assert.IsNull(Api.RetrieveColumnAsDateTime(this.session, this.tableid, this.coltypDict[JET_coltyp.DateTime]));
            Assert.IsNull(Api.RetrieveColumnAsString(this.session, this.tableid, this.coltypDict[JET_coltyp.LongText]));
            Assert.IsNull(Api.RetrieveColumn(this.session, this.tableid, this.coltypDict[JET_coltyp.LongBinary]));
            Assert.AreEqual(String.Empty, Api.RetrieveColumnAsString(this.session, this.tableid, this.coltypDict[JET_coltyp.Text]));
            CollectionAssert.AreEqual(new byte[0], Api.RetrieveColumn(this.session, this.tableid, this.coltypDict[JET_coltyp.Binary]));
        }

        #endregion
    }
}