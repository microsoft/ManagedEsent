//-----------------------------------------------------------------------
// <copyright file="CheckedApi.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------
// The Microsoft.Isam.Esent.Interop namespace will be developed with these principles:
//  -   Any program written with this Api should work with the ESENT.dll from either
//      Windows XP, Windows Server 2003, Windows Vista or Windows Server 2008.
//  -   The Esent.Interop DLL should only require version 2.0 of the .NET Framework.
//  -   Full and complete documentation. Intellisense should be able to
//      provide useful and extensive help.
//  -   Minimal editorialization. Whenever possible the Microsoft.Isam.Esent.Interop Jet* api will
//      exactly match the ESENT Api. In particular the names of structs, types
//      and functions will not be changed. Except for:
//  -   Cleaning up Api constants. Instead of providing the constants from
//      esent.h they will be grouped into useful enumerations. This will
//      eliminate a lot of common Api errors.
//  -   Provide helper methods/objects for common operations. These will be layered
//      on top of the ESENT Api.
//  Changes that will be made are:
//  -   Convert JET_coltyp etc. into real enumerations
//  -   Removing cbStruct from structures
//  -   Removing unused/reserved entries from structures
//  -   Throwing exceptions instead of returning errors
//  The Api has four layers:
//  -   NativeMethods (internal): this is the P/Invoke interop layer. This layer deals
//      with IntPtr and other basic types as opposed to the managed types
//      such as JET_TABLEID.
//  -   ErrApi (internal): this layer turns managed objects into
//      objects which can be passed into the P/Invoke interop layer.
//      Methods at this level return an error instead of throwing an exception.
//  -   Api (public): this layer provides error-handling, turning errors
//      returned by lower layers into exceptions and warnings.
//  -   Helper methods (public): this layer provides data conversion and
//      iteration for common API activities. These methods do not start
//      with 'Jet' but are implemented using the Jet methods.
//  -   Disposable objects (public): these disposable object automatically
//      release esent resources (instances, sessions, tables and transactions). 

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// Managed versions of the ESENT Api. This class contains static methods corresponding
    /// with the unmanaged ESENT Api. These methods throw exceptions when errors are returned.
    /// </summary>
    public static partial class Api
    {
        #region init/term

        /// <summary>
        /// Allocates a new instance of the database engine.
        /// </summary>
        /// <param name="instance">Returns the new instance.</param>
        /// <param name="name">The name of the instance. Names must be unique.</param>
        public static void JetCreateInstance(out JET_INSTANCE instance, string name)
        {
            Check(ErrApi.JetCreateInstance(out instance, name));
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
            Check(ErrApi.JetInit(ref instance));
        }

        /// <summary>
        /// Terminate an instance that was created with JetInit or
        /// JetCreateInstance.
        /// </summary>
        /// <param name="instance">The instance to terminate.</param>
        public static void JetTerm(JET_INSTANCE instance)
        {
            Check(ErrApi.JetTerm(instance));
        }

        /// <summary>
        /// Sets database configuration options.
        /// </summary>
        /// <param name="instance">The instance to set the option on or JET_INSTANCE.Nil to set the option on all instances.</param>
        /// <param name="sesid">The session to use.</param>
        /// <param name="paramid">The parameter to set.</param>
        /// <param name="paramValue">The value of the parameter to set, if the parameter is an integer type.</param>
        /// <param name="paramString">The value of the parameter to set, if the parameter is a string type.</param>
        /// <returns>An ESENT warning code.</returns>
        public static JET_wrn JetSetSystemParameter(JET_INSTANCE instance, JET_SESID sesid, JET_param paramid, int paramValue, string paramString)
        {
            return Check(ErrApi.JetSetSystemParameter(instance, sesid, paramid, paramValue, paramString));
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
        /// <returns>An ESENT warning code.</returns>
        /// <remarks>
        /// JET_param.ErrorToString passes in the error number in the paramValue, which is why it is
        /// a ref parameter and not an out parameter.
        /// </remarks>
        public static JET_wrn JetGetSystemParameter(JET_INSTANCE instance, JET_SESID sesid, JET_param paramid, ref int paramValue, out string paramString, int maxParam)
        {
            return Check(ErrApi.JetGetSystemParameter(instance, sesid, paramid, ref paramValue, out paramString, maxParam));
        }

        #endregion

        #region Databases

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
            Check(ErrApi.JetCreateDatabase(sesid, database, connect, out dbid, grbit));
        }

        /// <summary>
        /// Attaches a database file for use with a database instance. In order to use the
        /// database, it will need to be subsequently opened with JetOpenDatabase.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="database">The database to attach.</param>
        /// <param name="grbit">Attach options.</param>
        /// <returns>An ESENT warning code.</returns>
        public static JET_wrn JetAttachDatabase(JET_SESID sesid, string database, AttachDatabaseGrbit grbit)
        {
            return Check(ErrApi.JetAttachDatabase(sesid, database, grbit));
        }

        /// <summary>
        /// Opens a previously attached database,using the JetAttachDatabase function,
        /// for use with a database session. This function can be called multiple times
        /// for the same database.
        /// </summary>
        /// <param name="sesid">The session that is opening the database.</param>
        /// <param name="database">The database to open.</param>
        /// <param name="connect">Reserved for future use.</param>
        /// <param name="dbid">Returns the dbid of the attached database.</param>
        /// <param name="grbit">Open database options.</param>
        /// <returns>An ESENT warning code.</returns>
        public static JET_wrn JetOpenDatabase(JET_SESID sesid, string database, string connect, out JET_DBID dbid, OpenDatabaseGrbit grbit)
        {
            return Check(ErrApi.JetOpenDatabase(sesid, database, connect, out dbid, grbit));
        }

        /// <summary>
        /// Closes a database file that was previously opened with JetOpenDatabase or
        /// created with JetCreateDatabase.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database to close.</param>
        /// <param name="grbit">Close options.</param>
        public static void JetCloseDatabase(JET_SESID sesid, JET_DBID dbid, CloseDatabaseGrbit grbit)
        {
            Check(ErrApi.JetCloseDatabase(sesid, dbid, grbit));
        }

        /// <summary>
        /// Releases a database file that was previously attached to a database session.
        /// </summary>
        /// <param name="sesid">The database session to use.</param>
        /// <param name="database">The database to detach.</param>
        public static void JetDetachDatabase(JET_SESID sesid, string database)
        {
            Check(ErrApi.JetDetachDatabase(sesid, database));
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
            Check(ErrApi.JetBeginSession(instance, out sesid, username, password));
        }

        /// <summary>
        /// Ends a session.
        /// </summary>
        /// <param name="sesid">The session to end.</param>
        /// <param name="grbit">The parameter is not used.</param>
        public static void JetEndSession(JET_SESID sesid, EndSessionGrbit grbit)
        {
            Check(ErrApi.JetEndSession(sesid, grbit));
        }

        /// <summary>
        /// Initialize a new ESE session in the same instance as the given sesid.
        /// </summary>
        /// <param name="sesid">The session to duplicate.</param>
        /// <param name="newSesid">Returns the new session.</param>
        public static void JetDupSession(JET_SESID sesid, out JET_SESID newSesid)
        {
            Check(ErrApi.JetDupSession(sesid, out newSesid));
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
            Check(ErrApi.JetOpenTable(sesid, dbid, tablename, parameters, parametersSize, grbit, out tableid));
        }

        /// <summary>
        /// Close an open table.
        /// </summary>
        /// <param name="sesid">The session which opened the table.</param>
        /// <param name="tableid">The table to close.</param>
        public static void JetCloseTable(JET_SESID sesid, JET_TABLEID tableid)
        {
            Check(ErrApi.JetCloseTable(sesid, tableid));
        }

        /// <summary>
        /// Duplicates an open cursor and returns a handle to the duplicated cursor.
        /// If the cursor that was duplicated was a read-only cursor then the
        /// duplicated cursor is also a read-only cursor.
        /// Any state related to constructing a search key or updating a record is
        /// not copied into the duplicated cursor. In addition, the location of the
        /// original cursor is not duplicated into the duplicated cursor. The
        /// duplicated cursor is always opened on the clustered index and its
        /// location is always on the first row of the table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to duplicate.</param>
        /// <param name="newTableid">The duplicated cursor.</param>
        /// <param name="grbit">Reserved for future use.</param>
        public static void JetDupCursor(JET_SESID sesid, JET_TABLEID tableid, out JET_TABLEID newTableid, DupCursorGrbit grbit)
        {
            Check(ErrApi.JetDupCursor(sesid, tableid, out newTableid, grbit));
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
            Check(ErrApi.JetBeginTransaction(sesid));
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
            Check(ErrApi.JetCommitTransaction(sesid, grbit));
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
            Check(ErrApi.JetRollback(sesid, grbit));
        }

        #endregion

        #region DDL

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
            Check(ErrApi.JetCreateTable(sesid, dbid, table, pages, density, out tableid));
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
            Check(ErrApi.JetAddColumn(sesid, tableid, column, columndef, defaultValue, defaultValueSize, out columnid));
        }

        /// <summary>
        /// Deletes a column from a database table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">A cursor on the table to delete the column from.</param>
        /// <param name="column">The name of the column to be deleted.</param>
        public static void JetDeleteColumn(JET_SESID sesid, JET_TABLEID tableid, string column)
        {
            Check(ErrApi.JetDeleteColumn(sesid, tableid, column));
        }

        /// <summary>
        /// Deletes an index from a database table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">A cursor on the table to delete the index from.</param>
        /// <param name="index">The name of the index to be deleted.</param>
        public static void JetDeleteIndex(JET_SESID sesid, JET_TABLEID tableid, string index)
        {
            Check(ErrApi.JetDeleteIndex(sesid, tableid, index));
        }

        /// <summary>
        /// Deletes a table from a database.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database to delete the table from.</param>
        /// <param name="table">The name of the table to delete.</param>
        public static void JetDeleteTable(JET_SESID sesid, JET_DBID dbid, string table)
        {
            Check(ErrApi.JetDeleteTable(sesid, dbid, table));
        }

        /// <summary>
        /// Creates an index over data in an ESE database. An index can be used to locate
        /// specific data quickly.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to create the index on.</param>
        /// <param name="indexName">
        /// Pointer to a null-terminated string that specifies the name of the index to create. 
        /// </param>
        /// <param name="grbit">Index creation options.</param>
        /// <param name="keyDescription">
        /// Pointer to a double null-terminated string of null-delimited tokens.
        /// </param>
        /// <param name="keyDescriptionLength">
        /// The length, in characters, of szKey including the two terminating nulls.
        /// </param>
        /// <param name="density">Initial B+ tree density.</param>
        public static void JetCreateIndex(
            JET_SESID sesid,
            JET_TABLEID tableid,
            string indexName,
            CreateIndexGrbit grbit,
            string keyDescription,
            int keyDescriptionLength,
            int density)
        {
            Check(ErrApi.JetCreateIndex(sesid, tableid, indexName, grbit, keyDescription, keyDescriptionLength, density));
        }

        /// <summary>
        /// Retrieves information about a table column.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table containing the column.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="columndef">Filled in with information about the column.</param>
        public static void JetGetTableColumnInfo(
                JET_SESID sesid,
                JET_TABLEID tableid,
                string columnName,
                out JET_COLUMNDEF columndef)
        {
            Check(ErrApi.JetGetTableColumnInfo(sesid, tableid, columnName, out columndef));
        }

        /// <summary>
        /// Retrieves information about a table column.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table containing the column.</param>
        /// <param name="columnid">The columnid of the column.</param>
        /// <param name="columndef">Filled in with information about the column.</param>
        public static void JetGetTableColumnInfo(
                JET_SESID sesid,
                JET_TABLEID tableid,
                JET_COLUMNID columnid,
                out JET_COLUMNDEF columndef)
        {
            Check(ErrApi.JetGetTableColumnInfo(sesid, tableid, columnid, out columndef));
        }

        /// <summary>
        /// Retrieves information about all columns in the table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table containing the column.</param>
        /// <param name="columnName">The parameter is ignored.</param>
        /// <param name="columnlist">Filled in with information about the columns in the table.</param>
        public static void JetGetTableColumnInfo(
                JET_SESID sesid,
                JET_TABLEID tableid,
                string columnName,
                out JET_COLUMNLIST columnlist)
        {
            Check(ErrApi.JetGetTableColumnInfo(sesid, tableid, columnName, out columnlist));
        }

        /// <summary>
        /// Retrieves information about a table column.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database that contains the table.</param>
        /// <param name="tableName">The name of the table containing the column.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="columndef">Filled in with information about the column.</param>
        public static void JetGetColumnInfo(
                JET_SESID sesid,
                JET_DBID dbid,
                string tableName,
                string columnName,
                out JET_COLUMNDEF columndef)
        {
            Check(ErrApi.JetGetColumnInfo(sesid, dbid, tableName, columnName, out columndef));
        }

        /// <summary>
        /// Retrieves information about all columns in a table.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database that contains the table.</param>
        /// <param name="tableName">The name of the table containing the column.</param>
        /// <param name="columnName">This parameter is ignored.</param>
        /// <param name="columnlist">Filled in with information about the columns in the table.</param>
        public static void JetGetColumnInfo(
                JET_SESID sesid,
                JET_DBID dbid,
                string tableName,
                string columnName,
                out JET_COLUMNLIST columnlist)
        {
            Check(ErrApi.JetGetColumnInfo(sesid, dbid, tableName, columnName, out columnlist));
        }

        /// <summary>
        /// Retrieves information about database objects.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="dbid">The database to use.</param>
        /// <param name="objectlist">Filled in with information about the objects in the database.</param>
        public static void JetGetObjectInfo(JET_SESID sesid, JET_DBID dbid, out JET_OBJECTLIST objectlist)
        {
            Check(ErrApi.JetGetObjectInfo(sesid, dbid, out objectlist));
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
            Check(ErrApi.JetGetBookmark(sesid, tableid, bookmark, bookmarkSize, out actualBookmarkSize));
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
            Check(ErrApi.JetGotoBookmark(sesid, tableid, bookmark, bookmarkSize));
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
        /// <returns>An ESENT warning code.</returns>
        public static JET_wrn JetRetrieveColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, byte[] data, int dataSize, out int actualDataSize, RetrieveColumnGrbit grbit, JET_RETINFO retinfo)
        {
            return Check(ErrApi.JetRetrieveColumn(sesid, tableid, columnid, data, dataSize, out actualDataSize, grbit, retinfo));
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
            Check(ErrApi.JetMove(sesid, tableid, numRows, grbit));
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
            Check(ErrApi.JetMove(sesid, tableid, (int)numRows, grbit));
        }

        /// <summary>
        /// Constructs search keys that may then be used by JetSeek and JetSetIndexRange.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to create the key on.</param>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="dataSize">Size of the data.</param>
        /// <param name="grbit">Key options.</param>
        public static void JetMakeKey(JET_SESID sesid, JET_TABLEID tableid, byte[] data, int dataSize, MakeKeyGrbit grbit)
        {
            Check(ErrApi.JetMakeKey(sesid, tableid, data, dataSize, grbit));
        }

        /// <summary>
        /// Retrieves the key for the index entry at the current position of a cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the key from.</param>
        /// <param name="data">The buffer to retrieve the key into.</param>
        /// <param name="dataSize">The size of the buffer.</param>
        /// <param name="actualDataSize">Returns the actual size of the data.</param>
        /// <param name="grbit">Retrieve key options.</param>
        public static void JetRetrieveKey(JET_SESID sesid, JET_TABLEID tableid, byte[] data, int dataSize, out int actualDataSize, RetrieveKeyGrbit grbit)
        {
            Check(ErrApi.JetRetrieveKey(sesid, tableid, data, dataSize, out actualDataSize, grbit));
        }

        /// <summary>
        /// Efficiently positions a cursor to an index entry that matches the search
        /// criteria specified by the search key in that cursor and the specified
        /// inequality. A search key must have been previously constructed using JetMakeKey.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to position.</param>
        /// <param name="grbit">Seek options.</param>
        /// <returns>An ESENT warning.</returns>
        public static JET_wrn JetSeek(JET_SESID sesid, JET_TABLEID tableid, SeekGrbit grbit)
        {
            return Check(ErrApi.JetSeek(sesid, tableid, grbit));
        }

        /// <summary>
        /// Temporarily limits the set of index entries that the cursor can walk using
        /// JetMove to those starting from the current index entry and ending at the index
        /// entry that matches the search criteria specified by the search key in that cursor
        /// and the specified bound criteria. A search key must have been previously constructed
        /// using JetMakeKey. 
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to set the index range on.</param>
        /// <param name="grbit">Index range options.</param>
        public static void JetSetIndexRange(JET_SESID sesid, JET_TABLEID tableid, SetIndexRangeGrbit grbit)
        {
            Check(ErrApi.JetSetIndexRange(sesid, tableid, grbit));
        }

        /// <summary>
        /// Set the current index of a cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to set the index on.</param>
        /// <param name="index">
        /// The name of the index to be selected. If this is null or empty the primary
        /// index will be selected.
        /// </param>
        public static void JetSetCurrentIndex(JET_SESID sesid, JET_TABLEID tableid, string index)
        {
            Check(ErrApi.JetSetCurrentIndex(sesid, tableid, index));
        }

        /// <summary>
        /// Counts the number of entries in the current index from the current position forward.
        /// The current position is included in the count. The count can be greater than the
        /// total number of records in the table if the current index is over a multi-valued
        /// column and instances of the column have multiple-values. If the table is empty,
        /// then 0 will be returned for the count. 
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to count the records in.</param>
        /// <param name="numRecords">Returns the number of records.</param>
        /// <param name="maxRecordsToCount">
        /// The maximum number of records to count. A value of 0 indicates that the count
        /// is unlimited.
        /// </param>
        public static void JetIndexRecordCount(JET_SESID sesid, JET_TABLEID tableid, out int numRecords, int maxRecordsToCount)
        {
            Check(ErrApi.JetIndexRecordCount(sesid, tableid, out numRecords, maxRecordsToCount));
        }

        /// <summary>
        /// Notifies the database engine that the application is scanning the entire
        /// index that the cursor is positioned on. Consequently, the methods that
        /// are used to access the index data will be tuned to make this scenario as
        /// fast as possible. 
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor that will be accessing the data.</param>
        /// <param name="grbit">Reserved for future use.</param>
        public static void JetSetTableSequential(JET_SESID sesid, JET_TABLEID tableid, SetTableSequentialGrbit grbit)
        {
            Check(ErrApi.JetSetTableSequential(sesid, tableid, grbit));
        }

        /// <summary>
        /// Notifies the database engine that the application is no longer scanning the
        /// entire index the cursor is positioned on. This call reverses a notification
        /// sent by JetSetTableSequential.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor that was accessing the data.</param>
        /// <param name="grbit">Reserved for future use.</param>
        public static void JetResetTableSequential(JET_SESID sesid, JET_TABLEID tableid, ResetTableSequentialGrbit grbit)
        {
            Check(ErrApi.JetResetTableSequential(sesid, tableid, grbit));
        }

        /// <summary>
        /// Returns the fractional position of the current record in the current index
        /// in the form of a JET_RECPOS structure.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor positioned on the record.</param>
        /// <param name="recpos">Returns the approximate fractional position of the record.</param>
        public static void JetGetRecordPosition(JET_SESID sesid, JET_TABLEID tableid, out JET_RECPOS recpos)
        {
            Check(ErrApi.JetGetRecordPosition(sesid, tableid, out recpos));
        }

        /// <summary>
        /// Moves a cursor to a new location that is a fraction of the way through
        /// the current index. 
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to position.</param>
        /// <param name="recpos">The approximate position to move to.</param>
        public static void JetGotoPosition(JET_SESID sesid, JET_TABLEID tableid, JET_RECPOS recpos)
        {
            Check(ErrApi.JetGotoPosition(sesid, tableid, recpos));
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
            Check(ErrApi.JetDelete(sesid, tableid));
        }

        /// <summary>
        /// Prepare a cursor for update.
        /// </summary>
        /// <param name="sesid">The session which is starting the update.</param>
        /// <param name="tableid">The cursor to start the update for.</param>
        /// <param name="prep">The type of update to prepare.</param>
        public static void JetPrepareUpdate(JET_SESID sesid, JET_TABLEID tableid, JET_prep prep)
        {
            Check(ErrApi.JetPrepareUpdate(sesid, tableid, prep));
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
        /// Indexes are updated only by JetUpdate or and not during JetSetColumn or JetSetColumns
        /// </remarks>
        public static void JetUpdate(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize, out int actualBookmarkSize)
        {
            Check(ErrApi.JetUpdate(sesid, tableid, bookmark, bookmarkSize, out actualBookmarkSize));
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
            Check(ErrApi.JetSetColumn(sesid, tableid, columnid, data, dataSize, grbit, setinfo));
        }

        /// <summary>
        /// Explicitly reserve the ability to update a row, write lock, or to explicitly prevent a row from
        /// being updated by any other session, read lock. Normally, row write locks are acquired implicitly as a
        /// result of updating rows. Read locks are usually not required because of record versioning. However,
        /// in some cases a transaction may desire to explicitly lock a row to enforce serialization, or to ensure
        /// that a subsequent operation will succeed. 
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to use. A lock will be acquired on the current record.</param>
        /// <param name="grbit">Lock options, use this to specify which type of lock to obtain.</param>
        public static void JetGetLock(JET_SESID sesid, JET_TABLEID tableid, GetLockGrbit grbit)
        {
            Check(ErrApi.JetGetLock(sesid, tableid, grbit));
        }

        #endregion

        #region Error Handling

        /// <summary>
        /// Throw an exception if the parameter is an ESE error,
        /// returns a JET_wrn otherwise.
        /// </summary>
        /// <param name="err">The error code to check.</param>
        /// <returns>An ESENT warning code (possibly success).</returns>
        private static JET_wrn Check(int err)
        {
            if (err < 0)
            {
                throw new EsentException((JET_err)err);
            }

            return (JET_wrn)err;
        }

        #endregion Error Handling
    }
}
