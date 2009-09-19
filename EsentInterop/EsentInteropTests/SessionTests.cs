//-----------------------------------------------------------------------
// <copyright file="SessionTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using System.IO;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the disposable Session class, which wraps a JET_SESSION.
    /// </summary>
    [TestClass]
    public class SessionTests
    {
        /// <summary>
        /// The directory being used for the database and its files.
        /// </summary>
        private string directory;

        /// <summary>
        /// The instance used by the test.
        /// </summary>
        private JET_INSTANCE instance;

        #region Setup/Teardown

        /// <summary>
        /// Initialization method. Called once when the tests are started.
        /// All DDL should be done in this method.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.directory = SetupHelper.CreateRandomDirectory();
            this.instance = SetupHelper.CreateNewInstance(this.directory);

            // turn off logging so initialization is faster
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.Recovery, 0, "off");
            Api.JetSetSystemParameter(this.instance, JET_SESID.Nil, JET_param.MaxTemporaryTables, 0, null);
            Api.JetInit(ref this.instance);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            Api.JetTerm(this.instance);
            Directory.Delete(this.directory, true);
        }

        /// <summary>
        /// Verify that the test class has setup the test fixture properly.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void VerifyFixtureSetup()
        {
            Assert.AreNotEqual(JET_INSTANCE.Nil, this.instance);
        }

        #endregion Setup/Teardown

        /// <summary>
        /// Allocate a session and let it be disposed.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void CreateSession()
        {
           using (var session = new Session(this.instance))
            {
                Assert.AreNotEqual(JET_SESID.Nil, session.JetSesid);
                Api.JetBeginTransaction(session.JetSesid);
                Api.JetCommitTransaction(session.JetSesid, CommitTransactionGrbit.None);
            }
        }

        /// <summary>
        /// Test that a Session can be converted to a JET_SESID
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void SessionCanConvertToJetSesid()
        {
            using (var session = new Session(this.instance))
            {
                JET_SESID sesid = session;
                Assert.AreEqual(sesid, session.JetSesid);
            }
        }

        /// <summary>
        /// Allocate a session and end it.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void CreateAndEndSession()
        {
            using (var session = new Session(this.instance))
            {
                session.End();
            }
        }

        /// <summary>
        /// Check that ending a session zeroes the JetSesid member.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void CheckThatEndSessionZeroesJetSesid()
        {
            var session = new Session(this.instance);
            session.End();
            Assert.AreEqual(JET_SESID.Nil, session.JetSesid);
        }

        /// <summary>
        /// Check that calling End on a disposed session throws
        /// an exception.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void EndThrowsExceptionWhenSessionIsDisposed()
        {
            var session = new Session(this.instance);
            session.Dispose();
            session.End();
        }

        /// <summary>
        /// Check that accessing the JetSesid property on a disposed
        /// session throws an exception.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void JetSesidThrowsExceptionWhenSessionIsDisposed()
        {
            var session = new Session(this.instance);
            session.Dispose();
            JET_SESID x = session.JetSesid;
        }
    }
}