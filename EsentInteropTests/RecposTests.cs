//-----------------------------------------------------------------------
// <copyright file="RecposTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// JET_RECPOS tests
    /// </summary>
    [TestClass]
    public class RecposTests
    {
        /// <summary>
        /// Test conversion to the native stuct
        /// </summary>
        [TestMethod]
        public void ConvertRecposToNative()
        {
            var recpos = new JET_RECPOS();
            recpos.centriesLT = 5;
            recpos.centriesTotal = 10;

            var native = recpos.GetNativeRecpos();
            Assert.AreEqual<uint>(5, native.centriesLT);
            Assert.AreEqual<uint>(10, native.centriesTotal);
        }

        /// <summary>
        /// Test conversion from the native stuct
        /// </summary>
        [TestMethod]
        public void ConvertRecposFromNative()
        {
            var native = new NATIVE_RECPOS();
            native.centriesLT = 1;
            native.centriesTotal = 2;

            var recpos = new JET_RECPOS();
            recpos.SetFromNativeRecpos(native);

            Assert.AreEqual(1, recpos.centriesLT);
            Assert.AreEqual(2, recpos.centriesTotal);
        }
    }
}
