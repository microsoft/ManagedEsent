//-----------------------------------------------------------------------
// <copyright file="jet_setinfo.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// The native version of the JET_SETINFO structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NATIVE_SETINFO
    {
        public static readonly int Size = Marshal.SizeOf(typeof(NATIVE_SETINFO));
        public uint cbStruct;
        public uint ibLongValue;
        public uint itagSequence;
    }

    /// <summary>
    /// Settings for JetSetColumn.
    /// </summary>
    public class JET_SETINFO
    {
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
        /// Gets the NATIVE_SETINFO structure that represents the object.
        /// </summary>
        /// <returns>A NATIVE_SETINFO structure whose fields match the class.</returns>
        internal NATIVE_SETINFO GetNativeSetinfo()
        {
            var setinfo = new NATIVE_SETINFO();
            setinfo.cbStruct = checked((uint) NATIVE_SETINFO.Size);
            setinfo.ibLongValue = checked((uint) this.ibLongValue);
            setinfo.itagSequence = checked((uint) this.itagSequence);
            return setinfo;
    }
    }
}