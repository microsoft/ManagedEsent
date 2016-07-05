// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DictionaryCaseComparisonTests.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//   Compare a PersistentDictionary against a generic dictionary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EsentCollectionsTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Microsoft.Isam.Esent.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Ensure the case-sensitivity of key insertions behave as expected.
    /// </summary>
    [TestClass]
    public class DictionaryCaseComparisonTests
    {
        /// <summary>
        /// Where the dictionary will be located.
        /// </summary>
        private const string DictionaryLocation = "DictionaryCaseComparisonFixture";

        /// <summary>
        /// A generic case-insensitive dictionary that we will use as the oracle.
        /// </summary>
        private Dictionary<string, string> expected;

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
            this.expected = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
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
                Cleanup.DeleteDirectoryWithRetry(DictionaryLocation);
            }
        }

        /// <summary>
        /// Compare two empty dictionaries.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestEmptyDictionary()
        {
            DictionaryAssert.AreEqual(this.expected, this.actual);
        }

        /// <summary>
        /// Insert one item into the dictionary.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestInsert()
        {
            this.expected["foo"] = this.actual["foo"] = "1";
            this.expected["fOO"] = this.actual["Foo"] = "1";
            DictionaryAssert.AreEqual(this.expected, this.actual);
        }

        /// <summary>
        /// Insert one item into the dictionary, and Add() should through an exception.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestAddOfDuplicateWithDifferentCaseShouldThrow()
        {
            this.expected["new"] = this.actual["new"] = "1";
            try
            {
                this.expected.Add("NEW", "never!!!");
                Assert.Fail("Inserting a duplicate key should have thrown ArgumentException.");
            }
            catch (ArgumentException)
            {
            }

            try
            {
                this.actual.Add("NEW", "never!!!");
                Assert.Fail("Inserting a duplicate key should have thrown ArgumentException.");
            }
            catch (ArgumentException)
            {
            }

            DictionaryAssert.AreEqual(this.expected, this.actual);
        }

        /// <summary>
        /// Replace the item with a different case, but the key of the first item should be preserved.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestInsertPreservesCaseOfFirstItem()
        {
            this.expected["MixedCase"] = this.actual["MixedCase"] = "MixedCase";
            this.expected["mixedcase"] = this.actual["mixedcase"] = "lower";

            Assert.IsTrue(this.expected.ContainsKey("MixedCase"));
            Assert.IsTrue(this.expected.ContainsKey("mixedcase"));
            Assert.IsTrue(this.actual.ContainsKey("MixedCase"));
            Assert.IsTrue(this.actual.ContainsKey("mixedcase"));

            var expectedKeys = this.actual.Keys;
            var actualKeys = this.expected.Keys;

            Assert.IsTrue(actualKeys.Any(x => { return string.Equals(x, "MixedCase"); }));
            Assert.IsFalse(actualKeys.Any(x => { return string.Equals(x, "mixedcase"); }));
            //// Strange, I get a compile error for the regular Dictionary.
////            Assert.IsTrue(expectedKeys.Any(x => { return string.Equals(x, "MixedCase"); }));
////            Assert.IsTrue(expectedKeys.Any(x => { return string.Equals(x, "mixedcase"); }));

            DictionaryAssert.AreEqual(this.expected, this.actual);
            Assert.AreEqual("lower", this.expected["mIxEdCaSe"]);
        }

        /// <summary>
        /// Replace an item with different case of the key.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestReplaceWithDifferentCaseOfKey()
        {
            this.expected["foo"] = this.actual["foo"] = "1";
            this.expected["fOo"] = this.actual["FOO"] = "2";
            DictionaryAssert.AreEqual(this.expected, this.actual);
        }

        /// <summary>
        /// Delete an item using a different case.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestDeleteWithDifferentCase()
        {
            this.expected["foo"] = this.actual["foo"] = "1";
            this.expected["bar"] = this.actual["bar"] = "2";
            this.expected.Remove("Foo");
            Assert.IsTrue(this.actual.Remove("Foo"));
            DictionaryAssert.AreEqual(this.expected, this.actual);
        }

        /// <summary>
        /// Insert a KeyValuePair item into the dictionary and remove it.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestRemoveItem()
        {
            var item = new KeyValuePair<string, string>("thekey", "thevalue");
            var itemMixedCase = new KeyValuePair<string, string>("TheKey", "thevalue");
            this.expected.Add(item.Key, item.Value);
            this.actual.Add(item.Key, item.Value);
            ((ICollection<KeyValuePair<string, string>>)this.expected).Remove(itemMixedCase);

            Assert.IsTrue(this.actual.Remove(itemMixedCase));
            Assert.IsFalse(this.actual.Remove(item));

            DictionaryAssert.AreEqual(this.expected, this.actual);
        }

        /// <summary>
        /// Insert several items into the dictionary.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestAdds()
        {
            for (int i = 0xa; i < 0x1a; ++i)
            {
                this.expected.Add(i.ToString("x2"), i.ToString());
                this.actual.Add(i.ToString("x2"), i.ToString());
            }

            DictionaryAssert.AreEqual(this.expected, this.actual);
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
            DictionaryAssert.AreEqual(this.expected, this.actual);
        }

        /// <summary>
        /// Clear the dictionary.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestClear()
        {
            for (int i = 0xf7; i >= 0xf0; --i)
            {
                this.expected.Add(i.ToString("x2"), i.ToString());
                this.actual.Add(i.ToString("x2"), i.ToString());
            }

            this.expected.Clear();
            this.actual.Clear();
            DictionaryAssert.AreEqual(this.expected, this.actual);
        }

        /// <summary>
        /// Verifies that MakeKey is case-insensitive.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestMakeKeyIsInsensitive()
        {
            var cursor = this.actual.GetCursor();
            try
            {
                cursor.MakeKey("abc");
                byte[] keyLower = cursor.GetNormalizedKey();
                cursor.MakeKey("ABC");
                byte[] keyUpper = cursor.GetNormalizedKey();

                System.Console.WriteLine(
                    "keyLower=[{0}], keyUpper=[{1}]",
                    KeyTests.ByteArrayToString(keyLower),
                    KeyTests.ByteArrayToString(keyUpper));

                Assert.AreEqual(keyLower.Length, keyUpper.Length);
                EnumerableAssert.AreEqual(
                    keyLower,
                    keyUpper,
                    "Upper and lower case didn't normalize to the same values! keyLower=[{0}], keyUpper=[{1}]",
                    KeyTests.ByteArrayToString(keyLower),
                    KeyTests.ByteArrayToString(keyUpper));
            }
            finally
            {
                this.actual.FreeCursor(cursor);
            }
        }
    }
}
