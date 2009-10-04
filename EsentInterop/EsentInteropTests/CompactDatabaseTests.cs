//-----------------------------------------------------------------------
// <copyright file="CompactDatabaseTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for <see cref="Api.JetCompact"/>.
    /// </summary>
    [TestClass]
    public class CompactDatabaseTests
    {
        /// <summary>
        /// JetCompact should throw an exception when
        /// the source database is null.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestJetCompactThrowsExceptionWhenSourceIsNull()
        {
            using (var instance = CreateLightweightInstance())
            {
                using (var session = new Session(instance))
                {
                    Api.JetCompact(session, null, "destination", null, null, CompactGrbit.None);
                }
            }
        }

        /// <summary>
        /// JetCompact should throw an exception when
        /// the source database is null.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestJetCompactThrowsExceptionWhenDestinationIsNull()
        {
            using (var instance = CreateLightweightInstance())
            {
                using (var session = new Session(instance))
                {
                    Api.JetCompact(session, "source", null, null, null, CompactGrbit.None);
                }
            }
        }

        /// <summary>
        /// JetCompact should throw an exception when
        /// the ignored parameter is non-null.
        /// </summary>
        [TestMethod]
        [Priority(0)]
        [ExpectedException(typeof(ArgumentException))]
        public void TestJetCompactThrowsExceptionWhenIgnoredIsNonNull()
        {
            using (var instance = CreateLightweightInstance())
            {
                using (var session = new Session(instance))
                {
#pragma warning disable 618,612 // JET_CONVERT is obsolete
                    Api.JetCompact(session, "source", "destination", null, new JET_CONVERT(), CompactGrbit.None);
#pragma warning restore 618,612
                }
            }
        }

        /// <summary>
        /// Test <see cref="Api.JetCompact"/>
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestJetCompact()
        {
            var test = new DatabaseFileTestHelper("database", true);
            test.TestCompactDatabase();
        }

        /// <summary>
        /// Test <see cref="Api.JetCompact"/>
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestJetCompactExceptionHandling()
        {
            var ex = new ArgumentNullException();
            var test = new DatabaseFileTestHelper("database", true);
            test.TestCompactDatabaseCallbackExceptionHandling(ex);
        }

        /// <summary>
        /// Create an instance without logging, temporary table or events.
        /// The instance is initialized.
        /// </summary>
        /// <returns>A new instance.</returns>
        private static Instance CreateLightweightInstance()
        {
            var instance = new Instance("CompactDatabaseTests");
            instance.Parameters.NoInformationEvent = true;
            instance.Parameters.MaxTemporaryTables = 0;
            instance.Parameters.Recovery = false;
            instance.Init();
            return instance;
        }
    }
}