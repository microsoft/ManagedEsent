//-----------------------------------------------------------------------
// <copyright file="IndexlistTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the JET_INDEXLIST class
    /// </summary>
    [TestClass]
    public class IndexlistConversionsTests
    {       
        /// <summary>
        /// The native index list that will be converted into a managed object.
        /// </summary>
        private NATIVE_INDEXLIST native;

        /// <summary>
        /// The managed version of the native indexlist.
        /// </summary>
        private JET_INDEXLIST converted;

        /// <summary>
        /// Setup the test fixture. This creates a native structure and converts
        /// it to a managed object.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.native = new NATIVE_INDEXLIST()
            {
                tableid = (IntPtr)0x1000,
                cRecord = 100,
                columnidindexname = 0,
                columnidgrbitIndex = 1,
                columnidcKey = 2,
                columnidcEntry = 3,
                columnidcPage = 4,
                columnidcColumn = 5,
                columnidiColumn = 6,
                columnidcolumnid = 7,
                columnidcoltyp = 8,
                columnidCountry = 9,
                columnidLangid = 10,
                columnidCp = 11,
                columnidCollate = 12,
                columnidgrbitColumn = 13,
                columnidcolumnname = 14,
                columnidLCMapFlags = 15,
            };

            this.converted = new JET_INDEXLIST();
            this.converted.SetFromNativeIndexlist(this.native);
        }

        /// <summary>
        /// Check the conversion of tableid.
        /// </summary>
        [TestMethod]
        public void ConvertIndexlistFromNativeSetsTableid()
        {
            Assert.AreEqual(new JET_TABLEID { Value = this.native.tableid }, this.converted.tableid);
        }

        /// <summary>
        /// Check the conversion of cRecord.
        /// </summary>
        [TestMethod]
        public void ConvertIndexlistFromNativeSetsCrecord()
        {
            Assert.AreEqual((int)this.native.cRecord, this.converted.cRecord);
        }

        /// <summary>
        /// Check the conversion of columnidindexname.
        /// </summary>
        [TestMethod]
        public void ConvertIndexlistFromNativeSetsColumnidindexname()
        {
            Assert.AreEqual(new JET_COLUMNID { Value = this.native.columnidindexname }, this.converted.columnidindexname);
        }

        /// <summary>
        /// Check the conversion of columnidgrbitIndex.
        /// </summary>
        [TestMethod]
        public void ConvertIndexlistFromNativeSetsColumnidgrbitIndex()
        {
            Assert.AreEqual(new JET_COLUMNID { Value = this.native.columnidgrbitIndex }, this.converted.columnidgrbitIndex);
        }

        /// <summary>
        /// Check the conversion of columnidcColumn.
        /// </summary>
        [TestMethod]
        public void ConvertIndexlistFromNativeSetsColumnidcColumn()
        {
            Assert.AreEqual(new JET_COLUMNID { Value = this.native.columnidcColumn }, this.converted.columnidcColumn);
        }

        /// <summary>
        /// Check the conversion of columnidcColumn.
        /// </summary>
        [TestMethod]
        public void ConvertIndexlistFromNativeSetsColumnidiColumn()
        {
            Assert.AreEqual(new JET_COLUMNID { Value = this.native.columnidiColumn }, this.converted.columnidiColumn);
        }

        /// <summary>
        /// Check the conversion of columnidcolumnid.
        /// </summary>
        [TestMethod]
        public void ConvertIndexlistFromNativeSetsColumnidcolumnid()
        {
            Assert.AreEqual(new JET_COLUMNID { Value = this.native.columnidcolumnid }, this.converted.columnidcolumnid);
        }

        /// <summary>
        /// Check the conversion of columnidcoltyp.
        /// </summary>
        [TestMethod]
        public void ConvertIndexlistFromNativeSetsColumnidcoltyp()
        {
            Assert.AreEqual(new JET_COLUMNID { Value = this.native.columnidcoltyp }, this.converted.columnidcoltyp);
        }

        /// <summary>
        /// Check the conversion of columnidLangid.
        /// </summary>
        [TestMethod]
        public void ConvertIndexlistFromNativeSetsColumnidLangid()
        {
            Assert.AreEqual(new JET_COLUMNID { Value = this.native.columnidLangid }, this.converted.columnidLangid);
        }

        /// <summary>
        /// Check the conversion of columnidCp.
        /// </summary>
        [TestMethod]
        public void ConvertIndexlistFromNativeSetsColumnidCp()
        {
            Assert.AreEqual(new JET_COLUMNID { Value = this.native.columnidCp }, this.converted.columnidCp);
        }

        /// <summary>
        /// Check the conversion of columnidgrbitColumn.
        /// </summary>
        [TestMethod]
        public void ConvertIndexlistFromNativeSetsColumnidgrbitColumn()
        {
            Assert.AreEqual(new JET_COLUMNID { Value = this.native.columnidgrbitColumn }, this.converted.columnidgrbitColumn);
        }
 
        /// <summary>
        /// Check the conversion of columnidcolumnname.
        /// </summary>
        [TestMethod]
        public void ConvertIndexlistFromNativeSetsColumnidcolumnname()
        {
            Assert.AreEqual(new JET_COLUMNID { Value = this.native.columnidcolumnname }, this.converted.columnidcolumnname);
        }

        /// <summary>
        /// Check the conversion of columnidLCMapFlags.
        /// </summary>
        [TestMethod]
        public void ConvertIndexlistFromNativeSetsColumnidLCMapFlags()
        {
            Assert.AreEqual(new JET_COLUMNID { Value = this.native.columnidLCMapFlags }, this.converted.columnidLCMapFlags);
        }
    }
}