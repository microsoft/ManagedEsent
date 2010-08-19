//-----------------------------------------------------------------------
// <copyright file="ColumnCreateTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Vista;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test conversion to NATIVE_COLUMNCREATE
    /// </summary>
    [TestClass]
    public class ColumnCreateTests
    {
        /// <summary>
        /// Managed version of the indexcreate structure.
        /// </summary>
        private JET_COLUMNCREATE managedSource;

        /// <summary>
        /// The native conditional column structure created from the JET_COLUMNCREATE
        /// object.
        /// </summary>
        private NATIVE_COLUMNCREATE nativeTarget;

        /// <summary>
        /// Managed version of the indexcreate structure.
        /// </summary>
        private JET_COLUMNCREATE managedTarget;

        /// <summary>
        /// The native conditional column structure created from the JET_COLUMNCREATE
        /// object.
        /// </summary>
        private NATIVE_COLUMNCREATE nativeSource;

        /// <summary>
        /// Setup the test fixture. This creates a native structure and converts
        /// it to a managed object.
        /// </summary>
        [TestInitialize]
        [Description("Initialize the ColumnCreateTests fixture")]
        public void Setup()
        {
            this.managedSource = new JET_COLUMNCREATE()
            {
                szColumnName = "column9",
                coltyp = JET_coltyp.Binary,
                cbMax = 0x42,
                grbit = ColumndefGrbit.ColumnAutoincrement,
                pvDefault = null,
                cbDefault = 0,
                cp = JET_CP.Unicode,
                columnid = new JET_COLUMNID { Value = 7 },
                err = JET_err.RecoveredWithoutUndo,
            };
            this.nativeTarget = this.managedSource.GetNativeColumnCreate();

            this.nativeSource = new NATIVE_COLUMNCREATE()
            {
                szColumnName = Marshal.StringToHGlobalAnsi("column9"),
                coltyp = (uint) JET_coltyp.Binary,
                cbMax = 0x42,
                grbit = (uint) ColumndefGrbit.ColumnAutoincrement,
                pvDefault = IntPtr.Zero,
                cbDefault = 0,
                cp = (uint) JET_CP.Unicode,
                columnid = 7,
                err = (int) JET_err.RecoveredWithoutUndo,
            };

            this.managedTarget = new JET_COLUMNCREATE();
            this.managedTarget.SetFromNativeColumnCreate(this.nativeSource);
        }

        /// <summary>
        /// Test conversion from JET_COLUMNCREATE to NATIVE_COLUMNCREATE sets szColumnName.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversion from JET_COLUMNDEF to NATIVE_COLUMNDEF sets szColumnName.")]
        public void VerifyConversionToNativeSetsSzColumnName()
        {
            // The current model is to do the string conversion at pinvoke time.
            Assert.AreEqual<IntPtr>(IntPtr.Zero, this.nativeTarget.szColumnName);
        }

        /// <summary>
        /// Test conversion from JET_COLUMNCREATE to NATIVE_COLUMNCREATE sets coltyp.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversion from JET_COLUMNDEF to NATIVE_COLUMNDEF sets coltyp.")]
        public void VerifyConversionToNativeSetsColtyp()
        {
            Assert.AreEqual<uint>(9, this.nativeTarget.coltyp);
        }

        /// <summary>
        /// Test conversion from JET_COLUMNCREATE to NATIVE_COLUMNCREATE sets cbMax
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversion from JET_COLUMNDEF to NATIVE_COLUMNDEF sets cbMax.")]
        public void VerifyConversionToNativeSetscbMax()
        {
            Assert.AreEqual<uint>(0x42, this.nativeTarget.cbMax);
        }

        /// <summary>
        /// Test conversion from JET_COLUMNCREATE to NATIVE_COLUMNCREATE sets grbit.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversion from JET_COLUMNDEF to NATIVE_COLUMNDEF sets grbit.")]
        public void VerifyConversionToNativeSetsGrbit()
        {
            Assert.AreEqual<uint>(0x10, this.nativeTarget.grbit);
        }

        /// <summary>
        /// Test conversion from JET_COLUMNCREATE to NATIVE_COLUMNCREATE sets pvDefault.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversion from JET_COLUMNDEF to NATIVE_COLUMNDEF sets pvDefault.")]
        public void VerifyConversionToNativeSetsPvDefault()
        {
            Assert.AreEqual<IntPtr>(IntPtr.Zero, this.nativeTarget.pvDefault);
        }

        /// <summary>
        /// Test conversion from JET_COLUMNCREATE to NATIVE_COLUMNCREATE sets cbDefault.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversion from JET_COLUMNDEF to NATIVE_COLUMNDEF sets cbDefault.")]
        public void VerifyConversionToNativeSetsCbDefault()
        {
            Assert.AreEqual<uint>(0, this.nativeTarget.cbDefault);
        }

        /// <summary>
        /// Test conversion from JET_COLUMNCREATE to NATIVE_COLUMNCREATE sets cp.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversion from JET_COLUMNDEF to NATIVE_COLUMNDEF sets cp.")]
        public void VerifyConversionToNativeSetsCp()
        {
            Assert.AreEqual<uint>(1200, this.nativeTarget.cp);
        }

        /// <summary>
        /// Check the conversion to a NATIVE_COLUMNCREATE sets the structure size
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Check the conversion from JET_COLUMNCREATE to a NATIVE_COLUMNCREATE sets the structure size")]
        public void VerifyConversionToNativeSetsCbStruct()
        {
            Assert.AreEqual((uint)Marshal.SizeOf(this.nativeTarget), this.nativeTarget.cbStruct);
        }

        /// <summary>
        /// Verifies that the ToString() conversion is correct.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verifies that the ToString() conversion is correct.")]
        public void VerifyToString()
        {
            Assert.AreEqual<string>("JET_COLUMNCREATE(column9,Binary,ColumnAutoincrement)", this.managedSource.ToString());
        }

        /// <summary>
        /// Test conversion from NATIVE_COLUMNCREATE to JET_COLUMNCREATE sets columnid.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversion from NATIVE_COLUMNCREATE to JET_COLUMNCREATE sets columnid.")]
        public void VerifyConversionFromNativeSetsColumnId()
        {
            Assert.AreEqual<uint>(7, (uint) this.managedTarget.columnid.Value);
        }

        /// <summary>
        /// Test conversion from NATIVE_COLUMNCREATE to JET_COLUMNCREATE sets err.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversion from NATIVE_COLUMNCREATE to JET_COLUMNCREATE sets err.")]
        public void VerifyConversionFromNativeSetsErr()
        {
            Assert.AreEqual<int>(-579, (int) this.managedTarget.err);
        }

        /// <summary>
        /// Check that CheckMembersAreValid catches empty column name.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentNullException))]
        [Description("Check that CheckMembersAreValid catches empty column name.")]
        public void VerifyValidityCatchesEmptyColumnName()
        {
            var x = new JET_COLUMNCREATE()
            {
                szColumnName = null,
                coltyp = JET_coltyp.Binary,
                cbMax = 0x42,
                grbit = ColumndefGrbit.ColumnAutoincrement,
                pvDefault = null,
                cbDefault = 0,
                cp = JET_CP.Unicode,
                columnid = new JET_COLUMNID { Value = 7 },
                err = JET_err.RecoveredWithoutUndo,
            };

            var y = new JET_COLUMNCREATE();
            Assert.IsFalse(x.ContentEquals(y));
        }

        /// <summary>
        /// Check that CheckMembersAreValid catches negative cbDefault.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Description("Check that CheckMembersAreValid catches negative cbDeafult.")]
        public void VerifyValidityCatchesNegativeCbDefault()
        {
            var x = new JET_COLUMNCREATE()
            {
                szColumnName = "column9",
                coltyp = JET_coltyp.Binary,
                cbMax = 0x42,
                grbit = ColumndefGrbit.ColumnAutoincrement,
                pvDefault = null,
                cbDefault = -53,
                cp = JET_CP.Unicode,
                columnid = new JET_COLUMNID { Value = 7 },
                err = JET_err.RecoveredWithoutUndo,
            };

            var y = new JET_COLUMNCREATE();
            Assert.IsFalse(x.ContentEquals(y));
        }

        /// <summary>
        /// Check that CheckMembersAreValid catches wrong cbDefault.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Description("Check that CheckMembersAreValid catches wrong cbDefault.")]
        public void VerifyValidityCatchesEmptycolumnnameWrongCbDefault()
        {
            var x = new JET_COLUMNCREATE()
            {
                szColumnName = "column9",
                coltyp = JET_coltyp.Binary,
                cbMax = 0x42,
                grbit = ColumndefGrbit.ColumnAutoincrement,
                pvDefault = BitConverter.GetBytes(5678),
                cbDefault = 6,
                cp = JET_CP.Unicode,
                columnid = new JET_COLUMNID { Value = 7 },
                err = JET_err.RecoveredWithoutUndo,
            };

            var y = new JET_COLUMNCREATE();
            Assert.IsFalse(x.ContentEquals(y));
        }
    }
}