//-----------------------------------------------------------------------
// <copyright file="TableEnumerator.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Table enumerator object. This can enumerate over a table, returning objects for
    /// each record.
    /// </summary>
    /// <typeparam name="TReturn">The type of object returned by the enumerator.</typeparam>
    internal class TableEnumerator<TReturn> : IEnumerator<TReturn>
    {
        /// <summary>
        /// Function that produces the enumerated object.
        /// </summary>
        private readonly ObjectConversionDelegate converter;

        /// <summary>
        /// The session used for the enumeration.
        /// </summary>
        private readonly JET_SESID sesid;

        /// <summary>
        /// The table being iterated over. This will be closed when the Enumerator is closed.
        /// </summary>
        private readonly JET_TABLEID tableid;

        /// <summary>
        /// Initializes a new instance of the TableEnumerator class.
        /// </summary>
        /// <param name="sesid">The session to use.</param>
        /// <param name="tableid">
        /// The table to iterate over. This tableid will be closed when the iterator is disposed.
        /// </param>
        /// <param name="converter">The conversion function.</param>
        public TableEnumerator(JET_SESID sesid, JET_TABLEID tableid, ObjectConversionDelegate converter)
        {
            this.sesid = sesid;
            this.tableid = tableid;
            this.converter = converter;
            this.Reset();
        }

        #region Delegates

        /// <summary>
        /// Conversion function. This takes a tableid, which will be positioned on a record and
        /// should return the desired object.
        /// </summary>
        /// <param name="tableid">A tableid positioned on the record.</param>
        /// <returns>A new object.</returns>
        public delegate TReturn ObjectConversionDelegate(JET_TABLEID tableid);

        #endregion

        #region IEnumerator<TReturn> Members

        /// <summary>
        /// Gets the current element in the collection.
        /// </summary>
        public TReturn Current
        {
            get
            {
                return this.converter(this.tableid);
            }
        }

        /// <summary>
        /// Gets the current element in the collection.
        /// </summary>
        object IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }

        /// <summary>
        /// Free the JET_TABLEID when enumeration is finished.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the
        /// first element in the collection.
        /// </summary>
        public void Reset()
        {
            Api.MoveBeforeFirst(this.sesid, this.tableid);
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        /// True if the enumerator was successfully advanced to the next
        /// element; false if the enumerator has passed the end of the collection.
        /// </returns>
        public bool MoveNext()
        {
            return Api.TryMoveNext(this.sesid, this.tableid);
        }

        #endregion

        /// <summary>
        /// Called when the object is being disposed or finalized.
        /// </summary>
        /// <param name="disposing">True if the function was called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            Api.JetCloseTable(this.sesid, this.tableid);
        }
    }
}