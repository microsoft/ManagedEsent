//-----------------------------------------------------------------------
// <copyright file="ObjectlistTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// JET_OBJECTLIST tests
    /// </summary>
    [TestClass]
    public class ObjectlistTests
    {
        /// <summary>
        /// Test conversion from the native stuct
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void ConvertObjectlistFromNative()
        {
            var tableid = new JET_TABLEID { Value = (IntPtr)0x1000 };
            var col1 = new JET_COLUMNID { Value = 1 };
            var col2 = new JET_COLUMNID { Value = 2 };
            var col3 = new JET_COLUMNID { Value = 3 };
            var col4 = new JET_COLUMNID { Value = 4 };
            var col5 = new JET_COLUMNID { Value = 5 };
            var col6 = new JET_COLUMNID { Value = 6 };

            var native = new NATIVE_OBJECTLIST()
            {
                tableid = tableid.Value,
                cRecord = 100,
                columnidobjectname = col1.Value,
                columnidobjtyp = col2.Value,
                columnidgrbit = col3.Value,
                columnidflags = col4.Value,
                columnidcRecord = col5.Value,
                columnidcPage = col6.Value,
            };

            var objectlist = new JET_OBJECTLIST();
            objectlist.SetFromNativeObjectlist(native);

            Assert.AreEqual(tableid, objectlist.tableid);
            Assert.AreEqual(100, objectlist.cRecord);
            Assert.AreEqual(col1, objectlist.columnidobjectname);
            Assert.AreEqual(col2, objectlist.columnidobjtyp);
            Assert.AreEqual(col3, objectlist.columnidgrbit);
            Assert.AreEqual(col4, objectlist.columnidflags);
            Assert.AreEqual(col5, objectlist.columnidcRecord);
            Assert.AreEqual(col6, objectlist.columnidcPage);
        }
    }
}