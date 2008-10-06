//-----------------------------------------------------------------------
// <copyright file="ColumndefTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// JET_COLUMNDEF tests
    /// </summary>
    [TestClass]
    public class ColumndefTests
    {
        /// <summary>
        /// Test conversion to the native stuct
        /// </summary>
        [TestMethod]
        public void ConvertToNativeTest()
        {
            var columndef = new JET_COLUMNDEF();
            columndef.cbMax     = 0x1;
            columndef.coltyp    = JET_coltyp.Binary;
            columndef.cp        = JET_CP.Unicode;
            columndef.grbit     = ColumndefGrbit.ColumnAutoincrement;

            var native = columndef.GetNativeColumndef();
            Assert.AreEqual<uint>(0, native.columnid);
            Assert.AreEqual<uint>(9, native.coltyp);
            Assert.AreEqual<ushort>(0, native.wCountry);
            Assert.AreEqual<ushort>(0, native.langid);
            Assert.AreEqual<ushort>(1200, native.cp);
            Assert.AreEqual<ushort>(0, native.wCollate);
            Assert.AreEqual<uint>(1, native.cbMax);
            Assert.AreEqual<uint>(0x10, native.grbit);
        }
    }
}
