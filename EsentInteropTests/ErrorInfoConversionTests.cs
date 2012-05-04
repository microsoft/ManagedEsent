//-----------------------------------------------------------------------
// <copyright file="ErrorInfoConversionTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Windows8;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// JET_ERRINFOBASIC conversion tests.
    /// </summary>
    [TestClass]
    public class ErrorInfoConversionTests
    {
        /// <summary>
        /// The managed ERRINFOBASIC, original version.
        /// </summary>
        private JET_ERRINFOBASIC managedOriginal;

        /// <summary>
        /// The native ERRINFOBASIC, converted from managedOriginal.
        /// </summary>
        private NATIVE_ERRINFOBASIC native;

        /// <summary>
        /// The managed ERRINFOBASIC that's reconstitued from native.
        /// </summary>
        private JET_ERRINFOBASIC managed;

        /// <summary>
        /// Create a native ERRINFOBASIC and convert it to managed.
        /// </summary>
        [TestInitialize]
        [Description("Setup the ErrorInfoConversionTests test fixture")]
        public void Setup()
        {
            this.managedOriginal = new JET_ERRINFOBASIC()
            {
                errValue = JET_err.ReadVerifyFailure,
                errcat = JET_ERRCAT.Corruption,
                rgCategoricalHierarchy = new JET_ERRCAT[] { JET_ERRCAT.Error, JET_ERRCAT.Data, JET_ERRCAT.Fragmentation, 0, 0, 0, 0, 0 },
                lSourceLine = 42,
                rgszSourceFile = "sourcefile.cxx",
            };

            this.native = this.managedOriginal.GetNativeErrInfo();
            this.managed = new JET_ERRINFOBASIC();
            this.managed.SetFromNative(ref this.native);
        }

        /// <summary>
        /// Test conversion from the native stuct sets errValue.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversion from NATIVE_ERRINFOBASIC to JET_ERRINFOBASIC sets errValue.")]
        public void ConvertErrorInfoFromNativeSetsErrValue()
        {
            Assert.AreEqual(JET_err.ReadVerifyFailure, this.managed.errValue);
        }

        /// <summary>
        /// Test conversion from the native stuct sets errcatMostSpecific.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversion from NATIVE_ERRINFOBASIC to JET_ERRINFOBASIC sets errcatMostSpecific.")]
        public void ConvertErrorInfoFromNativeSetsErrcat()
        {
            Assert.AreEqual(JET_ERRCAT.Corruption, this.managed.errcat);
        }

        /// <summary>
        /// Test conversion from the native stuct sets lSourceLine.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversion from NATIVE_ERRINFOBASIC to JET_ERRINFOBASIC sets lSourceLine.")]
        public void ConvertErrorInfoFromNativeSetsLSourceLine()
        {
            Assert.AreEqual(42, this.managed.lSourceLine);
        }

        /// <summary>
        /// Test conversion from the native stuct sets rgszSourceFile.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversion from NATIVE_ERRINFOBASIC to JET_ERRINFOBASIC sets rgszSourceFile.")]
        public void ConvertErrorInfoFromNativeSetsRgszSourceFile()
        {
            Assert.AreEqual("sourcefile.cxx", this.managed.rgszSourceFile);
        }

        /// <summary>
        /// Test conversion from the native stuct sets rgCategoricalHierarchy.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversion from NATIVE_ERRINFOBASIC to JET_ERRINFOBASIC sets rgCategoricalHierarchy.")]
        public void ConvertErrorInfoFromNativeSetsRgCategoricalHierarchy()
        {
            Assert.AreEqual(JET_ERRCAT.Error, this.managed.rgCategoricalHierarchy[0]);
            Assert.AreEqual(JET_ERRCAT.Data, this.managed.rgCategoricalHierarchy[1]);
            Assert.AreEqual(JET_ERRCAT.Fragmentation, this.managed.rgCategoricalHierarchy[2]);
            Assert.AreEqual(JET_ERRCAT.Unknown, this.managed.rgCategoricalHierarchy[3]);
            Assert.AreEqual(JET_ERRCAT.Unknown, this.managed.rgCategoricalHierarchy[4]);
            Assert.AreEqual(JET_ERRCAT.Unknown, this.managed.rgCategoricalHierarchy[5]);
            Assert.AreEqual(JET_ERRCAT.Unknown, this.managed.rgCategoricalHierarchy[6]);
            Assert.AreEqual(JET_ERRCAT.Unknown, this.managed.rgCategoricalHierarchy[7]);
        }

        /// <summary>
        /// Test conversion to native and back loses nothing.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversion to native and back loses nothing.")]
        public void ConvertErrorInfoToNativeAndBackIsLossless()
        {
            var nativeTemp = this.managed.GetNativeErrInfo();
            var managedActual = new JET_ERRINFOBASIC();
            managedActual.SetFromNative(ref nativeTemp);

            Assert.IsTrue(managedActual.ContentEquals(this.managed));
            Assert.IsFalse(managedActual.ContentEquals(null));
        }

        /// <summary>
        /// Test DeepClone() works.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test duplication with DeepClone loses nothing.")]
        public void ConvertErrorInfoDeepClone()
        {
            var managedClone = this.managed.DeepClone();

            Assert.IsTrue(managedClone.ContentEquals(this.managed));
        }

        /// <summary>
        /// Verify comparison works with a null rgCategoricalHierarchy.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify comparison works with a null rgCategoricalHierarchy.")]
        public void VerifyConvertErrorInfoComparisonWithNullCategoryHierarchy()
        {
            var emptyHierarchy = this.managed.DeepClone();
            emptyHierarchy.rgCategoricalHierarchy = null;

            Assert.IsFalse(emptyHierarchy.ContentEquals(this.managed));
            Assert.IsFalse(this.managed.ContentEquals(emptyHierarchy));
        }

        /// <summary>
        /// Verify comparison fails with different members.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Verify comparison fails with different members.")]
        public void VerifyConvertErrorFailsWithDifferentMembers()
        {
            var miismatch = this.managed.DeepClone();
            miismatch.errcat = JET_ERRCAT.Obsolete;

            Assert.IsFalse(miismatch.ContentEquals(this.managed));
            Assert.IsFalse(this.managed.ContentEquals(miismatch));
        }
    }
}