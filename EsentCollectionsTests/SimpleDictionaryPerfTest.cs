//-----------------------------------------------------------------------
// <copyright file="SimpleDictionaryPerfTest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using Microsoft.Isam.Esent.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Miei = Microsoft.Isam.Esent.Interop;

    /// <summary>
    /// Basic performance tests
    /// </summary>
    [TestClass]
    public class SimpleDictionaryPerfTest
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
        /// The path to the database.
        /// </summary>
        private string database;

        /// <summary>
        /// The PersistentDictionary to use.
        /// </summary>
        private PersistentDictionary<long, PersistentBlob> dictionary;

        /// <summary>
        /// Random number generation object.
        /// </summary>
        private Random random;

        /// <summary>
        /// Setup for a test -- this creates the database
        /// </summary>
        [TestInitialize]
        [Description("Setup the SimpleDictionaryPerfTest fixture")]
        public void Setup()
        {
            this.directory = InteropApiTests.SetupHelper.CreateRandomDirectory();

            this.random = new Random();

            // Create the instance, database and table
            this.database = Path.Combine(this.directory, "esentperftest.db");
            this.dictionary = new PersistentDictionary<long, PersistentBlob>(this.database);

            // Reset the key for the worker thread
            PerfTestWorker.NextKey = 0;
        }

        /// <summary>
        /// Cleanup after the test
        /// </summary>
        [TestCleanup]
        [Description("Cleanup the SimpleDictionaryPerfTest fixture")]
        public void Teardown()
        {
            this.dictionary.Dispose();
            Cleanup.DeleteDirectoryWithRetry(this.directory);
        }

        /// <summary>
        /// Test inserting and retrieving records.
        /// </summary>
        [TestMethod]
        [Priority(4)]
        [Description("Run a basic performance test")]
        [Timeout(30 * 60 * 1000)]
        public void BasicPerfTest()
        {
            CheckMemoryUsage(this.InsertReadSeek);
        }

        /// <summary>
        /// Test inserting and retrieving records with multiple threads.
        /// </summary>
        [TestMethod]
        [Priority(4)]
        [Description("Run a basic multithreaded stress test")]
        [Timeout(40 * 60 * 1000)]
        public void BasicMultithreadedStressTest()
        {
            CheckMemoryUsage(this.MultithreadedStress);
        }

        /// <summary>
        /// Run garbage collection.
        /// </summary>
        private static void RunGarbageCollection()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// Perform an action, checking the system's memory usage before and after.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        private static void CheckMemoryUsage(Action action)
        {
            RunGarbageCollection();
            long memoryAtStart = EseInteropTestHelper.GCGetTotalMemory(true);
            int collectionCountAtStart = EseInteropTestHelper.GCCollectionCount(0);

            action();

            int collectionCountAtEnd = EseInteropTestHelper.GCCollectionCount(0);
            RunGarbageCollection();
            long memoryAtEnd = EseInteropTestHelper.GCGetTotalMemory(true);
            EseInteropTestHelper.ConsoleWriteLine(
                "Memory changed by {0:N} bytes ({1} GC cycles)",
                memoryAtEnd - memoryAtStart,
                collectionCountAtEnd - collectionCountAtStart);
        }

        /// <summary>
        /// Perform and time the given action.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        /// <param name="action">The operation to perform.</param>
        private static void TimeAction(string name, Action action)
        {
            var stopwatch = Miei.EsentStopwatch.StartNew();
            action();
            stopwatch.Stop();
            EseInteropTestHelper.ConsoleWriteLine("{0}: {1} ({2})", name, stopwatch.Elapsed, stopwatch.ThreadStats);
        }

        /// <summary>
        /// Run multithreaded operations.
        /// </summary>
        /// <param name="worker">The worker to use.</param>
        private static void StressThread(PerfTestWorker worker)
        {
            const int NumRecords = 50000;
            const int NumRetrieves = 100;

            worker.InsertRecordsWithAddFunction(NumRecords);
            worker.RepeatedlyRetrieveOneRecord(NumRetrieves);
            worker.RepeatedlyRetrieveOneRecordWithTryGetValue(NumRetrieves);
            worker.RepeatedlyRetrieveOneRecordWithBracketOperator(NumRetrieves);
            worker.RetrieveAllRecords();

            worker.InsertRecordsWithBracketOperator(NumRecords);
            worker.RepeatedlyRetrieveOneRecord(NumRetrieves);
            worker.RepeatedlyRetrieveOneRecordWithTryGetValue(NumRetrieves);
            worker.RepeatedlyRetrieveOneRecordWithBracketOperator(NumRetrieves);
            worker.RetrieveAllRecords();
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
        /// Get keys in the range (0, numKeys] in a random order.
        /// </summary>
        /// <param name="numKeys">The number of keys that are wanted.</param>
        /// <returns>Keys in the range (0, numKeys] in random order.</returns>
        private long[] GetRandomKeys(int numKeys)
        {
            long[] keys = (from x in Enumerable.Range(0, numKeys) select (long)x).ToArray();
            this.Shuffle(keys);
            return keys;
        }

        // UNDONE: Can this be moved to a Task-model?
#if !MANAGEDESENT_ON_WSA // Thread model has changed in Windows Store Apps.
        /// <summary>
        /// Perform a PerfTestWorker action on a separate thread.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        /// <returns>The thread.</returns>
        private Thread StartWorkerThread(Action<PerfTestWorker> action)
        {
            var thread = new Thread(
                () =>
                {
                    using (var worker = new PerfTestWorker(this.dictionary, this.database))
                    {
                        action(worker);
                    }
                });
            return thread;
        }
#endif // !MANAGEDESENT_ON_WSA

        /// <summary>
        /// Insert some records and then retrieve them.
        /// </summary>
        private void InsertReadSeek()
        {
            const int NumRecords = 100 * 1000;

            long[] keys = this.GetRandomKeys(NumRecords);

            using (var worker = new PerfTestWorker(this.dictionary, this.database))
            {
                TimeAction("Insert records", () => worker.InsertRecordsWithAddFunction(NumRecords / 2));
                TimeAction("Insert records with SetColumn", () => worker.InsertRecordsWithBracketOperator(NumRecords / 2));
                TimeAction("Read one record", () => worker.RepeatedlyRetrieveOneRecord(NumRecords));
                TimeAction("Read one record with JetRetrieveColumns", () => worker.RepeatedlyRetrieveOneRecordWithTryGetValue(NumRecords));
                TimeAction("Read one record with RetrieveColumns", () => worker.RepeatedlyRetrieveOneRecordWithBracketOperator(NumRecords));
                TimeAction("Read all records", worker.RetrieveAllRecords);
                TimeAction("Seek to all records", () => worker.SeekToAllRecords(keys));
            }
        }

        /// <summary>
        /// Insert some records and then retrieve them.
        /// </summary>
        private void MultithreadedStress()
        {
            // UNDONE: Can this be moved to a Task-model?
#if !MANAGEDESENT_ON_WSA // Thread model has changed in Windows Store Apps.
            const int NumThreads = 8;
            Thread[] threads = (from i in Enumerable.Repeat(0, NumThreads) select this.StartWorkerThread(StressThread)).ToArray();
            foreach (Thread thread in threads)
            {
                thread.Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }
#endif // !MANAGEDESENT_ON_WSA
        }

        /// <summary>
        /// Worker for the performance test.
        /// </summary>
        internal class PerfTestWorker : IDisposable
        {
            /// <summary>
            /// The next key value to be inserted. Used to insert records.
            /// </summary>
            private static long nextKey = 0;

            /// <summary>
            /// The database to use.
            /// </summary>
            private readonly string database;

            /// <summary>
            /// Data to be inserted into the data column.
            /// </summary>
            private readonly byte[] data;

            /// <summary>
            /// Used to wrap the <see cref="data"/> member.
            /// </summary>
            private readonly PersistentBlob persistentBlob;

            /// <summary>
            /// The PersistentDictionary to use.
            /// </summary>
            private PersistentDictionary<long, PersistentBlob> dictionary;

            /// <summary>
            /// The key of the last record to be inserted.
            /// </summary>
            private long lastKey;

            /// <summary>
            /// Initializes a new instance of the <see cref="PerfTestWorker"/> class.
            /// </summary>
            /// <param name="dictionary">
            /// The dictionary to use.
            /// </param>
            /// <param name="database">
            /// Path to the database. The database should already be created.
            /// </param>
            public PerfTestWorker(
                PersistentDictionary<long, PersistentBlob> dictionary,
                string database)
            {
                EseInteropTestHelper.ThreadBeginThreadAffinity();
                this.dictionary = dictionary;
                this.database = database;

                this.data = new byte[SimpleDictionaryPerfTest.DataSize];
                this.persistentBlob = new PersistentBlob(this.data);
            }

            /// <summary>
            /// Finalizes an instance of the <see cref="PerfTestWorker"/> class. 
            /// </summary>
            ~PerfTestWorker()
            {
                this.Dispose(false);
            }

            /// <summary>
            /// Sets the next key value to be inserted. Used to insert records.
            /// </summary>
            public static long NextKey
            {
                set
                {
                    nextKey = value;
                }
            }

            #region Dispose
            /// <summary>
            /// Disposes an instance of the PerfTestWorker class.
            /// </summary>
            public void Dispose()
            {
                this.Dispose(true);
            }
            #endregion

            /// <summary>
            /// Insert multiple records with the Add() function.
            /// </summary>
            /// <param name="numRecords">The number of records to insert.</param>
            public void InsertRecordsWithAddFunction(int numRecords)
            {
                for (int i = 0; i < numRecords; ++i)
                {
                    this.dictionary.Add(this.GetNextKey(), this.persistentBlob);
                }
            }

            /// <summary>
            /// Insert multiple records with the operator[] notation.
            /// </summary>
            /// <param name="numRecords">The number of records to insert.</param>
            public void InsertRecordsWithBracketOperator(int numRecords)
            {
                for (int i = 0; i < numRecords; ++i)
                {
                    this.dictionary[this.GetNextKey()] = this.persistentBlob;
                }
            }

            /// <summary>
            /// Retrieve all records in the table.
            /// </summary>
            public void RetrieveAllRecords()
            {
                foreach (var entry in this.dictionary.Keys)
                {
                    PersistentBlob value = this.dictionary[entry];
                }
            }

            /// <summary>
            /// Retrieve the current record multiple times.
            /// </summary>
            /// <param name="numRetrieves">The number of times to retrieve the record.</param>
            public void RepeatedlyRetrieveOneRecord(int numRetrieves)
            {
                for (int i = 0; i < numRetrieves; ++i)
                {
                    PersistentBlob value = this.dictionary[this.lastKey];
                }
            }

            /// <summary>
            /// Repeatedly retrieve one record using TryGetValue.
            /// </summary>
            /// <param name="numRetrieves">The number of times to retrieve the record.</param>
            public void RepeatedlyRetrieveOneRecordWithTryGetValue(int numRetrieves)
            {
                for (int i = 0; i < numRetrieves; ++i)
                {
                    PersistentBlob retrievedBytes;
                    bool isPresent = this.dictionary.TryGetValue(this.lastKey, out retrievedBytes);
                    Assert.IsTrue(isPresent);
                }
            }

            /// <summary>
            /// Repeatedly retrieve one record using Bracket Notation.
            /// </summary>
            /// <param name="numRetrieves">The number of times to retrieve the record.</param>
            public void RepeatedlyRetrieveOneRecordWithBracketOperator(int numRetrieves)
            {
                for (int i = 0; i < numRetrieves; ++i)
                {
                    var retrievedBytes = this.dictionary[this.lastKey];
                }
            }

            /// <summary>
            /// Seek to, and retrieve the key column from, the specified records.
            /// </summary>
            /// <param name="keys">The keys of the records to retrieve.</param>
            public void SeekToAllRecords(IEnumerable<long> keys)
            {
                foreach (long key in keys)
                {
                    Assert.IsTrue(this.dictionary.ContainsKey(key));
                }
            }

            /// <summary>
            /// Called for the disposer and finalizer.
            /// </summary>
            /// <param name="isDisposing">True if called from dispose.</param>
            protected virtual void Dispose(bool isDisposing)
            {
                EseInteropTestHelper.ThreadEndThreadAffinity();
            }

            /// <summary>
            /// Get the key for the next record to be inserted.
            /// </summary>
            /// <returns>The next key to use.</returns>
            private long GetNextKey()
            {
                this.lastKey = Interlocked.Increment(ref nextKey) - 1;
                return this.lastKey;
            }
        }
    }
}