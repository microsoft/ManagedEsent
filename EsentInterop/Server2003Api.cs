//-----------------------------------------------------------------------
// <copyright file="Server2003Api.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop.Server2003
{
    /// <summary>
    /// APIs that have been added to the Windows Server 2003 version of ESENT.
    /// </summary>
    public static class Server2003Api
    {
        /// <summary>
        /// Notifies the engine that it can resume normal IO operations after a
        /// freeze period ended with a failed snapshot.
        /// </summary>
        /// <param name="snapid">Identifier of the snapshot session.</param>
        /// <param name="grbit">Options for this call.</param>
        public static void JetOSSnapshotAbort(JET_OSSNAPID snapid, SnapshotAbortGrbit grbit)
        {
            Api.Check(Api.Impl.JetOSSnapshotAbort(snapid, grbit));
        }
    }
}