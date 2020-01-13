//-----------------------------------------------------------------------
// <copyright file="UnpublishedBasicTableTests.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.
// </copyright>
//-----------------------------------------------------------------------

namespace InteropApiTests
{
    using Microsoft.Isam.Esent.Interop;
    using Microsoft.Isam.Esent.Interop.Unpublished;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unpublished Basic Api tests
    /// </summary>
    public partial class BasicTableTests
    {
        #region JetDefragment Tests

        /// <summary>
        /// Test starting OLD2 with DefragmentBTreeBatch on read-only database.
        /// </summary>
        [TestMethod]
        [Priority(2)]
        [Description("Start OLD2 with DefragmentBTreeBatch on read-only database must fail")]
        public void TestStartOld2WithDefragmentBTreeBatchReadOnlyDbMustFail()
        {
            this.ReattachDatabase(AttachDatabaseGrbit.ReadOnly);
            try
            {
                DefragGrbit defragGrbit = UnpublishedGrbits.DefragmentBTreeBatch | DefragGrbit.BatchStart;
                Api.Defragment(this.sesid, this.dbid, null, defragGrbit);
                Assert.Fail("Starting OLD2 with {0} should have failed with EsentDatabaseFileReadOnlyException, but succeeded.", defragGrbit);
            }
            catch (EsentDatabaseFileReadOnlyException)
            {
                // Expected.
            }
            finally
            {
                this.ReattachDatabase(AttachDatabaseGrbit.None);
            }
        }

        #endregion
    }
}