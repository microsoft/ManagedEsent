//-----------------------------------------------------------------------
// <copyright file="UtilityMethodsTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Testing the utility methods used in this test framework.
    /// </summary>
    [TestClass]
    public class UtilityMethodsTests
    {
        /// <summary>
        /// Check that Any.Bytes returns an array of at least 1 byte.
        /// </summary>
        [TestMethod]
        public void TestAnyBytesIsAtLeastOneByte()
        {
            var bytes = Any.Bytes;
            Assert.IsTrue(bytes.Length >= 1);
        }

        /// <summary>
        /// Check that Any.Bytes returns an array of no more than 255 bytes.
        /// </summary>
        [TestMethod]
        public void TestAnyBytesIsNoMoreThan255Bytes()
        {
            var bytes = Any.Bytes;
            Assert.IsTrue(bytes.Length <= 255);
        }

        /// <summary>
        /// Check that Any.String returns a string of at least 1 character.
        /// </summary>
        [TestMethod]
        public void TestAnyStringIsAtLeastOneCharacter()
        {
            var s = Any.String;
            Assert.IsTrue(s.Length >= 1);
        }

        /// <summary>
        /// Check that Any.String returns a string of no more than 120 characters.
        /// </summary>
        [TestMethod]
        public void TestAnyStringIsNoMoreThan120Characters()
        {
            var s = Any.String;
            Assert.IsTrue(s.Length <= 120);
        }

        /// <summary>
        /// Check that Any.String returns a string of ASCII characters.
        /// </summary>
        [TestMethod]
        public void TestAnyStringIsASCIICharacters()
        {
            var s = Any.String;
            foreach (char c in s)
            {
                Assert.IsTrue(c <= '~');    // last ASCII character (127)
                Assert.IsTrue(c >= ' ');    // first ASCII character (32);
            }
        }
    }
}