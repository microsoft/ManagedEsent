//-----------------------------------------------------------------------
// <copyright file="UtilTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the Util class.
    /// </summary>
    [TestClass]
    public class UtilTests
    {
        /// <summary>
        /// Test DumpBytes with a null array.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test DumpBytes with a null array")]
        public void TestDumpBytesNull()
        {
            Assert.AreEqual("<null>", Util.DumpBytes(null, 0, 0));
        }

        /// <summary>
        /// Test DumpBytes with a zero-length array.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test DumpBytes with a zero-length array")]
        public void TestDumpBytesZeroLength()
        {
            Assert.AreEqual(String.Empty, Util.DumpBytes(new byte[0], 0, 0));
        }

        /// <summary>
        /// Test DumpBytes with a negative offset.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test DumpBytes with a negative offset")]
        public void TestDumpBytesNegativeOffset()
        {
            Assert.AreEqual("<invalid>", Util.DumpBytes(new byte[1], -1, 1));
        }

        /// <summary>
        /// Test DumpBytes with an offset past the start of the array.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test DumpBytes with an offset past the start of the array")]
        public void TestDumpBytesInvalidOffset()
        {
            Assert.AreEqual("<invalid>", Util.DumpBytes(new byte[1], 2, 1));
        }

        /// <summary>
        /// Test DumpBytes with a negative count.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test DumpBytes with a negative count")]
        public void TestDumpBytesNegativeCount()
        {
            Assert.AreEqual("<invalid>", Util.DumpBytes(new byte[2], 2, -1));
        }

        /// <summary>
        /// Test DumpBytes with a count past the end of the array.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test DumpBytes with a count past the end of the array")]
        public void TestDumpBytesInvalidCount()
        {
            Assert.AreEqual("<invalid>", Util.DumpBytes(new byte[1], 1, 2));
        }

        /// <summary>
        /// Test DumpBytes with a short array.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test DumpBytes with a short array")]
        public void TestDumpBytesShortArray()
        {
            Assert.AreEqual("DD-CC-BB-AA", Util.DumpBytes(BitConverter.GetBytes(0xAABBCCDD), 0, 4));
        }

        /// <summary>
        /// Test DumpBytes with an offset.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test DumpBytes with an offset")]
        public void TestDumpBytesOffset()
        {
            Assert.AreEqual("CC-BB", Util.DumpBytes(BitConverter.GetBytes(0xAABBCCDD), 1, 2));
        }

        /// <summary>
        /// Test DumpBytes with a truncated array.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test DumpBytes with a truncated array")]
        public void TestDumpBytesTruncatedArray()
        {
            var b = new byte[] { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9 };
            Assert.AreEqual("00-01-02-03-04-05-06-07... (10 bytes)", Util.DumpBytes(b, 0, b.Length));
        }
            
        /// <summary>
        /// Test ArrayEqual with equal arrays.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test ArrayEqual with equal arrays")]
        public void TestArrayEqualTrue()
        {
            byte[] a = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6 };
            byte[] b = new byte[] { 0xF, 0x2, 0x3, 0x4, 0x5, 0xF, 0x0 };
            Assert.IsTrue(Util.ArrayEqual(a, b, 1, 4));
        }

        /// <summary>
        /// Test ArrayEqual with unequal arrays.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test ArrayEqual with unequal arrays")]
        public void TestArrayEqualFalse()
        {
            byte[] a = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6 };
            byte[] b = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6 };

            for (int offset = 0; offset < a.Length - 1; ++offset)
            {
                for (int count = 1; count < a.Length - offset; ++count)
                {
                    for (int i = 0; i < count; ++i)
                    {
                        b[offset + i] ^= 0xFF;
                        Assert.IsFalse(
                            Util.ArrayEqual(a, b, offset, count),
                            "{0} is equal to {1} (offset = {2}, count = {3})",
                            BitConverter.ToString(a),
                            BitConverter.ToString(b),
                            offset,
                            count);
                        b[offset + i] ^= 0xFF;
                    }
                }
            }
        }

        /// <summary>
        /// Check that ArrayObjectContentEquals compares null correctly.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that ArrayObjectContentEquals compares null correctly.")]
        public void TestArrayObjectContentEqualsNullArgs()
        {
            var spacehints = new JET_SPACEHINTS[]
            {
                new JET_SPACEHINTS()
                {
                    ulInitialDensity = 33,
                    cbInitial = 4096,
                }
            };
            JET_SPACEHINTS[] nil = null;
            JET_SPACEHINTS[] empty = new JET_SPACEHINTS[] { };

            JET_SPACEHINTS[] nullArray1 = new JET_SPACEHINTS[]
            {
                null,
                null,
            };

            JET_SPACEHINTS[] nullArray2 = new JET_SPACEHINTS[]
            {
                null,
                null,
            };

            Assert.IsFalse(Util.ArrayObjectContentEquals(spacehints, null));
            Assert.IsFalse(Util.ArrayObjectContentEquals(null, spacehints));

            Assert.IsTrue(Util.ArrayObjectContentEquals(nil, null));
            Assert.IsTrue(Util.ArrayObjectContentEquals(null, nil));

            Assert.IsTrue(Util.ArrayObjectContentEquals(nullArray1, nullArray2));
            Assert.IsTrue(Util.ArrayObjectContentEquals(nullArray2, nullArray1));
        }

        /// <summary>
        /// Check that ArrayObjectContentEquals compares differing arrays correctly.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that ArrayObjectContentEquals compares differeing arrays correctly.")]
        public void TestArrayObjectContentEqualsDiffer()
        {
            var x = new JET_SPACEHINTS[]
            {
                new JET_SPACEHINTS()
                {
                    ulInitialDensity = 33,
                    cbInitial = 4096,
                }
            };

            var x2 = new JET_SPACEHINTS[]
            {
                x[0],
                x[0],
            };

            var y = new JET_SPACEHINTS[]
            {
                new JET_SPACEHINTS()
                {
                    ulInitialDensity = 33,
                    cbInitial = 4077,
                }
            };

            var z = new JET_SPACEHINTS[]
            {
                null,
            };

            Assert.IsFalse(Util.ArrayObjectContentEquals(x, y));
            Assert.IsFalse(Util.ArrayObjectContentEquals(y, x));

            Assert.IsFalse(Util.ArrayObjectContentEquals(y, z));
            Assert.IsFalse(Util.ArrayObjectContentEquals(z, y));

            Assert.IsFalse(Util.ArrayObjectContentEquals(x, z));
            Assert.IsFalse(Util.ArrayObjectContentEquals(z, x));

            Assert.IsFalse(Util.ArrayObjectContentEquals(x2, y));
            Assert.IsFalse(Util.ArrayObjectContentEquals(y, x2));

            Assert.IsFalse(Util.ArrayObjectContentEquals(x2, z));
            Assert.IsFalse(Util.ArrayObjectContentEquals(z, x2));

            Assert.IsFalse(Util.ArrayObjectContentEquals(x, x2));
            Assert.IsFalse(Util.ArrayObjectContentEquals(x2, x));
        }

        /// <summary>
        /// Check that ArrayObjectContentEquals compares identical arrays correctly.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check that ArrayObjectContentEquals compares identical arrays correctly.")]
        public void TestArrayObjectContentEqualsIdentical()
        {
            var x = new JET_SPACEHINTS[]
            {
                new JET_SPACEHINTS()
                {
                    ulInitialDensity = 33,
                    cbInitial = 4096,
                }
            };
            var y = new JET_SPACEHINTS[]
            {
                new JET_SPACEHINTS()
                {
                    ulInitialDensity = 33,
                    cbInitial = 4096,
                }
            };

            var z = new JET_SPACEHINTS[]
            {
                x[0],
            };

            Assert.IsTrue(Util.ArrayObjectContentEquals(x, y));
            Assert.IsTrue(Util.ArrayObjectContentEquals(y, x));

            Assert.IsTrue(Util.ArrayObjectContentEquals(y, z));
            Assert.IsTrue(Util.ArrayObjectContentEquals(z, y));

            Assert.IsTrue(Util.ArrayObjectContentEquals(x, z));
            Assert.IsTrue(Util.ArrayObjectContentEquals(z, x));
        }
    }
}