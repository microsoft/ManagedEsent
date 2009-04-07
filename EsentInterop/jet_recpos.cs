//-----------------------------------------------------------------------
// <copyright file="jet_recpos.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System.Runtime.InteropServices;

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// The native version of the JET_RETINFO structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NATIVE_RECPOS
    {
        public uint cbStruct;
        public uint centriesLT;
        public uint centriesInRange;
        public uint centriesTotal;
    }

    /// <summary>
    /// Represents a fractional position within an index. This is used by JetGotoPosition
    /// and JetGetRecordPosition.
    /// </summary>
    public class JET_RECPOS
    {
        /// <summary>
        /// Gets or sets the approximate number of index entries less than the key.
        /// </summary>
        public long centriesLT { get; set; }

        /// <summary>
        /// Gets or sets the approximate number of entries in the index.
        /// </summary>
        public long centriesTotal { get; set; }

        /// <summary>
        /// Get a NATIVE_RECPOS structure representing the object.
        /// </summary>
        /// <returns>A NATIVE_RECPOS whose members match the class.</returns>
        internal NATIVE_RECPOS GetNativeRecpos()
        {
            var recpos = new NATIVE_RECPOS();
            recpos.cbStruct = (uint)Marshal.SizeOf(recpos);
            recpos.centriesLT = (uint)this.centriesLT;
            recpos.centriesTotal = (uint)this.centriesTotal;
            return recpos;
        }

        /// <summary>
        /// Sets the fields of the object from a NATIVE_RECPOS structure.
        /// </summary>
        /// <param name="value">The NATIVE_RECPOS which will be used to set the fields.</param>
        internal void SetFromNativeRecpos(NATIVE_RECPOS value)
        {
            this.centriesLT = (int)value.centriesLT;
            this.centriesTotal = (int)value.centriesTotal;
        }
    }
}