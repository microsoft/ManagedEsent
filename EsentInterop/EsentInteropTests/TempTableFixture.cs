//-----------------------------------------------------------------------
// <copyright file="TempTableFixture.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Isam.Esent;
using Microsoft.Isam.Esent.Interop;
using Microsoft.Isam.Esent.Interop.Vista;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
    /// <summary>
    /// Tests for the various Set/RetrieveColumn* methods and
    /// the helper methods that retrieve meta-data.
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
        /// Test setting and retrieving the min value of a DateTime
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SetAndRetrieveDateTimeMin()
        {
            var columnid = this.coltypDict[JET_coltyp.Binary];
            // The base OLE Automation Date is midnight, 30 December 1899.
            var expected = new DateTime(1899, 12, 31, 23, 59, 59);

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
            var columnid = this.coltypDict[JET_coltyp.Binary];
            // The maximum OLE Automation Date is the same as DateTime.MaxValue, the last moment of 31 December 9999.
            var expected = new DateTime(9999, 12, 31, 23, 59, 59);

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
            int i = Any.Int32;
            string s = Any.String;
            double d = Any.Double;

            var setcolumns = new[]
            {
                new JET_SETCOLUMN { cbData = sizeof(int), columnid = this.coltypDict[JET_coltyp.Long], pvData = BitConverter.GetBytes(i) },
                new JET_SETCOLUMN { cbData = s.Length * sizeof(char), columnid = this.coltypDict[JET_coltyp.LongText], pvData = Encoding.Unicode.GetBytes(s) },
                new JET_SETCOLUMN { cbData = sizeof(double), columnid = this.coltypDict[JET_coltyp.IEEEDouble], pvData = BitConverter.GetBytes(d) },
            };

            using (var trx = new Transaction(this.session))
            using (var update = new Update(this.session, this.tableid, JET_prep.Insert))
            {
                Api.JetSetColumns(this.session, this.tableid, setcolumns, setcolumns.Length);
                update.Save();
                trx.Commit(CommitTransactionGrbit.None);
            }

            Api.TryMoveFirst(this.session, this.tableid);

            Assert.AreEqual(i, Api.RetrieveColumnAsInt32(this.session, this.tableid, this.coltypDict[JET_coltyp.Long]));
            Assert.AreEqual(s, Api.RetrieveColumnAsString(this.session, this.tableid, this.coltypDict[JET_coltyp.LongText]));
            Assert.AreEqual(d, Api.RetrieveColumnAsDouble(this.session, this.tableid, this.coltypDict[JET_coltyp.IEEEDouble]));
        }

        #endregion
    }
}