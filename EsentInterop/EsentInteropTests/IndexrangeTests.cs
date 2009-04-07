//-----------------------------------------------------------------------
// <copyright file="IndexrangeTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using Microsoft.Isam.Esent.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
    /// <summary>
    /// Tests for the NATIVE_INDEXRANGE class
    /// </summary>
    [TestClass]
    public class IndexrangeTests
    {
        /// <summary>
        /// The native index list that will be converted into a managed object.
        /// </summary>
        private NATIVE_INDEXRANGE native;

        /// <summary>
        /// The managed version of the native indexlist.
        /// </summary>
        private JET_TABLEID tableid;

        /// <summary>
        /// Setup the test fixture. This creates a native structure and converts
        /// it to a managed object.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.tableid = new JET_TABLEID { Value = new IntPtr(0x55) };
            this.native = NATIVE_INDEXRANGE.MakeIndexRangeFromTableid(this.tableid);
        }

        /// <summary>
        /// Check the conversion to a NATIVE_INDEXRANGE
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyMakeIndexRangeFromTableidSetsCbstruct()
        {
            Assert.AreEqual((uint)Marshal.SizeOf(this.native), this.native.cbStruct);
        }

        /// <summary>
        /// Check the conversion to a NATIVE_INDEXRANGE
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyMakeIndexRangeFromTableidSetsTableid()
        {
            Assert.AreEqual(this.tableid.Value, this.native.tableid);
        }

        /// <summary>
        /// Check the conversion to a NATIVE_INDEXRANGE
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyMakeIndexRangeFromTableidSetsGrbit()
        {
            const uint JET_bitRecordInIndex = 0x1;
            Assert.AreEqual(JET_bitRecordInIndex, this.native.grbit);
        }
    }
}