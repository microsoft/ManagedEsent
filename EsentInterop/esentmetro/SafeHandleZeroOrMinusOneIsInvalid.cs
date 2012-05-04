//-----------------------------------------------------------------------
// <copyright file="SafeHandleZeroOrMinusOneIsInvalid.cs" company="Microsoft Corporation">
//  Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//  Attribute stubs to allow compiling on CoreClr.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>Provides a base class for Win32 safe handle implementations in which the value of either 0 or -1 indicates an invalid handle.</summary>
    [SecurityCritical]
    public abstract class SafeHandleZeroOrMinusOneIsInvalid : SafeHandle
    {
        /// <summary>Initializes a new instance of the <see cref="SafeHandleZeroOrMinusOneIsInvalid" /> class, specifying whether the handle is to be reliably released. </summary>
        /// <param name="ownsHandle"><c>true</c> to reliably release the handle during the finalization phase; <c>false</c> to prevent reliable release (not recommended).</param>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected SafeHandleZeroOrMinusOneIsInvalid(bool ownsHandle) : base(IntPtr.Zero, ownsHandle)
        {
        }

        /// <summary>Gets a value that indicates whether the handle is invalid.</summary>
        /// <returns>true if the handle is not valid; otherwise, false.</returns>
        public override bool IsInvalid
        {
            [SecurityCritical]
            get
            {
                return this.handle == new IntPtr(0) || this.handle == new IntPtr(-1);
            }
        }
    }
}