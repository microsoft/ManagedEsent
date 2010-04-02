// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DictionaryPerformanceTests.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//   Test PersistentDictionary speed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EsentCollectionsTests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Microsoft.Isam.Esent.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test PersistentDictionary speed.
    /// </summary>
    [TestClass]
    public class DictionaryPerformanceTests
    {
        /// <summary>
        /// The location of the dictionary we use for the tests.
        /// </summary>
        private const string DictionaryLocation = "PerformanceDictionary";

        /// <summary>
        /// The dictionary we are testing.
        /// </summary>
        private PersistentDictionary<long, string> dictionary;

        /// <summary>
        /// Test initialization.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.dictionary = new PersistentDictionary<long, string>(DictionaryLocation);
        }

        /// <summary>
        /// Cleanup after the test.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            this.dictionary.Dispose();
            if (Directory.Exists(DictionaryLocation))
            {
                Cleanup.DeleteDirectoryWithRetry(DictionaryLocation);
            }
        }

        /// <summary>
        /// Sequentially insert records and measure the speed.
        /// </summary>
        [TestMethod]
        [Priority(4)]
        public void TestSequentialInsertAndLookupSpeed()
        {
            const int N = 500000;
            long[] keys = (from x in Enumerable.Range(0, N) select (long)x).ToArray();
            const string Data = "01234567890ABCDEF01234567890ABCDEF";

            var stopwatch = Stopwatch.StartNew();
            foreach (int key in keys)
            {
                this.dictionary.Add(key, Data);
            }

            stopwatch.Stop();
            Console.WriteLine(
                "Sequentially inserted {0:N0} records in {1} ({2:N0} records/second)",
                N,
                stopwatch.Elapsed,
                N * 1000 / stopwatch.ElapsedMilliseconds);

            // Repeatedly read one record
            string s  = this.dictionary[N / 2];
            Assert.AreEqual(Data, s);
            stopwatch = Stopwatch.StartNew();
            for (int j = 0; j < N; ++j)
            {
                s = this.dictionary[N / 2];
            }

            stopwatch.Stop();
            Console.WriteLine(
                "Read one record {0:N0} times {1} ({2:N0} reads/second)",
                N,
                stopwatch.Elapsed,
                N * 1000 / stopwatch.ElapsedMilliseconds);

            // Enumerate all records to bring data into memory
            int i = 0;
            stopwatch = Stopwatch.StartNew();
            foreach (var item in this.dictionary)
            {
                Assert.AreEqual(i, item.Key);
                i++;
            }

            stopwatch.Stop();
            Console.WriteLine(
                "Scanned {0:N0} records in {1} ({2:N0} records/second)",
                i,
                stopwatch.Elapsed,
                i * 1000 / stopwatch.ElapsedMilliseconds);

            // Now lookup entries
            keys.Shuffle();
            stopwatch = Stopwatch.StartNew();
            foreach (int key in keys)
            {
                if (!this.dictionary.TryGetValue(key, out s))
                {
                    Assert.Fail("Key wasn't found");
                }
            }

            stopwatch.Stop();
            Console.WriteLine(
                "Looked up {0:N0} records in {1} ({2:N0} records/second)",
                N,
                stopwatch.Elapsed,
                N * 1000 / stopwatch.ElapsedMilliseconds);

            // Now update the entries
            keys.Shuffle();
            const string Newdata = "something completely different";
            stopwatch = Stopwatch.StartNew();
            foreach (int key in keys)
            {
                this.dictionary[key] = Newdata;
            }

            stopwatch.Stop();
            Console.WriteLine(
                "Updated {0:N0} records in {1} ({2:N0} records/second)",
                N,
                stopwatch.Elapsed,
                N * 1000 / stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Randomly insert records and measure the speed.
        /// </summary>
        [TestMethod]
        [Priority(4)]
        public void TestRandomInsertSpeed()
        {
            const int NumInserts = 100000;
            var keys = Enumerable.Range(0, NumInserts).ToArray();
            keys.Shuffle();

            const string Data = "01234567890ABCDEF01234567890ABCDEF";
            var stopwatch = Stopwatch.StartNew();
            foreach (int key in keys)
            {
                this.dictionary.Add(key, Data);
            }

            stopwatch.Stop();
            Console.WriteLine(
                "Randomly inserted {0:N0} records in {1} ({2:N0} records/second)",
                NumInserts,
                stopwatch.Elapsed,
                NumInserts * 1000 / stopwatch.ElapsedMilliseconds);
        }
    }
}

