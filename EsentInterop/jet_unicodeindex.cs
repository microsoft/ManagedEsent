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
        /// Gets or sets the CultureInfo used for index comparisons.
        /// </summary>
        public CultureInfo CultureInfo { get; set; }

        /// <summary>
        /// Gets or sets the comparison options used for index comparisons.
        /// </summary>
        public CompareOptions CompareOptions { get; set; }

        /// <summary>
        /// Gets the native version of this object.
        /// </summary>
        /// <returns>The native version of this object.</returns>
        internal NATIVE_UNICODEINDEX GetNativeUnicodeIndex()
        {
            var native = new NATIVE_UNICODEINDEX
            {
                lcid = (uint)this.CultureInfo.LCID,
                dwMapFlags = Conversions.LCMapFlagsFromCompareOptions(this.CompareOptions),
            };
            native.dwMapFlags |= Conversions.NativeMethods.LCMAP_SORTKEY;
            return native;
        }        
    }
}
