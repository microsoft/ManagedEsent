//-----------------------------------------------------------------------
// <copyright file="EquatableTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Diagnostics;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for classes that implement IEquatable
    /// </summary>
    [TestClass]
    public class EquatableTests
    {
        /// <summary>
        /// Check that JET_INSTANCE structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_INSTANCE structures can be compared for equality")]
        public void VerifyJetInstanceEquality()
        {
            var x = JET_INSTANCE.Nil;
            var y = JET_INSTANCE.Nil;
            TestEqualObjects(x, y);
            Assert.IsTrue(x == y);
            Assert.IsFalse(x != y);
        }

        /// <summary>
        /// Check that JET_INSTANCE structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_INSTANCE structures can be compared for inequality")]
        public void VerifyJetInstanceInequality()
        {
            var x = JET_INSTANCE.Nil;
            var y = new JET_INSTANCE { Value = (IntPtr)0x7 };
            TestUnequalObjects(x, y);
            Assert.IsTrue(x != y);
            Assert.IsFalse(x == y);
        }

        /// <summary>
        /// Check that JET_SESID structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_SESID structures can be compared for equality")]
        public void VerifyJetSesidEquality()
        {
            var x = JET_SESID.Nil;
            var y = JET_SESID.Nil;
            TestEqualObjects(x, y);
            Assert.IsTrue(x == y);
            Assert.IsFalse(x != y);
        }

        /// <summary>
        /// Check that JET_SESID structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_SESID structures can be compared for inequality")]
        public void VerifyJetSesidInequality()
        {
            var x = JET_SESID.Nil;
            var y = new JET_SESID { Value = (IntPtr)0x7 };
            TestUnequalObjects(x, y);
            Assert.IsTrue(x != y);
            Assert.IsFalse(x == y);
        }

        /// <summary>
        /// Check that JET_TABLEID structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_TABLEID structures can be compared for equality")]
        public void VerifyJetTableidEquality()
        {
            var x = JET_TABLEID.Nil;
            var y = JET_TABLEID.Nil;
            TestEqualObjects(x, y);
            Assert.IsTrue(x == y);
            Assert.IsFalse(x != y);
        }

        /// <summary>
        /// Check that JET_TABLEID structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_TABLEID structures can be compared for inequality")]
        public void VerifyJetTableidInequality()
        {
            var x = JET_TABLEID.Nil;
            var y = new JET_TABLEID { Value = (IntPtr)0x7 };
            TestUnequalObjects(x, y);
            Assert.IsTrue(x != y);
            Assert.IsFalse(x == y);
        }

        /// <summary>
        /// Check that JET_DBID structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_DBID structures can be compared for equality")]
        public void VerifyJetDbidEquality()
        {
            var x = JET_DBID.Nil;
            var y = JET_DBID.Nil;
            TestEqualObjects(x, y);
            Assert.IsTrue(x == y);
            Assert.IsFalse(x != y);
        }

        /// <summary>
        /// Check that JET_DBID structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_DBID structures can be compared for inequality")]
        public void VerifyJetDbidInequality()
        {
            var x = JET_DBID.Nil;
            var y = new JET_DBID { Value = 0x2 };
            TestUnequalObjects(x, y);
            Assert.IsTrue(x != y);
            Assert.IsFalse(x == y);
        }

        /// <summary>
        /// Check that JET_COLUMNID structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_COLUMNID structures can be compared for equality")]
        public void VerifyJetColumnidEquality()
        {
            var x = JET_COLUMNID.Nil;
            var y = JET_COLUMNID.Nil;
            TestEqualObjects(x, y);
            Assert.IsTrue(x == y);
            Assert.IsFalse(x != y);
        }

        /// <summary>
        /// Check that JET_COLUMNID structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_COLUMNID structures can be compared for inequality")]
        public void VerifyJetColumnidInequality()
        {
            var x = JET_COLUMNID.Nil;
            var y = new JET_COLUMNID { Value = 0xF };
            TestUnequalObjects(x, y);
            Assert.IsTrue(x != y);
            Assert.IsFalse(x == y);
        }

        /// <summary>
        /// Check that JET_OSSNAPID structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_OSSNAPID structures can be compared for equality")]
        public void VerifyJetOsSnapidEquality()
        {
            var x = JET_OSSNAPID.Nil;
            var y = JET_OSSNAPID.Nil;
            TestEqualObjects(x, y);
            Assert.IsTrue(x == y);
            Assert.IsFalse(x != y);
        }

        /// <summary>
        /// Check that JET_OSSNAPID structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_OSSNAPID structures can be compared for inequality")]
        public void VerifyJetOsSnapidInequality()
        {
            var x = JET_OSSNAPID.Nil;
            var y = new JET_OSSNAPID { Value = (IntPtr)0x7 };
            TestUnequalObjects(x, y);
            Assert.IsTrue(x != y);
            Assert.IsFalse(x == y);
        }

        /// <summary>
        /// Check that JET_HANDLE structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_HANDLE structures can be compared for equality")]
        public void VerifyJetHandleEquality()
        {
            var x = JET_HANDLE.Nil;
            var y = JET_HANDLE.Nil;
            TestEqualObjects(x, y);
            Assert.IsTrue(x == y);
            Assert.IsFalse(x != y);
        }

        /// <summary>
        /// Check that JET_HANDLE structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_HANDLE structures can be compared for inequality")]
        public void VerifyJetHandleInequality()
        {
            var x = JET_HANDLE.Nil;
            var y = new JET_HANDLE { Value = (IntPtr)0x7 };
            TestUnequalObjects(x, y);
            Assert.IsTrue(x != y);
            Assert.IsFalse(x == y);
        }

        /// <summary>
        /// Check that JET_LS structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_LS structures can be compared for equality")]
        public void VerifyJetLsEquality()
        {
            var x = JET_LS.Nil;
            var y = JET_LS.Nil;
            TestEqualObjects(x, y);
            Assert.IsTrue(x == y);
            Assert.IsFalse(x != y);
        }

        /// <summary>
        /// Check that JET_LS structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_LS structures can be compared for inequality")]
        public void VerifyJetLsInequality()
        {
            var x = JET_LS.Nil;
            var y = new JET_LS { Value = (IntPtr)0x7 };
            TestUnequalObjects(x, y);
            Assert.IsTrue(x != y);
            Assert.IsFalse(x == y);
        }

        /// <summary>
        /// Check that JET_INDEXID structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_INDEXID structures can be compared for equality")]
        public void VerifyJetIndexIdEquality()
        {
            var x = new JET_INDEXID { IndexId1 = (IntPtr)0x1, IndexId2 = 0x2, IndexId3 = 0x3 };
            var y = new JET_INDEXID { IndexId1 = (IntPtr)0x1, IndexId2 = 0x2, IndexId3 = 0x3 };
            TestEqualObjects(x, y);
            Assert.IsTrue(x == y);
            Assert.IsFalse(x != y);
        }

        /// <summary>
        /// Check that JET_INDEXID structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_INDEXID structures can be compared for inequality")]
        public void VerifyJetIndexIdInequality()
        {
            var x = new JET_INDEXID { IndexId1 = (IntPtr)0x1, IndexId2 = 0x2, IndexId3 = 0x3 };
            var y = new JET_INDEXID { IndexId1 = (IntPtr)0x1, IndexId2 = 0x22, IndexId3 = 0x3 };
            var z = new JET_INDEXID { IndexId1 = (IntPtr)0x1, IndexId2 = 0x2, IndexId3 = 0x33 };

            TestUnequalObjects(x, y);
            TestUnequalObjects(x, z);
            TestUnequalObjects(y, z);

            Assert.IsTrue(x != y);
            Assert.IsFalse(x == y);
        }

        /// <summary>
        /// Check that null JET_LOGTIME structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that null JET_LOGTIME structures can be compared for equality")]
        public void VerifyNullJetLogtimeEquality()
        {
            var x = new JET_LOGTIME();
            var y = new JET_LOGTIME();
            TestEqualObjects(x, y);
            Assert.IsTrue(x == y);
            Assert.IsFalse(x != y);
        }

        /// <summary>
        /// Check that JET_LOGTIME structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_LOGTIME structures can be compared for equality")]
        public void VerifyJetLogtimeEquality()
        {
            var x = new JET_LOGTIME(DateTime.Now);
            var y = new JET_LOGTIME(DateTime.Now);
            TestEqualObjects(x, y);
            Assert.IsTrue(x == y);
            Assert.IsFalse(x != y);
        }

        /// <summary>
        /// Check that JET_LOGTIME structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_LOGTIME structures can be compared for inequality")]
        public void VerifyJetLogtimeInequality()
        {
            // None of these objects are equal, most differ in only one member from the
            // first object. We will compare them all against each other.
            var times = new[]
            {
                new JET_LOGTIME(new DateTime(2010, 5, 31, 4, 44, 17, DateTimeKind.Utc)),
                new JET_LOGTIME(new DateTime(2011, 5, 31, 4, 44, 17, DateTimeKind.Utc)),
                new JET_LOGTIME(new DateTime(2010, 7, 31, 4, 44, 17, DateTimeKind.Utc)),
                new JET_LOGTIME(new DateTime(2010, 5, 30, 4, 44, 17, DateTimeKind.Utc)),
                new JET_LOGTIME(new DateTime(2010, 5, 31, 5, 44, 17, DateTimeKind.Utc)),
                new JET_LOGTIME(new DateTime(2010, 5, 31, 4, 45, 17, DateTimeKind.Utc)),
                new JET_LOGTIME(new DateTime(2010, 5, 31, 4, 44, 18, DateTimeKind.Utc)),
                new JET_LOGTIME(new DateTime(2010, 5, 31, 4, 44, 17, DateTimeKind.Local)),
                new JET_LOGTIME(),
            };

            for (int i = 0; i < times.Length - 1; ++i)
            {
                for (int j = i + 1; j < times.Length; ++j)
                {
                    Debug.Assert(i != j, "About to compare the same JET_LOGTIME");
                    TestUnequalObjects(times[i], times[j]);
                    Assert.IsTrue(times[i] != times[j]);
                    Assert.IsFalse(times[i] == times[j]);
                }
            }
        }

        /// <summary>
        /// Helper method to compare two equal objects.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="x">The first object.</param>
        /// <param name="y">The second object.</param>
        private static void TestEqualObjects<T>(T x, T y) where T : struct, IEquatable<T> 
        {
            Assert.IsTrue(x.Equals(y));
            Assert.IsTrue(y.Equals(x));
            Assert.AreEqual(x.GetHashCode(), y.GetHashCode());
            Assert.AreEqual(x.ToString(), y.ToString());

            object objA = x;
            object objB = y;
            Assert.IsTrue(objA.Equals(objB));
            Assert.IsTrue(objB.Equals(objA));
            Assert.IsFalse(objA.Equals(Any.String));
        }

        /// <summary>
        /// Helper method to compare two unequal objects.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="x">The first object.</param>
        /// <param name="y">The second object.</param>
        private static void TestUnequalObjects<T>(T x, T y) where T : struct, IEquatable<T>
        {
            Assert.IsFalse(x.Equals(y));
            Assert.IsFalse(y.Equals(x));
            Assert.AreNotEqual(x.GetHashCode(), y.GetHashCode());
            Assert.AreNotEqual(x.ToString(), y.ToString());

            object objA = x;
            object objB = y;
            Assert.IsFalse(objA.Equals(objB));
            Assert.IsFalse(objB.Equals(objA));
            Assert.IsFalse(objA.Equals(null));
            Assert.IsFalse(objA.Equals(new Exception()));
        }
    }
}
