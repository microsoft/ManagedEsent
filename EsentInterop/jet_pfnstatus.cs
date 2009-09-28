//-----------------------------------------------------------------------
// <copyright file="jet_pfnstatus.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System.Runtime.InteropServices;

namespace Microsoft.Isam.Esent.Interop
{
    using System;

    /// <summary>
    /// Receives information about the progress of long-running operations,
    /// such as defragmentation, backup, or restore operations. During such
    /// operations, the database engine calls this callback function to give
    ///  an update on the progress of the operation.
    /// </summary>
    /// <param name="sesid">
    /// The session with which the long running operation was called.
    /// </param>
    /// <param name="snp">The type of operation.</param>
    /// <param name="snt">The status of the operation.</param>
    /// <param name="snprog">Optional <see cref="JET_SNPROG"/>.</param>
    /// <returns>An error code.</returns>
    public delegate JET_err JET_PFNSTATUS(JET_SESID sesid, JET_SNP snp, JET_SNT snt, JET_SNPROG snprog);

    /// <summary>
    /// Receives information about the progress of long-running operations,
    /// such as defragmentation, backup, or restore operations. During such
    /// operations, the database engine calls this callback function to give
    ///  an update on the progress of the operation.
    /// </summary>
    /// <remarks>
    /// This is the internal version of the callback. The final parameter is
    /// a void* pointer, which may point to a NATIVE_SNPROG.
    /// </remarks>
    /// <param name="nativeSesid">
    /// The session with which the long running operation was called.
    /// </param>
    /// <param name="snp">The type of operation.</param>
    /// <param name="snt">The status of the operation.</param>
    /// <param name="snprog">Optional <see cref="NATIVE_SNPROG"/>.</param>
    /// <returns>An error code.</returns>
    internal delegate JET_err NATIVE_PFNSTATUS(IntPtr nativeSesid, uint snp, uint snt, IntPtr snprog);

    /// <summary>
    /// Wraps a NATIVE_PFNSTATUS callback around a JET_PFNSTATUS. This is
    /// used to convert the snprog argument to a managed snprog.
    /// </summary>
    internal class StatusCallbackWrapper
    {
        /// <summary>
        /// The wrapped status callback.
        /// </summary>
        private readonly JET_PFNSTATUS wrappedCallback;

        /// <summary>
        /// Initializes a new instance of the StatusCallbackWrapper class.
        /// </summary>
        /// <param name="wrappedCallback">
        /// The managed callback to use.
        /// </param>
        public StatusCallbackWrapper(JET_PFNSTATUS wrappedCallback)
        {
            this.wrappedCallback = wrappedCallback;
        }

        /// <summary>
        /// Gets a NATIVE_PFNSTATUS callback that wraps the managed callback.
        /// </summary>
        public NATIVE_PFNSTATUS Callback
        {
            get
            {
                return this.CallbackImpl;
            }
        }

        /// <summary>
        /// Gets or sets the saved exception. If the callback throws an exception
        /// it is saved here and should be rethrown when the API call finishes.
        /// </summary>
        public Exception SavedException { get; set; }

        /// <summary>
        /// Callback function for native code.
        /// </summary>
        /// <param name="nativeSesid">
        /// The session with which the long running operation was called.
        /// </param>
        /// <param name="snp">The type of operation.</param>
        /// <param name="snt">The status of the operation.</param>
        /// <param name="nativeSnprog">Optional <see cref="NATIVE_SNPROG"/>.</param>
        /// <returns>An error code.</returns>
        private JET_err CallbackImpl(IntPtr nativeSesid, uint snp, uint snt, IntPtr nativeSnprog)
        {
            try
            {
                var sesid = new JET_SESID { Value = nativeSesid };
                JET_SNPROG snprog = null;
                if (IntPtr.Zero != nativeSnprog)
                {
                    NATIVE_SNPROG native = (NATIVE_SNPROG) Marshal.PtrToStructure(nativeSnprog, typeof (NATIVE_SNPROG));
                    snprog = new JET_SNPROG();
                    snprog.SetFromNative(native);
                }
                return this.wrappedCallback(sesid, (JET_SNP)snp, (JET_SNT)snt, snprog);
            }
            catch (Exception ex)
            {
                this.SavedException = ex;
                return JET_err.InternalError;
            }
        }
    }
}
