//-----------------------------------------------------------------------
// <copyright file="jet_indexcreate.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Isam.Esent.Interop.Vista;

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// The native version of the JET_INDEXCREATE structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct NATIVE_INDEXCREATE
    {
        public uint cbStruct;
        public string szIndexName;
        public string szKey;
        public uint cbKey;
        public uint grbit;
        public uint ulDensity;
        public NATIVE_UNICODEINDEX* pidxUnicode;
        public IntPtr cbVarSegMac;  // can also be JET_TUPLELIMITS*
        public IntPtr rgconditionalcolumn;
        public uint cConditionalColumn;
        public int err;
    }

    /// <summary>
    /// The native version of the JET_INDEXCREATE structure. This version includes the cbKeyMost
    /// member, which is only valid on Windows Vista and above.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NATIVE_INDEXCREATE2
    {
        public NATIVE_INDEXCREATE indexcreate;
        public uint cbKeyMost;
    }

    /// <summary>
    /// Contains the information needed to create an index over data in an ESE database.
    /// </summary>
    public class JET_INDEXCREATE
    {
        /// <summary>
        /// Option used to indicate that the pidxUnicode member of a NATIVE_INDEXCREATE
        /// structure points to a NATIVE_UNICODEINDEX.
        /// </summary>
        private const CreateIndexGrbit Unicode = (CreateIndexGrbit)0x00000800;

        /// <summary>
        /// Gets or sets the name of the index to create. 
        /// </summary>
        public string szIndexName { get; set; }

        /// <summary>
        /// Gets or sets the description of the index key. This is a double 
        /// null-terminated string of null-delimited tokens. Each token is
        /// of the form [direction-specifier][column-name], where
        /// direction-specification is either "+" or "-". for example, a
        /// szKey of "+abc\0-def\0+ghi\0" will index over the three columns
        /// "abc" (in ascending order), "def" (in descending order), and "ghi"
        /// (in ascending order).
        /// </summary>
        public string szKey { get; set; }

        /// <summary>
        /// Gets or sets the length, in characters, of szKey including the two terminating nulls.
        /// </summary>
        public int cbKey { get; set; }

        /// <summary>
        /// Gets or sets index creation options.
        /// </summary>
        public CreateIndexGrbit grbit { get; set; }

        /// <summary>
        /// Gets or sets the density of the index.
        /// </summary>
        public int ulDensity { get; set; }

        /// <summary>
        /// Gets or sets the optional unicode comparison options.
        /// </summary>
        public JET_UNICODEINDEX pidxUnicode { get; set; }

        /// <summary>
        /// Gets or sets the maximum length, in bytes, of each column to store in the index.
        /// </summary>
        public int cbVarSegMac { get; set; }

        /// <summary>
        /// Gets or sets the optional conditional columns.
        /// </summary>
        public JET_CONDITIONALCOLUMN[] rgconditionalcolumn { get; set; }

        /// <summary>
        /// Gets or sets the number of conditional columns.
        /// </summary>
        public int cConditionalColumn { get; set; }

        /// <summary>
        /// Gets or sets the error code from creating this index.
        /// </summary>
        public JET_err err { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowable size, in bytes, for keys in the index.
        /// The minimum supported maximum key size is JET_cbKeyMostMin (255) which
        /// is the legacy maximum key size. The maximum key size is dependent on
        /// the database page size <see cref="JET_param.DatabasePageSize"/>.
        /// <para>
        /// This parameter is ignored on Windows XP and Windows Server 2003.
        /// </para>
        /// <para>
        /// Unlike the unmanaged API, <see cref="VistaGrbits.IndexKeyMost"/>
        /// (JET_bitIndexKeyMost) is not needed, it will be added automatically.
        /// </para>
        /// </summary>
        public int cbKeyMost { get; set; }

        /// <summary>
        /// Gets the native (interop) version of this object.
        /// </summary>
        /// <returns>The native (interop) version of this object.</returns>
        internal NATIVE_INDEXCREATE GetNativeIndexcreate()
        {
            var native = new NATIVE_INDEXCREATE();
            native.cbStruct = (uint) Marshal.SizeOf(native);
            native.szIndexName = this.szIndexName;
            native.szKey = this.szKey;
            native.cbKey = (uint) this.cbKey;
            native.grbit = (uint) this.grbit;
            native.ulDensity = (uint) this.ulDensity;

            native.cbVarSegMac = new IntPtr(this.cbVarSegMac);

            native.cConditionalColumn = (uint) this.cConditionalColumn;
            return native;
        }

        /// <summary>
        /// Gets the native (interop) version of this object.
        /// </summary>
        /// <returns>The native (interop) version of this object.</returns>
        internal NATIVE_INDEXCREATE2 GetNativeIndexcreate2()
        {
            var native = new NATIVE_INDEXCREATE2();
            native.indexcreate = this.GetNativeIndexcreate();
            native.indexcreate.cbStruct = (uint)Marshal.SizeOf(native);
            if (0 != this.cbKeyMost)
            {
                native.cbKeyMost = (uint)this.cbKeyMost;
                native.indexcreate.grbit |= (uint)VistaGrbits.IndexKeyMost;
            }

            return native;
        }
    }
}

