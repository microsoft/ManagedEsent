// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeyTests.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//   Test the Key class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EsentCollectionsTests
{
    using System.Text;

    using Microsoft.Isam.Esent.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the Key class.
    /// </summary>
    [TestClass]
    public class KeyTests
    {
        /// <summary>
        /// Converts a byte array to a nicer string.
        /// </summary>
        /// <param name="byteArray">Byte array to convert to a 
        /// string.</param> 
        /// <returns>A string representation of a byte array.</returns>
        public static string ByteArrayToString(byte[] byteArray)
        {
            StringBuilder sb = new StringBuilder(40);

            if (byteArray == null)
            {
                return string.Empty;
            }

            for (int i = 0; i < byteArray.Length - 1; ++i)
            {
                sb.AppendFormat("{0:x2},", byteArray[i]);
            }

            if (byteArray.Length > 0)
            {
                sb.AppendFormat("{0:x2}", byteArray[byteArray.Length - 1]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Verifies the string conversion of a null array.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyByteArryToStringOnNullArray()
        {
            byte[] nullArray = null;

            Assert.AreEqual(string.Empty, ByteArrayToString(nullArray));
        }

        /// <summary>
        ///  Verifies the string conversion of an empty array.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyByteArryToStringOnEmptyArray()
        {
            byte[] emptyArray = new byte[] { };

            Assert.AreEqual(string.Empty, ByteArrayToString(emptyArray));
        }

        /// <summary>
        /// Verifiees the conversion of an array with one element.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyByteArryToStringOnOneElmement()
        {
            byte[] oneArray = new byte[] { 0x7a };

            Assert.AreEqual("7a", ByteArrayToString(oneArray));
        }

        /// <summary>
        /// Verifies the string conversion of an array with two elements.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyByteArryToStringOnTwoElements()
        {
            byte[] doubleArray = new byte[] { 0x43, 0xab };

            Assert.AreEqual("43,ab", ByteArrayToString(doubleArray));
        }

        /// <summary>
        /// Verifies the string conversion of an array with many elements.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyByteArryToStringOnManyElements()
        {
            byte[] largeArray = new byte[] { 0x43, 0xab, 0x0a, 0xff, 0xeb, 0xde, 0xad, 0xbe, 0xef };

            Assert.AreEqual("43,ab,0a,ff,eb,de,ad,be,ef", ByteArrayToString(largeArray));
        }

        /// <summary>
        /// Verify the key constructor sets the members.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify the key constructor sets the members")]
        public void VerifyKeyConstructorSetsMembers()
        {
            var key = Key<int>.CreateKey(7, true);
            Assert.AreEqual(7, key.Value);
            Assert.IsTrue(key.IsInclusive);
        }

        /// <summary>
        /// Verify Equals(null) returns false.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify Key.Equals(null) returns false")]
        public void VerifyKeyEqualsNullIsFalse()
        {
            var key = Key<int>.CreateKey(0, false);
            Assert.IsFalse(key.Equals(null));
        }

        /// <summary>
        /// Verify a key equals itself.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify a key equals itself")]
        public void VerifyKeyEqualsSelf()
        {
            var key = Key<int>.CreateKey(0, false);
            EqualityAsserts.TestEqualsAndHashCode(key, key, true);
        }

        /// <summary>
        /// Verify a key equals a key with the same value.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify a key equals a key with the same value")]
        public void VerifyKeyEqualsSameValue()
        {
            var key1 = Key<int>.CreateKey(1, true);
            var key2 = Key<int>.CreateKey(1, true);
            EqualityAsserts.TestEqualsAndHashCode(key1, key2, true);
        }

        /// <summary>
        /// Verify a key does not equal a key with different inclusive setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify a key does not equal a key with different inclusive setting")]
        public void VerifyKeyNotEqualDifferentInclusive()
        {
            var key1 = Key<int>.CreateKey(2, true);
            var key2 = Key<int>.CreateKey(2, false);
            EqualityAsserts.TestEqualsAndHashCode(key1, key2, false);
        }

        /// <summary>
        /// Verify a key does not equal a prefix key.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify a key does not equal a prefix key")]
        public void VerifyKeyNotEqualPrefixKey()
        {
            var key1 = Key<string>.CreateKey("foo", true);
            var key2 = Key<string>.CreatePrefixKey("foo");
            EqualityAsserts.TestEqualsAndHashCode(key1, key2, false);
        }

        /// <summary>
        /// Verify a key does not equal a key with different value.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify a key does not equal a key with different value")]
        public void VerifyKeyNotEqualDifferentValue()
        {
            var key1 = Key<int>.CreateKey(3, false);
            var key2 = Key<int>.CreateKey(4, false);
            EqualityAsserts.TestEqualsAndHashCode(key1, key2, false);
        }

        /// <summary>
        /// Verify the Key.Equals returns false for null object.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify the Key.Equals returns false for null object")]
        public void VerifyKeyEqualsNullOnjectIsFalse()
        {
            object obj = null;
            Assert.IsFalse(Key<double>.CreateKey(0, false).Equals(obj));
        }

        /// <summary>
        /// Verify the Key.Equals returns false for an object of a different type
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify the Key.Equals returns false for object of a different type")]
        public void VerifyKeyEqualsDifferentTypeIsFalse()
        {
            object obj = new object();
            Assert.IsFalse(Key<double>.CreateKey(0, false).Equals(obj));
        }

        /// <summary>
        /// Verify the hash code for byte arrays is consistent.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify the hash code for byte arrays is consistent.")]
        public void VerifyHashCodeForByteArraysIsConsistent()
        {
            byte[] one = new byte[] { (byte)'a', (byte)'b', (byte)'x' };
            byte[] two = new byte[] { (byte)'a', (byte)'b', (byte)'x' };

            int oneHash = PersistentDictionaryMath.GetHashCodeForKey(one);
            int twoHash = PersistentDictionaryMath.GetHashCodeForKey(two);

            Assert.AreEqual(oneHash, twoHash);
        }

        /// <summary>
        /// Verify the hash code for different byte arrays is different.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify the hash code for different byte arrays is different.")]
        public void VerifyHashCodeForDifferentByteArraysIsDifferent()
        {
            byte[] one = new byte[] { (byte)'a', (byte)'b', (byte)'x' };
            byte[] two = new byte[] { (byte)'a', (byte)'B', (byte)'X' };

            int oneHash = PersistentDictionaryMath.GetHashCodeForKey(one);
            int twoHash = PersistentDictionaryMath.GetHashCodeForKey(two);

            Assert.AreNotEqual(oneHash, twoHash);
        }
    }
}