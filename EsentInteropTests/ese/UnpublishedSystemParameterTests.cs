//-----------------------------------------------------------------------
// <copyright file="UnpublishedSystemParameterTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Unpublished;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test the SystemParameters class.
    /// </summary>
    public partial class SystemParameterTests
    {
        /// <summary>
        /// Verify RecordSizeMost returns correct size for every supported page size.
        /// </summary>
        [TestMethod]
        [Priority(1)]
        [Description("Verify RecordSizeMost returns correct size for every supported page size.")]
        public void VerifyRecordSizeMost()
        {
            // Don't need mocks
            Api.Impl = this.savedApi;

            try
            {
                const int SMALL_HDR_SIZE = 40;
                const int LARGE_HDR_SIZE = 80;
                const int RESVD_TAG_SIZE = 4;

                SystemParameters.DatabasePageSize = 4 * 1024;
                Assert.AreEqual(SystemParameters.RecordSizeMost, (4 * 1024) - SMALL_HDR_SIZE - RESVD_TAG_SIZE);

                SystemParameters.DatabasePageSize = 8 * 1024;
                Assert.AreEqual(SystemParameters.RecordSizeMost, (8 * 1024) - SMALL_HDR_SIZE - RESVD_TAG_SIZE);

                SystemParameters.DatabasePageSize = 16 * 1024;
                Assert.AreEqual(SystemParameters.RecordSizeMost, (16 * 1024) - LARGE_HDR_SIZE - RESVD_TAG_SIZE);

                SystemParameters.DatabasePageSize = 32 * 1024;
                Assert.AreEqual(SystemParameters.RecordSizeMost, (32 * 1024) - LARGE_HDR_SIZE - RESVD_TAG_SIZE);
            }
            finally
            {
                Api.Impl = this.mockApi;
            }
        }
    }
}
