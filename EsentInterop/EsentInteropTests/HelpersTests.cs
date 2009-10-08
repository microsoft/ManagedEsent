//-----------------------------------------------------------------------
// <copyright file="HelpersTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Vista;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the various Set/RetrieveColumn* methods and
    /// the helper methods that retrieve meta-data.
    /// </summary>
    [TestClass]
    public class HelpersTests
    {
        /// <summary>
        /// The directory being used for the database and its files.
        /// </summary>
        private string directory;

        /// <summary>
        /// The path to the database being used by the test.
        /// </summary>
        private string database;

        /// <summary>
        /// The name of the table.
        /// </summary>
        private string table;

        /// <summary>
        /// The instance used by the test.
        /// </summary>
        private JET_INSTANCE instance;

        /// <summary>
        /// The session used by the test.
        /// </summary>
        private JET_SESID sesid;

        /// <summary>
        /// Identifies the database used by the test.
        /// </summary>
        private JET_DBID dbid;

        /// <summary>
        /// The tableid being used by the test.
        /// </summary>
        private JET_TABLEID tableid;

        /// <summary>
        /// A dictionary that maps column names to column ids.
        /// </summary>
        private IDictionary<string, JET_COLUMNID> columnidDict;

        #region Setup/Teardown

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// All DDL should be done in this method.
        /// </summary>
        [TestInitialize]
        [Description("Fixture setup for HelpersTests")]
        public void Setup()
        {
            this.directory = SetupHelper.CreateRandomDirectory();
            this.database = Path.Combine(this.directory, "database.edb");
            this.table = "table";
            this.instance = SetupHelper.CreateNewInstance(this.directory);

            // turn off logging so initialization is faster
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.PageTempDBMin, SystemParameters.PageTempDBSmallest, null);
            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesid, String.Empty, String.Empty);
            Api.JetCreateDatabase(this.sesid, this.database, String.Empty, out this.dbid, CreateDatabaseGrbit.None);
            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateTable(this.sesid, this.dbid, this.table, 0, 100, out this.tableid);

            JET_COLUMNDEF columndef = null;
            JET_COLUMNID columnid;
            
            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Bit };
            Api.JetAddColumn(this.sesid, this.tableid, "Boolean", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.UnsignedByte };
            Api.JetAddColumn(this.sesid, this.tableid, "Byte", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Short };
            Api.JetAddColumn(this.sesid, this.tableid, "Int16", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Long };
            Api.JetAddColumn(this.sesid, this.tableid, "Int32", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Currency };
            Api.JetAddColumn(this.sesid, this.tableid, "Int64", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.IEEESingle };
            Api.JetAddColumn(this.sesid, this.tableid, "Float", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.IEEEDouble };
            Api.JetAddColumn(this.sesid, this.tableid, "Double", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.DateTime };
            Api.JetAddColumn(this.sesid, this.tableid, "DateTime", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.LongBinary };
            Api.JetAddColumn(this.sesid, this.tableid, "Binary", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.LongText, cp = JET_CP.ASCII };
            Api.JetAddColumn(this.sesid, this.tableid, "ASCII", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.LongText, cp = JET_CP.Unicode };
            Api.JetAddColumn(this.sesid, this.tableid, "Unicode", columndef, null, 0, out columnid);

            if (EsentVersion.SupportsVistaFeatures)
            {
                // Starting with windows Vista esent provides support for these columns.) 
                columndef = new JET_COLUMNDEF() { coltyp = VistaColtyp.UnsignedShort };
                Api.JetAddColumn(this.sesid, this.tableid, "UInt16", columndef, null, 0, out columnid);

                columndef = new JET_COLUMNDEF() { coltyp = VistaColtyp.UnsignedLong };
                Api.JetAddColumn(this.sesid, this.tableid, "UInt32", columndef, null, 0, out columnid);

                columndef = new JET_COLUMNDEF() { coltyp = VistaColtyp.GUID };
                Api.JetAddColumn(this.sesid, this.tableid, "Guid", columndef, null, 0, out columnid);
            }
            else
            {
                // Older version of esent don't support these column types natively so we'll just use binary columns.
                columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Binary, cbMax = 2 };
                Api.JetAddColumn(this.sesid, this.tableid, "UInt16", columndef, null, 0, out columnid);

                columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Binary, cbMax = 4 };
                Api.JetAddColumn(this.sesid, this.tableid, "UInt32", columndef, null, 0, out columnid);

                columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Binary, cbMax = 16 };
                Api.JetAddColumn(this.sesid, this.tableid, "Guid", columndef, null, 0, out columnid);
            }

            // Not natively supported by any version of Esent
            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Binary, cbMax = 8 };
            Api.JetAddColumn(this.sesid, this.tableid, "UInt64", columndef, null, 0, out columnid);

            Api.JetCloseTable(this.sesid, this.tableid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
            Api.JetOpenTable(this.sesid, this.dbid, this.table, null, 0, OpenTableGrbit.None, out this.tableid);

            this.columnidDict = Api.GetColumnDictionary(this.sesid, this.tableid);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        [Description("Fixture cleanup for HelpersTests")]
        public void Teardown()
        {
            Api.JetCloseTable(this.sesid, this.tableid);
            Api.JetEndSession(this.sesid, EndSessionGrbit.None);
            Api.JetTerm(this.instance);
            Cleanup.DeleteDirectoryWithRetry(this.directory);
        }

        /// <summary>
        /// Verify that the HelpersTests.Setup has setup the test fixture properly.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Verify that the HelpersTests.Setup has setup the test fixture properly.")]
        public void VerifyFixtureSetup()
        {
            Assert.IsNotNull(this.table);
            Assert.AreNotEqual(JET_INSTANCE.Nil, this.instance);
            Assert.AreNotEqual(JET_SESID.Nil, this.sesid);
            Assert.AreNotEqual(JET_DBID.Nil, this.dbid);
            Assert.AreNotEqual(JET_TABLEID.Nil, this.tableid);
            Assert.IsNotNull(this.columnidDict);

            Assert.IsTrue(this.columnidDict.ContainsKey("boolean"));
            Assert.IsTrue(this.columnidDict.ContainsKey("byte"));
            Assert.IsTrue(this.columnidDict.ContainsKey("int16"));
            Assert.IsTrue(this.columnidDict.ContainsKey("int32"));
            Assert.IsTrue(this.columnidDict.ContainsKey("int64"));
            Assert.IsTrue(this.columnidDict.ContainsKey("float"));
            Assert.IsTrue(this.columnidDict.ContainsKey("double"));
            Assert.IsTrue(this.columnidDict.ContainsKey("binary"));
            Assert.IsTrue(this.columnidDict.ContainsKey("ascii"));
            Assert.IsTrue(this.columnidDict.ContainsKey("unicode"));
            Assert.IsTrue(this.columnidDict.ContainsKey("guid"));
            Assert.IsTrue(this.columnidDict.ContainsKey("datetime"));
            Assert.IsTrue(this.columnidDict.ContainsKey("uint16"));
            Assert.IsTrue(this.columnidDict.ContainsKey("uint32"));
            Assert.IsTrue(this.columnidDict.ContainsKey("uint64"));

            Assert.IsFalse(this.columnidDict.ContainsKey("nosuchcolumn"));
        }

        #endregion Setup/Teardown

        #region MakeKey Tests

        /// <summary>
        /// Test making a key from true.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test making a key from true.")]
        public void MakeKeyBooleanTrue()
        {
            this.CreateIndexOnColumn("boolean");
            Api.MakeKey(this.sesid, this.tableid, true, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test making a key from false.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test making a key from false.")]
        public void MakeKeyBooleanFalse()
        {
            this.CreateIndexOnColumn("boolean");
            Api.MakeKey(this.sesid, this.tableid, false, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test making a key from a byte.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test making a key from a byte.")]
        public void MakeKeyByte()
        {
            this.CreateIndexOnColumn("byte");
            Api.MakeKey(this.sesid, this.tableid, Any.Byte, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test making a key from a short.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test making a key from a short.")]
        public void MakeKeyInt16()
        {
            this.CreateIndexOnColumn("int16");
            Api.MakeKey(this.sesid, this.tableid, Any.Int16, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test making a key from a ushort.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test making a key from a ushort.")]
        public void MakeKeyUInt16()
        {
            this.CreateIndexOnColumn("uint16");
            Api.MakeKey(this.sesid, this.tableid, Any.UInt16, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test making a key from an int.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test making a key from an int.")]
        public void MakeKeyInt32()
        {
            this.CreateIndexOnColumn("int32");
            Api.MakeKey(this.sesid, this.tableid, Any.Int32, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test making a key from a uint.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test making a key from a uint.")]
        public void MakeKeyUInt32()
        {
            this.CreateIndexOnColumn("uint32");
            Api.MakeKey(this.sesid, this.tableid, Any.UInt32, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test making a key from a long.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test making a key from a long.")]
        public void MakeKeyInt64()
        {
            this.CreateIndexOnColumn("int64");
            Api.MakeKey(this.sesid, this.tableid, Any.Int64, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test making a key from a ulong.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test making a key from a ulong.")]
        public void MakeKeyUInt64()
        {
            this.CreateIndexOnColumn("uint64");
            Api.MakeKey(this.sesid, this.tableid, Any.UInt64, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test making a key from a float.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test making a key from a float.")]
        public void MakeKeyFloat()
        {
            this.CreateIndexOnColumn("float");
            Api.MakeKey(this.sesid, this.tableid, Any.Float, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test making a key from a double.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test making a key from a double.")]
        public void MakeKeyDouble()
        {
            this.CreateIndexOnColumn("double");
            Api.MakeKey(this.sesid, this.tableid, Any.Double, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test making a key from a guid.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test making a key from a guid.")]
        public void MakeKeyGuid()
        {
            this.CreateIndexOnColumn("guid");
            Api.MakeKey(this.sesid, this.tableid, Any.Guid, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test making a key from a DateTime.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test making a key from a DateTime.")]
        public void MakeKeyDateTime()
        {
            this.CreateIndexOnColumn("DateTime");
            Api.MakeKey(this.sesid, this.tableid, DateTime.Now, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test making a key from a unicode string.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test making a key from a unicode string.")]
        public void MakeKeyUnicode()
        {
            this.CreateIndexOnColumn("unicode");
            Api.MakeKey(this.sesid, this.tableid, Any.String, Encoding.Unicode, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test making a key from an ASCII string.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test making a key from an ASCII string.")]
        public void MakeKeyASCII()
        {
            this.CreateIndexOnColumn("ascii");
            Api.MakeKey(this.sesid, this.tableid, Any.String, Encoding.ASCII, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Verify making a key with an invalid encoding throws an exception.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Verify making a key with an invalid encoding throws an exception.")]
        public void VerifyMakeKeyWithInvalidEncodingThrowsException()
        {
            this.CreateIndexOnColumn("unicode");

            try
            {
                Api.MakeKey(this.sesid, this.tableid, Any.String, Encoding.UTF32, MakeKeyGrbit.NewKey);
                Assert.Fail("Expected an EsentException");
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        /// <summary>
        /// Test making a key from an empty string.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test making a key from an empty string.")]
        public void MakeKeyEmptyString()
        {
            this.CreateIndexOnColumn("unicode");
            Api.MakeKey(this.sesid, this.tableid, string.Empty, Encoding.Unicode, MakeKeyGrbit.NewKey | MakeKeyGrbit.KeyDataZeroLength);
        }

        /// <summary>
        /// Test making a key from a null string.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test making a key from a null string.")]
        public void MakeKeyNullString()
        {
            this.CreateIndexOnColumn("unicode");
            Api.MakeKey(this.sesid, this.tableid, null, Encoding.Unicode, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test making a key from an array of bytes.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test making a key from an array of bytes.")]
        public void MakeKeyBinary()
        {
            this.CreateIndexOnColumn("binary");
            Api.MakeKey(this.sesid, this.tableid, Any.Bytes, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test making a key from a zero-length array of bytes.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test making a key from a zero-length array of bytes.")]
        public void MakeKeyZeroLengthBinary()
        {
            this.CreateIndexOnColumn("binary");
            Api.MakeKey(this.sesid, this.tableid, new byte[0], MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test making a key from a null array of bytes.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test making a key from a null array of bytes.")]
        public void MakeKeyNullBinary()
        {
            this.CreateIndexOnColumn("binary");
            Api.MakeKey(this.sesid, this.tableid, null, MakeKeyGrbit.NewKey);
        }

        #endregion MakeKey Tests

        #region MetaData helpers tests

        /// <summary>
        /// Test the helper method that gets table names.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test the helper method that gets table names.")]
        public void GetTableNames()
        {
            string actual = Api.GetTableNames(this.sesid, this.dbid).Single();
            Assert.AreEqual(this.table, actual);
        }

        /// <summary>
        /// Search the column information structures with Linq.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Search the column information structures with Linq.")]
        public void SearchColumnInfosWithLinq()
        {
            IEnumerable<string> columnnames = from c in Api.GetTableColumns(this.sesid, this.tableid)
                             where c.Coltyp == JET_coltyp.Long
                             select c.Name;
            Assert.AreEqual("Int32", columnnames.Single());
        }

        /// <summary>
        /// Iterate through the column information structures.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Iterate through the column information structures.")]
        public void GetTableColumnsFromTableid()
        {
            foreach (ColumnInfo col in Api.GetTableColumns(this.sesid, this.tableid))
            {
                Assert.AreEqual(this.columnidDict[col.Name], col.Columnid);
            }
        }

        /// <summary>
        /// Use GetTableColumnid to get a columnid.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Use GetTableColumnid to get a columnid.")]
        public void GetTableColumnid()
        {
            foreach (string column in this.columnidDict.Keys)
            {
                Assert.AreEqual(this.columnidDict[column], Api.GetTableColumnid(this.sesid, this.tableid, column));
            }
        }

        /// <summary>
        /// Iterate through the column information structures, using
        /// the dbid and tablename to specify the table.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Iterate through the column information structures, using the dbid and tablename to specify the table.")]
        public void GetTableColumnsByTableNameTest()
        {
            foreach (ColumnInfo col in Api.GetTableColumns(this.sesid, this.dbid, this.table))
            {
                Assert.AreEqual(this.columnidDict[col.Name], col.Columnid);
            }
        }

        /// <summary>
        /// Get index information when there are no indexes on the table.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Get index information when there are no indexes on the table.")]
        public void GetIndexInformationNoIndexes()
        {
            IEnumerable<IndexInfo> indexes = Api.GetTableIndexes(this.sesid, this.tableid);
            Assert.AreEqual(0, indexes.Count());
        }

        /// <summary>
        /// Get index information for one index.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Get index information for one index.")]
        public void GetIndexInformationOneIndex()
        {
            string indexname = "myindex";
            string indexdef = "+ascii\0\0";
            CreateIndexGrbit grbit = CreateIndexGrbit.IndexUnique;

            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateIndex(this.sesid, this.tableid, indexname, grbit, indexdef, indexdef.Length, 100);
            IEnumerable<IndexInfo> indexes = Api.GetTableIndexes(this.sesid, this.tableid);

            // There should be only one index
            IndexInfo info = indexes.Single();
            Assert.AreEqual(indexname, info.Name);
            Assert.AreEqual(grbit, info.Grbit);

            Assert.AreEqual(1, info.IndexSegments.Length);
            Assert.IsTrue(0 == string.Compare("ascii", info.IndexSegments[0].ColumnName, true));
            Assert.IsTrue(info.IndexSegments[0].IsAscending);
            Assert.AreEqual(JET_coltyp.LongText, info.IndexSegments[0].Coltyp);
            Assert.IsTrue(info.IndexSegments[0].IsASCII);

            Api.JetRollback(this.sesid, RollbackTransactionGrbit.None);
        }

        /// <summary>
        /// Get index information for one index, where the index has multiple segments.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Get index information for one index, where the index has multiple segments.")]
        public void GetIndexInformationOneIndexMultipleSegments()
        {
            string indexname = "multisegmentindex";
            string indexdef = "+ascii\0-boolean\0\0";
            CreateIndexGrbit grbit = CreateIndexGrbit.IndexUnique;

            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateIndex(this.sesid, this.tableid, indexname, grbit, indexdef, indexdef.Length, 100);
            IEnumerable<IndexInfo> indexes = Api.GetTableIndexes(this.sesid, this.tableid);

            // There should be only one index
            IndexInfo info = indexes.Single();
            Assert.AreEqual(indexname, info.Name);
            Assert.AreEqual(grbit, info.Grbit);

            Assert.AreEqual(2, info.IndexSegments.Length);
            Assert.IsTrue(0 == string.Compare("ascii", info.IndexSegments[0].ColumnName, true));
            Assert.IsTrue(info.IndexSegments[0].IsAscending);
            Assert.AreEqual(JET_coltyp.LongText, info.IndexSegments[0].Coltyp);
            Assert.IsTrue(info.IndexSegments[0].IsASCII);

            Assert.IsTrue(0 == string.Compare("boolean", info.IndexSegments[1].ColumnName, true));
            Assert.IsFalse(info.IndexSegments[1].IsAscending);
            Assert.AreEqual(JET_coltyp.Bit, info.IndexSegments[1].Coltyp);

            Api.JetRollback(this.sesid, RollbackTransactionGrbit.None);
        }

        /// <summary>
        /// Get index information for one index.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Get index information for one index.")]
        public void GetIndexInformationByTableNameOneIndex()
        {
            string indexname = "myindex";
            string indexdef = "+ascii\0\0";
            CreateIndexGrbit grbit = CreateIndexGrbit.IndexUnique;

            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateIndex(this.sesid, this.tableid, indexname, grbit, indexdef, indexdef.Length, 100);
            IEnumerable<IndexInfo> indexes = Api.GetTableIndexes(this.sesid, this.dbid, this.table);

            // There should be only one index
            IndexInfo info = indexes.Single();
            Assert.AreEqual(indexname, info.Name);
            Assert.AreEqual(grbit, info.Grbit);

            Assert.AreEqual(1, info.IndexSegments.Length);
            Assert.IsTrue(0 == string.Compare("ascii", info.IndexSegments[0].ColumnName, true));
            Assert.IsTrue(info.IndexSegments[0].IsAscending);
            Assert.AreEqual(JET_coltyp.LongText, info.IndexSegments[0].Coltyp);
            Assert.IsTrue(info.IndexSegments[0].IsASCII);

            Api.JetRollback(this.sesid, RollbackTransactionGrbit.None);
        }

        /// <summary>
        /// Get index information for one index.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Get index information for one index.")]
        public void GetIndexInformationOneIndexWithCompareOptions()
        {
            const string Indexname = "myindex";
            const string Indexdef = "-unicode\0\0";

            var pidxUnicode = new JET_UNICODEINDEX
            {
                lcid = CultureInfo.CurrentCulture.LCID,
                dwMapFlags = Conversions.LCMapFlagsFromCompareOptions(CompareOptions.IgnoreSymbols | CompareOptions.IgnoreCase),
            };

            var indexcreate = new JET_INDEXCREATE
            {
                szIndexName = Indexname,
                szKey = Indexdef,
                cbKey = Indexdef.Length,
                grbit = CreateIndexGrbit.IndexDisallowNull,
                pidxUnicode = pidxUnicode,
            };

            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateIndex2(this.sesid, this.tableid, new[] { indexcreate }, 1);
            IEnumerable<IndexInfo> indexes = Api.GetTableIndexes(this.sesid, this.tableid);

            // There should be only one index
            IndexInfo info = indexes.Single();
            Assert.AreEqual(Indexname, info.Name);
            Assert.AreEqual(CreateIndexGrbit.IndexDisallowNull, info.Grbit);

            Assert.AreEqual(1, info.IndexSegments.Length);
            Assert.IsTrue(0 == string.Compare("unicode", info.IndexSegments[0].ColumnName, true));
            Assert.IsFalse(info.IndexSegments[0].IsAscending);
            Assert.AreEqual(JET_coltyp.LongText, info.IndexSegments[0].Coltyp);
            Assert.IsFalse(info.IndexSegments[0].IsASCII);
            Assert.AreEqual(CompareOptions.IgnoreSymbols | CompareOptions.IgnoreCase, info.CompareOptions);

            Api.JetRollback(this.sesid, RollbackTransactionGrbit.None);
        }

        #endregion MetaData helpers tests

        #region Helper methods

        /// <summary>
        /// Creates a record with the given column set to the specified value.
        /// The tableid is positioned on the new record.
        /// </summary>
        /// <param name="columnid">The column to set.</param>
        /// <param name="data">The data to set.</param>
        private void InsertRecord(JET_COLUMNID columnid, byte[] data)
        {
            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.JetSetColumn(this.sesid, this.tableid, columnid, data, (null == data) ? 0 : data.Length, SetColumnGrbit.None, null);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);      
        }

        /// <summary>
        /// Test setting and retrieving a null column.
        /// </summary>
        /// <typeparam name="T">The struct type that is being returned.</typeparam>
        /// <param name="column">The name of the column to set.</param>
        /// <param name="retrieveFunc">The function to use when retrieving the column.</param>
        private void NullColumnTest<T>(string column, Func<JET_SESID, JET_TABLEID, JET_COLUMNID, T?> retrieveFunc) where T : struct
        {
            JET_COLUMNID columnid = this.columnidDict[column];
            this.InsertRecord(columnid, null);
            Assert.IsNull(retrieveFunc(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Update the cursor and goto the returned bookmark.
        /// </summary>
        private void UpdateAndGotoBookmark()
        {
            var bookmark = new byte[256];
            int bookmarkSize;
            Api.JetUpdate(this.sesid, this.tableid, bookmark, bookmark.Length, out bookmarkSize);
            Api.JetGotoBookmark(this.sesid, this.tableid, bookmark, bookmarkSize);
        }

        /// <summary>
        /// Create an ascending index over the given column. The tableid will be
        /// positioned to the new index.
        /// </summary>
        /// <param name="column">The name of the column to create the index on.</param>
        private void CreateIndexOnColumn(string column)
        {
            string indexname = String.Format("index_{0}", column);
            string indexdef = String.Format("+{0}\0\0", column);

            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateIndex(this.sesid, this.tableid, indexname, CreateIndexGrbit.None, indexdef, indexdef.Length, 100);
            Api.JetSetCurrentIndex(this.sesid, this.tableid, indexname);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.None);
        }

        #endregion Helper methods
    }
}
