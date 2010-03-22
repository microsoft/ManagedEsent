// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeyRangeTests.cs" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//   Test the Key class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EsentCollectionsTests
{
    using System;
    using Microsoft.Isam.Esent.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the KeyRange class.
    /// </summary>
    [TestClass]
    public class KeyRangeTests
    {
        /// <summary>
        /// Verify the KeyRange constructor sets the members.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify the KeyRange constructor sets the members")]
        public void VerifyKeyRangeConstructorSetsMembers()
        {
            var keyrange = new KeyRange<int>(new Key<int>(1, true), new Key<int>(2, false));
            Assert.IsTrue(keyrange.Min.Equals(new Key<int>(1, true)));
            Assert.IsTrue(keyrange.Max.Equals(new Key<int>(2, false)));
        }

        /// <summary>
        /// Call KeyRange.ToString() with an empty key range.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Call KeyRange.ToString with an empty range")]
        public void TestNullKeyRangeToString()
        {
            var keyrange = new KeyRange<int>(null, null);
            string s = keyrange.ToString();
            Assert.IsNotNull(s);
            Assert.AreNotEqual(s, String.Empty);
        }

        /// <summary>
        /// Call KeyRange.ToString() with an exclusive range.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Call KeyRange.ToString with an exclusive range")]
        public void TestKeyRangeToStringExclusive()
        {
            var keyrange = new KeyRange<int>(new Key<int>(1, false), new Key<int>(2, false));
            string s = keyrange.ToString();
            Assert.IsNotNull(s);
            Assert.AreNotEqual(s, String.Empty);
            StringAssert.Contains(s, "1");
            StringAssert.Contains(s, "2");
            StringAssert.Contains(s, "exclusive");
        }

        /// <summary>
        /// Call KeyRange.ToString() with an inclusive range.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Call KeyRange.ToString with an inclusive range")]
        public void TestKeyRangeToStringInclusive()
        {
            var keyrange = new KeyRange<int>(new Key<int>(3, true), new Key<int>(4, true));
            string s = keyrange.ToString();
            Assert.IsNotNull(s);
            Assert.AreNotEqual(s, String.Empty);
            StringAssert.Contains(s, "3");
            StringAssert.Contains(s, "4");
            StringAssert.Contains(s, "inclusive");
        }

        /// <summary>
        /// Verify the KeyRange.Equals returns false for null.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify the KeyRange.Equals returns false for null")]
        public void VerifyKeyRangeEqualsNullIsFalse()
        {
            var keyrange = new KeyRange<int>(new Key<int>(1, true), new Key<int>(2, false));
            Assert.IsFalse(keyrange.Equals(null));
        }

        /// <summary>
        /// Verify a KeyRange equals itself.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify a KeyRange equals itself")]
        public void VerifyKeyRangeEqualsSelf()
        {
            var keyrange = new KeyRange<int>(new Key<int>(1, true), new Key<int>(2, false));
            KeyRangeEqualityHelper(keyrange, keyrange, true);
        }

        /// <summary>
        /// Verify a KeyRange equals a range with the same values.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify a KeyRange equals a range with the same values")]
        public void VerifyKeyRangeEqualsSameValues()
        {
            var keyrange1 = new KeyRange<int>(new Key<int>(1, true), new Key<int>(2, false));
            var keyrange2 = new KeyRange<int>(new Key<int>(1, true), new Key<int>(2, false));
            KeyRangeEqualityHelper(keyrange1, keyrange2, true);
        }

        /// <summary>
        /// KeyRange.Equals test 1
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("KeyRange.Equals test 1 (null min vs non-null min)")]
        public void VerifyKeyRangeNotEquals1()
        {
            var keyrange1 = new KeyRange<int>(null, new Key<int>(2, false));
            var keyrange2 = new KeyRange<int>(new Key<int>(1, true), new Key<int>(2, false));
            KeyRangeEqualityHelper(keyrange1, keyrange2, false);
        }

        /// <summary>
        /// KeyRange.Equals test 2
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("KeyRange.Equals test 2 (null max vs non-null max)")]
        public void VerifyKeyRangeNotEquals2()
        {
            var keyrange1 = new KeyRange<int>(new Key<int>(1, true), null);
            var keyrange2 = new KeyRange<int>(new Key<int>(1, true), new Key<int>(2, false));
            KeyRangeEqualityHelper(keyrange1, keyrange2, false);
        }

        /// <summary>
        /// KeyRange.Equals test 3
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("KeyRange.Equals test 3 (different mins)")]
        public void VerifyKeyRangeNotEquals3()
        {
            var keyrange1 = new KeyRange<int>(new Key<int>(1, false), new Key<int>(2, false));
            var keyrange2 = new KeyRange<int>(new Key<int>(1, true), new Key<int>(2, false));
            KeyRangeEqualityHelper(keyrange1, keyrange2, false);
        }

        /// <summary>
        /// KeyRange.Equals test 4
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("KeyRange.Equals test 4 (different maxs)")]
        public void VerifyKeyRangeNotEquals4()
        {
            var keyrange1 = new KeyRange<int>(new Key<int>(1, true), new Key<int>(2, true));
            var keyrange2 = new KeyRange<int>(new Key<int>(1, true), new Key<int>(2, false));
            KeyRangeEqualityHelper(keyrange1, keyrange2, false);
        }

        /// <summary>
        /// KeyRange.Invert test 1
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("KeyRange.Invert test 1 (empty range)")]
        public void TestKeyRangeInvert1()
        {
            var keyrange = new KeyRange<int>(null, null);
            Assert.IsTrue(keyrange.Equals(keyrange.Invert()));
        }

        /// <summary>
        /// KeyRange.Invert test 2
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("KeyRange.Invert test 2 (range with min and max)")]
        public void TestKeyRangeInvert2()
        {
            var keyrange = new KeyRange<int>(new Key<int>(1, false), new Key<int>(2, true));
            Assert.IsTrue(keyrange.Invert().Equals(new KeyRange<int>(null, null)));
        }

        /// <summary>
        /// KeyRange.Invert test 3
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("KeyRange.Invert test 3 (range with min)")]
        public void TestKeyRangeInvert3()
        {
            var keyrange = new KeyRange<int>(new Key<int>(1, false), null);
            Assert.IsTrue(keyrange.Invert().Equals(new KeyRange<int>(null, new Key<int>(1, true))));
        }

        /// <summary>
        /// KeyRange.Invert test 4
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("KeyRange.Invert test 4 (range with max)")]
        public void TestKeyRangeInvert4()
        {
            var keyrange = new KeyRange<int>(null, new Key<int>(2, true));
            Assert.IsTrue(keyrange.Invert().Equals(new KeyRange<int>(new Key<int>(2, false), null)));
        }

        /// <summary>
        /// KeyRange intersect test 1
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("KeyRange.Intersect test 1 (empty ranges)")]
        public void TestKeyRangeIntersect1()
        {
            var nullRange = new KeyRange<int>(null, null);
            KeyRangeIntersectionHelper(nullRange, nullRange, nullRange);
        }

        /// <summary>
        /// KeyRange intersect test 2
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("KeyRange.Intersect test 2 (range with min and empty range)")]
        public void TestKeyRangeIntersect2()
        {
            var range1 = new KeyRange<int>(null, null);
            var range2 = new KeyRange<int>(new Key<int>(1, false), null);
            KeyRangeIntersectionHelper(range1, range2, range2);
        }

        /// <summary>
        /// KeyRange intersect test 3
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("KeyRange.Intersect test 3 (range with max and empty range)")]
        public void TestKeyRangeIntersect3()
        {
            var range1 = new KeyRange<int>(null, null);
            var range2 = new KeyRange<int>(null, new Key<int>(7, true));
            KeyRangeIntersectionHelper(range1, range2, range2);
        }

        /// <summary>
        /// KeyRange intersect test 4
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("KeyRange.Intersect test 4 (range with min+max and empty range)")]
        public void TestKeyRangeIntersect4()
        {
            var range1 = new KeyRange<int>(null, null);
            var range2 = new KeyRange<int>(new Key<int>(3, false), new Key<int>(7, true));
            KeyRangeIntersectionHelper(range1, range2, range2);
        }

        /// <summary>
        /// KeyRange intersect test 5
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("KeyRange.Intersect test 5 (range with min and range with max)")]
        public void TestKeyRangeIntersect5()
        {
            var range1 = new KeyRange<int>(null, new Key<int>(7, true));
            var range2 = new KeyRange<int>(new Key<int>(3, false), null);
            var expected = new KeyRange<int>(new Key<int>(3, false), new Key<int>(7, true));
            KeyRangeIntersectionHelper(range1, range2, expected);
        }

        /// <summary>
        /// KeyRange intersect test 6
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("KeyRange.Intersect test 6 (equal ranges)")]
        public void TestKeyRangeIntersect6()
        {
            var range = new KeyRange<int>(new Key<int>(6, true), new Key<int>(7, true));
            KeyRangeIntersectionHelper(range, range, range);
        }

        /// <summary>
        /// KeyRange intersect test 7
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("KeyRange.Intersect test 7 (ranges with same values)")]
        public void TestKeyRangeIntersect7()
        {
            var range1 = new KeyRange<int>(new Key<int>(3, true), new Key<int>(7, false));
            var range2 = new KeyRange<int>(new Key<int>(3, false), new Key<int>(7, true));
            var expected = new KeyRange<int>(new Key<int>(3, false), new Key<int>(7, false));
            KeyRangeIntersectionHelper(range1, range2, expected);
        }

        /// <summary>
        /// KeyRange intersect test 8
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("KeyRange.Intersect test 8 (different ranges)")]
        public void TestKeyRangeIntersect8()
        {
            var range1 = new KeyRange<int>(new Key<int>(2, true), new Key<int>(7, true));
            var range2 = new KeyRange<int>(new Key<int>(3, true), new Key<int>(8, true));
            var expected = new KeyRange<int>(new Key<int>(3, true), new Key<int>(7, true));
            KeyRangeIntersectionHelper(range1, range2, expected);
        }

        /// <summary>
        /// Helper function to check KeyRange equality.
        /// </summary>
        /// <typeparam name="T">The type of the KeyRange</typeparam>
        /// <param name="range1">The first range.</param>
        /// <param name="range2">The second range.</param>
        /// <param name="expected">The result of the equality comparison.</param>
        private static void KeyRangeEqualityHelper<T>(KeyRange<T> range1, KeyRange<T> range2, bool expected) where T : IComparable<T>
        {
            Assert.AreEqual(expected, range1.Equals(range2), "Range1 != Range2");
            Assert.AreEqual(expected, range2.Equals(range1), "Range2 != Range1");
        }

        /// <summary>
        /// Helper function to check KeyRange intersection.
        /// </summary>
        /// <typeparam name="T">The type of the KeyRange</typeparam>
        /// <param name="range1">The first range.</param>
        /// <param name="range2">The second range.</param>
        /// <param name="expected">The result of the intersection.</param>
        private static void KeyRangeIntersectionHelper<T>(KeyRange<T> range1, KeyRange<T> range2, KeyRange<T> expected) where T : IComparable<T>
        {
            Assert.IsTrue(expected.Equals(range1 & range2));
            Assert.IsTrue(expected.Equals(range2 & range1));
        }
    }
}