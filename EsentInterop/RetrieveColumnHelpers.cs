//-----------------------------------------------------------------------
// <copyright file="RetrieveColumnHelpers.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// Helper methods for the ESENT API. These aren't interop versions
    /// of the API, but encapsulate very common uses of the functions.
    /// The methods that operate on unsigned numbers are
    /// internal so that the public API remains CLS compliant.
    /// </summary>
    public static partial class Api
    {
        /// <summary>
        /// Cached retrieve buffers
        /// </summary>
        private static MemoryCache memoryCache = new MemoryCache();

        /// <summary>
        /// Conversion function delegate.
        /// </summary>
        /// <typeparam name="TResult">The return type.</typeparam>
        /// <param name="data">The data to convert,</param>
        /// <returns>An object of type TRresult</returns>
        /// <remarks>
        /// We create this delegate here, instead of using the built-in
        /// Func/Converter generics to avoid taking a dependency on 
        /// a higher version (3.5) of the CLR.
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
            var err = (JET_err)Impl.JetGetBookmark(sesid, tableid, null, 0, out bookmarkSize);
            if (err < JET_err.Success && JET_err.BufferTooSmall != err)
            {
                Api.Check((int)err);
            }

            var bookmark = new byte[bookmarkSize];
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

            var key = new byte[keySize];
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
            // We expect most column values retrieved this way to be small (retrieving a 1GB LV as one
            // chunk is a bit extreme!). Allocate a small buffer and use that, allocating a larger one
            // if needed. This also works around the problem of getting an error when retrieving an
            // escrow-update column with no buffer.
            var data = new byte[256];
            int dataSize;
            JET_wrn wrn = Api.JetRetrieveColumn(
                sesid, tableid, columnid, data, data.Length, out dataSize, grbit, retinfo);

            if (JET_wrn.ColumnNull == wrn)
            {
                // null column
                data = null;
            }
            else
            {
                Array.Resize(ref data, dataSize);
                if (JET_wrn.BufferTruncated == wrn)
                {
                    // there is more data to retrieve
                    wrn = Api.JetRetrieveColumn(
                        sesid, tableid, columnid, data, data.Length, out dataSize, grbit, retinfo);
                    if (JET_wrn.Success != wrn)
                    {
                        string error = String.Format(
                            "Column size changed from {0} to {1}. The record was probably updated by another thread.",
                            data.Length,
                            dataSize);
                        Trace.TraceError(error);
                        throw new InvalidOperationException(error);
                    }
                }
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
        /// The Unicode encoding is used.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column as a string. Null if the column is null.</returns>
        public static string RetrieveColumnAsString(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return Api.RetrieveColumnAsString(sesid, tableid, columnid, Encoding.Unicode, RetrieveColumnGrbit.None);
        }

        /// <summary>
        /// Retrieves a string column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="encoding">The string encoding to use when converting data.</param>
        /// <returns>The data retrieved from the column as a string. Null if the column is null.</returns>
        public static string RetrieveColumnAsString(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, Encoding encoding)
        {
            return Api.RetrieveColumnAsString(sesid, tableid, columnid, encoding, RetrieveColumnGrbit.None);
        }

        /// <summary>
        /// Retrieves a string column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="encoding">The string encoding to use when converting data.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a string. Null if the column is null.</returns>
        public static string RetrieveColumnAsString(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, Encoding encoding, RetrieveColumnGrbit grbit)
        {
            // Retrieving a string happens in two stages: first the data is retrieved into a data
            // buffer and then the buffer is converted to a string. The buffer isn't needed for
            // very long so we try to use a cached buffer.
            byte[] data = Api.memoryCache.Allocate();

            int dataSize;
            JET_wrn wrn = Api.JetRetrieveColumn(sesid, tableid, columnid, data, data.Length, out dataSize, grbit, null);
            if (JET_wrn.ColumnNull == wrn)
            {
                return null;
            }

            if (JET_wrn.BufferTruncated == wrn)
            {
                Debug.Assert(dataSize > data.Length, "Expected column to be bigger than buffer");
                data = new byte[dataSize];
                wrn = Api.JetRetrieveColumn(sesid, tableid, columnid, data, data.Length, out dataSize, grbit, null);
                if (JET_wrn.BufferTruncated == wrn)
                {
                    string error = String.Format(
                        "Column size changed from {0} to {1}. The record was probably updated by another thread.",
                        data.Length,
                        dataSize);
                    Trace.TraceError(error);
                    throw new InvalidOperationException(error);                    
                }
            }

            string s = encoding.GetString(data, 0, dataSize);

            // Now we have extracted the string from the buffer we can free (cache) the buffer.
            Api.memoryCache.Free(data);

            return s;
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
            return Api.RetrieveColumnAsInt16(sesid, tableid, columnid, RetrieveColumnGrbit.None);
        }

        /// <summary>
        /// Retrieves an int16 column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a short. Null if the column is null.</returns>
        public static short? RetrieveColumnAsInt16(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, RetrieveColumnGrbit grbit)
        {
            unsafe
            {
                const int DataSize = 2;
                short data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = Api.JetRetrieveColumn(sesid, tableid, columnid, pointer, DataSize, out actualDataSize, grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
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
            return Api.RetrieveColumnAsInt32(sesid, tableid, columnid, RetrieveColumnGrbit.None);
        }

        /// <summary>
        /// Retrieves an int32 column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as an int. Null if the column is null.</returns>
        public static int? RetrieveColumnAsInt32(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, RetrieveColumnGrbit grbit)
        {
            unsafe
            {
                const int DataSize = 4;
                int data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = Api.JetRetrieveColumn(sesid, tableid, columnid, pointer, DataSize, out actualDataSize, grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
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
            return Api.RetrieveColumnAsInt64(sesid, tableid, columnid, RetrieveColumnGrbit.None);
        }

        /// <summary>
        /// Retrieves a single column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a long. Null if the column is null.</returns>
        public static long? RetrieveColumnAsInt64(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, RetrieveColumnGrbit grbit)
        {
            unsafe
            {
                const int DataSize = 8;
                long data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = Api.JetRetrieveColumn(sesid, tableid, columnid, pointer, DataSize, out actualDataSize, grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>
        /// Retrieves a float column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column as a float. Null if the column is null.</returns>
        public static float? RetrieveColumnAsFloat(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return RetrieveColumnAsFloat(sesid, tableid, columnid, RetrieveColumnGrbit.None);
        }

        /// <summary>
        /// Retrieves a float column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a float. Null if the column is null.</returns>
        public static float? RetrieveColumnAsFloat(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, RetrieveColumnGrbit grbit)
        {
            unsafe
            {
                const int DataSize = 4;
                float data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = Api.JetRetrieveColumn(sesid, tableid, columnid, pointer, DataSize, out actualDataSize, grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>
        /// Retrieves a double column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column as a double. Null if the column is null.</returns>
        public static double? RetrieveColumnAsDouble(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return RetrieveColumnAsDouble(sesid, tableid, columnid, RetrieveColumnGrbit.None);
        }

        /// <summary>
        /// Retrieves a double column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a double. Null if the column is null.</returns>
        public static double? RetrieveColumnAsDouble(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, RetrieveColumnGrbit grbit)
        {
            unsafe
            {
                const int DataSize = 8;
                double data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = Api.JetRetrieveColumn(sesid, tableid, columnid, pointer, DataSize, out actualDataSize, grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>
        /// Retrieves a boolean column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column as a boolean. Null if the column is null.</returns>
        public static bool? RetrieveColumnAsBoolean(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return Api.RetrieveColumnAsBoolean(sesid, tableid, columnid, RetrieveColumnGrbit.None);
        }

        /// <summary>
        /// Retrieves a boolean column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a boolean. Null if the column is null.</returns>
        public static bool? RetrieveColumnAsBoolean(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, RetrieveColumnGrbit grbit)
        {
            byte? b = Api.RetrieveColumnAsByte(sesid, tableid, columnid, grbit);
            if (b.HasValue)
            {
                return 0 != b.Value;
            }

            return new bool?();
        }

        /// <summary>
        /// Retrieves a byte column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column as a byte. Null if the column is null.</returns>
        public static byte? RetrieveColumnAsByte(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return Api.RetrieveColumnAsByte(sesid, tableid, columnid, RetrieveColumnGrbit.None);
        }

        /// <summary>
        /// Retrieves a byte column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a byte. Null if the column is null.</returns>
        public static byte? RetrieveColumnAsByte(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, RetrieveColumnGrbit grbit)
        {
            unsafe
            {
                const int DataSize = 1;
                byte data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = Api.JetRetrieveColumn(sesid, tableid, columnid, pointer, DataSize, out actualDataSize, grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>
        /// Retrieves a guid column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column as a guid. Null if the column is null.</returns>
        public static Guid? RetrieveColumnAsGuid(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return RetrieveColumnAsGuid(sesid, tableid, columnid, RetrieveColumnGrbit.None);
        }

        /// <summary>
        /// Retrieves a guid column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a guid. Null if the column is null.</returns>
        public static Guid? RetrieveColumnAsGuid(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, RetrieveColumnGrbit grbit)
        {
            unsafe
            {
                const int DataSize = 16;
                Guid data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = Api.JetRetrieveColumn(sesid, tableid, columnid, pointer, DataSize, out actualDataSize, grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>
        /// Retrieves a datetime column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column as a datetime. Null if the column is null.</returns>
        public static DateTime? RetrieveColumnAsDateTime(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return Api.RetrieveColumnAsDateTime(sesid, tableid, columnid, RetrieveColumnGrbit.None);
        }

        /// <summary>
        /// Retrieves a datetime column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as a datetime. Null if the column is null.</returns>
        public static DateTime? RetrieveColumnAsDateTime(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, RetrieveColumnGrbit grbit)
        {
            // Internally DateTime is stored in OLE Automation format
            double? oadate = Api.RetrieveColumnAsDouble(sesid, tableid, columnid, grbit);
            return oadate.HasValue ? DateTime.FromOADate(oadate.Value) : new DateTime?();
        }

        /// <summary>
        /// Retrieves a uint16 column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column as an UInt16. Null if the column is null.</returns>
        /// <remarks>Internal becaue unsigned types aren't CLS compliant.</remarks>
        internal static ushort? RetrieveColumnAsUInt16(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return Api.RetrieveColumnAsUInt16(sesid, tableid, columnid, RetrieveColumnGrbit.None);
        }

        /// <summary>
        /// Retrieves a uint16 column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as an UInt16. Null if the column is null.</returns>
        /// <remarks>Internal becaue unsigned types aren't CLS compliant.</remarks>
        internal static ushort? RetrieveColumnAsUInt16(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, RetrieveColumnGrbit grbit)
        {
            unsafe
            {
                const int DataSize = 2;
                ushort data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = Api.JetRetrieveColumn(sesid, tableid, columnid, pointer, DataSize, out actualDataSize, grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>
        /// Retrieves a uint32 column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column as an UInt32. Null if the column is null.</returns>
        /// <remarks>Internal becaue unsigned types aren't CLS compliant.</remarks>
        internal static uint? RetrieveColumnAsUInt32(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return Api.RetrieveColumnAsUInt32(sesid, tableid, columnid, RetrieveColumnGrbit.None);
        }

        /// <summary>
        /// Retrieves a uint32 column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as an UInt32. Null if the column is null.</returns>
        /// <remarks>Internal becaue unsigned types aren't CLS compliant.</remarks>
        internal static uint? RetrieveColumnAsUInt32(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, RetrieveColumnGrbit grbit)
        {
            unsafe
            {
                const int DataSize = 4;
                uint data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = Api.JetRetrieveColumn(sesid, tableid, columnid, pointer, DataSize, out actualDataSize, grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
        }

        /// <summary>
        /// Retrieves a uint64 column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <returns>The data retrieved from the column as an UInt64. Null if the column is null.</returns>
        /// <remarks>Internal becaue unsigned types aren't CLS compliant.</remarks>
        internal static ulong? RetrieveColumnAsUInt64(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            return Api.RetrieveColumnAsUInt64(sesid, tableid, columnid, RetrieveColumnGrbit.None);
        }

        /// <summary>
        /// Retrieves a uint64 column value from the current record. The record is that
        /// record associated with the index entry at the current position of the cursor.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to retrieve the column from.</param>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieval options.</param>
        /// <returns>The data retrieved from the column as an UInt64. Null if the column is null.</returns>
        /// <remarks>Internal becaue unsigned types aren't CLS compliant.</remarks>
        internal static ulong? RetrieveColumnAsUInt64(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, RetrieveColumnGrbit grbit)
        {
            unsafe
            {
                const int DataSize = 8;
                ulong data;
                var pointer = new IntPtr(&data);
                int actualDataSize;
                JET_wrn wrn = Api.JetRetrieveColumn(sesid, tableid, columnid, pointer, DataSize, out actualDataSize, grbit, null);
                return CreateReturnValue(data, DataSize, wrn, actualDataSize);
            }
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

        /// <summary>
        /// Create the nullable return value.
        /// </summary>
        /// <typeparam name="T">The (struct) type to return.</typeparam>
        /// <param name="data">The data retrieved from the column.</param>
        /// <param name="dataSize">The size of the data.</param>
        /// <param name="wrn">The warning code from esent.</param>
        /// <param name="actualDataSize">The actual size of the data retireved fomr esent.</param>
        /// <returns>A nullable struct of type T.</returns>
        private static T? CreateReturnValue<T>(T data, int dataSize, JET_wrn wrn, int actualDataSize) where T : struct
        {
            if (JET_wrn.ColumnNull == wrn)
            {
                return new T?();
            }

            CheckDataSize(dataSize, actualDataSize);
            return data;
        }

        /// <summary>
        /// Make sure the retrieved data size is at least as long as the expected size.
        /// An exception is thrown if the data isn't long enough.
        /// </summary>
        /// <param name="expectedDataSize">The expected data size.</param>
        /// <param name="actualDataSize">The size of the retrieved data.</param>
        private static void CheckDataSize(int expectedDataSize, int actualDataSize)
        {
            if (actualDataSize < expectedDataSize)
            {
                throw new EsentInvalidColumnException();
            }
        }
    }
}
