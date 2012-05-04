//-----------------------------------------------------------------------
// <copyright file="Windows8IdxInfo.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop.Windows8
{
    using System;

    /// <summary>
    /// Column info levels that have been added to the Windows 8 version of ESENT.
    /// </summary>
    public static class Windows8IdxInfo
    {
        /// <summary>
        /// Introduced in Windows 8. Returns a JET_INDEXCREATE3 structure suitable
        /// for use by JetCreateIndex3().
        /// </summary>
        /// <remarks>Not currently implemented in this layer.</remarks>
        internal const JET_IdxInfo InfoCreateIndex3 = (JET_IdxInfo)13;

        /// <summary>
        /// Introduced in Windows 8. Returns the locale name of the given index.
        /// </summary>
        internal const JET_IdxInfo LocaleName = (JET_IdxInfo)14;
    }
}
