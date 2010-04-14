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
    using System.Collections.Generic;
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
            const int N = 1000000;
            const string Data = "01234567890ABCDEF01234567890ABCDEF";
            const string Newdata = "something completely different";

            long[] keys = (from x in Enumerable.Range(0, N) select (long)x).ToArray();

            // Insert the records
            this.Insert(keys, Data);

            // Repeatedly read one record
            keys.Shuffle();
            long key = keys[0];
            Assert.AreEqual(Data, this.dictionary[key]);
            this.RetrieveOneRecord(N, key);

            // Scan all entries to make sure they are in the cache
            this.ScanEntries();

            // Now lookup entries
            keys.Shuffle();
            this.LookupEntries(keys);

            // Use LINQ to find records
            this.SlowLinqQueries(5000);

            // Use LINQ to find records
            this.LinqQueries(20000);

            // Repeatedly run a parameterized LINQ query
            this.FastLinqQueries(N);

            // Now update the entries
            keys.Shuffle();
            this.UpdateAllEntries(keys, Newdata);
        }

        /// <summary>
        /// Randomly insert records and measure the speed.
        /// </summary>
        [TestMethod]
        [Priority(4)]
        public void TestRandomInsertAndLookupSpeed()
        {
            const int N = 100000;
            long[] keys = (from x in Enumerable.Range(0, N) select (long)x).ToArray();
            keys.Shuffle();

            const string Data = "01234567890ABCDEF01234567890ABCDEF";
            const string Newdata = "something completely different";

            // Insert the records
            this.Insert(keys, Data);

            // Scan all entries to make sure they are in the cache
            this.ScanEntries();

            // Now lookup entries
            keys.Shuffle();
            this.LookupEntries(keys);

            // Use LINQ to find records
            this.SlowLinqQueries(5000);

            // Use LINQ to find records
            this.LinqQueries(20000);

            // Repeatedly run a parameterized LINQ query
            this.FastLinqQueries(N);

            // Now update the entries
            keys.Shuffle();
            this.UpdateAllEntries(keys, Newdata);
        }

        /// <summary>
        /// Measure the speed of slow LINQ queries against the dictionary.
        /// These queries are slow because they create the query inside of
        /// the loop and the query has to be compiled each time.
        /// </summary>
        /// <param name="numQueries">Number of queries to perform.</param>
        private void SlowLinqQueries(int numQueries)
        {
            var rand = new Random();
            Stopwatch stopwatch = Stopwatch.StartNew();
            int n = this.dictionary.Count;
            int total = 0;
            for (int i = 0; i < numQueries; ++i)
            {
                // Retrieve up to 10 records (average of 5)
                int min = rand.Next(0, n - 1);
                int max = rand.Next(min, Math.Min(min + 10, n)); // we'll add 1 to this below

                var query = from x in this.dictionary where min <= x.Key && x.Key < max + 1 && x.Value.Length > 0 select x.Value;
                Assert.AreEqual((max + 1) - min, query.Count());
                total += max - min;
            }

            stopwatch.Stop();
            Console.WriteLine(
                "Did {0:N0} LINQ queries in {1} ({2:N0} queries/second, {3:N0} records/second)",
                numQueries,
                stopwatch.Elapsed,
                numQueries * 1000 / stopwatch.ElapsedMilliseconds,
                total * 1000 / stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Measure the speed of LINQ queries against the dictionary.
        /// </summary>
        /// <param name="numQueries">Number of queries to perform.</param>
        private void LinqQueries(int numQueries)
        {
            var rand = new Random();
            Stopwatch stopwatch = Stopwatch.StartNew();
            int n = this.dictionary.Count;
            int total = 0;
            for (int i = 0; i < numQueries; ++i)
            {
                // Retrieve up to 10 records (average of 5)
                int min = rand.Next(0, n - 1);
                int max = rand.Next(min + 1, Math.Min(min + 11, n));

                var query = from x in this.dictionary where min <= x.Key && x.Key < max select x.Value;
                Assert.AreEqual(max - min, query.Count());
                total += max - min;
            }

            stopwatch.Stop();
            Console.WriteLine(
                "Did {0:N0} LINQ queries in {1} ({2:N0} queries/second, {3:N0} records/second)",
                numQueries,
                stopwatch.Elapsed,
                numQueries * 1000 / stopwatch.ElapsedMilliseconds,
                total * 1000 / stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Measure the speed of fast LINQ queries against the dictionary.
        /// </summary>
        /// <param name="numQueries">Number of queries to perform.</param>
        private void FastLinqQueries(int numQueries)
        {
            var rand = new Random();
            Stopwatch stopwatch = Stopwatch.StartNew();
            int n = this.dictionary.Count;

            // Create the Enumerable outside of the loop. This means the expression
            // tree only has to be compiled once.
            int key = 0;
            var query = from x in this.dictionary where x.Key == key select x;

            int total = 0;
            for (int i = 0; i < numQueries; ++i)
            {
                key = rand.Next(0, n);
                foreach (var x in query)
                {
                    Assert.AreEqual(key, x.Key);
                    total++;
                }
            }

            stopwatch.Stop();
            stopwatch.Stop();
            Console.WriteLine(
                "Did {0:N0} LINQ queries in {1} ({2:N0} queries/second, {3:N0} records/second)",
                numQueries,
                stopwatch.Elapsed,
                numQueries * 1000 / stopwatch.ElapsedMilliseconds,
                total * 1000 / stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Repeatedly retrieve the same entry.
        /// </summary>
        /// <param name="numRetrieves">Number of times to retrieve the entry.</param>
        /// <param name="key">The key of the entry to retrieve.</param>
        private void RetrieveOneRecord(int numRetrieves, long key)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < numRetrieves; ++i)
            {
                string s = this.dictionary[key];
            }

            stopwatch.Stop();
            Console.WriteLine(
                "Read one record {0:N0} times {1} ({2:N0} reads/second)",
                numRetrieves,
                stopwatch.Elapsed,
                numRetrieves * 1000 / stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Update the specified entries.
        /// </summary>
        /// <param name="keys">The keys of the entries to update.</param>
        /// <param name="newData">The data to set the entries to.</param>
        private void UpdateAllEntries(ICollection<long> keys, string newData)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            foreach (int key in keys)
            {
                this.dictionary[key] = newData;
            }

            stopwatch.Stop();
            Console.WriteLine(
                "Updated {0:N0} records in {1} ({2:N0} records/second)",
                keys.Count,
                stopwatch.Elapsed,
                keys.Count * 1000 / stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Retrieve all the entries specified by the keys.
        /// </summary>
        /// <param name="keys">The keys to retrieve.</param>
        private void LookupEntries(ICollection<long> keys)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
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
                keys.Count,
                stopwatch.Elapsed,
                keys.Count * 1000 / stopwatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Scan all the dictionary entries.
        /// </summary>
        private void ScanEntries()
        {
            int i = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();
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
        }

        /// <summary>
        /// Insert the specified data using Add.
        /// </summary>
        /// <param name="keys">The keys to insert.</param>
        /// <param name="data">The data for the keys.</param>
        private void Insert(ICollection<long> keys, string data)
        {
            var stopwatch = Stopwatch.StartNew();
            foreach (int key in keys)
            {
                this.dictionary.Add(key, data);
            }

            stopwatch.Stop();
            Console.WriteLine(
                "Sequentially inserted {0:N0} records in {1} ({2:N0} records/second)",
                keys.Count,
                stopwatch.Elapsed,
                keys.Count * 1000 / stopwatch.ElapsedMilliseconds);
        }
    }
}

