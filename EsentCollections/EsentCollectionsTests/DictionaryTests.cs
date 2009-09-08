//-----------------------------------------------------------------------
// <copyright file="DictionaryTests.cs" company="Microsoft Corporation">
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
    /// Test the PersistentDictionary.
    /// </summary>
    [TestClass]
    public class DictionaryTests
    {
        /// <summary>
        /// Where the dictionary will be located.
        /// </summary>
        private const string DictionaryLocation = "DictionaryFixture";

        /// <summary>
        /// The dictionary we are testing.
        /// </summary>
        private PersistentDictionary<DateTime, Guid?> dictionary;

        /// <summary>
        /// Test initialization.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.dictionary = new PersistentDictionary<DateTime, Guid?>(DictionaryLocation);
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
                Directory.Delete(DictionaryLocation, true);
            }
        }

        /// <summary>
        /// A PersistentDictionary is read-write.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyDatabasePropertyReturnsDatabaseDirectory()
        {
            Assert.AreEqual(DictionaryLocation, this.dictionary.Database);
        }

        /// <summary>
        /// A PersistentDictionary is read-write.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyDictionaryIsNotReadOnly()
        {
            Assert.IsFalse(this.dictionary.IsReadOnly);
        }

        /// <summary>
        /// A PersistentDictionary's Key collection is read-only.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyDictionaryKeysAreReadOnly()
        {
            CheckCollectionIsReadOnly(this.dictionary.Keys);
        }

        /// <summary>
        /// A PersistentDictionary's Value collection is read-only.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyDictionaryValuesAreReadOnly()
        {
            CheckCollectionIsReadOnly(this.dictionary.Values);
        }

        /// <summary>
        /// Contains should return false for items that don't exist.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyContainsReturnsFalseWhenItemIsNotPresent()
        {
            var item = new KeyValuePair<DateTime, Guid?>(DateTime.Now, Guid.NewGuid());
            Assert.IsFalse(this.dictionary.Contains(item));
        }

        /// <summary>
        /// ContainsKey should return false for keys that don't exist.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyTryGetValueReturnsFalseWhenKeyIsNotPresent()
        {
            Guid? v;
            Assert.IsFalse(this.dictionary.TryGetValue(DateTime.Now, out v));
        }

        /// <summary>
        /// ContainsKey should return false for keys that don't exist.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyContainsKeyReturnsFalseWhenKeyIsNotPresent()
        {
            Assert.IsFalse(this.dictionary.ContainsKey(DateTime.Now));
        }

        /// <summary>
        /// Keys.Contains should return false for keys that don't exist.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyKeysContainsReturnsFalseWhenKeyIsNotPresent()
        {
            Assert.IsFalse(this.dictionary.Keys.Contains(DateTime.Now));
        }

        /// <summary>
        /// ContainsValue should return false for values that don't exist.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyContainsValueReturnsFalseWhenValueIsNotPresent()
        {
            Assert.IsFalse(this.dictionary.ContainsValue(Guid.Empty));
        }

        /// <summary>
        /// ContainsValue should return false for values that don't exist.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyValuesContainsReturnsFalseWhenValueIsNotPresent()
        {
            Assert.IsFalse(this.dictionary.Values.Contains(Guid.Empty));
        }

        /// <summary>
        /// Contains should return false when the value doesn't match.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyContainsItemReturnsFalseWhenValueDoesNotMatch()
        {
            var item = new KeyValuePair<DateTime, Guid?>(DateTime.Now, Guid.NewGuid());
            this.dictionary.Add(item);
            var otherItem = new KeyValuePair<DateTime, Guid?>(item.Key, Guid.NewGuid());
            Assert.IsFalse(this.dictionary.Contains(otherItem));
        }

        /// <summary>
        /// Remove should return false for keys that don't exist.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyRemoveReturnsFalseWhenKeyIsNotPresent()
        {
            Assert.IsFalse(this.dictionary.Remove(DateTime.Now));
        }

        /// <summary>
        /// Remove should return false for items that don't exist.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyRemoveItemReturnsFalseWhenItemIsNotPresent()
        {
            var item = new KeyValuePair<DateTime, Guid?>(DateTime.Now, Guid.NewGuid());
            Assert.IsFalse(this.dictionary.Remove(item));
        }

        /// <summary>
        /// Remove should return false (and not remove anything) when
        /// the value doesn't match.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyRemoveItemReturnsFalseWhenValueDoesNotMatch()
        {
            var item = new KeyValuePair<DateTime, Guid?>(DateTime.Now, Guid.NewGuid());
            this.dictionary.Add(item);
            var itemToRemove = new KeyValuePair<DateTime, Guid?>(item.Key, Guid.NewGuid());
            Assert.IsFalse(this.dictionary.Remove(itemToRemove));
            Assert.AreEqual(item.Value, this.dictionary[item.Key]);
        }

        /// <summary>
        /// Remove should return true (and remove the item) when
        /// the value does match.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyRemoveItemReturnsTrueWhenValueDoesMatch()
        {
            var item = new KeyValuePair<DateTime, Guid?>(DateTime.Now, Guid.NewGuid());
            this.dictionary.Add(item);
            Assert.IsTrue(this.dictionary.Remove(item));
            Assert.IsFalse(this.dictionary.Contains(item));
        }

        /// <summary>
        /// Calling Add() with a duplicate key throws an exception.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentException))]
        public void VerifyAddThrowsExceptionForDuplicateKey()
        {
            var k = DateTime.UtcNow;
            var v = Guid.NewGuid();
            this.dictionary.Add(k, v);
            this.dictionary.Add(k, v);
        }

        /// <summary>
        /// Calling Add() with a duplicate item throws an exception.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentException))]
        public void VerifyAddItemThrowsExceptionForDuplicateKey()
        {
            var k = DateTime.UtcNow;
            var v = Guid.NewGuid();
            this.dictionary.Add(k, v);
            this.dictionary.Add(new KeyValuePair<DateTime, Guid?>(k, Guid.NewGuid()));
        }

        /// <summary>
        /// Getting a value throws an exception if the key isn't found.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void VerifyGettingValueThrowsExceptionIfKeyIsNotPresent()
        {
            var ignored = this.dictionary[DateTime.MaxValue];
        }

        /// <summary>
        /// Getting the last key throws an exception if the dictionary is empty.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void VerifyGetLastKeyThrowsExceptionIfDictionaryIsEmpty()
        {
            this.dictionary.GetLastKey();
        }

        /// <summary>
        /// Getting the last key returns the last key.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyGetLastKeyReturnsLastKey()
        {
            var key = DateTime.Now;
            this.dictionary[key] = Guid.NewGuid();
            Assert.AreEqual(key, this.dictionary.GetLastKey());
        }


        /// <summary>
        /// Exercise the Flush code path.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void FlushDatabase()
        {
            this.dictionary.Add(DateTime.Now, Guid.NewGuid());
            this.dictionary.Flush();
        }

        /// <summary>
        /// This dictionary has a nullable value. Set a value to null.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void VerifyValueCanBeNull()
        {
            var key = DateTime.Now;
            this.dictionary[key] = null;
            Assert.IsNull(this.dictionary[key]);
        }

        /// <summary>
        /// PersistentDatabaseFile.Exists should return true for this database.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void ExistsReturnsTrue()
        {
            Assert.IsTrue(PersistentDictionaryFile.Exists(DictionaryLocation));
        }

        /// <summary>
        /// Retrieve the first element from an empty dictionary. An exception
        /// should be thrown.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(InvalidOperationException))]
        public void VerifyFirstThrowsExceptionWhenDictionaryIsEmpty()
        {
            var ignored = this.dictionary.First();
        }

        /// <summary>
        /// Retrieve the first element from the keys of an empty dictionary. An exception
        /// should be thrown.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(InvalidOperationException))]
        public void VerifyFirstKeyThrowsExceptionWhenDictionaryIsEmpty()
        {
            var ignored = this.dictionary.Keys.First();
        }

        /// <summary>
        /// Retrieve the last element from an empty dictionary. An exception
        /// should be thrown.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(InvalidOperationException))]
        public void VerifyLastThrowsExceptionWhenDictionaryIsEmpty()
        {
            var ignored = this.dictionary.Last();
        }

        /// <summary>
        /// Retrieve the last element from the keys of an empty dictionary. An exception
        /// should be thrown.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(InvalidOperationException))]
        public void VerifyLastKeyThrowsExceptionWhenDictionaryIsEmpty()
        {
            var ignored = this.dictionary.Keys.Last();
        }

        /// <summary>
        /// Make sure the given collection is read-only.
        /// </summary>
        /// <typeparam name="T">The type of the collection.</typeparam>
        /// <param name="collection">The collection to check.</param>
        private static void CheckCollectionIsReadOnly<T>(ICollection<T> collection)
        {
            // IsReadOnly is true
            Assert.IsTrue(collection.IsReadOnly);

            // Add() throws an exception
            try
            {
                collection.Add(default(T));
                Assert.Fail("Should have thrown a NotSupportedException");
            }
            catch (NotSupportedException)
            {
            }

            // Remove() throws an exception
            try
            {
                collection.Remove(default(T));
                Assert.Fail("Should have thrown a NotSupportedException");
            }
            catch (NotSupportedException)
            {
            }

            // Clear() throws an exception
            try
            {
                collection.Clear();
                Assert.Fail("Should have thrown a NotSupportedException");
            }
            catch (NotSupportedException)
            {
            }
        }
    }
}