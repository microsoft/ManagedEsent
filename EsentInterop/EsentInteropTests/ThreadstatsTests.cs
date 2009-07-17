//-----------------------------------------------------------------------
// <copyright file="ThreadstatsTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Isam.Esent.Interop.Vista;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
    /// <summary>
    /// JET_THREADSTATS tests
    /// </summary>
    [TestClass]
    public class ThreadstatTests
    {
        private NATIVE_THREADSTATS native;

        private JET_THREADSTATS managed;
 
        [TestInitialize]
        public void Setup()
        {
            this.native = new NATIVE_THREADSTATS
            {
                cPageReferenced = 1,
                cPageRead = 2,
                cPagePreread = 3,
                cPageDirtied = 4,
                cPageRedirtied = 5,
                cLogRecord = 6,
                cbLogRecord = 7,
            };
            this.managed = new JET_THREADSTATS();
            this.managed.SetFromNativeThreadstats(this.native);
        }

        /// <summary>
        /// Test conversion from the native stuct
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void TestSetFromNativeSetsCpageReferenced()
        {
            Assert.AreEqual(1, this.managed.cPageReferenced);
        }

        /// <summary>
        /// Test conversion from the native stuct
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void TestSetFromNativeSetsCpageRead()
        {
            Assert.AreEqual(2, this.managed.cPageRead);
        }

        /// <summary>
        /// Test conversion from the native stuct
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void TestSetFromNativeSetsCpagePreread()
        {
            Assert.AreEqual(3, this.managed.cPagePreread);
        }

        /// <summary>
        /// Test conversion from the native stuct
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void TestSetFromNativeSetsCpageDirtied()
        {
            Assert.AreEqual(4, this.managed.cPageDirtied);
        }

        /// <summary>
        /// Test conversion from the native stuct
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void TestSetFromNativeSetsCpageRedirtied()
        {
            Assert.AreEqual(5, this.managed.cPageRedirtied);
        }

        /// <summary>
        /// Test conversion from the native stuct
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void TestSetFromNativeSetsClogrecord()
        {
            Assert.AreEqual(6, this.managed.cLogRecord);
        }

        /// <summary>
        /// Test conversion from the native stuct
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void TestSetFromNativeSetsCblogrecord()
        {
            Assert.AreEqual(7, this.managed.cbLogRecord);
        }

        /// <summary>
        /// Test adding two JET_THREADSTATS
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void TestJetThreadstatsAddition()
        {
            var t1 = new JET_THREADSTATS
            {
                cPageReferenced = 1,
                cPageRead = 2,
                cPagePreread = 3,
                cPageDirtied = 4,
                cPageRedirtied = 5,
                cLogRecord = 6,
                cbLogRecord = 7,
            };
            var t2 = new JET_THREADSTATS
            {
                cPageReferenced = 8,
                cPageRead = 9,
                cPagePreread = 10,
                cPageDirtied = 11,
                cPageRedirtied = 12,
                cLogRecord = 13,
                cbLogRecord = 14,
            };

            JET_THREADSTATS sum = t1 + t2;
            Assert.AreEqual(9, sum.cPageReferenced);
            Assert.AreEqual(11, sum.cPageRead);
            Assert.AreEqual(13, sum.cPagePreread);
            Assert.AreEqual(15, sum.cPageDirtied);
            Assert.AreEqual(17, sum.cPageRedirtied);
            Assert.AreEqual(19, sum.cLogRecord);
            Assert.AreEqual(21, sum.cbLogRecord);
        }

        /// <summary>
        /// Test adding two JET_THREADSTATS
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void TestJetThreadstatsSubtraction()
        {
            var t1 = new JET_THREADSTATS
            {
                cPageReferenced = 20,
                cPageRead = 19,
                cPagePreread = 18,
                cPageDirtied = 17,
                cPageRedirtied = 16,
                cLogRecord = 15,
                cbLogRecord = 14,
            };
            var t2 = new JET_THREADSTATS
            {
                cPageReferenced = 8,
                cPageRead = 9,
                cPagePreread = 10,
                cPageDirtied = 11,
                cPageRedirtied = 12,
                cLogRecord = 13,
                cbLogRecord = 14,
            };

            JET_THREADSTATS sum = t1 - t2;
            Assert.AreEqual(12, sum.cPageReferenced);
            Assert.AreEqual(10, sum.cPageRead);
            Assert.AreEqual(8, sum.cPagePreread);
            Assert.AreEqual(6, sum.cPageDirtied);
            Assert.AreEqual(4, sum.cPageRedirtied);
            Assert.AreEqual(2, sum.cLogRecord);
            Assert.AreEqual(0, sum.cbLogRecord);
        }
    }
}