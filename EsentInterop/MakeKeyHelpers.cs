//-----------------------------------------------------------------------
// <copyright file="MakeKeyHelpers.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Text;

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// Helper methods for the ESENT API. These do data conversion for
    /// JetMakeKey.
    /// </summary>
    public static partial class Api
    {
        /// <summary>
        /// Constructs a search key that may then be used by <see cref="JetSeek"/>
        /// and <see cref="JetSetIndexRange"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to create the key on.</param>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public static void MakeKey(JET_SESID sesid, JET_TABLEID tableid, byte[] data, MakeKeyGrbit grbit)
        {
            if (null == data)
            {
                Api.JetMakeKey(sesid, tableid, null, 0, grbit);
            }
            else if (0 == data.Length)
            {
                Api.JetMakeKey(sesid, tableid, data, data.Length, grbit | MakeKeyGrbit.KeyDataZeroLength);                
            }
            else
            {
                Api.JetMakeKey(sesid, tableid, data, data.Length, grbit);
            }
        }

        /// <summary>
        /// Constructs a search key that may then be used by <see cref="JetSeek"/>
        /// and <see cref="JetSetIndexRange"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to create the key on.</param>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="encoding">The encoding used to convert the string.</param>
        /// <param name="grbit">Key options.</param>
        public static void MakeKey(JET_SESID sesid, JET_TABLEID tableid, string data, Encoding encoding, MakeKeyGrbit grbit)
        {
            CheckEncodingIsValid(encoding);

            if (null == data)
            {
                Api.JetMakeKey(sesid, tableid, null, 0, grbit);
            }
            else if (0 == data.Length)
            {
                Api.JetMakeKey(sesid, tableid, null, 0, grbit | MakeKeyGrbit.KeyDataZeroLength);
            }
            else if (Encoding.Unicode == encoding)
            {
                // Optimization for Unicode strings
                unsafe
                {
                    fixed (char* buffer = data)
                    {
                        Api.JetMakeKey(sesid, tableid, (IntPtr) buffer, data.Length * sizeof(char), grbit);
                    }
                }
            }
            else
            {
                byte[] bytes = encoding.GetBytes(data);
                Api.JetMakeKey(sesid, tableid, bytes, bytes.Length, grbit);
            }
        }

        /// <summary>
        /// Constructs a search key that may then be used by <see cref="JetSeek"/>
        /// and <see cref="JetSetIndexRange"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to create the key on.</param>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public static void MakeKey(JET_SESID sesid, JET_TABLEID tableid, bool data, MakeKeyGrbit grbit)
        {
            byte b = data ? (byte)0xff : (byte)0x0;
            Api.MakeKey(sesid, tableid, b, grbit);
        }

        /// <summary>
        /// Constructs a search key that may then be used by <see cref="JetSeek"/>
        /// and <see cref="JetSetIndexRange"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to create the key on.</param>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public static void MakeKey(JET_SESID sesid, JET_TABLEID tableid, byte data, MakeKeyGrbit grbit)
        {
            unsafe
            {
                const int DataSize = 1;
                var pointer = new IntPtr(&data);
                Api.JetMakeKey(sesid, tableid, pointer, DataSize, grbit);
            }
        }

        /// <summary>
        /// Constructs a search key that may then be used by <see cref="JetSeek"/>
        /// and <see cref="JetSetIndexRange"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to create the key on.</param>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public static void MakeKey(JET_SESID sesid, JET_TABLEID tableid, short data, MakeKeyGrbit grbit)
        {
            unsafe
            {
                const int DataSize = 2;
                var pointer = new IntPtr(&data);
                Api.JetMakeKey(sesid, tableid, pointer, DataSize, grbit);
            }
        }

        /// <summary>
        /// Constructs a search key that may then be used by <see cref="JetSeek"/>
        /// and <see cref="JetSetIndexRange"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to create the key on.</param>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public static void MakeKey(JET_SESID sesid, JET_TABLEID tableid, int data, MakeKeyGrbit grbit)
        {
            unsafe
            {
                const int DataSize = 4;
                var pointer = new IntPtr(&data);
                Api.JetMakeKey(sesid, tableid, pointer, DataSize, grbit);
            }
        }

        /// <summary>
        /// Constructs a search key that may then be used by <see cref="JetSeek"/>
        /// and <see cref="JetSetIndexRange"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to create the key on.</param>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public static void MakeKey(JET_SESID sesid, JET_TABLEID tableid, long data, MakeKeyGrbit grbit)
        {
            unsafe
            {
                const int DataSize = 8;
                var pointer = new IntPtr(&data);
                Api.JetMakeKey(sesid, tableid, pointer, DataSize, grbit);
            }
        }

        /// <summary>
        /// Constructs a search key that may then be used by <see cref="JetSeek"/>
        /// and <see cref="JetSetIndexRange"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to create the key on.</param>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public static void MakeKey(JET_SESID sesid, JET_TABLEID tableid, Guid data, MakeKeyGrbit grbit)
        {
            byte[] bytes = data.ToByteArray();
            Api.JetMakeKey(sesid, tableid, bytes, bytes.Length, grbit);
        }

        /// <summary>
        /// Constructs a search key that may then be used by <see cref="JetSeek"/>
        /// and <see cref="JetSetIndexRange"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to create the key on.</param>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public static void MakeKey(JET_SESID sesid, JET_TABLEID tableid, DateTime data, MakeKeyGrbit grbit)
        {
            Api.MakeKey(sesid, tableid, data.ToOADate(), grbit);
        }

        /// <summary>
        /// Constructs a search key that may then be used by <see cref="JetSeek"/>
        /// and <see cref="JetSetIndexRange"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to create the key on.</param>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public static void MakeKey(JET_SESID sesid, JET_TABLEID tableid, float data, MakeKeyGrbit grbit)
        {
            unsafe
            {
                const int DataSize = 4;
                var pointer = new IntPtr(&data);
                Api.JetMakeKey(sesid, tableid, pointer, DataSize, grbit);
            }
        }

        /// <summary>
        /// Constructs a search key that may then be used by <see cref="JetSeek"/>
        /// and <see cref="JetSetIndexRange"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to create the key on.</param>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        public static void MakeKey(JET_SESID sesid, JET_TABLEID tableid, double data, MakeKeyGrbit grbit)
        {
            unsafe
            {
                const int DataSize = 8;
                var pointer = new IntPtr(&data);
                Api.JetMakeKey(sesid, tableid, pointer, DataSize, grbit);
            }
        }

        /// <summary>
        /// Constructs a search key that may then be used by <see cref="JetSeek"/>
        /// and <see cref="JetSetIndexRange"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to create the key on.</param>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        internal static void MakeKey(JET_SESID sesid, JET_TABLEID tableid, ushort data, MakeKeyGrbit grbit)
        {
            unsafe
            {
                const int DataSize = 2;
                var pointer = new IntPtr(&data);
                Api.JetMakeKey(sesid, tableid, pointer, DataSize, grbit);
            }
        }

        /// <summary>
        /// Constructs a search key that may then be used by <see cref="JetSeek"/>
        /// and <see cref="JetSetIndexRange"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to create the key on.</param>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        internal static void MakeKey(JET_SESID sesid, JET_TABLEID tableid, uint data, MakeKeyGrbit grbit)
        {
            unsafe
            {
                const int DataSize = 4;
                var pointer = new IntPtr(&data);
                Api.JetMakeKey(sesid, tableid, pointer, DataSize, grbit);
            }
        }

        /// <summary>
        /// Constructs a search key that may then be used by <see cref="JetSeek"/>
        /// and <see cref="JetSetIndexRange"/>.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to create the key on.</param>
        /// <param name="data">Column data for the current key column of the current index.</param>
        /// <param name="grbit">Key options.</param>
        internal static void MakeKey(JET_SESID sesid, JET_TABLEID tableid, ulong data, MakeKeyGrbit grbit)
        {
            unsafe
            {
                const int DataSize = 8;
                var pointer = new IntPtr(&data);
                Api.JetMakeKey(sesid, tableid, pointer, DataSize, grbit);
            }
        }
    }
}