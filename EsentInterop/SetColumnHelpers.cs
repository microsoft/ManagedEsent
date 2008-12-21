//-----------------------------------------------------------------------
// <copyright file="SetColumnHelpers.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;
    using System.Text;

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
        public static void SetColumn(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid, string data, Encoding encoding)
        {
            if (null == data)
            {
                Api.JetSetColumn(sesid, tableid, columnid, null, 0, SetColumnGrbit.None, null);
            }
            else
            {
                byte[] bytes = encoding.GetBytes(data);
                Api.JetSetColumn(sesid, tableid, columnid, bytes, bytes.Length, SetColumnGrbit.None, null);
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
            int dataLength = (null == data) ? 0 : data.Length;
            Api.JetSetColumn(sesid, tableid, columnid, data, dataLength, SetColumnGrbit.None, null);
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
            byte[] bytes = data ? new byte[] { 0xff } : new byte[] { 0x0 };
            Api.JetSetColumn(sesid, tableid, columnid, bytes, bytes.Length, SetColumnGrbit.None, null);
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
            byte[] bytes = new byte[] { data };
            Api.JetSetColumn(sesid, tableid, columnid, bytes, bytes.Length, SetColumnGrbit.None, null);
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
            byte[] bytes = BitConverter.GetBytes(data);
            Api.JetSetColumn(sesid, tableid, columnid, bytes, bytes.Length, SetColumnGrbit.None, null);
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
            byte[] bytes = BitConverter.GetBytes(data);
            Api.JetSetColumn(sesid, tableid, columnid, bytes, bytes.Length, SetColumnGrbit.None, null);
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
            byte[] bytes = BitConverter.GetBytes(data);
            Api.JetSetColumn(sesid, tableid, columnid, bytes, bytes.Length, SetColumnGrbit.None, null);
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
            Api.JetSetColumn(sesid, tableid, columnid, bytes, bytes.Length, SetColumnGrbit.None, null);
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
            byte[] bytes = BitConverter.GetBytes(data);
            Api.JetSetColumn(sesid, tableid, columnid, bytes, bytes.Length, SetColumnGrbit.None, null);
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
            byte[] bytes = BitConverter.GetBytes(data);
            Api.JetSetColumn(sesid, tableid, columnid, bytes, bytes.Length, SetColumnGrbit.None, null);
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
            byte[] bytes = BitConverter.GetBytes(data);
            Api.JetSetColumn(sesid, tableid, columnid, bytes, bytes.Length, SetColumnGrbit.None, null);
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
            byte[] bytes = BitConverter.GetBytes(data);
            Api.JetSetColumn(sesid, tableid, columnid, bytes, bytes.Length, SetColumnGrbit.None, null);
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
            byte[] bytes = BitConverter.GetBytes(data);
            Api.JetSetColumn(sesid, tableid, columnid, bytes, bytes.Length, SetColumnGrbit.None, null);
        }
    }
}