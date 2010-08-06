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
    using Microsoft.Isam.Esent.Interop.Vista;
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
            DateTime t = DateTime.Now;
            var x = new JET_LOGTIME(t);
            var y = new JET_LOGTIME(t);
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

            // It would be nice if this was a generic helper method, but that won't
            // work for operator== and operator!=.
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
        /// Check that JET_BKLOGTIME structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_BKLOGTIME structures can be compared for equality")]
        public void VerifyJetBklogtimeEquality()
        {
            DateTime t = DateTime.Now;
            var x = new JET_BKLOGTIME(t, false);
            var y = new JET_BKLOGTIME(t, false);
            TestEqualObjects(x, y);
            Assert.IsTrue(x == y);
            Assert.IsFalse(x != y);
        }

        /// <summary>
        /// Check that JET_BKLOGTIME structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_BKLOGTIME structures can be compared for inequality")]
        public void VerifyJetBklogtimeInequality()
        {
            // None of these objects are equal, most differ in only one member from the
            // first object. We will compare them all against each other.
            var times = new[]
            {
                new JET_BKLOGTIME(new DateTime(2010, 5, 31, 4, 44, 17, DateTimeKind.Utc), true),
                new JET_BKLOGTIME(new DateTime(2010, 5, 31, 4, 44, 17, DateTimeKind.Utc), false),
                new JET_BKLOGTIME(new DateTime(2011, 5, 31, 4, 44, 17, DateTimeKind.Utc), true),
                new JET_BKLOGTIME(new DateTime(2010, 7, 31, 4, 44, 17, DateTimeKind.Utc), true),
                new JET_BKLOGTIME(new DateTime(2010, 5, 30, 4, 44, 17, DateTimeKind.Utc), true),
                new JET_BKLOGTIME(new DateTime(2010, 5, 31, 5, 44, 17, DateTimeKind.Utc), true),
                new JET_BKLOGTIME(new DateTime(2010, 5, 31, 4, 45, 17, DateTimeKind.Utc), true),
                new JET_BKLOGTIME(new DateTime(2010, 5, 31, 4, 44, 18, DateTimeKind.Utc), true),
                new JET_BKLOGTIME(new DateTime(2010, 5, 31, 4, 44, 17, DateTimeKind.Local), true),
                new JET_BKLOGTIME(),
            };

            // It would be nice if this was a generic helper method, but that won't
            // work for operator== and operator!=.
            for (int i = 0; i < times.Length - 1; ++i)
            {
                for (int j = i + 1; j < times.Length; ++j)
                {
                    Debug.Assert(i != j, "About to compare the same JET_BKLOGTIME");
                    TestUnequalObjects(times[i], times[j]);
                    Assert.IsTrue(times[i] != times[j]);
                    Assert.IsFalse(times[i] == times[j]);
                }
            }
        }

        /// <summary>
        /// Check that null JET_SIGNATURE structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that null JET_SIGNATURE structures can be compared for equality")]
        public void VerifyNullJetSignatureEquality()
        {
            var x = new JET_SIGNATURE();
            var y = new JET_SIGNATURE();
            TestEqualObjects(x, y);
            Assert.IsTrue(x == y);
            Assert.IsFalse(x != y);
        }

        /// <summary>
        /// Check that JET_SIGNATURE structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_SIGNATURE structures can be compared for equality")]
        public void VerifyJetSignatureEquality()
        {
            DateTime t = DateTime.Now;
            var x = new JET_SIGNATURE(1, t, "COMPUTER");
            var y = new JET_SIGNATURE(1, t, "COMPUTER");
            TestEqualObjects(x, y);
            Assert.IsTrue(x == y);
            Assert.IsFalse(x != y);
        }

        /// <summary>
        /// Check that JET_SIGNATURE structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_SIGNATURE structures can be compared for inequality")]
        public void VerifyJetSignatureInequality()
        {
            // None of these objects are equal, most differ in only one member from the
            // first object. We will compare them all against each other.
            DateTime t = DateTime.UtcNow;
            var times = new[]
            {
                new JET_SIGNATURE(1, t, "COMPUTER"),
                new JET_SIGNATURE(2, t, "COMPUTER"),
                new JET_SIGNATURE(1, DateTime.Now, "COMPUTER"),
                new JET_SIGNATURE(1, null, "COMPUTER"),
                new JET_SIGNATURE(1, t, "COMPUTER2"),
                new JET_SIGNATURE(1, t, null),
                new JET_SIGNATURE(),
            };

            // It would be nice if this was a generic helper method, but that won't
            // work for operator== and operator!=.
            for (int i = 0; i < times.Length - 1; ++i)
            {
                for (int j = i + 1; j < times.Length; ++j)
                {
                    Debug.Assert(i != j, "About to compare the same JET_SIGNATURE");
                    TestUnequalObjects(times[i], times[j]);
                    Assert.IsTrue(times[i] != times[j]);
                    Assert.IsFalse(times[i] == times[j]);
                }
            }
        }

        /// <summary>
        /// Check that JET_LGPOS structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_LGPOS structures can be compared for equality")]
        public void VerifyJetLgposEquality()
        {
            var x = new JET_LGPOS { lGeneration = 1, isec = 2, ib = 3 };
            var y = new JET_LGPOS { lGeneration = 1, isec = 2, ib = 3 };
            TestEqualObjects(x, y);
            Assert.IsTrue(x == y);
            Assert.IsFalse(x != y);
        }

        /// <summary>
        /// Check that JET_LGPOS structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_LGPOS structures can be compared for inequality")]
        public void VerifyJetLgposInequality()
        {
            // None of these objects are equal, most differ in only one member from the
            // first object. We will compare them all against each other.
            var positions = new[]
            {
                new JET_LGPOS { lGeneration = 1, isec = 2, ib = 3 },
                new JET_LGPOS { lGeneration = 1, isec = 2, ib = 999 },
                new JET_LGPOS { lGeneration = 1, isec = 999, ib = 3 },
                new JET_LGPOS { lGeneration = 999, isec = 2, ib = 3 },
            };

            // It would be nice if this was a generic helper method, but that won't
            // work for operator== and operator!=.
            for (int i = 0; i < positions.Length - 1; ++i)
            {
                for (int j = i + 1; j < positions.Length; ++j)
                {
                    Debug.Assert(i != j, "About to compare the same JET_LGPOS");
                    TestUnequalObjects(positions[i], positions[j]);
                    Assert.IsTrue(positions[i] != positions[j]);
                    Assert.IsFalse(positions[i] == positions[j]);
                }
            }
        }

        /// <summary>
        /// Check that JET_BKINFO structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_BKINFO structures can be compared for equality")]
        public void VerifyJetBkinfoEquality()
        {
            var bklogtime = new JET_BKLOGTIME(DateTime.Now, false);
            var lgpos = new JET_LGPOS { lGeneration = 1, isec = 2, ib = 3 };

            var x = new JET_BKINFO { bklogtimeMark = bklogtime, genHigh = 11, genLow = 3, lgposMark = lgpos };
            var y = new JET_BKINFO { bklogtimeMark = bklogtime, genHigh = 11, genLow = 3, lgposMark = lgpos };
            TestEqualObjects(x, y);
            Assert.IsTrue(x == y);
            Assert.IsFalse(x != y);
        }

        /// <summary>
        /// Check that JET_BKINFO structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_BKINFO structures can be compared for inequality")]
        public void VerifyJetBkinfoInequality()
        {
            var bklogtime1 = new JET_BKLOGTIME(DateTime.Now, false);
            var bklogtime2 = new JET_BKLOGTIME(DateTime.Now, true);
            var lgpos1 = new JET_LGPOS { lGeneration = 7, isec = 8, ib = 5 };
            var lgpos2 = new JET_LGPOS { lGeneration = 7, isec = 8, ib = 9 };

            // None of these objects are equal, most differ in only one member from the
            // first object. We will compare them all against each other.
            var positions = new[]
            {
                new JET_BKINFO { bklogtimeMark = bklogtime1, genHigh = 11, genLow = 3, lgposMark = lgpos1 },
                new JET_BKINFO { bklogtimeMark = bklogtime1, genHigh = 11, genLow = 3, lgposMark = lgpos2 },
                new JET_BKINFO { bklogtimeMark = bklogtime1, genHigh = 11, genLow = 4, lgposMark = lgpos1 },
                new JET_BKINFO { bklogtimeMark = bklogtime1, genHigh = 12, genLow = 3, lgposMark = lgpos1 },
                new JET_BKINFO { bklogtimeMark = bklogtime2, genHigh = 11, genLow = 3, lgposMark = lgpos1 },
            };

            // It would be nice if this was a generic helper method, but that won't
            // work for operator== and operator!=.
            for (int i = 0; i < positions.Length - 1; ++i)
            {
                for (int j = i + 1; j < positions.Length; ++j)
                {
                    Debug.Assert(i != j, "About to compare the same JET_LGPOS");
                    TestUnequalObjects(positions[i], positions[j]);
                    Assert.IsTrue(positions[i] != positions[j]);
                    Assert.IsFalse(positions[i] == positions[j]);
                }
            }
        }

        /// <summary>
        /// Check that JET_RECSIZE structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_RECSIZE structures can be compared for equality")]
        public void VerifyJetRecsizeEquality()
        {
            var x = new JET_RECSIZE
            {
                cbData = 1,
                cbDataCompressed = 2,
                cbLongValueData = 3,
                cbLongValueDataCompressed = 4,
                cbLongValueOverhead = 5,
                cbOverhead = 6,
                cCompressedColumns = 7,
                cLongValues = 8,
                cMultiValues = 9,
                cNonTaggedColumns = 10,
                cTaggedColumns = 11
            };
            var y = new JET_RECSIZE
            {
                cbData = 1,
                cbDataCompressed = 2,
                cbLongValueData = 3,
                cbLongValueDataCompressed = 4,
                cbLongValueOverhead = 5,
                cbOverhead = 6,
                cCompressedColumns = 7,
                cLongValues = 8,
                cMultiValues = 9,
                cNonTaggedColumns = 10,
                cTaggedColumns = 11
            };
            TestEqualObjects(x, y);
            Assert.IsTrue(x == y);
            Assert.IsFalse(x != y);
        }

        /// <summary>
        /// Check that JET_RECSIZE structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_RECSIZE structures can be compared for inequality")]
        public void VerifyJetRecsizeInequality()
        {
            // None of these objects are equal, most differ in only one member from the
            // first object. We will compare them all against each other.
            var sizes = new[]
            {
                new JET_RECSIZE
                {
                    cbData = 1,
                    cbDataCompressed = 2,
                    cbLongValueData = 3,
                    cbLongValueDataCompressed = 4,
                    cbLongValueOverhead = 5,
                    cbOverhead = 6,
                    cCompressedColumns = 7,
                    cLongValues = 8,
                    cMultiValues = 9,
                    cNonTaggedColumns = 10,
                    cTaggedColumns = 11
                },
                new JET_RECSIZE
                {
                    cbData = 11,
                    cbDataCompressed = 2,
                    cbLongValueData = 3,
                    cbLongValueDataCompressed = 4,
                    cbLongValueOverhead = 5,
                    cbOverhead = 6,
                    cCompressedColumns = 7,
                    cLongValues = 8,
                    cMultiValues = 9,
                    cNonTaggedColumns = 10,
                    cTaggedColumns = 11
                },
                new JET_RECSIZE
                {
                    cbData = 1,
                    cbDataCompressed = 12,
                    cbLongValueData = 3,
                    cbLongValueDataCompressed = 4,
                    cbLongValueOverhead = 5,
                    cbOverhead = 6,
                    cCompressedColumns = 7,
                    cLongValues = 8,
                    cMultiValues = 9,
                    cNonTaggedColumns = 10,
                    cTaggedColumns = 11
                },
                new JET_RECSIZE
                {
                    cbData = 1,
                    cbDataCompressed = 2,
                    cbLongValueData = 13,
                    cbLongValueDataCompressed = 4,
                    cbLongValueOverhead = 5,
                    cbOverhead = 6,
                    cCompressedColumns = 7,
                    cLongValues = 8,
                    cMultiValues = 9,
                    cNonTaggedColumns = 10,
                    cTaggedColumns = 11
                },
                new JET_RECSIZE
                {
                    cbData = 1,
                    cbDataCompressed = 2,
                    cbLongValueData = 3,
                    cbLongValueDataCompressed = 14,
                    cbLongValueOverhead = 5,
                    cbOverhead = 6,
                    cCompressedColumns = 7,
                    cLongValues = 8,
                    cMultiValues = 9,
                    cNonTaggedColumns = 10,
                    cTaggedColumns = 11
                },
                new JET_RECSIZE
                {
                    cbData = 1,
                    cbDataCompressed = 2,
                    cbLongValueData = 3,
                    cbLongValueDataCompressed = 4,
                    cbLongValueOverhead = 15,
                    cbOverhead = 6,
                    cCompressedColumns = 7,
                    cLongValues = 8,
                    cMultiValues = 9,
                    cNonTaggedColumns = 10,
                    cTaggedColumns = 11
                },
                new JET_RECSIZE
                {
                    cbData = 1,
                    cbDataCompressed = 2,
                    cbLongValueData = 3,
                    cbLongValueDataCompressed = 4,
                    cbLongValueOverhead = 5,
                    cbOverhead = 16,
                    cCompressedColumns = 7,
                    cLongValues = 8,
                    cMultiValues = 9,
                    cNonTaggedColumns = 10,
                    cTaggedColumns = 11
                },
                new JET_RECSIZE
                {
                    cbData = 1,
                    cbDataCompressed = 2,
                    cbLongValueData = 3,
                    cbLongValueDataCompressed = 4,
                    cbLongValueOverhead = 5,
                    cbOverhead = 6,
                    cCompressedColumns = 17,
                    cLongValues = 8,
                    cMultiValues = 9,
                    cNonTaggedColumns = 10,
                    cTaggedColumns = 11
                },
                new JET_RECSIZE
                {
                    cbData = 1,
                    cbDataCompressed = 2,
                    cbLongValueData = 3,
                    cbLongValueDataCompressed = 4,
                    cbLongValueOverhead = 5,
                    cbOverhead = 6,
                    cCompressedColumns = 7,
                    cLongValues = 18,
                    cMultiValues = 9,
                    cNonTaggedColumns = 10,
                    cTaggedColumns = 11
                },
                new JET_RECSIZE
                {
                    cbData = 1,
                    cbDataCompressed = 2,
                    cbLongValueData = 3,
                    cbLongValueDataCompressed = 4,
                    cbLongValueOverhead = 5,
                    cbOverhead = 6,
                    cCompressedColumns = 7,
                    cLongValues = 8,
                    cMultiValues = 19,
                    cNonTaggedColumns = 10,
                    cTaggedColumns = 11
                },
                new JET_RECSIZE
                {
                    cbData = 1,
                    cbDataCompressed = 2,
                    cbLongValueData = 3,
                    cbLongValueDataCompressed = 4,
                    cbLongValueOverhead = 5,
                    cbOverhead = 6,
                    cCompressedColumns = 7,
                    cLongValues = 8,
                    cMultiValues = 9,
                    cNonTaggedColumns = 20,
                    cTaggedColumns = 11
                },
                new JET_RECSIZE
                {
                    cbData = 1,
                    cbDataCompressed = 2,
                    cbLongValueData = 3,
                    cbLongValueDataCompressed = 4,
                    cbLongValueOverhead = 5,
                    cbOverhead = 6,
                    cCompressedColumns = 7,
                    cLongValues = 8,
                    cMultiValues = 9,
                    cNonTaggedColumns = 10,
                    cTaggedColumns = 21
                },
            };

            // It would be nice if this was a generic helper method, but that won't
            // work for operator== and operator!=.
            for (int i = 0; i < sizes.Length - 1; ++i)
            {
                for (int j = i + 1; j < sizes.Length; ++j)
                {
                    Debug.Assert(i != j, "About to compare the same JET_RECSIZE");
                    TestUnequalObjects(sizes[i], sizes[j]);
                    Assert.IsTrue(sizes[i] != sizes[j]);
                    Assert.IsFalse(sizes[i] == sizes[j]);
                }
            }
        }

        /// <summary>
        /// Check that JET_THREADSTATS structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_THREADSTATS structures can be compared for equality")]
        public void VerifyJetThreadstatsEquality()
        {
            DateTime t = DateTime.Now;
            var x = new JET_THREADSTATS
            {
                cbLogRecord = 1,
                cLogRecord = 2,
                cPageDirtied = 3,
                cPagePreread = 4,
                cPageRead = 5,
                cPageRedirtied = 6,
                cPageReferenced = 7,
            };
            var y = new JET_THREADSTATS
            {
                cbLogRecord = 1,
                cLogRecord = 2,
                cPageDirtied = 3,
                cPagePreread = 4,
                cPageRead = 5,
                cPageRedirtied = 6,
                cPageReferenced = 7,
            };
            TestEqualObjects(x, y);
            Assert.IsTrue(x == y);
            Assert.IsFalse(x != y);
        }

        /// <summary>
        /// Check that JET_THREADSTATS structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_THREADSTATS structures can be compared for inequality")]
        public void VerifyJetThreadstatsInequality()
        {
            // None of these objects are equal, most differ in only one member from the
            // first object. We will compare them all against each other.
            var threadstats = new[]
            {
                new JET_THREADSTATS
                {
                    cbLogRecord = 1,
                    cLogRecord = 2,
                    cPageDirtied = 3,
                    cPagePreread = 4,
                    cPageRead = 5,
                    cPageRedirtied = 6,
                    cPageReferenced = 7,
                },
                new JET_THREADSTATS
                {
                    cbLogRecord = 11,
                    cLogRecord = 2,
                    cPageDirtied = 3,
                    cPagePreread = 4,
                    cPageRead = 5,
                    cPageRedirtied = 6,
                    cPageReferenced = 7,
                },
                new JET_THREADSTATS
                {
                    cbLogRecord = 1,
                    cLogRecord = 12,
                    cPageDirtied = 3,
                    cPagePreread = 4,
                    cPageRead = 5,
                    cPageRedirtied = 6,
                    cPageReferenced = 7,
                },
                new JET_THREADSTATS
                {
                    cbLogRecord = 1,
                    cLogRecord = 2,
                    cPageDirtied = 13,
                    cPagePreread = 4,
                    cPageRead = 5,
                    cPageRedirtied = 6,
                    cPageReferenced = 7,
                },
                new JET_THREADSTATS
                {
                    cbLogRecord = 1,
                    cLogRecord = 2,
                    cPageDirtied = 3,
                    cPagePreread = 14,
                    cPageRead = 5,
                    cPageRedirtied = 6,
                    cPageReferenced = 7,
                },
                new JET_THREADSTATS
                {
                    cbLogRecord = 1,
                    cLogRecord = 2,
                    cPageDirtied = 3,
                    cPagePreread = 4,
                    cPageRead = 15,
                    cPageRedirtied = 6,
                    cPageReferenced = 7,
                },
                new JET_THREADSTATS
                {
                    cbLogRecord = 1,
                    cLogRecord = 2,
                    cPageDirtied = 3,
                    cPagePreread = 4,
                    cPageRead = 5,
                    cPageRedirtied = 16,
                    cPageReferenced = 7,
                },
                new JET_THREADSTATS
                {
                    cbLogRecord = 1,
                    cLogRecord = 2,
                    cPageDirtied = 3,
                    cPagePreread = 4,
                    cPageRead = 5,
                    cPageRedirtied = 6,
                    cPageReferenced = 17,
                },
            };

            // It would be nice if this was a generic helper method, but that won't
            // work for operator== and operator!=.
            for (int i = 0; i < threadstats.Length - 1; ++i)
            {
                for (int j = i + 1; j < threadstats.Length; ++j)
                {
                    Debug.Assert(i != j, "About to compare the same JET_THREADSTATS");
                    TestUnequalObjects(threadstats[i], threadstats[j]);
                    Assert.IsTrue(threadstats[i] != threadstats[j]);
                    Assert.IsFalse(threadstats[i] == threadstats[j]);
                }
            }
        }

        /// <summary>
        /// Check that JET_CONDITIONALCOLUMN objects can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_CONDITIONALCOLUMN objects can be compared for equality")]
        public void VerifyJetConditionalColumnEquality()
        {
            var x = new JET_CONDITIONALCOLUMN { szColumnName = "Column", grbit = ConditionalColumnGrbit.ColumnMustBeNonNull };
            var y = new JET_CONDITIONALCOLUMN { szColumnName = "Column", grbit = ConditionalColumnGrbit.ColumnMustBeNonNull };
            TestEqualObjects(x, y);

            // This is a reference class. Operator == and != still do reference comparisons.
        }

        /// <summary>
        /// Check that JET_CONDITIONALCOLUMN objects can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_CONDITIONALCOLUMN objects can be compared for inequality")]
        public void VerifyJetConditionalColumnInequality()
        {
            // None of these objects are equal, most differ in only one member from the
            // first object. We will compare them all against each other.
            var conditionalcolumns = new[]
            {
                new JET_CONDITIONALCOLUMN { szColumnName = "Column", grbit = ConditionalColumnGrbit.ColumnMustBeNonNull },
                new JET_CONDITIONALCOLUMN { szColumnName = "Column", grbit = ConditionalColumnGrbit.ColumnMustBeNull },
                new JET_CONDITIONALCOLUMN { szColumnName = "Column2", grbit = ConditionalColumnGrbit.ColumnMustBeNonNull },
                new JET_CONDITIONALCOLUMN { szColumnName = null, grbit = ConditionalColumnGrbit.ColumnMustBeNonNull },
            };

            TestUnequal(conditionalcolumns);
        }

        /// <summary>
        /// Check that JET_UNICODEINDEX objects can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_UNICODEINDEX objects can be compared for equality")]
        public void VerifyJetUnicodeIndexEquality()
        {
            var x = new JET_UNICODEINDEX { lcid = 1033, dwMapFlags = 1 };
            var y = new JET_UNICODEINDEX { lcid = 1033, dwMapFlags = 1 };
            TestEqualObjects(x, y);

            // This is a reference class. Operator == and != still do reference comparisons.
        }

        /// <summary>
        /// Check that JET_UNICODEINDEX structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_UNICODEINDEX objects can be compared for inequality")]
        public void VerifyJetUnicodeIndexInequality()
        {
            // None of these objects are equal, most differ in only one member from the
            // first object. We will compare them all against each other.
            var unicodeindexes = new[]
            {
                new JET_UNICODEINDEX { lcid = 1033, dwMapFlags = 1 },
                new JET_UNICODEINDEX { lcid = 1033, dwMapFlags = 2 },
                new JET_UNICODEINDEX { lcid = 1034, dwMapFlags = 1 },
            };

            TestUnequal(unicodeindexes);
        }

        /// <summary>
        /// Check that JET_SNPROG objects can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_SNPROG objects can be compared for equality")]
        public void VerifyJetSnprogEquality()
        {
            var x = new JET_SNPROG { cunitDone = 1, cunitTotal = 2 };
            var y = new JET_SNPROG { cunitDone = 1, cunitTotal = 2 };
            TestEqualObjects(x, y);

            // This is a reference class. Operator == and != still do reference comparisons.
        }

        /// <summary>
        /// Check that JET_SNPROG structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_SNPROG objects can be compared for inequality")]
        public void VerifyJetSnprogInequality()
        {
            // None of these objects are equal, most differ in only one member from the
            // first object. We will compare them all against each other.
            var snprogs = new[]
            {
                new JET_SNPROG { cunitDone = 1, cunitTotal = 2 },
                new JET_SNPROG { cunitDone = 1, cunitTotal = 3 },
                new JET_SNPROG { cunitDone = 2, cunitTotal = 2 },
            };

            TestUnequal(snprogs);
        }

        /// <summary>
        /// Check that JET_RECPOS objects can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_RECPOS objects can be compared for equality")]
        public void VerifyJetRecposEquality()
        {
            var x = new JET_RECPOS { centriesLT = 1, centriesTotal = 2 };
            var y = new JET_RECPOS { centriesLT = 1, centriesTotal = 2 };
            TestEqualObjects(x, y);

            // This is a reference class. Operator == and != still do reference comparisons.
        }

        /// <summary>
        /// Check that JET_RECPOS structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_RECPOS objects can be compared for inequality")]
        public void VerifyJetRecposInequality()
        {
            // None of these objects are equal, most differ in only one member from the
            // first object. We will compare them all against each other.
            var positions = new[]
            {
                new JET_RECPOS { centriesLT = 1, centriesTotal = 2 },
                new JET_RECPOS { centriesLT = 1, centriesTotal = 3 },
                new JET_RECPOS { centriesLT = 2, centriesTotal = 2 },
            };

            TestUnequal(positions);
        }

        /// <summary>
        /// Check that JET_INDEXCREATE objects can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_INDEXCREATE objects can be compared for equality")]
        public void VerifyJetIndexcreateEquality()
        {
            var x = new JET_INDEXCREATE { cbKey = 6, szKey = "-C1\0\0", szIndexName = "Index" };
            var y = new JET_INDEXCREATE { cbKey = 6, szKey = "-C1\0\0", szIndexName = "Index" };
            TestEqualObjects(x, y);

            // This is a reference class. Operator == and != still do reference comparisons.
        }

        /// <summary>
        /// Check that JET_INDEXCREATE structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_INDEXCREATE objects can be compared for inequality")]
        public void VerifyJetIndexCreateInequality()
        {
            // create an array of indexcreate objects
            var indexcreates = new JET_INDEXCREATE[13];

            // First make them all the same
            // (different objects with the same values)
            for (int i = 0; i < indexcreates.Length; ++i)
            {
                indexcreates[i] = new JET_INDEXCREATE
                {
                    cbKey = 6,
                    cbKeyMost = 300,
                    cbVarSegMac = 100,
                    cConditionalColumn = 2,
                    err = JET_err.Success,
                    grbit = CreateIndexGrbit.None,
                    pidxUnicode = new JET_UNICODEINDEX { dwMapFlags = 0x1, lcid = 100 },
                    rgconditionalcolumn = new[]
                    {
                        new JET_CONDITIONALCOLUMN { grbit = ConditionalColumnGrbit.ColumnMustBeNonNull, szColumnName = "a" },
                        new JET_CONDITIONALCOLUMN { grbit = ConditionalColumnGrbit.ColumnMustBeNull, szColumnName = "b" },
                    },
                    szIndexName = "index",
                    szKey = "+foo\0\0",
                };
            }

            // Now make them all different
            int j = 1;
            indexcreates[j++].cbKey--;
            indexcreates[j++].cbKeyMost--;
            indexcreates[j++].cbVarSegMac--;
            indexcreates[j++].cConditionalColumn--;
            indexcreates[j++].cConditionalColumn = 0;
            indexcreates[j++].err = JET_err.VersionStoreOutOfMemory;
            indexcreates[j++].grbit = CreateIndexGrbit.IndexUnique;
            indexcreates[j++].pidxUnicode = new JET_UNICODEINDEX { dwMapFlags = 0x2, lcid = 100 };
            indexcreates[j++].pidxUnicode = null;
            indexcreates[j++].rgconditionalcolumn[0].szColumnName = "c";
            indexcreates[j++].szIndexName = "index2";
            indexcreates[j++].szKey = "+bar\0\0";
            Debug.Assert(j == indexcreates.Length, "Too many indexcreates in array");

            // Finally compare them
            TestUnequal(indexcreates);
        }

        /// <summary>
        /// Check that JET_INSTANCE_INFO objects can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_INSTANCE_INFO objects can be compared for equality")]
        public void VerifyJetInstanceInfoEquality()
        {
            var x = new JET_INSTANCE_INFO(JET_INSTANCE.Nil, "instance", new[] { "foo.edb" });
            var y = new JET_INSTANCE_INFO(JET_INSTANCE.Nil, "instance", new[] { "foo.edb" });
            TestEqualObjects(x, y);

            // This is a reference class. Operator == and != still do reference comparisons.
        }

        /// <summary>
        /// Check that JET_INSTANCE_INFO structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_INSTANCE_INFO objects can be compared for inequality")]
        public void VerifyJetInstanceInfoInequality()
        {
            // None of these objects are equal, most differ in only one member from the
            // first object. We will compare them all against each other.
            var positions = new[]
            {
                new JET_INSTANCE_INFO(JET_INSTANCE.Nil, "instance", new[] { "foo.edb" }),
                new JET_INSTANCE_INFO(JET_INSTANCE.Nil, "instance", new[] { "foo.edb", "bar.edb" }),
                new JET_INSTANCE_INFO(JET_INSTANCE.Nil, "instance", new[] { "bar.edb" }),
                new JET_INSTANCE_INFO(JET_INSTANCE.Nil, "instance", null),
                new JET_INSTANCE_INFO(JET_INSTANCE.Nil, "instance2", new[] { "foo.edb" }),
                new JET_INSTANCE_INFO(JET_INSTANCE.Nil, null, new[] { "foo.edb" }),
                new JET_INSTANCE_INFO(new JET_INSTANCE { Value = new IntPtr(1) }, "instance", new[] { "foo.edb" }),
            };

            TestUnequal(positions);
        }

        /// <summary>
        /// Helper method to compare two equal objects.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="x">The first object.</param>
        /// <param name="y">The second object.</param>
        private static void TestEqualObjects<T>(T x, T y) where T : IEquatable<T> 
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
        private static void TestUnequalObjects<T>(T x, T y) where T : IEquatable<T>
        {
            Assert.IsFalse(x.Equals(y));
            Assert.IsFalse(y.Equals(x));
            Assert.AreNotEqual(x.GetHashCode(), y.GetHashCode(), "{0} and {1} have the same hash code", x, y);

            object objA = x;
            object objB = y;
            Assert.IsFalse(objA.Equals(objB));
            Assert.IsFalse(objB.Equals(objA));
            Assert.IsFalse(objA.Equals(null));
            Assert.IsFalse(objA.Equals(new Exception()));
        }

        /// <summary>
        /// Verify that all objects in the collection are not equal to each other.
        /// </summary>
        /// <remarks>
        /// This method doesn't test operator == or operator != so it should be 
        /// used for reference classes, which don't normally provide those operators.
        /// </remarks>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="values">Collection of distinct objects.</param>
        private static void TestUnequal<T>(T[] values) where T : class, IEquatable<T>
        {
            for (int i = 0; i < values.Length - 1; ++i)
            {
                TestEqualObjects(values[i], values[i]);
                for (int j = i + 1; j < values.Length; ++j)
                {
                    Debug.Assert(i != j, "About to compare the same values");
                    TestUnequalObjects(values[i], values[j]);
                    Assert.IsFalse(values[i].Equals(null));
                }
            }           
        }
    }
}
