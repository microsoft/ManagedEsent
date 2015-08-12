//-----------------------------------------------------------------------
// <copyright file="NullableStructureTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for nullable structures.
    /// </summary>
    [TestClass]
    public class NullableStructureTests
    {
        /// <summary>
        /// Non-empty logtime used for testing.
        /// </summary>
        private static readonly JET_LOGTIME Logtime = new JET_LOGTIME(DateTime.Now);

        /// <summary>
        /// Non-empty bklogtime used for testing.
        /// </summary>
        private static readonly JET_BKLOGTIME Bklogtime = new JET_BKLOGTIME(DateTime.Now, Any.Boolean);

        /// <summary>
        /// Non-empty lgpos used for testing.
        /// </summary>
        private static readonly JET_LGPOS Lgpos = new JET_LGPOS { lGeneration = 1 };

        /// <summary>
        /// Non-empty bkinfo used for testing.
        /// </summary>
        private static readonly JET_BKINFO Bkinfo = new JET_BKINFO
                                                        {
                                                            bklogtimeMark = Bklogtime,
                                                            genHigh = 10,
                                                            genLow = 8,
                                                            lgposMark = Lgpos
                                                        };

        /// <summary>
        /// Verify an empty JET_LOGTIME has no value.
        /// </summary>
        [TestMethod]
        [Description("Verify an empty JET_LOGTIME has no value")]
        [Priority(0)]
        public void VerifyEmptyJetLogtimeHasNoValue()
        {
            TestDefaultHasNoValue<JET_LOGTIME>();
        }

        /// <summary>
        /// Verify a non-empty JET_LOGTIME has a value.
        /// </summary>
        [TestMethod]
        [Description("Verify a non-empty JET_LOGTIME has a value")]
        [Priority(0)]
        public void VerifyNonEmptyJetLogtimeHasNoValue()
        {
            Assert.IsTrue(Logtime.HasValue);
        }

        /// <summary>
        /// Verify an empty JET_BKLOGTIME has no value.
        /// </summary>
        [TestMethod]
        [Description("Verify an empty JET_BKLOGTIME has no value")]
        [Priority(0)]
        public void VerifyEmptyJetBklogtimeHasNoValue()
        {
            TestDefaultHasNoValue<JET_BKLOGTIME>();
        }

        /// <summary>
        /// Verify a non-empty JET_BKLOGTIME has a value.
        /// </summary>
        [TestMethod]
        [Description("Verify a non-empty JET_BKLOGTIME has a value")]
        [Priority(0)]
        public void VerifyNonEmptyJetBklogtimeHasNoValue()
        {
            Assert.IsTrue(Bklogtime.HasValue);
        }

        /// <summary>
        /// Verify an empty JET_LGPOS has no value.
        /// </summary>
        [TestMethod]
        [Description("Verify an empty JET_LGPOS has no value")]
        [Priority(0)]
        public void VerifyEmptyJetLgposHasNoValue()
        {
            TestDefaultHasNoValue<JET_LGPOS>();
        }

        /// <summary>
        /// Verify a non-empty JET_LGPOS has a value.
        /// </summary>
        [TestMethod]
        [Description("Verify a non-empty JET_LGPOS has a value")]
        [Priority(0)]
        public void VerifyNonEmptyJetLgposHasNoValue()
        {
            Assert.IsTrue(Lgpos.HasValue);
        }

        /// <summary>
        /// Verify an empty JET_BKINFO has no value.
        /// </summary>
        [TestMethod]
        [Description("Verify an empty JET_BKINFO has no value")]
        [Priority(0)]
        public void VerifyEmptyJetBkinfoHasNoValue()
        {
            TestDefaultHasNoValue<JET_BKINFO>();
        }

        /// <summary>
        /// Verify a non-empty JET_BKINFO has a value.
        /// </summary>
        [TestMethod]
        [Description("Verify a non-empty JET_BKINFO has a value")]
        [Priority(0)]
        public void VerifyNonEmptyJetBkinfoHasNoValue()
        {
            Assert.IsTrue(Bkinfo.HasValue);
        }

        /// <summary>
        /// Assert that a default structure has no value.
        /// </summary>
        /// <typeparam name="T">The nullable type to test.</typeparam>
        private static void TestDefaultHasNoValue<T>() where T : INullableJetStruct
        {
            var value = default(T);
            Assert.IsFalse(value.HasValue);
        }
    }
}