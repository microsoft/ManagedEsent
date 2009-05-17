//-----------------------------------------------------------------------
// <copyright file="IndexcreateTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Isam.Esent.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
    /// <summary>
    /// Test conversion to NATIVE_COLUMNDEF
    /// </summary>
    [TestClass]
    public class IndexcreateTests
    {
        private JET_INDEXCREATE managed;

        /// <summary>
        /// The native conditional column structure created from the JET_INDEXCREATE
        /// object.
        /// </summary>
        private NATIVE_INDEXCREATE native;

        /// <summary>
        /// Setup the test fixture. This creates a native structure and converts
        /// it to a managed object.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.managed = new JET_INDEXCREATE()
                           {
                               szIndexName = "index",
                               szKey = "+foo\0-bar\0\0",
                               cbKey = 8,
                               grbit = CreateIndexGrbit.IndexSortNullsHigh,
                               ulDensity = 100,
                               pidxUnicode = null,
                               cbVarSegMac = 200,
                               rgconditionalcolumn = null,
                               cConditionalColumn = 0,
                           };
            this.native = this.managed.GetNativeIndexcreate();
        }

        /// <summary>
        /// Check the conversion to a NATIVE_INDEXCREATE sets the structure size
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyConversionToNativeSetsCbStruct()
        {
            Assert.AreEqual((uint)Marshal.SizeOf(this.native), this.native.cbStruct);
        }

        /// <summary>
        /// Check the conversion to a NATIVE_INDEXCREATE sets the name
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyConversionToNativeSetsName()
        {
            Assert.AreEqual("index", this.native.szIndexName);
        }

        /// <summary>
        /// Check the conversion to a NATIVE_INDEXCREATE sets the key
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyConversionToNativeSetsKey()
        {
            Assert.AreEqual("+foo\0-bar\0\0", this.native.szKey);
        }

        /// <summary>
        /// Check the conversion to a NATIVE_INDEXCREATE sets the key length
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyConversionToNativeSetsKeyLength()
        {
            Assert.AreEqual((uint) 8, this.native.cbKey);
        }

        /// <summary>
        /// Check the conversion to a NATIVE_INDEXCREATE sets the grbit
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyConversionToNativeSetsGrbit()
        {
            Assert.AreEqual((uint) CreateIndexGrbit.IndexSortNullsHigh, this.native.grbit);
        }

        /// <summary>
        /// Check the conversion to a NATIVE_INDEXCREATE sets the density
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyConversionToNativeSetsDensity()
        {
            Assert.AreEqual((uint) 100, this.native.ulDensity);
        }

        /// <summary>
        /// Check the conversion to a NATIVE_INDEXCREATE sets the JET_UNICODEINDEX
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyConversionToNativeSetsUnicodeIndexToNull()
        {
            Assert.AreEqual(IntPtr.Zero, this.native.pidxUnicode);
        }

        /// <summary>
        /// Check the conversion to a NATIVE_INDEXCREATE sets the cbVarSegMac
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyConversionToNativeSetsCbVarSegMac()
        {
            Assert.AreEqual(new IntPtr(200), this.native.cbVarSegMac);
        }

        /// <summary>
        /// Check the conversion to a NATIVE_INDEXCREATE sets the JET_CONDITIONALCOLUMNs
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyConversionToNativeSetsConditionalColumnsToNull()
        {
            Assert.AreEqual(IntPtr.Zero, this.native.rgconditionalcolumn);
        }

        /// <summary>
        /// Check the conversion to a NATIVE_INDEXCREATE sets the cbVarSegMac
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyConversionToNativeSetsCConditionalColumn()
        {
            Assert.AreEqual((uint) 0, this.native.cConditionalColumn);
        }
    }
}