//-----------------------------------------------------------------------
// <copyright file="jet_setcolumn.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// The native version of the JET_SETCOLUMN structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NATIVE_SETCOLUMN
    {
        public uint columnid;
        public IntPtr pvData;
        public uint cbData;
        public uint grbit;
        public uint ibLongValue;
        public uint itagSequence;
        public uint err;
    }

    /// <summary>
    /// Contains input and output parameters for JetSetColumns. Fields in the
    /// structure describe what column value to set, how to set it, and where
    /// to get the column set data.
    /// </summary>
    public class JET_SETCOLUMN
    {
        /// <summary>
        /// Gets or sets the column identifier for a column to set.
        /// </summary>
        public JET_COLUMNID columnid { get; set; }

        /// <summary>
        /// Gets or sets a pointer to the data to set.
        /// </summary>
        public byte[] pvData { get; set; }

        /// <summary>
        /// Gets or sets the size of the data to set.
        /// </summary>
        public int cbData { get; set; }

        /// <summary>
        /// Gets or sets options for the set column operation.
        /// </summary>
        public SetColumnGrbit grbit { get; set; }

        /// <summary>
        /// Gets or sets offset to the first byte to be set in a column of type JET_coltypLongBinary or JET_coltypLongText.
        /// </summary>
        public int ibLongValue { get; set; }

        /// <summary>
        /// Gets or sets the sequence number of value in a multi-valued column to be set. The array of values is one-based.
        /// The first value is sequence 1, not 0 (zero). If the record column has only one value then 1 should be passed
        /// as the itagSequence if that value is being replaced. A value of 0 (zero) means to add a new column value instance
        /// to the end of the sequence of column values.
        /// </summary>
        public int itagSequence { get; set; }

        /// <summary>
        /// Gets the error code or warning returned from the set column operation.
        /// </summary>
        public JET_err err { get; internal set; }

        /// <summary>
        /// Gets or sets the pinned data. This is the data which will be used when the column is set. It is the caller's 
        /// responsibility to pin the data in <see cref="pvData"/> and set the pinned value.
        /// </summary>
        internal IntPtr PinnedData { get; set; }

        /// <summary>
        /// Gets the NATIVE_SETCOLUMN structure that represents the object.
        /// </summary>
        /// <returns>A NATIVE_SETCOLUMN structure whose fields match the class.</returns>
        internal NATIVE_SETCOLUMN GetNativeSetcolumn()
        {
            Debug.Assert(null == this.pvData || IntPtr.Zero != this.PinnedData, "pvData is non-null, but PinnedData is null");
            var setinfo = new NATIVE_SETCOLUMN
            {
                columnid = this.columnid.Value,
                pvData = this.PinnedData,
                cbData = (uint) this.cbData,
                grbit = (uint) this.grbit,
                ibLongValue = (uint) this.ibLongValue,
                itagSequence = (uint) this.itagSequence,
            };
            return setinfo;
        }
    }
}