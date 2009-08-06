//-----------------------------------------------------------------------
// <copyright file="XpCompatabilityTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using Microsoft.Isam.Esent.Interop;
using Microsoft.Isam.Esent.Interop.Implementation;
using Microsoft.Isam.Esent.Interop.Vista;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
    /// <summary>
    /// Test the Api class functionality when we have an XP version of Esent.
    /// </summary>
    [TestClass]
    public class XpCompatabilityTests
    {
        /// <summary>
        /// The saved API, replaced when finished.
        /// </summary>
        private IJetApi savedImpl;

        /// <summary>
        /// Setup the mock object repository.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.savedImpl = Api.Impl;
            Api.Impl = new JetApi(Constants.XpVersion);
        }

        /// <summary>
        /// Cleanup after the test.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            Api.Impl = this.savedImpl;
        }

        /// <summary>
        /// Verify that the XP version of ESENT doesn't support
        /// large keys.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyXpDoesNotSupportLargeKeys()
        {
            Assert.IsFalse(EsentVersion.SupportsLargeKeys);
        }

        /// <summary>
        /// Verify that the XP version of ESENT doesn't support
        /// Windows Server 2003 features.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyXpDoesNotSupportServer2003Features()
        {
            Assert.IsFalse(EsentVersion.SupportsServer2003Features);
        }

        /// <summary>
        /// Verify that the XP version of ESENT doesn't support
        /// Unicode paths.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyXpDoesNotSupportUnicodePaths()
        {
            Assert.IsFalse(EsentVersion.SupportsUnicodePaths);
        }

        /// <summary>
        /// Verify that the XP version of ESENT doesn't support
        /// Windows Vista features.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyXpDoesNotSupportVistaFeatures()
        {
            Assert.IsFalse(EsentVersion.SupportsVistaFeatures);
        }

        /// <summary>
        /// Verify that the XP version of ESENT doesn't support
        /// Windows 7 features.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        public void VerifyXpDoesNotSupportWindows7Features()
        {
            Assert.IsFalse(EsentVersion.SupportsWindows7Features);
        }

        /// <summary>
        /// Verify that JetGetThreadStats throws an exception when using the
        /// XP version of ESENT.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(InvalidOperationException))]
        public void VerifyXpThrowsExceptionOnJetGetThreadStats()
        {
            JET_THREADSTATS threadstats;
            VistaApi.JetGetThreadStats(out threadstats);
        }

        /// <summary>
        /// Verify that JetOpenTemporaryTable throws an exception when using the
        /// XP version of ESENT.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(InvalidOperationException))]
        public void VerifyXpThrowsExceptionOnJetOpenTemporaryTable()
        {
            var sesid = new JET_SESID();
            var temporarytable = new JET_OPENTEMPORARYTABLE();
            VistaApi.JetOpenTemporaryTable(sesid, temporarytable);
        }
    }
}