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
    using Microsoft.Isam.Esent;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the various Set/RetrieveColumn* methods and
    /// the helper methods that retrieve meta-data.
    /// </summary>
    [TestClass]
    public class RetrieveColumnHelpers
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
        private Dictionary<string, JET_COLUMNID> columnidDict;

        #region Setup/Teardown

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// All DDL should be done in this method.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.directory = SetupHelper.CreateRandomDirectory();
            this.database = Path.Combine(this.directory, "database.edb");
            this.table = "table";
            this.instance = SetupHelper.CreateNewInstance(this.directory);

            // turn off logging so initialization is faster
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesid, String.Empty, String.Empty);
            Api.JetCreateDatabase(this.sesid, this.database, String.Empty, out this.dbid, CreateDatabaseGrbit.None);
            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateTable(this.sesid, this.dbid, this.table, 0, 100, out this.tableid);

            JET_COLUMNDEF columndef = null;
            JET_COLUMNID columnid = new JET_COLUMNID();
            
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

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.LongBinary };
            Api.JetAddColumn(this.sesid, this.tableid, "Binary", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.LongText, cp = JET_CP.ASCII };
            Api.JetAddColumn(this.sesid, this.tableid, "ASCII", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.LongText, cp = JET_CP.Unicode };
            Api.JetAddColumn(this.sesid, this.tableid, "Unicode", columndef, null, 0, out columnid);

            // Not all version of esent support these column types natively so we'll just use binary columns.
            // (Starting with windows Vista esent provides support for these columns.)  
            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Binary, cbMax = 2 };
            Api.JetAddColumn(this.sesid, this.tableid, "UInt16", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Binary, cbMax = 4 };
            Api.JetAddColumn(this.sesid, this.tableid, "UInt32", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Binary, cbMax = 8 };
            Api.JetAddColumn(this.sesid, this.tableid, "UInt64", columndef, null, 0, out columnid);

            columndef = new JET_COLUMNDEF() { coltyp = JET_coltyp.Binary, cbMax = 16 };
            Api.JetAddColumn(this.sesid, this.tableid, "Guid", columndef, null, 0, out columnid);

            Api.JetCloseTable(this.sesid, this.tableid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
            Api.JetOpenTable(this.sesid, this.dbid, this.table, null, 0, OpenTableGrbit.None, out this.tableid);

            this.columnidDict = Api.GetColumnDictionary(this.sesid, this.tableid);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            Api.JetCloseTable(this.sesid, this.tableid);
            Api.JetEndSession(this.sesid, EndSessionGrbit.None);
            Api.JetTerm(this.instance);
            Directory.Delete(this.directory, true);
        }

        /// <summary>
        /// Verify that the test class has setup the test fixture properly.
        /// </summary>
        [TestMethod]
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
            Assert.IsTrue(this.columnidDict.ContainsKey("uint16"));
            Assert.IsTrue(this.columnidDict.ContainsKey("uint32"));
            Assert.IsTrue(this.columnidDict.ContainsKey("uint64"));

            Assert.IsFalse(this.columnidDict.ContainsKey("nosuchcolumn"));
        }

        #endregion Setup/Teardown

        #region RetrieveColumnAs tests

        /// <summary>
        /// Check that retrieving a column returns null
        /// </summary>
        [TestMethod]
        public void RetrieveNullColumn()
        {
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            this.UpdateAndGotoBookmark();
            Assert.IsNull(Api.RetrieveColumnAsInt32(this.sesid, this.tableid, this.columnidDict["Int32"]));
        }

        /// <summary>
        /// Retrieve a column as boolean.
        /// </summary>
        [TestMethod]
        public void RetrieveAsBoolean()
        {
            var columnid = this.columnidDict["Boolean"];
            var value = Any.Boolean;
            this.InsertRecord(columnid, BitConverter.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsBoolean(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a column as a byte.
        /// </summary>
        [TestMethod]
        public void RetrieveAsByte()
        {
            var columnid = this.columnidDict["Byte"];
            byte[] b = new byte[] { 0x55 };
            this.InsertRecord(columnid, b);
            Assert.AreEqual(b[0], Api.RetrieveColumnAsByte(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a column as a short.
        /// </summary>
        [TestMethod]
        public void RetrieveAsInt16()
        {
            var columnid = this.columnidDict["Int16"];
            var value = Any.Int16;
            this.InsertRecord(columnid, BitConverter.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsInt16(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a column as a ushort.
        /// </summary>
        [TestMethod]
        public void RetrieveAsUInt16()
        {
            var columnid = this.columnidDict["UInt16"];
            var value = Any.UInt16;
            this.InsertRecord(columnid, BitConverter.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsUInt16(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a column as an int.
        /// </summary>
        [TestMethod]
        public void RetrieveAsInt32()
        {
            var columnid = this.columnidDict["Int32"];
            var value = Any.Int32;
            this.InsertRecord(columnid, BitConverter.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsInt32(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a column as a uint.
        /// </summary>
        [TestMethod]
        public void RetrieveAsUInt32()
        {
            var columnid = this.columnidDict["UInt32"];
            var value = Any.UInt32;
            this.InsertRecord(columnid, BitConverter.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsUInt32(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a column as a long.
        /// </summary>
        [TestMethod]
        public void RetrieveAsInt64()
        {
            var columnid = this.columnidDict["Int64"];
            var value = Any.Int64;
            this.InsertRecord(columnid, BitConverter.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsInt64(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a column as a ulong.
        /// </summary>
        [TestMethod]
        public void RetrieveAsUInt64()
        {
            var columnid = this.columnidDict["UInt64"];
            var value = Any.UInt64;
            this.InsertRecord(columnid, BitConverter.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsUInt64(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a column as a float.
        /// </summary>
        [TestMethod]
        public void RetrieveAsFloat()
        {
            var columnid = this.columnidDict["Float"];
            var value = Any.Float;
            this.InsertRecord(columnid, BitConverter.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsFloat(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a column as a double.
        /// </summary>
        [TestMethod]
        public void RetrieveAsDouble()
        {
            var columnid = this.columnidDict["Double"];
            var value = Any.Double;
            this.InsertRecord(columnid, BitConverter.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsDouble(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a column as a Guid.
        /// </summary>
        [TestMethod]
        public void RetrieveAsGuid()
        {
            var columnid = this.columnidDict["Guid"];
            var value = Any.Guid;
            this.InsertRecord(columnid, value.ToByteArray());
            Assert.AreEqual(value, Api.RetrieveColumnAsGuid(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve a column as ASCII
        /// </summary>
        [TestMethod]
        public void RetrieveAsASCII()
        {
            var columnid = this.columnidDict["ASCII"];
            var value = Any.String;
            this.InsertRecord(columnid, Encoding.ASCII.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsString(this.sesid, this.tableid, columnid, Encoding.ASCII));
        }

        /// <summary>
        /// Retrieve a column as Unicode
        /// </summary>
        [TestMethod]
        public void RetrieveAsUnicode()
        {
            var columnid = this.columnidDict["Unicode"];
            var value = Any.String;
            this.InsertRecord(columnid, Encoding.Unicode.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsString(this.sesid, this.tableid, columnid, Encoding.Unicode));
        }

        /// <summary>
        /// Retrieve a column as a (unicode) string
        /// </summary>
        [TestMethod]
        public void RetrieveAsString()
        {
            var columnid = this.columnidDict["Unicode"];
            var value = Any.String;
            this.InsertRecord(columnid, Encoding.Unicode.GetBytes(value));
            Assert.AreEqual(value, Api.RetrieveColumnAsString(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Retrieve an empty string to make sure
        /// it is handled differently from a null column.
        /// </summary>
        [TestMethod]
        public void RetrieveEmptyString()
        {
            var columnid = this.columnidDict["Unicode"];
            var value = String.Empty;
            byte[] data = Encoding.Unicode.GetBytes(value);
            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.JetSetColumn(this.sesid, this.tableid, columnid, data, data.Length, SetColumnGrbit.ZeroLength, null);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
            Assert.AreEqual(value, Api.RetrieveColumnAsString(this.sesid, this.tableid, columnid, Encoding.Unicode));
        }

        #endregion RetrieveColumnAs tests

        #region SetColumn Tests

        /// <summary>
        /// Test setting a unicode column from a string.
        /// </summary>
        [TestMethod]
        public void SetUnicodeString()
        {
            var columnid = this.columnidDict["unicode"];
            var expected = Any.String;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected, Encoding.Unicode);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
            
            var actual = Encoding.Unicode.GetString(Api.RetrieveColumn(this.sesid, this.tableid, columnid));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting an ASCII column from a string.
        /// </summary>
        [TestMethod]
        public void SetASCIIString()
        {
            var columnid = this.columnidDict["ascii"];
            var expected = Any.String;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected, Encoding.ASCII);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            var actual = Encoding.ASCII.GetString(Api.RetrieveColumn(this.sesid, this.tableid, columnid));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Using an encoding which is neither ASCII nor Unicode should thrown an exception.
        /// </summary>
        [TestMethod]
        public void VerifySetStringWithInvalidEncodingThrowsException()
        {
            var columnid = this.columnidDict["unicode"];

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);

            try
            {
                Api.SetColumn(this.sesid, this.tableid, columnid, Any.String, Encoding.UTF8);
                Assert.Fail("Expected an ESENT exception");
            }
            catch (EsentException)
            {
            }
        }

        /// <summary>
        /// Test setting a column from an empty string.
        /// </summary>
        [TestMethod]
        public void SetEmptyString()
        {
            var columnid = this.columnidDict["unicode"];
            var expected = string.Empty;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected, Encoding.Unicode);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            var actual = Encoding.Unicode.GetString(Api.RetrieveColumn(this.sesid, this.tableid, columnid));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a column from a null string.
        /// </summary>
        [TestMethod]
        public void SetNullString()
        {
            var columnid = this.columnidDict["unicode"];

            this.InsertRecord(columnid, Encoding.Unicode.GetBytes(Any.String));

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Replace);
            Api.SetColumn(this.sesid, this.tableid, columnid, null, Encoding.Unicode);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            Assert.IsNull(Api.RetrieveColumn(this.sesid, this.tableid, columnid));
        }

        /// <summary>
        /// Test setting a column from a boolean.
        /// </summary>
        [TestMethod]
        public void SetBoolean()
        {
            var columnid = this.columnidDict["boolean"];
            var expected = Any.Boolean;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            var actual = BitConverter.ToBoolean(Api.RetrieveColumn(this.sesid, this.tableid, columnid), 0);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a column from a boolean.
        /// </summary>
        [TestMethod]
        public void SetByte()
        {
            var columnid = this.columnidDict["byte"];
            var expected = Any.Byte;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            byte actual = Api.RetrieveColumn(this.sesid, this.tableid, columnid)[0];
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a column from a short.
        /// </summary>
        [TestMethod]
        public void SetInt16()
        {
            var columnid = this.columnidDict["int16"];
            var expected = Any.Int16;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            var actual = BitConverter.ToInt16(Api.RetrieveColumn(this.sesid, this.tableid, columnid), 0);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a column from an int.
        /// </summary>
        [TestMethod]
        public void SetInt32()
        {
            var columnid = this.columnidDict["int32"];
            var expected = Any.Int32;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            var actual = BitConverter.ToInt32(Api.RetrieveColumn(this.sesid, this.tableid, columnid), 0);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a column from a long.
        /// </summary>
        [TestMethod]
        public void SetInt64()
        {
            var columnid = this.columnidDict["int64"];
            var expected = Any.Int64;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            var actual = BitConverter.ToInt64(Api.RetrieveColumn(this.sesid, this.tableid, columnid), 0);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a column from a ushort.
        /// </summary>
        [TestMethod]
        public void SetUInt16()
        {
            var columnid = this.columnidDict["uint16"];
            var expected = Any.UInt16;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            var actual = BitConverter.ToUInt16(Api.RetrieveColumn(this.sesid, this.tableid, columnid), 0);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a column from a uint.
        /// </summary>
        [TestMethod]
        public void SetUInt32()
        {
            var columnid = this.columnidDict["uint32"];
            var expected = Any.UInt32;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            var actual = BitConverter.ToUInt32(Api.RetrieveColumn(this.sesid, this.tableid, columnid), 0);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a column from a ulong.
        /// </summary>
        [TestMethod]
        public void SetUInt64()
        {
            var columnid = this.columnidDict["uint64"];
            var expected = Any.UInt64;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            var actual = BitConverter.ToUInt64(Api.RetrieveColumn(this.sesid, this.tableid, columnid), 0);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a column from a float.
        /// </summary>
        [TestMethod]
        public void SetFloat()
        {
            var columnid = this.columnidDict["float"];
            var expected = Any.Float;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            var actual = BitConverter.ToSingle(Api.RetrieveColumn(this.sesid, this.tableid, columnid), 0);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a column from a double.
        /// </summary>
        [TestMethod]
        public void SetDouble()
        {
            var columnid = this.columnidDict["double"];
            var expected = Any.Double;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            var actual = BitConverter.ToDouble(Api.RetrieveColumn(this.sesid, this.tableid, columnid), 0);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a column from a guid.
        /// </summary>
        [TestMethod]
        public void SetGuid()
        {
            var columnid = this.columnidDict["guid"];
            var expected = Any.Guid;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            var actual = new Guid(Api.RetrieveColumn(this.sesid, this.tableid, columnid));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a column from an array of bytes.
        /// </summary>
        [TestMethod]
        public void SetBytes()
        {
            var columnid = this.columnidDict["binary"];
            var expected = Any.Bytes;

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, expected);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            var actual = Api.RetrieveColumn(this.sesid, this.tableid, columnid);
            this.AssertAreEqual(expected, actual);
        }

        /// <summary>
        /// Test setting a binary column from a zero-length array.
        /// </summary>
        [TestMethod]
        public void SetZeroLengthBytes()
        {
            var columnid = this.columnidDict["binary"];

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, new byte[0]);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            Assert.AreEqual(0, Api.RetrieveColumn(this.sesid, this.tableid, columnid).Length);
        }

        /// <summary>
        /// Test setting a binary column from a null object.
        /// </summary>
        [TestMethod]
        public void SetNullBytes()
        {
            var columnid = this.columnidDict["binary"];

            Api.JetBeginTransaction(this.sesid);
            Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            Api.SetColumn(this.sesid, this.tableid, columnid, null);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);

            Assert.IsNull(Api.RetrieveColumn(this.sesid, this.tableid, columnid));
        }

        #endregion SetColumn Tests

        #region MakeKey Tests

        /// <summary>
        /// Test make a key from true.
        /// </summary>
        [TestMethod]
        public void MakeKeyBooleanTrue()
        {
            this.CreateIndexOnColumn("boolean");
            Api.MakeKey(this.sesid, this.tableid, true, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a boolean.
        /// </summary>
        [TestMethod]
        public void MakeKeyBooleanFalse()
        {
            this.CreateIndexOnColumn("boolean");
            Api.MakeKey(this.sesid, this.tableid, false, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a byte.
        /// </summary>
        [TestMethod]
        public void MakeKeyByte()
        {
            this.CreateIndexOnColumn("byte");
            Api.MakeKey(this.sesid, this.tableid, Any.Byte, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a short.
        /// </summary>
        [TestMethod]
        public void MakeKeyInt16()
        {
            this.CreateIndexOnColumn("int16");
            Api.MakeKey(this.sesid, this.tableid, Any.Int16, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a ushort.
        /// </summary>
        [TestMethod]
        public void MakeKeyUInt16()
        {
            this.CreateIndexOnColumn("uint16");
            Api.MakeKey(this.sesid, this.tableid, Any.UInt16, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from an int.
        /// </summary>
        [TestMethod]
        public void MakeKeyInt32()
        {
            this.CreateIndexOnColumn("int32");
            Api.MakeKey(this.sesid, this.tableid, Any.Int32, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a uint.
        /// </summary>
        [TestMethod]
        public void MakeKeyUInt32()
        {
            this.CreateIndexOnColumn("uint32");
            Api.MakeKey(this.sesid, this.tableid, Any.UInt32, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a long.
        /// </summary>
        [TestMethod]
        public void MakeKeyInt64()
        {
            this.CreateIndexOnColumn("int64");
            Api.MakeKey(this.sesid, this.tableid, Any.Int64, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a ulong.
        /// </summary>
        [TestMethod]
        public void MakeKeyUInt64()
        {
            this.CreateIndexOnColumn("uint64");
            Api.MakeKey(this.sesid, this.tableid, Any.UInt64, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a float.
        /// </summary>
        [TestMethod]
        public void MakeKeyFloat()
        {
            this.CreateIndexOnColumn("float");
            Api.MakeKey(this.sesid, this.tableid, Any.Float, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a double.
        /// </summary>
        [TestMethod]
        public void MakeKeyDouble()
        {
            this.CreateIndexOnColumn("double");
            Api.MakeKey(this.sesid, this.tableid, Any.Double, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a guid.
        /// </summary>
        [TestMethod]
        public void MakeKeyGuid()
        {
            this.CreateIndexOnColumn("guid");
            Api.MakeKey(this.sesid, this.tableid, Any.Guid, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a string.
        /// </summary>
        [TestMethod]
        public void MakeKeyUnicode()
        {
            this.CreateIndexOnColumn("unicode");
            Api.MakeKey(this.sesid, this.tableid, Any.String, Encoding.Unicode, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a string.
        /// </summary>
        [TestMethod]
        public void MakeKeyASCII()
        {
            this.CreateIndexOnColumn("ascii");
            Api.MakeKey(this.sesid, this.tableid, Any.String, Encoding.ASCII, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Making a key with an invalid encoding throws an exception.
        /// </summary>
        [TestMethod]
        public void VerifyMakeKeyWithInvalidEncodingThrowsException()
        {
            this.CreateIndexOnColumn("unicode");

            try
            {
                Api.MakeKey(this.sesid, this.tableid, Any.String, Encoding.UTF32, MakeKeyGrbit.NewKey);
                Assert.Fail("Expected an EsentException");
            }
            catch (EsentException)
            {
            }
        }

        /// <summary>
        /// Test make a key from an empty string.
        /// </summary>
        [TestMethod]
        public void MakeKeyEmptyString()
        {
            this.CreateIndexOnColumn("unicode");
            Api.MakeKey(this.sesid, this.tableid, string.Empty, Encoding.Unicode, MakeKeyGrbit.NewKey | MakeKeyGrbit.KeyDataZeroLength);
        }

        /// <summary>
        /// Test make a key from a string.
        /// </summary>
        [TestMethod]
        public void MakeKeyNullString()
        {
            this.CreateIndexOnColumn("unicode");
            Api.MakeKey(this.sesid, this.tableid, null, Encoding.Unicode, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from an array of bytes.
        /// </summary>
        [TestMethod]
        public void MakeKeyBinary()
        {
            this.CreateIndexOnColumn("binary");
            Api.MakeKey(this.sesid, this.tableid, Any.Bytes, MakeKeyGrbit.NewKey);
        }

        /// <summary>
        /// Test make a key from a null array of bytes.
        /// </summary>
        [TestMethod]
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
        public void GetTableNames()
        {
            string actual = Api.GetTableNames(this.sesid, this.dbid).Single();
            Assert.AreEqual(this.table, actual);
        }

        /// <summary>
        /// Search the column information structures with Linq.
        /// </summary>
        [TestMethod]
        public void SearchColumnInfos()
        {
            var columnnames = from c in Api.GetTableColumns(this.sesid, this.tableid)
                             where c.Coltyp == JET_coltyp.Long
                             select c.Name;
            Assert.AreEqual("Int32", columnnames.Single());
        }

        /// <summary>
        /// Iterate through the column information structures.
        /// </summary>
        [TestMethod]
        public void GetTableColumnsFromTableidTest()
        {
            foreach (ColumnInfo col in Api.GetTableColumns(this.sesid, this.tableid))
            {
                Assert.AreEqual(this.columnidDict[col.Name], col.Columnid);
            }
        }

        /// <summary>
        /// Iterate through the column information structures, using
        /// the dbid and tablename to specify the table.
        /// </summary>
        [TestMethod]
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
        public void GetIndexInformationNoIndexes()
        {
            IEnumerable<IndexInfo> indexes = Api.GetTableIndexes(this.sesid, this.tableid);
            Assert.AreEqual(0, indexes.Count());
        }

        /// <summary>
        /// Get index information for one index
        /// </summary>
        [TestMethod]
        public void GetIndexInformationOneIndex()
        {
            var indexname = "myindex";
            var indexdef = "+ascii\0\0";
            var grbit = CreateIndexGrbit.IndexUnique;

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

        #endregion MetaData helpers tests

        #region Helper methods

        /// <summary>
        /// Assert that two byte arrays are equal to each other.
        /// (Same length and contents).
        /// </summary>
        /// <param name="a">The first byte array.</param>
        /// <param name="b">The second byte array.</param>
        private void AssertAreEqual(byte[] a, byte[] b)
        {
            Assert.AreEqual(a.Length, b.Length);
            for (int i = 0; i < a.Length; ++i)
            {
                Assert.AreEqual(a[i], b[i]);
            }
        }

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
            Api.JetSetColumn(this.sesid, this.tableid, columnid, data, data.Length, SetColumnGrbit.None, null);
            this.UpdateAndGotoBookmark();
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);      
        }

        /// <summary>
        /// Update the cursor and goto the returned bookmark.
        /// </summary>
        private void UpdateAndGotoBookmark()
        {
            byte[] bookmark = new byte[256];
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
            var indexname = String.Format("index_{0}", column);
            var indexdef = String.Format("+{0}\0\0", column);

            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateIndex(this.sesid, this.tableid, indexname, CreateIndexGrbit.None, indexdef, indexdef.Length, 100);
            Api.JetSetCurrentIndex(this.sesid, this.tableid, indexname);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.None);
        }

        #endregion Helper methods
    }
}
