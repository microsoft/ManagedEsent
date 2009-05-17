//-----------------------------------------------------------------------
// <copyright file="IJetApi.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace Microsoft.Isam.Esent.Interop.Implementation
{
    internal interface IJetApi
    {
        JetCapabilities Capabilities { get;  }

        int JetCreateInstance(out JET_INSTANCE instance, string name);

        /// <summary>
        /// Allocate a new instance of the database engine for use in a single
        /// process, with a display name specified.
        /// </summary>
        /// <param name="instance">Returns the newly create instance.</param>
        /// <param name="name">
        /// Specifies a unique string identifier for the instance to be created.
        /// This string must be unique within a given process hosting the
        /// database engine.
        /// </param>
        /// <param name="displayName">
        /// A display name for the instance to be created. This will be used
        /// in eventlog entries.
        /// </param>
        /// <param name="grbit">Creation options.</param>
        /// <returns>An error if the call fails.</returns>
        int JetCreateInstance2(out JET_INSTANCE instance, string name, string displayName, CreateInstanceGrbit grbit);

        int JetInit(ref JET_INSTANCE instance);

        /// <summary>
        /// Initialize the ESENT database engine.
        /// </summary>
        /// <param name="instance">
        /// The instance to initialize. If an instance hasn't been
        /// allocated then a new one is created and the engine
        /// will operate in single-instance mode.
        /// </param>
        /// <param name="grbit">
        /// Initialization options.
        /// </param>
        /// <returns>An error or warning.</returns>
        int JetInit2(ref JET_INSTANCE instance, InitGrbit grbit);

        int JetTerm(JET_INSTANCE instance);

        int JetTerm2(JET_INSTANCE instance, TermGrbit grbit);

        int JetSetSystemParameter(JET_INSTANCE instance, JET_SESID sesid, JET_param paramid, int paramValue, string paramString);

        int JetGetSystemParameter(JET_INSTANCE instance, JET_SESID sesid, JET_param paramid, ref int paramValue, out string paramString, int maxParam);

        /// <summary>
        /// Retrieves the version of the database engine.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="version">Returns the version number of the database engine.</param>
        /// <returns>An error code if the call fails.</returns>
        int JetGetVersion(JET_SESID sesid, out int version);

        int JetCreateDatabase(JET_SESID sesid, string database, string connect, out JET_DBID dbid, CreateDatabaseGrbit grbit);

        int JetAttachDatabase(JET_SESID sesid, string database, AttachDatabaseGrbit grbit);

        int JetOpenDatabase(JET_SESID sesid, string database, string connect, out JET_DBID dbid, OpenDatabaseGrbit grbit);

        int JetCloseDatabase(JET_SESID sesid, JET_DBID dbid, CloseDatabaseGrbit grbit);

        int JetDetachDatabase(JET_SESID sesid, string database);

        int JetBeginSession(JET_INSTANCE instance, out JET_SESID sesid, string username, string password);

        /// <summary>
        /// Associates a session with the current thread using the given context
        /// handle. This association overrides the default engine requirement
        /// that a transaction for a given session must occur entirely on the
        /// same thread. 
        /// </summary>
        /// <param name="sesid">The session to set the context on.</param>
        /// <param name="context">The context to set.</param>
        /// <returns>An error if the call fails.</returns>
        int JetSetSessionContext(JET_SESID sesid, IntPtr context);

        /// <summary>
        /// Disassociates a session from the current thread. This should be
        /// used in conjunction with JetSetSessionContext.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <returns>An error if the call fails.</returns>
        int JetResetSessionContext(JET_SESID sesid);

        int JetEndSession(JET_SESID sesid, EndSessionGrbit grbit);

        int JetDupSession(JET_SESID sesid, out JET_SESID newSesid);

        int JetOpenTable(JET_SESID sesid, JET_DBID dbid, string tablename, byte[] parameters, int parametersLength, OpenTableGrbit grbit, out JET_TABLEID tableid);

        int JetCloseTable(JET_SESID sesid, JET_TABLEID tableid);

        int JetDupCursor(JET_SESID sesid, JET_TABLEID tableid, out JET_TABLEID newTableid, DupCursorGrbit grbit);

        int JetBeginTransaction(JET_SESID sesid);

        int JetCommitTransaction(JET_SESID sesid, CommitTransactionGrbit grbit);

        int JetRollback(JET_SESID sesid, RollbackTransactionGrbit grbit);

        int JetCreateTable(JET_SESID sesid, JET_DBID dbid, string table, int pages, int density, out JET_TABLEID tableid);

        int JetAddColumn(JET_SESID sesid, JET_TABLEID tableid, string column, JET_COLUMNDEF columndef, byte[] defaultValue, int defaultValueSize, out JET_COLUMNID columnid);

        int JetDeleteColumn(JET_SESID sesid, JET_TABLEID tableid, string column);

        int JetDeleteIndex(JET_SESID sesid, JET_TABLEID tableid, string index);

        int JetDeleteTable(JET_SESID sesid, JET_DBID dbid, string table);

        int JetCreateIndex(
            JET_SESID sesid,
            JET_TABLEID tableid,
            string indexName,
            CreateIndexGrbit grbit, 
            string keyDescription,
            int keyDescriptionLength,
            int density);

        /// <summary>
        /// Creates indexes over data in an ESE database
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The table to create the index on.</param>
        /// <param name="indexcreates">Array of objects describing the indexes to be created.</param>
        /// <param name="numIndexCreates">Number of index description objects.</param>
        /// <returns>An error code.</returns>
        int JetCreateIndex2(
            JET_SESID sesid,
            JET_TABLEID tableid,
            JET_INDEXCREATE[] indexcreates,
            int numIndexCreates);

        int JetGetTableColumnInfo(
            JET_SESID sesid,
            JET_TABLEID tableid,
            string columnName,
            out JET_COLUMNDEF columndef);

        int JetGetTableColumnInfo(
            JET_SESID sesid,
            JET_TABLEID tableid,
            JET_COLUMNID columnid,
            out JET_COLUMNDEF columndef);

        int JetGetTableColumnInfo(
            JET_SESID sesid,
            JET_TABLEID tableid,
            string ignored,
            out JET_COLUMNLIST columnlist);

        int JetGetColumnInfo(
            JET_SESID sesid,
            JET_DBID dbid,
            string tablename,
            string columnName,
            out JET_COLUMNDEF columndef);

        int JetGetColumnInfo(
            JET_SESID sesid,
            JET_DBID dbid,
            string tablename,
            string ignored,
            out JET_COLUMNLIST columnlist);

        int JetGetObjectInfo(JET_SESID sesid, JET_DBID dbid, out JET_OBJECTLIST objectlist);

        /// <summary>
        /// JetGetCurrentIndex function determines the name of the current
        /// index of a given cursor. This name is also used to later re-select
        /// that index as the current index using JetSetCurrentIndex. It can
        /// also be used to discover the properties of that index using
        /// JetGetTableIndexInfo.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to get the index name for.</param>
        /// <param name="indexName">Returns the name of the index.</param>
        /// <param name="maxNameLength">
        /// The maximum length of the index name. Index names are no more than 
        /// Api.MaxNameLength characters.
        /// </param>
        /// <returns>An error if the call fails.</returns>
        int JetGetCurrentIndex(JET_SESID sesid, JET_TABLEID tableid, out string indexName, int maxNameLength);

        int JetGetIndexInfo(
            JET_SESID sesid,
            JET_DBID dbid,
            string tablename,
            string ignored,
            out JET_INDEXLIST indexlist);

        int JetGetTableIndexInfo(
            JET_SESID sesid,
            JET_TABLEID tableid,
            string ignored,
            out JET_INDEXLIST indexlist);

        int JetGetBookmark(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize, out int actualBookmarkSize);

        int JetGotoBookmark(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize);

        int JetMakeKey(JET_SESID sesid, JET_TABLEID tableid, IntPtr data, int dataSize, MakeKeyGrbit grbit);

        int JetRetrieveKey(JET_SESID sesid, JET_TABLEID tableid, byte[] data, int dataSize, out int actualDataSize, RetrieveKeyGrbit grbit);

        int JetSeek(JET_SESID sesid, JET_TABLEID tableid, SeekGrbit grbit);

        int JetMove(JET_SESID sesid, JET_TABLEID tableid, int numRows, MoveGrbit grbit);

        int JetSetIndexRange(JET_SESID sesid, JET_TABLEID tableid, SetIndexRangeGrbit grbit);

        /// <summary>
        /// Computes the intersection between multiple sets of index entries from different secondary
        /// indices over the same table. This operation is useful for finding the set of records in a
        /// table that match two or more criteria that can be expressed using index ranges. 
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="ranges">
        /// An the index ranges to intersect. The tableids in the ranges
        ///  must have index ranges set on them.
        /// </param>
        /// <param name="numRanges">
        /// The number of index ranges.
        /// </param>
        /// <param name="recordlist">
        /// Returns information about the temporary table containing the intersection results.
        /// </param>
        /// <param name="grbit">Intersection options.</param>
        /// <returns>An error if the call fails.</returns>
        int JetIntersectIndexes(
            JET_SESID sesid,
            JET_INDEXRANGE[] ranges,
            int numRanges,
            out JET_RECORDLIST recordlist,
            IntersectIndexesGrbit grbit);

        int JetSetCurrentIndex(JET_SESID sesid, JET_TABLEID tableid, string index);

        int JetIndexRecordCount(JET_SESID sesid, JET_TABLEID tableid, out int numRecords, int maxRecordsToCount);

        int JetSetTableSequential(JET_SESID sesid, JET_TABLEID tableid, SetTableSequentialGrbit grbit);

        int JetResetTableSequential(JET_SESID sesid, JET_TABLEID tableid, ResetTableSequentialGrbit grbit);

        int JetGetRecordPosition(JET_SESID sesid, JET_TABLEID tableid, out JET_RECPOS recpos);

        int JetGotoPosition(JET_SESID sesid, JET_TABLEID tableid, JET_RECPOS recpos);

        int JetRetrieveColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, IntPtr data, int dataSize, out int actualDataSize, RetrieveColumnGrbit grbit, JET_RETINFO retinfo);

        int JetDelete(JET_SESID sesid, JET_TABLEID tableid);

        int JetPrepareUpdate(JET_SESID sesid, JET_TABLEID tableid, JET_prep prep);

        int JetUpdate(JET_SESID sesid, JET_TABLEID tableid, byte[] bookmark, int bookmarkSize, out int actualBookmarkSize);

        int JetSetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, IntPtr data, int dataSize, SetColumnGrbit grbit, JET_SETINFO setinfo);

        int JetGetLock(JET_SESID sesid, JET_TABLEID tableid, GetLockGrbit grbit);

        /// <summary>
        /// Performs an atomic addition operation on one column. This function allows
        /// multiple sessions to update the same record concurrently without conflicts.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update.</param>
        /// <param name="columnid">
        /// The column to update. This must be an escrow updatable column.
        /// </param>
        /// <param name="delta">The buffer containing the addend.</param>
        /// <param name="deltaSize">The size of the addend.</param>
        /// <param name="previousValue">
        /// An output buffer that will recieve the current value of the column. This buffer
        /// can be null.
        /// </param>
        /// <param name="previousValueLength">The size of the previousValue buffer.</param>
        /// <param name="actualPreviousValueLength">Returns the actual size of the previousValue.</param>
        /// <param name="grbit">Escrow update options.</param>
        /// <returns>An error code if the operation fails.</returns>
        int JetEscrowUpdate(
            JET_SESID sesid,
            JET_TABLEID tableid,
            JET_COLUMNID columnid,
            byte[] delta,
            int deltaSize,
            byte[] previousValue,
            int previousValueLength,
            out int actualPreviousValueLength,
            EscrowUpdateGrbit grbit);
    }
}