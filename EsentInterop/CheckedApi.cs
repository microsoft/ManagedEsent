//-----------------------------------------------------------------------
// <copyright file="CheckedApi.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------
// The Esent.Interop namespace will be developed with these principles:
//  -   Full and complete documentation. Intellisense should be able to
//      provide useful and extensive help.
//  -   Minimal editorialization. Whenever possible the Esent.Interop API will
//      exactly match the ESENT API. In particular the names of structs, types
//      and functions will not be changed. Except for:
//  -   Cleaning up API constants. Instead of providing the constants from
//      esent.h they will be grouped into useful enumerations. This will
//      eliminate a lot of common API errors.
//  Changes that will be made are:
//  -   Convert JET_coltyp etc. into real enumerations
//  -   Removing cbStruct from structures
//  -   Removing unused/reserved entries from structures
//  -   Throwing exceptions instead of returning errors
//          (Need to work out what to do with APIs where errors are common)
//  The API has three layers:
//  -   API (public): this layer provides error-handling, turning errors
//      returned by lower layers into exceptions.
//  -   UNCHECKED_API (internal): this layer turns managed objects into
//      objects which can be passed into the P/Invoke interop layer.
//      Methods at this level return JET_ERR instead of throwing an exception.
//  -   NativeMethods: this is the P/Invoke interop layer. This layer deals
//      with IntPtr and other basic types as opposed to the managed types
//      such as JET_TABLEID.

namespace Esent.Interop
{
    /// <summary>
    /// Managed versions of the ESENT API. This class contains static methods corresponding
    /// with the unmanaged ESENT API. These methods throw exceptions when errors are returned.
    /// </summary>
    public static class API
    {
        #region init/term

        /// <summary>
        /// Allocates a new instance of the database engine.
        /// </summary>
        /// <param name="instance">Returns the new instance.</param>
        /// <param name="name">The name of the instance. Names must be unique.</param>
        public static void JetCreateInstance(out JET_INSTANCE instance, string name)
        {
            Check(UncheckedApi.JetCreateInstance(out instance, name));
        }

        /// <summary>
        /// Initialize the ESENT database engine.
        /// </summary>
        /// <param name="instance">
        /// The instance to initialize. If an instance hasn't been
        /// allocated then a new one is created and the engine
        /// will operate in single-instance mode.
        /// </param>
        /// <remarks>
        /// For Windows 2000 only single-instance mode is supported.
        /// </remarks>
        public static void JetInit(ref JET_INSTANCE instance)
        {
            Check(UncheckedApi.JetInit(ref instance));
        }

        /// <summary>
        /// Terminate an instance that was created with JetInit or
        /// JetCreateInstance.
        /// </summary>
        /// <param name="instance">The instance to terminate.</param>
        public static void JetTerm(JET_INSTANCE instance)
        {
            Check(UncheckedApi.JetTerm(instance));
        }

        #endregion

        #region sessions

        /// <summary>
        /// Initialize a new ESENT session.
        /// </summary>
        /// <param name="instance">The initialized instance to create the session in.</param>
        /// <param name="sesid">Returns the created session.</param>
        /// <param name="username">The parameter is not used.</param>
        /// <param name="password">The parameter is not used.</param>
        public static void JetBeginSession(JET_INSTANCE instance, out JET_SESID sesid, string username, string password)
        {
            Check(UncheckedApi.JetBeginSession(instance, out sesid, username, password));
        }

        /// <summary>
        /// Ends a session.
        /// </summary>
        /// <param name="sesid">The session to end.</param>
        /// <param name="grbit">The parameter is not used.</param>
        public static void JetEndSession(JET_SESID sesid, EndSessionGrbit grbit)
        {
            Check(UncheckedApi.JetEndSession(sesid, grbit));
        }

        /// <summary>
        /// Sets database configuration options.
        /// </summary>
        /// <param name="instance">The instance to set the option on or JET_INSTANCE.Nil to set the option on all instances.</param>
        /// <param name="sesid">The session to use.</param>
        /// <param name="paramid">The parameter to set.</param>
        /// <param name="paramValue">The value of the parameter to set, if the parameter is an integer type.</param>
        /// <param name="paramString">The value of the parameter to set, if the parameter is a string type.</param>
        public static void JetSetSystemParameter(JET_INSTANCE instance, JET_SESID sesid, JET_param paramid, int paramValue, string paramString)
        {
            Check(UncheckedApi.JetSetSystemParameter(instance, sesid, paramid, paramValue, paramString));
        }
       
        /// <summary>
        /// Gets database configuration options.
        /// </summary>
        /// <param name="instance">The instance to retrieve the options from.</param>
        /// <param name="sesid">The session to use.</param>
        /// <param name="paramid">The parameter to get.</param>
        /// <param name="paramValue">Returns the value of the parameter, if the value is an integer.</param>
        /// <param name="paramString">Returns the value of the parameter, if the value is a string.</param>
        /// <param name="maxParam">The maximum size of the parameter string.</param>
        public static void JetGetSystemParameter(JET_INSTANCE instance, JET_SESID sesid, JET_param paramid, out int paramValue, out string paramString, int maxParam)
        {
            Check(UncheckedApi.JetGetSystemParameter(instance, sesid, paramid, out paramValue, out paramString, maxParam));
        }

        #endregion

        #region tables

        /// <summary>
        /// Opens a cursor on a previously created table.
        /// </summary>
        /// <param name="sesid">The database session to use.</param>
        /// <param name="dbid">The database to open the table in.</param>
        /// <param name="tablename">The name of the table to open.</param>
        /// <param name="parameters">The parameter is not used.</param>
        /// <param name="parametersSize">The parameter is not used.</param>
        /// <param name="grbit">Table open options.</param>
        /// <param name="tableid">Returns the opened table.</param>
        public static void JetOpenTable(JET_SESID sesid, JET_DBID dbid, string tablename, byte[] parameters, int parametersSize, OpenTableGrbit grbit, out JET_TABLEID tableid)
        {
            Check(UncheckedApi.JetOpenTable(sesid, dbid, tablename, parameters, parametersSize, grbit, out tableid));
        }

        /// <summary>
        /// Close an open table.
        /// </summary>
        /// <param name="sesid">The session which opened the table.</param>
        /// <param name="tableid">The table to close.</param>
        public static void JetCloseTable(JET_SESID sesid, JET_TABLEID tableid)
        {
            Check(UncheckedApi.JetCloseTable(sesid, tableid));
        }

        #endregion

        #region Transactions

        /// <summary>
        /// Causes a session to enter a transaction or create a new save point in an existing
        /// transaction.
        /// </summary>
        /// <param name="sesid">The session to begin the transaction for.</param>
        public static void JetBeginTransaction(JET_SESID sesid)
        {
            Check(UncheckedApi.JetBeginTransaction(sesid));
        }

        /// <summary>
        /// commits the changes made to the state of the database during the current save point
        /// and migrates them to the previous save point. If the outermost save point is committed
        /// then the changes made during that save point will be committed to the state of the
        /// database and the session will exit the transaction.
        /// </summary>
        /// <param name="sesid">The session to commit the transaction for.</param>
        /// <param name="grbit">Commit options.</param>
        public static void JetCommitTransaction(JET_SESID sesid, CommitTransactionGrbit grbit)
        {
            Check(UncheckedApi.JetCommitTransaction(sesid, grbit));
        }

        /// <summary>
        /// The JetRollback function undoes the changes made to the state of the database
        /// and returns to the last save point. JetRollback will also close any cursors
        /// opened during the save point. If the outermost save point is undone, the
        /// session will exit the transaction.
        /// </summary>
        /// <param name="sesid">The session to rollback the transaction for.</param>
        /// <param name="grbit">Rollback options.</param>
        public static void JetRollback(JET_SESID sesid, RollbackTransactionGrbit grbit)
        {
            Check(UncheckedApi.JetRollback(sesid, grbit));
        }

        #endregion

        #region DDL

        /// <summary>
        /// Creates and attaches a database file.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="database">The path to the database file to create.</param>
        /// <param name="connect">The parameter is not used.</param>
        /// <param name="dbid">Returns the dbid of the new database.</param>
        /// <param name="grbit">Database creation options.</param>
        public static void JetCreateDatabase(JET_SESID sesid, string database, string connect, out JET_DBID dbid, CreateDatabaseGrbit grbit)
        {
            Check(UncheckedApi.JetCreateDatabase(sesid, database, connect, out dbid, grbit));
        }

        /// <summary>
        /// Create an empty table. The newly created table is opened exclusively.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database to create the table in.</param>
        /// <param name="table">The name of the table to create.</param>
        /// <param name="pages">Initial number of pages in the table.</param>
        /// <param name="density">
        /// The default density of the table. This is used when doing sequential inserts.
        /// </param>
        /// <param name="tableid">Returns the tableid of the new table.</param>
        public static void JetCreateTable(JET_SESID sesid, JET_DBID dbid, string table, int pages, int density, out JET_TABLEID tableid)
        {
            Check(UncheckedApi.JetCreateTable(sesid, dbid, table, pages, density, out tableid));
        }

        /// <summary>
        /// Add a new column to an existing table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to add the column to.</param>
        /// <param name="column">The name of the column.</param>
        /// <param name="columndef">The definition of the column.</param>
        /// <param name="defaultValue">The default value of the column.</param>
        /// <param name="defaultValueSize">The size of the default value.</param>
        /// <param name="columnid">Returns the columnid of the new column.</param>
        public static void JetAddColumn(JET_SESID sesid, JET_TABLEID tableid, string column, JET_COLUMNDEF columndef, byte[] defaultValue, int defaultValueSize, out JET_COLUMNID columnid)
        {
            Check(UncheckedApi.JetAddColumn(sesid, tableid, column, columndef, defaultValue, defaultValueSize, out columnid));
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Retrieves the bookmark for the record that is associated with the index entry
        /// at the current position of a cursor. This bookmark can then be used to
        /// reposition that cursor back to the same record using JetGotoBookmark. 
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the bookmark from.</param>
        /// <param name="bookmark">Buffer to contain the bookmark.</param>
        /// <param name="bookmarkSize">Size of the bookmark buffer.</param>
        /// <param name="actualBookmarkSize">Returns the actual size of the bookmark.</param>
        public static void JetGetBookmark(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize, out int actualBookmarkSize)
        {
            Check(UncheckedApi.JetGetBookmark(sesid, tableid, bookmark, bookmarkSize, out actualBookmarkSize));
        }

        /// <summary>
        /// Retrieves the bookmark for the record that is associated with the index entry
        /// at the current position of a cursor. This bookmark can then be used to
        /// reposition that cursor back to the same record using JetGotoBookmark. 
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the bookmark from.</param>
        /// <param name="bookmark">Buffer to contain the bookmark.</param>
        /// <param name="bookmarkSize">Size of the bookmark buffer.</param>
        public static void JetGetBookmark(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize)
        {
            int actualBookmarkSize;
            Check(UncheckedApi.JetGetBookmark(sesid, tableid, bookmark, bookmarkSize, out actualBookmarkSize));
        }

        /// <summary>
        /// Positions a cursor to an index entry for the record that is associated with
        /// the specified bookmark. The bookmark can be used with any index defined over
        /// a table. The bookmark for a record can be retrieved using JetGetBookmark. 
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to position.</param>
        /// <param name="bookmark">The bookmark used to position the cursor.</param>
        /// <param name="bookmarkSize">The size of the bookmark.</param>
        public static void JetGotoBookmark(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize)
        {
            Check(UncheckedApi.JetGotoBookmark(sesid, tableid, bookmark, bookmarkSize));
        }

        /// <summary>
        /// Retrieves a single column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// Alternatively, this function can retrieve a column from a record being created
        /// in the cursor copy buffer. This function can also retrieve column data from an
        /// index entry that references the current record. In addition to retrieving the
        /// actual column value, JetRetrieveColumn can also be used to retrieve the size
        /// of a column, before retrieving the column data itself so that application
        /// buffers can be sized appropriately.  
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="data">The data buffer to be retrieved into.</param>
        /// <param name="dataSize">The size of the data buffer.</param>
        /// <param name="actualDataSize">Returns the actual size of the data buffer.</param>
        /// <param name="grbit">Retrieve column options.</param>
        /// <param name="retinfo">
        /// If pretinfo is give as NULL then the function behaves as though an itagSequence
        /// of 1 and an ibLongValue of 0 (zero) were given. This causes column retrieval to
        /// retrieve the first value of a multi-valued column, and to retrieve long data at
        /// offset 0 (zero).
        /// </param>
        public static void JetRetrieveColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, byte[] data, int dataSize, out int actualDataSize, RetrieveColumnGrbit grbit, JET_RETINFO retinfo)
        {
            Check(UncheckedApi.JetRetrieveColumn(sesid, tableid, columnid, data, dataSize, out actualDataSize, grbit, retinfo));
        }

        /// <summary>
        /// Navigate through an index. The cursor can be positioned at the start or
        /// end of the index and moved backwards and forwards by a specified number
        /// of index entries.
        /// </summary>
        /// <param name="sesid">The session to use for the call.</param>
        /// <param name="tableid">The cursor to position.</param>
        /// <param name="numRows">An offset which indicates how far to move the cursor.</param>
        /// <param name="grbit">Move options.</param>
        public static void JetMove(JET_SESID sesid, JET_TABLEID tableid, int numRows, MoveGrbit grbit)
        {
            Check(UncheckedApi.JetMove(sesid, tableid, numRows, grbit));
        }

        /// <summary>
        /// Navigate through an index. The cursor can be positioned at the start or
        /// end of the index and moved backwards and forwards by a specified number
        /// of index entries.
        /// </summary>
        /// <param name="sesid">The session to use for the call.</param>
        /// <param name="tableid">The cursor to position.</param>
        /// <param name="numRows">An offset which indicates how far to move the cursor.</param>
        /// <param name="grbit">Move options.</param>
        public static void JetMove(JET_SESID sesid, JET_TABLEID tableid, JET_Move numRows, MoveGrbit grbit)
        {
            Check(UncheckedApi.JetMove(sesid, tableid, (int)numRows, grbit));
        }

        #endregion

        #region DML

        /// <summary>
        /// Deletes the current record in a database table.
        /// </summary>
        /// <param name="sesid">The session that opened the cursor.</param>
        /// <param name="tableid">The cursor on a database table. The current row will be deleted.</param>
        public static void JetDelete(JET_SESID sesid, JET_TABLEID tableid)
        {
            Check(UncheckedApi.JetDelete(sesid, tableid));
        }

        /// <summary>
        /// Prepare a cursor for update.
        /// </summary>
        /// <param name="sesid">The session which is starting the update.</param>
        /// <param name="tableid">The cursor to start the update for.</param>
        /// <param name="prep">The type of update to prepare.</param>
        public static void JetPrepareUpdate(JET_SESID sesid, JET_TABLEID tableid, JET_prep prep)
        {
            Check(UncheckedApi.JetPrepareUpdate(sesid, tableid, prep));
        }

        /// <summary>
        /// The JetUpdate function performs an update operation including inserting a new row into
        /// a table or updating an existing row. Deleting a table row is performed by calling JetDelete.
        /// </summary>
        /// <param name="sesid">The session which started the update.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="bookmark">Returns the bookmark of the updated record. This can be null.</param>
        /// <param name="bookmarkSize">The size of the bookmark buffer.</param>
        /// <param name="actualBookmarkSize">Returns the actual size of the bookmark.</param>
        /// <remarks>
        /// JetUpdate is the final step in performing an insert or an update. The update is begun by
        /// calling JetPrepareUpdate and then by calling JetSetColumn or JetSetColumns one or more times
        /// to set the record state. Finally, JetUpdate is called to complete the update operation.
        /// Indexes are updated only by JetUpdate or JetUpdate2, and not during JetSetColumn or JetSetColumns
        /// </remarks>
        public static void JetUpdate(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize, out int actualBookmarkSize)
        {
            Check(UncheckedApi.JetUpdate(sesid, tableid, bookmark, bookmarkSize, out actualBookmarkSize));
        }

        /// <summary>
        /// The JetSetColumn function modifies a single column value in a modified record to be inserted or to
        /// update the current record. It can overwrite an existing value, add a new value to a sequence of
        /// values in a multi-valued column, remove a value from a sequence of values in a multi-valued column,
        /// or update all or part of a long value, a column of type JET_coltyp.LongText or JET_coltyp.LongBinary. 
        /// </summary>
        /// <param name="sesid">The session which is performing the update.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        /// <param name="dataSize">The size of data to set.</param>
        /// <param name="grbit">SetColumn options.</param>
        /// <param name="setinfo">Used to specify itag or long-value offset.</param>
        public static void JetSetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, byte[] data, int dataSize, SetColumnGrbit grbit, JET_SETINFO setinfo)
        {
            Check(UncheckedApi.JetSetColumn(sesid, tableid, columnid, data, dataSize, grbit, setinfo));
        }

        #endregion

        /// <summary>
        /// Throw an exception if the parameter is an ESE error.
        /// </summary>
        /// <param name="err">The error code to check.</param>
        private static void Check(int err)
        {
            if (err < 0)
            {
                throw new EsentException(err);
            }
        }
    }
}
