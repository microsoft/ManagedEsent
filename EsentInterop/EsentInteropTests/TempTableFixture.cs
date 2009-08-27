//-----------------------------------------------------------------------
// <copyright file="TempTableFixture.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Isam.Esent.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
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
        /// Test JetSetColumns
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetColumns()
        {
            bool bit = Any.Boolean;
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

        #region DDL Parameter Checking

        /// <summary>
        /// Check that an exception is thrown when JetAddColumn gets a 
        /// null column name.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetAddColumnThrowsExceptionWhenColumnNameIsNull()
        {
            var columndef = new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Binary,
            };

            JET_COLUMNID columnid;
            Api.JetAddColumn(
                this.session,
                this.tableid,
                null,
                columndef,
                null,
                0,
                out columnid);
        }

        /// <summary>
        /// Check that an exception is thrown when JetAddColumn gets a 
        /// null column definition.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetAddColumnThrowsExceptionWhenColumndefIsNull()
        {
            JET_COLUMNID columnid;
            Api.JetAddColumn(
                this.session,
                this.tableid,
                "column",
                null,
                null,
                0,
                out columnid);
        }

        /// <summary>
        /// Check that an exception is thrown when JetAddColumn gets a 
        /// default value length that is negative.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetAddColumnThrowsExceptionWhenDefaultValueLengthIsNegative()
        {
            var columndef = new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Binary,
            };

            JET_COLUMNID columnid;
            Api.JetAddColumn(
                this.session,
                this.tableid,
                "NegativeDefaultValue",
                columndef,
                null,
                -1,
                out columnid);
        }

        /// <summary>
        /// Check that an exception is thrown when JetAddColumn gets a 
        /// default value length that is too long.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetAddColumnThrowsExceptionWhenDefaultValueLengthIsTooLong()
        {
            var defaultValue = new byte[10];
            var columndef = new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Binary,
            };

            JET_COLUMNID columnid;
            Api.JetAddColumn(
                this.session,
                this.tableid,
                "BadDefaultValue",
                columndef,
                defaultValue,
                defaultValue.Length + 1,
                out columnid);
        }

        /// <summary>
        /// Check that an exception is thrown when JetAddColumn gets a 
        /// default value that is null with a non-zero default value size.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetAddColumnThrowsExceptionWhenDefaultValueIsUnexpectedNull()
        {
            var defaultValue = new byte[10];
            var columndef = new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Binary,
            };

            JET_COLUMNID columnid;
            Api.JetAddColumn(
                this.session,
                this.tableid,
                "BadDefaultValue",
                columndef,
                null,
                1,
                out columnid);
        }

        /// <summary>
        /// Check that an exception is thrown when JetCreateIndex gets a 
        /// null name.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetCreateIndexThrowsExceptionWhenNameIsNull()
        {
            Api.JetCreateIndex(this.session, this.tableid, null, CreateIndexGrbit.None, "+foo\0", 6, 100);
        }

        /// <summary>
        /// Check that an exception is thrown when JetCreateIndex gets a 
        /// density that is negative.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetCreateIndexThrowsExceptionWhenDensityIsNegative()
        {
            Api.JetCreateIndex(this.session, this.tableid, "BadIndex,", CreateIndexGrbit.None, "+foo\0", 6, -1);
        }

        /// <summary>
        /// Check that an exception is thrown when JetCreateIndex gets a 
        /// key description length that is negative.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetCreateIndexThrowsExceptionWhenKeyDescriptionLengthIsNegative()
        {
            Api.JetCreateIndex(this.session, this.tableid, "BadIndex,", CreateIndexGrbit.None, "+foo\0", -1, 100);
        }

        /// <summary>
        /// Check that an exception is thrown when JetCreateIndex gets a 
        /// key description length that is too long.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetCreateIndexThrowsExceptionWhenKeyDescriptionLengthIsTooLong()
        {
            Api.JetCreateIndex(this.session, this.tableid, "BadIndex,", CreateIndexGrbit.None, "+foo\0", 77, 100);
        }

        /// <summary>
        /// Check that an exception is thrown when JetCreateIndex2 gets 
        /// null indexcreates.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetCreateIndex2ThrowsExceptionWhenIndexcreatesAreNull()
        {
            Api.JetCreateIndex2(this.session, this.tableid, null, 0);
        }

        /// <summary>
        /// Check that an exception is thrown when JetCreateIndex2 gets 
        /// a negative indexcreate count.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetCreateIndex2ThrowsExceptionWhenNumIndexcreatesIsNegative()
        {
            var indexcreates = new[] { new JET_INDEXCREATE() };
            Api.JetCreateIndex2(this.session, this.tableid, indexcreates, -1);
        }

        /// <summary>
        /// Check that an exception is thrown when JetCreateIndex2 gets 
        /// an indexcreate count that is too long.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetCreateIndex2ThrowsExceptionWhenNumIndexcreatesIsTooLong()
        {
            var indexcreates = new[] { new JET_INDEXCREATE() };
            Api.JetCreateIndex2(this.session, this.tableid, indexcreates, indexcreates.Length + 1);
        }

        /// <summary>
        /// Check that an exception is thrown when JetCreateIndex2 gets a 
        /// null index name.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetCreateIndex2ThrowsExceptionWhenIndexNameIsNull()
        {
            const string Key = "+column\0";
            var indexcreates = new[]
            {
                new JET_INDEXCREATE
                {
                    cbKey = Key.Length,
                    szKey = Key,
                },
            };
            Api.JetCreateIndex2(this.session, this.tableid, indexcreates, indexcreates.Length);
        }

        /// <summary>
        /// Check that an exception is thrown when JetDeleteColumn gets a 
        /// null column name.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetDeleteColumnThrowsExceptionWhenColumnNameIsNull()
        {
            Api.JetDeleteColumn(this.session, this.tableid, null);
        }

        /// <summary>
        /// Check that an exception is thrown when JetDeleteIndex gets a 
        /// null index name.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetDeleteIndexThrowsExceptionWhenIndexNameIsNull()
        {
            Api.JetDeleteIndex(this.session, this.tableid, null);
        }

        /// <summary>
        /// Check that an exception is thrown when JetGetTableColumnInfo gets a 
        /// null column name.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetGetTableColumnInfoThrowsExceptionWhenColumnNameIsNull()
        {
            JET_COLUMNDEF columndef;
            Api.JetGetTableColumnInfo(this.session, this.tableid, null, out columndef);
        }

        #endregion

        #region Navigation Parameter Checking

        /// <summary>
        /// Check that an exception is thrown when JetGotoBookmark gets a 
        /// null bookmark.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetGotoBookmarkThrowsExceptionWhenBookmarkIsNull()
        {
            Api.JetGotoBookmark(this.session, this.tableid, null, 0);
        }

        /// <summary>
        /// Check that an exception is thrown when JetGotoBookmark gets a 
        /// negative bookmark length.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetGotoBookmarkThrowsExceptionWhenBookmarkLengthIsNegative()
        {
            var bookmark = new byte[1];
            Api.JetGotoBookmark(this.session, this.tableid, bookmark, -1);
        }

        /// <summary>
        /// Check that an exception is thrown when JetGotoBookmark gets a 
        /// bookmark length that is too long.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetGotoBookmarkThrowsExceptionWhenBookmarkLengthIsTooLong()
        {
            var bookmark = new byte[1];
            Api.JetGotoBookmark(this.session, this.tableid, bookmark, bookmark.Length + 1);
        }

        /// <summary>
        /// Check that an exception is thrown when JetMakeKey gets 
        /// null data and a non-null length.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetMakeKeyThrowsExceptionWhenDataIsNull()
        {
            Api.JetMakeKey(this.session, this.tableid, null, 2, MakeKeyGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetMakeKey gets a 
        /// data length that is negative.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetMakeKeyThrowsExceptionWhenDataLengthIsNegative()
        {
            var data = new byte[1];
            Api.JetMakeKey(this.session, this.tableid, data, -1, MakeKeyGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetMakeKey gets a 
        /// data length that is too long.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetMakeKeyThrowsExceptionWhenDataLengthIsTooLong()
        {
            var data = new byte[1];
            Api.JetMakeKey(this.session, this.tableid, data, data.Length + 1, MakeKeyGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetIndexRecordCount gets a 
        /// negative max record count.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetIndexRecordCountThrowsExceptionWhenMaxRecordsIsNegative()
        {
            int numRecords;
            Api.JetIndexRecordCount(this.session, this.tableid, out numRecords, -1);
        }

        /// <summary>
        /// Check that an exception is thrown when passing in NULL as the 
        /// ranges to JetIntersectIndexes.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetIntersectIndexesThrowsExceptionWhenTableidsIsNull()
        {
            JET_RECORDLIST recordlist;
            Api.JetIntersectIndexes(this.session, null, 0, out recordlist, IntersectIndexesGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when intersecting just one index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetIntersectIndexesThrowsExceptionWhenIntersectingOneTableid()
        {
            var ranges = new JET_INDEXRANGE[1];
            ranges[0] = new JET_INDEXRANGE { tableid = this.tableid };

            JET_RECORDLIST recordlist;
            Api.JetIntersectIndexes(this.session, ranges, 1, out recordlist, IntersectIndexesGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when IntersectIndexes gets null
        /// as the tableid argument.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IntersectIndexesThrowsExceptionWhenTableidIsNull()
        {
            Api.IntersectIndexes(this.session, null).ToArray();
        }

        #endregion

        #region Data Retrieval Parameter Checking

        /// <summary>
        /// Check that an exception is thrown when JetGetBookmark gets a 
        /// null bookmark and non-null length.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetGetBookmarkThrowsExceptionWhenBookmarkIsNull()
        {
            int actualSize;
            Api.JetGetBookmark(this.session, this.tableid, null, 10, out actualSize);
        }

        /// <summary>
        /// Check that an exception is thrown when JetGetBookmark gets a 
        /// bookmark length that is negative.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetGetBookmarkThrowsExceptionWhenBookmarkLengthIsNegative()
        {
            int actualSize;
            var bookmark = new byte[1];
            Api.JetGetBookmark(this.session, this.tableid, bookmark, -1, out actualSize);
        }

        /// <summary>
        /// Check that an exception is thrown when JetGetBookmark gets a 
        /// bookmark length that is too long.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetGetBookmarkThrowsExceptionWhenBookmarkLengthIsTooLong()
        {
            int actualSize;
            var bookmark = new byte[1];
            Api.JetGetBookmark(this.session, this.tableid, bookmark, bookmark.Length + 1, out actualSize);
        }

        /// <summary>
        /// Check that an exception is thrown when JetRetrieveKey gets 
        /// null data and a non-null length.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetRetrieveKeyThrowsExceptionWhenDataIsNull()
        {
            int actualSize;
            Api.JetRetrieveKey(this.session, this.tableid, null, 1, out actualSize, RetrieveKeyGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetMakeKey gets a 
        /// data length that is negative.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetRetrieveKeyThrowsExceptionWhenDataLengthIsNegative()
        {
            var data = new byte[1];
            int actualSize;
            Api.JetRetrieveKey(this.session, this.tableid, data, -1, out actualSize, RetrieveKeyGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetMakeKey gets a 
        /// data length that is too long.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetRetrieveKeyThrowsExceptionWhenDataLengthIsTooLong()
        {
            var data = new byte[1];
            int actualSize;
            Api.JetRetrieveKey(this.session, this.tableid, data, data.Length + 1, out actualSize, RetrieveKeyGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetRetrieveColumn gets a 
        /// null buffer and non-null length.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetRetrieveColumnThrowsExceptionWhenDataIsNull()
        {
            int actualSize;
            Api.JetRetrieveColumn(this.session, this.tableid, this.coltypDict[JET_coltyp.UnsignedByte], null, 1, out actualSize, RetrieveColumnGrbit.None, null);
        }

        /// <summary>
        /// Check that an exception is thrown when JetRetrieveColumn gets a 
        /// data length that is negative.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetRetrieveColumnThrowsExceptionWhenDataSizeIsNegative()
        {
            int actualSize;
            var data = new byte[1];
            Api.JetRetrieveColumn(this.session, this.tableid, this.coltypDict[JET_coltyp.UnsignedByte], data, -1, out actualSize, RetrieveColumnGrbit.None, null);
        }

        /// <summary>
        /// Check that an exception is thrown when JetRetrieveColumn gets a 
        /// data length that is too long.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetRetrieveColumnThrowsExceptionWhenDataSizeIsTooLong()
        {
            int actualSize;
            var data = new byte[1];
            Api.JetRetrieveColumn(this.session, this.tableid, this.coltypDict[JET_coltyp.UnsignedByte], data, data.Length + 1, out actualSize, RetrieveColumnGrbit.None, null);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEnumerateColumns gets a 
        /// null allocator callback.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetEnumerateColumnsThrowsExceptionWhenAllocatorIsNull()
        {
            int numColumnValues;
            JET_ENUMCOLUMN[] columnValues;
            Api.JetEnumerateColumns(
                this.session,
                this.tableid,
                0,
                null,
                out numColumnValues,
                out columnValues,
                null,
                IntPtr.Zero,
                0,
                EnumerateColumnsGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEnumerateColumns gets a 
        /// negative maximum column size.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetEnumerateColumnsThrowsExceptionWhenMaxSizeIsNegative()
        {
            int numColumnValues;
            JET_ENUMCOLUMN[] columnValues;
            JET_PFNREALLOC allocator = (context, pv, cb) => IntPtr.Zero;
            Api.JetEnumerateColumns(
                this.session,
                this.tableid,
                0,
                null,
                out numColumnValues,
                out columnValues,
                allocator,
                IntPtr.Zero,
                -1,
                EnumerateColumnsGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEnumerateColumns gets a 
        /// null columnids when numColumnids is non-zero.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetEnumerateColumnsThrowsExceptionWhenColumnidsIsUnexpectedNull()
        {
            int numColumnValues;
            JET_ENUMCOLUMN[] columnValues;
            JET_PFNREALLOC allocator = (context, pv, cb) => IntPtr.Zero;
            Api.JetEnumerateColumns(
                this.session,
                this.tableid,
                1,
                null,
                out numColumnValues,
                out columnValues,
                allocator,
                IntPtr.Zero,
                0,
                EnumerateColumnsGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEnumerateColumns gets a 
        /// negative numColumnids.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetEnumerateColumnsThrowsExceptionWhenNumColumnidsIsNegative()
        {
            int numColumnValues;
            JET_ENUMCOLUMN[] columnValues;
            JET_PFNREALLOC allocator = (context, pv, cb) => IntPtr.Zero;
            var columnids = new JET_ENUMCOLUMNID[2];
            Api.JetEnumerateColumns(
                this.session,
                this.tableid,
                -1,
                columnids,
                out numColumnValues,
                out columnValues,
                allocator,
                IntPtr.Zero,
                0,
                EnumerateColumnsGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEnumerateColumns gets a 
        /// numColumnids count which is greater than the size of columnids.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetEnumerateColumnsThrowsExceptionWhenNumColumnidsIsTooLong()
        {
            int numColumnValues;
            JET_ENUMCOLUMN[] columnValues;
            JET_PFNREALLOC allocator = (context, pv, cb) => IntPtr.Zero;
            var columnids = new JET_ENUMCOLUMNID[2];
            Api.JetEnumerateColumns(
                this.session,
                this.tableid,
                columnids.Length + 1,
                columnids,
                out numColumnValues,
                out columnValues,
                allocator,
                IntPtr.Zero,
                0,
                EnumerateColumnsGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEnumerateColumns gets a 
        /// numColumnids count which is greater than the size of columnids.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetEnumerateColumnsThrowsExceptionWhenNumColumnidRgtagIsInvalid()
        {
            int numColumnValues;
            JET_ENUMCOLUMN[] columnValues;
            JET_PFNREALLOC allocator = (context, pv, cb) => IntPtr.Zero;
            var columnids = new[]
            {
                new JET_ENUMCOLUMNID { columnid = this.coltypDict[JET_coltyp.Currency], ctagSequence = 2, rgtagSequence = new int[1] },
            };
            Api.JetEnumerateColumns(
                this.session,
                this.tableid,
                columnids.Length,
                columnids,
                out numColumnValues,
                out columnValues,
                allocator,
                IntPtr.Zero,
                0,
                EnumerateColumnsGrbit.None);
        }

        #endregion

        #region DML Parameter Checking

        /// <summary>
        /// Check that an exception is thrown when JetSetColumn gets a 
        /// null buffer and non-null length (and SetSizeLV isn't specified).
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetSetColumnThrowsExceptionWhenDataIsNull()
        {
            Api.JetSetColumn(this.session, this.tableid, this.coltypDict[JET_coltyp.UnsignedByte], null, 1, SetColumnGrbit.None, null);
        }

        /// <summary>
        /// Check that an exception is thrown when JetSetColumn gets a 
        /// negative data length.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetSetColumnThrowsExceptionWhenDataSizeIsNegative()
        {
            var data = new byte[1];
            Api.JetSetColumn(this.session, this.tableid, this.coltypDict[JET_coltyp.UnsignedByte], data, -1, SetColumnGrbit.None, null);
        }

        /// <summary>
        /// Check that an exception is thrown when JetSetColumn gets a 
        /// negative data length.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetSetColumnThrowsExceptionWhenDataSizeIsTooLong()
        {
            var data = new byte[1];
            Api.JetSetColumn(this.session, this.tableid, this.coltypDict[JET_coltyp.UnsignedByte], data, data.Length + 1, SetColumnGrbit.None, null);
        }

        /// <summary>
        /// Check that an exception is thrown when JetSetColumns gets a 
        /// null setcolumns array. 
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetSetColumnsThrowsExceptionWhenSetColumnsIsNull()
        {
            JET_SETCOLUMN[] columns = null;
            Api.JetSetColumns(this.session, this.tableid, columns, 0);
        }

        /// <summary>
        /// Check that an exception is thrown when JetSetColumns gets a 
        /// negative number of columns.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetSetColumnsThrowsExceptionWhenNumColumnsIsNegative()
        {
            Api.JetSetColumns(this.session, this.tableid, new JET_SETCOLUMN[1], -1);
        }

        /// <summary>
        /// Check that an exception is thrown when JetSetColumns gets a 
        /// numColumns count that is greater than the number of columns.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetSetColumnsThrowsExceptionWhenDataSizeIsTooLong()
        {
            Api.JetSetColumns(this.session, this.tableid, new JET_SETCOLUMN[1], 2);
        }

        /// <summary>
        /// Check that an exception is thrown when JetSetColumns gets a 
        /// cbData that is greater than the size of the pvData.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetSetColumnsThrowsExceptionWhenSetColumnDataIsInvalid()
        {
            var setcolumns = new[]
            {
                new JET_SETCOLUMN
                {
                    cbData = 100,
                    pvData = new byte[10],
                },
            };
            Api.JetSetColumns(this.session, this.tableid, setcolumns, setcolumns.Length);
        }

        /// <summary>
        /// Check that an exception is thrown when SetColumns gets a 
        /// null column name.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetColumnsThrowsExceptionWhenColumnValuesIsNull()
        {
            Api.SetColumns(this.session, this.tableid, null);
        }

        /// <summary>
        /// Check that an exception is thrown when SetColumns gets a 
        /// zero-length array.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetColumnsThrowsExceptionWhenColumnValuesIsZeroLength()
        {
            Api.SetColumns(this.session, this.tableid, new ColumnValue[0]);
        }

        /// <summary>
        /// Check that an exception is thrown when JetUpdate gets a 
        /// null buffer and non-null length.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetUpdateThrowsExceptionWhenDataIsNull()
        {
            int actualSize;
            Api.JetUpdate(this.session, this.tableid, null, 1, out actualSize);
        }

        /// <summary>
        /// Check that an exception is thrown when JetUpdate gets a 
        /// data length that is negative.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetUpdateThrowsExceptionWhenDataSizeIsNegative()
        {
            int actualSize;
            var data = new byte[1];
            Api.JetUpdate(this.session, this.tableid, data, -1, out actualSize);
        }

        /// <summary>
        /// Check that an exception is thrown when JetUpdate gets a 
        /// data length that is too long.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetUpdateThrowsExceptionWhenDataSizeIsTooLong()
        {
            int actualSize;
            var data = new byte[1];
            Api.JetUpdate(this.session, this.tableid, data, data.Length + 1, out actualSize);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEscrowUpdate gets a 
        /// null delta.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetEscrowUpdateThrowsExceptionWhenDeltaIsNull()
        {
            int actualSize;
            Api.JetEscrowUpdate(this.session, this.tableid, this.coltypDict[JET_coltyp.UnsignedByte], null, 0, null, 0, out actualSize, EscrowUpdateGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEscrowUpdate gets a 
        /// negative delta length.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetEscrowUpdateThrowsExceptionWhenDeltaSizeIsNegative()
        {
            int actualSize;
            var delta = new byte[4];
            Api.JetEscrowUpdate(this.session, this.tableid, this.coltypDict[JET_coltyp.UnsignedByte], delta, -1, null, 0, out actualSize, EscrowUpdateGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEscrowUpdate gets a 
        /// delta length that is too long.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetEscrowUpdateThrowsExceptionWhenDeltaSizeIsTooLong()
        {
            int actualSize;
            var delta = new byte[1];
            Api.JetEscrowUpdate(this.session, this.tableid, this.coltypDict[JET_coltyp.UnsignedByte], delta, delta.Length + 1, null, 0, out actualSize, EscrowUpdateGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEscrowUpdate gets a 
        /// null previous value and non-zero length.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetEscrowUpdateThrowsExceptionWhenPreviousValueIsNull()
        {
            int actualSize;
            var delta = new byte[4];
            Api.JetEscrowUpdate(this.session, this.tableid, this.coltypDict[JET_coltyp.UnsignedByte], delta, delta.Length, null, 4, out actualSize, EscrowUpdateGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEscrowUpdate gets a 
        /// negative previous value length.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetEscrowUpdateThrowsExceptionWhenPreviousValueSizeIsNegative()
        {
            int actualSize;
            var delta = new byte[4];
            var previous = new byte[4];
            Api.JetEscrowUpdate(this.session, this.tableid, this.coltypDict[JET_coltyp.UnsignedByte], delta, delta.Length, previous, -1, out actualSize, EscrowUpdateGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEscrowUpdate gets a 
        /// previous value length that is too long.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetEscrowUpdateThrowsExceptionWhenPreviousValueSizeIsTooLong()
        {
            int actualSize;
            var delta = new byte[4];
            var previous = new byte[4];
            Api.JetEscrowUpdate(this.session, this.tableid, this.coltypDict[JET_coltyp.UnsignedByte], delta, delta.Length, previous, previous.Length + 1, out actualSize, EscrowUpdateGrbit.None);
        }

        #endregion
    }
}