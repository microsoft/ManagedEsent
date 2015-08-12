//-----------------------------------------------------------------------
// <copyright file="Windows10EquatableTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System.Diagnostics;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Windows10;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for classes that implement IEquatable
    /// </summary>
    public partial class EquatableTests
    {
        /// <summary>
        /// Check that <see cref="JET_THREADSTATS2"/> structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_THREADSTATS2 structures can be compared for equality")]
        public void VerifyJetThreadstats2Equality()
        {
            var x = new JET_THREADSTATS2
            {
                cbLogRecord = 1,
                cLogRecord = 2,
                cPageDirtied = 3,
                cPagePreread = 4,
                cPageRead = 5,
                cPageRedirtied = 6,
                cPageReferenced = 7,
                cusecPageCacheMiss = 8,
                cPageCacheMiss = 9,
            };
            var y = new JET_THREADSTATS2
            {
                cbLogRecord = 1,
                cLogRecord = 2,
                cPageDirtied = 3,
                cPagePreread = 4,
                cPageRead = 5,
                cPageRedirtied = 6,
                cPageReferenced = 7,
                cusecPageCacheMiss = 8,
                cPageCacheMiss = 9,
            };
            TestEquals(x, y);
            Assert.IsTrue(x == y);
            Assert.IsFalse(x != y);
        }

        /// <summary>
        /// Check that <see cref="JET_THREADSTATS2"/> structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_THREADSTATS2 structures can be compared for inequality")]
        public void VerifyJetThreadstats2Inequality()
        {
            // None of these objects are equal, most differ in only one member from the
            // first object. We will compare them all against each other.
            var threadstats2 = new[]
            {
                new JET_THREADSTATS2
                {
                    cbLogRecord = 1,
                    cLogRecord = 2,
                    cPageDirtied = 3,
                    cPagePreread = 4,
                    cPageRead = 5,
                    cPageRedirtied = 6,
                    cPageReferenced = 7,
                    cusecPageCacheMiss = 8,
                    cPageCacheMiss = 9,
                },
                new JET_THREADSTATS2
                {
                    cbLogRecord = 11,
                    cLogRecord = 2,
                    cPageDirtied = 3,
                    cPagePreread = 4,
                    cPageRead = 5,
                    cPageRedirtied = 6,
                    cPageReferenced = 7,
                    cusecPageCacheMiss = 8,
                    cPageCacheMiss = 9,
                },
                new JET_THREADSTATS2
                {
                    cbLogRecord = 1,
                    cLogRecord = 12,
                    cPageDirtied = 3,
                    cPagePreread = 4,
                    cPageRead = 5,
                    cPageRedirtied = 6,
                    cPageReferenced = 7,
                    cusecPageCacheMiss = 8,
                    cPageCacheMiss = 9,
                },
                new JET_THREADSTATS2
                {
                    cbLogRecord = 1,
                    cLogRecord = 2,
                    cPageDirtied = 13,
                    cPagePreread = 4,
                    cPageRead = 5,
                    cPageRedirtied = 6,
                    cPageReferenced = 7,
                    cusecPageCacheMiss = 8,
                    cPageCacheMiss = 9,
                },
                new JET_THREADSTATS2
                {
                    cbLogRecord = 1,
                    cLogRecord = 2,
                    cPageDirtied = 3,
                    cPagePreread = 14,
                    cPageRead = 5,
                    cPageRedirtied = 6,
                    cPageReferenced = 7,
                    cusecPageCacheMiss = 8,
                    cPageCacheMiss = 9,
                },
                new JET_THREADSTATS2
                {
                    cbLogRecord = 1,
                    cLogRecord = 2,
                    cPageDirtied = 3,
                    cPagePreread = 4,
                    cPageRead = 15,
                    cPageRedirtied = 6,
                    cPageReferenced = 7,
                    cusecPageCacheMiss = 8,
                    cPageCacheMiss = 9,
                },
                new JET_THREADSTATS2
                {
                    cbLogRecord = 1,
                    cLogRecord = 2,
                    cPageDirtied = 3,
                    cPagePreread = 4,
                    cPageRead = 5,
                    cPageRedirtied = 16,
                    cPageReferenced = 7,
                    cusecPageCacheMiss = 8,
                    cPageCacheMiss = 9,
                },
                new JET_THREADSTATS2
                {
                    cbLogRecord = 1,
                    cLogRecord = 2,
                    cPageDirtied = 3,
                    cPagePreread = 4,
                    cPageRead = 5,
                    cPageRedirtied = 6,
                    cPageReferenced = 17,
                    cusecPageCacheMiss = 8,
                    cPageCacheMiss = 9,
                },
                new JET_THREADSTATS2
                {
                    cbLogRecord = 1,
                    cLogRecord = 2,
                    cPageDirtied = 3,
                    cPagePreread = 4,
                    cPageRead = 5,
                    cPageRedirtied = 6,
                    cPageReferenced = 7,
                    cusecPageCacheMiss = 18,
                    cPageCacheMiss = 9,
                },
                new JET_THREADSTATS2
                {
                    cbLogRecord = 1,
                    cLogRecord = 2,
                    cPageDirtied = 3,
                    cPagePreread = 4,
                    cPageRead = 5,
                    cPageRedirtied = 6,
                    cPageReferenced = 7,
                    cusecPageCacheMiss = 8,
                    cPageCacheMiss = 19,
                },
            };

            // It would be nice if this was a generic helper method, but that won't
            // work for operator== and operator!=.
            for (int i = 0; i < threadstats2.Length - 1; ++i)
            {
                for (int j = i + 1; j < threadstats2.Length; ++j)
                {
                    Debug.Assert(i != j, "About to compare the same JET_THREADSTATS2");
                    TestNotEquals(threadstats2[i], threadstats2[j]);
                    Assert.IsTrue(threadstats2[i] != threadstats2[j]);
                    Assert.IsFalse(threadstats2[i] == threadstats2[j]);
                }
            }
        }

                /// <summary>
        /// Check that <see cref="JET_OPERATIONCONTEXT"/> structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_OPERATIONCONTEXT structures can be compared for equality")]
        public void VerifyJetOperationContextEquality()
        {
            var x = new JET_OPERATIONCONTEXT
            {
                UserID = 1,
                OperationID = 2,
                OperationType = 3,
                ClientType = 4,
                Flags = 5,
            };
            var y = new JET_OPERATIONCONTEXT
            {
                UserID = 1,
                OperationID = 2,
                OperationType = 3,
                ClientType = 4,
                Flags = 5,
            };

            TestEquals(x, y);
        }

        /// <summary>
        /// Check that <see cref="JET_OPERATIONCONTEXT"/> structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_OPERATIONCONTEXT structures can be compared for inequality")]
        public void VerifyJetOperationContextInequality()
        {
            // None of these objects are equal, most differ in only one member from the
            // first object. We will compare them all against each other.
            var operationcontexts = new[]
            {
                new JET_OPERATIONCONTEXT
                {
                    UserID = 11,
                    OperationID = 2,
                    OperationType = 3,
                    ClientType = 4,
                    Flags = 5,
                },
                new JET_OPERATIONCONTEXT
                {
                    UserID = 1,
                    OperationID = 22,
                    OperationType = 3,
                    ClientType = 4,
                    Flags = 5,
                },
                new JET_OPERATIONCONTEXT
                {
                    UserID = 1,
                    OperationID = 2,
                    OperationType = 33,
                    ClientType = 4,
                    Flags = 5,
                },
                new JET_OPERATIONCONTEXT
                {
                    UserID = 1,
                    OperationID = 2,
                    OperationType = 3,
                    ClientType = 44,
                    Flags = 5,
                },
                new JET_OPERATIONCONTEXT
                {
                    UserID = 1,
                    OperationID = 2,
                    OperationType = 3,
                    ClientType = 4,
                    Flags = 55,
                },
            };

            // It would be nice if this was a generic helper method, but that won't
            // work for operator== and operator!=.
            for (int i = 0; i < operationcontexts.Length - 1; ++i)
            {
                for (int j = i + 1; j < operationcontexts.Length; ++j)
                {
                    Debug.Assert(i != j, "About to compare the same JET_OPERATIONCONTEXT");
                    TestNotEquals(operationcontexts[i], operationcontexts[j]);
                    Assert.IsTrue(operationcontexts[i] != operationcontexts[j]);
                    Assert.IsFalse(operationcontexts[i] == operationcontexts[j]);

                    Assert.IsTrue(operationcontexts[i].GetHashCode() != operationcontexts[j].GetHashCode());
                    Assert.IsFalse(operationcontexts[i].GetHashCode() == operationcontexts[j].GetHashCode());
                }
            }
        }
    }
}