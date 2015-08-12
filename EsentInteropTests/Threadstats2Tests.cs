//-----------------------------------------------------------------------
// <copyright file="Threadstats2Tests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;

    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Vista;
    using Microsoft.Isam.Esent.Interop.Windows10;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// JET_THREADSTATS2 tests
    /// </summary>
    [TestClass]
    public class Threadstats2Tests
    {
        /// <summary>
        /// Test the Create method.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that JET_THREADSTATS2.Create sets the members")]
        public void TestCreateSetsMembers()
        {
            JET_THREADSTATS2 actual = JET_THREADSTATS2.Create(1, 2, 3, 4, 5, 6, 7, 8, 9);
            Assert.AreEqual(1, actual.cPageReferenced);
            Assert.AreEqual(2, actual.cPageRead);
            Assert.AreEqual(3, actual.cPagePreread);
            Assert.AreEqual(4, actual.cPageDirtied);
            Assert.AreEqual(5, actual.cPageRedirtied);
            Assert.AreEqual(6, actual.cLogRecord);
            Assert.AreEqual(7, actual.cbLogRecord);
            Assert.AreEqual(8, actual.cusecPageCacheMiss);
            Assert.AreEqual(9, actual.cPageCacheMiss);
        }

        /// <summary>
        /// Test adding two JET_THREADSTATS2
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test adding two JET_THREADSTATS2")]
        public void TestJetThreadstats2Addition()
        {
            var t1 = new JET_THREADSTATS2
            {
                cPageReferenced = 1,
                cPageRead = 2,
                cPagePreread = 3,
                cPageDirtied = 4,
                cPageRedirtied = 5,
                cLogRecord = 6,
                cbLogRecord = 7,
                cusecPageCacheMiss = 8,
                cPageCacheMiss = 9,
            };
            var t2 = new JET_THREADSTATS2
            {
                cPageReferenced = 101,
                cPageRead = 102,
                cPagePreread = 103,
                cPageDirtied = 104,
                cPageRedirtied = 105,
                cLogRecord = 106,
                cbLogRecord = 107,
                cusecPageCacheMiss = 108,
                cPageCacheMiss = 109,
            };

            JET_THREADSTATS2 sum = t1 + t2;
            Assert.AreEqual(102, sum.cPageReferenced);
            Assert.AreEqual(104, sum.cPageRead);
            Assert.AreEqual(106, sum.cPagePreread);
            Assert.AreEqual(108, sum.cPageDirtied);
            Assert.AreEqual(110, sum.cPageRedirtied);
            Assert.AreEqual(112, sum.cLogRecord);
            Assert.AreEqual(114, sum.cbLogRecord);
            Assert.AreEqual(116, sum.cusecPageCacheMiss);
            Assert.AreEqual(118, sum.cPageCacheMiss);

            Assert.AreEqual(sum, JET_THREADSTATS2.Add(t1, t2));
        }

        /// <summary>
        /// Test subtracting two JET_THREADSTATS2
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test subtracting two JET_THREADSTATS2")]
        public void TestJetThreadstats2Subtraction()
        {
            var t1 = new JET_THREADSTATS2
            {
                cPageReferenced = 101,
                cPageRead = 102,
                cPagePreread = 103,
                cPageDirtied = 104,
                cPageRedirtied = 105,
                cLogRecord = 106,
                cbLogRecord = 107,
                cusecPageCacheMiss = 108,
                cPageCacheMiss = 109,
            };
            var t2 = new JET_THREADSTATS2
            {
                cPageReferenced = 1,
                cPageRead = 2,
                cPagePreread = 3,
                cPageDirtied = 4,
                cPageRedirtied = 5,
                cLogRecord = 6,
                cbLogRecord = 7,
                cusecPageCacheMiss = 8,
                cPageCacheMiss = 9,
            };

            JET_THREADSTATS2 difference = t1 - t2;
            Assert.AreEqual(100, difference.cPageReferenced);
            Assert.AreEqual(100, difference.cPageRead);
            Assert.AreEqual(100, difference.cPagePreread);
            Assert.AreEqual(100, difference.cPageDirtied);
            Assert.AreEqual(100, difference.cPageRedirtied);
            Assert.AreEqual(100, difference.cLogRecord);
            Assert.AreEqual(100, difference.cbLogRecord);
            Assert.AreEqual(100, difference.cusecPageCacheMiss);
            Assert.AreEqual(100, difference.cPageCacheMiss);

            Assert.AreEqual(difference, JET_THREADSTATS2.Subtract(t1, t2));
        }

        /// <summary>
        /// Test JET_THREADSTATS2.ToString()
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_THREADSTATS2.ToString with singular counts")]
        public void TestJetThreadstats2ToStringSingular()
        {
            var t = new JET_THREADSTATS2
            {
                cPageReferenced = 1,
                cPageRead = 1,
                cPagePreread = 1,
                cPageDirtied = 1,
                cPageRedirtied = 1,
                cLogRecord = 1,
                cbLogRecord = 1,
                cusecPageCacheMiss = 1,
                cPageCacheMiss = 1,
            };
            const string Expected = "1 page reference, 1 page read, 1 page preread, 1 page dirtied, 1 page redirtied, 1 log record, 1 byte logged, 1 page cache miss latency (us), 1 page cache miss count";
            Assert.AreEqual(Expected, t.ToString());
        }

        /// <summary>
        /// Test JET_THREADSTATS2.ToString()
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_THREADSTATS2.ToString with zero counts")]
        public void TestJetThreadstats2ToStringZero()
        {
            var t = new JET_THREADSTATS2
            {
                cPageReferenced = 0,
                cPageRead = 0,
                cPagePreread = 0,
                cPageDirtied = 0,
                cPageRedirtied = 0,
                cLogRecord = 0,
                cbLogRecord = 0,
                cusecPageCacheMiss = 0,
                cPageCacheMiss = 0,
            };
            const string Expected = "0 page references, 0 pages read, 0 pages preread, 0 pages dirtied, 0 pages redirtied, 0 log records, 0 bytes logged, 0 page cache miss latency (us), 0 page cache miss count";
            Assert.AreEqual(Expected, t.ToString());
        }

        /// <summary>
        /// Test JET_THREADSTATS2.ToString()
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_THREADSTATS2.ToString with plural counts")]
        public void TestJetThreadstats2ToString()
        {
            var t = new JET_THREADSTATS2
            {
                cPageReferenced = 2,
                cPageRead = 3,
                cPagePreread = 4,
                cPageDirtied = 5,
                cPageRedirtied = 6,
                cLogRecord = 7,
                cbLogRecord = 8,
                cusecPageCacheMiss = 9,
                cPageCacheMiss = 10,
            };
            const string Expected = "2 page references, 3 pages read, 4 pages preread, 5 pages dirtied, 6 pages redirtied, 7 log records, 8 bytes logged, 9 page cache miss latency (us), 10 page cache miss count";
            Assert.AreEqual(Expected, t.ToString());
        }
    }
}