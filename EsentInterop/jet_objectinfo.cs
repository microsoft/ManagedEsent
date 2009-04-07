//-----------------------------------------------------------------------
// <copyright file="jet_objectinfo.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System.Runtime.InteropServices;

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// The native version of the JET_OBJECTINFO structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NATIVE_OBJECTINFO
    {
        public uint cbStruct;
        public uint objtyp;
        public double ignored1;
        public double ignored2;
        public uint grbit;
        public uint flags;
        public uint cRecord;
        public uint cPage;
    }

    /// <summary>
    /// The JET_OBJECTINFO structure holds information about an object.
    /// Tables are the only object types that are currently supported.
    /// </summary>
    public class JET_OBJECTINFO
    {
        /// <summary>
        /// Sets the fields of the object from a native JET_OBJECTINFO struct.
        /// </summary>
        /// <param name="value">
        /// The native objectlist to set the values from.
        /// </param>
        internal void SetFromNativeObjectinfo(NATIVE_OBJECTINFO value)
        {
        }
    }
}