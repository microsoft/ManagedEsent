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
    using Microsoft.Isam.Esent.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the Key class.
    /// </summary>
    [TestClass]
    public class KeyTests
    {
        /// <summary>
        /// Verify the key constructor sets the members.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify the key constructor sets the members")]
        public void VerifyKeyConstructorSetsMembers()
        {
            var key = new Key<int>(7, true);
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
            var key = new Key<int>(0, false);
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
            var key = new Key<int>(0, false);
            Assert.IsTrue(key.Equals(key));
        }

        /// <summary>
        /// Verify a key equals a key with the same value.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify a key equals a key with the same value")]
        public void VerifyKeyEqualsSameValue()
        {
            var key1 = new Key<int>(1, true);
            var key2 = new Key<int>(1, true);
            Assert.IsTrue(key1.Equals(key2));
        }

        /// <summary>
        /// Verify a key does not equal a key with different inclusive setting.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify a key does not equal a key with different inclusive setting")]
        public void VerifyKeyNotEqualDifferentInclusive()
        {
            var key1 = new Key<int>(2, true);
            var key2 = new Key<int>(2, false);
            Assert.IsFalse(key1.Equals(key2));
        }

        /// <summary>
        /// Verify a key does not equal a key with different value.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify a key does not equal a key with different value")]
        public void VerifyKeyNotEqualDifferentValue()
        {
            var key1 = new Key<int>(3, false);
            var key2 = new Key<int>(4, false);
            Assert.IsFalse(key1.Equals(key2));
        }
    }
}