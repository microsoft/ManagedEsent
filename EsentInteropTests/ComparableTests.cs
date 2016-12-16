//-----------------------------------------------------------------------
// <copyright file="ComparableTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Windows8;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for classes that implement IComparable
    /// </summary>
    [TestClass]
    public partial class ComparableTests
    {
        /// <summary>
        /// Check that JET_LGPOS structures can be compared.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_LGPOS structures can be compared")]
        public void VerifyJetLgposComparison()
        {
            // These positions are in ascending order
            var positions = new[]
            {
                new JET_LGPOS { lGeneration = 1, isec = 3, ib = 5 },
                new JET_LGPOS { lGeneration = 1, isec = 3, ib = 6 },
                new JET_LGPOS { lGeneration = 1, isec = 4, ib = 4 },
                new JET_LGPOS { lGeneration = 1, isec = 4, ib = 5 },
                new JET_LGPOS { lGeneration = 2, isec = 2, ib = 2 },
                new JET_LGPOS { lGeneration = 2, isec = 3, ib = 5 },
            };

            // It would be nice if this was a generic helper method, but that won't
            // work for the operators.
            for (int i = 0; i < positions.Length - 1; ++i)
            {
                TestEqualObjects(positions[i], positions[i]);
                Assert.IsTrue(positions[i] <= positions[i], "<=");
                Assert.IsTrue(positions[i] >= positions[i], ">=");

                for (int j = i + 1; j < positions.Length; ++j)
                {
                    TestOrderedObjects(positions[i], positions[j]);
                    Assert.IsTrue(positions[i] < positions[j], "<");
                    Assert.IsTrue(positions[i] <= positions[j], "<=");
                    Assert.IsTrue(positions[j] > positions[i], ">");
                    Assert.IsTrue(positions[j] >= positions[i], ">=");
                }
            }
        }

        /// <summary>
        /// Check that JET_COLUMNID structures can be compared.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_COLUMNID structures can be compared")]
        public void VerifyJetColumnidComparison()
        {
            // These positions are in ascending order
            var columnids = new[]
            {
                JET_COLUMNID.Nil,
                new JET_COLUMNID { Value = 1U },
                new JET_COLUMNID { Value = 2U },
                new JET_COLUMNID { Value = 3U },
                new JET_COLUMNID { Value = 4U },
            };

            // It would be nice if this was a generic helper method, but that won't
            // work for the operators.);
            for (int i = 0; i < columnids.Length - 1; ++i)
            {
                TestEqualObjects(columnids[i], columnids[i]);
                Assert.IsTrue(columnids[i] <= columnids[i], "<=");
                Assert.IsTrue(columnids[i] >= columnids[i], ">=");

                for (int j = i + 1; j < columnids.Length; ++j)
                {
                    TestOrderedObjects(columnids[i], columnids[j]);
                    Assert.IsTrue(columnids[i] < columnids[j], "<");
                    Assert.IsTrue(columnids[i] <= columnids[j], "<=");
                    Assert.IsTrue(columnids[j] > columnids[i], ">");
                    Assert.IsTrue(columnids[j] >= columnids[i], ">=");
                }
            }
        }

        /// <summary>
        /// Verifies that Instance.IsInvalid works as expected.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verifies that Instance.IsInvalid works as expected.")]
        public void VerifyInstanceIsInvalidWorksAsExpected()
        {
            var zero = JET_INSTANCE.Nil;
            var minusOne = new JET_INSTANCE()
            {
                Value = new IntPtr(~0)
            };
            var shouldBeValid = new JET_INSTANCE()
            {
                Value = new IntPtr(4)
            };

            Assert.IsTrue(zero.IsInvalid);
            Assert.IsTrue(minusOne.IsInvalid);
            Assert.IsFalse(shouldBeValid.IsInvalid);
        }

        /// <summary>
        /// Verifies that JET_SESID.IsInvalid works as expected.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verifies that JET_SESID.IsInvalid works as expected.")]
        public void VerifySesidIsInvalidWorksAsExpected()
        {
            var zero = JET_SESID.Nil;
            var minusOne = new JET_SESID()
            {
                Value = new IntPtr(~0)
            };
            var shouldBeValid = new JET_SESID()
            {
                Value = new IntPtr(4)
            };

            Assert.IsTrue(zero.IsInvalid);
            Assert.IsTrue(minusOne.IsInvalid);
            Assert.IsFalse(shouldBeValid.IsInvalid);
        }

        /// <summary>
        /// Verifies that JET_TABLEID.IsInvalid works as expected.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verifies that JET_TABLEID.IsInvalid works as expected.")]
        public void VerifyTableidIsInvalidWorksAsExpected()
        {
            var zero = JET_TABLEID.Nil;
            var minusOne = new JET_TABLEID()
            {
                Value = new IntPtr(~0)
            };
            var shouldBeValid = new JET_TABLEID()
            {
                Value = new IntPtr(4)
            };

            Assert.IsTrue(zero.IsInvalid);
            Assert.IsTrue(minusOne.IsInvalid);
            Assert.IsFalse(shouldBeValid.IsInvalid);
        }

        /// <summary>
        /// Verifies that JET_COLUMNID.IsInvalid works as expected.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verifies that JET_COLUMNID.IsInvalid works as expected.")]
        public void VerifyColumnidIsInvalidWorksAsExpected()
        {
            var zero = JET_COLUMNID.Nil;
            var minusOne = new JET_COLUMNID()
            {
                Value = unchecked((uint)~0)
            };
            var shouldBeValid = new JET_COLUMNID()
            {
                Value = 4
            };

            Assert.IsTrue(zero.IsInvalid);
            Assert.IsTrue(minusOne.IsInvalid);
            Assert.IsFalse(shouldBeValid.IsInvalid);
        }

        /// <summary>
        /// Verifies that JET_HANDLE.IsInvalid works as expected.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verifies that JET_HANDLE.IsInvalid works as expected.")]
        public void VerifyJethandleIsInvalidWorksAsExpected()
        {
            var zero = JET_HANDLE.Nil;
            var minusOne = new JET_HANDLE()
            {
                Value = new IntPtr(~0)
            };
            var shouldBeValid = new JET_HANDLE()
            {
                Value = new IntPtr(4)
            };

            Assert.IsTrue(zero.IsInvalid);
            Assert.IsTrue(minusOne.IsInvalid);
            Assert.IsFalse(shouldBeValid.IsInvalid);
        }

        #region Structures not avaiable to Windows Store Apps.
#if !MANAGEDESENT_ON_WSA
        /// <summary>
        /// Verifies that JET_OSSNAPID.IsInvalid works as expected.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verifies that JET_OSSNAPID.IsInvalid works as expected.")]
        public void VerifyOssnapidIsInvalidWorksAsExpected()
        {
            var zero = JET_OSSNAPID.Nil;
            var minusOne = new JET_OSSNAPID()
            {
                Value = new IntPtr(~0)
            };
            var shouldBeValid = new JET_OSSNAPID()
            {
                Value = new IntPtr(4)
            };

            Assert.IsTrue(zero.IsInvalid);
            Assert.IsTrue(minusOne.IsInvalid);
            Assert.IsFalse(shouldBeValid.IsInvalid);
        }

        /// <summary>
        /// Verifies that JET_LS.IsInvalid works as expected.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verifies that JET_LS.IsInvalid works as expected.")]
        public void VerifyJetlsIsInvalidWorksAsExpected()
        {
            var zero = JET_LS.Nil;
            var minusOne = new JET_LS()
            {
                Value = new IntPtr(~0)
            };
            var shouldBeValid = new JET_LS()
            {
                Value = new IntPtr(4)
            };

            Assert.IsTrue(zero.IsInvalid);
            Assert.IsTrue(minusOne.IsInvalid);
            Assert.IsFalse(shouldBeValid.IsInvalid);
        }
#endif
        #endregion

        #region structures new to Windows 8.

        /// <summary>
        /// Check that two equal JET_COMMIT_ID structure can be compared
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that two equal JET_COMMIT_ID structure can be compared")]
        public void VerifyJetCommitIdEquality()
        {
            DateTime d = DateTime.Now;
            JET_COMMIT_ID commitId1 = DurableCommitTests.CreateJetCommitId(1, d, "computer", 2);
            JET_COMMIT_ID commitId2 = DurableCommitTests.CreateJetCommitId(1, d, "computer", 2);
            Assert.IsTrue(commitId1 == commitId2);
            Assert.IsTrue(commitId1.GetHashCode() == commitId2.GetHashCode());
            Assert.IsTrue(commitId1.Equals(commitId2));
            Assert.IsFalse(commitId1 != commitId2);
            Assert.IsTrue(commitId1 <= commitId2);
            Assert.IsFalse(commitId1 < commitId2);
            Assert.IsTrue(commitId1 >= commitId2);
            Assert.IsFalse(commitId1 > commitId2);
        }

        /// <summary>
        /// Check that two unequal JET_COMMIT_ID structures can be compared.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that two unequal JET_COMMIT_ID structures can be compared")]
        public void VerifyJetCommitIdInequality()
        {
            DateTime d = DateTime.Now;
            JET_COMMIT_ID commitId1 = DurableCommitTests.CreateJetCommitId(1, d, "computer", 2);
            JET_COMMIT_ID commitId2 = DurableCommitTests.CreateJetCommitId(1, d, "computer", 3);
            Assert.IsFalse(commitId1 == commitId2);
            Assert.IsTrue(commitId1.GetHashCode() != commitId2.GetHashCode());
            Assert.IsFalse(commitId1.Equals(commitId2));
            Assert.IsTrue(commitId1 != commitId2);
            Assert.IsTrue(commitId1 <= commitId2);
            Assert.IsTrue(commitId1 < commitId2);
            Assert.IsFalse(commitId1 >= commitId2);
            Assert.IsFalse(commitId1 > commitId2);
        }

        /// <summary>
        /// Check that JET_COMMIT_ID structures with different signature cannot be
        /// compared
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentException))]
        [Description("Check that JET_COMMIT_ID structures with different signature cannot be compared")]
        public void VerifyJetCommitIdIncomparable()
        {
            DateTime d = DateTime.Now;
            JET_COMMIT_ID commitId1 = DurableCommitTests.CreateJetCommitId(1, d, "computer", 3);
            JET_COMMIT_ID commitId2 = DurableCommitTests.CreateJetCommitId(2, d, "computer", 3);
            Assert.IsTrue(commitId1.GetHashCode() != commitId2.GetHashCode());
            Assert.IsFalse(commitId1 == commitId2);
        }

        #endregion

        /// <summary>
        /// Helper method to compare two equal objects.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="a">The first object.</param>
        /// <param name="b">The second object.</param>
        private static void TestEqualObjects<T>(T a, T b) where T : struct, IComparable<T>
        {
            Assert.AreEqual(0, a.CompareTo(b), "{0}.CompareTo({1})", a, b);
            Assert.AreEqual(0, b.CompareTo(a), "{0}.CompareTo({1})", b, a);
        }

        /// <summary>
        /// Helper method to compare two ordered objects.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="smaller">The smaller object.</param>
        /// <param name="larger">The larger object.</param>
        private static void TestOrderedObjects<T>(T smaller, T larger) where T : struct, IComparable<T>
        {
            int compare = smaller.CompareTo(larger);
            Assert.IsTrue(compare < 0, "expected < 0 ({0})", compare);
            compare = larger.CompareTo(smaller);
            Assert.IsTrue(compare > 0, "expected > 0 ({0})", compare);
        }
    }
}