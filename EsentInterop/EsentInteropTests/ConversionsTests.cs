//-----------------------------------------------------------------------
// <copyright file="ConversionsTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System.Globalization;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the methods of the Conversions class
    /// </summary>
    [TestClass]
    public class ConversionsTests
    {
        /// <summary>
        /// Converting default (0) LCMapFlags should return CompareOptions.None.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void ConvertDefaultLCMapFlags()
        {
            Assert.AreEqual(CompareOptions.None, Conversions.CompareOptionsFromLCMapFlags(0));
        }

        /// <summary>
        /// Convert one LCMapFlag
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void ConvertOneLCMapFlag()
        {
            uint flags = 0x01; // NORM_IGNORECASE
            Assert.AreEqual(CompareOptions.IgnoreCase, Conversions.CompareOptionsFromLCMapFlags(flags));
        }

        /// <summary>
        /// Convert multiple LCMapFlags
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void ConvertMultipleLCMapFlags()
        {
            uint flags = 0x6; // NORM_IGNORENONSPACE | NORM_IGNORESYMBOLS
            Assert.AreEqual(
                CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols,
                Conversions.CompareOptionsFromLCMapFlags(flags));
        }

        /// <summary>
        /// Convert unknown LCMapFlags
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void ConvertUnknownLCMapFlags()
        {
            uint flags = 0x8020000; // NORM_LINGUISTIC_CASING | NORM_IGNOREWIDTH
            Assert.AreEqual(
                CompareOptions.IgnoreWidth,
                Conversions.CompareOptionsFromLCMapFlags(flags));
        }

        /// <summary>
        /// Converting CompareOptions.None should return 0.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void ConvertDefaultCompareOptions()
        {
            uint flags = 0;
            Assert.AreEqual(flags, Conversions.LCMapFlagsFromCompareOptions(CompareOptions.None));
        }

        /// <summary>
        /// Convert one CompareOption
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void ConvertOneCompareOption()
        {
            uint flags = 0x1; // NORM_IGNORECASE
            Assert.AreEqual(flags, Conversions.LCMapFlagsFromCompareOptions(CompareOptions.IgnoreCase));
        }

        /// <summary>
        /// Convert multiple CompareOptions
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void ConvertMultipleCompareOptions()
        {
            uint flags = 0x6; // NORM_IGNORENONSPACE | NORM_IGNORESYMBOLS
            Assert.AreEqual(flags, Conversions.LCMapFlagsFromCompareOptions(CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols));
        }

        /// <summary>
        /// Convert unknown CompareOption
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void ConvertUnknownCompareOptions()
        {
            uint flags = 0;
            Assert.AreEqual(flags, Conversions.LCMapFlagsFromCompareOptions(CompareOptions.Ordinal));
        }
    }
}