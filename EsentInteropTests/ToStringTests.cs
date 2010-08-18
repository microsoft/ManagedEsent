//-----------------------------------------------------------------------
// <copyright file="ToStringTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Globalization;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Testing the ToString methods of the basic types.
    /// </summary>
    [TestClass]
    public class ToStringTests
    {
        /// <summary>
        /// Test JET_INSTANCE.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_INSTANCE.ToString()")]
        public void JetInstanceToString()
        {
            var instance = new JET_INSTANCE() { Value = (IntPtr)0x123ABC };
            Assert.AreEqual("JET_INSTANCE(0x123abc)", instance.ToString());
        }

        /// <summary>
        /// Test JET_SESID.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_SESID.ToString()")]
        public void JetSesidToString()
        {
            var sesid = new JET_SESID() { Value = (IntPtr)0x123ABC };
            Assert.AreEqual("JET_SESID(0x123abc)", sesid.ToString());
        }

        /// <summary>
        /// Test JET_DBID.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_DBID.ToString()")]
        public void JetDbidToString()
        {
            var dbid = new JET_DBID() { Value = 23 };
            Assert.AreEqual("JET_DBID(23)", dbid.ToString());
        }

        /// <summary>
        /// Test JET_TABLEID.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_TABLEID.ToString()")]
        public void JetTableidToString()
        {
            var tableid = new JET_TABLEID() { Value = (IntPtr)0x123ABC };
            Assert.AreEqual("JET_TABLEID(0x123abc)", tableid.ToString());
        }

        /// <summary>
        /// Test JET_COLUMNID.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_COLUMNID.ToString()")]
        public void JetColumnidToString()
        {
            var columnid = new JET_COLUMNID() { Value = 0x12EC };
            Assert.AreEqual("JET_COLUMNID(0x12ec)", columnid.ToString());
        }

        /// <summary>
        /// Test JET_OSSNAPID.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_OSSNAPID.ToString()")]
        public void JetOsSnapidToString()
        {
            var ossnapid = new JET_OSSNAPID { Value = (IntPtr)0x123ABC };
            Assert.AreEqual("JET_OSSNAPID(0x123abc)", ossnapid.ToString());
        }

        /// <summary>
        /// Test JET_HANDLE.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_HANDLE.ToString()")]
        public void JetHandleToString()
        {
            var handle = new JET_HANDLE { Value = (IntPtr)0x123ABC };
            Assert.AreEqual("JET_HANDLE(0x123abc)", handle.ToString());
        }

        /// <summary>
        /// Test JET_LS.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_LS.ToString()")]
        public void JetLsToString()
        {
            var handle = new JET_LS { Value = (IntPtr)0x123ABC };
            Assert.AreEqual("JET_LS(0x123abc)", handle.ToString());
        }

        /// <summary>
        /// Test JET_INDEXID.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_INDEXID.ToString()")]
        public void JetIndexIdToString()
        {
            var indexid = new JET_INDEXID { IndexId1 = (IntPtr)0x1, IndexId2 = 0x2, IndexId3 = 0x3 };
            Assert.AreEqual("JET_INDEXID(0x1:0x2:0x3)", indexid.ToString());
        }

        /// <summary>
        /// Test JET_LOGTIME.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_LOGTIME.ToString()")]
        public void JetLogtimeToString()
        {
            var logtime = new JET_LOGTIME(new DateTime(2010, 5, 31, 4, 44, 17, DateTimeKind.Utc));
            Assert.AreEqual("JET_LOGTIME(17:44:4:31:5:110:0x80:0x0)", logtime.ToString());
        }

        /// <summary>
        /// Test JET_BKLOGTIME.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_BKLOGTIME.ToString()")]
        public void JetBklogtimeToString()
        {
            var bklogtime = new JET_BKLOGTIME(new DateTime(2010, 5, 31, 4, 44, 17, DateTimeKind.Utc), true);
            Assert.AreEqual("JET_BKLOGTIME(17:44:4:31:5:110:0x80:0x80)", bklogtime.ToString());
        }

        /// <summary>
        /// Test JET_SIGNATURE.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_SIGNATURE.ToString()")]
        public void JetSignatureToString()
        {
            var t = new DateTime(2010, 5, 31, 4, 44, 17, DateTimeKind.Utc);
            var signature = new JET_SIGNATURE(99, t, "COMPUTER");
            Assert.AreEqual("JET_SIGNATURE(99:05/31/2010 04:44:17:COMPUTER)", signature.ToString());
        }

        /// <summary>
        /// Test JET_LGPOS.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_LGPOS.ToString()")]
        public void JetLgposToString()
        {
            var lgpos = new JET_LGPOS { lGeneration = 1, isec = 0x1F, ib = 3 };
            Assert.AreEqual("JET_LGPOS(0x1,1F,3)", lgpos.ToString());
        }

        /// <summary>
        /// Test JET_BKINFO.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_BKINFO.ToString()")]
        public void JetBkinfoToString()
        {
            var bklogtime = new JET_BKLOGTIME(new DateTime(2010, 5, 31, 4, 44, 17, DateTimeKind.Utc), true);
            var lgpos = new JET_LGPOS { lGeneration = 1, isec = 2, ib = 3 };
            var bkinfo = new JET_BKINFO { bklogtimeMark = bklogtime, genHigh = 57, genLow = 36, lgposMark = lgpos };
            Assert.AreEqual("JET_BKINFO(36-57:JET_LGPOS(0x1,2,3):JET_BKLOGTIME(17:44:4:31:5:110:0x80:0x80))", bkinfo.ToString());
        }

        /// <summary>
        /// Test JET_SNPROG.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_SNPROG.ToString()")]
        public void JetSnprogToString()
        {
            var snprog = new JET_SNPROG { cunitDone = 5, cunitTotal = 10 };
            Assert.AreEqual("JET_SNPROG(5/10)", snprog.ToString());
        }

        /// <summary>
        /// Test JET_RECPOS.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_RECPOS.ToString()")]
        public void JetRecposToString()
        {
            var recpos = new JET_RECPOS { centriesLT = 5, centriesTotal = 10 };
            Assert.AreEqual("JET_RECPOS(5/10)", recpos.ToString());
        }

        /// <summary>
        /// Test JET_CONDITIONALCOLUMN.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_CONDITIONALCOLUMN.ToString()")]
        public void JetConditionalColumnToString()
        {
            var conditionalcolumn = new JET_CONDITIONALCOLUMN { szColumnName = "Foo", grbit = ConditionalColumnGrbit.ColumnMustBeNull };
            Assert.AreEqual("JET_CONDITIONALCOLUMN(Foo:ColumnMustBeNull)", conditionalcolumn.ToString());
        }

        /// <summary>
        /// Test JET_UNICODEINDEX.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_UNICODEINDEX.ToString()")]
        public void JetUnicodeIndexToString()
        {
            var unicodeindex = new JET_UNICODEINDEX { lcid = 1033, dwMapFlags = 0x12f };
            Assert.AreEqual("JET_UNICODEINDEX(1033:0x12F)", unicodeindex.ToString());
        }

        /// <summary>
        /// Test JET_INDEXCREATE.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_INDEXCREATE.ToString()")]
        public void JetIndexCreateToString()
        {
            var indexcreate = new JET_INDEXCREATE { cbKey = 5, szKey = "+C\0\0", szIndexName = "Index" };
            Assert.AreEqual("JET_INDEXCREATE(Index:+C\0\0)", indexcreate.ToString());
        }

        /// <summary>
        /// Test JET_INSTANCE_INFO.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_INSTANCE_INFO.ToString()")]
        public void JetInstanceInfoToString()
        {
            var instanceInfo = new JET_INSTANCE_INFO(JET_INSTANCE.Nil, "name", null);
            Assert.AreEqual("JET_INSTANCE_INFO(name)", instanceInfo.ToString());
        }

        /// <summary>
        /// Test JET_COLUMNDEF.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_COLUMNDEF.ToString()")]
        public void JetColumndefToString()
        {
            var columndef = new JET_COLUMNDEF { coltyp = JET_coltyp.Text, grbit = ColumndefGrbit.ColumnFixed };
            Assert.AreEqual("JET_COLUMNDEF(Text,ColumnFixed)", columndef.ToString());
        }

        /// <summary>
        /// Test JET_INDEXRANGE.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_INDEXRANGE.ToString()")]
        public void JetIndexrangeToString()
        {
            var indexrange = new JET_INDEXRANGE { grbit = IndexRangeGrbit.RecordInIndex };
            Assert.AreEqual("JET_INDEXRANGE(0x0,RecordInIndex)", indexrange.ToString());
        }

        /// <summary>
        /// Test JET_SETCOLUMN.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_SETCOLUMN.ToString()")]
        public void JetSetColumnToString()
        {
            var value = new JET_SETCOLUMN { columnid = new JET_COLUMNID { Value = 0x1234F } };
            Assert.AreEqual("JET_SETCOLUMN(0x1234f,<null>,ibLongValue=0,itagSequence=0)", value.ToString());
        }

        /// <summary>
        /// Test JET_RETINFO.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_RETINFO.ToString()")]
        public void JetRetinfoToString()
        {
            var value = new JET_RETINFO { ibLongValue = 1, itagSequence = 2 };
            Assert.AreEqual("JET_RETINFO(ibLongValue=1,itagSequence=2)", value.ToString());
        }

        /// <summary>
        /// Test JET_SETINFO.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_SETINFO.ToString()")]
        public void JetSetinfoToString()
        {
            var value = new JET_SETINFO { ibLongValue = 1, itagSequence = 2 };
            Assert.AreEqual("JET_SETINFO(ibLongValue=1,itagSequence=2)", value.ToString());
        }

        /// <summary>
        /// Test JET_DBINFOMISC.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test JET_DBINFOMISC.ToString()")]
        public void JetDbinfoMiscToString()
        {
            var value = new JET_DBINFOMISC();
            Assert.AreEqual("JET_DBINFOMISC(JET_SIGNATURE(0::))", value.ToString());
        }

        /// <summary>
        /// Test ColumnStream.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test ColumnStream.ToString()")]
        public void ColumnStreamToString()
        {
            var value = new ColumnStream(JET_SESID.Nil, JET_TABLEID.Nil, new JET_COLUMNID { Value = 0x1a });
            value.Itag = 2;
            Assert.AreEqual("ColumnStream(0x1a:2)", value.ToString());
        }

        /// <summary>
        /// Test Instance.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test Instance.ToString()")]
        public void InstanceToString()
        {
            var value = new Instance("name", "display");
            Assert.AreEqual("display (name)", value.ToString());
        }

        /// <summary>
        /// Test InstanceParameters.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test InstanceParameters.ToString()")]
        public void InstanceParametersToString()
        {
            var value = new InstanceParameters(JET_INSTANCE.Nil);
            Assert.AreEqual("InstanceParameters (0x0)", value.ToString());
        }

        /// <summary>
        /// Test IndexSegment.ToString() with an ascending segment.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test IndexSegment.ToString() with an ascending segment")]
        public void AscendingIndexSegmentToString()
        {
            var segment = new IndexSegment("column", JET_coltyp.IEEEDouble, true, false);
            Assert.AreEqual("+column(IEEEDouble)", segment.ToString());
        }

        /// <summary>
        /// Test IndexSegment.ToString() with an ascending segment.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test IndexSegment.ToString() with a descending segment")]
        public void DescendingIndexSegmentToString()
        {
            var segment = new IndexSegment("othercolumn", JET_coltyp.Bit, false, false);
            Assert.AreEqual("-othercolumn(Bit)", segment.ToString());
        }

        /// <summary>
        /// Test IndexInfo.ToString().
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test IndexInfo.ToString()")]
        public void IndexInfoToString()
        {
            var segments = new[]
            {
                new IndexSegment("foo", JET_coltyp.Short, true, false),
                new IndexSegment("bar", JET_coltyp.Bit, false, false),
            };
            var expected = new IndexInfo(
                "index",
                CultureInfo.CurrentCulture,
                CompareOptions.IgnoreKanaType,
                segments,
                CreateIndexGrbit.IndexUnique,
                1,
                2,
                3);
            Assert.AreEqual("index (+foo(Short)-bar(Bit))", expected.ToString());
        }
    }
}