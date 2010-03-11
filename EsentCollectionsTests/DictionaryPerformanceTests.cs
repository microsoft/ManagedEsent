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
            const int NumInserts = 500000;
            var keys = Enumerable.Range(0, NumInserts).ToArray();
            const string Data = "01234567890ABCDEF01234567890ABCDEF";
            var stopwatch = Stopwatch.StartNew();
            foreach (int key in keys)
            {
                this.dictionary.Add(key, Data);
            }

            stopwatch.Stop();
            Console.WriteLine(
                "Sequentially inserted {0:N0} records in {1} ({2:N0} records/second)",
                NumInserts,
                stopwatch.Elapsed,
                NumInserts * 1000 / stopwatch.ElapsedMilliseconds);

            // Enumerate all records to bring data into memory
            int i = 0;
            foreach (var item in this.dictionary)
            {
                Assert.AreEqual(i, item.Key);
                i++;
            }

            // Now lookup entries
            keys.Shuffle();
            stopwatch = Stopwatch.StartNew();
            foreach (int key in keys)
            {
                string s;
                if (!this.dictionary.TryGetValue(key, out s))
                {
                    Assert.Fail("Key wasn't found");
                }
            }

            stopwatch.Stop();
            Console.WriteLine(
                "Looked up {0:N0} records in {1} ({2:N0} records/second)",
                NumInserts,
                stopwatch.Elapsed,
                NumInserts * 1000 / stopwatch.ElapsedMilliseconds);

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
                NumInserts,
                stopwatch.Elapsed,
                NumInserts * 1000 / stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Randomly insert records and measure the speed.
        /// </summary>
        [TestMethod]
        [Priority(4)]
        public void TestRandomInsertSpeed()
        {
            const int NumInserts = 50000;
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

