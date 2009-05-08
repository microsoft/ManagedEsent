//-----------------------------------------------------------------------
// <copyright file="Windows7Grbits.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop.Windows7
{
    /// <summary>
    /// Grbits that have been added to the Windows 7 version of ESENT.
    /// </summary>
    public static class Windows7Grbits
    {
        /// <summary>
        /// Compress data in the column, if possible.
        /// </summary>
        public const ColumndefGrbit ColumnCompressed = (ColumndefGrbit)0x80000;

        /// <summary>
        /// Try to compress the data when storing it.
        /// </summary>
        public const SetColumnGrbit Compressed = (SetColumnGrbit)0x20000;

        /// <summary>
        /// Don't compress the data when storing it.
        /// </summary>
        public const SetColumnGrbit Uncompressed = (SetColumnGrbit)0x20000;

        // UNDONE: ReplayIgnoreLostLogs = 0x80

        /// <summary>
        /// Terminate without flushing the database cache.
        /// </summary>
        public const TermGrbit Dirty = (TermGrbit)0x8;
    }
}