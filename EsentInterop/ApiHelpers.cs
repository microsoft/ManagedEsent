//-----------------------------------------------------------------------
// <copyright file="ApiHelpers.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// Helper methods for the ESENT API. These aren't interop versions
    /// of the API, but encapsulate very common uses of the functions.
    /// </summary>
    public static partial class Api
    {
        /// <summary>
        /// Retrieves the bookmark for the record that is associated with the index entry
        /// at the current position of a cursor. This bookmark can then be used to
        /// reposition that cursor back to the same record using JetGotoBookmark. 
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the bookmark from.</param>
        /// <returns>The bookmark of the record.</returns>
        public static byte[] GetBookmark(JET_SESID sesid, JET_TABLEID tableid)
        {
            // Get the size of the bookmark, allocate memory, retrieve the bookmark.
            int bookmarkSize;
            JET_err err = (JET_err)ErrApi.JetGetBookmark(sesid, tableid, null, 0, out bookmarkSize);
            if (err < JET_err.Success && JET_err.BufferTooSmall != err)
            {
                throw new EsentException(err);
            }

            byte[] bookmark = new byte[bookmarkSize];
            Api.JetGetBookmark(sesid, tableid, bookmark, bookmark.Length, out bookmarkSize);

            return bookmark;
        }

        /// <summary>
        /// Retrieves the key for the index entry at the current position of a cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the key from.</param>
        /// <param name="grbit">Retrieve key options.</param>
        /// <returns>The retrieved key.</returns>
        public static byte[] RetrieveKey(JET_SESID sesid, JET_TABLEID tableid, RetrieveKeyGrbit grbit)
        {
            // Get the size of the key, allocate memory, retrieve the key.
            int keySize;
            Api.JetRetrieveKey(sesid, tableid, null, 0, out keySize, grbit);

            byte[] key = new byte[keySize];
            Api.JetRetrieveKey(sesid, tableid, key, key.Length, out keySize, grbit);

            return key;
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
        /// <param name="grbit">Retrieve column options.</param>
        /// <param name="retinfo">
        /// If pretinfo is give as NULL then the function behaves as though an itagSequence
        /// of 1 and an ibLongValue of 0 (zero) were given. This causes column retrieval to
        /// retrieve the first value of a multi-valued column, and to retrieve long data at
        /// offset 0 (zero).
        /// </param>
        /// <returns>The data retrieved from the column. Null if the column is null.</returns>
        public static byte[] RetrieveColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, RetrieveColumnGrbit grbit, JET_RETINFO retinfo)
        {
            // Get the size of the column, allocate memory, retrieve the column.
            int dataSize;
            JET_wrn wrn = Api.JetRetrieveColumn(sesid, tableid, columnid, null, 0, out dataSize, grbit, retinfo);

            byte[] data;
            if (JET_wrn.ColumnNull == wrn)
            {
                // null column
                data = null;
            }
            else if (0 == dataSize)
            {
                // zero-length column
                data = new byte[0];
            }
            else
            {
                // there is data to retrieve
                data = new byte[dataSize];
                Api.JetRetrieveColumn(sesid, tableid, columnid, data, data.Length, out dataSize, grbit, retinfo);
            }

            return data;
        }

        /// <summary>
        /// Try to move to the first record in the table. If the table is empty this
        /// returns false, if a different error is encountered an exception is thrown.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to position.</param>
        /// <returns>True if the move was successful.</returns>
        public static bool TryMoveFirst(JET_SESID sesid, JET_TABLEID tableid)
        {
            JET_err err = (JET_err)ErrApi.JetMove(sesid, tableid, (int)JET_Move.First, MoveGrbit.None);
            if (JET_err.Success == err)
            {
                return true;
            }
            else if (JET_err.NoCurrentRecord == err)
            {
                return false;
            }

            throw new EsentException(err);
        }

        /// <summary>
        /// Try to move to the last record in the table. If the table is empty this
        /// returns false, if a different error is encountered an exception is thrown.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to position.</param>
        /// <returns>True if the move was successful.</returns>
        public static bool TryMoveLast(JET_SESID sesid, JET_TABLEID tableid)
        {
            JET_err err = (JET_err)ErrApi.JetMove(sesid, tableid, (int)JET_Move.Last, MoveGrbit.None);
            if (JET_err.Success == err)
            {
                return true;
            }
            else if (JET_err.NoCurrentRecord == err)
            {
                return false;
            }

            throw new EsentException(err);
        }

        /// <summary>
        /// Try to move to the next record in the table. If there is not a next record
        /// this returns false, if a different error is encountered an exception is thrown.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to position.</param>
        /// <returns>True if the move was successful.</returns>
        public static bool TryMoveNext(JET_SESID sesid, JET_TABLEID tableid)
        {
            JET_err err = (JET_err)ErrApi.JetMove(sesid, tableid, (int)JET_Move.Next, MoveGrbit.None);
            if (JET_err.Success == err)
            {
                return true;
            }
            else if (JET_err.NoCurrentRecord == err)
            {
                return false;
            }

            throw new EsentException(err);
        }

        /// <summary>
        /// Try to move to the previous record in the table. If there is not a previous record
        /// this returns false, if a different error is encountered an exception is thrown.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to position.</param>
        /// <returns>True if the move was successful.</returns>
        public static bool TryMovePrevious(JET_SESID sesid, JET_TABLEID tableid)
        {
            JET_err err = (JET_err)ErrApi.JetMove(sesid, tableid, (int)JET_Move.Previous, MoveGrbit.None);
            if (JET_err.Success == err)
            {
                return true;
            }
            else if (JET_err.NoCurrentRecord == err)
            {
                return false;
            }

            throw new EsentException(err);
        }

        /// <summary>
        /// Efficiently positions a cursor to an index entry that matches the search
        /// criteria specified by the search key in that cursor and the specified
        /// inequality. A search key must have been previously constructed using JetMakeKey.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to position.</param>
        /// <param name="grbit">Seek option.</param>
        /// <returns>True if a record matching the criteria was found.</returns>
        public static bool TrySeek(JET_SESID sesid, JET_TABLEID tableid, SeekGrbit grbit)
        {
            JET_err err = (JET_err)ErrApi.JetSeek(sesid, tableid, grbit);
            if (JET_err.Success == err)
            {
                return true;
            }
            else if (JET_err.NoCurrentRecord == err)
            {
                return false;
            }

            throw new EsentException(err);
        }

        /// <summary>
        /// Temporarily limits the set of index entries that the cursor can walk using
        /// JetMove to those starting from the current index entry and ending at the index
        /// entry that matches the search criteria specified by the search key in that cursor
        /// and the specified bound criteria. A search key must have been previously constructed
        /// using JetMakeKey. Returns true if the index range is non-empty, false otherwise.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to position.</param>
        /// <param name="grbit">Seek option.</param>
        /// <returns>True if the seek was successful.</returns>
        public static bool TrySetIndexRange(JET_SESID sesid, JET_TABLEID tableid, SetIndexRangeGrbit grbit)
        {
            JET_err err = (JET_err)ErrApi.JetSetIndexRange(sesid, tableid, grbit);
            if (JET_err.Success == err)
            {
                return true;
            }
            else if (JET_err.NoCurrentRecord == err)
            {
                return false;
            }

            throw new EsentException(err);
        }
    }
}
