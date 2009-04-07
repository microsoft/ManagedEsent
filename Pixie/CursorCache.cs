//-----------------------------------------------------------------------
// <copyright file="CursorCache.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Isam.Esent
{
    /// <summary>
    /// Caches Cursors for Records to use. This structure is not
    /// threadsafe.
    /// </summary>
    /// <typeparam name="TCursor">The type of the cursor to cache.</typeparam>
    internal class CursorCache<TCursor>
    {
        /// <summary>
        /// The function used to open the table.
        /// </summary>
        private readonly Func<TCursor> openFunc;

        /// <summary>
        /// The function used to close the table.
        /// </summary>
        private readonly Action<TCursor> closeFunc;

        /// <summary>
        /// The timestamp for this cache. The timestamp is used to determine which cached
        /// tableid hasn't been used in the longest.
        /// </summary>
        private long timestamp;

        /// <summary>
        /// A list of the cached cursors.
        /// </summary>
        private List<CachedCursor> cachedCursors;

        /// <summary>
        /// Initializes a new instance of the CursorCache class.
        /// </summary>
        /// <param name="openFunc">Delegate used to open a new cursor.</param>
        /// <param name="closeFunc">Delegate used to close a cursor opened by openFunc.</param>
        /// <param name="tablename">The name of the table.</param>
        /// <param name="maxCursors">The number of Cursors to open.</param>
        public CursorCache(Func<TCursor> openFunc, Action<TCursor> closeFunc, string tablename, int maxCursors)
        {
            this.Tracer = new Tracer("CursorCache", "Esent cursor cache", String.Format("CursorCache ({0})", tablename));

            this.openFunc = openFunc;
            this.closeFunc = closeFunc;

            this.cachedCursors = new List<CachedCursor>(maxCursors);
            for (int i = 0; i < maxCursors; ++i)
            {
                TCursor cursor = this.openFunc();

                var cachedtableid = new CachedCursor
                {
                    Id = 0,
                    Cursor = cursor,
                    Timestamp = 0,
                };

                this.cachedCursors.Add(cachedtableid);
                this.Tracer.TraceInfo("cached cursor {0}", cachedtableid.Cursor);
            }
        }

        /// <summary>
        /// Gets or sets the Tracer object for this instance.
        /// </summary>
        private Tracer Tracer { get; set; }

        /// <summary>
        /// Close all the cached cursors.
        /// </summary>
        public void Close()
        {
            if (null != this.cachedCursors)
            {
                foreach (CachedCursor cachedtableid in this.cachedCursors)
                {
                    this.closeFunc(cachedtableid.Cursor);
                    this.Tracer.TraceInfo("closed cursor {0}", cachedtableid.Cursor);
                }

                this.cachedCursors = null;
            }
        }

        /// <summary>
        /// Is there a cached cursor for the record?
        /// </summary>
        /// <param name="recordId">The ID of the record.</param>
        /// <returns>True if there is a cached cursor, false otherwise.</returns>
        public bool HasCachedCursor(long recordId)
        {
            return this.cachedCursors.Any(x => x.Id == recordId);
        }

        /// <summary>
        /// Get the cached cursor for the record. There must be a cached
        /// cursor for the record.
        /// </summary>
        /// <param name="recordId">The id of the record.</param>
        /// <returns>The cached cursor for the record.</returns>
        public TCursor GetCachedCursor(long recordId)
        {
            Debug.Assert(this.HasCachedCursor(recordId), "No cached cursor for the record");

            CachedCursor cachedcursor = this.cachedCursors.First(x => x.Id == recordId);
            cachedcursor.Timestamp = this.timestamp++;
            this.Tracer.TraceVerbose("returned cached tableid {0} for {1}", cachedcursor.Cursor, recordId);
            return cachedcursor.Cursor;
        }

        /// <summary>
        /// Get a new cursor for the record. There must NOT be a cached
        /// tableid for the record.
        /// </summary>
        /// <param name="recordId">The id of the record.</param>
        /// <returns>The cursor for the record.</returns>
        public TCursor GetNewCursor(long recordId)
        {
            Debug.Assert(!this.HasCachedCursor(recordId), "There is already a cached cusor for the record");

            CachedCursor victim = this.cachedCursors.OrderBy(x => x.Timestamp).First();
            victim.Timestamp = this.timestamp++;
            victim.Id = recordId;
            this.Tracer.TraceVerbose("returned new cusor {0} for {1}", victim.Cursor, recordId);
            return victim.Cursor;
        }

        /// <summary>
        /// Describe a cached cursor.
        /// </summary>
        private class CachedCursor
        {
            /// <summary>
            /// Gets or sets the id of the record that is using the cached tableid.
            /// </summary>
            public long Id { get; set; }

            /// <summary>
            /// Gets or sets the timestamp of the last access of the tableid.
            /// </summary>
            public long Timestamp { get; set; }

            /// <summary>
            /// Gets or sets the cached tableid.
            /// </summary>
            public TCursor Cursor { get; set; }
        }
    }
}
