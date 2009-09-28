//-----------------------------------------------------------------------
// <copyright file="jet_snprog.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    /// <summary>
    /// The native version of the JET_SNPROG structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NATIVE_SNPROG
    {
        /// <summary>
        /// Size of this structure.
        /// </summary>
        public static readonly int Size = Marshal.SizeOf(typeof(NATIVE_SNPROG));

        /// <summary>
        /// Size of the structure.
        /// </summary>
        public uint cbStruct;

        /// <summary>
        /// The number of work units that are already completed during the long
        /// running operation.
        /// </summary>
        public uint cunitDone;

        /// <summary>
        /// The number of work units that need to be completed. This value will
        /// always be bigger than or equal to cunitDone.
        /// </summary>
        public uint cunitTotal;
    }

    /// <summary>
    /// Contains information about the progress of a long-running operation.
    /// </summary>
    public class JET_SNPROG
    {
        /// <summary>
        /// Gets or sets the number of work units that are already completed during the long
        /// running operation.
        /// </summary>
        public int cunitDone { get; set; }

        /// <summary>
        /// Gets or sets the number of work units that need to be completed. This value will
        /// always be bigger than or equal to cunitDone.
        /// </summary>
        public int cunitTotal { get; set; }

        /// <summary>
        /// Set the members of this class from a <see cref="NATIVE_SNPROG"/>.
        /// </summary>
        /// <param name="native">The native struct.</param>
        internal void SetFromNative(NATIVE_SNPROG native)
        {
            Debug.Assert(native.cbStruct == NATIVE_SNPROG.Size, "NATIVE_SNPROG is the wrong size");
            this.cunitDone = checked((int) native.cunitDone);
            this.cunitTotal = checked((int) native.cunitTotal);
        }
    }
}
