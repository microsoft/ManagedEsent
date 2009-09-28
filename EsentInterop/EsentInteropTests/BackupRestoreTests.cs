//-----------------------------------------------------------------------
// <copyright file="BackupRestoreTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for JetBackupInstance and JetRestoreInstance.
    /// </summary>
    [TestClass]
    public class BackupRestoreTests
    {
        /// <summary>
        /// Test backup and restore of a database.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestBackupRestore()
        {
            var test = new BackupRestoreDatabase("database", "backup", true);
            test.TestBackupRestore();
        }

        /// <summary>
        /// Test exception handling for exceptions thrown from
        /// the status callback during backup.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestBackupCallbackExceptionHandling()
        {
            var ex = new ArgumentNullException();
            var test = new BackupRestoreDatabase("database", "backup", true);
            test.TestBackupCallbackExceptionHandling(ex);
        }

        /// <summary>
        /// Test exception handling for exceptions thrown from
        /// the status callback during restore.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestBackupRestoreCallbackExceptionHandling()
        {
            var ex = new ArgumentNullException();
            var test = new BackupRestoreDatabase("database", "backup", true);
            test.TestRestoreCallbackExceptionHandling(ex);
        }
    }
}
