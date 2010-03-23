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
    using System.Collections.Generic;
    using System.Linq.Expressions;
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
        /// Verify a NOT of an EQ removes limits.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify a NOT of == removes limits")]
        public void VerifyNotOfEqRemovesLimits()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => !(x.Key == 3));
            Assert.IsNull(keyRange.Min);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Verify a NOT of an NE doesn't work.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify a NOT of != doesn't work (provides no limits)")]
        public void VerifyNotOfNeDoesntWork()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => !(x.Key != 3));
            Assert.IsNull(keyRange.Min);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Verify a NOT of an LT gives GE.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify a NOT of < gives >=")]
        public void VerifyNotOfLtGivesGe()
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => !(x.Key < 4));
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
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => !(x.Key <= 4));
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
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => !(x.Key > 4));
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
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, string>.GetKeyRange(x => !(x.Key >= 4));
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(4, keyRange.Max.Value);
            Assert.IsFalse(keyRange.Max.IsInclusive);
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
        /// Verify no constant folding with functions.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify no constant folding with functions")]
        public void VerifyNoConstantFoldingWithFunctions()
        {
            var rnd = new Random();
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, long>.GetKeyRange(x => x.Key <= rnd.Next());
            Assert.IsNull(keyRange.Min);
            Assert.IsNull(keyRange.Max);
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
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, int>.GetKeyRange(x => k.Key == x.Key);
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
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, int>.GetKeyRange(x => x.Key == x.Key);
            Assert.IsNull(keyRange.Min);
            Assert.IsNull(keyRange.Max);
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

            // This fails because the Key is promoted to an int and the KeyExpressionEvaluator
            // can't handle that.
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

        /// <summary>
        /// Test expression 6.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test expression 6")]
        public void TestSample6()
        {
            KeyRange<double> keyRange = KeyExpressionEvaluator<double, string>.GetKeyRange(d => d.Key >= 5.0 && !(d.Key <= 10.0));
            Assert.AreEqual(10.0, keyRange.Min.Value);
            Assert.IsFalse(keyRange.Min.IsInclusive);
            Assert.IsNull(keyRange.Max);
        }

        /// <summary>
        /// Common test for constant folding tests.
        /// </summary>
        /// <param name="expression">
        /// An expression which should come out to x.Key LE 32
        /// </param>
        private static void ConstantFoldingHelper(Expression<Predicate<KeyValuePair<int, long>>> expression)
        {
            KeyRange<int> keyRange = KeyExpressionEvaluator<int, long>.GetKeyRange(expression);
            Assert.IsNull(keyRange.Min);
            Assert.AreEqual(32, keyRange.Max.Value);
            Assert.IsTrue(keyRange.Max.IsInclusive);
        }
    }
}
