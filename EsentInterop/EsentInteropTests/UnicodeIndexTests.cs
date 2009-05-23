//-----------------------------------------------------------------------
// <copyright file="UnicodeIndexTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Isam.Esent.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
    /// <summary>
    /// Test conversion to NATIVE_COLUMNDEF
    /// </summary>
    [TestClass]
    public class UnicodeIndexTests
    {
        private JET_UNICODEINDEX managed;

        /// <summary>
        /// The native conditional column structure created from the JET_UNICODEINDEX
        /// object.
        /// </summary>
        private NATIVE_UNICODEINDEX native;

        /// <summary>
        /// Setup the test fixture. This creates a native structure and converts
        /// it to a managed object.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.managed = new JET_UNICODEINDEX()
            {
                lcid = 1033,
                dwMapFlags = 0x400,
            };
            this.native = this.managed.GetNativeUnicodeIndex();
        }

        /// <summary>
        /// Check the conversion to a NATIVE_INDEXCREATE sets the map flags
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyConversionToNativeSetsDwMapFlags()
        {
            Assert.AreEqual((uint)0x400, this.native.dwMapFlags);
        }

        /// <summary>
        /// Check the conversion to a NATIVE_INDEXCREATE sets the lcid
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyConversionToNativeSetsLcid()
        {
            Assert.AreEqual((uint)1033, this.native.lcid);
        }
    }
}