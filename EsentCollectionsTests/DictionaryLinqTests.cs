// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DictionaryLinqTests.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//   Basic PersistentDictionary tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EsentCollectionsTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Isam.Esent.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the PersistentDictionary.
    /// </summary>
    [TestClass]
    public class DictionaryLinqTests
    {
        /// <summary>
        /// Where the dictionary will be located.
        /// </summary>
        private const string DictionaryLocation = "DictionaryLinqFixture";

        /// <summary>
        /// The dictionary we are testing.
        /// </summary>
        private PersistentDictionary<int, long> dictionary;

        /// <summary>
        /// Test initialization.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.dictionary = new PersistentDictionary<int, long>(DictionaryLocation);
            for (int i = 0; i < 100; ++i)
            {
                this.dictionary[i] = i * 100;
            }
        }

        /// <summary>
        /// Cleanup after the test.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            this.dictionary.Dispose();
            Cleanup.DeleteDirectoryWithRetry(DictionaryLocation);
        }

        /// <summary>
        /// Linq test 1.
        /// </summary>
        [TestMethod]
        [Description("Linq test 1")]
        [Priority(2)]
        public void LinqTest1()
        {
            long[] expected = new long[] { 1100, 1200 };
            IEnumerable<long> results = from x in this.dictionary where x.Key > 10 && x.Key < 13 select x.Value;
            Assert.IsTrue(results.SequenceEqual(expected));
        }

        /// <summary>
        /// Linq test 2.
        /// </summary>
        [TestMethod]
        [Description("Linq test 2")]
        [Priority(2)]
        public void LinqTest2()
        {
            int first = 50;
            int count = 20;
            IEnumerable<long> expected = from x in Enumerable.Range(50, count) select (long)(x * 100);
            IEnumerable<long> results = from x in this.dictionary where x.Key >= first && x.Key < first + count select x.Value;
            Assert.IsTrue(results.SequenceEqual(expected));
        }
    }
}