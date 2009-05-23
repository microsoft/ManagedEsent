//-----------------------------------------------------------------------
// <copyright file="jet_unicodeindex.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// The native version of the JET_UNICODEINDEX structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NATIVE_UNICODEINDEX
    {
        public uint lcid;
        public uint dwMapFlags;
    }

    /// <summary>
    /// Customizes how Unicode data gets normalized when an index is created over a Unicode column.
    /// </summary>
    public class JET_UNICODEINDEX
    {
        /// <summary>
        /// Gets or sets the LCID to be used when normalizing unicode. data.
        /// </summary>
        public int lcid { get; set; }

        /// <summary>
        /// Gets or sets the flags to be used with LCMapString when normalizing unicode data.
        /// </summary>
        public uint dwMapFlags { get; set; }

        /// <summary>
        /// Gets the native version of this object.
        /// </summary>
        /// <returns>The native version of this object.</returns>
        internal NATIVE_UNICODEINDEX GetNativeUnicodeIndex()
        {
            var native = new NATIVE_UNICODEINDEX
            {
                lcid = (uint) this.lcid,
                dwMapFlags = (uint) this.dwMapFlags,
            };
            return native;
        }        
    }
}
