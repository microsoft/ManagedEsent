//-----------------------------------------------------------------------
// <copyright file="IndexSegment.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;

    /// <summary>
    /// Describes one segment of an index.
    /// </summary>
    [Serializable]
    public class IndexSegment
    {
        /// <summary>
        /// The name of the column.
        /// </summary>
        private readonly string columnName;

        /// <summary>
        /// The type of the column.
        /// </summary>
        private readonly JET_coltyp coltyp;

        /// <summary>
        /// True if the column is sorted in ascending order.
        /// </summary>
        private readonly bool isAscending;

        /// <summary>
        /// True if the column is an ASCII column.
        /// </summary>
        private readonly bool isASCII;

        /// <summary>
        /// Initializes a new instance of the IndexSegment class.
        /// </summary>
        /// <param name="name">The name of the indexed column.</param>
        /// <param name="coltyp">The type of the column.</param>
        /// <param name="isAscending">True if the column is ascending.</param>
        /// <param name="isASCII">True if the column is over an ASCII column.</param>
        internal IndexSegment(
            string name,
            JET_coltyp coltyp,
            bool isAscending,
            bool isASCII)
        {
            this.columnName = name;
            this.coltyp = coltyp;
            this.isAscending = isAscending;
            this.isASCII = isASCII;
        }

        /// <summary>
        /// Gets name of the column being indexed.
        /// </summary>
        public string ColumnName
        {
            get { return this.columnName; }
        }

        /// <summary>
        /// Gets the type of the column being indexed.
        /// </summary>
        public JET_coltyp Coltyp
        {
            get { return this.coltyp; }
        }

        /// <summary>
        /// Gets a value indicating whether the index segment is ascending.
        /// </summary>
        public bool IsAscending
        {
            get { return this.isAscending; }
        }

        /// <summary>
        /// Gets a value indicating whether the index segment is over an ASCII text
        /// column. This value is only meaningful for text column segments.
        /// </summary>
        public bool IsASCII
        {
            get { return this.isASCII; }
        }
    }
}