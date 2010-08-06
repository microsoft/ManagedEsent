//-----------------------------------------------------------------------
// <copyright file="jet_conditionalcolumn.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.Isam.Esent.Interop
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// The native version of the JET_CONDITIONALCOLUMN structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage(
        "Microsoft.StyleCop.CSharp.NamingRules",
        "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter",
        Justification = "This should match the unmanaged API, which isn't capitalized.")]
    internal struct NATIVE_CONDITIONALCOLUMN
    {
        /// <summary>
        /// Size of the structure.
        /// </summary>
        public uint cbStruct;

        /// <summary>
        /// Name of the column.
        /// </summary>
        public IntPtr szColumnName;

        /// <summary>
        /// Conditional column option.
        /// </summary>
        public uint grbit;
    }

    /// <summary>
    /// Defines how conditional indexing is performed for a given index. A
    /// conditional index contains an index entry for only those rows that
    /// match the specified condition. However, the conditional column is not
    /// part of the index's key, it only controls the presence of the index entry.
    /// </summary>
    [SuppressMessage(
        "Microsoft.StyleCop.CSharp.NamingRules",
        "SA1300:ElementMustBeginWithUpperCaseLetter",
        Justification = "This should match the unmanaged API, which isn't capitalized.")]
    [Serializable]
    public class JET_CONDITIONALCOLUMN : IEquatable<JET_CONDITIONALCOLUMN>
    {
        /// <summary>
        /// Column name.
        /// </summary>
        private string columnName;

        /// <summary>
        /// Conditional column option.
        /// </summary>
        private ConditionalColumnGrbit option;

        /// <summary>
        /// Gets or sets the name of the conditional column.
        /// </summary>
        public string szColumnName
        {
            [DebuggerStepThrough]
            get { return this.columnName; }
            set { this.columnName = value; }
        }

        /// <summary>
        /// Gets or sets the options for the conditional index.
        /// </summary>
        public ConditionalColumnGrbit grbit
        {
            [DebuggerStepThrough]
            get { return this.option; }
            set { this.option = value; }
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal
        /// to another instance.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>True if the two instances are equal.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return this.Equals((JET_CONDITIONALCOLUMN)obj);
        }

        /// <summary>
        /// Generate a string representation of the instance.
        /// </summary>
        /// <returns>The structure as a string.</returns>
        public override string ToString()
        {
            return String.Format(
                CultureInfo.InvariantCulture,
                "JET_CONDITIONALCOLUMN({0}:{1})",
                this.columnName,
                this.option);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return (null == this.columnName) ? -1 : this.columnName.GetHashCode()
                         ^ (int)this.option;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal
        /// to another instance.
        /// </summary>
        /// <param name="other">An instance to compare with this instance.</param>
        /// <returns>True if the two instances are equal.</returns>
        public bool Equals(JET_CONDITIONALCOLUMN other)
        {
            if (null == other)
            {
                return false;
            }

            return this.columnName == other.columnName && this.option == other.option;
        }

        /// <summary>
        /// Gets the NATIVE_CONDITIONALCOLUMN version of this object.
        /// </summary>
        /// <returns>A NATIVE_CONDITIONALCOLUMN for this object.</returns>
        internal NATIVE_CONDITIONALCOLUMN GetNativeConditionalColumn()
        {
            var native = new NATIVE_CONDITIONALCOLUMN();
            native.cbStruct = (uint) Marshal.SizeOf(native);
            native.grbit = (uint) this.grbit;
            return native;
        }
    }
}