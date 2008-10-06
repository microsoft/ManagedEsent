//-----------------------------------------------------------------------
// <copyright file="BasicTableTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.IO;
    using System.Text;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Basic API tests
    /// </summary>
    [TestClass]
    public class BasicTableTests
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
        /// Columnid of the LongText column in the table.
        /// </summary>
        private JET_COLUMNID columnidLongText;

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

            API.JetInit(ref this.instance);
            API.JetBeginSession(this.instance, out this.sesid, String.Empty, String.Empty);
            API.JetCreateDatabase(this.sesid, this.database, String.Empty, out this.dbid, CreateDatabaseGrbit.None);
            API.JetBeginTransaction(this.sesid);
            API.JetCreateTable(this.sesid, this.dbid, this.table, 0, 100, out this.tableid);

            var columndef = new JET_COLUMNDEF()
            {
                cp = JET_CP.Unicode,
                coltyp = JET_coltyp.LongText,
            };
            API.JetAddColumn(this.sesid, this.tableid, "LongText", columndef, null, 0, out this.columnidLongText);

            API.JetCloseTable(this.sesid, this.tableid);
            API.JetCommitTransaction(this.sesid, CommitTransactionGrbit.LazyFlush);
            API.JetOpenTable(this.sesid, this.dbid, this.table, null, 0, OpenTableGrbit.None, out this.tableid);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            API.JetCloseTable(this.sesid, this.tableid);
            API.JetEndSession(this.sesid, EndSessionGrbit.None);
            API.JetTerm(this.instance);
            Directory.Delete(this.directory, true);
        }

        /// <summary>
        /// Inserts a record and retrieve it.
        /// </summary>
        [TestMethod]
        public void InsertRecordTest()
        {
            string s = "a test string";

            API.JetBeginTransaction(this.sesid);
            API.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            this.SetColumnFromString(s);
            this.UpdateAndGotoBookmark();
            Assert.AreEqual(s, this.RetrieveColumnAsString());
        }

        /// <summary>
        /// Test setting and retrieving a column with the ColumnStream class.
        /// </summary>
        [TestMethod]
        public void ColumnStreamTest()
        {
            string s = "the string to be inserted";

            API.JetBeginTransaction(this.sesid);
            API.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            using (var writer = new StreamWriter(new ColumnStream(this.sesid, this.tableid, this.columnidLongText)))
            {
                writer.WriteLine(s);
            }

            this.UpdateAndGotoBookmark();

            using (var reader = new StreamReader(new ColumnStream(this.sesid, this.tableid, this.columnidLongText)))
            {
                string actual = reader.ReadLine();
                Assert.AreEqual(s, actual);
            }
        }

        /// <summary>
        /// Insert a record and delete it.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(EsentException))]
        public void InsertAndDeleteTest()
        {
            API.JetBeginTransaction(this.sesid);
            API.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Insert);
            this.UpdateAndGotoBookmark();
            API.JetDelete(this.sesid, this.tableid);
            this.RetrieveColumnAsString();
        }

        /// <summary>
        /// Update the cursor and goto the returned bookmark.
        /// </summary>
        private void UpdateAndGotoBookmark()
        {
            byte[] bookmark = new byte[256];
            int bookmarkSize;
            API.JetUpdate(this.sesid, this.tableid, bookmark, bookmark.Length, out bookmarkSize);
            API.JetGotoBookmark(this.sesid, this.tableid, bookmark, bookmarkSize);
        }

        /// <summary>
        /// Sets the LongText column in the table from a string. An update must be prepared.
        /// </summary>
        /// <param name="s">The string to set.</param>
        private void SetColumnFromString(string s)
        {
            byte[] data = Encoding.Unicode.GetBytes(s);
            API.JetSetColumn(this.sesid, this.tableid, this.columnidLongText, data, data.Length, SetColumnGrbit.None, null);
        }

        /// <summary>
        /// Returns the value in the LongText column as a string. The cursor must be on a record.
        /// </summary>
        /// <returns>The value of the LongText column as a string.</returns>
        private string RetrieveColumnAsString()
        {
            int retrievedSize;
            API.JetRetrieveColumn(this.sesid, this.tableid, this.columnidLongText, null, 0, out retrievedSize, RetrieveColumnGrbit.None, null);
            var buffer = new byte[retrievedSize];
            API.JetRetrieveColumn(this.sesid, this.tableid, this.columnidLongText, buffer, buffer.Length, out retrievedSize, RetrieveColumnGrbit.None, null);
            return Encoding.Unicode.GetString(buffer, 0, retrievedSize);
        }
    }
}
