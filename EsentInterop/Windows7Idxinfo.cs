//-----------------------------------------------------------------------
// <copyright file="Windows7IdxInfo.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop.Windows7
{
    using System;

    /// <summary>
    /// Column info levels that have been added to the Windows 7 version of ESENT.
    /// </summary>
    public static class Windows7IdxInfo
    {
        /// <summary>
        /// Introduced in Windows 7. Returns a JET_INDEXCREATE structure suitable
        /// for use by JetCreateIndex2().
        /// </summary>
        /// <remarks>Not currently implemented in this layer.</remarks>
        internal const JET_IdxInfo CreateIndex = (JET_IdxInfo)11;

        /// <summary>
        /// Introduced in Windows 7. Returns a JET_INDEXCREATE2 structure suitable
        /// for use by JetCreateIndex2().
        /// </summary>
        /// <remarks>Not currently implemented in this layer.</remarks>
        internal const JET_IdxInfo CreateIndex2 = (JET_IdxInfo)12;
    }
}
