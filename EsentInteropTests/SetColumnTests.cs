//-----------------------------------------------------------------------
// <copyright file="SetColumnTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test conversion to NATIVE_SETCOLUMN
    /// </summary>
    [TestClass]
    public class SetColumnTests
    {
        /// <summary>
        /// The managed version of the struct.
        /// </summary>
        private JET_SETCOLUMN managed;

        /// <summary>
        /// The native structure created from the managed version.
        /// </summary>
        private NATIVE_SETCOLUMN native;

        /// <summary>
        /// Setup the test fixture. This creates a native structure and converts
        /// it to a managed object.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.managed = new JET_SETCOLUMN
            {
                cbData = 1,
                columnid = new JET_COLUMNID { Value = 2 },
                grbit = SetColumnGrbit.AppendLV,
                ibLongValue = 3,
                itagSequence = 4,
            };
            this.native = this.managed.GetNativeSetcolumn();
        }

        /// <summary>
        /// CheckDataSize should detect a negative data length.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyCheckThrowsExceptionWhenCbDataIsNegative()
        {
            var setcolumn = new JET_SETCOLUMN
            {
                cbData = -1,
                pvData = new byte[1],
            };
            setcolumn.CheckDataSize();
        }

        /// <summary>
        /// CheckDataSize should detect null pvData and non-zero cbData.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyCheckThrowsExceptionWhenPvDataIsNull()
        {
            var setcolumn = new JET_SETCOLUMN
            {
                cbData = 1,
            };
            setcolumn.CheckDataSize();
        }

        /// <summary>
        /// CheckDataSize should detect cbData that is too long.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyCheckThrowsExceptionWhenCbDataIsTooLong()
        {
            var setcolumn = new JET_SETCOLUMN
            {
                cbData = 100,
                pvData = new byte[9],
            };
            setcolumn.CheckDataSize();
        }

        /// <summary>
        /// Check the conversion to a native structure sets the cbData
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyConversionToNativeSetsCbData()
        {
            Assert.AreEqual((uint)1, this.native.cbData);
        }

        /// <summary>
        /// Check the conversion to a native structure sets the columnid
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyConversionToNativeSetsColumnid()
        {
            Assert.AreEqual((uint)2, this.native.columnid);
        }

        /// <summary>
        /// Check the conversion to a native structure sets the grbit
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyConversionToNativeSetsGrbit()
        {
            Assert.AreEqual((uint)SetColumnGrbit.AppendLV, this.native.grbit);
        }

        /// <summary>
        /// Check the conversion to a native structure sets the ibLongValue
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyConversionToNativeSetsIbLongValue()
        {
            Assert.AreEqual((uint)3, this.native.ibLongValue);
        }

        /// <summary>
        /// Check the conversion to a native structure sets the itagSequence
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyConversionToNativeSetsItagSequence()
        {
            Assert.AreEqual((uint)4, this.native.itagSequence);
        }

        /// <summary>
        /// Check the conversion to a native structure sets the pvData
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyConversionToNativeDoesNotSetPvData()
        {
            Assert.AreEqual(IntPtr.Zero, this.native.pvData);
        }
    }
}