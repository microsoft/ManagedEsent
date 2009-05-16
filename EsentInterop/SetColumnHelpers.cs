//-----------------------------------------------------------------------
// <copyright file="SetColumnHelpers.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// Helper methods for the ESENT API. These do data conversion for
    /// setting columns.
    /// The methods that operate on unsigned numbers are
    /// internal so that the public API remains CLS compliant.
    /// </summary>
    public static partial class Api
    {
        /// <summary>
        /// Modifies a single column value in a modified record to be inserted or to
        /// update the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        /// <param name="encoding">The encoding used to convert the string.</param>
        public static void SetColumn(
            JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, string data, Encoding encoding)
        {
            CheckEncodingIsValid(encoding);

            if (null == data)
            {
                JetSetColumn(sesid, tableid, columnid, null, 0, SetColumnGrbit.None, null);
            }
            else if (0 == data.Length)
            {
                JetSetColumn(sesid, tableid, columnid, null, 0, SetColumnGrbit.ZeroLength, null);
            }
            else if (Encoding.Unicode == encoding)
            {
                // Optimization for setting Unicode strings.
                unsafe
                {
                    fixed (char* buffer = data)
                    {
                        JetSetColumn(
                            sesid,
                            tableid,
                            columnid,
                            new IntPtr(buffer),
                            data.Length * sizeof(char),
                            SetColumnGrbit.None,
                            null);
                    }
                }
            }
            else
            {
                byte[] bytes = encoding.GetBytes(data);
                JetSetColumn(sesid, tableid, columnid, bytes, bytes.Length, SetColumnGrbit.None, null);
            }
        }

        /// <summary>
        /// Modifies a single column value in a modified record to be inserted or to
        /// update the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, byte[] data)
        {
            SetColumnGrbit grbit = ((null != data) && (0 == data.Length))
                                       ? SetColumnGrbit.ZeroLength
                                       : SetColumnGrbit.None;
            int dataLength = (null == data) ? 0 : data.Length;
            JetSetColumn(sesid, tableid, columnid, data, dataLength, grbit, null);
        }

        /// <summary>
        /// Modifies a single column value in a modified record to be inserted or to
        /// update the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, bool data)
        {
            byte b = data ? (byte) 0xff : (byte) 0x0;
            SetColumn(sesid, tableid, columnid, b);
        }

        /// <summary>
        /// Modifies a single column value in a modified record to be inserted or to
        /// update the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, byte data)
        {
            unsafe
            {
                const int DataSize = sizeof(byte);
                var pointer = new IntPtr(&data);
                JetSetColumn(sesid, tableid, columnid, pointer, DataSize, SetColumnGrbit.None, null);
            }
        }

        /// <summary>
        /// Modifies a single column value in a modified record to be inserted or to
        /// update the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, short data)
        {
            unsafe
            {
                const int DataSize = sizeof(short);
                var pointer = new IntPtr(&data);
                JetSetColumn(sesid, tableid, columnid, pointer, DataSize, SetColumnGrbit.None, null);
            }
        }

        /// <summary>
        /// Modifies a single column value in a modified record to be inserted or to
        /// update the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, int data)
        {
            unsafe
            {
                const int DataSize = sizeof(int);
                var pointer = new IntPtr(&data);
                JetSetColumn(sesid, tableid, columnid, pointer, DataSize, SetColumnGrbit.None, null);
            }
        }

        /// <summary>
        /// Modifies a single column value in a modified record to be inserted or to
        /// update the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, long data)
        {
            unsafe
            {
                const int DataSize = sizeof(long);
                var pointer = new IntPtr(&data);
                JetSetColumn(sesid, tableid, columnid, pointer, DataSize, SetColumnGrbit.None, null);
            }
        }

        /// <summary>
        /// Modifies a single column value in a modified record to be inserted or to
        /// update the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, Guid data)
        {
            byte[] bytes = data.ToByteArray();
            JetSetColumn(sesid, tableid, columnid, bytes, bytes.Length, SetColumnGrbit.None, null);
        }

        /// <summary>
        /// Modifies a single column value in a modified record to be inserted or to
        /// update the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, DateTime data)
        {
            SetColumn(sesid, tableid, columnid, data.ToOADate());
        }

        /// <summary>
        /// Modifies a single column value in a modified record to be inserted or to
        /// update the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, float data)
        {
            unsafe
            {
                const int DataSize = sizeof(float);
                var pointer = new IntPtr(&data);
                JetSetColumn(sesid, tableid, columnid, pointer, DataSize, SetColumnGrbit.None, null);
            }
        }

        /// <summary>
        /// Modifies a single column value in a modified record to be inserted or to
        /// update the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, double data)
        {
            unsafe
            {
                const int DataSize = sizeof(double);
                var pointer = new IntPtr(&data);
                JetSetColumn(sesid, tableid, columnid, pointer, DataSize, SetColumnGrbit.None, null);
            }
        }

        /// <summary>
        /// Perform atomic addition on one column. The column must be of type
        /// JET_coltyp.Long. This function allows multiple sessions to update the
        /// same record concurrently without conflicts.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update.</param>
        /// <param name="columnid">The column to update. This must be an escrow-updatable column.</param>
        /// <param name="delta">The delta to apply to the column.</param>
        /// <returns>The current value of the column as stored in the database (versioning is ignored).</returns>
        public static int EscrowUpdate(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, int delta)
        {
            var previousValue = new byte[sizeof(int)];
            int actualPreviousValueLength;
            JetEscrowUpdate(
                sesid,
                tableid,
                columnid,
                BitConverter.GetBytes(delta),
                sizeof(int),
                previousValue,
                previousValue.Length,
                out actualPreviousValueLength,
                EscrowUpdateGrbit.None);
            Debug.Assert(
                previousValue.Length == actualPreviousValueLength,
                "Unexpected previous value length. Expected an Int32");
            return BitConverter.ToInt32(previousValue, 0);
        }

        /// <summary>
        /// Modifies a single column value in a modified record to be inserted or to
        /// update the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        internal static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, ushort data)
        {
            unsafe
            {
                const int DataSize = sizeof(ushort);
                var pointer = new IntPtr(&data);
                JetSetColumn(sesid, tableid, columnid, pointer, DataSize, SetColumnGrbit.None, null);
            }
        }

        /// <summary>
        /// Modifies a single column value in a modified record to be inserted or to
        /// update the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        internal static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, uint data)
        {
            unsafe
            {
                const int DataSize = sizeof(uint);
                var pointer = new IntPtr(&data);
                JetSetColumn(sesid, tableid, columnid, pointer, DataSize, SetColumnGrbit.None, null);
            }
        }

        /// <summary>
        /// Modifies a single column value in a modified record to be inserted or to
        /// update the current record.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to update. An update should be prepared.</param>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        internal static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, ulong data)
        {
            unsafe
            {
                const int DataSize = sizeof(ulong);
                var pointer = new IntPtr(&data);
                JetSetColumn(sesid, tableid, columnid, pointer, DataSize, SetColumnGrbit.None, null);
            }
        }

        /// <summary>
        /// Verifies that the given encoding is valid for setting/retrieving data. Only
        /// the ASCII and Unicode encodings are allowed. An EsentException is thrown if
        /// the encoding isn't valid.
        /// </summary>
        /// <param name="encoding">The encoding to check.</param>
        private static void CheckEncodingIsValid(Encoding encoding)
        {
            if (!((encoding is ASCIIEncoding) || (encoding is UnicodeEncoding)))
            {
                throw new EsentException("Invalid Encoding type. Only ASCII and Unicode encodings are allowed");
            }
        }
    }
}