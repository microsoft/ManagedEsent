//-----------------------------------------------------------------------
// <copyright file="Windows8EquatableTests.cs" company="Microsoft Corporation">
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
    /// Tests for classes that implement IEquatable
    /// </summary>
    public partial class EquatableTests
    {
        /// <summary>
        /// Check that JET_COMMIT_ID structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_COMMIT_ID structures can be compared for equality")]
        public void VerifyJetCommitIdEquality()
        {
            var sigX = new NATIVE_SIGNATURE()
            {
                ulRandom = 1,
                logtimeCreate = Any.Logtime,
                szComputerName = "Komputer",
            };
            var sigY = new NATIVE_SIGNATURE()
            {
                ulRandom = 1,
                logtimeCreate = sigX.logtimeCreate,
                szComputerName = "Komputer",
            };

            var x = new JET_COMMIT_ID(new NATIVE_COMMIT_ID()
                {
                    signLog = sigX,
                    commitId = 123,
                });
            var y = new JET_COMMIT_ID(new NATIVE_COMMIT_ID()
            {
                signLog = sigY,
                commitId = 123,
            });

            TestEquals(x, y);
            Assert.IsTrue(x == y);
            Assert.IsFalse(x != y);
        }

        /// <summary>
        /// Check that JET_COMMIT_ID structures works when compared to null.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_COMMIT_ID structures works when compared to null.")]
        public void VerifyJetCommitIdEqualityToNullIsFalse()
        {
            var sigX = new NATIVE_SIGNATURE()
            {
                ulRandom = 1,
                logtimeCreate = Any.Logtime,
                szComputerName = "Komputer",
            };

            var x = new JET_COMMIT_ID(new NATIVE_COMMIT_ID()
            {
                signLog = sigX,
                commitId = 123,
            });

            Assert.IsFalse(x == null);

            Assert.AreEqual(1, x.CompareTo(null));

            var commitZero = new JET_COMMIT_ID(new NATIVE_COMMIT_ID()
            {
                signLog = sigX,
                commitId = 0,
            });
            Assert.AreEqual(0, commitZero.CompareTo(null));
        }

        /// <summary>
        /// Check that JET_COMMIT_ID structures works when compared to a non-JET_COMMIT_ID.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_COMMIT_ID structures works when compared to a non-JET_COMMIT_ID.")]
        public void VerifyJetCommitIdEqualityToWrongTypeIsFalse()
        {
            var sigX = new NATIVE_SIGNATURE()
            {
                ulRandom = 1,
                logtimeCreate = Any.Logtime,
                szComputerName = "Komputer",
            };

            var x = new JET_COMMIT_ID(new NATIVE_COMMIT_ID()
            {
                signLog = sigX,
                commitId = 123,
            });

            Assert.IsFalse(x.Equals(sigX));
        }

        /// <summary>
        /// Check that JET_COMMIT_ID structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_COMMIT_ID structures can be compared for inequality")]
        public void VerifyJetCommitIdInequality()
        {
            var sigX = new NATIVE_SIGNATURE()
            {
                ulRandom = 1,
                logtimeCreate = Any.Logtime,
                szComputerName = "Komputer",
            };

            // Note that there isn't a case that has a different log signature; that's because
            // JET_COMMIT_ID.CompareTo() throws an exception if the log signature is different.
            //
            // None of these objects are equal, most differ in only one member from the
            // first object. We will compare them all against each other.
            var commitIds = new[]
            {
                new JET_COMMIT_ID(new NATIVE_COMMIT_ID()
                {
                    signLog = sigX,
                    commitId = 567,
                }),
                new JET_COMMIT_ID(new NATIVE_COMMIT_ID()
                {
                    signLog = sigX,
                    commitId = 9999, // Different
                }),
            };
            VerifyAll(commitIds);
        }

        /// <summary>
        /// Check that JET_COMMIT_ID structures throws when the signature does not match.
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_COMMIT_ID structures throws when the signature does not match.")]
        [ExpectedException(typeof(ArgumentException))]
        public void VerifyJetCommitIdThrowsExceptionWithInequalSignatures()
        {
            var sigX = new NATIVE_SIGNATURE()
            {
                ulRandom = 1,
                logtimeCreate = Any.Logtime,
                szComputerName = "Komputer",
            };
            var sigDifferent = new NATIVE_SIGNATURE()
            {
                ulRandom = 7777,
                logtimeCreate = Any.Logtime,
                szComputerName = "Different!",
            };

            var x = new JET_COMMIT_ID(new NATIVE_COMMIT_ID()
            {
                signLog = sigX,
                commitId = 789,
            });
            var y = new JET_COMMIT_ID(new NATIVE_COMMIT_ID()
            {
                signLog = sigDifferent,
                commitId = 789,
            });
            Assert.AreNotEqual(0, x.CompareTo(y));
        }
    }
}