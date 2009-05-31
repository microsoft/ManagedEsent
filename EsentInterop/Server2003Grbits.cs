//-----------------------------------------------------------------------
// <copyright file="Server2003Grbits.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop.Server2003
{
    /// <summary>
    /// Grbits that have been added to the Windows Server 2003 version of ESENT.
    /// </summary>
    public static class Server2003Grbits
    {
        /// <summary>
        /// This option requests that the temporary table only be created if the
        /// temporary table manager can use the implementation optimized for
        /// intermediate query results. If any characteristic of the temporary
        /// table would prevent the use of this optimization then the operation
        /// will fail with JET_errCannotMaterializeForwardOnlySort. A side effect
        /// of this option is to allow the temporary table to contain records
        /// with duplicate index keys. See <see cref="TempTableGrbit.Unique"/>
        /// for more information.
        /// </summary>        
        public const TempTableGrbit ForwardOnly = (TempTableGrbit) 0x40;
    }
}