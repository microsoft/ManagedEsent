//-----------------------------------------------------------------------
// <copyright file="CommitIdConversionTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Windows8;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// JET_COMMIT_ID conversion tests.
    /// </summary>
    [TestClass]
    public class CommitIdConversionTests
    {
        /// <summary>
        /// The managed JET_COMMIT_ID, original version.
        /// </summary>
        private JET_COMMIT_ID managedOriginal;

        /// <summary>
        /// The native JET_COMMIT_ID, converted from managedOriginal.
        /// </summary>
        private NATIVE_COMMIT_ID native;

        /// <summary>
        /// The managed JET_COMMIT_ID that's reconstitued from native.
        /// </summary>
        private JET_COMMIT_ID managed;

        /// <summary>
        /// Create a native JET_COMMIT_ID and convert it to managed.
        /// </summary>
        [TestInitialize]
        [Description("Setup the CommitIdConversionTests test fixture")]
        public void Setup()
        {
            var sigX = new NATIVE_SIGNATURE()
            {
                ulRandom = 1,
                logtimeCreate = Any.Logtime,
                szComputerName = "Komputer",
            };

            this.managedOriginal = new JET_COMMIT_ID(new NATIVE_COMMIT_ID()
            {
                signLog = sigX,
                commitId = 123,
            });

            this.native = this.managedOriginal.GetNativeCommitId();
            this.managed = new JET_COMMIT_ID(this.native);
        }

        /// <summary>
        /// Test conversion from the native stuct works.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [Description("Test conversation from NATIVE_COMMIT_ID to JET_COMMIT_ID works.")]
        public void ConvertCommitIdFromNativeWorks()
        {
            Assert.AreEqual(this.managedOriginal, this.managed);
        }
    }
}