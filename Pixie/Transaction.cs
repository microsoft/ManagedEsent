//-----------------------------------------------------------------------
// <copyright file="Transaction.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace Microsoft.Isam.Esent
{
    /// <summary>
    /// A class that encapsulates a transaction.
    /// </summary>
    public abstract class Transaction : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the Transaction class.
        /// </summary>
        protected Transaction()
        {
        }

        /// <summary>
        /// Occurs when the transaction rolls back.
        /// </summary>
        public abstract event Action RolledBack;

        /// <summary>
        /// Occurs when the transaction commits.
        /// </summary>
        public abstract event Action Committed;

        /// <summary>
        /// Disposes of an instance of the Transaction class. This will rollback the transaction
        /// if still active.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Commit a transaction. This object must be in a transaction.
        /// </summary>
        public abstract void Commit();

        /// <summary>
        /// Commit a transaction. This object must be in a transaction.
        /// </summary>
        /// <param name="commitIsLazy">
        /// Makes the commit lazy. A lazy commit doesn't synchronously flush the
        /// commit log record to the disk.
        /// </param>
        public abstract void Commit(bool commitIsLazy);

        /// <summary>
        /// Rollback a transaction. This object must be in a transaction.
        /// </summary>
        public abstract void Rollback();
   }
}