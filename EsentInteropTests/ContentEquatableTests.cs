//-----------------------------------------------------------------------
// <copyright file="ContentEquatableTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for classes that implement IContentEquals and IDeepCloneable
    /// </summary>
    [TestClass]
    public class ContentEquatableTests
    {
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
            TestContentEquals(x, y);
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

            VerifyAll(conditionalcolumns);
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
            TestContentEquals(x, y);
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

            VerifyAll(unicodeindexes);
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
            TestContentEquals(x, y);
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

            VerifyAll(positions);
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
            var x = new JET_INDEXCREATE
            {
                cbKey = 6,
                szKey = "-C1\0\0",
                szIndexName = "Index",
                cConditionalColumn = 1,
                rgconditionalcolumn = new[] { new JET_CONDITIONALCOLUMN { grbit = ConditionalColumnGrbit.ColumnMustBeNonNull, szColumnName = "a" } }
            };
            var y = new JET_INDEXCREATE
            {
                cbKey = 6,
                szKey = "-C1\0\0",
                szIndexName = "Index",
                cConditionalColumn = 1,
                rgconditionalcolumn = new[]
                {
                    new JET_CONDITIONALCOLUMN { grbit = ConditionalColumnGrbit.ColumnMustBeNonNull, szColumnName = "a" },
                    new JET_CONDITIONALCOLUMN { grbit = ConditionalColumnGrbit.ColumnMustBeNonNull, szColumnName = "b" },
                }
            };
            TestContentEquals(x, y);
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
                        null,
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
            VerifyAll(indexcreates);
        }

        /// <summary>
        /// Check that JET_INDEXRANGE structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_INDEXRANGE structures can be compared for equality")]
        public void VerifyJetIndexrangeEquality()
        {
            var x = new JET_INDEXRANGE { grbit = IndexRangeGrbit.RecordInIndex };
            var y = new JET_INDEXRANGE { grbit = IndexRangeGrbit.RecordInIndex };
            TestContentEquals(x, y);
        }

        /// <summary>
        /// Check that JET_INDEXRANGE structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_INDEXRANGE structures can be compared for inequality")]
        public void VerifyJetIndexrangeInequality()
        {
            // None of these objects are equal, most differ in only one member from the
            // first object. We will compare them all against each other.
            var ranges = new[]
            {
                new JET_INDEXRANGE { tableid = new JET_TABLEID { Value = new IntPtr(1) }, grbit = IndexRangeGrbit.RecordInIndex },
                new JET_INDEXRANGE { tableid = new JET_TABLEID { Value = new IntPtr(1) }, grbit = (IndexRangeGrbit)49 },
                new JET_INDEXRANGE { tableid = new JET_TABLEID { Value = new IntPtr(2) }, grbit = IndexRangeGrbit.RecordInIndex },
            };
            VerifyAll(ranges);
        }

        /// <summary>
        /// Check that JET_COLUMNDEF structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_COLUMNDEF structures can be compared for equality")]
        public void VerifyJetColumndefEquality()
        {
            var x = new JET_COLUMNDEF
            {
                cbMax = 1,
                coltyp = JET_coltyp.Bit,
                columnid = new JET_COLUMNID { Value = 1 },
                cp = JET_CP.ASCII,
                grbit = ColumndefGrbit.None
            };
            var y = new JET_COLUMNDEF
            {
                cbMax = 1,
                coltyp = JET_coltyp.Bit,
                columnid = new JET_COLUMNID { Value = 1 },
                cp = JET_CP.ASCII,
                grbit = ColumndefGrbit.None
            };
            TestContentEquals(x, y);
        }

        /// <summary>
        /// Check that JET_COLUMNDEF structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_COLUMNDEF structures can be compared for inequality")]
        public void VerifyJetColumndefInequality()
        {
            // None of these objects are equal, most differ in only one member from the
            // first object. We will compare them all against each other.
            var positions = new[]
            {
                new JET_COLUMNDEF
                {
                    cbMax = 1,
                    coltyp = JET_coltyp.Bit,
                    columnid = new JET_COLUMNID { Value = 2 },
                    cp = JET_CP.ASCII,
                    grbit = ColumndefGrbit.None
                },
                new JET_COLUMNDEF
                {
                    cbMax = 1,
                    coltyp = JET_coltyp.Bit,
                    columnid = new JET_COLUMNID { Value = 2 },
                    cp = JET_CP.ASCII,
                    grbit = ColumndefGrbit.ColumnFixed
                },
                new JET_COLUMNDEF
                {
                    cbMax = 1,
                    coltyp = JET_coltyp.Bit,
                    columnid = new JET_COLUMNID { Value = 2 },
                    cp = JET_CP.Unicode,
                    grbit = ColumndefGrbit.None
                },
                new JET_COLUMNDEF
                {
                    cbMax = 1,
                    coltyp = JET_coltyp.Bit,
                    columnid = new JET_COLUMNID { Value = 3 },
                    cp = JET_CP.ASCII,
                    grbit = ColumndefGrbit.None
                },
                new JET_COLUMNDEF
                {
                    cbMax = 1,
                    coltyp = JET_coltyp.UnsignedByte,
                    columnid = new JET_COLUMNID { Value = 2 },
                    cp = JET_CP.ASCII,
                    grbit = ColumndefGrbit.None
                },
                new JET_COLUMNDEF
                {
                    cbMax = 2,
                    coltyp = JET_coltyp.Bit,
                    columnid = new JET_COLUMNID { Value = 2 },
                    cp = JET_CP.ASCII,
                    grbit = ColumndefGrbit.None
                },
            };
            VerifyAll(positions);
        }

        /// <summary>
        /// Check that JET_SETCOLUMN structures can be
        /// compared for equality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_SETCOLUMN structures can be compared for equality")]
        public void VerifyJetSetcolumnEquality()
        {
            var x = new JET_SETCOLUMN { cbData = 4, pvData = BitConverter.GetBytes(99) };
            var y = new JET_SETCOLUMN { cbData = 4, pvData = BitConverter.GetBytes(99) };
            TestContentEquals(x, y);
        }

        /// <summary>
        /// Check that JET_SETCOLUMN structures can be
        /// compared for inequality.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that JET_SETCOLUMN structures can be compared for inequality")]
        public void VerifyJetSetcolumnInequality()
        {
            var setcolumns = new JET_SETCOLUMN[9];
            for (int i = 0; i < setcolumns.Length; ++i)
            {
                setcolumns[i] = new JET_SETCOLUMN
                {
                    cbData = 6,
                    columnid = new JET_COLUMNID { Value = 1 },
                    err = JET_wrn.Success,
                    grbit = SetColumnGrbit.None,
                    ibData = 0,
                    ibLongValue = 0,
                    itagSequence = 1,
                    pvData = BitConverter.GetBytes(0xBADF00DL)
                };
            }

            int j = 1;
            setcolumns[j++].cbData++;
            setcolumns[j++].columnid = JET_COLUMNID.Nil;
            setcolumns[j++].err = JET_wrn.ColumnTruncated;
            setcolumns[j++].grbit = SetColumnGrbit.UniqueMultiValues;
            setcolumns[j++].ibData++;
            setcolumns[j++].ibLongValue++;
            setcolumns[j++].itagSequence++;
            setcolumns[j++].pvData = BitConverter.GetBytes(1L);
            Debug.Assert(j == setcolumns.Length, "Didn't fill in all entries of setcolumns");
            VerifyAll(setcolumns);
        }

        /// <summary>
        /// Make sure all reference non-string types are copied during cloning.
        /// </summary>
        /// <typeparam name="T">The type being cloned.</typeparam>
        /// <param name="obj">The object being cloned.</param>
        private static void VerifyDeepCloneClones<T>(T obj) where T : class, IDeepCloneable<T>
        {
            T clone = obj.DeepClone();
            Assert.AreNotSame(obj, clone);
            foreach (FieldInfo field in typeof(T).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (field.FieldType != typeof(string))
                {
                    object value = field.GetValue(obj);
                    object clonedValue = field.GetValue(clone);
                    if (null != value)
                    {
                        Assert.AreNotSame(value, clonedValue, "Field {0} was not cloned", field);
                    }
                }
            }
        }

        /// <summary>
        /// Helper method to compare two objects with equal content.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="x">The first object.</param>
        /// <param name="y">The second object.</param>
        private static void TestContentEquals<T>(T x, T y) where T : class, IContentEquatable<T>, IDeepCloneable<T>
        {
            Assert.IsTrue(x.ContentEquals(x));
            Assert.IsTrue(y.ContentEquals(y));

            Assert.IsTrue(x.ContentEquals(y));
            Assert.IsTrue(y.ContentEquals(x));

            Assert.AreEqual(x.ToString(), y.ToString());

            Assert.IsTrue(x.ContentEquals(x.DeepClone()));
            Assert.IsTrue(x.ContentEquals(y.DeepClone()));
            Assert.IsTrue(y.ContentEquals(y.DeepClone()));
            Assert.IsTrue(y.ContentEquals(x.DeepClone()));
        }

        /// <summary>
        /// Helper method to compare two objects with unequal content.
        /// </summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="x">The first object.</param>
        /// <param name="y">The second object.</param>
        private static void TestNotContentEquals<T>(T x, T y) where T : class, IContentEquatable<T>, IDeepCloneable<T>
        {
            Assert.IsFalse(x.ContentEquals(y), "{0} is content equal to {1}", x, y);
            Assert.IsFalse(y.ContentEquals(x), "{0} is content equal to {1}", y, x);

            Assert.IsFalse(x.ContentEquals(null));
            Assert.IsFalse(y.ContentEquals(null));
        }

        /// <summary>
        /// Verify that all objects in the collection are not content equal to each other
        /// and can be cloned.
        /// </summary>
        /// <remarks>
        /// This method doesn't test operator == or operator != so it should be 
        /// used for reference classes, which don't normally provide those operators.
        /// </remarks>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="values">Collection of distinct objects.</param>
        private static void VerifyAll<T>(IList<T> values) where T : class, IContentEquatable<T>, IDeepCloneable<T>
        {
            foreach (T obj in values)
            {
                VerifyDeepCloneClones(obj);
            }

            for (int i = 0; i < values.Count - 1; ++i)
            {
                TestContentEquals(values[i], values[i]);
                for (int j = i + 1; j < values.Count; ++j)
                {
                    Debug.Assert(i != j, "About to compare the same values");
                    TestNotContentEquals(values[i], values[j]);
                }
            }
        }
    }
}
