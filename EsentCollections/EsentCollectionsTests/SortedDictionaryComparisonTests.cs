//-----------------------------------------------------------------------
// <copyright file="DictionaryComparisonTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Isam.Esent.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EsentCollectionsTests
{
    /// <summary>
    /// Compare a PersistentDictionary against a generic SortedDictionary.
    /// </summary>
    [TestClass]
    public class SortedDictionaryComparisonTests
    {
        /// <summary>
        /// Where the dictionary will be located.
        /// </summary>
        private const string DictionaryLocation = "SortedDictionaryComparisonFixture";

        /// <summary>
        /// A generic sorted dictionary that we will use as the oracle.
        /// </summary>
        private SortedDictionary<string, string> expected;

        /// <summary>
        /// The dictionary we are testing.
        /// </summary>
        private PersistentDictionary<string, string> actual;

        /// <summary>
        /// Test initialization.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.expected = new SortedDictionary<string, string>();
            this.actual = new PersistentDictionary<string, string>(DictionaryLocation);
        }

        /// <summary>
        /// Cleanup after the test.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            this.actual.Dispose();
            if (Directory.Exists(DictionaryLocation))
            {
                Directory.Delete(DictionaryLocation, true);
            }
        }

        /// <summary>
        /// Compare two empty dictionaries.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestEmptyDictionary()
        {
            this.CompareDictionaries();
        }

        /// <summary>
        /// Insert one item into the dictionary.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestInsert()
        {
            this.expected["foo"] = this.actual["foo"] = "1";
            this.CompareDictionaries();
        }

        /// <summary>
        /// Replace an item.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestReplace()
        {
            this.expected["foo"] = this.actual["foo"] = "1";
            this.expected["foo"] = this.actual["foo"] = "2";
            this.CompareDictionaries();
        }

        /// <summary>
        /// Delete an item.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestDelete()
        {
            this.expected["foo"] = this.actual["foo"] = "1";
            this.expected["bar"] = this.actual["bar"] = "2";
            this.expected.Remove("foo");
            Assert.IsTrue(this.actual.Remove("foo"));
            this.CompareDictionaries();
        }

        /// <summary>
        /// Insert an item into the dictionary.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestAddItem()
        {
            var item = new KeyValuePair<string, string>("thekey", "thevalue");
            ((ICollection<KeyValuePair<string, string>>) this.expected).Add(item);
            this.actual.Add(item);
            this.CompareDictionaries();
        }

        /// <summary>
        /// Insert an item into the dictionary and remove it.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestRemoveItem()
        {
            var item = new KeyValuePair<string, string>("thekey", "thevalue");
            this.expected.Add(item.Key, item.Value);
            this.actual.Add(item.Key, item.Value);
            ((ICollection<KeyValuePair<string, string>>) this.expected).Remove(item);
            Assert.IsTrue(this.actual.Remove(item));
            this.CompareDictionaries();
        }

        /// <summary>
        /// Insert several items into the dictionary.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestAdds()
        {
            for (int i = 0; i < 10; ++i)
            {
                this.expected.Add(i.ToString(), i.ToString());
                this.actual.Add(i.ToString(), i.ToString());
            }

            this.CompareDictionaries();
        }

        /// <summary>
        /// Clear an empty dictionary.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestClearEmptyDictionary()
        {
            this.expected.Clear();
            this.actual.Clear();
            this.CompareDictionaries();
        }

        /// <summary>
        /// Clear the dictionary.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestClear()
        {
            for (int i = 7; i >= 0; --i)
            {
                this.expected.Add(i.ToString(), i.ToString());
                this.actual.Add(i.ToString(), i.ToString());
            }

            this.expected.Clear();
            this.actual.Clear();
            this.CompareDictionaries();
        }

        /// <summary>
        /// Clear the dictionary twice.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestClearTwice()
        {
            this.expected["foo"] = this.actual["foo"] = "!";

            this.expected.Clear();
            this.actual.Clear();
            this.expected.Clear();
            this.actual.Clear();
            this.CompareDictionaries();
        }

        /// <summary>
        /// Store a null value in the dictionary.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestNullValue()
        {
            this.expected["a"] = this.actual["a"] = null;
            this.CompareDictionaries();
        }

        /// <summary>
        /// Close and reopen the database.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestCloseAndReopen()
        {
            var rand = new Random();
            for (int i = 0; i < 100; ++i)
            {
                string k = rand.Next().ToString();
                string v = rand.NextDouble().ToString();
                this.expected.Add(k, v);
                this.actual.Add(k, v);
            }

            this.actual.Dispose();
            this.actual = new PersistentDictionary<string, string>(DictionaryLocation);
            this.CompareDictionaries();
        }

        /// <summary>
        /// Close and delete the database.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestCloseAndDelete()
        {
            var rand = new Random();
            for (int i = 0; i < 64; ++i)
            {
                string k = rand.NextDouble().ToString();
                string v = rand.Next().ToString();
                this.expected.Add(k, v);
                this.actual.Add(k, v);
            }

            this.actual.Dispose();
            PersistentDictionaryFile.DeleteFiles(DictionaryLocation);

            // Deleting the files clears the dictionary
            this.expected.Clear();

            this.actual = new PersistentDictionary<string, string>(DictionaryLocation);
            this.CompareDictionaries();
        }

        /// <summary>
        /// Determine if two enumerations are equivalent. Enumerations are
        /// equivalent if they contain the same members in any order.
        /// </summary>
        /// <typeparam name="T">The type of the enumeration.</typeparam>
        /// <param name="c1">The first enumeration.</param>
        /// <param name="c2">The second enumeration.</param>
        /// <returns>True if the enumerations are equivalent.</returns>
        private static bool AreEquivalent<T>(IEnumerable<T> c1, IEnumerable<T> c2)
        {
            var s1 = c1.OrderBy(x => x);
            var s2 = c2.OrderBy(x => x);
            return s1.SequenceEqual(s2);
        }

        /// <summary>
        /// Compare the expected and actual dictionaries.
        /// </summary>
        private void CompareDictionaries()
        {
            Assert.AreEqual(this.expected.Count, this.actual.Count);
            Assert.AreEqual(this.expected.Keys.Count, this.actual.Keys.Count);
            Assert.AreEqual(this.expected.Values.Count, this.actual.Values.Count);

            Assert.IsTrue(AreEquivalent(this.expected.Keys, this.actual.Keys));
            Assert.IsTrue(AreEquivalent(this.expected.Values, this.actual.Values));

            var enumeratedKeys = from i in this.actual select i.Key;
            Assert.IsTrue(AreEquivalent(this.expected.Keys, enumeratedKeys));

            var enumeratedValues = from i in this.actual select i.Value;
            Assert.IsTrue(AreEquivalent(this.expected.Values, enumeratedValues));

            Assert.IsTrue(this.expected.SequenceEqual(this.actual));

            if (expected.Count > 0)
            {
                Assert.AreEqual(this.expected.Keys.Min(), this.actual.Keys.Min());
                Assert.AreEqual(this.expected.Keys.Max(), this.actual.Keys.Max());
            }

            foreach (string k in this.expected.Keys)
            {
                Assert.IsTrue(this.actual.ContainsKey(k));
                Assert.IsTrue(this.actual.Keys.Contains(k));

                string v;
                Assert.IsTrue(this.actual.TryGetValue(k, out v));
                Assert.AreEqual(this.expected[k], v);
                Assert.AreEqual(this.expected[k], this.actual[k]);

                Assert.IsTrue(this.actual.ContainsValue(v));
                Assert.IsTrue(this.actual.Values.Contains(v));

                Assert.IsTrue(this.actual.Contains(new KeyValuePair<string, string>(k, v)));
            }
        }
    }
}
