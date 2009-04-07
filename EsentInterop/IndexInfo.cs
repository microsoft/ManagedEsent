//-----------------------------------------------------------------------
// <copyright file="IndexInfo.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System.Globalization;

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// Information about one esent index. This is not an interop
    /// class, but is used by the meta-data helper methods.
    /// </summary>
    public class IndexInfo
    {
        internal IndexInfo(
            string name,
            CultureInfo cultureInfo,
            CompareOptions compareOptions,
            IndexSegment[] indexSegments,
            CreateIndexGrbit grbit)
        {
            this.Name = name;
            this.CultureInfo = cultureInfo;
            this.CompareOptions = compareOptions;
            this.IndexSegments = indexSegments;
            this.Grbit = grbit;
        }

        /// <summary>
        /// Gets the name of the index.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the CultureInfo the index is sorted by.
        /// </summary>
        public CultureInfo CultureInfo { get; private set; }

        /// <summary>
        /// Gets the CompareOptions for the index.
        /// </summary>
        public CompareOptions CompareOptions { get; private set; }

        /// <summary>
        /// Gets the segments of the index.
        /// </summary>
        public IndexSegment[] IndexSegments { get; private set; }

        /// <summary>
        /// Gets the index options.
        /// </summary>
        public CreateIndexGrbit Grbit { get; private set; }
    }
}