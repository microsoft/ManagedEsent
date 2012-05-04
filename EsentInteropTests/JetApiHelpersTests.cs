//-----------------------------------------------------------------------
// <copyright file="JetApiHelpersTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using Microsoft.Isam.Esent.Interop.Implementation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for helper methods in the JetApi class.
    /// </summary>
    [TestClass]
    public class JetApiHelpersTests
    {
        /// <summary>
        /// Verify GetActualSize returns 0 when passed 0.
        /// </summary>
        [TestMethod]
        [Description("Verify GetActualSize returns 0 when passed 0")]
        [Priority(0)]
        public void VerifyGetActualSizeReturnsZeroForZero()
        {
            Assert.AreEqual(0, JetApi.GetActualSize(0U));
        }

        /// <summary>
        /// Verify GetActualSize returns a positive number passed to it.
        /// </summary>
        [TestMethod]
        [Description("Verify GetActualSize returns a positive number passed to it")]
        [Priority(0)]
        public void VerifyGetActualSizeReturnsPositiveNumber()
        {
            Assert.AreEqual(17, JetApi.GetActualSize(17U));
        }

        /// <summary>
        /// Verify GetActualSize throws exception on overflow.
        /// </summary>
        [TestMethod]
        [Description("Verify GetActualSize throws an exception on overflow")]
        [Priority(0)]
        [ExpectedException(typeof(OverflowException))]
        public void VerifyGetActualSizeThrowsExceptionOnOverflow()
        {
            int ignored = JetApi.GetActualSize(uint.MaxValue);
        }

        /// <summary>
        /// Verify GetActualSize returns 0 for debug fill.
        /// </summary>
        [TestMethod]
        [Description("Verify GetActualSize returns 0 for debug fill")]
        [Priority(0)]
        public void VerifyGetActualSizeReturnsZeroForDebugFill()
        {
            Assert.AreEqual(0, JetApi.GetActualSize(0xDDDDDDDD));
        }
    }
}