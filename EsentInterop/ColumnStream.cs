//-----------------------------------------------------------------------
// <copyright file="ColumnStream.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Isam.Esent.Interop.Windows7;

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// This class provides a streaming interface to a long-value column
    /// (i.e. a column of type JET_coltyp.LongBinary or JET_coltyp.LongText).
    /// </summary>
    public class ColumnStream : Stream
    {
        /// <summary>
        /// Session to use.
        /// </summary>
        private readonly JET_SESID sesid;

        /// <summary>
        /// Cursor to use.
        /// </summary>
        private readonly JET_TABLEID tableid;

        /// <summary>
        /// Columnid to use.
        /// </summary>
        private readonly JET_COLUMNID columnid;

        /// <summary>
        /// Current LV offset.
        /// </summary>
        private int ibLongValue;

        /// <summary>
        /// Initializes a new instance of the ColumnStream class.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">The cursor to use.</param>
        /// <param name="columnid">The columnid of the column to set/retrieve data from.</param>
        public ColumnStream(JET_SESID sesid, JET_TABLEID tableid, JET_COLUMNID columnid)
        {
            this.sesid = sesid;
            this.tableid = tableid;
            this.columnid = columnid;
            this.ibLongValue = 0;
            this.Itag = 1;
        }

        /// <summary>
        /// Gets or sets the itag of the column.
        /// </summary>
        public int Itag { get; set; }

        /// <summary>
        /// Gets a value indicating whether the stream supports reading.
        /// </summary>
        public override bool CanRead
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the stream supports writing.
        /// </summary>
        public override bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the stream supports seeking.
        /// </summary>
        public override bool CanSeek
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets the current position in the stream.
        /// </summary>
        public override long Position
        {
            get
            {
                return this.ibLongValue;
            }

            set
            {
                if (value < 0 || value > 0x7fffffff)
                {
                    throw new ArgumentOutOfRangeException("value", value, "A long-value offset has to be between 0 and 0x7fffffff bytes");
                }

                this.ibLongValue = (int)value;
            }
        }

        /// <summary>
        /// Gets the current length of the stream.
        /// </summary>
        public override long Length
        {
            get
            {
                int size;
                var retinfo = new JET_RETINFO() { itagSequence = this.Itag, ibLongValue = 0 };
                Api.JetRetrieveColumn(this.sesid, this.tableid, this.columnid, null, 0, out size, RetrieveGrbit, retinfo);
                return size;
            }
        }

        /// <summary>
        /// Gets the options that should be used with JetRetrieveColumn
        /// </summary>
        private static RetrieveColumnGrbit RetrieveGrbit
        {
            get
            {
                // Always use the RetrieveCopy options. This makes the ColumnStream work
                // well when setting a column. If we don't always use RetrieveCopy then
                // things like seeking from the end of a column might not work properly.
                return RetrieveColumnGrbit.RetrieveCopy;
            }
        }

        /// <summary>
        /// Flush the stream.
        /// </summary>
        public override void Flush()
        {
            // nothing is required
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current
        /// position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">The buffer to write from.</param>
        /// <param name="offset">The offset in the buffer to write.</param>
        /// <param name="count">The number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckBufferArguments(buffer, offset, count);

            int length = (int)this.Length;
            JET_SETINFO setinfo;

            // If our current position is beyond the end of the LV extend
            // the LV to the write point
            if (this.ibLongValue > length)
            {
                setinfo = new JET_SETINFO { itagSequence = this.Itag };
                Api.JetSetColumn(this.sesid, this.tableid, this.columnid, null, this.ibLongValue, SetColumnGrbit.SizeLV, setinfo);
                length = this.ibLongValue;
            }

            int newIbLongValue = this.ibLongValue + count;
            SetColumnGrbit grbit;
            /*if (this.ibLongValue == length)
            {
                grbit = SetColumnGrbit.AppendLV;
            }
            else */ if (newIbLongValue >= length)
            {
                grbit = SetColumnGrbit.OverwriteLV | SetColumnGrbit.SizeLV;
            }
            else
            {
                grbit = SetColumnGrbit.OverwriteLV;
            }

            setinfo = new JET_SETINFO { itagSequence = this.Itag, ibLongValue = this.ibLongValue };
            if (0 == offset)
            {
                Api.JetSetColumn(this.sesid, this.tableid, this.columnid, buffer, count, grbit, setinfo);
            }
            else
            {
                var offsetBuffer = new byte[count];
                Array.Copy(buffer, offset, offsetBuffer, 0, count);
                Api.JetSetColumn(this.sesid, this.tableid, this.columnid, offsetBuffer, count, grbit, setinfo);
            }

            this.ibLongValue += count;
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the 
        /// position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">The buffer to read into.</param>
        /// <param name="offset">The offset in the buffer to read into.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The number of bytes read into the buffer.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckBufferArguments(buffer, offset, count);

            if (this.ibLongValue >= this.Length)
            {
                return 0;
            }

            int bytesToRead = (int)Math.Min(this.Length - this.ibLongValue, count);

            int ignored;
            var retinfo = new JET_RETINFO { itagSequence = this.Itag, ibLongValue = this.ibLongValue };
            if (0 == offset)
            {
                Api.JetRetrieveColumn(this.sesid, this.tableid, this.columnid, buffer, bytesToRead, out ignored, RetrieveGrbit, retinfo);
            }
            else
            {
                var offsetBuffer = new byte[bytesToRead];
                Api.JetRetrieveColumn(this.sesid, this.tableid, this.columnid, offsetBuffer, bytesToRead, out ignored, RetrieveGrbit, retinfo);
                Array.Copy(offsetBuffer, 0, buffer, offset, bytesToRead);
            }

            this.ibLongValue += bytesToRead;
            return bytesToRead;
        }

        /// <summary>
        /// Sets the length of the stream.
        /// </summary>
        /// <param name="value">The desired length, in bytes.</param>
        public override void SetLength(long value)
        {
            if (value > 0x7FFFFFFF || value < 0)
            {
                throw new ArgumentOutOfRangeException("value", value, "A LongValueStream cannot be longer than 0x7FFFFFF or less than 0 bytes");
            }

            var setinfo = new JET_SETINFO { itagSequence = this.Itag };
            SetColumnGrbit grbit = (0 == value) ? SetColumnGrbit.ZeroLength : SetColumnGrbit.SizeLV;
            Api.JetSetColumn(this.sesid, this.tableid, this.columnid, null, (int)value, grbit, setinfo);

            // Setting the length moves the offset back to the end of the data
            if (this.ibLongValue > value)
            {
                this.ibLongValue = (int)value;
            }
        }

        /// <summary>
        /// Sets the position in the current stream.
        /// </summary>
        /// <param name="offset">Byte offset relative to the origin parameter.</param>
        /// <param name="origin">A SeekOrigin indicating the reference point for the new position.</param>
        /// <returns>The new position in the current stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            long newOffset;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    newOffset = offset;
                    break;
                case SeekOrigin.End:
                    newOffset = this.Length + offset;
                    break;
                case SeekOrigin.Current:
                    newOffset = this.ibLongValue + offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("origin", origin, "Unknown origin");
            }

            if (newOffset < 0 || newOffset > 0x7fffffff)
            {
                throw new ArgumentOutOfRangeException("offset", offset, "invalid offset/origin combination");
            }

            this.ibLongValue = (int)newOffset;
            return this.ibLongValue;
        }

        /// <summary>
        /// Check the buffer arguments given to Read/Write 
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset in the buffer to read/write to.</param>
        /// <param name="count">The number of bytes to read/write.</param>
        private static void CheckBufferArguments(byte[] buffer, int offset, int count)
        {
            if (null == buffer)
            {
                throw new ArgumentNullException("buffer");
            }

            if (buffer.Length - offset < count)
            {
                throw new ArgumentOutOfRangeException("count", count, "cannot be larger than the size of the buffer");
            }
        }
   }
}
