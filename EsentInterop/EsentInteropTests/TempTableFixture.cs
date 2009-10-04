//-----------------------------------------------------------------------
// <copyright file="TempTableFixture.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Isam.Esent.Interop;
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
        public void Teardown()
        {
            this.instance.Term();
        }

        #endregion Setup/Teardown

        #region Set and Retrieve Columns

        /// <summary>
        /// Test setting and retrieving the min value of an Byte
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetAndRetrieveByteMin()
        {
            var columnid = this.coltypDict[JET_coltyp.UnsignedByte];
            const byte Expected = Byte.MinValue;

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                int bookmarkSize;
                var bookmark = new byte[SystemParameters.BookmarkMost];
                Api.SetColumn(this.session, this.tableid, columnid, Expected);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                trx.Commit(CommitTransactionGrbit.None);
                Api.JetGotoBookmark(this.session, this.tableid, bookmark, bookmarkSize);
            }

            Assert.AreEqual(Expected, Api.RetrieveColumnAsByte(this.session, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting and retrieving the max value of an Byte
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetAndRetrieveByteMax()
        {
            var columnid = this.coltypDict[JET_coltyp.UnsignedByte];
            const byte Expected = Byte.MaxValue;

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                int bookmarkSize;
                var bookmark = new byte[SystemParameters.BookmarkMost];
                Api.SetColumn(this.session, this.tableid, columnid, Expected);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                trx.Commit(CommitTransactionGrbit.None);
                Api.JetGotoBookmark(this.session, this.tableid, bookmark, bookmarkSize);
            }

            Assert.AreEqual(Expected, Api.RetrieveColumnAsByte(this.session, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting and retrieving the min value of an Int16
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetAndRetrieveInt16Min()
        {
            var columnid = this.coltypDict[JET_coltyp.Short];
            const short Expected = Int16.MinValue;

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                int bookmarkSize;
                var bookmark = new byte[SystemParameters.BookmarkMost];
                Api.SetColumn(this.session, this.tableid, columnid, Expected);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                trx.Commit(CommitTransactionGrbit.None);
                Api.JetGotoBookmark(this.session, this.tableid, bookmark, bookmarkSize);
            }

            Assert.AreEqual(Expected, Api.RetrieveColumnAsInt16(this.session, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting and retrieving the max value of an Int16
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetAndRetrieveInt16Max()
        {
            var columnid = this.coltypDict[JET_coltyp.Short];
            const short Expected = Int16.MaxValue;

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                int bookmarkSize;
                var bookmark = new byte[SystemParameters.BookmarkMost];
                Api.SetColumn(this.session, this.tableid, columnid, Expected);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                trx.Commit(CommitTransactionGrbit.None);
                Api.JetGotoBookmark(this.session, this.tableid, bookmark, bookmarkSize);
            }

            Assert.AreEqual(Expected, Api.RetrieveColumnAsInt16(this.session, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting and retrieving the min value of a UInt16
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetAndRetrieveUInt16Min()
        {
            var columnid = this.coltypDict[JET_coltyp.Binary];
            const ushort Expected = UInt16.MinValue;

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                int bookmarkSize;
                var bookmark = new byte[SystemParameters.BookmarkMost];
                Api.SetColumn(this.session, this.tableid, columnid, Expected);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                trx.Commit(CommitTransactionGrbit.None);
                Api.JetGotoBookmark(this.session, this.tableid, bookmark, bookmarkSize);
            }

            Assert.AreEqual(Expected, Api.RetrieveColumnAsUInt16(this.session, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting and retrieving the max value of a UInt16
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetAndRetrieveUInt16Max()
        {
            var columnid = this.coltypDict[JET_coltyp.Binary];
            const ushort Expected = UInt16.MaxValue;

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                int bookmarkSize;
                var bookmark = new byte[SystemParameters.BookmarkMost];
                Api.SetColumn(this.session, this.tableid, columnid, Expected);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                trx.Commit(CommitTransactionGrbit.None);
                Api.JetGotoBookmark(this.session, this.tableid, bookmark, bookmarkSize);
            }

            Assert.AreEqual(Expected, Api.RetrieveColumnAsUInt16(this.session, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting and retrieving the min value of an Int32
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetAndRetrieveInt32Min()
        {
            var columnid = this.coltypDict[JET_coltyp.Long];
            const int Expected = Int32.MinValue;

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                int bookmarkSize;
                var bookmark = new byte[SystemParameters.BookmarkMost];
                Api.SetColumn(this.session, this.tableid, columnid, Expected);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                trx.Commit(CommitTransactionGrbit.None);
                Api.JetGotoBookmark(this.session, this.tableid, bookmark, bookmarkSize);
            }

            Assert.AreEqual(Expected, Api.RetrieveColumnAsInt32(this.session, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting and retrieving the max value of an Int32
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetAndRetrieveInt32Max()
        {
            var columnid = this.coltypDict[JET_coltyp.Long];
            const int Expected = Int32.MaxValue;

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                int bookmarkSize;
                var bookmark = new byte[SystemParameters.BookmarkMost];
                Api.SetColumn(this.session, this.tableid, columnid, Expected);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                trx.Commit(CommitTransactionGrbit.None);
                Api.JetGotoBookmark(this.session, this.tableid, bookmark, bookmarkSize);
            }

            Assert.AreEqual(Expected, Api.RetrieveColumnAsInt32(this.session, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting and retrieving the min value of a UInt32
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetAndRetrieveUInt32Min()
        {
            var columnid = this.coltypDict[JET_coltyp.Binary];
            const uint Expected = UInt32.MinValue;

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                int bookmarkSize;
                var bookmark = new byte[SystemParameters.BookmarkMost];
                Api.SetColumn(this.session, this.tableid, columnid, Expected);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                trx.Commit(CommitTransactionGrbit.None);
                Api.JetGotoBookmark(this.session, this.tableid, bookmark, bookmarkSize);
            }

            Assert.AreEqual(Expected, Api.RetrieveColumnAsUInt32(this.session, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting and retrieving the max value of a UInt32
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetAndRetrieveUInt32Max()
        {
            var columnid = this.coltypDict[JET_coltyp.Binary];
            const uint Expected = UInt32.MaxValue;

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                int bookmarkSize;
                var bookmark = new byte[SystemParameters.BookmarkMost];
                Api.SetColumn(this.session, this.tableid, columnid, Expected);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                trx.Commit(CommitTransactionGrbit.None);
                Api.JetGotoBookmark(this.session, this.tableid, bookmark, bookmarkSize);
            }

            Assert.AreEqual(Expected, Api.RetrieveColumnAsUInt32(this.session, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting and retrieving the min value of an Int64
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetAndRetrieveInt64Min()
        {
            var columnid = this.coltypDict[JET_coltyp.Currency];
            const long Expected = Int64.MinValue;

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                int bookmarkSize;
                var bookmark = new byte[SystemParameters.BookmarkMost];
                Api.SetColumn(this.session, this.tableid, columnid, Expected);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                trx.Commit(CommitTransactionGrbit.None);
                Api.JetGotoBookmark(this.session, this.tableid, bookmark, bookmarkSize);
            }

            Assert.AreEqual(Expected, Api.RetrieveColumnAsInt64(this.session, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting and retrieving the max value of an Int64
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetAndRetrieveInt64Max()
        {
            var columnid = this.coltypDict[JET_coltyp.Currency];
            const long Expected = Int64.MaxValue;

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                int bookmarkSize;
                var bookmark = new byte[SystemParameters.BookmarkMost];
                Api.SetColumn(this.session, this.tableid, columnid, Expected);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                trx.Commit(CommitTransactionGrbit.None);
                Api.JetGotoBookmark(this.session, this.tableid, bookmark, bookmarkSize);
            }

            Assert.AreEqual(Expected, Api.RetrieveColumnAsInt64(this.session, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting and retrieving the min value of a UInt64
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetAndRetrieveUInt64Min()
        {
            var columnid = this.coltypDict[JET_coltyp.Binary];
            const ulong Expected = UInt64.MinValue;

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                int bookmarkSize;
                var bookmark = new byte[SystemParameters.BookmarkMost];
                Api.SetColumn(this.session, this.tableid, columnid, Expected);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                trx.Commit(CommitTransactionGrbit.None);
                Api.JetGotoBookmark(this.session, this.tableid, bookmark, bookmarkSize);
            }

            Assert.AreEqual(Expected, Api.RetrieveColumnAsUInt64(this.session, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting and retrieving the max value of a UInt64
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetAndRetrieveUInt64Max()
        {
            var columnid = this.coltypDict[JET_coltyp.Binary];
            const ulong Expected = UInt64.MaxValue;

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                int bookmarkSize;
                var bookmark = new byte[SystemParameters.BookmarkMost];
                Api.SetColumn(this.session, this.tableid, columnid, Expected);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                trx.Commit(CommitTransactionGrbit.None);
                Api.JetGotoBookmark(this.session, this.tableid, bookmark, bookmarkSize);
            }

            Assert.AreEqual(Expected, Api.RetrieveColumnAsUInt64(this.session, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting and retrieving the min value of an Float
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetAndRetrieveFloatMin()
        {
            var columnid = this.coltypDict[JET_coltyp.IEEESingle];
            const float Expected = Single.MinValue;

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                int bookmarkSize;
                var bookmark = new byte[SystemParameters.BookmarkMost];
                Api.SetColumn(this.session, this.tableid, columnid, Expected);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                trx.Commit(CommitTransactionGrbit.None);
                Api.JetGotoBookmark(this.session, this.tableid, bookmark, bookmarkSize);
            }

            Assert.AreEqual(Expected, Api.RetrieveColumnAsFloat(this.session, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting and retrieving the max value of an Float
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetAndRetrieveFloatMax()
        {
            var columnid = this.coltypDict[JET_coltyp.IEEESingle];
            const float Expected = Single.MaxValue;

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                int bookmarkSize;
                var bookmark = new byte[SystemParameters.BookmarkMost];
                Api.SetColumn(this.session, this.tableid, columnid, Expected);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                trx.Commit(CommitTransactionGrbit.None);
                Api.JetGotoBookmark(this.session, this.tableid, bookmark, bookmarkSize);
            }

            Assert.AreEqual(Expected, Api.RetrieveColumnAsFloat(this.session, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting and retrieving the min value of an Double
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetAndRetrieveDoubleMin()
        {
            var columnid = this.coltypDict[JET_coltyp.IEEEDouble];
            const double Expected = Double.MinValue;

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                int bookmarkSize;
                var bookmark = new byte[SystemParameters.BookmarkMost];
                Api.SetColumn(this.session, this.tableid, columnid, Expected);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                trx.Commit(CommitTransactionGrbit.None);
                Api.JetGotoBookmark(this.session, this.tableid, bookmark, bookmarkSize);
            }

            Assert.AreEqual(Expected, Api.RetrieveColumnAsDouble(this.session, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting and retrieving the max value of an Double
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetAndRetrieveDoubleMax()
        {
            var columnid = this.coltypDict[JET_coltyp.IEEEDouble];
            const double Expected = Double.MaxValue;

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                int bookmarkSize;
                var bookmark = new byte[SystemParameters.BookmarkMost];
                Api.SetColumn(this.session, this.tableid, columnid, Expected);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                trx.Commit(CommitTransactionGrbit.None);
                Api.JetGotoBookmark(this.session, this.tableid, bookmark, bookmarkSize);
            }

            Assert.AreEqual(Expected, Api.RetrieveColumnAsDouble(this.session, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting DateTime.Min. Dates before Jan 1, year 100 can't be represented
        /// in ESENT (which uses the OLE Automation Date format), so this generates an
        /// exceptions.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(OverflowException))]
        public void SetDateTimeThrowsExceptionWhenDateIsTooSmall()
        {
            // Can't represent Dec 31, 99
            var invalid = new DateTime(99, 12, 31, 23, 59, 59);
            var columnid = this.coltypDict[JET_coltyp.Binary];

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                Api.SetColumn(this.session, this.tableid, columnid, invalid);
            }
        }

        /// <summary>
        /// This is a bit bizarre. DateTime.MinValue.ToOADate gives the OLE Automation
        /// base date, not the smallest possible OLE Automation Date or an overflow
        /// exception (either of which would seem to be more sensible).
        /// This test is documentating, but not endorsing, this behaviour.
        /// The MSDN documentation says that "An uninitialized DateTime, that is,
        /// an instance with a tick value of 0, is converted to the equivalent
        /// uninitialized OLE Automation Date, that is, a date with a value of
        /// 0.0 which represents midnight, 30 December 1899.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetDateTimeMinGivesOleAutomationBaseTime()
        {
            // The base OLE Automation Date is midnight, 30 December 1899
            var expected = new DateTime(1899, 12, 30, 0, 0, 0);

            var columnid = this.coltypDict[JET_coltyp.Binary];

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                int bookmarkSize;
                var bookmark = new byte[SystemParameters.BookmarkMost];
                Api.SetColumn(this.session, this.tableid, columnid, DateTime.MinValue);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                trx.Commit(CommitTransactionGrbit.None);
                Api.JetGotoBookmark(this.session, this.tableid, bookmark, bookmarkSize);
            }

            Assert.AreEqual(expected, Api.RetrieveColumnAsDateTime(this.session, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting and retrieving the min value of a DateTime
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetAndRetrieveDateTimeOleAutomationMin()
        {
            // The earliest OLE Automation Date is Jan 1, year 100
            var expected = new DateTime(100, 1, 1, 00, 00, 1);

            var columnid = this.coltypDict[JET_coltyp.Binary];

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                int bookmarkSize;
                var bookmark = new byte[SystemParameters.BookmarkMost];
                Api.SetColumn(this.session, this.tableid, columnid, expected);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                trx.Commit(CommitTransactionGrbit.None);
                Api.JetGotoBookmark(this.session, this.tableid, bookmark, bookmarkSize);
            }

            Assert.AreEqual(expected, Api.RetrieveColumnAsDateTime(this.session, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting and retrieving the max value of a DateTime
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetAndRetrieveDateTimeMax()
        {
            // MSDN says the maximum OLE Automation Date is the same as
            // DateTime.MaxValue, the last moment of 31 December 9999.
            var expected = new DateTime(9999, 12, 31, 23, 59, 59);

            var columnid = this.coltypDict[JET_coltyp.Binary];

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                int bookmarkSize;
                var bookmark = new byte[SystemParameters.BookmarkMost];
                Api.SetColumn(this.session, this.tableid, columnid, expected);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                trx.Commit(CommitTransactionGrbit.None);
                Api.JetGotoBookmark(this.session, this.tableid, bookmark, bookmarkSize);
            }

            Assert.AreEqual(expected, Api.RetrieveColumnAsDateTime(this.session, this.tableid, columnid));
        }

        /// <summary>
        /// Serialize and deserialize null.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SerializeAndDeserializeNull()
        {
            var columnid = this.coltypDict[JET_coltyp.Binary];

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                Api.SerializeObjectToColumn(this.session, this.tableid, columnid, null);
                update.SaveAndGotoBookmark();
                trx.Commit(CommitTransactionGrbit.None);
            }

            Assert.IsNull(Api.DeserializeObjectFromColumn(this.session, this.tableid, columnid));
        }

        /// <summary>
        /// Serialize and deserialize and object.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SerializeAndDeserializeObject()
        {
            var columnid = this.coltypDict[JET_coltyp.LongBinary];
            var expected = new List<double> { Math.PI, Math.E, Double.PositiveInfinity, Double.NegativeInfinity, Double.Epsilon };

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                Api.SerializeObjectToColumn(this.session, this.tableid, columnid, expected);
                update.SaveAndGotoBookmark();
                trx.Commit(CommitTransactionGrbit.None);
            }

            var actual = Api.DeserializeObjectFromColumn(this.session, this.tableid, columnid) as List<double>;
            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test JetSetColumns
        /// </summary>
        [TestMethod]
        [Priority(1)]
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
        /// Test JetRetrieveColumns
        /// </summary>
        [TestMethod]
        [Priority(1)]
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
        /// Test JetRetrieveColumns with a null buffer
        /// </summary>
        [TestMethod]
        [Priority(1)]
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
        /// Test JetSetColumns
        /// </summary>
        [TestMethod]
        [Priority(1)]
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
        /// Test JetSetColumns
        /// </summary>
        [TestMethod]
        [Priority(1)]
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