//-----------------------------------------------------------------------
// <copyright file="ParameterCheckingTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Isam.Esent.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
    /// <summary>
    /// Test for API parameter validation code
    /// </summary>
    [TestClass]
    public class ParameterCheckingTests
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
        /// Columnid of the Long column in the table.
        /// </summary>
        private JET_COLUMNID columnid;

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

            // turn off logging and temporary tables so initialization is faster
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            Api.JetInit(ref this.instance);
            Api.JetBeginSession(this.instance, out this.sesid, String.Empty, String.Empty);
            Api.JetCreateDatabase(this.sesid, this.database, String.Empty, out this.dbid, CreateDatabaseGrbit.None);
            Api.JetBeginTransaction(this.sesid);
            Api.JetCreateTable(this.sesid, this.dbid, this.table, 0, 100, out this.tableid);

            var columndef = new JET_COLUMNDEF
                {
                    coltyp = JET_coltyp.Long,
                    grbit = ColumndefGrbit.ColumnEscrowUpdate
                };

            Api.JetAddColumn(this.sesid, this.tableid, "column", columndef, BitConverter.GetBytes(0), 4, out this.columnid);

            Api.JetCloseTable(this.sesid, this.tableid);
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
            Api.JetOpenTable(this.sesid, this.dbid, this.table, null, 0, OpenTableGrbit.None, out this.tableid);
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
        [Priority(1)]
        public void VerifyFixtureSetup()
        {
            Assert.AreNotEqual(JET_INSTANCE.Nil, this.instance);
            Assert.AreNotEqual(JET_SESID.Nil, this.sesid);
            Assert.AreNotEqual(JET_COLUMNID.Nil, this.columnid);

            JET_COLUMNDEF columndef;
            Api.JetGetTableColumnInfo(this.sesid, this.tableid, this.columnid, out columndef);
            Assert.AreEqual(JET_coltyp.Long, columndef.coltyp);
        }

        #endregion Setup/Teardown

        #region EsentErrorException tests

        /// <summary>
        /// Check that an exception is thrown when an API calls fails
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(EsentErrorException))]
        public void EsentExceptionIsThrownOnApiError()
        {
            // The session shouldn't be in a transaction so this will
            // generate an error.
            Api.JetCommitTransaction(this.sesid, CommitTransactionGrbit.None);
        }

        /// <summary>
        /// Check that an exception contains the right error code.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void EsentExceptionHasErrorCode()
        {
            try
            {
                JET_TABLEID tableid;
                Api.JetOpenTable(this.sesid, this.dbid, "nosuchtable", null, 0, OpenTableGrbit.None, out tableid);
                Assert.Fail("Should have thrown an exception");
            }
            catch (EsentErrorException ex)
            {
                Assert.AreEqual(JET_err.ObjectNotFound, ex.Error);
            }
        }

        #endregion EsentErrorException tests

        #region Database API

        /// <summary>
        /// Check that an exception is thrown when JetCreateDatabase gets a 
        /// null database name.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetCreateDatabaseThrowsExceptionWhenDatabaseNameIsNull()
        {
            JET_DBID dbid;
            Api.JetCreateDatabase(this.sesid, null, null, out dbid, CreateDatabaseGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetAttachDatabase gets a 
        /// null database name.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetAttachDatabaseThrowsExceptionWhenDatabaseNameIsNull()
        {
            Api.JetAttachDatabase(this.sesid, null, AttachDatabaseGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetOpenDatabase gets a 
        /// null database name.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetOpenDatabaseThrowsExceptionWhenDatabaseNameIsNull()
        {
            JET_DBID dbid;
            Api.JetOpenDatabase(this.sesid, null, null, out dbid, OpenDatabaseGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetDetachDatabase gets a 
        /// null database name.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetDetachDatabaseThrowsExceptionWhenDatabaseNameIsNull()
        {
            Api.JetDetachDatabase(this.sesid, null);
        }

        /// <summary>
        /// Check that an exception is thrown when JetOpenTable gets a 
        /// null table name.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetOpenTableThrowsExceptionWhenTableNameIsNull()
        {
            JET_TABLEID tableid;
            Api.JetOpenTable(this.sesid, this.dbid, null, null, 0, OpenTableGrbit.None, out tableid);
        }

        #endregion Database API

        #region DDL

        /// <summary>
        /// Check that an exception is thrown when JetCreateTable gets a 
        /// null table name.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetCreateTableThrowsExceptionWhenTableNameIsNull()
        {
            JET_TABLEID tableid;
            Api.JetCreateTable(this.sesid, this.dbid, null, 0, 100, out tableid);
        }

        /// <summary>
        /// Check that an exception is thrown when JetAddColumn gets a 
        /// null column name.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetAddColumnThrowsExceptionWhenColumnNameIsNull()
        {
            var columndef = new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Binary,
            };

            JET_COLUMNID columnid;
            Api.JetAddColumn(
                this.sesid,
                this.tableid,
                null,
                columndef,
                null,
                0,
                out columnid);
        }

        /// <summary>
        /// Check that an exception is thrown when JetAddColumn gets a 
        /// null column definition.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetAddColumnThrowsExceptionWhenColumndefIsNull()
        {
            JET_COLUMNID columnid;
            Api.JetAddColumn(
                this.sesid,
                this.tableid,
                "column",
                null,
                null,
                0,
                out columnid);
        }

        /// <summary>
        /// Check that an exception is thrown when JetAddColumn gets a 
        /// default value length that is negative.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetAddColumnThrowsExceptionWhenDefaultValueLengthIsNegative()
        {
            var columndef = new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Binary,
            };

            JET_COLUMNID columnid;
            Api.JetAddColumn(
                this.sesid,
                this.tableid,
                "NegativeDefaultValue",
                columndef,
                null,
                -1,
                out columnid);
        }

        /// <summary>
        /// Check that an exception is thrown when JetAddColumn gets a 
        /// default value length that is too long.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetAddColumnThrowsExceptionWhenDefaultValueLengthIsTooLong()
        {
            var defaultValue = new byte[10];
            var columndef = new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Binary,
            };

            JET_COLUMNID columnid;
            Api.JetAddColumn(
                this.sesid,
                this.tableid,
                "BadDefaultValue",
                columndef,
                defaultValue,
                defaultValue.Length + 1,
                out columnid);
        }

        /// <summary>
        /// Check that an exception is thrown when JetAddColumn gets a 
        /// default value that is null with a non-zero default value size.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetAddColumnThrowsExceptionWhenDefaultValueIsUnexpectedNull()
        {
            var defaultValue = new byte[10];
            var columndef = new JET_COLUMNDEF()
            {
                coltyp = JET_coltyp.Binary,
            };

            JET_COLUMNID columnid;
            Api.JetAddColumn(
                this.sesid,
                this.tableid,
                "BadDefaultValue",
                columndef,
                null,
                1,
                out columnid);
        }

        /// <summary>
        /// Check that an exception is thrown when JetCreateIndex gets a 
        /// null name.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetCreateIndexThrowsExceptionWhenNameIsNull()
        {
            Api.JetCreateIndex(this.sesid, this.tableid, null, CreateIndexGrbit.None, "+foo\0", 6, 100);
        }

        /// <summary>
        /// Check that an exception is thrown when JetCreateIndex gets a 
        /// density that is negative.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetCreateIndexThrowsExceptionWhenDensityIsNegative()
        {
            Api.JetCreateIndex(this.sesid, this.tableid, "BadIndex,", CreateIndexGrbit.None, "+foo\0", 6, -1);
        }

        /// <summary>
        /// Check that an exception is thrown when JetCreateIndex gets a 
        /// key description length that is negative.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetCreateIndexThrowsExceptionWhenKeyDescriptionLengthIsNegative()
        {
            Api.JetCreateIndex(this.sesid, this.tableid, "BadIndex,", CreateIndexGrbit.None, "+foo\0", -1, 100);
        }

        /// <summary>
        /// Check that an exception is thrown when JetCreateIndex gets a 
        /// key description length that is too long.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetCreateIndexThrowsExceptionWhenKeyDescriptionLengthIsTooLong()
        {
            Api.JetCreateIndex(this.sesid, this.tableid, "BadIndex,", CreateIndexGrbit.None, "+foo\0", 77, 100);
        }

        /// <summary>
        /// Check that an exception is thrown when JetCreateIndex2 gets 
        /// null indexcreates.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetCreateIndex2ThrowsExceptionWhenIndexcreatesAreNull()
        {
            Api.JetCreateIndex2(this.sesid, this.tableid, null, 0);
        }

        /// <summary>
        /// Check that an exception is thrown when JetCreateIndex2 gets 
        /// a negative indexcreate count.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetCreateIndex2ThrowsExceptionWhenNumIndexcreatesIsNegative()
        {
            var indexcreates = new[] { new JET_INDEXCREATE() };
            Api.JetCreateIndex2(this.sesid, this.tableid, indexcreates, -1);
        }

        /// <summary>
        /// Check that an exception is thrown when JetCreateIndex2 gets 
        /// an indexcreate count that is too long.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetCreateIndex2ThrowsExceptionWhenNumIndexcreatesIsTooLong()
        {
            var indexcreates = new[] { new JET_INDEXCREATE() };
            Api.JetCreateIndex2(this.sesid, this.tableid, indexcreates, indexcreates.Length + 1);
        }

        /// <summary>
        /// Check that an exception is thrown when JetDeleteColumn gets a 
        /// null column name.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetDeleteColumnThrowsExceptionWhenColumnNameIsNull()
        {
            Api.JetDeleteColumn(this.sesid, this.tableid, null);
        }

        /// <summary>
        /// Check that an exception is thrown when JetDeleteIndex gets a 
        /// null index name.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetDeleteIndexThrowsExceptionWhenIndexNameIsNull()
        {
            Api.JetDeleteIndex(this.sesid, this.tableid, null);
        }

        /// <summary>
        /// Check that an exception is thrown when JetDeleteTable gets a 
        /// null table name.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetDeleteTableThrowsExceptionWhenTableNameIsNull()
        {
            Api.JetDeleteTable(this.sesid, this.dbid, null);
        }

        /// <summary>
        /// Check that an exception is thrown when JetGetTableColumnInfo gets a 
        /// null column name.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetGetTableColumnInfoThrowsExceptionWhenColumnNameIsNull()
        {
            JET_COLUMNDEF columndef;
            Api.JetGetTableColumnInfo(this.sesid, this.tableid, null, out columndef);
        }

        /// <summary>
        /// Check that an exception is thrown when JetGetColumnInfo gets a 
        /// null table name.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetGetColumnInfoThrowsExceptionWhenTableNameIsNull()
        {
            JET_COLUMNDEF columndef;
            Api.JetGetColumnInfo(this.sesid, this.dbid, null, "column", out columndef);
        }

        /// <summary>
        /// Check that an exception is thrown when JetGetColumnInfo gets a 
        /// null column name.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetGetColumnInfoThrowsExceptionWhenColumnNameIsNull()
        {
            JET_COLUMNDEF columndef;
            Api.JetGetColumnInfo(this.sesid, this.dbid, "table", null, out columndef);
        }

        /// <summary>
        /// Check that an exception is thrown when JetGetColumnInfo gets a 
        /// null table name.
        /// </summary>
        /// <remarks>
        /// This tests the version of the API that takes a JET_COLUMNLIST.
        /// </remarks>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetGetColumnInfoThrowsExceptionWhenTableNameIsNull2()
        {
            JET_COLUMNLIST columnlist;
            Api.JetGetColumnInfo(this.sesid, this.dbid, null, null, out columnlist);
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Check that an exception is thrown when JetGotoBookmark gets a 
        /// null bookmark.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetGotoBookmarkThrowsExceptionWhenBookmarkIsNull()
        {
            Api.JetGotoBookmark(this.sesid, this.tableid, null, 0);
        }

        /// <summary>
        /// Check that an exception is thrown when JetGotoBookmark gets a 
        /// negative bookmark length.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetGotoBookmarkThrowsExceptionWhenBookmarkLengthIsNegative()
        {
            var bookmark = new byte[1];
            Api.JetGotoBookmark(this.sesid, this.tableid, bookmark, -1);
        }

        /// <summary>
        /// Check that an exception is thrown when JetGotoBookmark gets a 
        /// bookmark length that is too long.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetGotoBookmarkThrowsExceptionWhenBookmarkLengthIsTooLong()
        {
            var bookmark = new byte[1];
            Api.JetGotoBookmark(this.sesid, this.tableid, bookmark, bookmark.Length + 1);
        }

        /// <summary>
        /// Check that an exception is thrown when JetMakeKey gets 
        /// null data and a non-null length.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetMakeKeyThrowsExceptionWhenDataIsNull()
        {
            Api.JetMakeKey(this.sesid, this.tableid, null, 2, MakeKeyGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetMakeKey gets a 
        /// data length that is negative.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetMakeKeyThrowsExceptionWhenDataLengthIsNegative()
        {
            var data = new byte[1];
            Api.JetMakeKey(this.sesid, this.tableid, data, -1, MakeKeyGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetMakeKey gets a 
        /// data length that is too long.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetMakeKeyThrowsExceptionWhenDataLengthIsTooLong()
        {
            var data = new byte[1];
            Api.JetMakeKey(this.sesid, this.tableid, data, data.Length + 1, MakeKeyGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetIndexRecordCount gets a 
        /// negative max record count.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetIndexRecordCountThrowsExceptionWhenMaxRecordsIsNegative()
        {
            int numRecords;
            Api.JetIndexRecordCount(this.sesid, this.tableid, out numRecords, -1);
        }

        /// <summary>
        /// Check that an exception is thrown when passing in NULL as the 
        /// ranges to JetIntersectIndexes.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetIntersectIndexesThrowsExceptionWhenTableidsIsNull()
        {
            JET_RECORDLIST recordlist;
            Api.JetIntersectIndexes(this.sesid, null, 0, out recordlist, IntersectIndexesGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when intersecting just one index.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetIntersectIndexesThrowsExceptionWhenIntersectingOneTableid()
        {
            var ranges = new JET_INDEXRANGE[1];
            ranges[0] = new JET_INDEXRANGE { tableid = this.tableid };

            JET_RECORDLIST recordlist;
            Api.JetIntersectIndexes(this.sesid, ranges, 1, out recordlist, IntersectIndexesGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when IntersectIndexes gets null
        /// as the tableid argument.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IntersectIndexesThrowsExceptionWhenTableidIsNull()
        {
            Api.IntersectIndexes(this.sesid, null).ToArray();
        }

        #endregion

        #region Data Retrieval

        /// <summary>
        /// Check that an exception is thrown when JetGetBookmark gets a 
        /// null bookmark and non-null length.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetGetBookmarkThrowsExceptionWhenBookmarkIsNull()
        {
            int actualSize;
            Api.JetGetBookmark(this.sesid, this.tableid, null, 10, out actualSize);
        }

        /// <summary>
        /// Check that an exception is thrown when JetGetBookmark gets a 
        /// bookmark length that is negative.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetGetBookmarkThrowsExceptionWhenBookmarkLengthIsNegative()
        {
            int actualSize;
            var bookmark = new byte[1];
            Api.JetGetBookmark(this.sesid, this.tableid, bookmark, -1, out actualSize);
        }

        /// <summary>
        /// Check that an exception is thrown when JetGetBookmark gets a 
        /// bookmark length that is too long.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetGetBookmarkThrowsExceptionWhenBookmarkLengthIsTooLong()
        {
            int actualSize;
            var bookmark = new byte[1];
            Api.JetGetBookmark(this.sesid, this.tableid, bookmark, bookmark.Length + 1, out actualSize);
        }

        /// <summary>
        /// Check that an exception is thrown when JetRetrieveKey gets 
        /// null data and a non-null length.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetRetrieveKeyThrowsExceptionWhenDataIsNull()
        {
            int actualSize;
            Api.JetRetrieveKey(this.sesid, this.tableid, null, 1, out actualSize, RetrieveKeyGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetMakeKey gets a 
        /// data length that is negative.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetRetrieveKeyThrowsExceptionWhenDataLengthIsNegative()
        {
            var data = new byte[1];
            int actualSize;
            Api.JetRetrieveKey(this.sesid, this.tableid, data, -1, out actualSize, RetrieveKeyGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetMakeKey gets a 
        /// data length that is too long.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetRetrieveKeyThrowsExceptionWhenDataLengthIsTooLong()
        {
            var data = new byte[1];
            int actualSize;
            Api.JetRetrieveKey(this.sesid, this.tableid, data, data.Length + 1, out actualSize, RetrieveKeyGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetRetrieveColumn gets a 
        /// null buffer and non-null length.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetRetrieveColumnThrowsExceptionWhenDataIsNull()
        {
            int actualSize;
            Api.JetRetrieveColumn(this.sesid, this.tableid, this.columnid, null, 1, out actualSize, RetrieveColumnGrbit.None, null);
        }

        /// <summary>
        /// Check that an exception is thrown when JetRetrieveColumn gets a 
        /// data length that is negative.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetRetrieveColumnThrowsExceptionWhenDataSizeIsNegative()
        {
            int actualSize;
            var data = new byte[1];
            Api.JetRetrieveColumn(this.sesid, this.tableid, this.columnid, data, -1, out actualSize, RetrieveColumnGrbit.None, null);
        }

        /// <summary>
        /// Check that an exception is thrown when JetRetrieveColumn gets a 
        /// data length that is too long.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetRetrieveColumnThrowsExceptionWhenDataSizeIsTooLong()
        {
            int actualSize;
            var data = new byte[1];
            Api.JetRetrieveColumn(this.sesid, this.tableid, this.columnid, data, data.Length + 1, out actualSize, RetrieveColumnGrbit.None, null);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEnumerateColumns gets a 
        /// null allocator callback.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetEnumerateColumnsThrowsExceptionWhenAllocatorIsNull()
        {
            int numColumnValues;
            JET_ENUMCOLUMN[] columnValues;
            Api.JetEnumerateColumns(
                this.sesid,
                this.tableid,
                0,
                null,
                out numColumnValues,
                out columnValues,
                null,
                IntPtr.Zero,
                0,
                EnumerateColumnsGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEnumerateColumns gets a 
        /// negative maximum column size.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetEnumerateColumnsThrowsExceptionWhenMaxSizeIsNegative()
        {
            int numColumnValues;
            JET_ENUMCOLUMN[] columnValues;
            JET_PFNREALLOC allocator = (context, pv, cb) => IntPtr.Zero;
            Api.JetEnumerateColumns(
                this.sesid,
                this.tableid,
                0,
                null,
                out numColumnValues,
                out columnValues,
                allocator,
                IntPtr.Zero,
                -1,
                EnumerateColumnsGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEnumerateColumns gets a 
        /// null columnids when numColumnids is non-zero.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetEnumerateColumnsThrowsExceptionWhenColumnidsIsNull()
        {
            int numColumnValues;
            JET_ENUMCOLUMN[] columnValues;
            JET_PFNREALLOC allocator = (context, pv, cb) => IntPtr.Zero;
            Api.JetEnumerateColumns(
                this.sesid,
                this.tableid,
                1,
                null,
                out numColumnValues,
                out columnValues,
                allocator,
                IntPtr.Zero,
                0,
                EnumerateColumnsGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEnumerateColumns gets a 
        /// negative numColumnids.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetEnumerateColumnsThrowsExceptionWhenNumColumnidsIsNegative()
        {
            int numColumnValues;
            JET_ENUMCOLUMN[] columnValues;
            JET_PFNREALLOC allocator = (context, pv, cb) => IntPtr.Zero;
            var columnids = new JET_ENUMCOLUMNID[2];
            Api.JetEnumerateColumns(
                this.sesid,
                this.tableid,
                -1,
                columnids,
                out numColumnValues,
                out columnValues,
                allocator,
                IntPtr.Zero,
                0,
                EnumerateColumnsGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEnumerateColumns gets a 
        /// numColumnids count which is greater than the size of columnids.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetEnumerateColumnsThrowsExceptionWhenNumColumnidsIsTooLong()
        {
            int numColumnValues;
            JET_ENUMCOLUMN[] columnValues;
            JET_PFNREALLOC allocator = (context, pv, cb) => IntPtr.Zero;
            var columnids = new JET_ENUMCOLUMNID[2];
            Api.JetEnumerateColumns(
                this.sesid,
                this.tableid,
                columnids.Length + 1,
                columnids,
                out numColumnValues,
                out columnValues,
                allocator,
                IntPtr.Zero,
                0,
                EnumerateColumnsGrbit.None);
        }

        #endregion

        #region DML

        /// <summary>
        /// Check that an exception is thrown when JetSetColumn gets a 
        /// null buffer and non-null length (and SetSizeLV isn't specified).
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetSetColumnThrowsExceptionWhenDataIsNull()
        {
            Api.JetSetColumn(this.sesid, this.tableid, this.columnid, null, 1, SetColumnGrbit.None, null);
        }

        /// <summary>
        /// Check that an exception is thrown when JetSetColumn gets a 
        /// negative data length.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetSetColumnThrowsExceptionWhenDataSizeIsNegative()
        {
            var data = new byte[1];
            Api.JetSetColumn(this.sesid, this.tableid, this.columnid, data, -1, SetColumnGrbit.None, null);
        }

        /// <summary>
        /// Check that an exception is thrown when JetSetColumn gets a 
        /// negative data length.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetSetColumnThrowsExceptionWhenDataSizeIsTooLong()
        {
            var data = new byte[1];
            Api.JetSetColumn(this.sesid, this.tableid, this.columnid, data, data.Length + 1, SetColumnGrbit.None, null);
        }

        /// <summary>
        /// Check that an exception is thrown when JetSetColumns gets a 
        /// null setcolumns array. 
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetSetColumnsThrowsExceptionWhenSetColumnsIsNull()
        {
            Api.JetSetColumns(this.sesid, this.tableid, null, 0);
        }

        /// <summary>
        /// Check that an exception is thrown when JetSetColumns gets a 
        /// negative number of columns.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetSetColumnsThrowsExceptionWhenNumColumnsIsNegative()
        {
            Api.JetSetColumns(this.sesid, this.tableid, new JET_SETCOLUMN[1], -1);
        }

        /// <summary>
        /// Check that an exception is thrown when JetSetColumns gets a 
        /// numColumns count that is greater than the number of columns.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetSetColumnsThrowsExceptionWhenDataSizeIsTooLong()
        {
            Api.JetSetColumns(this.sesid, this.tableid, new JET_SETCOLUMN[1], 2);
        }

        /// <summary>
        /// Check that an exception is thrown when JetSetColumns gets a 
        /// numColumns count that is greater than the number of columns.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetSetColumnsThrowsExceptionWhenSetColumnDataIsInvalid()
        {
            var setcolumns = new[]
            {
                new JET_SETCOLUMN
                {
                    cbData = 100,
                    pvData = new byte[10],
                },
            };
            Api.JetSetColumns(this.sesid, this.tableid, setcolumns, setcolumns.Length);
        }

        /// <summary>
        /// Check that an exception is thrown when JetUpdate gets a 
        /// null buffer and non-null length.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetUpdateThrowsExceptionWhenDataIsNull()
        {
            int actualSize;
            Api.JetUpdate(this.sesid, this.tableid, null, 1, out actualSize);
        }

        /// <summary>
        /// Check that an exception is thrown when JetUpdate gets a 
        /// data length that is negative.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetUpdateThrowsExceptionWhenDataSizeIsNegative()
        {
            int actualSize;
            var data = new byte[1];
            Api.JetUpdate(this.sesid, this.tableid, data, -1, out actualSize);
        }

        /// <summary>
        /// Check that an exception is thrown when JetUpdate gets a 
        /// data length that is too long.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetUpdateThrowsExceptionWhenDataSizeIsTooLong()
        {
            int actualSize;
            var data = new byte[1];
            Api.JetUpdate(this.sesid, this.tableid, data, data.Length + 1, out actualSize);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEscrowUpdate gets a 
        /// null delta.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JetEscrowUpdateThrowsExceptionWhenDeltaIsNull()
        {
            int actualSize;
            Api.JetEscrowUpdate(this.sesid, this.tableid, this.columnid, null, 0, null, 0, out actualSize, EscrowUpdateGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEscrowUpdate gets a 
        /// negative delta length.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetEscrowUpdateThrowsExceptionWhenDeltaSizeIsNegative()
        {
            int actualSize;
            var delta = new byte[4];
            Api.JetEscrowUpdate(this.sesid, this.tableid, this.columnid, delta, -1, null, 0, out actualSize, EscrowUpdateGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEscrowUpdate gets a 
        /// delta length that is too long.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetEscrowUpdateThrowsExceptionWhenDeltaSizeIsTooLong()
        {
            int actualSize;
            var delta = new byte[1];
            Api.JetEscrowUpdate(this.sesid, this.tableid, this.columnid, delta, delta.Length + 1, null, 0, out actualSize, EscrowUpdateGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEscrowUpdate gets a 
        /// null previous value and non-zero length.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetEscrowUpdateThrowsExceptionWhenPreviousValueIsNull()
        {
            int actualSize;
            var delta = new byte[4];
            Api.JetEscrowUpdate(this.sesid, this.tableid, this.columnid, delta, delta.Length, null, 4, out actualSize, EscrowUpdateGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEscrowUpdate gets a 
        /// negative previous value length.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetEscrowUpdateThrowsExceptionWhenPreviousValueSizeIsNegative()
        {
            int actualSize;
            var delta = new byte[4];
            var previous = new byte[4];
            Api.JetEscrowUpdate(this.sesid, this.tableid, this.columnid, delta, delta.Length, previous, -1, out actualSize, EscrowUpdateGrbit.None);
        }

        /// <summary>
        /// Check that an exception is thrown when JetEscrowUpdate gets a 
        /// previous value length that is too long.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void JetEscrowUpdateThrowsExceptionWhenPreviousValueSizeIsTooLong()
        {
            int actualSize;
            var delta = new byte[4];
            var previous = new byte[4];
            Api.JetEscrowUpdate(this.sesid, this.tableid, this.columnid, delta, delta.Length, previous, previous.Length + 1, out actualSize, EscrowUpdateGrbit.None);
        }

        #endregion DML
    }
}
