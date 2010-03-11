//-----------------------------------------------------------------------
// <copyright file="SimplePerfTest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Basic performance tests
    /// </summary>
    [TestClass]
    public class SimplePerfTest
    {
        /// <summary>
        /// Size of the data being inserted in the data column.
        /// </summary>
        private const int DataSize = 32;

        /// <summary>
        /// The directory to put the database files in.
        /// </summary>
        private string directory;

        /// <summary>
        /// The instance to use.
        /// </summary>
        private Instance instance;
        
        /// <summary>
        /// The session to use.
        /// </summary>
        private Session session;
        
        /// <summary>
        /// The table to use.
        /// </summary>
        private Table table;

        /// <summary>
        /// The columnid of the key column.
        /// </summary>
        private JET_COLUMNID columnidKey;
        
        /// <summary>
        /// The columnid of the data column.
        /// </summary>
        private JET_COLUMNID columnidData;

        /// <summary>
        /// The next key value to be inserted. Used to insert records.
        /// </summary>
        private long nextKey = 0;
        
        /// <summary>
        /// Data to be inserted into the data column.
        /// </summary>
        private byte[] data;

        /// <summary>
        /// Used to retrieve the data column.
        /// </summary>
        private byte[] dataBuf;

        /// <summary>
        /// Random number generation object.
        /// </summary>
        private Random random;
        
        /// <summary>
        /// Previous minimum cache size. Used to restore the previous setting.
        /// </summary>
        private int cacheSizeMinSaved = 0;

        /// <summary>
        /// Setup for a test -- this creates the database
        /// </summary>
        [TestInitialize]
        [Description("Setup the SimplePerfTest fixture")]
        public void Setup()
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            this.directory = SetupHelper.CreateRandomDirectory();

            this.random = new Random();
            this.data = Any.BytesOfLength(DataSize);
            this.dataBuf = new byte[DataSize];

            JET_DBID dbid;

            string ignored;
            Api.JetGetSystemParameter(
                JET_INSTANCE.Nil, JET_SESID.Nil, JET_param.CacheSizeMin, ref this.cacheSizeMinSaved, out ignored, 0);
            Api.JetSetSystemParameter(JET_INSTANCE.Nil, JET_SESID.Nil, JET_param.CacheSizeMin, 16384, null);

            this.instance = new Instance(Guid.NewGuid().ToString(), "SimplePerfTest");
            this.instance.Parameters.LogFileDirectory = this.directory;
            this.instance.Parameters.SystemDirectory = this.directory;
            this.instance.Parameters.MaxVerPages = 1024;
            this.instance.Parameters.Recovery = false;

            // Create the instance, database and table
            this.instance.Init();
            this.session = new Session(this.instance);
            Api.JetCreateDatabase(this.session, Path.Combine(this.directory, "esentperftest.db"), string.Empty, out dbid, CreateDatabaseGrbit.None);

            // Create a dummy table to force the database to grow
            using (var trx = new Transaction(this.session))
            {
                JET_TABLEID tableid;
                Api.JetCreateTable(this.session, dbid, "dummy_table", 64 * 1024 * 1024 / SystemParameters.DatabasePageSize, 100, out tableid);
                Api.JetCloseTable(this.session, tableid);
                Api.JetDeleteTable(this.session, dbid, "dummy_table");
                trx.Commit(CommitTransactionGrbit.LazyFlush);
            }

            // Create the table
            using (var trx = new Transaction(this.session))
            {
                JET_TABLEID tableid;
                var columndef = new JET_COLUMNDEF();

                Api.JetCreateTable(this.session, dbid, "table", 0, 100, out tableid);
                columndef.coltyp = JET_coltyp.Currency;
                Api.JetAddColumn(this.session, tableid, "Key", columndef, null, 0, out this.columnidKey);
                columndef.coltyp = JET_coltyp.Binary;
                Api.JetAddColumn(this.session, tableid, "Data", columndef, null, 0, out this.columnidData);
                Api.JetCreateIndex(this.session, tableid, "primary", CreateIndexGrbit.IndexPrimary, "+key\0\0", 6, 100);
                Api.JetCloseTable(this.session, tableid);
                trx.Commit(CommitTransactionGrbit.None);
            }

            this.table = new Table(this.session, dbid, "table", OpenTableGrbit.None);
        }

        /// <summary>
        /// Cleanup after the test
        /// </summary>
        [TestCleanup]
        [Description("Cleanup the SimplePerfTest fixture")]
        public void Teardown()
        {
            this.table.Close();
            this.session.End();
            this.instance.Term();
            Api.JetSetSystemParameter(JET_INSTANCE.Nil, JET_SESID.Nil, JET_param.CacheSizeMin, this.cacheSizeMinSaved, null);
            Cleanup.DeleteDirectoryWithRetry(this.directory);
            Thread.CurrentThread.Priority = ThreadPriority.Normal;
        }

        /// <summary>
        /// Test inserting and retrieving records.
        /// </summary>
        [TestMethod]
        [Priority(4)]
        [Description("Run a basic performance test")]
        public void BasicPerfTest()
        {
            this.CheckMemoryUsage(this.InsertReadSeek);
        }

        /// <summary>
        /// Perform and time the given action.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        /// <param name="action">The operation to perform.</param>
        private static void TimeAction(string name, Action action)
        {
            var stopwatch = EsentStopwatch.StartNew();
            action();
            stopwatch.Stop();
            Console.WriteLine("{0}: {1} ({2})", name, stopwatch.Elapsed, stopwatch.ThreadStats);
        }

        /// <summary>
        /// Insert come records and then retrieve them.
        /// </summary>
        private void InsertReadSeek()
        {
            const int NumRecords = 1000000;

            // Randomly seek to all records in the table
            long[] keys = (from x in Enumerable.Range(0, NumRecords) select (long)x).ToArray();
            this.Shuffle(keys);

            TimeAction("Insert records", () => this.InsertRecords(NumRecords / 2));
            TimeAction("Insert records with SetColumns", () => this.InsertRecordsWithSetColumns(NumRecords / 2));
            TimeAction("Read one record", () => this.RepeatedlyRetrieveOneRecord(NumRecords));
            TimeAction("Read one record with JetRetrieveColumns", () => this.RepeatedlyRetrieveOneRecordWithJetRetrieveColumns(NumRecords));
            TimeAction("Read one record with RetrieveColumns", () => this.RepeatedlyRetrieveOneRecordWithRetrieveColumns(NumRecords));
            TimeAction("Read one record with JetEnumerateColumns", () => this.RepeatedlyRetrieveOneRecordWithEnumColumns(NumRecords));
            TimeAction("Read all records", this.RetrieveAllRecords);
            TimeAction("Seek to all records", () => this.SeekToAllRecords(keys));
        }

        /// <summary>
        /// Perform an action, checking the system's memory usage before and after.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        private void CheckMemoryUsage(Action action)
        {
            this.RunGarbageCollection();
            long memoryAtStart = GC.GetTotalMemory(true);
            int collectionCountAtStart = GC.CollectionCount(0);

            action();

            int collectionCountAtEnd = GC.CollectionCount(0);
            this.RunGarbageCollection();
            long memoryAtEnd = GC.GetTotalMemory(true);
            Console.WriteLine(
                "Memory changed by {0} bytes ({1} GC cycles)",
                memoryAtEnd - memoryAtStart,
                collectionCountAtEnd - collectionCountAtStart);
        }

        /// <summary>
        /// Randomly shuffle an array.
        /// </summary>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <param name="arrayToShuffle">The array to shuffle.</param>
        private void Shuffle<T>(T[] arrayToShuffle)
        {
            for (int i = 0; i < arrayToShuffle.Length; ++i)
            {
                int swap = this.random.Next(i, arrayToShuffle.Length);
                T temp = arrayToShuffle[i];
                arrayToShuffle[i] = arrayToShuffle[swap];
                arrayToShuffle[swap] = temp;
            }
        }

        /// <summary>
        /// Insert a record. The key will be <see cref="nextKey"/>.
        /// </summary>
        private void InsertRecord()
        {
            long key = this.nextKey++;
            Api.JetPrepareUpdate(this.session, this.table, JET_prep.Insert);
            Api.SetColumn(this.session, this.table, this.columnidKey, key);
            Api.SetColumn(this.session, this.table, this.columnidData, this.data);
            Api.JetUpdate(this.session, this.table);
        }

        /// <summary>
        /// Insert multiple records.
        /// </summary>
        /// <param name="numRecords">The number of records to insert.</param>
        private void InsertRecords(int numRecords)
        {
            for (int i = 0; i < numRecords; ++i)
            {
                Api.JetBeginTransaction(this.session);
                this.InsertRecord();
                Api.JetCommitTransaction(this.session, CommitTransactionGrbit.LazyFlush);
            }
        }

        /// <summary>
        /// Insert multiple records with the <see cref="Api.SetColumns"/> API.
        /// </summary>
        /// <param name="numRecords">The number of records to insert.</param>
        private void InsertRecordsWithSetColumns(int numRecords)
        {
            var keyColumn = new Int64ColumnValue { Columnid = this.columnidKey };
            var dataColumn = new BytesColumnValue { Columnid = this.columnidData, Value = this.data };

            var columns = new ColumnValue[] { keyColumn, dataColumn };

            for (int i = 0; i < numRecords; ++i)
            {
                Api.JetBeginTransaction(this.session);
                Api.JetPrepareUpdate(this.session, this.table, JET_prep.Insert);
                keyColumn.Value = this.nextKey++;
                Api.SetColumns(this.session, this.table, columns);
                Api.JetUpdate(this.session, this.table);
                Api.JetCommitTransaction(this.session, CommitTransactionGrbit.LazyFlush);
            }
        }

        /// <summary>
        /// Retrieve the current record.
        /// </summary>
        private void RetrieveRecord()
        {
            int actualSize;
            Api.RetrieveColumnAsInt64(this.session, this.table, this.columnidKey);
            Api.JetRetrieveColumn(
                this.session,
                this.table,
                this.columnidData,
                this.dataBuf,
                this.dataBuf.Length,
                out actualSize,
                RetrieveColumnGrbit.None,
                null);
        }

        /// <summary>
        /// Retrieve all records in the table.
        /// </summary>
        private void RetrieveAllRecords()
        {
            Api.MoveBeforeFirst(this.session, this.table);
            while (Api.TryMoveNext(this.session, this.table))
            {
                Api.JetBeginTransaction(this.session);
                this.RetrieveRecord();
                Api.JetCommitTransaction(this.session, CommitTransactionGrbit.LazyFlush);
            }
        }

        /// <summary>
        /// Retrieve the current record multiple times.
        /// </summary>
        /// <param name="numRetrieves">The number of times to retrieve the record.</param>
        private void RepeatedlyRetrieveOneRecord(int numRetrieves)
        {
            Api.JetMove(this.session, this.table, JET_Move.First, MoveGrbit.None);
            for (int i = 0; i < numRetrieves; ++i)
            {
                Api.JetBeginTransaction(this.session);
                this.RetrieveRecord();
                Api.JetCommitTransaction(this.session, CommitTransactionGrbit.None);
            }
        }

        /// <summary>
        /// Repeatedly retrieve one record using <see cref="Api.JetRetrieveColumns"/>.
        /// </summary>
        /// <param name="numRetrieves">The number of times to retrieve the record.</param>
        private void RepeatedlyRetrieveOneRecordWithJetRetrieveColumns(int numRetrieves)
        {
            Api.JetMove(this.session, this.table, JET_Move.First, MoveGrbit.None);

            var keyBuffer = new byte[sizeof(long)];
            var retcols = new[]
            {
                new JET_RETRIEVECOLUMN
                {
                    columnid = this.columnidKey,
                    pvData = keyBuffer,
                    cbData = keyBuffer.Length,
                    itagSequence = 1,
                },
                new JET_RETRIEVECOLUMN
                {
                    columnid = this.columnidData,
                    pvData = this.dataBuf,
                    cbData = this.dataBuf.Length,
                    itagSequence = 1,
                },
            };

            for (int i = 0; i < numRetrieves; ++i)
            {
                Api.JetBeginTransaction(this.session);
                Api.JetRetrieveColumns(this.session, this.table, retcols, retcols.Length);
                Assert.AreEqual(0, BitConverter.ToInt64(keyBuffer, 0));
                Api.JetCommitTransaction(this.session, CommitTransactionGrbit.None);
            }
        }

        /// <summary>
        /// Repeatedly retrieve one record using <see cref="Api.RetrieveColumns"/>.
        /// </summary>
        /// <param name="numRetrieves">The number of times to retrieve the record.</param>
        private void RepeatedlyRetrieveOneRecordWithRetrieveColumns(int numRetrieves)
        {
            Api.JetMove(this.session, this.table, JET_Move.First, MoveGrbit.None);

            var columnValues = new ColumnValue[]
            {
                new Int64ColumnValue { Columnid = this.columnidKey },
                new BytesColumnValue { Columnid = this.columnidData },
            };

            for (int i = 0; i < numRetrieves; ++i)
            {
                Api.JetBeginTransaction(this.session);
                Api.RetrieveColumns(this.session, this.table, columnValues);
                Api.JetCommitTransaction(this.session, CommitTransactionGrbit.None);
            }
        }

        /// <summary>
        /// Repeatedly retrieve one record using <see cref="Api.JetEnumerateColumns"/>.
        /// </summary>
        /// <param name="numRetrieves">The number of times to retrieve the record.</param>
        private void RepeatedlyRetrieveOneRecordWithEnumColumns(int numRetrieves)
        {
            Api.JetMove(this.session, this.table, JET_Move.First, MoveGrbit.None);
            var columnids = new[]
            {
                new JET_ENUMCOLUMNID { columnid = this.columnidKey },
                new JET_ENUMCOLUMNID { columnid = this.columnidData },
            };
            JET_PFNREALLOC allocator = (context, pv, cb) => IntPtr.Zero == pv ? Marshal.AllocHGlobal(new IntPtr(cb)) : Marshal.ReAllocHGlobal(pv, new IntPtr(cb));

            for (int i = 0; i < numRetrieves; ++i)
            {
                Api.JetBeginTransaction(this.session);
                int numValues;
                JET_ENUMCOLUMN[] values;
                Api.JetEnumerateColumns(
                    this.session,
                    this.table,
                    columnids.Length,
                    columnids,
                    out numValues,
                    out values,
                    allocator,
                    IntPtr.Zero,
                    0,
                    EnumerateColumnsGrbit.EnumerateCompressOutput);
                Marshal.ReadInt32(values[0].pvData);
                allocator(IntPtr.Zero, values[0].pvData, 0);
                allocator(IntPtr.Zero, values[1].pvData, 0);
                Api.JetCommitTransaction(this.session, CommitTransactionGrbit.None);
            }
        }

        /// <summary>
        /// Seek to, and retrieve the key column from, the specified records.
        /// </summary>
        /// <param name="keys">The keys of the records to retrieve.</param>
        private void SeekToAllRecords(IEnumerable<long> keys)
        {
            foreach (long key in keys)
            {
                Api.JetBeginTransaction(this.session);
                Api.MakeKey(this.session, this.table, key, MakeKeyGrbit.NewKey);
                Api.JetSeek(this.session, this.table, SeekGrbit.SeekEQ);
                Assert.AreEqual(key, Api.RetrieveColumnAsInt64(this.session, this.table, this.columnidKey));
                Api.JetCommitTransaction(this.session, CommitTransactionGrbit.None);
            }
        }

        /// <summary>
        /// Run garbage collection.
        /// </summary>
        private void RunGarbageCollection()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
