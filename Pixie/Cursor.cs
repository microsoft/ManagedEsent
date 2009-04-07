//-----------------------------------------------------------------------
// <copyright file="Cursor.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Isam.Esent.Interop;

namespace Microsoft.Isam.Esent
{
    /// <summary>
    /// A cursor object is used to move through a table.
    /// </summary>
    internal class Cursor : IDisposable
    {
        /// <summary>
        /// Tracing object 
        /// </summary>
        private readonly Tracer tracer;

        /// <summary>
        /// The JET_SESID associated with the table.
        /// </summary>
        private readonly JET_SESID sesid;

        /// <summary>
        /// The JET_DBID associated with the table.
        /// </summary>
        private readonly JET_DBID dbid;

        /// <summary>
        /// The name of the table.
        /// </summary>
        private readonly string tablename;

        /// <summary>
        /// The JET_TABLEID owned by the cursor.
        /// </summary>
        private JET_TABLEID tableid;

        /// <summary>
        /// True if the cursor is positioned on a record.
        /// </summary>
        private bool hasCurrency;

        /// <summary>
        /// True if the cursor has been disposed.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// True if the cursor is currently in an update.
        /// </summary>
        private bool inUpdate;

        /// <summary>
        /// Initializes a new instance of the Cursor class.
        /// </summary>
        /// <param name="sesid">The session to use for the cursor.</param>
        /// <param name="dbid">The database containing the table.</param>
        /// <param name="tablename">The name of the table.</param>
        public Cursor(JET_SESID sesid, JET_DBID dbid, string tablename)
        {
            this.tracer = new Tracer("Cursor", "Esent Cursor object", String.Format("Cursor {0}", tablename));

            this.sesid = sesid;
            this.dbid = dbid;
            this.tablename = tablename;

            Api.JetOpenTable(this.sesid, this.dbid, this.tablename, null, 0, OpenTableGrbit.None, out this.tableid);
            this.Tracer.TraceVerbose("opened");
        }

        /// <summary>
        /// Occurs when the cursor is disposed.
        /// </summary>
        public virtual event Action<Cursor> Disposed;

        /// <summary>
        /// Gets the tracing object for this cursor.
        /// </summary>
        private Tracer Tracer
        {
            get
            {
                return this.tracer;
            }
        }

        /// <summary>
        /// Closes the JET_TABLEID opened by this cursor.
        /// </summary>
        public virtual void Dispose()
        {
            if (!this.isDisposed)
            {
                this.Tracer.TraceVerbose("dispose");
                this.CancelUpdate();

                if (null != this.Disposed)
                {
                    this.Disposed(this);
                }

                Api.JetCloseTable(this.sesid, this.tableid);
                this.tableid = JET_TABLEID.Nil;
                this.isDisposed = true;

                GC.SuppressFinalize(this);
            }
        }

        #region MetaData

        /// <summary>
        /// Iterates over all the columns in the table, returning information about each one.
        /// </summary>
        /// <returns>An iterator over ColumnInfo for each column in the table.</returns>
        public virtual IEnumerable<ColumnInfo> GetColumns()
        {
            this.CheckNotDisposed();
            return Api.GetTableColumns(this.sesid, this.tableid);
        }

        /// <summary>
        /// Add a column to the cursor.
        /// </summary>
        /// <param name="column">The name of the column.</param>
        /// <param name="columndef">The columndefinition.</param>
        /// <param name="defaultValue">Default value of the column.</param>
        /// <param name="columnid">The columnid of the column.</param>
        public virtual void AddColumn(string column, JET_COLUMNDEF columndef, byte[] defaultValue, out JET_COLUMNID columnid)
        {
            this.CheckNotDisposed();
            if (null == defaultValue)
            {
                Api.JetAddColumn(this.sesid, this.tableid, column, columndef, null, 0, out columnid);                
            }
            else
            {
                Api.JetAddColumn(this.sesid, this.tableid, column, columndef, defaultValue, defaultValue.Length, out columnid);                
            }
        }

        #endregion MetaData

        #region Navigation

        /// <summary>
        /// Set the current index for the cursor to the specified index. 
        /// </summary>
        /// <param name="index">The name of the index.</param>
        public virtual void SetCurrentIndex(string index)
        {
            Api.JetSetCurrentIndex(this.sesid, this.tableid, index);
        }

        /// <summary>
        /// Set the cursor to sequential scan mode.
        /// </summary>
        public virtual void SetSequential()
        {
            Api.JetSetTableSequential(this.sesid, this.tableid, SetTableSequentialGrbit.None);
        }

        /// <summary>
        /// Reset sequential mode.
        /// </summary>
        public virtual void ResetSequential()
        {
            Api.JetResetTableSequential(this.sesid, this.tableid, ResetTableSequentialGrbit.None);
        }

        /// <summary>
        /// Move to the first record in the table.
        /// </summary>
        /// <returns>True if the Cursor is positioned on a record.</returns>
        public virtual bool TryMoveFirst()
        {
            this.Tracer.TraceVerbose("TryMoveFirst");
            this.CheckNotDisposed();
            this.CancelUpdate();

            this.hasCurrency = Api.TryMoveFirst(this.sesid, this.tableid);
            return this.hasCurrency;
        }

        /// <summary>
        /// Move to the last record in the table.
        /// </summary>
        /// <returns>True if the Cursor is positioned on a record.</returns>
        public virtual bool TryMoveLast()
        {
            this.Tracer.TraceVerbose("TryMoveLast");
            this.CheckNotDisposed();
            this.CancelUpdate();
            
            this.hasCurrency = Api.TryMoveLast(this.sesid, this.tableid);
            return this.hasCurrency;
        }

        /// <summary>
        /// Move to the next record in the table.
        /// </summary>
        /// <returns>True if the Cursor is positioned on a record.</returns>
        public virtual bool TryMoveNext()
        {
            this.Tracer.TraceVerbose("TryMoveNext");
            this.CheckNotDisposed();
            this.CancelUpdate();

            this.hasCurrency = Api.TryMoveNext(this.sesid, this.tableid);
            return this.hasCurrency;
        }

        /// <summary>
        /// Move to the previous record in the table.
        /// </summary>
        /// <returns>True if the Cursor is positioned on a record.</returns>
        public virtual bool TryMovePrevious()
        {
            this.Tracer.TraceVerbose("TryMovePrevious");
            this.CheckNotDisposed();
            this.CancelUpdate();

            this.hasCurrency = Api.TryMovePrevious(this.sesid, this.tableid);
            return this.hasCurrency;
        }

        /// <summary>
        /// Move before the first record in the table. A subsequent MoveFirst
        /// will position the cursor on the first record.
        /// </summary>
        public virtual void MoveBeforeFirst()
        {
            this.Tracer.TraceVerbose("MoveBeforeFirst");
            this.CheckNotDisposed();
            this.CancelUpdate();

            this.hasCurrency = false;
            Api.MoveBeforeFirst(this.sesid, this.tableid);
        }

        /// <summary>
        /// Move after the last record in the table. A subsequent MovePrevious
        /// will position the cursor on the last record.
        /// </summary>
        public virtual void MoveAfterLast()
        {
            this.Tracer.TraceVerbose("MoveAfterLast");
            this.CheckNotDisposed();
            this.CancelUpdate();

            this.hasCurrency = false;
            Api.MoveAfterLast(this.sesid, this.tableid);
        }

        /// <summary>
        /// Move to the first record in the table. This will throw an exception
        /// if the table is empty.
        /// </summary>
        public virtual void MoveFirst()
        {
            this.Tracer.TraceVerbose("MoveFirst");
            this.CheckNotDisposed();
            this.CancelUpdate();

            Api.JetMove(this.sesid, this.tableid, JET_Move.First, MoveGrbit.None);
            this.hasCurrency = true;
        }

        /// <summary>
        /// Move to the last record in the table. This will throw an exception
        /// if the table is empty.
        /// </summary>
        public virtual void MoveLast()
        {
            this.Tracer.TraceVerbose("MoveLast");
            this.CheckNotDisposed();
            this.CancelUpdate();

            Api.JetMove(this.sesid, this.tableid, JET_Move.Last, MoveGrbit.None);
            this.hasCurrency = true;
        }

        /// <summary>
        /// Get the bookmark of the current record.
        /// </summary>
        /// <returns>The bookmark of the current record.</returns>
        public virtual Bookmark GetBookmark()
        {
            this.Tracer.TraceVerbose("GetBookmark");
            this.CheckNotDisposed();
            this.CheckHasCurrency();

            byte[] bookmarkData = Api.GetBookmark(this.sesid, this.tableid);
            var bookmark = new Bookmark { BookmarkData = bookmarkData };
            return bookmark;
        }

        /// <summary>
        /// Positions the cursor on a record specified by a bookmark.
        /// </summary>
        /// <param name="bookmark">The bookmarkto go to.</param>
        public virtual void GotoBookmark(Bookmark bookmark)
        {
            this.Tracer.TraceVerbose("GotoBookmark");
            this.CheckNotDisposed();
            this.CancelUpdate();

            Api.JetGotoBookmark(this.sesid, this.tableid, bookmark.BookmarkData, bookmark.BookmarkData.Length);
            this.hasCurrency = true;
            this.CheckNotDisposed();
        }

        /// <summary>
        /// Make a key for the cursor.
        /// </summary>
        /// <param name="data">The data to use.</param>
        /// <param name="grbit">MakeKey option.</param>
        public virtual void MakeKey(byte[] data, MakeKeyGrbit grbit)
        {
            Api.JetMakeKey(this.sesid, this.tableid, data, data.Length, grbit);            
        }

        /// <summary>
        /// Seek for a record after making a key.
        /// </summary>
        /// <param name="grbit">Seek option.</param>
        public virtual void Seek(SeekGrbit grbit)
        {
            Api.JetSeek(this.sesid, this.tableid, grbit);
        }

        #endregion Navigation

        #region Updates

        /// <summary>
        /// Prepare an update for the current record.
        /// </summary>
        /// <param name="prep">Type of update to prepare.</param>
        public virtual void PrepareUpdate(JET_prep prep)
        {
            this.Tracer.TraceVerbose("PrepareUpdate {0}", prep);
            this.CheckNotDisposed();

            if (JET_prep.Cancel == prep)
            {
                string error = "Cannot use JET_prep.Cancel with PrepareUpdate. Use the Cancel method";
                this.Tracer.TraceError(error);
                throw new ArgumentException(error);
            }

            Api.JetPrepareUpdate(this.sesid, this.tableid, prep);
            this.inUpdate = true;

            this.CheckNotDisposed();
        }

        /// <summary>
        /// Update the current record.
        /// </summary>
        /// <returns>The bookmark of the updated record.</returns>
        public virtual Bookmark Update()
        {
            this.Tracer.TraceVerbose("Update");
            this.CheckNotDisposed();

            var updateBookmark = new byte[Api.BookmarkMost];
            int bookmarkSize;

            Api.JetUpdate(this.sesid, this.tableid, updateBookmark, updateBookmark.Length, out bookmarkSize);
            this.inUpdate = false;

            Array.Resize(ref updateBookmark, bookmarkSize);

            var bookmark = new Bookmark { BookmarkData = updateBookmark };
            this.GotoBookmark(bookmark);

            return bookmark;
        }

        /// <summary>
        /// Cancel the current update.
        /// </summary>
        public virtual void CancelUpdate()
        {
            this.CheckNotDisposed();

            if (this.inUpdate)
            {
                this.Tracer.TraceWarning("cancelling update");
                Api.JetPrepareUpdate(this.sesid, this.tableid, JET_prep.Cancel);
                this.inUpdate = false;
            }
        }

        /// <summary>
        /// Set a column in a record. An update must be prepared.
        /// </summary>
        /// <param name="columnid">The columnid to set.</param>
        /// <param name="data">The data to set.</param>
        /// <param name="grbit">Options for JetSetColumn.</param>
        public virtual void SetColumn(JET_COLUMNID columnid, byte[] data, SetColumnGrbit grbit)
        {
            this.Tracer.TraceVerbose("SetColumn");
            this.CheckNotDisposed();
            this.CheckInUpdate();

            if (null == data)
            {
                Api.JetSetColumn(this.sesid, this.tableid, columnid, null, 0, grbit, null);                
            }
            else
            {
                Api.JetSetColumn(this.sesid, this.tableid, columnid, data, data.Length, grbit, null);                
            }
        }

        /// <summary>
        /// Perform atomic addition on one column. The column must be of type
        /// JET_coltyp.Long. This function allows multiple sessions to update the
        /// same record concurrently without conflicts.
        /// </summary>
        /// <param name="columnid">The column to update. This must be an escrow-updatable column.</param>
        /// <param name="delta">The delta to apply to the column.</param>
        /// <returns>The current value of the column as stored in the database (versioning is ignored).</returns>
        public virtual int EscrowUpdate(JET_COLUMNID columnid, int delta)
        {
            this.Tracer.TraceVerbose("EscrowUpdate");
            this.CheckNotDisposed();
            this.CheckHasCurrency();

            return Api.EscrowUpdate(this.sesid, this.tableid, columnid, delta);
        }

        /// <summary>
        /// Delete the current record.
        /// </summary>
        public virtual void Delete()
        {
            this.Tracer.TraceVerbose("Delete");
            this.CheckNotDisposed();
            this.CheckHasCurrency();
            this.CancelUpdate();

            Api.JetDelete(this.sesid, this.tableid);
        }

        #endregion Updates

        /// <summary>
        /// Retrieve a column from the current record.
        /// </summary>
        /// <param name="columnid">The columnid to retrieve.</param>
        /// <param name="grbit">Retrieve options.</param>
        /// <returns>The data from the column.</returns>
        public virtual byte[] RetrieveColumn(JET_COLUMNID columnid, RetrieveColumnGrbit grbit)
        {
            this.Tracer.TraceVerbose("RetrieveColumn");
            this.CheckNotDisposed();
            if (0 == (RetrieveColumnGrbit.RetrieveCopy & grbit))
            {
                this.CheckHasCurrency();
            }

            return Api.RetrieveColumn(this.sesid, this.tableid, columnid, grbit, null);
        }

        /// <summary>
        /// Check to see if the cursor is in an update. Throws an exception if the cursor
        /// is not in an update.
        /// </summary>
        private void CheckInUpdate()
        {
            if (!this.inUpdate)
            {
                string error = "Cursor is not in an update";
                this.Tracer.TraceError(error);
                throw new InvalidOperationException(error);
            }
        }

        /// <summary>
        /// Check to see if the cursor is positioned on a record. Throws an exception if
        /// the cursor is not positioned on a record.
        /// </summary>
        private void CheckHasCurrency()
        {
            if (!this.hasCurrency)
            {
                string error = "Cursor is not positioned on a record";
                this.Tracer.TraceError(error);
                throw new InvalidOperationException(error);
            }
        }

        /// <summary>
        /// Check to see if the cursor is disposed. Throws an exception if the cursor has
        /// been disposed.
        /// </summary>
        private void CheckNotDisposed()
        {
            if (this.isDisposed)
            {
                string error = String.Format("Cursor on table {0} is disposed", this.tablename);
                this.Tracer.TraceError(error);
                throw new ObjectDisposedException(error);
            }
        }

        /// <summary>
        /// A bookmark represents a cursor location.
        /// </summary>
        public struct Bookmark
        {
            /// <summary>
            /// Gets or sets the ESE bookmark of the record.
            /// </summary>
            internal byte[] BookmarkData { get; set; }
        }
    }
}