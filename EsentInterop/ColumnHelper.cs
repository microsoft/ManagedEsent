//-----------------------------------------------------------------------
// <copyright file="ColumnHelper.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Help to retrieve retrieve the column value from the current record
    /// </summary>
    internal sealed class ColumnHelper
    {
        /// <summary>
        /// Provides a hook to allow retrieve the column value from the current record
        /// These circumstances are based on API elements not yet published on MSDN.
        /// </summary>
        /// <param name="grbit">Retrieve column options</param>>
        internal static void IsValidRetrieveColumnGrbit(RetrieveColumnGrbit grbit)
        {
        }
    }
}