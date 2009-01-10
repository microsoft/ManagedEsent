//-----------------------------------------------------------------------
// <copyright file="ColumnStream.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;
    using System.IO;

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
        private int offset;

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
            this.offset = 0;
            this.Itag = 1;
        }

        /// <summary>
        /// Gets the chunk size a long-value is stored in. A column
        /// stream should be buffered in multiples of this size.
        /// </summary>
        public int ChunkSize
        {
            get
            {
                int columnLVPageOverhead = 82;  // from esent.h
                int pageSize = 0;
                string ignored;
                if (0 == ErrApi.JetGetSystemParameter(JET_INSTANCE.Nil, JET_SESID.Nil, JET_param.DatabasePageSize, ref pageSize, out ignored, 0))
                {
                    return pageSize - columnLVPageOverhead;
                }

                // Assume 4kb pages
                return 4096 - columnLVPageOverhead;
            }
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
                return this.offset;
            }

            set
            {
                if (value < 0 || value > 0x7fffffff)
                {
                    throw new ArgumentOutOfRangeException("position", value, "A long-value offset has to be between 0 and 0x7fffffff bytes");
                }

                if (value > this.Length)
                {
                    this.SetLength(value);
                }

                this.offset = (int)value;
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
                Api.JetRetrieveColumn(this.sesid, this.tableid, this.columnid, null, 0, out size, this.RetrieveGrbit, retinfo);
                return size;
            }
        }

        /// <summary>
        /// Gets the options that should be used with JetRetrieveColumn
        /// </summary>
        private RetrieveColumnGrbit RetrieveGrbit
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
            if (buffer.Length - offset < count)
            {
                throw new ArgumentException("buffer is not large enough");
            }

            var setinfo = new JET_SETINFO() { itagSequence = this.Itag, ibLongValue = this.offset };
            if (0 == offset)
            {
                Api.JetSetColumn(this.sesid, this.tableid, this.columnid, buffer, count, SetColumnGrbit.None, setinfo);
            }
            else
            {
                byte[] offsetBuffer = new byte[count - offset];
                Array.Copy(buffer, offset, offsetBuffer, 0, count);
                Api.JetSetColumn(this.sesid, this.tableid, this.columnid, offsetBuffer, count, SetColumnGrbit.None, setinfo);
            }

            this.offset += count;
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
            if (buffer.Length - offset < count)
            {
                throw new ArgumentException("buffer is not large enough");
            }

            int bytesRead;
            var retinfo = new JET_RETINFO() { itagSequence = this.Itag, ibLongValue = this.offset };
            if (0 == offset)
            {
                Api.JetRetrieveColumn(this.sesid, this.tableid, this.columnid, buffer, count, out bytesRead, this.RetrieveGrbit, retinfo);
            }
            else
            {
                byte[] offsetBuffer = new byte[count - offset];
                Api.JetRetrieveColumn(this.sesid, this.tableid, this.columnid, offsetBuffer, count, out bytesRead, this.RetrieveGrbit, retinfo);
                Array.Copy(offsetBuffer, 0, buffer, offset, bytesRead);
            }

            this.offset += bytesRead;
            return bytesRead;
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

            Api.JetSetColumn(this.sesid, this.tableid, this.columnid, null, (int)value, SetColumnGrbit.SizeLV, null);
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
                    newOffset = this.Length - offset;
                    break;
                case SeekOrigin.Current:
                    newOffset = this.offset + offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("origin", origin, "Unknown origin");
            }

            if (newOffset < 0 || newOffset > 0x7fffffff)
            {
                throw new ArgumentOutOfRangeException("offset", offset, "invalid offset/origin combination");
            }

            if (newOffset > this.Length)
            {
                this.SetLength(newOffset);
            }

            this.offset = (int)newOffset;
            return this.offset;
        }
   }
}
