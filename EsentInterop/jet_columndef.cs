//-----------------------------------------------------------------------
// <copyright file="jet_columndef.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// The native version of the JET_COLUMNDEF structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NATIVE_COLUMNDEF
    {
        public uint cbStruct;
        public uint columnid;
        public uint coltyp;
        public ushort wCountry;
        public ushort langid;
        public ushort cp;
        public ushort wCollate;
        public uint cbMax;
        public uint grbit;
    }

    /// <summary>
    /// Describes a column in a table of an ESENT database.
    /// </summary>
    public class JET_COLUMNDEF
    {
        /// <summary>
        /// Gets or sets type of the column.
        /// </summary>
        public JET_coltyp coltyp { get; set; }

        /// <summary>
        /// Gets or sets code page of the column. This is only meaningful for columns of type
        /// JET_coltyp.Text and JET_coltyp.LongText.
        /// </summary>
        public JET_CP cp { get; set; }

        /// <summary>
        /// Gets or sets the maximum length of the column. This is only meaningful for columns of
        /// type JET_coltyp.Text, JET_coltyp.LongText, JET_coltyp.Binary and
        /// JET_coltyp.LongBinary.
        /// </summary>
        public int cbMax { get; set; }

        /// <summary>
        /// Gets or sets the column options.
        /// </summary>
        public ColumndefGrbit grbit { get; set; }

        /// <summary>
        /// Gets or sets the columnid.
        /// </summary>
        public JET_COLUMNID columnid { get; set; }

        /// <summary>
        /// Returns the unmanaged columndef that represents this managed class.
        /// </summary>
        /// <returns>A native (interop) version of the JET_COLUMNDEF</returns>
        internal NATIVE_COLUMNDEF GetNativeColumndef()
        {
            var columndef       = new NATIVE_COLUMNDEF();
            columndef.cbStruct  = (uint)Marshal.SizeOf(columndef);
            columndef.cp        = (ushort)this.cp;
            columndef.cbMax     = (uint)this.cbMax;
            columndef.grbit     = (uint)this.grbit;
            columndef.coltyp    = (uint)this.coltyp;
            return columndef;
        }

        /// <summary>
        /// Sets the fields of the object from a native JET_COLUMNDEF struct.
        /// </summary>
        /// <param name="value">
        /// The native columndef to set the values from.
        /// </param>
        internal void SetFromNativeColumndef(NATIVE_COLUMNDEF value)
        {
            this.coltyp = (JET_coltyp)value.coltyp;
            this.cp = (JET_CP)value.cp;
            this.cbMax = (int)value.cbMax;
            this.grbit = (ColumndefGrbit)value.grbit;
            this.columnid = new JET_COLUMNID() { Value = value.columnid };
        }
    }
}