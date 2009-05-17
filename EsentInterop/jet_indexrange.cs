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
                grbit = (uint) IndexRangeGrbit.RecordInIndex,
            };
            s.cbStruct = (uint)Marshal.SizeOf(s);
            return s;
        }
    }

    /// <summary>
    /// Identifies an index range when it is used with the JetIntersectIndexes function.
    /// </summary>
    public class JET_INDEXRANGE
    {
        /// <summary>
        /// Initializes a new instance of the JET_INDEXRANGE class.
        /// </summary>
        public JET_INDEXRANGE()
        {
            // set the grbit to the only valid value
            this.grbit = IndexRangeGrbit.RecordInIndex;
        }

        /// <summary>
        /// Gets or sets the cursor containing the index range. The cursor should have an
        /// index range set with JetSetIndexRange
        /// </summary>
        public JET_TABLEID tableid { get; set; }

        /// <summary>
        /// Gets or sets the indexrange option.
        /// </summary>
        public IndexRangeGrbit grbit { get; set; }

        /// <summary>
        /// Get a NATIVE_INDEXRANGE structure representing the object.
        /// </summary>
        /// <returns>A NATIVE_INDEXRANGE whose members match the class.</returns>
        internal NATIVE_INDEXRANGE GetNativeIndexRange()
        {
            var indexrange = new NATIVE_INDEXRANGE();
            indexrange.cbStruct = (uint) Marshal.SizeOf(indexrange);
            indexrange.tableid = this.tableid.Value;
            indexrange.grbit = (uint) this.grbit;
            return indexrange;
        }
    }
}

