//-----------------------------------------------------------------------
// <copyright file="SimplePerfTest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Isam.Esent.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
    /// <summary>
    /// Basic performance tests
    /// </summary>
    [TestClass]
    public class SimplePerfTest
    {
        private const int DataSize = 32;

        private string directory;

        private Instance instance;
        private Session session;
        private Table table;

        private JET_COLUMNID columnidKey;
        private JET_COLUMNID columnidData;

        // Used to insert records
        private long nextKey = 0;
        private byte[] data;

        // Used to retrieve records
        private byte[] dataBuf;

        private Random random;
        private int cacheSizeMinSaved = 0;

        /// <summary>
        /// Setup for a test -- this creates the database
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.directory = SetupHelper.CreateRandomDirectory();

            this.random = new Random();
            this.data = new byte[DataSize];
            this.random.NextBytes(this.data);

            this.dataBuf = new byte[DataSize];

            JET_DBID dbid;

            string ignored;
            Api.JetGetSystemParameter(
                JET_INSTANCE.Nil, JET_SESID.Nil, JET_param.CacheSizeMin, ref this.cacheSizeMinSaved, out ignored, 0);
            Api.JetSetSystemParameter(JET_INSTANCE.Nil, JET_SESID.Nil, JET_param.CacheSizeMin, 16384, null);

            this.instance = new Instance("SimplePerfTest");
            this.instance.Parameters.LogFileDirectory = this.directory;
            this.instance.Parameters.SystemDirectory = this.directory;
            this.instance.Parameters.MaxVerPages = 1024;

            // Circular logging, 16MB logfiles, 8MB of log buffer
            this.instance.Parameters.CircularLog = true;
            this.instance.Parameters.LogFileSize = 16 * 1024; // in KB
            this.instance.Parameters.LogBuffers = 16 * 1024; // in 512-byte units

            // Create the instance, database and table
            this.instance.Init();
            this.session = new Session(this.instance);
            Api.JetCreateDatabase(this.session, Path.Combine(this.directory, "esentperftest.db"), string.Empty, out dbid, CreateDatabaseGrbit.None);

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
        public void Teardown()
        {
            this.table.Close();
            this.session.End();
            this.instance.Term();
            Api.JetSetSystemParameter(JET_INSTANCE.Nil, JET_SESID.Nil, JET_param.CacheSizeMin, this.cacheSizeMinSaved, null);
            Directory.Delete(this.directory, true);
        }

        /// <summary>
        /// Test inserting and retrieving records.
        /// </summary>
        [TestMethod]
        [Priority(4)]
        public void BasicPerfTest()
        {
            this.CheckMemoryUsage(this.InsertReadSeek);
        }

        private static void TimeAction(string name, Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            Console.WriteLine("{0}: {1}", name, stopwatch.Elapsed);
        }

        private void InsertReadSeek()
        {
            const int NumRecords = 1000000;

            // Randomly seek to all records in the table
            long[] keys = (from x in Enumerable.Range(0, NumRecords) select (long)x).ToArray();
            this.Shuffle(keys);

            TimeAction("Insert records", () => this.InsertRecords(NumRecords / 2));
            TimeAction("Insert records with JetSetColumns", () => this.InsertRecordsWithSetColumns(NumRecords / 2));
            TimeAction("Read one record", () => this.RepeatedlyRetrieveOneRecord(NumRecords));
            TimeAction("Read one record with JetEnumerateColumns", () => this.RepeatedlyRetrieveOneRecordWithEnumColumns(NumRecords));
            TimeAction("Read all records", this.RetrieveAllRecords);
            TimeAction("Seek to all records", () => this.SeekToAllRecords(keys));
            TimeAction("Delete all records", () => this.DeleteAllRecords());
        }

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

        private void InsertRecord()
        {
            long key = this.nextKey++;
            Api.JetPrepareUpdate(this.session, this.table, JET_prep.Insert);
            Api.SetColumn(this.session, this.table, this.columnidKey, key);
            Api.SetColumn(this.session, this.table, this.columnidData, this.data);
            Api.JetUpdate(this.session, this.table);
        }

        private void InsertRecords(int numRecords)
        {
            for (int i = 0; i < numRecords; ++i)
            {
                Api.JetBeginTransaction(this.session);
                this.InsertRecord();
                Api.JetCommitTransaction(this.session, CommitTransactionGrbit.LazyFlush);
            }
        }

        private void InsertRecordsWithSetColumns(int numRecords)
        {
            var setcolumns = new[]
            {
                new JET_SETCOLUMN
                {
                    columnid = this.columnidKey,
                    cbData = sizeof(long),
                    itagSequence = 1,
                },
                new JET_SETCOLUMN
                {
                    columnid = this.columnidData,
                    cbData = this.data.Length,
                    pvData = this.data,
                    itagSequence = 1,
                },
            };
            for (int i = 0; i < numRecords; ++i)
            {
                Api.JetBeginTransaction(this.session);
                Api.JetPrepareUpdate(this.session, this.table, JET_prep.Insert);
                setcolumns[0].pvData = BitConverter.GetBytes(this.nextKey++);
                Api.JetSetColumns(this.session, this.table, setcolumns, setcolumns.Length);
                Api.JetUpdate(this.session, this.table);
                Api.JetCommitTransaction(this.session, CommitTransactionGrbit.LazyFlush);
            }
        }

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

        private void DeleteAllRecords()
        {
            Api.MoveBeforeFirst(this.session, this.table);
            while (Api.TryMoveNext(this.session, this.table))
            {
                Api.JetBeginTransaction(this.session);
                Api.JetDelete(this.session, this.table);
                Api.JetCommitTransaction(this.session, CommitTransactionGrbit.LazyFlush);
            }
        }

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

        private void RunGarbageCollection()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
