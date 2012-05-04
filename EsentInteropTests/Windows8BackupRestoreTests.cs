//-----------------------------------------------------------------------
// <copyright file="Windows8BackupRestoreTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using System;
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for JetBackupInstance and JetRestoreInstance.
    /// </summary>
    public partial class BackupRestoreTests
    {
#if !MANAGEDESENT_ON_METRO
        /// <summary>
        /// Test exception handling for exceptions thrown from
        /// the status callback during JetRestore.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Test exception handling for exceptions thrown from the status callback during JetRestore")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestBackupRestoreCallbackExceptionHandling()
        {
            var ex = new ArgumentNullException();
            var test = new DatabaseFileTestHelper("database", "backup", true);            
            test.TestRestoreCallbackExceptionHandling(ex);
        }
#endif // !MANAGEDESENT_ON_METRO
    }
}