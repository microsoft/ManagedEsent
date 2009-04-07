//-----------------------------------------------------------------------
// <copyright file="TransactionTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Isam.Esent.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InteropApiTests
{
    /// <summary>
    /// Test the disposable Transaction class, which wraps
    /// JetBeginTransaction, JetCommitTransaction and JetRollback.
    /// </summary>
    [TestClass]
    public class TransactionTests
    {
        /// <summary>
        /// The directory being used for the database and its files.
        /// </summary>
        private string directory;

        /// <summary>
        /// The instance used by the test.
        /// </summary>
        private JET_INSTANCE instance;

        /// <summary>
        /// The session used by the test.
        /// </summary>
        private JET_SESID sesid;

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
            Api.JetBeginSession(this.instance, out this.sesid, String.Empty, String.Empty);
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            Api.JetEndSession(this.sesid, EndSessionGrbit.None);
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
            Assert.AreNotEqual(JET_SESID.Nil, this.sesid);
        }

        #endregion Setup/Teardown

        /// <summary>
        /// Start a transaction, commit and restart.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void CreateCommitAndBegin()
        {
            using (var transaction = new Transaction(this.sesid)) 
            {
                Assert.IsTrue(transaction.IsInTransaction);
                transaction.Commit(CommitTransactionGrbit.None);
                Assert.IsFalse(transaction.IsInTransaction);
                transaction.Begin();
                Assert.IsTrue(transaction.IsInTransaction);
            }
        }

        /// <summary>
        /// Start a transaction, rollback and restart.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void CreateRollbackAndBegin()
        {
            using (var transaction = new Transaction(this.sesid))
            {
                Assert.IsTrue(transaction.IsInTransaction);
                transaction.Rollback();
                Assert.IsFalse(transaction.IsInTransaction);
                transaction.Begin();
                Assert.IsTrue(transaction.IsInTransaction);
            }
        }

        /// <summary>
        /// Start a transaction twice, expecting an exception
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestDoubleTransactionBeginThrowsException()
        {
            using (var transaction = new Transaction(this.sesid))
            {
                transaction.Begin();
            }
        }

        /// <summary>
        /// Commit a transaction twice, expecting an exception
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestDoubleTransactionCommitThrowsException()
        {
            using (var transaction = new Transaction(this.sesid))
            {
                transaction.Commit(CommitTransactionGrbit.None);
                transaction.Commit(CommitTransactionGrbit.None);
            }
        }

        /// <summary>
        /// Rollback a transaction twice, expecting an exception
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestDoubleTransactionRollbackThrowsException()
        {
            using (var transaction = new Transaction(this.sesid))
            {
                transaction.Rollback();
                transaction.Rollback();
            }
        }

        /// <summary>
        /// Dispose the transaction object and then call a method,
        /// expecting an exception.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestBeginThrowsExceptionWhenDisposed()
        {
            var transaction = new Transaction(this.sesid);
            transaction.Dispose();
            transaction.Begin();
        }

        /// <summary>
        /// Dispose the transaction object and then call a method,
        /// expecting an exception.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestCommitThrowsExceptionWhenDisposed()
        {
            var transaction = new Transaction(this.sesid);
            transaction.Dispose();
            transaction.Commit(CommitTransactionGrbit.None);
        }

        /// <summary>
        /// Dispose the transaction object and then call a method,
        /// expecting an exception.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestRollbackThrowsExceptionWhenDisposed()
        {
            var transaction = new Transaction(this.sesid);
            transaction.Dispose();
            transaction.Rollback();
        }

        /// <summary>
        /// Dispose the transaction object and then call a method,
        /// expecting an exception.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestPropertyThrowsExceptionWhenDisposed()
        {
            var transaction = new Transaction(this.sesid);
            transaction.Dispose();
            bool x = transaction.IsInTransaction;
       }
    }
}