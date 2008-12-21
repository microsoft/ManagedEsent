//-----------------------------------------------------------------------
// <copyright file="RetrieveColumnHelpers.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;
    using System.Text;

    /// <summary>
    /// Helper methods for the ESENT API. These aren't interop versions
    /// of the API, but encapsulate very common uses of the functions.
    /// The methods that operate on unsigned numbers are
    /// internal so that the public API remains CLS compliant.
    /// </summary>
    public static partial class Api
    {
        /// <summary>
        /// Conversion function delegate.
        /// </summary>
        /// <typeparam name="TResult">The return type.</typeparam>
        /// <param name="data">The data to convert,</param>
        /// <returns>An object of type TRresult</returns>
        /// <remarks>
        /// We create this delegate here, instead of using the built-in
        /// Func generic to avoid taking a dependency on a higher version
        /// of the CLR.
        /// </remarks>
        private delegate TResult ConversionFunc<TResult>(byte[] data);

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
                Api.Check((int)err);
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
        /// Retrieves a single column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column. Null if the column is null.</returns>
        public static byte[] RetrieveColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return Api.RetrieveColumn(sesid, tableid, columnid, RetrieveColumnGrbit.None, null);
        }

        /// <summary>
        /// Retrieves a single column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="encoding">The string encoding to use when converting data.</param>
        /// <returns>The data retrieved from the column as a string. Null if the column is null.</returns>
        public static string RetrieveColumnAsString(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, Encoding encoding)
        {
            byte[] data = Api.RetrieveColumn(sesid, tableid, columnid, RetrieveColumnGrbit.None, null);
            if (null == data)
            {
                return null;
            }

            return encoding.GetString(data);
        }

        /// <summary>
        /// Retrieves a single column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// The Unicode encoding is used.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column as a string. Null if the column is null.</returns>
        public static string RetrieveColumnAsString(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return Api.RetrieveColumnAsString(sesid, tableid, columnid, Encoding.Unicode);
        }

        /// <summary>
        /// Retrieves a single column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column as a short. Null if the column is null.</returns>
        public static short? RetrieveColumnAsInt16(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return RetrieveColumnAndConvert(sesid, tableid, columnid, data => BitConverter.ToInt16(data, 0));
        }

        /// <summary>
        /// Retrieves a single column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column as an int. Null if the column is null.</returns>
        public static int? RetrieveColumnAsInt32(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return RetrieveColumnAndConvert(sesid, tableid, columnid, data => BitConverter.ToInt32(data, 0));
        }

        /// <summary>
        /// Retrieves a single column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column as a long. Null if the column is null.</returns>
        public static long? RetrieveColumnAsInt64(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return RetrieveColumnAndConvert(sesid, tableid, columnid, data => BitConverter.ToInt64(data, 0));
        }

        /// <summary>
        /// Retrieves a single column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column as a float. Null if the column is null.</returns>
        public static float? RetrieveColumnAsFloat(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return RetrieveColumnAndConvert(sesid, tableid, columnid, data => BitConverter.ToSingle(data, 0));
        }

        /// <summary>
        /// Retrieves a single column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column as a double. Null if the column is null.</returns>
        public static double? RetrieveColumnAsDouble(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return RetrieveColumnAndConvert(sesid, tableid, columnid, data => BitConverter.ToDouble(data, 0));
        }

        /// <summary>
        /// Retrieves a single column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column as a boolean. Null if the column is null.</returns>
        public static bool? RetrieveColumnAsBoolean(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return RetrieveColumnAndConvert(sesid, tableid, columnid, data => BitConverter.ToBoolean(data, 0));
        }

        /// <summary>
        /// Retrieves a single column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column as a byte. Null if the column is null.</returns>
        public static byte? RetrieveColumnAsByte(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return RetrieveColumnAndConvert(sesid, tableid, columnid, data => data[0]);
        }

        /// <summary>
        /// Retrieves a single column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column as a guid. Null if the column is null.</returns>
        public static Guid? RetrieveColumnAsGuid(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return RetrieveColumnAndConvert(sesid, tableid, columnid, data => new Guid(data));
        }

        /// <summary>
        /// Retrieves a single column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column as an UInt16. Null if the column is null.</returns>
        /// <remarks>Internal becaue unsigned types aren't CLS compliant.</remarks>
        internal static ushort? RetrieveColumnAsUInt16(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return RetrieveColumnAndConvert(sesid, tableid, columnid, data => BitConverter.ToUInt16(data, 0));
        }

        /// <summary>
        /// Retrieves a single column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column as an UInt32. Null if the column is null.</returns>
        /// <remarks>Internal becaue unsigned types aren't CLS compliant.</remarks>
        internal static uint? RetrieveColumnAsUInt32(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return RetrieveColumnAndConvert(sesid, tableid, columnid, data => BitConverter.ToUInt32(data, 0));
        }

        /// <summary>
        /// Retrieves a single column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column as an UInt64. Null if the column is null.</returns>
        /// <remarks>Internal becaue unsigned types aren't CLS compliant.</remarks>
        internal static ulong? RetrieveColumnAsUInt64(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return RetrieveColumnAndConvert(sesid, tableid, columnid, data => BitConverter.ToUInt64(data, 0));
        }

        /// <summary>
        /// Retrieves a single column value from the current record and converts it.
        /// The record is that record associated with the index entry at the current position
        /// of the cursor.
        /// </summary>
        /// <typeparam name="TResult">The return type.</typeparam>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="converter">The conversion function to use.</param>
        /// <returns>The data retrieved from the column. Null if the column is null.</returns>
        private static TResult? RetrieveColumnAndConvert<TResult>(
            JET_SESID sesid,
            JET_TABLEID tableid,
            JET_COLUMNID columnid,
            ConversionFunc<TResult> converter) where TResult : struct
        {
            byte[] data = Api.RetrieveColumn(sesid, tableid, columnid, RetrieveColumnGrbit.None, null);
            if (null == data)
            {
                return null;
            }

            return converter(data);
        }
    }
}
