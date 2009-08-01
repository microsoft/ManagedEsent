//-----------------------------------------------------------------------
// <copyright file="EnumColumnTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Microsoft.Isam.Esent.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
    /// <summary>
    /// Tests for JET_ENUMCOLUMN conversion.
    /// </summary>
    [TestClass]
    public class EnumColumnTests
    {
        /// <summary>
        /// Test conversion of a single value
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void TestSingleValueConversion()
        {
            var native = new NATIVE_ENUMCOLUMN
            {
                columnid = 1,
                err = (int) JET_wrn.ColumnSingleValue,
                cEnumColumnValue = 3,
                rgEnumColumnValue = new IntPtr(4),
            };

            var managed = new JET_ENUMCOLUMN();
            managed.SetFromNativeEnumColumn(native);

            Assert.AreEqual<uint>(1, managed.columnid.Value);
            Assert.AreEqual(JET_wrn.ColumnSingleValue, managed.err);
            Assert.AreEqual(3, managed.cbData);
            Assert.AreEqual(new IntPtr(4), managed.pvData);
        }
    }
}
