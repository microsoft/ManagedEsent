//-----------------------------------------------------------------------
// <copyright file="jet_indexrange.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// The native version of the JET_INDEXRANGE structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NATIVE_INDEXRANGE
    {
        public uint cbStruct;
        public IntPtr tableid;
        public uint grbit;

        public static NATIVE_INDEXRANGE MakeIndexRangeFromTableid(JET_TABLEID tableid)
        {
            var s = new NATIVE_INDEXRANGE
            {
                tableid = tableid.Value,
                grbit = 0x1, // JET_bitRecordInIndex
            };
            s.cbStruct = (uint)Marshal.SizeOf(s);
            return s;
        }
    }
}

