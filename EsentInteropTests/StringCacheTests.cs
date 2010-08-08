//-----------------------------------------------------------------------
// <copyright file="StringCacheTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Text;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the StringCache class.
    /// </summary>
    [TestClass]
    public class StringCacheTests
    {
        /// <summary>
        /// Try to intern a random string (should not be interned).
        /// </summary>
        [TestMethod]
        [Description("Try to intern a random string (should not be interned)")]
        [Priority(0)]
        public void TryToInternRandomString()
        {
            string s = StringCache.TryToIntern(Any.String);
            Assert.IsNull(String.IsInterned(s), "Should not have been interned");
        }

        /// <summary>
        /// Try to intern an interned string.
        /// </summary>
        [TestMethod]
        [Description("Try to intern an interned string")]
        [Priority(0)]
        public void TryToInternInternedString()
        {
            string s = String.Intern(StringCache.TryToIntern(Any.String));
            Assert.IsNotNull(String.IsInterned(s), "Should not have been interned");
        }

        /// <summary>
        /// Get a string with a null buffer.
        /// </summary>
        [TestMethod]
        [Description("Test StringCache.GetString with a null buffer")]
        [Priority(0)]
        public void GetStringWithNull()
        {
            byte[] buffer = null;
            Assert.AreEqual(String.Empty, StringCache.GetString(buffer, 0, 0));
        }

        /// <summary>
        /// Get a string with a null buffer.
        /// </summary>
        [TestMethod]
        [Description("Test StringCache.GetString")]
        [Priority(0)]
        public void GetString()
        {
            byte[] buffer = Encoding.Unicode.GetBytes("Hello");
            Assert.AreEqual("Hello", StringCache.GetString(buffer, 0, buffer.Length));
        }
    }
}