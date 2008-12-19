//-----------------------------------------------------------------------
// <copyright file="MoveHelpers.cs" company="Microsoft Corporation">
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
