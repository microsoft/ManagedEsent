//-----------------------------------------------------------------------
// <copyright file="UtilityMethodsTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
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
        [Priority(0)]
        public void TestAnyBytesIsAtLeastOneByte()
        {
            byte[] bytes = Any.Bytes;
            Assert.IsTrue(bytes.Length >= 1);
        }

        /// <summary>
        /// Check that Any.Bytes returns an array of no more than 255 bytes.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void TestAnyBytesIsNoMoreThan255Bytes()
        {
            byte[] bytes = Any.Bytes;
            Assert.IsTrue(bytes.Length <= 255);
        }

        /// <summary>
        /// Check that Any.BytesOfLength returns a string of the correct length.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void TestAnyBytesOfLengthIsCorrectLength()
        {
            byte[] s = Any.BytesOfLength(20);
            Assert.AreEqual(20, s.Length);
        }

        /// <summary>
        /// Check that Any.String returns a string of at least 1 character.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void TestAnyStringIsAtLeastOneCharacter()
        {
            string s = Any.String;
            Assert.IsTrue(s.Length >= 1);
        }

        /// <summary>
        /// Check that Any.String returns a string of no more than 120 characters.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void TestAnyStringIsNoMoreThan120Characters()
        {
            string s = Any.String;
            Assert.IsTrue(s.Length <= 120);
        }

        /// <summary>
        /// Check that Any.String returns a string of ASCII characters.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void TestAnyStringIsASCIICharacters()
        {
            string s = Any.String;
            foreach (char c in s)
            {
                Assert.IsTrue(c <= '~');    // last ASCII character (127)
                Assert.IsTrue(c >= ' ');    // first ASCII character (32);
            }
        }

        /// <summary>
        /// Check that Any.StringOfLength returns a string of the correct length.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void TestAnyStringOfLengthIsCorrectLength()
        {
            string s = Any.StringOfLength(10);
            Assert.AreEqual(10, s.Length);
        }
    }
}