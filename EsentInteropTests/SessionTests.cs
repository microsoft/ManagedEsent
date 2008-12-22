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
        /// Allocate a session and let it be disposed.
        /// </summary>
        [TestMethod]
        public void CreateSession()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            try
            {
                JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
                Api.JetInit(ref instance);
                using (Session session = new Session(instance))
                {
                    Assert.AreNotEqual(JET_SESID.Nil, session.JetSesid);
                    Api.JetBeginTransaction(session.JetSesid);
                    Api.JetCommitTransaction(session.JetSesid, CommitTransactionGrbit.None);
                }

                Api.JetTerm(instance);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        /// <summary>
        /// Allocate a session and end it.
        /// </summary>
        [TestMethod]
        public void CreateAndEndSession()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            try
            {
                JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
                Api.JetInit(ref instance);
                using (Session session = new Session(instance))
                {
                    session.End();
                }

                Api.JetTerm(instance);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        /// <summary>
        /// Check that ending a session zeroes the JetSesid member.
        /// </summary>
        [TestMethod]
        public void CheckThatEndSessionZeroesJetSesid()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            Api.JetInit(ref instance);

            Session session = new Session(instance);
            session.End();
            Assert.AreEqual(JET_SESID.Nil, session.JetSesid);

            Api.JetTerm(instance);
            Directory.Delete(dir, true);
        }

        /// <summary>
        /// Check that calling End on a disposed session throws
        /// an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void EndThrowsExceptionWhenSessionIsDisposed()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            Api.JetInit(ref instance);

            Session session = new Session(instance);
            session.Dispose();
            try
            {
                session.End();
            }
            finally
            {
                Api.JetTerm(instance);
                Directory.Delete(dir, true);
            }
        }

        /// <summary>
        /// Check that accessing the JetSesid property on a disposed
        /// session throws an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void JetSesidThrowsExceptionWhenSessionIsDisposed()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            Api.JetInit(ref instance);

            Session session = new Session(instance);
            session.Dispose();
            try
            {
                var x = session.JetSesid;
            }
            finally
            {
                Api.JetTerm(instance);
                Directory.Delete(dir, true);
            }
        }
    }
}