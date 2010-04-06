// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeyValueExpressionEvaluatorTests.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace EsentCollectionsTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Microsoft.Isam.Esent.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test evaluation of KeyValue expressions.
    /// </summary>
    [TestClass]
    public class KeyValueExpressionEvaluatorTests
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
            KeyRange<short> keyRange = KeyValueExpressionEvaluator<short, decimal>.GetKeyRange(null);
        }

        /// <summary>
        /// Verify that a true expression gives an open range.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a true expression gives an open range")]
        public void VerifyTrueExpressionGivesOpenRange()
        {
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => true);
            Assert.AreEqual(KeyRange<int>.OpenRange, keyRange);
        }

        /// <summary>
        /// Verify that an expression without ranges gives an open range.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that an expression without ranges gives an open range")]
        public void VerifyExpressionWithoutRangeGivesOpenRange()
        {
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => (0 == (x.Key % 2)));
            Assert.AreEqual(KeyRange<int>.OpenRange, keyRange);
        }

        /// <summary>
        /// Verify that an expression on the value gives an open range.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that an expression without ranges gives an open range")]
        public void VerifyValueExpressionGivesNoLimits()
        {
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, int>.GetKeyRange(x => x.Value < 100);
            Assert.AreEqual(KeyRange<int>.OpenRange, keyRange);
        }

        /// <summary>
        /// Verify that a LT comparison gives a max limit.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a < comparison gives a max limit")]
        public void VerifyLtComparisonGivesMaxLimit()
        {
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => x.Key < 10);
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
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => x.Key <= 11);
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
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => 10 > x.Key);
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
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => 11 >= x.Key);
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
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => x.Key < 3 && x.Key < 2 && x.Key < 5);
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
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => x.Key <= 1 && 2 >= x.Key && x.Key <= 3);
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
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => x.Key <= 11 && x.Key < 11);
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
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => x.Key > 10);
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
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => x.Key >= 11);
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
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => 10 < x.Key);
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
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => 11 <= x.Key);
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
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => x.Key > 3 && x.Key > 2 && x.Key > 5);
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
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => x.Key >= 1 && 2 <= x.Key && x.Key >= 3);
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
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => x.Key >= 11 && x.Key > 11);
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
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => x.Key == 7);
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
            KeyRange<long> keyRange = KeyValueExpressionEvaluator<long, string>.GetKeyRange(x => 8 == x.Key);
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
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => 19 < x.Key && x.Key < 101);
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
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => 19 <= x.Key && x.Key <= 101 && x.Value.StartsWith("foo"));
            Assert.AreEqual(19, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.AreEqual(101, keyRange.Max.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Verify an AND clause intersects limits.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify an AND clause intersects limits")]
        public void VerifyAndIntersectsLimits()
        {
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => 19 <= x.Key && x.Key <= 101 && 21 < x.Key && x.Key < 99);
            Assert.AreEqual(21, keyRange.Min.Value);
            Assert.IsFalse(keyRange.Min.IsInclusive);
            Assert.AreEqual(99, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Verify an OR clause unions limits.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify an OR clause unions limits")]
        public void VerifyOrUnionsLimits()
        {
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => (19 <= x.Key && x.Key <= 101) || x.Key > 200);
            Assert.AreEqual(19, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Verify an OR clause removes limits.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify an OR clause removes limits")]
        public void VerifyOrRemovesLimits()
        {
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => (19 <= x.Key && x.Key <= 101) || x.Value.StartsWith("foo"));
            Assert.AreEqual(KeyRange<int>.OpenRange, keyRange);
        }

        /// <summary>
        /// Verify a NOT of an EQ removes limits.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify a NOT of == removes limits")]
        public void VerifyNotOfEqRemovesLimits()
        {
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => !(x.Key == 3));
            Assert.AreEqual(KeyRange<int>.OpenRange, keyRange);
        }

        /// <summary>
        /// Verify a NOT of an NE doesn't work.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify a NOT of != doesn't work (provides no limits)")]
        public void VerifyNotOfNeDoesntWork()
        {
            KeyRange<int> actual = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => !(x.Key != 3));
            KeyRange<int> expected = new KeyRange<int>(Key<int>.CreateKey(3, true), Key<int>.CreateKey(3, true));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Verify a NOT of an LT gives GE.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify a NOT of < gives >=")]
        public void VerifyNotOfLtGivesGe()
        {
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => !(x.Key < 4));
            Assert.AreEqual(4, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Verify a NOT of an LE gives GT.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify a NOT of <= gives >")]
        public void VerifyNotOfLeGivesGt()
        {
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => !(x.Key <= 4));
            Assert.AreEqual(4, keyRange.Min.Value);
            Assert.IsFalse(keyRange.Min.IsInclusive);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Verify a NOT of an GT gives LE.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify a NOT of > gives <=")]
        public void VerifyNotOfGtGivesLe()
        {
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => !(x.Key > 4));
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(4, keyRange.Max.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Verify a NOT of an GE gives LT.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify a NOT of >= gives <")]
        public void VerifyNotOfGeGivesLt()
        {
            KeyRange<int> actual = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => !(x.Key >= 4));
            KeyRange<int> expected = new KeyRange<int>(null, Key<int>.CreateKey(4, false));
        }

        /// <summary>
        /// Verify DeMorgans law works for OR.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify DeMorgans law works for ||")]
        public void VerifyDeMorgansForOr()
        {
            // This should be the same as (x.Key >= 11 && x.Key < 22)
            KeyRange<int> actual = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => !(x.Key < 11 || x.Key >= 22));
            KeyRange<int> expected = new KeyRange<int>(Key<int>.CreateKey(11, true), Key<int>.CreateKey(22, false));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Verify DeMorgans law works for AND.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify DeMorgans law works for &&")]
        public void VerifyDeMorgansForAnd()
        {
            // This should be the same as (x.Key > 11 || x.Key > 22)
            KeyRange<int> actual = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => !(x.Key <= 11 && x.Key <= 22));
            KeyRange<int> expected = new KeyRange<int>(Key<int>.CreateKey(11, false), null);
            Assert.AreEqual(expected, actual);
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
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, long>.GetKeyRange(x => x.Key <= i);
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
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, long>.GetKeyRange(x => x.Key <= this.member);
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(this.member, keyRange.Max.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Verify constant folding with functions.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify constant folding with functions")]
        public void VerifyConstantFoldingWithFunctions()
        {
            var rnd = new Random();
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, long>.GetKeyRange(x => x.Key <= rnd.Next());
            Assert.IsNull(keyRange.Min);
            Assert.IsNotNull(keyRange.Max);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Verify constant folding with add.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify constant folding (add)")]
        public void VerifyConstantFoldingAdd()
        {
            short i = 22;
            ConstantFoldingHelper(x => x.Key <= i + 10);
        }

        /// <summary>
        /// Verify constant folding with checked add.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify constant folding (checked add)")]
        public void VerifyConstantFoldingCheckedAdd()
        {
            short i = 22;
            ConstantFoldingHelper(x => x.Key <= checked(i + 10));
        }

        /// <summary>
        /// Verify constant folding with minus.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify constant folding (minus)")]
        public void VerifyConstantFoldingMinus()
        {
            int i = 42;
            ConstantFoldingHelper(x => x.Key <= i - 10);
        }

        /// <summary>
        /// Verify constant folding with checked minus.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify constant folding (checked minus)")]
        public void VerifyConstantFoldingCheckedMinus()
        {
            int i = 42;
            ConstantFoldingHelper(x => x.Key <= checked(i - 10));
        }

        /// <summary>
        /// Verify constant folding with multiply.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify constant folding (multiply)")]
        public void VerifyConstantFoldingMultiply()
        {
            int i = 16;
            ConstantFoldingHelper(x => x.Key <= i * 2);
        }

        /// <summary>
        /// Verify constant folding with checked multiply.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify constant folding (checked multiply)")]
        public void VerifyConstantFoldingCheckedMultiply()
        {
            int i = 16;
            ConstantFoldingHelper(x => x.Key <= checked(i * 2));
        }

        /// <summary>
        /// Verify constant folding with divide.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify constant folding (divide)")]
        public void VerifyConstantFoldingDivide()
        {
            int i = 320;
            ConstantFoldingHelper(x => x.Key <= i / 10);
        }

        /// <summary>
        /// Verify constant folding with checked divide.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify constant folding (checked divide)")]
        public void VerifyConstantFoldingCheckedDivide()
        {
            int i = 320;
            ConstantFoldingHelper(x => x.Key <= checked(i / 10));
        }

        /// <summary>
        /// Verify constant folding with negate.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify constant folding (negate)")]
        public void VerifyConstantFoldingNegate()
        {
            int i = -32;
            ConstantFoldingHelper(x => x.Key <= -i);
        }

        /// <summary>
        /// Verify constant folding with checked negate.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify constant folding (checked negate)")]
        public void VerifyConstantFoldingCheckedNegate()
        {
            int i = -32;
            ConstantFoldingHelper(x => x.Key <= checked(-i));
        }

        /// <summary>
        /// Verify constant folding with mod.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify constant folding (mod)")]
        public void VerifyConstantFoldingMod()
        {
            int i = 128;
            ConstantFoldingHelper(x => x.Key <= 32 % i);
        }

        /// <summary>
        /// Verify constant folding with right shift.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify constant folding (right shift)")]
        public void VerifyConstantFoldingRightShift()
        {
            int i = 128;
            int j = 2;
            ConstantFoldingHelper(x => x.Key <= i >> j);
        }

        /// <summary>
        /// Verify constant folding with left shift.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify constant folding (left shift)")]
        public void VerifyConstantFoldingLeftShift()
        {
            int i = 16;
            const int One = 1;
            ConstantFoldingHelper(x => x.Key <= i << One);
        }

        /// <summary>
        /// Verify constant folding with and.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify constant folding (and)")]
        public void VerifyConstantFoldingAnd()
        {
            int i = 33;
            ConstantFoldingHelper(x => x.Key <= (i & 32));
        }

        /// <summary>
        /// Verify constant folding with or.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify constant folding (or)")]
        public void VerifyConstantFoldingOr()
        {
            int i = 0;
            ConstantFoldingHelper(x => x.Key <= (i | 32));
        }

        /// <summary>
        /// Verify constant folding with xor.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify constant folding (xor)")]
        public void VerifyConstantFoldingXor()
        {
            int i = 33;
            int j = 1;
            ConstantFoldingHelper(x => x.Key <= (i ^ j));
        }

        /// <summary>
        /// Verify constant folding with not.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify constant folding (not)")]
        public void VerifyConstantFoldingNot()
        {
            int i = ~32;
            ConstantFoldingHelper(x => x.Key <= ~i);
        }

        /// <summary>
        /// Verify that key access only works for the parameter.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify key access only works for the parameter")]
        public void VerifyKeyAccessIsForParameterOnly()
        {
            var k = new KeyValuePair<int, int>(1, 2);
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, int>.GetKeyRange(x => k.Key == x.Key);
            Assert.AreEqual(1, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.AreEqual(1, keyRange.Max.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test key access against the key.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test key access against the key")]
        public void TestKeyAccessAgainstSelf()
        {
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, int>.GetKeyRange(x => x.Key == x.Key);
            Assert.AreEqual(KeyRange<int>.OpenRange, keyRange);
        }

        /// <summary>
        /// Verify conditional access is optimized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify conditional access is optimized")]
        public void VerifyConditionalParameterAccessIsOptimized()
        {
            KeyValuePair<int, string> kvp1 = new KeyValuePair<int, string>(0, "hello");
            KeyValuePair<int, string> kvp2 = new KeyValuePair<int, string>(1, "hello");
            KeyRange<int> keyRange =
                KeyValueExpressionEvaluator<int, string>.GetKeyRange(
                    x => x.Key < (0 == DateTime.Now.Ticks ? kvp1 : kvp2).Key);
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(1, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Verify conditional parameter access is recognized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify conditional parameter access is recognized")]
        public void VerifyConditionalParameterAccessIsRecognized()
        {
            KeyValuePair<int, string> kvp = new KeyValuePair<int, string>(0, "hello");
            KeyRange<int> keyRange =
                KeyValueExpressionEvaluator<int, string>.GetKeyRange(
                    x => x.Key < (0 == DateTime.Now.Ticks ? x : kvp).Key);
            Assert.AreEqual(KeyRange<int>.OpenRange, keyRange);
        }

        /// <summary>
        /// Verify array access is optimized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify array access is optimized")]
        public void VerifyArrayAccessIsOptimized()
        {
            KeyValuePair<int, string> kvp = new KeyValuePair<int, string>(1, "hello");
            KeyRange<int> keyRange =
                KeyValueExpressionEvaluator<int, string>.GetKeyRange(
                    x => x.Key < (new[] { kvp })[0].Key);
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(1, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Verify array parameter access is recognized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify array parameter access is recognized")]
        public void VerifyArrayParameterAccessIsRecognized()
        {
            KeyRange<int> keyRange =
                KeyValueExpressionEvaluator<int, string>.GetKeyRange(
                    x => x.Key < (new[] { x })[0].Key);
            Assert.AreEqual(KeyRange<int>.OpenRange, keyRange);
        }

        /// <summary>
        /// Verify delegate access is optimized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify delegate access is optimized")]
        public void VerifyDelegateAccessIsOptimized()
        {
            Func<int, int> f = x => x * 2;
            KeyRange<int> keyRange =
                KeyValueExpressionEvaluator<int, string>.GetKeyRange(
                    x => x.Key <= f(1));
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(2, keyRange.Max.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Verify delegate parameter access is recognized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify delegate parameter access is recognized")]
        public void VerifyDelegateParameterAccessIsRecognized()
        {
            Func<KeyValuePair<int, string>, int> f = x => x.Key;
            KeyRange<int> keyRange =
                KeyValueExpressionEvaluator<int, string>.GetKeyRange(
                    x => x.Key < f(x));
            Assert.AreEqual(KeyRange<int>.OpenRange, keyRange);
        }

        /// <summary>
        /// Verify method call parameter access is optimized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify method call access is optimized")]
        public void VerifyMethodCallAccessIsOptimized()
        {
            KeyRange<int> keyRange =
                KeyValueExpressionEvaluator<int, string>.GetKeyRange(
                    x => x.Key < (String.IsNullOrEmpty("foo") ? 0 : 1));
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(1, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Verify method call parameter access is recognized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify method call parameter access is recognized")]
        public void VerifyMethodCallParameterAccessIsRecognized()
        {
            Func<KeyValuePair<int, string>, int> f = x => x.Key;
            KeyRange<int> keyRange =
                KeyValueExpressionEvaluator<int, string>.GetKeyRange(
                    x => x.Key < (String.IsNullOrEmpty(x.Value) ? 0 : 1));
            Assert.AreEqual(KeyRange<int>.OpenRange, keyRange);
        }

        /// <summary>
        /// Verify method call parameter access is recognized (2).
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify method call parameter access is recognized (2)")]
        public void VerifyMethodCallParameterAccessIsRecognized2()
        {
            Func<KeyValuePair<int, string>, int> f = x => x.Key;
            KeyRange<int> keyRange =
                KeyValueExpressionEvaluator<int, string>.GetKeyRange(
                    x => x.Key < Math.Max(0, "foo".StartsWith("f") ? 10 : x.Key));
            Assert.AreEqual(KeyRange<int>.OpenRange, keyRange);
        }

        /// <summary>
        /// Verify method call object access is recognized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify method call object access is recognized")]
        public void VerifyMethodCallObjectAccessIsRecognized()
        {
            KeyRange<int> keyRange =
                KeyValueExpressionEvaluator<int, string>.GetKeyRange(
                    x => x.Key < x.Key.GetHashCode());
            Assert.AreEqual(KeyRange<int>.OpenRange, keyRange);
        }

        /// <summary>
        /// Test Boolean expression (true).
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test Bool expression (true)")]
        public void TestTrueBoolExpression()
        {
            KeyRange<bool> keyRange = KeyValueExpressionEvaluator<bool, string>.GetKeyRange(x => x.Key == true);
            Assert.AreEqual(true, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.AreEqual(true, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
        }

        /// <summary>
        /// Test Boolean expression (false).
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test Bool expression (false)")]
        public void TestFalseBoolExpression()
        {
            KeyRange<bool> keyRange = KeyValueExpressionEvaluator<bool, string>.GetKeyRange(x => x.Key == false);
            Assert.AreEqual(false, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.AreEqual(false, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
        }

        /// <summary>
        /// Test Byte expression
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test Byte expression")]
        public void TestByteExpression()
        {
            KeyRange<byte> keyRange = KeyValueExpressionEvaluator<byte, string>.GetKeyRange(x => x.Key < 2);

            // This fails because the Key is promoted and the KeyValueExpressionEvaluator doesn't handle that.
            // Arguably this isn't currently critical because we can only have 2^8 records with this type of key.
            Assert.AreEqual(
                new KeyRange<byte>(null, Key<byte>.CreateKey(2, false)),
                keyRange,
                "Byte promotion not handled");
        }

        /// <summary>
        /// Test Int16 expression
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test Int16 expression")]
        public void TestInt16Expression()
        {
            KeyRange<short> keyRange = KeyValueExpressionEvaluator<short, string>.GetKeyRange(x => x.Key < 2);

            // This fails because the Key is promoted and the KeyValueExpressionEvaluator doesn't handle that.
            // Arguably this isn't currently critical because we can only have 2^16 records with this type of key.
            Assert.AreEqual(
                new KeyRange<short>(null, Key<short>.CreateKey(2, false)),
                keyRange,
                "Int16 promotion not handled");
        }

        /// <summary>
        /// Test UInt16 expression
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test UInt16 expression")]
        public void TestUInt16Expression()
        {
            KeyRange<ushort> keyRange = KeyValueExpressionEvaluator<ushort, string>.GetKeyRange(x => x.Key < 2);

            // This fails because the Key is promoted and the KeyValueExpressionEvaluator doesn't handle that.
            // Arguably this isn't currently critical because we can only have 2^16 records with this type of key.
            Assert.AreEqual(
                new KeyRange<ushort>(null, Key<ushort>.CreateKey(2, false)),
                keyRange,
                "UInt16 promotion not handled");
        }

        /// <summary>
        /// Test Int32 expression
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test Int32 expression")]
        public void TestInt32Expression()
        {
            KeyRange<int> keyRange = KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => x.Key < 2);
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(2, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test UInt32 expression
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test UInt32 expression")]
        public void TestUInt32Expression()
        {
            KeyRange<uint> keyRange = KeyValueExpressionEvaluator<uint, string>.GetKeyRange(x => x.Key < 2);
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(2U, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test Int64 expression
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test Int64 expression")]
        public void TestInt64Expression()
        {
            KeyRange<long> keyRange = KeyValueExpressionEvaluator<long, string>.GetKeyRange(x => x.Key < 2);
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(2L, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test UInt64 expression
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test UInt64 expression")]
        public void TestUInt64Expression()
        {
            KeyRange<ulong> keyRange = KeyValueExpressionEvaluator<ulong, string>.GetKeyRange(x => x.Key < 2);
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(2UL, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test float expression
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test float expression")]
        public void TestFloatExpression()
        {
            KeyRange<float> keyRange = KeyValueExpressionEvaluator<float, string>.GetKeyRange(x => x.Key < 2.0f);
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(2.0f, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test double expression
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test double expression")]
        public void TestDoubleExpression()
        {
            KeyRange<double> keyRange = KeyValueExpressionEvaluator<double, string>.GetKeyRange(x => x.Key < 2.0);
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(2.0, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test TimeSpan expression
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test TimeSpan expression")]
        public void TestTimeSpanExpression()
        {
            KeyRange<TimeSpan> keyRange = KeyValueExpressionEvaluator<TimeSpan, string>.GetKeyRange(x => x.Key < TimeSpan.FromSeconds(2));
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(TimeSpan.FromSeconds(2), keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test DateTime expression
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test DateTime expression")]
        public void TestDateTimeExpression()
        {
            DateTime dateTime = DateTime.UtcNow;
            KeyRange<DateTime> keyRange = KeyValueExpressionEvaluator<DateTime, string>.GetKeyRange(x => x.Key < dateTime);
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(dateTime, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test Guid expression
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test Guid expression")]
        public void TestGuidExpression()
        {
            Guid guid = Guid.NewGuid();
            KeyRange<Guid> keyRange = KeyValueExpressionEvaluator<Guid, string>.GetKeyRange(x => x.Key == guid);
            Assert.AreEqual(guid, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
            Assert.AreEqual(guid, keyRange.Max.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test String equals expression
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test String == expression")]
        public void TestStringEquality()
        {
            string s = "foo";
            KeyRange<string> keyRange = KeyValueExpressionEvaluator<string, string>.GetKeyRange(x => s == x.Key);
            Assert.AreEqual(s, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
            Assert.AreEqual(s, keyRange.Max.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test String.Equals expression
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test String.Equals")]
        public void TestStringEquals()
        {
            string s = "baz";
            KeyRange<string> keyRange = KeyValueExpressionEvaluator<string, string>.GetKeyRange(x => x.Key.Equals(s));
            Assert.AreEqual(s, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
            Assert.AreEqual(s, keyRange.Max.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test String.StartsWith expression
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test String.StartWith")]
        public void TestStringStartsWith()
        {
            string s = "baz";
            KeyRange<string> actual = KeyValueExpressionEvaluator<string, string>.GetKeyRange(x => x.Key.StartsWith(s));
            var expected = new KeyRange<string>(Key<string>.CreateKey("baz", true), Key<string>.CreatePrefixKey("baz"));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test String.StartsWith intersection
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test String.StartWith intersection")]
        public void TestStringStartsWithIntersection()
        {
            string s = "baz";
            KeyRange<string> actual = KeyValueExpressionEvaluator<string, string>.GetKeyRange(x => x.Key.StartsWith(s) && x.Key.CompareTo("z") < 0);
            var expected = new KeyRange<string>(Key<string>.CreateKey(s, true), Key<string>.CreatePrefixKey(s));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test String.StartsWith union
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test String.StartWith union")]
        public void TestStringStartsWithUnion()
        {
            KeyRange<string> actual =
                KeyValueExpressionEvaluator<string, string>.GetKeyRange(
                    x => x.Key.StartsWith("b") || (x.Key.CompareTo("a") > 0 && x.Key.CompareTo("b") <= 0));
            var expected = new KeyRange<string>(Key<string>.CreateKey("a", false), Key<string>.CreatePrefixKey("b"));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test String.CompareTo
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test String.CompareTo")]
        public void TestStringCompareTo()
        {
            KeyRange<string> keyRange = KeyValueExpressionEvaluator<string, string>.GetKeyRange(x => x.Key.CompareTo("foo") < 0);
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual("foo", keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test String.CompareTo reversed
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test String.CompareTo reversed")]
        public void TestStringCompareToReversed()
        {
            KeyRange<string> keyRange = KeyValueExpressionEvaluator<string, string>.GetKeyRange(x => 0 <= x.Key.CompareTo("bar"));
            Assert.AreEqual("bar", keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Test String.CompareTo argument folding
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test String.CompareTo argument folding")]
        public void TestStringCompareToArgumentFolding()
        {
            KeyRange<string> keyRange = KeyValueExpressionEvaluator<string, string>.GetKeyRange(x => 0 <= x.Key.CompareTo("foo" + "bar"));
            Assert.AreEqual("foobar", keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Test CompareTo with non-string
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test CompareTo with non-string")]
        public void TestCompareToNonString()
        {
            KeyRange<long> keyRange =
                KeyValueExpressionEvaluator<long, string>.GetKeyRange(
                    x => x.Key.CompareTo(7) < 0 && x.Key.CompareTo(-8) >= 0);
            Assert.AreEqual(-8, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.AreEqual(7, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test CompareTo with non-zero comparand
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test CompareTo with non-zero comparand")]
        public void TestCompareToNonZeroComparand()
        {
            KeyRange<int> keyRange =
                KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => x.Key.CompareTo(7) < 1);
            Assert.IsNull(keyRange.Min);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Test CompareTo with the wrong type
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test CompareTo the wrong type")]
        public void TestCompareToWrongType()
        {
            KeyRange<int> keyRange =
                KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => x.Key.CompareTo(Guid.NewGuid()) < 0);
            Assert.IsNull(keyRange.Min);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Test CompareTo without parameter access
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test CompareTo with no parameter access")]
        public void TestCompareToNoParameter()
        {
            KeyRange<int> keyRange =
                KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => 0 < 5.CompareTo(4));
            Assert.AreEqual(keyRange, KeyRange<int>.OpenRange);
        }

        /// <summary>
        /// Test String.Compare
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test String.Compare")]
        public void TestStringCompare()
        {
            KeyRange<string> keyRange =
                KeyValueExpressionEvaluator<string, string>.GetKeyRange(x => String.Compare(x.Key, "foo") > 0);
            Assert.AreEqual("foo", keyRange.Min.Value);
            Assert.IsFalse(keyRange.Min.IsInclusive);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Test String.Compare 2
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test String.Compare 2")]
        public void TestStringCompare2()
        {
            KeyRange<string> keyRange =
                KeyValueExpressionEvaluator<string, string>.GetKeyRange(x => String.Compare("foo", x.Key) <= 0);
            Assert.AreEqual("foo", keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Test String.Compare 3
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test String.Compare 3")]
        public void TestStringCompare3()
        {
            KeyRange<string> keyRange =
                KeyValueExpressionEvaluator<string, string>.GetKeyRange(x => 0 == String.Compare(x.Key, "b" + "ar"));
            Assert.AreEqual("bar", keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.AreEqual("bar", keyRange.Max.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test String.Compare 4
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test String.Compare 4")]
        public void TestStringCompare4()
        {
            KeyRange<string> keyRange =
                KeyValueExpressionEvaluator<string, string>.GetKeyRange(x => 0 > String.Compare(5.ToString(), x.Key));
            Assert.AreEqual("5", keyRange.Min.Value);
            Assert.IsFalse(keyRange.Min.IsInclusive);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Test String.Compare without parameter access
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test String.Compare without parameter access")]
        public void TestStringCompareWithoutParameterAccess()
        {
            KeyRange<string> keyRange =
                KeyValueExpressionEvaluator<string, string>.GetKeyRange(x => 0 > String.Compare("a", "b"));
            Assert.AreEqual(keyRange, KeyRange<string>.OpenRange);
        }

        /// <summary>
        /// Test expression 1.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test expression 1")]
        public void TestSample1()
        {
            KeyRange<double> keyRange = KeyValueExpressionEvaluator<double, string>.GetKeyRange(
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
            KeyRange<int> keyRange =
                KeyValueExpressionEvaluator<int, string>.GetKeyRange(x => x.Key < 1 && x.Key < 2 && x.Key < 3);

            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(1, keyRange.Max.Value);
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
            KeyRange<uint> keyRange = KeyValueExpressionEvaluator<uint, string>.GetKeyRange(
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
            KeyRange<ulong> keyRange = KeyValueExpressionEvaluator<ulong, string>.GetKeyRange(
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

            KeyRange<DateTime> keyRange = KeyValueExpressionEvaluator<DateTime, string>.GetKeyRange(d => d.Key >= date && d.Key <= date + timespan);
            Assert.AreEqual(date, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.AreEqual(date + timespan, keyRange.Max.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test expression 6.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test expression 6")]
        public void TestSample6()
        {
            KeyRange<double> keyRange = KeyValueExpressionEvaluator<double, string>.GetKeyRange(d => d.Key >= 5.0 && !(d.Key <= 10.0));
            Assert.AreEqual(10.0, keyRange.Min.Value);
            Assert.IsFalse(keyRange.Min.IsInclusive);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Test expression 7.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test expression 7")]
        public void TestSample7()
        {
            KeyRange<float> keyRange = KeyValueExpressionEvaluator<float, string>.GetKeyRange(d => d.Key >= 5.0F && d.Key <= 10.0F);
            Assert.AreEqual(5.0F, keyRange.Min.Value);
            Assert.IsTrue(keyRange.Min.IsInclusive);
            Assert.AreEqual(10.0F, keyRange.Max.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test expression 8.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test expression 8")]
        public void TestSample8()
        {
            KeyRange<int> keyRange =
                KeyValueExpressionEvaluator<int, int>.GetKeyRange(
                    i => i.Key < 100 && i.Key > -50 && i.Key < i.Value && i.Key < Math.Min(50, 100));
            Assert.AreEqual(-50, keyRange.Min.Value);
            Assert.IsFalse(keyRange.Min.IsInclusive);
            Assert.AreEqual(50, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
        }

        /// <summary>
        /// Test expression 9.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test expression 9")]
        public void TestSample9()
        {
            // Regression test for:
            //   KeyRange is too small.
            //   Expression:
            //     x => Not(((2137 >= x.Key) && Not((-3611 = x.Key))))
            KeyRange<int> actual =
                KeyValueExpressionEvaluator<int, int>.GetKeyRange(
                    x => !((2137 >= x.Key) && !(-3611 == x.Key)));
            KeyRange<int> expected = new KeyRange<int>(Key<int>.CreateKey(-3611, true), null);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test expression 10.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test expression 10")]
        public void TestSample10()
        {
            // Regression test for:
            //   Assert.IsTrue failed.
            //   Error at entry 21. Not enough entries in actual.
            //   First missing entry is [ibh, ibh]
            //    expression = x => (x.Key.StartsWith("i") || (x.Key.Equals("ibg") || ("f" = x.Key)))
            KeyRange<string> actual =
                KeyValueExpressionEvaluator<string, string>.GetKeyRange(
                    x => (x.Key.StartsWith("i") || (x.Key.Equals("ibg") || ("f" == x.Key))));
            KeyRange<string> expected = new KeyRange<string>(Key<string>.CreateKey("f", true), Key<string>.CreatePrefixKey("i"));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test expression 11.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test expression 11")]
        public void TestSample11()
        {
            // Regression test for:
            //   Assert.IsTrue failed.
            //   Error at entry 0. Not enough entries in actual.
            //   First missing entry is [ggi, ggi] 
            //    expression = x => ((Compare("ggi", x.Key) <= 0) && (x.Key.Equals("fag") || x.Key.StartsWith("g")))
            KeyRange<string> actual =
                KeyValueExpressionEvaluator<string, string>.GetKeyRange(
                    x => ((String.Compare("ggi", x.Key) <= 0) && (x.Key.Equals("fag") || x.Key.StartsWith("g"))));
            KeyRange<string> expected = new KeyRange<string>(
                Key<string>.CreateKey("ggi", true),
                Key<string>.CreatePrefixKey("g"));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Common test for constant folding tests.
        /// </summary>
        /// <param name="expression">
        /// An expression which should come out to x.Key LE 32
        /// </param>
        private static void ConstantFoldingHelper(Expression<Predicate<KeyValuePair<int, long>>> expression)
        {
            KeyRange<int> actual = KeyValueExpressionEvaluator<int, long>.GetKeyRange(expression);
            KeyRange<int> expected = new KeyRange<int>(null, Key<int>.CreateKey(32, true));
            Assert.AreEqual(expected, actual);
        }
    }
}
