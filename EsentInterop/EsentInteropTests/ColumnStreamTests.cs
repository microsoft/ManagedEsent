//-----------------------------------------------------------------------
// <copyright file="ColumnStreamTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Cryptography;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the ColumnStream class
    /// </summary>
    [TestClass]
    public class ColumnStreamTests
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

        /// <summary>
        /// Columnid of the LongText column in the table.
        /// </summary>
        private JET_COLUMNID columnidLongText;

        #region Setup/Teardown

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// All DDL should be done in this method.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.directory = SetupHelper.CreateRandomDirectory();
            this.database = Path.Combine(this.directory, "database.edb");
            this.table = "table";
            this.instance = SetupHelper.CreateNewInstance(this.directory);

            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesid, String.Empty, String.Empty);
            Api.JetCreateDatabase(this.sesid, this.database, String.Empty, out this.dbid, CreateDatabaseGrbit.None);
            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateTable(this.sesid, this.dbid, this.table, 0, 100, out this.tableid);

            var columndef = new JET_COLUMNDEF()
            {
                cp = JET_CP.Unicode,
                coltyp = JET_coltyp.LongText,
            };
            Api.JetAddColumn(this.sesid, this.tableid, "TestColumn", columndef, null, 0, out this.columnidLongText);

            Api.JetCloseTable(this.sesid, this.tableid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
            Api.JetOpenTable(this.sesid, this.dbid, this.table, null, 0, OpenTableGrbit.None, out this.tableid);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            Api.JetCloseTable(this.sesid, this.tableid);
            Api.JetEndSession(this.sesid, EndSessionGrbit.None);
            Api.JetTerm(this.instance);
            Directory.Delete(this.directory, true);
        }

        /// <summary>
        /// Verify that the test class has setup the test fixture properly.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyFixtureSetup()
        {
            Assert.IsNotNull(this.table);
            Assert.AreNotEqual(JET_INSTANCE.Nil, this.instance);
            Assert.AreNotEqual(JET_SESID.Nil, this.sesid);
            Assert.AreNotEqual(JET_DBID.Nil, this.dbid);
            Assert.AreNotEqual(JET_TABLEID.Nil, this.tableid);
            Assert.AreNotEqual(JET_COLUMNID.Nil, this.columnidLongText);

            JET_COLUMNDEF columndef;
            Api.JetGetTableColumnInfo(this.sesid, this.tableid, this.columnidLongText, out columndef);
            Assert.AreEqual(JET_coltyp.LongText, columndef.coltyp);
        }

        #endregion Setup/Teardown

        #region ColumnStream Tests

        /// <summary>
        /// Test that a ColumnStream supports reading.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void ColumnStreamSupportsRead()
        {
            using (var t = new Transaction(this.sesid))
            using (var u = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                Assert.IsTrue(stream.CanRead);
            }
        }

        /// <summary>
        /// Test that a ColumnStream supports writing.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void ColumnStreamSupportsWrite()
        {
            using (var t = new Transaction(this.sesid))
            using (var u = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                Assert.IsTrue(stream.CanWrite);
            }
        }

        /// <summary>
        /// Test that a ColumnStream supports seeking.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void ColumnStreamSupportsSeek()
        {
            using (var t = new Transaction(this.sesid))
            using (var u = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                Assert.IsTrue(stream.CanSeek);
            }
        }

        /// <summary>
        /// Test setting the length of a ColumnStream.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetColumnStreamLength()
        {
            var bookmark = new byte[SystemParameters.BookmarkMost];
            int bookmarkSize;

            const long Length = 1345;

            using (var transaction = new Transaction(this.sesid))
            using (var update = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                stream.SetLength(Length);
                Assert.AreEqual(Length, stream.Length);

                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                transaction.Commit(CommitTransactionGrbit.LazyFlush);
            }

            Api.JetGotoBookmark(this.sesid, this.tableid, bookmark, bookmarkSize);
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                Assert.AreEqual(Length, stream.Length);
            }
        }

        /// <summary>
        /// Test setting the position of a ColumnStream.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetColumnStreamPosition()
        {
            var bookmark = new byte[SystemParameters.BookmarkMost];
            int bookmarkSize;

            using (var transaction = new Transaction(this.sesid))
            using (var update = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                stream.Write(Any.BytesOfLength(1024), 0, 1024);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                transaction.Commit(CommitTransactionGrbit.LazyFlush);
            }

            Api.JetGotoBookmark(this.sesid, this.tableid, bookmark, bookmarkSize);
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                stream.Position = 10;
                Assert.AreEqual(10, stream.Position);
            }
        }

        /// <summary>
        /// Test writing to a column stream with a non-zero buffer offset.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void WriteAtNonZeroOffset()
        {
            var bookmark = new byte[SystemParameters.BookmarkMost];
            int bookmarkSize;

            var data = Any.BytesOfLength(1024);
            int offset = data.Length / 2;

            using (var transaction = new Transaction(this.sesid))
            using (var update = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                stream.Write(data, offset, data.Length - offset);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                transaction.Commit(CommitTransactionGrbit.LazyFlush);
            }

            Api.JetGotoBookmark(this.sesid, this.tableid, bookmark, bookmarkSize);
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                var retrieved = new byte[data.Length - offset];
                stream.Read(retrieved, 0, retrieved.Length);
                for (int i = 0; i < retrieved.Length; ++i)
                {
                    Assert.AreEqual(retrieved[i], data[i + offset]);
                }
            }
        }

        /// <summary>
        /// Test reading from a column stream with a non-zero buffer offset.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void ReadAtNonZeroOffset()
        {
            var bookmark = new byte[SystemParameters.BookmarkMost];
            int bookmarkSize;

            var data = Any.BytesOfLength(1024);

            using (var transaction = new Transaction(this.sesid))
            using (var update = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                stream.Write(data, 0, data.Length);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                transaction.Commit(CommitTransactionGrbit.LazyFlush);
            }

            Api.JetGotoBookmark(this.sesid, this.tableid, bookmark, bookmarkSize);
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                var retrieved = new byte[data.Length * 2];
                stream.Read(retrieved, data.Length, data.Length);
                for (int i = data.Length; i < retrieved.Length; ++i)
                {
                    Assert.AreEqual(retrieved[i], data[i - data.Length]);
                }
            }
        }

        /// <summary>
        /// Test writing to a column stream with a non-zero buffer offset.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void OverwriteColumnStream()
        {
            var bookmark = new byte[SystemParameters.BookmarkMost];
            int bookmarkSize;

            var data = Any.BytesOfLength(1024);
            var newData = Any.BytesOfLength(128);
            const int Offset = 10;

            using (var transaction = new Transaction(this.sesid))
            using (var update = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                stream.Seek(Offset, SeekOrigin.Current);
                stream.Write(newData, 0, newData.Length);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                transaction.Commit(CommitTransactionGrbit.LazyFlush);
            }

            Api.JetGotoBookmark(this.sesid, this.tableid, bookmark, bookmarkSize);
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                Assert.AreEqual(data.Length, stream.Length);
                var retrieved = new byte[data.Length];
                var expected = new byte[data.Length];
                Array.Copy(data, 0, expected, 0, data.Length);
                Array.Copy(newData, 0, expected, Offset, newData.Length);
                Assert.AreEqual(retrieved.Length, stream.Read(retrieved, 0, retrieved.Length));
                CollectionAssert.AreEqual(expected, retrieved);
            }
        }

        /// <summary>
        /// Test extending a column stream by writing to an offset before the end
        /// but with a length that goes past the end.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void ExtendingColumnStream()
        {
            var bookmark = new byte[SystemParameters.BookmarkMost];
            int bookmarkSize;

            var data = Any.BytesOfLength(4096);

            using (var transaction = new Transaction(this.sesid))
            using (var update = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                // Write some of the data, rewind a bit and then overwrite the
                // last few bytes and append some more data
                stream.Write(data, 0, data.Length - 10);
                stream.Seek(-10, SeekOrigin.End);
                stream.Write(data, data.Length - 20, 20);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                transaction.Commit(CommitTransactionGrbit.LazyFlush);
            }

            Api.JetGotoBookmark(this.sesid, this.tableid, bookmark, bookmarkSize);
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                Assert.AreEqual(data.Length, stream.Length);
                var retrieved = new byte[data.Length];
                Assert.AreEqual(retrieved.Length, stream.Read(retrieved, 0, retrieved.Length));
                CollectionAssert.AreEqual(data, retrieved);
            }
        }

        /// <summary>
        /// ColumnStream.Read should return the number of bytes read from the stream.
        /// This also tests seeking from the end.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void ReadReturnsNumberOfBytesRead()
        {
            var bookmark = new byte[SystemParameters.BookmarkMost];
            int bookmarkSize;

            var data = Any.BytesOfLength(1024);

            using (var transaction = new Transaction(this.sesid))
            using (var update = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                stream.Write(data, 0, data.Length);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                transaction.Commit(CommitTransactionGrbit.LazyFlush);
            }

            Api.JetGotoBookmark(this.sesid, this.tableid, bookmark, bookmarkSize);
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                var retrieved = new byte[data.Length];
                stream.Seek(-1, SeekOrigin.End);
                Assert.AreEqual(1, stream.Read(retrieved, 0, retrieved.Length));
                Assert.AreEqual(data[data.Length - 1], retrieved[0]);
            }
        }

        /// <summary>
        /// Test shrinking a column stream. There is special code to handle this.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void ShrinkColumnStream()
        {
            var bookmark = new byte[SystemParameters.BookmarkMost];
            int bookmarkSize;

            const int Length = 1345;
            var data = Any.BytesOfLength(Length);

            using (var transaction = new Transaction(this.sesid))
            using (var update = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                stream.Write(data, 0, data.Length);
                stream.Write(data, 0, data.Length);
                Assert.AreEqual(Length * 2, stream.Length);

                stream.SetLength(Length);
                Assert.AreEqual(Length, stream.Length);

                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                transaction.Commit(CommitTransactionGrbit.LazyFlush);
            }

            Api.JetGotoBookmark(this.sesid, this.tableid, bookmark, bookmarkSize);
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                Assert.AreEqual(Length, stream.Length);
                var buffer = new byte[Length];
                stream.Read(buffer, 0, buffer.Length);
                CollectionAssert.AreEqual(data, buffer);
            }
        }

        /// <summary>
        /// Test setting the length of a ColumnStream.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void GrowColumnStreamByWritingPastEnd()
        {
            var bookmark = new byte[SystemParameters.BookmarkMost];
            int bookmarkSize;

            const int Length = 1345;
            const int Position = 1500;
            var data = Any.BytesOfLength(Length);

            using (var transaction = new Transaction(this.sesid))
            using (var update = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                stream.Position = Position;
                stream.Write(data, 0, data.Length);
                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                transaction.Commit(CommitTransactionGrbit.LazyFlush);
            }

            Api.JetGotoBookmark(this.sesid, this.tableid, bookmark, bookmarkSize);
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                Assert.AreEqual(Length + Position, stream.Length);
                var expected = new byte[Length + Position];
                var actual = new byte[Length + Position];
                Array.Copy(data, 0, expected, Position, Length);
                Assert.AreEqual(Length + Position, stream.Read(actual, 0, actual.Length));
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        /// <summary>
        /// Test setting the length of a ColumnStream.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetColumnStreamToZeroLength()
        {
            var bookmark = new byte[SystemParameters.BookmarkMost];
            int bookmarkSize;

            using (var transaction = new Transaction(this.sesid))
            using (var update = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                byte[] data = Any.Bytes;
                stream.Write(data, 0, data.Length);

                stream.SetLength(0);
                Assert.AreEqual(0, stream.Length);

                update.Save(bookmark, bookmark.Length, out bookmarkSize);
                transaction.Commit(CommitTransactionGrbit.LazyFlush);
            }

            Api.JetGotoBookmark(this.sesid, this.tableid, bookmark, bookmarkSize);
            CollectionAssert.AreEqual(new byte[0], Api.RetrieveColumn(this.sesid, this.tableid, this.columnidLongText));
        }

        /// <summary>
        /// Test setting and retrieving a column with the ColumnStream class.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetAndRetrieveColumnStream()
        {
            string s = Any.String;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            using (var writer = new StreamWriter(new ColumnStream(this.sesid, this.tableid, this.columnidLongText)))
            {
                writer.WriteLine(s);
            }

            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            using (var reader = new StreamReader(new ColumnStream(this.sesid, this.tableid, this.columnidLongText)))
            {
                string actual = reader.ReadLine();
                Assert.AreEqual(s, actual);
            }
        }

        /// <summary>
        /// Buffer a ColumnStream
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void BufferColumnStream()
        {
            var data = new byte[1024];

            var memoryStream = new MemoryStream();

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            using (var stream = new BufferedStream(new ColumnStream(this.sesid, this.tableid, this.columnidLongText), SystemParameters.LVChunkSizeMost))
            {
                for (int i = 0; i < 10; ++i)
                {
                    stream.Write(data, 0, data.Length);
                    memoryStream.Write(data, 0, data.Length);
                }
            }

            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            var hasher = new SHA512Managed();
            memoryStream.Position = 0;
            var expected = hasher.ComputeHash(memoryStream);

            using (var stream = new BufferedStream(new ColumnStream(this.sesid, this.tableid, this.columnidLongText), SystemParameters.LVChunkSizeMost))
            {
                var actual = hasher.ComputeHash(stream);
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        /// <summary>
        /// Test that seeking beyond the length of the stream doesn't grow the stream.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SeekingPastEndOfColumnStreamDoesNotGrowStream()
        {
            const int Offset = 1200;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                stream.Seek(Offset, SeekOrigin.Begin);
            }

            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                Assert.AreEqual(0, stream.Length);
            }
        }

        /// <summary>
        /// Test setting and retrieving a column with the ColumnStream class
        /// and multivalues.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void SetAndRetrieveMultiValueColumnStream()
        {
            string[] data = { Any.String, Any.String, Any.String, Any.String, Any.String, Any.String };                                

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            for (int i = 0; i < data.Length; ++i)
            {
                var column = new ColumnStream(this.sesid, this.tableid, this.columnidLongText);
                column.Itag = i + 1;
                using (var writer = new StreamWriter(column))
                {
                    writer.WriteLine(data[i]);
                }
            }

            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            for (int i = 0; i < data.Length; ++i)
            {
                var column = new ColumnStream(this.sesid, this.tableid, this.columnidLongText);
                column.Itag = i + 1;
                using (var reader = new StreamReader(column))
                {
                    string actual = reader.ReadLine();
                    Assert.AreEqual(data[i], actual);
                }
            }
        }

        /// <summary>
        /// Trying to seek to an invalid offset generates an exception.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ColumnStreamThrowsExceptionWhenSeekOffsetIsTooLarge()
        {
            using (var t = new Transaction(this.sesid))
            using (var u = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                stream.Seek(0x800000000, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// Setting the size past the maximum LV size generates an exception.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ColumnStreamSetLengthThrowsExceptionWhenLengthIsTooLong()
        {
            using (var t = new Transaction(this.sesid))
            using (var u = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                stream.SetLength(0x800000000);
            }
        }

        /// <summary>
        /// Setting the size to a negative number generates an exception.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ColumnStreamSetLengthThrowsExceptionWhenLengthIsNegative()
        {
            using (var t = new Transaction(this.sesid))
            using (var u = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                stream.SetLength(-1);
            }
        }

        /// <summary>
        /// Setting the position to a negative number generates an exception.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SettingPositionThrowsExceptionWhenPositionIsNegative()
        {
            using (var t = new Transaction(this.sesid))
            using (var u = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                stream.Position = -1;
            }
        }

        /// <summary>
        /// Setting the position past the maximum LV size generates an exception.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SettingPositionThrowsExceptionWhenPositionIsTooLong()
        {
            using (var t = new Transaction(this.sesid))
            using (var u = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                stream.Position = 0x800000000;
            }
        }

        /// <summary>
        /// Writing throws an exception when the buffer is null
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WriteThrowsExceptionWhenBufferIsNull()
        {
            using (var t = new Transaction(this.sesid))
            using (var u = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                stream.Write(null, 0, 0);
            }
        }

        /// <summary>
        /// Writing throws an exception when the buffer offset is negative
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WriteThrowsExceptionWhenBufferOffsetIsNegative()
        {
            using (var t = new Transaction(this.sesid))
            using (var u = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                var buffer = new byte[10];
                stream.Write(buffer, -1, 1);
            }
        }

        /// <summary>
        /// Writing throws an exception when the buffer offset is past the end of 
        /// the buffer.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WriteThrowsExceptionWhenBufferOffsetIsTooBig()
        {
            using (var t = new Transaction(this.sesid))
            using (var u = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                var buffer = new byte[10];
                stream.Write(buffer, buffer.Length, 1);
            }
        }

        /// <summary>
        /// Writing throws an exception when the number of bytes to write is
        /// negative.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WriteThrowsExceptionWhenNumberOfBytesIsNegative()
        {
            using (var t = new Transaction(this.sesid))
            using (var u = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                var buffer = new byte[10];
                stream.Write(buffer, 0, -1);
            }
        }

        /// <summary>
        /// Writing throws an exception when the number of bytes to write is
        /// too large.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WriteThrowsExceptionWhenNumberOfBytesIsTooLarge()
        {
            using (var t = new Transaction(this.sesid))
            using (var u = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                var buffer = new byte[10];
                stream.Write(buffer, 1, buffer.Length);
            }
        }

        /// <summary>
        /// Test that a ColumnStream can serialize an object.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void ColumnStreamCanSerializeBasicType()
        {
            var expected = Any.Int64;

            using (var t = new Transaction(this.sesid))
            using (var u = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                var serializer = new BinaryFormatter
                {
                    Context = new StreamingContext(StreamingContextStates.Persistence)
                };
                serializer.Serialize(stream, expected);
                u.Save();
                t.Commit(CommitTransactionGrbit.LazyFlush);
            }

            Api.JetMove(this.sesid, this.tableid, JET_Move.First, MoveGrbit.None);
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                var deseriaizer = new BinaryFormatter();
                var actual = (long)deseriaizer.Deserialize(stream);
                Assert.AreEqual(expected, actual);
            }
        }

        /// <summary>
        /// Test that a ColumnStream can serialize an object.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void ColumnStreamCanSerializeObject()
        {
            var expected = new Dictionary<string, long> { { "foo", 1 }, { "bar", 2 }, { "baz", 3 } };

            using (var t = new Transaction(this.sesid))
            using (var u = new Update(this.sesid, this.tableid, JET_prep.Insert))
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                var serializer = new BinaryFormatter();
                serializer.Serialize(stream, expected);
                u.Save();
                t.Commit(CommitTransactionGrbit.LazyFlush);
            }

            Api.JetMove(this.sesid, this.tableid, JET_Move.First, MoveGrbit.None);
            using (var stream = new ColumnStream(this.sesid, this.tableid, this.columnidLongText))
            {
                var deseriaizer = new BinaryFormatter();
                var actual = (Dictionary<string, long>) deseriaizer.Deserialize(stream);
                CollectionAssert.AreEqual(expected, actual);
            }
        }

        #endregion ColumnStream Tests

        #region Helper Methods

        /// <summary>
        /// Update the cursor and goto the returned bookmark.
        /// </summary>
        private void UpdateAndGotoBookmark()
        {
            var bookmark = new byte[SystemParameters.BookmarkMost];
            int bookmarkSize;
            Api.JetUpdate(this.sesid, this.tableid, bookmark, bookmark.Length, out bookmarkSize);
            Api.JetGotoBookmark(this.sesid, this.tableid, bookmark, bookmarkSize);
        }

        #endregion HelperMethods
    }
}
