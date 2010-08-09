//-----------------------------------------------------------------------
// <copyright file="SerializationTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Vista;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for serialization/deserialization of objects.
    /// </summary>
    [TestClass]
    public class SerializationTests
    {
        /// <summary>
        /// Verify that a JET_LOGTIME can be serialized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a JET_LOGTIME can be serialized")]
        public void VerifyLogtimeCanBeSerialized()
        {
            var expected = new JET_LOGTIME(DateTime.Now);
            SerializeAndCompare(expected);
        }

        /// <summary>
        /// Verify that a JET_BKLOGTIME can be serialized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a JET_BKLOGTIME can be serialized")]
        public void VerifyBklogtimeCanBeSerialized()
        {
            var expected = new JET_BKLOGTIME(DateTime.Now, false);
            SerializeAndCompare(expected);
        }

        /// <summary>
        /// Verify that a JET_LGPOS can be serialized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a JET_LGPOS can be serialized")]
        public void VerifyLgposCanBeSerialized()
        {
            var expected = new JET_LGPOS { lGeneration = 13 };
            SerializeAndCompare(expected);
        }

        /// <summary>
        /// Verify that a JET_SIGNATURE can be serialized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a JET_SIGNATURE can be serialized")]
        public void VerifySignatureCanBeSerialized()
        {
            var expected = new JET_SIGNATURE(1, DateTime.Now, "BROWNVM");
            SerializeAndCompare(expected);
        }

        /// <summary>
        /// Verify that a JET_BKINFO can be serialized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a JET_BKINFO can be serialized")]
        public void VerifyBkinfoCanBeSerialized()
        {
            var expected = new JET_BKINFO
            {
                bklogtimeMark = new JET_BKLOGTIME(DateTime.UtcNow, false),
                genHigh = 1,
                genLow = 2,
                lgposMark = new JET_LGPOS { ib = 7, isec = 8, lGeneration = 9 },
            };
            SerializeAndCompare(expected);
        }

        /// <summary>
        /// Verify that an IndexSegment can be serialized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that an IndexSegment can be serialized")]
        public void VerifyIndexSegmentCanBeSerialized()
        {
            var expected = new IndexSegment("column", JET_coltyp.Text, false, true);
            SerializeAndCompare(expected);
        }

        /// <summary>
        /// Verify that an IndexInfo can be serialized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that an IndexInfo can be serialized")]
        public void VerifyIndexInfoCanBeSerialized()
        {
            var segments = new[] { new IndexSegment("column", JET_coltyp.Currency, true, false) };
            var expected = new IndexInfo(
                "index",
                CultureInfo.CurrentCulture,
                CompareOptions.IgnoreKanaType,
                segments,
                CreateIndexGrbit.IndexUnique,
                1,
                2,
                3);
            var actual = SerializeDeserialize(expected);
            Assert.AreNotSame(expected, actual);
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.CultureInfo, actual.CultureInfo);
            Assert.AreEqual(expected.IndexSegments[0].ColumnName, actual.IndexSegments[0].ColumnName);
        }

        /// <summary>
        /// Verify that a JET_COLUMNDEF can be serialized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a JET_COLUMNDEF can be serialized")]
        public void VerifyColumndefCanBeSerialized()
        {
            var expected = new JET_COLUMNDEF { coltyp = JET_coltyp.IEEESingle };
            SerializeAndCompareContent(expected);
        }

        /// <summary>
        /// Verify that serializing a JET_COLUMNDEF clears the columnid.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify serializing a JET_COLUMNDEF clears the columnid")]
        public void VerifyColumndefSerializationClearsColumnid()
        {
            var expected = new JET_COLUMNDEF { columnid = new JET_COLUMNID { Value = 0x9 } };
            var actual = SerializeDeserialize(expected);
            Assert.AreEqual(new JET_COLUMNID { Value = 0 }, actual.columnid);
        }

        /// <summary>
        /// Verify that a JET_CONDITIONALCOLUMN can be serialized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a JET_CONDITIONALCOLUMN can be serialized")]
        public void VerifyConditionalColumnCanBeSerialized()
        {
            var expected = new JET_CONDITIONALCOLUMN { szColumnName = "column" };
            SerializeAndCompareContent(expected);
        }

        /// <summary>
        /// Verify that a JET_INDEXCREATE can be serialized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a JET_INDEXCREATE can be serialized")]
        public void VerifyIndexCreateCanBeSerialized()
        {
            var expected = new JET_INDEXCREATE { szIndexName = "index", cbKey = 6, szKey = "+key\0\0" };
            SerializeAndCompareContent(expected);
        }

        /// <summary>
        /// Verify that a JET_RECPOS can be serialized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a JET_RECPOS can be serialized")]
        public void VerifyRecposCanBeSerialized()
        {
            var expected = new JET_RECPOS { centriesLT = 10, centriesTotal = 11 };
            SerializeAndCompareContent(expected);
        }

        /// <summary>
        /// Verify that a JET_RECSIZE can be serialized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a JET_RECSIZE can be serialized")]
        public void VerifyRecsizeCanBeSerialized()
        {
            var expected = new JET_RECSIZE { cbData = 101 };
            SerializeAndCompare(expected);
        }

        /// <summary>
        /// Verify that a JET_SNPROG can be serialized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a JET_SNPROG can be serialized")]
        public void VerifySnprogCanBeSerialized()
        {
            var expected = new JET_SNPROG { cunitDone = 10, cunitTotal = 11 };
            SerializeAndCompare(expected);
        }

        /// <summary>
        /// Verify that a JET_UNICODEINDEX can be serialized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a JET_UNICODEINDEX can be serialized")]
        public void VerifyUnicodeIndexCanBeSerialized()
        {
            var expected = new JET_UNICODEINDEX { lcid = 1234 };
            SerializeAndCompareContent(expected);
        }

        /// <summary>
        /// Verify that a JET_THREADSTATS can be serialized.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify that a JET_THREADSTATS can be serialized")]
        public void VerifyThreadstatsCanBeSerialized()
        {
            var expected = new JET_THREADSTATS { cbLogRecord = 946 };
            SerializeAndCompare(expected);
        }

        /// <summary>
        /// Serialize an object to an in-memory stream then deserialize it
        /// and compare to the original.
        /// </summary>
        /// <typeparam name="T">The type to serialize.</typeparam>
        /// <param name="expected">The object to serialize.</param>
        private static void SerializeAndCompare<T>(T expected) where T : IEquatable<T>
        {
            T actual = SerializeDeserialize(expected);
            Assert.AreNotSame(expected, actual);
            Assert.AreEqual(expected, actual);            
        }

        /// <summary>
        /// Serialize an object to an in-memory stream then deserialize it
        /// and compare to the original.
        /// </summary>
        /// <typeparam name="T">The type to serialize.</typeparam>
        /// <param name="expected">The object to serialize.</param>
        private static void SerializeAndCompareContent<T>(T expected) where T : IContentEquatable<T>
        {
            T actual = SerializeDeserialize(expected);
            Assert.AreNotSame(expected, actual);
            Assert.IsTrue(expected.ContentEquals(actual));
        }

        /// <summary>
        /// Serialize an object to an in-memory stream then deserialize it.
        /// </summary>
        /// <typeparam name="T">The type to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A deserialized copy of the object.</returns>
        private static T SerializeDeserialize<T>(T obj)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);

                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}