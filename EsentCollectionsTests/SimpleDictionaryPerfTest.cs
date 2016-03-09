//-----------------------------------------------------------------------
// <copyright file="SimpleDictionaryPerfTest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
        /// The path to the database that stores integers.
        /// </summary>
        private string longDatabase;

        /// <summary>
        /// The path to the database that stores strings.
        /// </summary>
        private string stringDatabase;

        /// <summary>
        /// The PersistentDictionary to use for integer keys.
        /// </summary>
        private PersistentDictionary<long, PersistentBlob> longDictionary;

        /// <summary>
        /// The PersistentDictionary to use for storing string-based keys.
        /// </summary>
        private PersistentDictionary<string, PersistentBlob> stringDictionary;

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
            this.longDatabase = Path.Combine(this.directory, "esentperftest.db");
            this.longDictionary = new PersistentDictionary<long, PersistentBlob>(this.longDatabase);

            this.stringDatabase = Path.Combine(this.directory, "esentperfteststrings.db");
            this.stringDictionary = new PersistentDictionary<string, PersistentBlob>(this.stringDatabase);

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
            this.longDictionary.Dispose();
            this.stringDictionary.Dispose();
            Cleanup.DeleteDirectoryWithRetry(this.directory);
        }

        /// <summary>
        /// Test inserting and retrieving integer records.
        /// </summary>
        [TestMethod]
        [Priority(4)]
        [Description("Run a basic performance test for long's.")]
        [Timeout(30 * 60 * 1000)]
        public void BasicLongDictPerfTest()
        {
            CheckMemoryUsage(this.InsertReadSeekLongs);
        }

        /// <summary>
        /// Test inserting and retrieving string records.
        /// </summary>
        [TestMethod]
        [Priority(4)]
        [Description("Run a basic performance test for strings.")]
        [Timeout(30 * 60 * 1000)]
        public void BasicStringDictPerfTest()
        {
            CheckMemoryUsage(this.InsertReadSeekStrings);
        }

        /// <summary>
        /// Test inserting and retrieving integer records with multiple threads.
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
        /// Test inserting and retrieving string records with multiple threads.
        /// </summary>
        [TestMethod]
        [Priority(4)]
        [Description("Run a basic multithreaded stress test with string records")]
        [Timeout(40 * 60 * 1000)]
        public void BasicMultithreadedStringStressTest()
        {
            System.Console.WriteLine("BasicMultithreadedStringStressTest begun.");
            Debug.WriteLine("dbg:BasicMultithreadedStringStressTest begun.");
            CheckMemoryUsage(this.MultithreadedStringStress);
            System.Console.WriteLine("BasicMultithreadedStringStressTest ended.");
            Debug.WriteLine("dbg:BasicMultithreadedStringStressTest ended.");
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
        /// Run multithreaded stress string-related operations.
        /// </summary>
        /// <param name="worker">The worker to use.</param>
        private static void StringStressThread(PerfTestWorker worker)
        {
            const int NumRecords = 10 * 1000;
            const int NumRetrieves = 100;

            worker.InsertStringRecordsWithBracketOperator(NumRecords);
            worker.RepeatedlyRetrieveOneStringRecord(NumRetrieves);
            worker.RepeatedlyRetrieveOneStringRecordWithTryGetValue(NumRetrieves);
            worker.RepeatedlyRetrieveOneStringRecordWithBracketOperator(NumRetrieves);
            worker.RetrieveAllStringRecords();

            worker.InsertStringRecordsWithAddFunction(NumRecords);
            worker.RepeatedlyRetrieveOneStringRecord(NumRetrieves);
            worker.RepeatedlyRetrieveOneStringRecordWithTryGetValue(NumRetrieves);
            worker.RepeatedlyRetrieveOneStringRecordWithBracketOperator(NumRetrieves);
            worker.RetrieveAllStringRecords();
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
                    using (var worker = new PerfTestWorker(
                        this.longDictionary,
                        this.longDatabase,
                        this.stringDictionary,
                        this.stringDatabase))
                    {
                        action(worker);
                    }
                });
            return thread;
        }
#endif // !MANAGEDESENT_ON_WSA

        /// <summary>
        /// Insert some integer records and then retrieve them.
        /// </summary>
        private void InsertReadSeekLongs()
        {
            const int NumRecords = 100 * 1000;

            long[] keys = this.GetRandomKeys(NumRecords);

            using (var worker = new PerfTestWorker(
                this.longDictionary,
                this.longDatabase,
                this.stringDictionary,
                this.stringDatabase))
            {
                TimeAction("Insert records with Add()s", () => worker.InsertRecordsWithAddFunction(NumRecords / 2));
                TimeAction("Insert records with []", () => worker.InsertRecordsWithBracketOperator(NumRecords / 2));
                TimeAction("Read one record", () => worker.RepeatedlyRetrieveOneRecord(NumRecords));
                TimeAction("Read one record with JetRetrieveColumns", () => worker.RepeatedlyRetrieveOneRecordWithTryGetValue(NumRecords));
                TimeAction("Read one record with RetrieveColumns", () => worker.RepeatedlyRetrieveOneRecordWithBracketOperator(NumRecords));
                TimeAction("Read all records sequentially", worker.RetrieveAllRecords);
                TimeAction("Seek to all records randomly", () => worker.SeekToAllRecords(keys));
            }
        }

        /// <summary>
        /// Insert some string records and then retrieve them.
        /// </summary>
        private void InsertReadSeekStrings()
        {
            const int NumRecords = 100 * 1000;

            long[] keys = this.GetRandomKeys(NumRecords);
            IEnumerable<string> stringKeys = keys.Select((key) => { return key.ToString(); });

            using (var worker = new PerfTestWorker(
                this.longDictionary,
                this.longDatabase,
                this.stringDictionary,
                this.stringDatabase))
            {
                TimeAction("Insert string record with Add()s", () => worker.InsertStringRecordsWithAddFunction(NumRecords / 2));
                TimeAction("Insert string records with []", () => worker.InsertStringRecordsWithBracketOperator(NumRecords / 2));
                TimeAction("Read one string record", () => worker.RepeatedlyRetrieveOneStringRecord(NumRecords));
                TimeAction("Read one string record with JetRetrieveColumns", () => worker.RepeatedlyRetrieveOneStringRecordWithTryGetValue(NumRecords));
                TimeAction("Read one string record with RetrieveColumns", () => worker.RepeatedlyRetrieveOneStringRecordWithBracketOperator(NumRecords));
                TimeAction("Read all string records", worker.RetrieveAllStringRecords);
                TimeAction("Seek to all string records randomly", () => worker.SeekToAllStringRecords(stringKeys));
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
        /// Insert some records and then retrieve them.
        /// </summary>
        private void MultithreadedStringStress()
        {
            // UNDONE: Can this be moved to a Task-model?
#if !MANAGEDESENT_ON_WSA // Thread model has changed in Windows Store Apps.
            const int NumThreads = 12;
            Thread[] threads = (from i in Enumerable.Repeat(0, NumThreads)
                                select this.StartWorkerThread(StringStressThread)).ToArray();
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
            /// A random number generator.
            /// </summary>
            private static Random staticRandom = new Random(System.Threading.Thread.CurrentThread.ManagedThreadId);

            /// <summary>
            /// The database to use for integer lookups.
            /// </summary>
            private readonly string longDatabase;

            /// <summary>
            /// The database to use for string lookups.
            /// </summary>
            private readonly string stringDatabase;

            /// <summary>
            /// Data to be inserted into the data column.
            /// </summary>
            private readonly byte[] data;

            /// <summary>
            /// Used to wrap the <see cref="data"/> member.
            /// </summary>
            private readonly PersistentBlob persistentBlob;

            /// <summary>
            /// The PersistentDictionary to use for integer keys.
            /// </summary>
            private PersistentDictionary<long, PersistentBlob> longDictionary;

            /// <summary>
            /// The PersistentDictionary to use for storing string-based keys.
            /// </summary>
            private PersistentDictionary<string, PersistentBlob> stringDictionary;

            /// <summary>
            /// The key of the last record to be inserted.
            /// </summary>
            private long lastKey;

            /// <summary>
            /// The key of the last record to be inserted.
            /// </summary>
            private string lastStringKey;

            /// <summary>
            /// A random number generator.
            /// </summary>
            private Random myRandom = new Random(System.Threading.Thread.CurrentThread.ManagedThreadId);

            /// <summary>
            /// Initializes a new instance of the <see cref="PerfTestWorker"/> class.
            /// </summary>
            /// <param name="longDictionary">
            /// The dictionary to use for integer lookups.
            /// </param>
            /// <param name="longDatabase">
            /// Path to the integer-lookup database. The database should already be created.
            /// </param>
            /// <param name="stringDictionary">
            /// The dictionary to use for string lookups.
            /// </param>
            /// <param name="stringDatabase">
            /// Path to the string-lookup database. The database should already be created.
            /// </param>
            public PerfTestWorker(
                PersistentDictionary<long, PersistentBlob> longDictionary,
                string longDatabase,
                PersistentDictionary<string, PersistentBlob> stringDictionary,
                string stringDatabase)
            {
                EseInteropTestHelper.ThreadBeginThreadAffinity();
                this.longDictionary = longDictionary;
                this.longDatabase = longDatabase;
                this.stringDictionary = stringDictionary;
                this.stringDatabase = stringDatabase;

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
                    this.longDictionary.Add(this.GetNextKey(), this.persistentBlob);
                }
            }

            /// <summary>
            /// Insert multiple string records with the Add() function.
            /// </summary>
            /// <param name="numRecords">The number of records to insert.</param>
            public void InsertStringRecordsWithAddFunction(int numRecords)
            {
                Random random = new Random(System.Threading.Thread.CurrentThread.ManagedThreadId);

                for (int i = 0; i < numRecords; ++i)
                {
                    int currentMaxKey = (int)this.GetNextKey();

                    if (0 == currentMaxKey % 100)
                    {
                        System.Console.Write('.');
                    }

                    try
                    {
                        // Insert/Append:
                        this.stringDictionary.Add(currentMaxKey.ToString(), this.persistentBlob);
                    }
                    catch (ArgumentException)
                    {
                    }

                    // Try a replace (but it will fail).
                    try
                    {
                        this.stringDictionary.Add((currentMaxKey - random.Next(300)).ToString(), this.persistentBlob);
                    }
                    catch (ArgumentException)
                    {
                    }
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
                    this.longDictionary[this.GetNextKey()] = this.persistentBlob;
                }
            }

            /// <summary>
            /// Insert multiple string records with the operator[] notation.
            /// </summary>
            /// <param name="numRecords">The number of records to insert.</param>
            public void InsertStringRecordsWithBracketOperator(int numRecords)
            {
                for (int i = 0; i < numRecords; ++i)
                {
                    int currentMaxKey = (int)this.GetNextKey();

                    if (0 == currentMaxKey % 100)
                    {
                        System.Console.Write('.');
                    }

                    // Insert/Append:
                    this.stringDictionary[currentMaxKey.ToString()] = this.persistentBlob;

                    // Replace:
                    this.stringDictionary[GetStringKey(currentMaxKey - this.myRandom.Next(30))] = this.persistentBlob;

                    // Delete:
                    string deleteRecreateKey = GetStringKey(currentMaxKey - this.myRandom.Next(30));
                    this.stringDictionary.Remove(deleteRecreateKey);

                    // And re-insert it. Otherwise SeekToAllStringRecords will be missing some keys
                    // and will fail!
                    this.stringDictionary[deleteRecreateKey] = this.persistentBlob;
                }
            }

            /// <summary>
            /// Retrieve all records in the table.
            /// </summary>
            public void RetrieveAllRecords()
            {
                foreach (var entry in this.longDictionary.Keys)
                {
                    PersistentBlob value = this.longDictionary[entry];
                }
            }

            /// <summary>
            /// Retrieve all string records in the table.
            /// </summary>
            public void RetrieveAllStringRecords()
            {
                foreach (var entry in this.stringDictionary.Keys)
                {
                    PersistentBlob value = this.stringDictionary[entry];
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
                    PersistentBlob value = this.longDictionary[this.lastKey];
                }
            }

            /// <summary>
            /// Retrieve the current record multiple times using string keys.
            /// </summary>
            /// <param name="numRetrieves">The number of times to retrieve the record.</param>
            public void RepeatedlyRetrieveOneStringRecord(int numRetrieves)
            {
                for (int i = 0; i < numRetrieves; ++i)
                {
                    PersistentBlob value = this.stringDictionary[this.lastStringKey];
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
                    bool isPresent = this.longDictionary.TryGetValue(this.lastKey, out retrievedBytes);
                    Assert.IsTrue(isPresent);
                }
            }

            /// <summary>
            /// Repeatedly retrieve one string record using TryGetValue.
            /// </summary>
            /// <param name="numRetrieves">The number of times to retrieve the record.</param>
            public void RepeatedlyRetrieveOneStringRecordWithTryGetValue(int numRetrieves)
            {
                for (int i = 0; i < numRetrieves; ++i)
                {
                    PersistentBlob retrievedBytes;
                    bool isPresent = this.stringDictionary.TryGetValue(this.lastStringKey, out retrievedBytes);
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
                    var retrievedBytes = this.longDictionary[this.lastKey];
                }
            }

            /// <summary>
            /// Repeatedly retrieve one record using Bracket Notation and string keys.
            /// </summary>
            /// <param name="numRetrieves">The number of times to retrieve the record.</param>
            public void RepeatedlyRetrieveOneStringRecordWithBracketOperator(int numRetrieves)
            {
                for (int i = 0; i < numRetrieves; ++i)
                {
                    var retrievedBytes = this.stringDictionary[this.lastStringKey];
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
                    Assert.IsTrue(this.longDictionary.ContainsKey(key));
                }
            }

            /// <summary>
            /// Seek to, and retrieve the key column from, the specified records.
            /// </summary>
            /// <param name="keys">The keys of the records to retrieve.</param>
            public void SeekToAllStringRecords(IEnumerable<string> keys)
            {
                foreach (string key in keys)
                {
                    Assert.IsTrue(
                        this.stringDictionary.ContainsKey(key),
                        "All keys should be present, but '{0}' is not present.",
                        key);
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
            /// Returns a string form of the specified integer key. Some of the time it
            /// will be an upper case hex number, some of the time it will be lower case.
            /// </summary>
            /// <param name="longKey">The numerical key to convert to a string.</param>
            /// <returns>The string form of the numerical key.</returns>
            private static string GetStringKey(long longKey)
            {
                if (staticRandom.Next(2) == 1)
                {
                    return longKey.ToString("x");
                }
                else
                {
                    return longKey.ToString("X");
                }
            }

            /// <summary>
            /// Get the key for the next record to be inserted.
            /// </summary>
            /// <returns>The next key to use.</returns>
            private long GetNextKey()
            {
                this.lastKey = Interlocked.Increment(ref nextKey) - 1;
                this.lastStringKey = this.lastKey.ToString();
                return this.lastKey;
            }
        }
    }
}
