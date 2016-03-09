//-----------------------------------------------------------------------
// <copyright file="UnicodeIndex2Tests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;

    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test conversion to NATIVE_COLUMNDEF
    /// </summary>
    [TestClass]
    public class UnicodeIndex2Tests
    {
        /// <summary>
        /// The LocaleName to use for this test.
        /// </summary>
        private const string LocaleName = "pt-br";

        /// <summary>
        /// Managed object being tested.
        /// </summary>
        private JET_UNICODEINDEX managed;

        /// <summary>
        /// The native conditional column structure created from the JET_UNICODEINDEX
        /// object.
        /// </summary>
        private NATIVE_UNICODEINDEX2 native;

        /// <summary>
        /// Setup the test fixture. This creates a native structure and converts
        /// it to a managed object.
        /// </summary>
        [TestInitialize]
        [Description("Setup the UnicodeIndex2Tests fixture")]
        public void Setup()
        {
            this.managed = new JET_UNICODEINDEX()
            {
                szLocaleName = LocaleName,
                dwMapFlags = 0x400,
            };
            this.native = this.managed.GetNativeUnicodeIndex2();
        }

        /// <summary>
        /// Verify GetEffectiveLocaleName() works with null.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify GetEffectiveLocaleName() works with null.")]
        public void VerifyGetEffectiveLocaleNameWorksWithNull()
        {
            var unicodeIndex = new JET_UNICODEINDEX()
            {
                szLocaleName = null,
                dwMapFlags = 0x400,
            };
            Assert.AreEqual(null, unicodeIndex.GetEffectiveLocaleName());
        }

        /// <summary>
        /// Verify GetEffectiveLocaleName() works with empty string.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify GetEffectiveLocaleName() works with empty string.")]
        public void VerifyGetEffectiveLocaleNameWorksWithEmptyString()
        {
            var unicodeIndex = new JET_UNICODEINDEX()
            {
                szLocaleName = string.Empty,
                dwMapFlags = 0x400,
            };
            Assert.AreEqual(string.Empty, unicodeIndex.GetEffectiveLocaleName());
        }

        /// <summary>
        /// Verify GetEffectiveLocaleName() works with LCID 127.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify GetEffectiveLocaleName() works with LCID 127.")]
        public void VerifyGetEffectiveLocaleNameWorksWithLcid127()
        {
            var unicodeIndex = new JET_UNICODEINDEX()
            {
                lcid = 127,
                dwMapFlags = 0x400,
            };
            Assert.AreEqual(string.Empty, unicodeIndex.GetEffectiveLocaleName());
        }

        /// <summary>
        /// Verify GetEffectiveLocaleName() works with LCID 1033.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify GetEffectiveLocaleName() works with LCID 1033.")]
        public void VerifyGetEffectiveLocaleNameWorksWithLcid1033()
        {
            var unicodeIndex = new JET_UNICODEINDEX()
            {
                lcid = 1033,
                dwMapFlags = 0x400,
            };
            Assert.AreEqual("en-us", unicodeIndex.GetEffectiveLocaleName());
        }

        /// <summary>
        /// Verify GetEffectiveLocaleName() works with LCID not in cache.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify GetEffectiveLocaleName() works with LCID not in cache.")]
        public void VerifyGetEffectiveLocaleNameWorksWithLcidNotInCache()
        {
            var unicodeIndex = new JET_UNICODEINDEX()
            {
                lcid = 0x0477, // "la-Latn"
                dwMapFlags = 0x400,
            };
            Assert.AreEqual(null, unicodeIndex.GetEffectiveLocaleName());
        }

        /// <summary>
        /// Check the conversion to a NATIVE_UNICODEINDEX2 sets the map flags
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that converting a JET_UNICODEINDEX to a NATIVE_UNICODEINDEX2 sets the map flags")]
        public void VerifyConversionToNativeSetsDwMapFlags()
        {
            Assert.AreEqual((uint)0x400, this.native.dwMapFlags);
        }

        /// <summary>
        /// Check the conversion to a NATIVE_UNICODEINDEX2 throws when an LCID is specified.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that converting a JET_UNICODEINDEX throws when an LCID is specified.")]
        public void VerifyUnicodeIndexConversionToNative2ThrowsWithLcid()
        {
            var unicodeIndexWithLcid = new JET_UNICODEINDEX()
            {
                lcid = 1001,
                dwMapFlags = 0x30403,
            };
            try
            {
                NATIVE_UNICODEINDEX2 unicodeindex2 = unicodeIndexWithLcid.GetNativeUnicodeIndex2();
                Assert.Fail("The conversion should have thrown an ArgumentException!");
            }
            catch (ArgumentException)
            {
            }
        }

        /// <summary>
        /// Check the conversion to a NATIVE_UNICODEINDEX throws when a locale name is specified.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that converting a JET_UNICODEINDEX to a NATIVE_UNICODEINDEX throws when a locale name is specified.")]
        public void VerifyUnicodeIndexConversionToNativeThrowsWithLcid()
        {
            var unicodeIndexWithLcid = new JET_UNICODEINDEX()
            {
                szLocaleName = "de-de",
                dwMapFlags = 0x30403,
            };
            try
            {
                NATIVE_UNICODEINDEX unicodeindex = unicodeIndexWithLcid.GetNativeUnicodeIndex();
                Assert.Fail("The conversion should have thrown an ArgumentException!");
            }
            catch (ArgumentException)
            {
            }
        }
    }
}