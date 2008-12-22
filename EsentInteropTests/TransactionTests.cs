//-----------------------------------------------------------------------
// <copyright file="TransactionTests.cs" company="Microsoft Corporation">
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
    /// Test the disposable Transaction class, which wraps
    /// JetBeginTransaction, JetCommitTransaction and JetRollback.
    /// </summary>
    [TestClass]
    public class TransactionTests
    {
        /// <summary>
        /// Start a transaction, commit and restart.
        /// </summary>
        [TestMethod]
        public void CreateCommitAndBegin()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            try
            {
                JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
                Api.JetInit(ref instance);
                JET_SESID sesid;
                Api.JetBeginSession(instance, out sesid, null, null);
                using (Transaction transaction = new Transaction(sesid)) 
                {
                    Assert.IsTrue(transaction.IsInTransaction);
                    transaction.Commit(CommitTransactionGrbit.None);
                    Assert.IsFalse(transaction.IsInTransaction);
                    transaction.Begin();
                    Assert.IsTrue(transaction.IsInTransaction);
                }

                Api.JetTerm(instance);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        /// <summary>
        /// Start a transaction, rollback and restart.
        /// </summary>
        [TestMethod]
        public void CreateRollbackAndBegin()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            try
            {
                JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
                Api.JetInit(ref instance);
                JET_SESID sesid;
                Api.JetBeginSession(instance, out sesid, null, null);
                using (Transaction transaction = new Transaction(sesid))
                {
                    Assert.IsTrue(transaction.IsInTransaction);
                    transaction.Rollback();
                    Assert.IsFalse(transaction.IsInTransaction);
                    transaction.Begin();
                    Assert.IsTrue(transaction.IsInTransaction);
                }

                Api.JetTerm(instance);
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }

        /// <summary>
        /// Start a transaction twice, expecting an exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestDoubleTransactionBeginThrowsException()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            try
            {
                Api.JetInit(ref instance);
                JET_SESID sesid;
                Api.JetBeginSession(instance, out sesid, null, null);

                using (Transaction transaction = new Transaction(sesid))
                {
                    transaction.Begin();
                }
            }
            finally
            {
                Api.JetTerm(instance);
                Directory.Delete(dir, true);
            }
        }

        /// <summary>
        /// Commit a transaction twice, expecting an exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestDoubleTransactionCommitThrowsException()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            try
            {
                Api.JetInit(ref instance);
                JET_SESID sesid;
                Api.JetBeginSession(instance, out sesid, null, null);

                using (Transaction transaction = new Transaction(sesid))
                {
                    transaction.Commit(CommitTransactionGrbit.None);
                    transaction.Commit(CommitTransactionGrbit.None);
                }
            }
            finally
            {
                Api.JetTerm(instance);
                Directory.Delete(dir, true);
            }
        }

        /// <summary>
        /// Rollback a transaction twice, expecting an exception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestDoubleTransactionRollbackThrowsException()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            try
            {
                Api.JetInit(ref instance);
                JET_SESID sesid;
                Api.JetBeginSession(instance, out sesid, null, null);

                using (Transaction transaction = new Transaction(sesid))
                {
                    transaction.Rollback();
                    transaction.Rollback();
                }
            }
            finally
            {
                Api.JetTerm(instance);
                Directory.Delete(dir, true);
            }
        }

        /// <summary>
        /// Dispose the transaction object and then call a method,
        /// expecting an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestBeginThrowsExceptionWhenDisposed()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            try
            {
                Api.JetInit(ref instance);
                JET_SESID sesid;
                Api.JetBeginSession(instance, out sesid, null, null);

                Transaction transaction = new Transaction(sesid);
                transaction.Dispose();
                transaction.Begin();
            }
            finally
            {
                Api.JetTerm(instance);
                Directory.Delete(dir, true);
            }
        }

        /// <summary>
        /// Dispose the transaction object and then call a method,
        /// expecting an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestCommitThrowsExceptionWhenDisposed()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            try
            {
                Api.JetInit(ref instance);
                JET_SESID sesid;
                Api.JetBeginSession(instance, out sesid, null, null);

                Transaction transaction = new Transaction(sesid);
                transaction.Dispose();
                transaction.Commit(CommitTransactionGrbit.None);
            }
            finally
            {
                Api.JetTerm(instance);
                Directory.Delete(dir, true);
            }
        }

        /// <summary>
        /// Dispose the transaction object and then call a method,
        /// expecting an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestRollbackThrowsExceptionWhenDisposed()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            try
            {
                Api.JetInit(ref instance);
                JET_SESID sesid;
                Api.JetBeginSession(instance, out sesid, null, null);

                Transaction transaction = new Transaction(sesid);
                transaction.Dispose();
                transaction.Rollback();
            }
            finally
            {
                Api.JetTerm(instance);
                Directory.Delete(dir, true);
            }
        }

        /// <summary>
        /// Dispose the transaction object and then call a method,
        /// expecting an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TestPropertyThrowsExceptionWhenDisposed()
        {
            string dir = SetupHelper.CreateRandomDirectory();
            JET_INSTANCE instance = SetupHelper.CreateNewInstance(dir);
            try
            {
                Api.JetInit(ref instance);
                JET_SESID sesid;
                Api.JetBeginSession(instance, out sesid, null, null);

                Transaction transaction = new Transaction(sesid);
                transaction.Dispose();
                var x = transaction.IsInTransaction;
            }
            finally
            {
                Api.JetTerm(instance);
                Directory.Delete(dir, true);
            }
        }
    }
}