//-----------------------------------------------------------------------
// <copyright file="jet_enumcolumn.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Isam.Esent.Interop
{
    /// <summary>
    /// Native (unmanaged) version of the JET_ENUMCOLUMN structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NATIVE_ENUMCOLUMN
    {
        /// <summary>
        /// The columnid that was enumerated.
        /// </summary>
        public uint columnid;

        /// <summary>
        /// The column status code from the enumeration of the column.
        /// </summary>
        public uint err;

        /// <summary>
        /// Number of entries in rgEnumColumnValue.
        /// This member is only used if <see cref="err"/> is not
        /// <see cref="JET_wrn.ColumnSingleValue"/>.
        /// </summary>
        /// <remarks>
        /// The unmanaged JET_ENUMCOLUMN structure is a union so this
        /// value is aliased with the cbData member. A separate property
        /// is provided for cbData.
        /// </remarks>
        public uint cEnumColumnValue;

        /// <summary>
        /// Array of column values.
        /// This member is only used if <see cref="err"/> is not
        /// <see cref="JET_wrn.ColumnSingleValue"/>.
        /// </summary>
        /// <remarks>
        /// The unmanaged JET_ENUMCOLUMN structure is a union so this
        /// value is aliased with the pvData member. A separate property
        /// is provided for pvData.
        /// </remarks>
        public IntPtr rgEnumColumnValue;

        /// <summary>
        /// Gets the size of the value that was enumerated for the column.
        /// This member is only used if <see cref="err"/> is equal to
        /// <see cref="JET_wrn.ColumnSingleValue"/>.
        /// </summary>
        /// <remarks>
        /// The unmanaged JET_ENUMCOLUMN structure is a union so this
        /// property uses cEnumColumnValue as its backing storage.
        /// </remarks>
        public uint cbData
        {
            get
            {
                return this.cEnumColumnValue;
            }
        }

        /// <summary>
        /// Gets the the value that was enumerated for the column.
        /// This member is only used if <see cref="err"/> is equal to
        /// <see cref="JET_wrn.ColumnSingleValue"/>.
        /// </summary>
        /// <remarks>
        /// The unmanaged JET_ENUMCOLUMN structure is a union so this
        /// property uses rgEnumColumnValue as its backing storage.
        /// </remarks>
        public IntPtr pvData
        {
            get
            {
                return this.rgEnumColumnValue;
            }
        }
    }

    /// <summary>
    /// Enumerates the column values of a record using the JetEnumerateColumns
    /// function. JetEnumerateColumns returns an array of JET_ENUMCOLUMNVALUE
    /// structures. The array is returned in memory that was allocated using
    /// the callback that was supplied to that function.
    /// </summary>
    public class JET_ENUMCOLUMN
    {
        /// <summary>
        /// Gets the columnid ID that was enumerated.
        /// </summary>
        public JET_COLUMNID columnid { get; internal set; }

        /// <summary>
        /// Gets the column status code that results from the enumeration.
        /// </summary>
        public JET_wrn err { get; internal set; }

        /// <summary>
        /// Gets the number of column values enumerated for the column.
        /// This member is only used if <see cref="err"/> is not
        /// <see cref="JET_wrn.ColumnSingleValue"/>.
        /// </summary>
        public int cEnumColumnValue { get; internal set; }

        /// <summary>
        /// Gets the enumerated column values for the column.
        /// This member is only used if <see cref="err"/> is not
        /// <see cref="JET_wrn.ColumnSingleValue"/>.
        /// </summary>
        public JET_ENUMCOLUMNVALUE[] rgEnumColumnValue { get; internal set; }

        /// <summary>
        /// Gets the size of the value that was enumerated for the column.
        /// This member is only used if <see cref="err"/> is equal to
        /// <see cref="JET_wrn.ColumnSingleValue"/>.
        /// </summary>
        public int cbData { get; internal set; }

        /// <summary>
        /// Gets the the value that was enumerated for the column.
        /// This member is only used if <see cref="err"/> is equal to
        /// <see cref="JET_wrn.ColumnSingleValue"/>.
        /// This points to memory allocated with the 
        /// <see cref="JET_PFNREALLOC"/> allocator callback passed to
        /// <see cref="Api.JetEnumerateColumns"/>. Remember to
        /// release the memory when finished.
        /// </summary>
        public IntPtr pvData { get; internal set; }

        /// <summary>
        /// Sets the fields of the object from a native JET_ENUMCOLUMN struct.
        /// </summary>
        /// <param name="value">
        /// The native enumcolumn to set the values from.
        /// </param>
        internal void SetFromNativeEnumColumn(NATIVE_ENUMCOLUMN value)
        {
            this.columnid = new JET_COLUMNID { Value = value.columnid };
            this.err = (JET_wrn) value.err;
            if (JET_wrn.ColumnSingleValue == this.err)
            {
                this.cbData = checked((int) value.cbData);
                this.pvData = value.pvData;
            }
            else
            {
                throw new Exception("Not Yet Implemented");
            }
        }
    }
}