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
    using System.Diagnostics;
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
        /// Test dictionary.
        /// </summary>
        private readonly IDictionary<int, string> testDictionary1 = new SortedDictionary<int, string>
        {
            { 0, "alpha" },
            { 1, "foo" },
            { 2, "bar" },
            { 3, "baz" },
            { 4, "qux" },
            { 5, "xyzzy" },
            { 6, "omega" },
        };

        /// <summary>
        /// Test dictionary.
        /// </summary>
        private readonly IDictionary<string, int> testDictionary3 = new SortedDictionary<string, int>
        {
            { "a", 1 },
            { "alpha", 2 },
            { "apple", 3 },
            { "b", 4 },
            { "bing", 6 },
            { "biing", 7 },
            { "biiing", 8 },
            { "bravo", 9 },
            { "c", 10 },
            { "c#", 11 },
            { "c++", 12 },
            { "d", 13 },
            { "delta", 14 },
            { "decimal", 15 },
            { "e", 16 },
            { "echo", 17 },
            { "f", 18 },
            { "g", 19 },
        };

        /// <summary>
        /// Test dictionary.
        /// </summary>
        private IDictionary<DateTime, Guid> testDictionary2;

        /// <summary>
        /// Trace listener. Added to capture PersistentDictionary traces.
        /// </summary>
        private TraceListener traceListener;

        /// <summary>
        /// Test initialization.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.testDictionary2 = new SortedDictionary<DateTime, Guid>();
            var entries = from x in Enumerable.Range(0, 100)
                          select
                              new KeyValuePair<DateTime, Guid>(
                              DateTime.UtcNow + TimeSpan.FromSeconds(x), Guid.NewGuid());
            foreach (KeyValuePair<DateTime, Guid> entry in entries)
            {
                this.testDictionary2.Add(entry);
            }

            this.traceListener = new ConsoleTraceListener();
            Trace.Listeners.Add(this.traceListener);
        }

        /// <summary>
        /// Cleanup after the test.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            Trace.Listeners.Remove(this.traceListener);
            Cleanup.DeleteDirectoryWithRetry(DictionaryLocation);
        }

        /// <summary>
        /// Verify that PersistentDictionary.Where throws an exception when the expression is null.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Verify that PersistentDictionary.Where throws an exception when the expression is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyWhereThrowsExceptionWhenExpressionIsNull()
        {
            using (var persistentDictionary = new PersistentDictionary<Guid, Guid>(DictionaryLocation))
            {
                persistentDictionary.Where(null);
            }            
        }

        /// <summary>
        /// Verify that PersistentDictionary.Any throws an exception when the expression is null.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Verify that PersistentDictionary.Any throws an exception when the expression is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyAnyThrowsExceptionWhenExpressionIsNull()
        {
            using (var persistentDictionary = new PersistentDictionary<Guid, Guid>(DictionaryLocation))
            {
                persistentDictionary.Any(null);
            }
        }

        /// <summary>
        /// Test the LINQ Any operator when records are found.
        /// </summary>
        [TestMethod]
        [Description("Test the LINQ Any operator when records are found")]
        [Priority(2)]
        public void TestLinqAnyTrue()
        {
            using (var persistentDictionary = CloneDictionary(this.testDictionary1))
            {
                Assert.IsTrue(persistentDictionary.Any(x => x.Key < 5));
            }
        }

        /// <summary>
        /// Test the LINQ Any operator when records are found.
        /// </summary>
        [TestMethod]
        [Description("Test the LINQ Any operator when records are not found")]
        [Priority(2)]
        public void TestLinqAnyNotFound()
        {
            using (var persistentDictionary = CloneDictionary(this.testDictionary1))
            {
                Assert.IsFalse(persistentDictionary.Any(x => x.Key < 0));
                Assert.IsFalse(persistentDictionary.Any(x => x.Key > 6));
            }
        }

        /// <summary>
        /// Linq test 1.
        /// </summary>
        [TestMethod]
        [Description("Linq test 1")]
        [Priority(2)]
        public void LinqTest1()
        {
            using (var persistentDictionary = CloneDictionary(this.testDictionary1))
            {
                var expected = from x in this.testDictionary1 where x.Key > 3 select x.Value;
                var actual = from x in persistentDictionary where x.Key > 3 select x.Value;
                EnumerableAssert.AreEqual(expected, actual, null);
            }
        }

        /// <summary>
        /// Linq test 2.
        /// </summary>
        [TestMethod]
        [Description("Linq test 2")]
        [Priority(2)]
        public void LinqTest2()
        {
            using (var persistentDictionary = CloneDictionary(this.testDictionary1))
            {
                var expected = from x in this.testDictionary1 where x.Key >= 1 && x.Key <= 5 && x.Value.StartsWith("b") select x.Value;
                var actual = from x in persistentDictionary where x.Key >= 1 && x.Key <= 5 && x.Value.StartsWith("b") select x.Value;
                EnumerableAssert.AreEqual(expected, actual, null);
            }
        }

        /// <summary>
        /// Linq test 3.
        /// </summary>
        [TestMethod]
        [Description("Linq test 3")]
        [Priority(2)]
        public void LinqTest3()
        {
            using (var persistentDictionary = CloneDictionary(this.testDictionary1))
            {
                var expected = from x in this.testDictionary1 where x.Value.Length == 3 select x.Value;
                var actual = from x in persistentDictionary where x.Value.Length == 3 select x.Value;
                EnumerableAssert.AreEqual(expected, actual, null);
            }
        }

        /// <summary>
        /// Linq test 4.
        /// </summary>
        [TestMethod]
        [Description("Linq test 4")]
        [Priority(2)]
        public void LinqTest4()
        {
            using (var persistentDictionary = CloneDictionary(this.testDictionary2))
            {
                DateTime time = DateTime.UtcNow + TimeSpan.FromSeconds(2);
                var expected = from x in this.testDictionary2 where x.Key > time select x;
                var actual = from x in persistentDictionary where x.Key > time select x;
                EnumerableAssert.AreEqual(expected, actual, null);
            }
        }

        /// <summary>
        /// Linq test 5.
        /// </summary>
        [TestMethod]
        [Description("Linq test 5")]
        [Priority(2)]
        public void LinqTest5()
        {
            using (var persistentDictionary = CloneDictionary(this.testDictionary1))
            {
                var expected = from x in this.testDictionary1 where !(x.Key < 1 || x.Key > 5) && (0 == x.Key % 2) select x.Value;
                var actual = from x in persistentDictionary where !(x.Key < 1 || x.Key > 5) && (0 == x.Key % 2) select x.Value;
                EnumerableAssert.AreEqual(expected, actual, null);
            }
        }

        /// <summary>
        /// Linq test 6.
        /// </summary>
        [TestMethod]
        [Description("Linq test 6")]
        [Priority(2)]
        public void LinqTest6()
        {
            using (var persistentDictionary = CloneDictionary(this.testDictionary1))
            {
                var expected = from x in this.testDictionary1
                               where !(x.Key < 1 || x.Key > 5) && (x.Key > 3 || x.Key > 2)
                               select x.Value;
                var actual = from x in persistentDictionary
                             where !(x.Key < 1 || x.Key > 5) && (x.Key > 3 || x.Key > 2)
                             select x.Value;
                EnumerableAssert.AreEqual(expected, actual, null);
            }
        }

        /// <summary>
        /// Linq test 7.
        /// </summary>
        [TestMethod]
        [Description("Linq test 7")]
        [Priority(2)]
        public void LinqTest7()
        {
            var rand = new Random();
            using (var persistentDictionary = CloneDictionary(this.testDictionary1))
            {
                for (int i = 0; i < 128; ++i)
                {
                    int min = rand.Next(-1, 8);
                    int max = rand.Next(-1, 8);

                    var expected = from x in this.testDictionary1 where x.Key >= min && x.Key <= max select x.Value;
                    var actual = from x in persistentDictionary where x.Key >= min && x.Key <= max select x.Value;
                    EnumerableAssert.AreEqual(expected, actual, null);
                }
            }
        }

        /// <summary>
        /// Linq test 8.
        /// </summary>
        [TestMethod]
        [Description("Linq test 8")]
        [Priority(2)]
        public void LinqTest8()
        {
            using (var persistentDictionary = CloneDictionary(this.testDictionary3))
            {
                var expected = from x in this.testDictionary3 where x.Key.StartsWith("b") select x.Value;
                var actual = from x in persistentDictionary where x.Key.StartsWith("b") select x.Value;
                EnumerableAssert.AreEqual(expected, actual, null);
            }
        }

        /// <summary>
        /// Linq test 9.
        /// </summary>
        [TestMethod]
        [Description("Linq test 9")]
        [Priority(2)]
        public void LinqTest9()
        {
            using (var persistentDictionary = CloneDictionary(this.testDictionary3))
            {
                var expected = from x in this.testDictionary3 where x.Key.StartsWith("de") || x.Key.StartsWith("bi") select x.Value;
                var actual = from x in persistentDictionary where x.Key.StartsWith("de") || x.Key.StartsWith("bi") select x.Value;
                EnumerableAssert.AreEqual(expected, actual, null);
            }
        }

        /// <summary>
        /// Linq test 10.
        /// </summary>
        [TestMethod]
        [Description("Linq test 10")]
        [Priority(2)]
        public void LinqTest10()
        {
            using (var persistentDictionary = CloneDictionary(this.testDictionary3))
            {
                var expected = from x in this.testDictionary3 where (x.Key.CompareTo("a") > 0 && x.Key.CompareTo("c") <= 0) || x.Key.StartsWith("c") select x.Value;
                var actual = from x in persistentDictionary where (x.Key.CompareTo("a") > 0 && x.Key.CompareTo("c") <= 0) || x.Key.StartsWith("c") select x.Value;
                EnumerableAssert.AreEqual(expected, actual, null);
            }
        }

        /// <summary>
        /// Linq test 11. Make sure the Enumerable is lazy with key range evaluation.
        /// </summary>
        [TestMethod]
        [Description("Linq test 11")]
        [Priority(2)]
        public void LinqTest11()
        {
            using (var persistentDictionary = CloneDictionary(this.testDictionary1))
            {
                int min = 3;
                int max = 6;

                var expected = from x in this.testDictionary1 where x.Key >= min && x.Key <= max select x.Key;
                var actual = from x in persistentDictionary where x.Key >= min && x.Key <= max select x.Key;
                EnumerableAssert.AreEqual(expected, actual, null);

                min = 1;
                max = 7;
                EnumerableAssert.AreEqual(expected, actual, null);

                min = 4;
                max = 5;
                EnumerableAssert.AreEqual(expected, actual, null);
            }
        }

        /// <summary>
        /// Create a PersistentDictionary that is a copy of another dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the dictionary key.</typeparam>
        /// <typeparam name="TValue">The type of the dictionary value.</typeparam>
        /// <param name="source">The dictionary to clone.</param>
        /// <returns>A persistent dictionary cloned from the input.</returns>
        private static PersistentDictionary<TKey, TValue> CloneDictionary<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> source) where TKey : IComparable<TKey>
        {
            var dict = new PersistentDictionary<TKey, TValue>(source, DictionaryLocation);
            dict.TraceSwitch.Level = TraceLevel.Verbose;
            return dict;
        }
    }
}