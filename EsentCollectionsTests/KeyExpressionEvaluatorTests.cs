// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeyExpressionEvaluatorTests.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//   Compare a PersistentDictionary against a generic dictionary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EsentCollectionsTests
{
    using System;
    using Microsoft.Isam.Esent.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Compare a PersistentDictionary against a generic dictionary.
    /// </summary>
    [TestClass]
    public class KeyExpressionEvaluatorTests
    {
        /// <summary>
        /// Const member used for tests.
        /// </summary>
        private const uint ConstMember = 18U;

        /// <summary>
        /// Static member used for tests.
        /// </summary>
        private static ulong staticMember;

        /// <summary>
        /// Member used for tests.
        /// </summary>
        private int member;

        /// <summary>
        /// Verify that GetKeyRange throws an exception when its argument is null.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that GetKeyRange throws an exception when its argument is null")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyGetKeyRangeThrowsExceptionWhenArgumentIsNull()
        {
            KeyRange<short> keyRange = KeyExpressionEvaluator<short, decimal>.GetKeyRange(null);
        }

        /// <summary>
        /// Verify that a true expression gives an open range.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a true expression gives an open range")]
        public void VerifyTrueExpressionGivesOpenRange()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => true);
            Assert.IsNull(keyRange.Min);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Verify that an expression without ranges gives an open range.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that an expression without ranges gives an open range")]
        public void VerifyExpressionWithoutRangeGivesOpenRange()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => (0 == (x.Key % 2)));
            Assert.IsNull(keyRange.Min);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Verify that an expression on the value gives an open range.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that an expression without ranges gives an open range")]
        public void VerifyValueExpressionGivesNoLimits()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, int>.GetKeyRange(x => x.Value < 100);
            Assert.IsNull(keyRange.Min);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Verify that a LT comparison gives a max limit.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a < comparison gives a max limit")]
        public void VerifyLtComparisonGivesMaxLimit()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => x.Key < 10);
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(10, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Verify that a LE comparison gives a max limit.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a <= comparison gives a max limit")]
        public void VerifyLeComparisonGivesMaxLimit()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => x.Key <= 11);
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(11, keyRange.Max.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Verify that a LT comparison gives a max limit.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a reversed < comparison gives a max limit")]
        public void VerifyLtComparisonReversedGivesMaxLimit()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => 10 > x.Key);
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(10, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Verify that a LE comparison gives a max limit.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a reversed <= comparison gives a max limit")]
        public void VerifyLeComparisonReversedGivesMaxLimit()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => 11 >= x.Key);
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(11, keyRange.Max.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Verify that multiple LT comparisons give a max limit.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that multiple < comparisons give a max limit")]
        public void VerifyLtCollapse()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => x.Key < 3 && x.Key < 2 && x.Key < 5);
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(2, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Verify that multiple LE comparisons give a max limit.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that multiple <= comparisons give a max limit")]
        public void VerifyLeCollapse()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => x.Key <= 1 && 2 >= x.Key && x.Key <= 3);
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(1, keyRange.Max.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Verify that LT and LE comparisons give a max limit.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that < and <= comparisons give a max limit")]
        public void VerifyLtAndLeCollapse()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => x.Key <= 11 && x.Key < 11);
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(11, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Verify that a GT comparison gives a min limit.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a > comparison gives a min limit")]
        public void VerifyGtComparisonGivesMinLimit()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => x.Key > 10);
            Assert.AreEqual(10, keyRange.Min.Value);
            Assert.IsFalse(keyRange.Min.IsInclusive);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Verify that a GE comparison gives a min limit.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a >= comparison gives a min limit")]
        public void VerifyGeComparisonGivesMinLimit()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => x.Key >= 11);
            Assert.AreEqual(11, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Verify that a GT comparison gives a min limit.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a reversed > comparison gives a min limit")]
        public void VerifyGtComparisonReversedGivesMinLimit()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => 10 < x.Key);
            Assert.AreEqual(10, keyRange.Min.Value);
            Assert.IsFalse(keyRange.Min.IsInclusive);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Verify that a GE comparison gives a min limit.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a reversed >= comparison gives a min limit")]
        public void VerifyGeComparisonReversedGivesMinLimit()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => 11 <= x.Key);
            Assert.AreEqual(11, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Verify that multiple GT comparisons give a min limit.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that multiple > comparisons give a min limit")]
        public void VerifyGtCollapse()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => x.Key > 3 && x.Key > 2 && x.Key > 5);
            Assert.AreEqual(5, keyRange.Min.Value);
            Assert.IsFalse(keyRange.Min.IsInclusive);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Verify that multiple GE comparisons give a min limit.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that multiple >= comparisons give a min limit")]
        public void VerifyGeCollapse()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => x.Key >= 1 && 2 <= x.Key && x.Key >= 3);
            Assert.AreEqual(3, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Verify that GT and GE comparisons give a min limit.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that > and >= comparisons give a min limit")]
        public void VerifyGtAndGeCollapse()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => x.Key >= 11 && x.Key > 11);
            Assert.AreEqual(11, keyRange.Min.Value);
            Assert.IsFalse(keyRange.Min.IsInclusive);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Verify that an == comparison gives upper and lower limits.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that an == comparison gives upper and lower limits")]
        public void VerifyEqGivesMinAndMaxLimits()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => x.Key == 7);
            Assert.AreEqual(7, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.AreEqual(7, keyRange.Max.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Verify that a reversed == comparison gives upper and lower limits.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a reversed == comparison gives upper and lower limits")]
        public void VerifyEqReversedGivesMinAndMaxLimits()
        {
            KeyRange<long> keyRange = KeyExpressionEvaluator<long, string>.GetKeyRange(x => 8 == x.Key);
            Assert.AreEqual(8L, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.AreEqual(8L, keyRange.Max.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Verify that LT and GT comparisons give min and max limits.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify < and > comparisons give min and max limits")]
        public void VerifyLtAndGtComparisonsGiveMinAndMaxLimits()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => 19 < x.Key && x.Key < 101);
            Assert.AreEqual(19, keyRange.Min.Value);
            Assert.IsFalse(keyRange.Min.IsInclusive);
            Assert.AreEqual(101, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Verify an AND clause still produces limits.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify an AND clause still produces limits")]
        public void VerifyAndStillGivesLimits()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => 19 <= x.Key && x.Key <= 101 && x.Value.StartsWith("foo"));
            Assert.AreEqual(19, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.AreEqual(101, keyRange.Max.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Verify an OR clause removes limits.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify an OR clause removes limits")]
        public void VerifyOrRemovesLimits()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => (19 <= x.Key && x.Key <= 101) || x.Value.StartsWith("foo"));
            Assert.IsNull(keyRange.Min);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Verify local variable evaluation.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify local variable evaluation")]
        public void VerifyLocalVariableEvaluation()
        {
            int i = 29;
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, long>.GetKeyRange(x => x.Key <= i);
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(i, keyRange.Max.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Verify member variable evaluation.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify member variable evaluation")]
        public void VerifyMemberVariableEvaluation()
        {
            this.member = 18;
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, long>.GetKeyRange(x => x.Key <= this.member);
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(this.member, keyRange.Max.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Verify constant folding.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify constant folding")]
        public void VerifyConstantFolding()
        {
            this.member = 18;
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, long>.GetKeyRange(x => x.Key <= this.member + 10);
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(28, keyRange.Max.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test expression 1.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test expression 1")]
        public void TestSample1()
        {
            KeyRange<double> keyRange = KeyExpressionEvaluator<double, string>.GetKeyRange(
                x => 18 <= x.Key && x.Key < 99 && x.Key > 7 && x.Key <= 99 && (0 == x.Key % 2));
            Assert.AreEqual(18.0, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.AreEqual(99.0, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test expression 2.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test expression 2")]
        public void TestSample2()
        {
            KeyRange<short> keyRange = KeyExpressionEvaluator<short, string>.GetKeyRange(
                x => 18 <= x.Key && x.Key < 99 && x.Key > 7 && x.Key <= 99 && (0 == x.Key % 2));
            Assert.AreEqual<short>(18, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.AreEqual<short>(99, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test expression 3.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test expression 3")]
        public void TestSample3()
        {
            KeyRange<uint> keyRange = KeyExpressionEvaluator<uint, string>.GetKeyRange(
                x => ConstMember <= x.Key && x.Key < 99 && x.Key > 7 && x.Key <= 99 && x.Value.Length == 2);
            Assert.AreEqual(18U, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.AreEqual(99U, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test expression 4.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test expression 4")]
        public void TestSample4()
        {
            staticMember = 18;
            KeyRange<ulong> keyRange = KeyExpressionEvaluator<ulong, string>.GetKeyRange(
                x => staticMember <= x.Key && x.Key < 99 && x.Key > 7 && x.Key <= 99 && x.Value == "bar");
            Assert.AreEqual(18UL, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.AreEqual(99UL, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test expression 5.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test expression 5")]
        public void TestSample5()
        {
            DateTime date = DateTime.UtcNow;
            TimeSpan timespan = TimeSpan.FromDays(90);

            KeyRange<DateTime> keyRange = KeyExpressionEvaluator<DateTime, string>.GetKeyRange(d => d.Key >= date && d.Key <= date + timespan);
            Assert.AreEqual(date, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.AreEqual(date + timespan, keyRange.Max.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }
    }
}
