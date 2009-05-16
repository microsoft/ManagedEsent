//-----------------------------------------------------------------------
// <copyright file="jet_conditionalcolumn.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System.Runtime.InteropServices;

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// The native version of the JET_UNICODEINDEX structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NATIVE_CONDITIONALCOLUMN
    {
        public uint cbStruct;
        public string szColumnName;
        public uint grbit;
    }

    /// <summary>
    /// Defines how conditional indexing is performed for a given index. A
    /// conditional index contains an index entry for only those rows that
    /// match the specified condition. However, the conditional column is not
    /// part of the index's key, it only controls the presence of the index entry.
    /// </summary>
    public class JET_CONDITIONALCOLUMN
    {
        /// <summary>
        /// Gets or sets the name of the conditional column.
        /// </summary>
        public string szColumnName { get; set; }

        /// <summary>
        /// Gets or sets the options for the conditional index.
        /// </summary>
        public ConditionalColumnGrbit grbit { get; set; }

        /// <summary>
        /// Gets the NATIVE_CONDITIONALCOLUMN version of this object.
        /// </summary>
        /// <returns>A NATIVE_CONDITIONALCOLUMN for this object.</returns>
        internal NATIVE_CONDITIONALCOLUMN GetNativeConditionalColumn()
        {
            var native = new NATIVE_CONDITIONALCOLUMN();
            native.cbStruct = (uint)Marshal.SizeOf(native);
            native.szColumnName = this.szColumnName;
            native.grbit = (uint) this.grbit;
            return native;
        }
    }
}